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
using inverter.common.model.messages;


namespace inverter.comms
{
  public class Worker : BackgroundService
  {
    private readonly ILogger<Worker> _logger;
    private readonly IOptions<AppSettings> _appConfig;

    private static FixedProps fixedProps = new FixedProps();
    private static UserSettings userSettings = new UserSettings();
    private static OperatingProps operatingProps = new OperatingProps();

    public Worker(ILogger<Worker> logger, IOptions<AppSettings> appConfig)

    {
      _logger = logger;
      _appConfig = appConfig;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
      var factory = new ConnectionFactory() { HostName = _appConfig.Value.RabbitMQHost };
      using (var connection = factory.CreateConnection())
      {
        using (var model = connection.CreateModel())
        {
          model.QueueDeclare(queue: "inverter",
                           durable: false,
                           exclusive: false,
                           autoDelete: false,
                           arguments: null);


          int counter = 0;
          while (!stoppingToken.IsCancellationRequested)
          {

            var status = QueryDeviceStatus();
            var rating = QueuryDeviceRating();
            var flags = QueryDeviceFlags();

            if (status != null)
            {
              userSettings.Update(status);
              fixedProps.Update(status);
              operatingProps.Update(status);
            }

            if (rating != null)
            {
              userSettings.Update(rating);
              fixedProps.Update(rating);
              operatingProps.Update(rating);
            }

            if (flags != null)
            {
              userSettings.Update(flags);
              fixedProps.Update(flags);
              operatingProps.Update(flags);
            }


            // always send Operating Props
            SendOperatingProps(model);
            // UserSettings every 10 secs
            if ((counter % 10 == 0))
              SendUserSettings(model);
            // UserSettings every 30 secs, will add setting later
            if ((counter % 30 == 0))
            {
              SendUserSettings(model);
              counter = 0;
            }

            counter += 1;

            await Task.Delay(1000, stoppingToken);
          }
        }
      }
    }

    private void SendFixedProps(IModel model)
    {
      IBasicProperties props = model.CreateBasicProperties();
      props.ContentType = "application/json";
      props.DeliveryMode = 2;
      props.Headers = new Dictionary<string, object>();
      props.Headers.Add("type", FixedProps.MESSAGE_TYPE);


      model.BasicPublish(exchange: "",
          routingKey: "inverter",
          basicProperties: props,
          body: fixedProps.toJSON());
    }
    private void SendUserSettings(IModel model)
    {
      IBasicProperties props = model.CreateBasicProperties();
      props.ContentType = "application/json";
      props.DeliveryMode = 2;
      props.Headers = new Dictionary<string, object>();
      props.Headers.Add("command", UserSettings.MESSAGE_TYPE);


      model.BasicPublish(exchange: "",
          routingKey: "inverter",
          basicProperties: props,
          body: userSettings.toJSON());
    }
    private void SendOperatingProps(IModel model)
    {
      IBasicProperties props = model.CreateBasicProperties();
      props.ContentType = "application/json";
      props.DeliveryMode = 2;
      props.Headers = new Dictionary<string, object>();
      props.Headers.Add("command", OperatingProps.MESSAGE_TYPE);


      model.BasicPublish(exchange: "",
          routingKey: "inverter",
          basicProperties: props,
          body: operatingProps.toJSON());
    }

    private DeviceStatus QueryDeviceStatus()
    {
      string message = SendCommand(DeviceStatus.COMMAND);

      if (DeviceStatus.CanProcess(message))
        return new DeviceStatus(message);
      else
        _logger.LogInformation($"Error in Command {DeviceStatus.COMMAND} : {message}");
      return null;

    }

    private DeviceRating QueuryDeviceRating()
    {
      string message = SendCommand(DeviceRating.COMMAND);

      if (DeviceRating.CanProcess(message))
        return new DeviceRating(message);
      else
        _logger.LogInformation($"Error in Command {DeviceRating.COMMAND} : {message}");
      return null;
    }

    private DeviceFlags QueryDeviceFlags()
    {
      string message = SendCommand("QFLAG");
      if (DeviceFlags.CanProcess(message))
        return new DeviceFlags(message);
      else
        _logger.LogInformation($"Error in Command {DeviceRating.COMMAND} : {message}");
      return null;
    }

    private string SendCommand(string cmd)
    {

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
