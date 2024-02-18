using System;
using System.Diagnostics.Tracing;
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


namespace inverter.mqtt
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private IOptions<AppSettings> _appConfig;
        private IConnection rabbitConnection;
        private IModel rabbitModel;
        private static int counter = 0;

        private MqttWorker mqttWorker;
        public Worker(ILogger<Worker> logger, IOptions<AppSettings> appConfig)
        {
            _logger = logger;
            _appConfig = appConfig;

            mqttWorker = new MqttWorker(logger, appConfig.Value);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // create client instance 

            ConnectRabbitMQ();
            await mqttWorker.ConnectMQTT(stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {

                //_logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                await Task.Delay(1000);
            }
            _logger.LogInformation("Shutdown requested at: {time}", DateTimeOffset.Now);

            rabbitModel.Close();
            rabbitConnection.Close();
            mqttWorker.Disconnect();
        }

        private void ConnectRabbitMQ()
        {
            var rabbitConfig = _appConfig.Value.RabbitMQ;

            _logger.LogInformation("Rabbit connect: {0}",  _appConfig.Value.RabbitMQ);

            var factory = new ConnectionFactory() { UserName = rabbitConfig.username, Password = rabbitConfig.password, HostName = rabbitConfig.server };
            rabbitConnection = factory.CreateConnection();
            rabbitModel = rabbitConnection.CreateModel();

            rabbitModel.QueueDeclare(queue: "inverter",
                                durable: false,
                                exclusive: false,
                                autoDelete: false,
                                arguments: null);

            var consumer = new EventingBasicConsumer(rabbitModel);
            consumer.Received += onReceived;
            rabbitModel.BasicConsume(queue: "inverter",
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
                }
            }
            var body = ea.Body.Span;
            var message = Encoding.UTF8.GetString(body);

            _logger.LogInformation("RABBIT RECEIVED - " + msgType);

            if (mqttWorker.MqttClient.IsConnected)
            {
                //_logger.LogInformation(message);
                if (msgType.Equals(OperatingProps.MESSAGE_TYPE)) {
                    OperatingProps opProps = JsonConvert.DeserializeObject<OperatingProps>(message);
                    mqttWorker.UpdateOpProps(opProps, counter);
                }
                if (msgType.Equals(UserSettings.MESSAGE_TYPE)) {
                    UserSettings userSettings = JsonConvert.DeserializeObject<UserSettings>(message);
                    mqttWorker.UpdateUserSettings(userSettings, counter);
                }

                counter += 1;
                counter %= 100000;
            }
            else
                _logger.LogWarning("No Connection to Mqtt");
        }
    }
}
