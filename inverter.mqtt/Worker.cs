using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;

namespace inverter.mqtt
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;

        public Worker(ILogger<Worker> logger)
        {
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // create client instance 

            MqttClient client = new MqttClient("10.0.1.52");

            string clientId = Guid.NewGuid().ToString();
            client.Connect(clientId, "inverter", "inverter");

            Random random = new Random();
            string topic1 = "homeassistant/sensor/voltronic/config";
            string body1 = "{\"name\": \"voltronic\",\"state_topic\": \"$homeassistant/sensor/voltronic\"}";
            string topic2 = "homeassistant/sensor/voltronic_Load_watt/config";
            string body2 = "{\"name\": \"voltronic_Load_watt\",\"unit_of_measurement\": \"V\",\"state_topic\": \"homeassistant/sensor/voltronic_Load_watt\",\"icon\": \"mdi:chart-bell-curve\"}";
            string topic3 = "homeassistant/sensor/voltronic_Load_watt";

            client.Publish(topic1, Encoding.UTF8.GetBytes(body1));
            client.Publish(topic2, Encoding.UTF8.GetBytes(body2));

            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);

                string strValue = Convert.ToString(random.Next(0,35));
                // publish a message on "/home/temperature" topic with QoS 2 
                client.Publish(topic3, Encoding.UTF8.GetBytes(strValue));

                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}
