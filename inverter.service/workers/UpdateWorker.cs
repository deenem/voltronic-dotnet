
using System;
using System.Net;
using System.Text;
using System.Threading;
using System.IO;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using inverter.common.model;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;

namespace inverter.service.workers
{
  public class UpdateWorker
  {
    private CancellationToken _token;
    private ILogger<Worker> _logger;
    private AppSettings _appSettings;
    private MQTT _mqttConfig;
    private string clientId = "invertUpd";

    public UpdateWorker(CancellationToken token, ILogger<Worker> logger, AppSettings appSettings)
    {
      _token = token;
      _logger = logger;
      _appSettings = appSettings;
      _mqttConfig = appSettings.MQTT;
    }

    public void Run()
    {

      ConnectAndSubscribe();

      while (!_token.IsCancellationRequested)
      {

        ConnectAndSubscribe();
        try
        {

          Thread.Sleep(1000);
        }
        catch (Exception ex)
        {
          _logger.LogError(2, ex, ex.Message);
        }
      }

    }

    private void ConnectAndSubscribe() 
    {
      // create client instance
      MqttClient client = new MqttClient(_mqttConfig.server);
      client.Connect(clientId, _mqttConfig.username, _mqttConfig.password, true, 10);

      // register to message received
      client.MqttMsgPublishReceived += client_MqttMsgPublishReceived;
 
      // subscribe to the topic "/home/temperature" with QoS 2
      client.Subscribe(new string[] { "inverter/command/#" }, new byte[] { MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE });
      
    }

    private void client_MqttMsgPublishReceived(object sender, MqttMsgPublishEventArgs e)
    {
      // handle message received
      var byteArray = e.Message;
      string cmd = Encoding.UTF8.GetString(byteArray, 0, byteArray.Length);
      _logger.Log(LogLevel.Information, "Message Received.. {0}", cmd);
      string resp = SendCommand(cmd);
      _logger.Log(LogLevel.Information, "Response.. {0}", resp);
    }

    private string SendCommand(string cmd)
    {
      using (Process process = new Process())
      {
        process.StartInfo.FileName = _appSettings.LibVoltronicPath;
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