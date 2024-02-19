
using System;
using uPLibrary.Networking.M2Mqtt;
using inverter.common.model;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Text;
using inverter.common.model.messages;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace inverter.mqtt
{
    public class MqttWorker
    {

        public class Device
        {
            [JsonPropertyName("name")]
            public string Name { get; set; }
            [JsonPropertyName("ids")]
            public string[] Ids { get; set; }
        }
        public class SensorConfig
        {

            [JsonIgnore]
            public string Name { get; set; }
            [JsonPropertyName("name")]
            public string FriendlyName { get; set; }


            [JsonPropertyName("unit_of_meas")]
            public string UnitOfMeasure { get; set; }
            [JsonPropertyName("ic")]
            public string IconName { get; set; }
            [JsonPropertyName("stat_t")]
            public string StateTopic
            {
                get
                {
                    return $"{Config.devicename}/{Name}/state";
                }
            }
            [JsonPropertyName("dev_cla")]
            public string DeviceClass { get; set; }
            [JsonPropertyName("state_class")]
            //[JsonIgnore]
            public string StateClass { get; set; }

            [JsonPropertyName("options")]
            public string[] Options { get; set; }
            //[JsonIgnore]
            [JsonPropertyName("uniq_id")]
            public string UniqueId
            {
                get
                {
                    return $"{Config.devicename}/{Name}";
                }
            }

            [JsonPropertyName("dev")]
            public Device device { get; set; }
            [JsonIgnore]
            public int UpdatePeriod { get; set; }
            [JsonIgnore]
            public MQTT Config { get; set; }
        }


        static string[] output_options = new string[] {"USB","SUB","SBU"};
        static string[] charge_options = new string[] {"Grid First","Solar First","Solar - Grid", "Only Solar"};
        static string[] inverter_mode = new string[] {"Power On","Standby","Line in", "Battery", "Fault", "Power Saving"};

        public SensorConfig[] sensorValues = new SensorConfig[] {
            new SensorConfig { Name =  "pv_in_voltage", FriendlyName="Solar Panel Voltage",  UnitOfMeasure = "V", DeviceClass = "voltage",   StateClass="measurement", UpdatePeriod = 6 }, // 0
            new SensorConfig { Name =  "pv_in_current", FriendlyName="Solar Panel Current", UnitOfMeasure = "A", DeviceClass= "current",  StateClass="measurement", UpdatePeriod = 6 }, // 1
            new SensorConfig { Name =  "pv_in_watts", FriendlyName = "Solar Panel Power", UnitOfMeasure = "W", DeviceClass = "power", StateClass="measurement",  UpdatePeriod = 3 }, // 2
            new SensorConfig { Name =  "scc_voltage", FriendlyName = "MPTT Charger Voltage", UnitOfMeasure = "V", DeviceClass = "voltage",  StateClass="measurement", UpdatePeriod = 12 }, // 3
            new SensorConfig { Name =  "load_watt",  FriendlyName = "Inverter Load", UnitOfMeasure = "W", DeviceClass = "power", StateClass="measurement",  UpdatePeriod = 1 }, // 4
            new SensorConfig { Name =  "bus_voltage", FriendlyName = "Inverter Bus Voltage", UnitOfMeasure = "V", DeviceClass = "voltage",  StateClass="measurement", UpdatePeriod = 12 }, // 5
            new SensorConfig { Name =  "heatsink_temperature", FriendlyName = "Inverter Temperature", DeviceClass = "temperature", StateClass="measurement",  UnitOfMeasure = "C", UpdatePeriod = 12 }, // 6
            new SensorConfig { Name =  "battery_capacity", FriendlyName = "Battery Level", UnitOfMeasure = "%", DeviceClass = "battery", StateClass="measurement", UpdatePeriod =  12 }, // 7
            new SensorConfig { Name =  "battery_charge_current", FriendlyName="Battery Charge Current", UnitOfMeasure = "A", DeviceClass= "current", StateClass="measurement",  UpdatePeriod = 1 }, //8
            new SensorConfig { Name =  "battery_discharge_current", FriendlyName = "Battery Discharge Current",  UnitOfMeasure = "A", DeviceClass= "current", StateClass="measurement" , UpdatePeriod = 1 }, // 9
            new SensorConfig { Name =  "battery_voltage", FriendlyName = "Battery Voltage",  UnitOfMeasure = "V", DeviceClass= "voltage", StateClass="measurement",  UpdatePeriod = 12 }, // 10
            new SensorConfig { Name =  "inverter_mode", FriendlyName = "Inverter Mode", DeviceClass="enum",  Options=inverter_mode,  UnitOfMeasure = "",  UpdatePeriod = 5 }, // 11
            new SensorConfig { Name =  "time_to_charge", FriendlyName = "Battery Time To Charge ",  UnitOfMeasure = "Minutes", IconName ="mdi:timer-sand" ,  UpdatePeriod = 1 }, // 12
            new SensorConfig { Name =  "time_to_discharge", FriendlyName = "Battery Time To Discharge ",  UnitOfMeasure = "Minutes", IconName ="mdi:timer-sand" ,  UpdatePeriod = 1 }, // 13
            new SensorConfig { Name =  "grid_voltage", FriendlyName = "Grid Voltage ",  UnitOfMeasure = "V", DeviceClass= "voltage", StateClass="measurement", UpdatePeriod = 3 } // 14
        };

        public SensorConfig[] userValues = new SensorConfig[] {
            new SensorConfig { Name =  "output_priority", FriendlyName = "Output Priority", DeviceClass="enum", Options=output_options,  UnitOfMeasure = "",  UpdatePeriod = 5 }, // 0
            new SensorConfig { Name =  "charge_priority", FriendlyName = "Charge Priority", DeviceClass="enum", Options=charge_options, UnitOfMeasure = "",  UpdatePeriod = 5 } // 1
        };


        private readonly ILogger<Worker> _logger;
        private MQTT config;
        private Inverter inverterConfig;
        public MqttClient MqttClient { get; private set; }
        public Device sensorDevice;
        public MqttWorker(ILogger<Worker> logger, AppSettings appSettings)
        {
            _logger = logger;
            config = appSettings.MQTT;
            inverterConfig = appSettings.Inverter;
            MqttClient = new MqttClient(config.server);
            sensorDevice = new Device { Name = config.devicename };
            sensorDevice.Ids = new string[] { config.id };
        }

        public async Task ConnectMQTT(CancellationToken stoppingToken)
        {
            await ConnectAndInitialiseMQTT(stoppingToken);
        }


        public void Disconnect()
        {
            _logger.Log(LogLevel.Warning, "Disconnnecting...{0}", MqttClient.ClientId);

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
                    MqttClient = new MqttClient(config.server);
                    MqttClient.Connect(clientId, config.username, config.password, false, 10);
                    connected = MqttClient.IsConnected;
                    MqttClient.ConnectionClosed += async (sender, e) =>
                    {
                        _logger.Log(LogLevel.Information, "Connection Closed.. {0}", config.server);
                        await ConnectAndInitialiseMQTT(stoppingToken);
                    };
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
            string sensorConfigTopic = $"{config.topic}/sensor/{config.devicename}/config";

            var options = new JsonSerializerOptions
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                WriteIndented = false
            };

            foreach (SensorConfig sensor in sensorValues)
            {
                _logger.Log(LogLevel.Information, "Item...{0}:{1}:{2}", sensor.Name, sensor.UnitOfMeasure, sensor.IconName);
                sensor.Config = config;
                sensorConfigTopic = $"{config.topic}/sensor/{sensor.UniqueId}/config";
                sensor.device = sensorDevice;

                _logger.Log(LogLevel.Information, "Sending...{0}:{1}", sensorConfigTopic, JsonSerializer.Serialize(sensor, options));

                MqttClient.Publish(sensorConfigTopic, Encoding.UTF8.GetBytes(JsonSerializer.Serialize(sensor, options)));
            }

            foreach (SensorConfig sensor in userValues)
            {
                _logger.Log(LogLevel.Information, "Item...{0}:{1}:{2}", sensor.Name, sensor.UnitOfMeasure, sensor.IconName);
                sensor.Config = config;
                sensorConfigTopic = $"{config.topic}/sensor/{sensor.UniqueId}/config";
                sensor.device = sensorDevice;
                _logger.Log(LogLevel.Information, "Sending...{0}:{1}", sensorConfigTopic, JsonSerializer.Serialize(sensor, options));

                MqttClient.Publish(sensorConfigTopic, Encoding.UTF8.GetBytes(JsonSerializer.Serialize(sensor, options)));
            }            
        }

        public void UpdateOpProps(OperatingProps opProps, int counter)
        {
            if (MqttClient.IsConnected)
            {
                foreach (SensorConfig sensor in sensorValues)
                {
                    sensor.Config = config;
                    if (counter % sensor.UpdatePeriod == 0)
                    {
                        // publish a message on "/home/temperature" topic with QoS 2 
                        byte[] value = SensorValueByte(opProps, sensor.Name);
                        MqttClient.Publish(sensor.StateTopic, value);
                    }
                }
            }
            else
                _logger.Log(LogLevel.Information, "Updated paused until connected...{0}", config.server);

        }

        public void UpdateUserSettings(UserSettings userSettings, int counter)
        {
            if (MqttClient.IsConnected)
            {
                foreach (SensorConfig userVal in userValues)
                {
                    userVal.Config = config;
                    if (counter % userVal.UpdatePeriod == 0)
                    {
                        // publish a message on "/home/temperature" topic with QoS 2 
                        byte[] value = UserValueByte(userSettings, userVal.Name);
                        MqttClient.Publish(userVal.StateTopic, value);
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
            if (sensorName == sensorValues[12].Name)
            {
                // time to charge. ( 100 - Battery % * Ah) / current
                if (opProps.battery.BatteryChargingCurrent > 0)
                {

                    decimal remCap = 1 - ((decimal)opProps.battery.BatteryCapacity / 100);
                    decimal ahRem = (remCap * inverterConfig.BatteryAmpHours);
                    decimal hRem = (ahRem / (decimal)opProps.battery.BatteryChargingCurrent);
                    decimal minutes = hRem * 60;

                    //_logger.Log(LogLevel.Information, "Charge..{0}:{1}:{2}:{3}", remCap, ahRem, hRem, minutes);
                    return minutes;
                }
                else return 0;
            }
            if (sensorName == sensorValues[13].Name)
            {
                // time to discharge. ( ( Battery % - (DoD * 100) * Ah) / current
                if (opProps.battery.BatteryDischargeCurrent > 0)
                {
                    decimal capRem = opProps.battery.BatteryCapacity - (100 * inverterConfig.DoD);
                    decimal cap = (capRem / 100);
                    decimal ahRem = (cap * inverterConfig.BatteryAmpHours);
                    decimal hRem = (ahRem / (decimal)opProps.battery.BatteryDischargeCurrent);
                    decimal minutes = hRem * 60;

                    //_logger.Log(LogLevel.Information, "DisCharge..{0}:{1}:{2}:{3}", minutes, opProps.battery.BatteryCapacity, inverterConfig.BatteryAmpHours, opProps.battery.BatteryDischargeCurrent);
                    return minutes;
                }
                else return 0;

            }
            if (sensorName == sensorValues[14].Name) return opProps.inverter.GridVoltage;

            return 0;
        }

        private byte[] SensorValueByte(OperatingProps opProps, string sensorName)
        {
            if (sensorName == sensorValues[11].Name)
            {
                if (opProps.inverter.ModeId == DeviceModes.Battery)
                {
                    if ((opProps.inverter.DeviceStatus1 & DeviceStatus1Flags.BatteryChargingSCC) == DeviceStatus1Flags.BatteryChargingSCC)
                    {
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

        private byte[] UserValueByte(UserSettings userSettings, string sensorName)
        {
            if (sensorName == userValues[0].Name) // output priority
            {
                if (userSettings.inverter.OutputSourcePriority == OutputSourcePriority.UtilityFirst)    
                    return Encoding.UTF8.GetBytes("USB");
                else if (userSettings.inverter.OutputSourcePriority == OutputSourcePriority.SolarFirst)    
                    return Encoding.UTF8.GetBytes("SUB");
                else if (userSettings.inverter.OutputSourcePriority == OutputSourcePriority.SBUFirst)    
                    return Encoding.UTF8.GetBytes("SBU");
            }
            else if (sensorName == userValues[1].Name) // charge priority
            {
                if (userSettings.inverter.ChargerSourcePriority == ChargerSourcePriority.UtilityFirst)    
                    return Encoding.UTF8.GetBytes("Grid First");
                else if (userSettings.inverter.ChargerSourcePriority == ChargerSourcePriority.SolarFirst)    
                    return Encoding.UTF8.GetBytes("Solar First");
                else if (userSettings.inverter.ChargerSourcePriority == ChargerSourcePriority.SolarUtility)    
                    return Encoding.UTF8.GetBytes("Solar - Grid");
                else if (userSettings.inverter.ChargerSourcePriority == ChargerSourcePriority.OnlySolar)    
                    return Encoding.UTF8.GetBytes("Only Solar");                    
            }
            return null;
        }

    }
}