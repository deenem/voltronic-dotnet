using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using inverter.common.model;
using inverter.common.model.messages;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;

namespace inverter.mqtt
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private IOptions<AppSettings> _appConfig;
        private MqttClient mqttClient;

        public Worker(ILogger<Worker> logger, IOptions<AppSettings> appConfig)
        {
            _logger = logger;
            _appConfig = appConfig;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // create client instance 

            ConnectRabbitMQ();
            ConnectMQTT();

            while (!stoppingToken.IsCancellationRequested)
            {
                //_logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);

                await Task.Delay(1000, stoppingToken);
            }
        }

        private void ConnectRabbitMQ()
        {
            var rabbitConfig = _appConfig.Value.RabbitMQ;

            var factory = new ConnectionFactory() { UserName = rabbitConfig.username, Password = rabbitConfig.password, HostName = rabbitConfig.server };
            var connection = factory.CreateConnection();
            var model = connection.CreateModel();

                    model.QueueDeclare(queue: "inverter",
                                     durable: false,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: null);

            var consumer = new EventingBasicConsumer(model);
            consumer.Received += onReceived;
            model.BasicConsume(queue: "inverter",
                                autoAck: true,
                                consumer: consumer);


        }

        private void onReceived(object sender, BasicDeliverEventArgs ea)
        {
            var config = _appConfig.Value.MQTT;
            IBasicProperties props = ea.BasicProperties;
            string msgType = "";
            if (props.Headers != null)
            {
                foreach (var key in props.Headers.Keys)
                {
                    byte[] b = (byte[])props.Headers[key];
                    msgType = Encoding.Default.GetString(b);
                    //Console.WriteLine(" Header {0}:{1}", key, msgType);
                }
            }
            var body = ea.Body;
            var message = Encoding.UTF8.GetString(body);
            //Console.WriteLine(" [x] Received {0}", message);

            if (msgType == "OpProp")
            {
                string topic3 = $"{config.topic}/sensor/{config.devicename}_load_watt";
                OperatingProps opProps = JsonConvert.DeserializeObject<OperatingProps>(message);
                string json = JsonConvert.SerializeObject(opProps.inverter, Formatting.Indented);

                string strValue = Convert.ToString(opProps.inverter.ACOutputActivePower);
                // publish a message on "/home/temperature" topic with QoS 2 
                mqttClient.Publish(topic3, Encoding.UTF8.GetBytes(strValue));
            }

        }

        private void ConnectMQTT()
        {
            var config = _appConfig.Value.MQTT;
            mqttClient = new MqttClient(config.server);

            string clientId = Guid.NewGuid().ToString();
            mqttClient.Connect(clientId, config.username, config.password);

            Random random = new Random();
            string topic1 = $"{config.topic}/sensor/{config.devicename}/config";
            string body1 =  $"{{\"name\": \"{config.devicename}\",\"state_topic\": \"{config.topic}/sensor/{config.devicename}\"}}";
            string topic2 = $"{config.topic}/sensor/{config.devicename}_load_watt/config";
            string body2 = $"{{\"name\": \"{config.devicename}_load_watt\",\"unit_of_measurement\": \"V\",\"state_topic\": \"{config.topic}/sensor/{config.devicename}_load_watt\",\"icon\": \"mdi:chart-bell-curve\"}}";


            mqttClient.Publish(topic1, Encoding.UTF8.GetBytes(body1));
            mqttClient.Publish(topic2, Encoding.UTF8.GetBytes(body2));

        }
    }
}
