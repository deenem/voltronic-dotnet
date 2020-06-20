
using System;
using System.Threading;
using Microsoft.Extensions.Logging;
using inverter.common.model;
using inverter.common.model.messages;
using inverter.service.util;
using RabbitMQ.Client;
using System.Collections.Generic;
using System.Text;

namespace inverter.service.workers
{
  public class QueryWorker
  {


    private static FixedProps fixedProps = new FixedProps();
    private static UserSettings userSettings = new UserSettings();
    private static OperatingProps operatingProps = new OperatingProps();

    private int cyclePeriod = 1000; //milliseconds
    private int fixedPropPeriod = 60; // send every 10 periods
    private int userSettingsPeriod = 10; // send every 3 periods
    private int operatingPropsPeriod = 1; // send every period
    private CancellationToken _cancelToken;
    private ILogger<Worker> _logger;
    private AppSettings _appSettings;

    public QueryWorker(CancellationToken cancelToken, ILogger<Worker> logger, AppSettings appSettings)
    {
      _cancelToken = cancelToken;
      _logger = logger;
      _appSettings = appSettings;
    }

    public void Run()
    {

      var factory = new ConnectionFactory() { HostName = _appSettings.RabbitMQ.server };
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

          var rating = QueryDeviceRating();
          var flags = QueryDeviceFlags();
          var status = QueryDeviceStatus();
          var mode = QueryDeviceMode();

          while (!_cancelToken.IsCancellationRequested)
          {

            try
            {
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

              if (mode != null){
                operatingProps.Update(mode);
              }

              // always send Operating Props
              SendOperatingProps(model);
              // UserSettings every 10 secs
              if ((counter % userSettingsPeriod == 0))
                SendUserSettings(model);
              // UserSettings every 30 secs, will add setting later
              if ((counter % fixedPropPeriod == 0))
              {
                SendFixedProps(model);
                counter = 0;
              }

              if ((counter > fixedPropPeriod) && (counter > userSettingsPeriod))
                counter = 0;

              counter += 1;
              Thread.Sleep(cyclePeriod);

              status = QueryDeviceStatus();
              flags = QueryDeviceFlags();
              rating = QueryDeviceRating();
              mode = QueryDeviceMode();

            }
            catch (Exception ex)
            {
              _logger.LogError(1, ex, "QueryWorker:Run");
            }
          }
        }
      }
    }

    private void SendFixedProps(IModel model)
    {
      IBasicProperties props = model.CreateBasicProperties();
      props.ContentType = "application/json";
      props.DeliveryMode = 2;
      props.Expiration = $"{(fixedPropPeriod + 3) * cyclePeriod }";
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
      props.Expiration = $"{(userSettingsPeriod + 3) * cyclePeriod }";

      props.Headers = new Dictionary<string, object>();
      props.Headers.Add("type", UserSettings.MESSAGE_TYPE);


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
      props.Expiration = $"{(operatingPropsPeriod + 3) * cyclePeriod }";

      props.Headers = new Dictionary<string, object>();
      props.Headers.Add("type", OperatingProps.MESSAGE_TYPE);

      var bytes = operatingProps.toJSON();

      //_logger.LogInformation(Encoding.UTF8.GetString(bytes, 0, bytes.Length));
      model.BasicPublish(exchange: "",
          routingKey: "inverter",
          basicProperties: props,
          body: bytes);
    }

    private DeviceStatus QueryDeviceStatus()
    {
      string message = Command.SendCommand(_appSettings.LibVoltronicPath, DeviceStatus.COMMAND);

      if (DeviceStatus.CanProcess(message))
        return new DeviceStatus(message);
      else
        _logger.LogInformation($"Error in Command {DeviceStatus.COMMAND} : {message}");
      return null;

    }

    private DeviceRating QueryDeviceRating()
    {
      string message = Command.SendCommand(_appSettings.LibVoltronicPath, DeviceRating.COMMAND);

      if (DeviceRating.CanProcess(message))
        return new DeviceRating(message);
      else
        _logger.LogInformation($"Error in Command {DeviceRating.COMMAND} : {message}");
      return null;
    }

    private DeviceFlags QueryDeviceFlags()
    {
      string message = Command.SendCommand(_appSettings.LibVoltronicPath, DeviceFlags.COMMAND);

      if (DeviceFlags.CanProcess(message))
        return new DeviceFlags(message);
      else
        _logger.LogInformation($"Error in Command {DeviceFlags.COMMAND} : {message}");
      return null;
    }

    private DeviceMode QueryDeviceMode()
    {
      string message = Command.SendCommand(_appSettings.LibVoltronicPath, DeviceMode.COMMAND);
      if (DeviceMode.CanProcess(message))
        return new DeviceMode(message);
      else
        _logger.LogInformation($"Error in Command {DeviceMode.COMMAND} : {message}");
      return null;
    }
  }
}