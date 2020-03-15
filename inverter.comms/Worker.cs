using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using System.Text;
using System.IO;
using System.Diagnostics;
using Microsoft.Extensions.Options;
using inverter.comms.model;
using inverter.common.model;


namespace inverter.comms
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IOptions<AppSettings> _appConfig;

        public Worker(ILogger<Worker> logger, IOptions<AppSettings> appConfig )
        {
            _logger = logger;
            _appConfig = appConfig;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var factory = new ConnectionFactory() { HostName = _appConfig.Value.RabbitMQHost };
            using(var connection = factory.CreateConnection()) 
            {
                using(var model = connection.CreateModel()) 
                {
                    model.QueueDeclare(queue: "inverter",
                                     durable: false,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: null);

                    while (!stoppingToken.IsCancellationRequested)
                    {

                        //QueryDeviceStatus(model);
                        QueuryDeviceRating(model);
                        PublishQFLAGS(model);    
                        await Task.Delay(1000, stoppingToken);
                    }
                }
            }
        }

        private void QueryDeviceStatus(IModel model) {
            string message = SendCommand(DeviceStatus.COMMAND);

            if (message.StartsWith(DeviceStatus.SUCCESS)) {

                var result = new DeviceStatus(message);
                IBasicProperties props = model.CreateBasicProperties();
                props.ContentType = "application/json";
                props.DeliveryMode = 2;
                props.Headers = new Dictionary<string, object>();
                props.Headers.Add("command",  DeviceStatus.COMMAND);


                model.BasicPublish(exchange: "",
                    routingKey: "inverter",
                    basicProperties: props,
                    body: result.toJSON());
            } else 
                _logger.LogInformation($"Error in Command {DeviceStatus.COMMAND} : {message}");
            
        }

        private void QueuryDeviceRating(IModel model) 
        {
            string message = SendCommand(DeviceRating.COMMAND);

            if (message.StartsWith(DeviceRating.SUCCESS)) {
                var result = new DeviceRating(message);

                IBasicProperties props = model.CreateBasicProperties();
                props.ContentType = "application/json";
                props.DeliveryMode = 2;
                props.Headers = new Dictionary<string, object>();
                props.Headers.Add("command",  DeviceRating.COMMAND);

                model.BasicPublish(exchange: "",
                    routingKey: "inverter",
                    basicProperties: props,
                    body: result.toJSON());
            }else 
                _logger.LogInformation($"Error in Command {DeviceRating.COMMAND} : {message}");
        }

        private void PublishQFLAGS(IModel model) 
        {
            string message = SendCommand("QFLAG");


            if (message.StartsWith("SUCCESS:QFLAG=>(")) {
                var body = Encoding.UTF8.GetBytes(message);

                IBasicProperties props = model.CreateBasicProperties();
                props.ContentType = "text/plain";
                props.DeliveryMode = 2;
                props.Headers = new Dictionary<string, object>();
                props.Headers.Add("command",  "QFLAG");

                model.BasicPublish(exchange: "",
                    routingKey: "inverter",
                    basicProperties: null,
                    body: body);
            }
        }

        private string SendCommand(string cmd) {

            using (Process process = new Process())
            {
                process.StartInfo.FileName = _appConfig.Value.LibVoltronicPath;
                process.StartInfo.Arguments = cmd;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.Start();

                // Synchronously read the standard output of the spawned process. 
                StreamReader reader = process.StandardOutput;
                string output = reader.ReadToEnd();
                process.WaitForExit();

                // Write the redirected output to this application's window.
                return output;
               
            }
        }
    }
}
