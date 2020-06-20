
using System;
using uPLibrary.Networking.M2Mqtt;
using inverter.common.model;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Text;
using inverter.common.model.messages;
using System.Globalization;
using static AverageValueOverTime;

namespace inverter.mqtt
{
    public class MqttWorker
    {
        public class SensorConfig
        {
            public string Name { get; set; }
            public string FriendlyName { get; set; }
            public string UnitOfMeasure { get; set; }
            public string IconName { get; set; }
            public bool HasRunningAverage { get; set; }
            public int UpdatePeriod { get; set; }
            public string DataType {get; set;}

            public string DeviceClass {get; set;}

        }

        private const int periodUpdateCount = 2;
        public SensorConfig[] sensorValues = new SensorConfig[] {
            new SensorConfig { Name =  "PV_in_voltage", FriendlyName="Energy - Solar Panel Voltage", UnitOfMeasure = "V", IconName = "solar-panel-large" , HasRunningAverage = false, UpdatePeriod = 20 },
            new SensorConfig { Name =  "PV_in_current", FriendlyName="Energy - Solar Panel Current", UnitOfMeasure = "A", IconName = "solar-panel-large" , HasRunningAverage = false, UpdatePeriod = 20 },
            new SensorConfig { Name =  "PV_in_watts", FriendlyName = "Energy - Solar Panel Power", DeviceClass = "power", UnitOfMeasure = "W", IconName = "solar-panel-large" , HasRunningAverage = true, UpdatePeriod = 5 },
            new SensorConfig { Name =  "SCC_voltage", FriendlyName = "Energy - MPTT Charger Voltage", UnitOfMeasure = "V", IconName = "current-dc" , HasRunningAverage = false, UpdatePeriod = 60 },
            new SensorConfig { Name =  "load_watt",  FriendlyName = "Energy - Inverter Load", UnitOfMeasure = "W", DeviceClass = "power", IconName ="chart-bell-curve" , HasRunningAverage = true, UpdatePeriod = 5 },
            new SensorConfig { Name =  "bus_voltage", FriendlyName = "Energy - Inverter Bus Voltage", UnitOfMeasure = "V",IconName ="details" , HasRunningAverage = false, UpdatePeriod = 60 },
            new SensorConfig { Name =  "heatsink_temperature", FriendlyName = "Energy - Inverter Temperature",DeviceClass = "temperature", UnitOfMeasure = "C",IconName ="details" , HasRunningAverage = false, UpdatePeriod = 60 },
            new SensorConfig { Name =  "battery_capacity", FriendlyName = "Energy - Battery Level",DeviceClass = "battery", UnitOfMeasure = "%",IconName ="battery-outline" , HasRunningAverage = false, UpdatePeriod =  30 },
            new SensorConfig { Name =  "battery_charge_current", FriendlyName="Energy - Battery Charge Current", UnitOfMeasure = "A",IconName ="current-dc" , HasRunningAverage = false, UpdatePeriod = 10 },
            new SensorConfig { Name =  "battery_discharge_current", FriendlyName = "Energy - Battery Discharge Current",  UnitOfMeasure = "A", IconName ="current-dc" , HasRunningAverage = false, UpdatePeriod = 10 },
            new SensorConfig { Name =  "battery_voltage", FriendlyName = "Energy - Battery Voltage",  UnitOfMeasure = "V", IconName ="current-dc" , HasRunningAverage = false, UpdatePeriod = 120 },
            new SensorConfig { Name =  "inverter_mode", FriendlyName = "Energy - Inverter Mode",  UnitOfMeasure = "", IconName ="current-dc" , HasRunningAverage = false, UpdatePeriod = 30 },
            new SensorConfig { Name =  "time_to_charge", FriendlyName = "Energy - Battery Time To Charge ",  UnitOfMeasure = "Minutes", IconName ="timer-sand" , HasRunningAverage = false, UpdatePeriod = 60 },
            new SensorConfig { Name =  "time_to_discharge", FriendlyName = "Energy - Battery Time To Discharge ",  UnitOfMeasure = "Minutes", IconName ="timer-sand" , HasRunningAverage = false, UpdatePeriod = 60 }

        };

        private AverageValueOverTime[] periodUpdates = new AverageValueOverTime[periodUpdateCount];


        private readonly ILogger<Worker> _logger;
        private MQTT config;
        private Inverter inverterConfig;
        public MqttClient MqttClient { get; private set; }

        public MqttWorker(ILogger<Worker> logger, AppSettings appSettings)
        {
            _logger = logger;
            config = appSettings.MQTT;
            inverterConfig = appSettings.Inverter;
            MqttClient = new MqttClient(config.server);
        }
        
        public async Task ConnectMQTT(CancellationToken stoppingToken)
        {
            MqttClient.ConnectionClosed += async (sender, e) =>
            {
                _logger.Log(LogLevel.Information, "Connection Closed.. {0}", config.server);
                await ConnectAndInitialiseMQTT(stoppingToken);
            };

            await ConnectAndInitialiseMQTT(stoppingToken);
        }


        public void Disconnect()
        {
            _logger.Log(LogLevel.Warning, "Disconnnecting...{0}", MqttClient.ClientId);
            periodUpdates = new AverageValueOverTime[periodUpdateCount];

            if (MqttClient.IsConnected)
                MqttClient.Disconnect();
        }

        private async Task ConnectAndInitialiseMQTT(CancellationToken stoppingToken)
        {
            string clientId = Environment.GetEnvironmentVariable("COMPUTERNAME") + Guid.NewGuid();

            var connected = MqttClient.IsConnected;
            while (!connected && !stoppingToken.IsCancellationRequested)
            {
                try
                {
                    MqttClient.Connect(clientId, config.username, config.password, true, 3);
                    connected = MqttClient.IsConnected;
                    _logger.Log(LogLevel.Information, "MQTT Conected to to...{0}", config.server);
                }
                catch
                {
                    _logger.Log(LogLevel.Warning, "No connection to...{0}", config.server);
                }
                connected = MqttClient.IsConnected;
                if (!connected)
                    await Task.Delay(10000, stoppingToken);
                else
                    _logger.Log(LogLevel.Information, "MQTT Conected to to...{0}", config.server);
            }

            // After connect, create sensors
            string sensorName = $"{config.topic}/sensor/{config.devicename}/config";
            string sensorMsg = $"{{\"name\": \"{config.devicename}\",\"state_topic\": \"{config.topic}/sensor/{config.devicename}\"}}";
            MqttClient.Publish(sensorName, Encoding.UTF8.GetBytes(sensorMsg));
            int iPeriodUpdateId = 0;

            foreach (SensorConfig sensor in sensorValues)
            {
                _logger.Log(LogLevel.Information, "Item...{0}:{1}:{2}", sensor.Name, sensor.UnitOfMeasure, sensor.IconName);

                if (sensor.HasRunningAverage)
                {
                    // will have three values, minute, hour , day
                    periodUpdates[iPeriodUpdateId] = new AverageValueOverTime(sensor.Name, periodUpdate);
                    iPeriodUpdateId += 1;
                    sensorName = $"{config.topic}/sensor/{config.devicename}_{sensor.Name}_min/config";
                    sensorMsg = $"{{\"name\": \"{sensor.FriendlyName} (last minute)\",\"unit_of_measurement\": \"{sensor.UnitOfMeasure}\",\"state_topic\": \"{config.topic}/sensor/{config.devicename}_{sensor.Name}_min\",\"icon\": \"mdi:{sensor.IconName}\"}}";

                    MqttClient.Publish(sensorName, Encoding.UTF8.GetBytes(sensorMsg));

                    sensorName = $"{config.topic}/sensor/{config.devicename}_{sensor.Name}_hour/config";
                    sensorMsg = $"{{\"name\": \"{sensor.FriendlyName} (last hour)\",\"unit_of_measurement\": \"{sensor.UnitOfMeasure}\",\"state_topic\": \"{config.topic}/sensor/{config.devicename}_{sensor.Name}_hour\",\"icon\": \"mdi:{sensor.IconName}\"}}";
                    MqttClient.Publish(sensorName, Encoding.UTF8.GetBytes(sensorMsg));

                    sensorName = $"{config.topic}/sensor/{config.devicename}_{sensor.Name}_day/config";
                    sensorMsg = $"{{\"name\": \"{sensor.FriendlyName} (last day)\",\"unit_of_measurement\": \"{sensor.UnitOfMeasure}\",\"state_topic\": \"{config.topic}/sensor/{config.devicename}_{sensor.Name}_day\",\"icon\": \"mdi:{sensor.IconName}\"}}";
                    MqttClient.Publish(sensorName, Encoding.UTF8.GetBytes(sensorMsg));
                }

                sensorName = $"{config.topic}/sensor/{config.devicename}_{sensor.Name}/config";
                sensorMsg = $"{{\"name\": \"{sensor.FriendlyName}\",\"unit_of_measurement\": \"{sensor.UnitOfMeasure}\",\"state_topic\": \"{config.topic}/sensor/{config.devicename}_{sensor.Name}\",\"icon\": \"mdi:{sensor.IconName}\"}}";
                MqttClient.Publish(sensorName, Encoding.UTF8.GetBytes(sensorMsg));
            }
        }

        private void periodUpdate(AverageOverTimeUpdate update)
        {
            if (MqttClient.IsConnected)
            {
                string sensorName = $"{config.topic}/sensor/{config.devicename}_{update.valueName}";

                if (update.timePeriod == TimePeriod.Minute)
                    sensorName += "_min";
                else if (update.timePeriod == TimePeriod.Hour)
                    sensorName += "_hour";
                else if (update.timePeriod == TimePeriod.Day)
                    sensorName += "_day";
                string sensorValue = update.updatedValue.ToString(CultureInfo.InvariantCulture);
                MqttClient.Publish(sensorName, Encoding.UTF8.GetBytes(sensorValue));

            }
        }

        public void Update(OperatingProps opProps, int counter)
        {
            if (MqttClient.IsConnected)
            {
                int iPeriodUpdateId = 0;
                foreach (SensorConfig sensor in sensorValues)
                {
                    string sensorName = $"{config.topic}/sensor/{config.devicename}_{sensor.Name}";
                    decimal sensorValue = SensorValue(opProps, sensor.Name);

                    if (sensor.HasRunningAverage)
                    {
                        periodUpdates[iPeriodUpdateId].addValue(sensorValue);
                        iPeriodUpdateId += 1;
                    }
                    if (counter % sensor.UpdatePeriod == 0)
                    {
                        // publish a message on "/home/temperature" topic with QoS 2 
                        byte[] value = SensorValueByte(opProps, sensor.Name);
                        MqttClient.Publish(sensorName, value);
                    }
                }
            }
            else
                _logger.Log(LogLevel.Information, "Updated paused until connected...{0}", config.server);

        }

        private decimal SensorValue(OperatingProps opProps, string sensorName)
        {
            if (sensorName == sensorValues[0].Name) return opProps.solar.PVInputVoltage1;
            if (sensorName == sensorValues[1].Name) return opProps.solar.PVInputCurrentForBattery;
            if (sensorName == sensorValues[2].Name) return opProps.solar.PVChargingPower;
            if (sensorName == sensorValues[3].Name) return opProps.solar.BatteryVoltageFromSCC;
            if (sensorName == sensorValues[4].Name) return opProps.inverter.ACOutputActivePower;
            if (sensorName == sensorValues[5].Name) return opProps.battery.BusVoltage;
            if (sensorName == sensorValues[6].Name) return opProps.enviroment.HeatSinkTemperature;
            if (sensorName == sensorValues[7].Name) return opProps.battery.BatteryCapacity;
            if (sensorName == sensorValues[8].Name) return opProps.battery.BatteryChargingCurrent;
            if (sensorName == sensorValues[9].Name) return opProps.battery.BatteryDischargeCurrent;
            if (sensorName == sensorValues[10].Name) return opProps.battery.BatteryVoltage;
            if (sensorName == sensorValues[12].Name) { 
                // time to charge. ( 100 - Battery % * Ah) / 
                if (opProps.battery.BatteryChargingCurrent > 0) {

                    decimal remCap = 1 - ((decimal)opProps.battery.BatteryCapacity / 100);
                    decimal ahRem = ( remCap * inverterConfig.BatteryAmpHours);
                    decimal hRem = (ahRem / (decimal)opProps.battery.BatteryChargingCurrent);
                    decimal minutes = hRem * 60;

                    //_logger.Log(LogLevel.Information, "Charge..{0}:{1}:{2}:{3}", remCap, ahRem, hRem, minutes);
                    return minutes;
                } else return 0;
            }
            if (sensorName == sensorValues[13].Name) {
                // time to discharge. ( ( Battery % - (DoD * 100) * Ah) / 
                if (opProps.battery.BatteryDischargeCurrent > 0) {
                    decimal capRem = opProps.battery.BatteryCapacity - (100 * inverterConfig.DoD);
                    decimal cap = (capRem / 100);
                    decimal ahRem = (cap * inverterConfig.BatteryAmpHours);
                    decimal hRem = (ahRem / (decimal)opProps.battery.BatteryDischargeCurrent);
                    decimal minutes = hRem * 60;

                    //_logger.Log(LogLevel.Information, "DisCharge..{0}:{1}:{2}:{3}", minutes, opProps.battery.BatteryCapacity, inverterConfig.BatteryAmpHours, opProps.battery.BatteryDischargeCurrent);
                    return minutes;
                } else return 0;

            }
            

            return 0;
        }

        private byte[] SensorValueByte(OperatingProps opProps, string sensorName)
        {
            if (sensorName == sensorValues[11].Name) {
                if (opProps.inverter.ModeId == DeviceModes.Battery){
                    if ((opProps.inverter.DeviceStatus1 & DeviceStatus1Flags.BatteryChargingSCC) == DeviceStatus1Flags.BatteryChargingSCC) {
                        if ((opProps.battery.BatteryChargingCurrent == 0) && (opProps.battery.BatteryDischargeCurrent == 0))
                            return Encoding.UTF8.GetBytes("Solar Only");
                        else
                            return Encoding.UTF8.GetBytes("Solar And Charging");
                    }
                    else if ((opProps.inverter.DeviceStatus1 & DeviceStatus1Flags.BatteryChargingAC) == DeviceStatus1Flags.BatteryChargingAC)
                        return Encoding.UTF8.GetBytes("Utility Charging");
                    else 
                        return Encoding.UTF8.GetBytes("Battery");

                }
                else 
                    return Encoding.UTF8.GetBytes(opProps.inverter.Mode.ToString(CultureInfo.InvariantCulture));

            } 
            
            return Encoding.UTF8.GetBytes(SensorValue(opProps, sensorName).ToString(CultureInfo.InvariantCulture));
        }
    }
}