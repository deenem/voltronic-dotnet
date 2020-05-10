
using System;
using uPLibrary.Networking.M2Mqtt;
using inverter.common.model;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Text;
using inverter.common.model.messages;
using uPLibrary.Networking.M2Mqtt.Messages;

namespace inverter.mqtt
{
    public class MqttWorker
    {
        public string[,] sensorValues = new string[,] {
            { "AC_grid_voltage","V" ,"power-plug" },
            { "AC_out_voltage","V","power-plug" },
            { "PV_in_voltage","V","solar-panel-large" },
            { "PV_in_current","A","solar-panel-large" },
            { "PV_in_watts","W","solar-panel-large" },
            { "PV_in_watthour","Wh","solar-panel-large" },
            { "SCC_voltage","V","current-dc" },
            { "load_pct","%","brightness-percent" },
            { "load_watt", "W", "chart-bell-curve" },
            { "load_watthour","Wh","chart-bell-curve" },
            { "load_va","VA","chart-bell-curve" },
            { "bus_voltage","V","details" },
            { "heatsink_temperature","C","details" },
            { "battery_capacity","%","battery-outline" },
            { "battery_voltage","V","battery-outline" },
            { "battery_charge_current","A","current-dc" },
            { "battery_discharge_current","A","current-dc" }
        };

        private readonly ILogger<Worker> _logger;
        private MQTT config;
        public MqttClient MqttClient { get; private set; }

        public MqttWorker(ILogger<Worker> logger, AppSettings appSettings)
        {
            _logger = logger;
            config = appSettings.MQTT;
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

            if (MqttClient.IsConnected)
                MqttClient.Disconnect();
        }

        private async Task ConnectAndInitialiseMQTT(CancellationToken stoppingToken)
        {
            string clientId = Environment.GetEnvironmentVariable("COMPUTERNAME");

            var connected = MqttClient.IsConnected;
            while (!connected && !stoppingToken.IsCancellationRequested)
            {
                try
                {
                    
                    MqttClient.Connect(clientId, config.username, config.password);
                    connected = MqttClient.IsConnected;
                    _logger.Log(LogLevel.Information, "MQTT Conected to to...{0}", config.server);
                }
                catch
                {
                    _logger.Log(LogLevel.Warning, "No connection to...{0}", config.server);
                    await Task.Delay(10000, stoppingToken);
                }
            }

            // After connect, create sensors
            string sensorName = $"{config.topic}/sensor/{config.devicename}/config";
            string sensorMsg = $"{{\"name\": \"{config.devicename}\",\"state_topic\": \"{config.topic}/sensor/{config.devicename}\"}}";
            MqttClient.Publish(sensorName, Encoding.UTF8.GetBytes(sensorMsg));

            for (int i = 0; i < sensorValues.GetLength(0); ++i)
            {
                _logger.Log(LogLevel.Information, "Item...{0}:{1}:{2}", sensorValues[i, 0], sensorValues[i, 1], sensorValues[i, 2]);

                sensorName = $"{config.topic}/sensor/{config.devicename}_{sensorValues[i, 0]}/config";
                sensorMsg = $"{{\"name\": \"{config.devicename}_{sensorValues[i, 0]}\",\"unit_of_measurement\": \"{sensorValues[i, 1]}\",\"state_topic\": \"{config.topic}/sensor/{config.devicename}_{sensorValues[i, 0]}\",\"icon\": \"mdi:{sensorValues[0, 2]}\"}}";
                MqttClient.Publish(sensorName, Encoding.UTF8.GetBytes(sensorMsg));
            }
        }

        public void Update(OperatingProps opProps)
        {
            if (MqttClient.IsConnected)
            {
                for (int i = 0; i < sensorValues.GetLength(0); ++i)
                {
                    string sensorName = $"{config.topic}/sensor/{config.devicename}_{sensorValues[i, 0]}";
                    string sensorValue = SensorValue(opProps, sensorValues[i, 0]);
                    // publish a message on "/home/temperature" topic with QoS 2 
                    MqttClient.Publish(sensorName, Encoding.UTF8.GetBytes(sensorValue));
                }
            }
            else
                _logger.Log(LogLevel.Information, "Updated paused until connected...{0}", config.server);

        }

        private string SensorValue(OperatingProps opProps, string sensorName)
        {
            if (sensorName == sensorValues[0, 0]) return Convert.ToString(opProps.inverter.GridVoltage);
            if (sensorName == sensorValues[1, 0]) return Convert.ToString(opProps.inverter.ACOutputVoltage);
            if (sensorName == sensorValues[2, 0]) return Convert.ToString(opProps.solar.PVInputVoltage1);
            if (sensorName == sensorValues[3, 0]) return Convert.ToString(opProps.solar.PVInputCurrentForBattery);
            if (sensorName == sensorValues[4, 0]) return "-";
            if (sensorName == sensorValues[5, 0]) return "-";
            if (sensorName == sensorValues[6, 0]) return Convert.ToString(opProps.solar.BatteryVoltageFromSCC);
            if (sensorName == sensorValues[7, 0]) return Convert.ToString(opProps.inverter.OutputLoadPercent);
            if (sensorName == sensorValues[8, 0]) return Convert.ToString(opProps.inverter.ACOutputActivePower);
            if (sensorName == sensorValues[9, 0]) return "-";
            if (sensorName == sensorValues[10, 0]) return Convert.ToString(opProps.inverter.ACOutputApparentPower);
            if (sensorName == sensorValues[11, 0]) return Convert.ToString(opProps.battery.BusVoltage);
            if (sensorName == sensorValues[12, 0]) return Convert.ToString(opProps.enviroment.HeatSinkTemperature);
            if (sensorName == sensorValues[13, 0]) return Convert.ToString(opProps.battery.BatteryCapacity);
            if (sensorName == sensorValues[14, 0]) return Convert.ToString(opProps.battery.BatteryVoltage);
            if (sensorName == sensorValues[15, 0]) return Convert.ToString(opProps.battery.BatteryChargingCurrent);
            if (sensorName == sensorValues[16, 0]) return Convert.ToString(opProps.battery.BatteryDischargeCurrent);
            return "";
        }
    }
}