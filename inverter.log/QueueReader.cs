using System;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.IO;
using System.Net.Http;
using Newtonsoft.Json;
using inverter.common.model.messages;

namespace Inverter.Log
{
  public class QueueReader
  {

    private static readonly HttpClient client = new HttpClient();

    public void ReadQueue(string QueueHost)
    {
      var factory = new ConnectionFactory() { UserName = "inverter", Password = "inverter", HostName = QueueHost };
      using (var connection = factory.CreateConnection())
      {
        using (var channel = connection.CreateModel())
        {
          channel.QueueDeclare(queue: "inverter",
                              durable: false,
                              exclusive: false,
                              autoDelete: false,
                              arguments: null);

          var consumer = new EventingBasicConsumer(channel);
          consumer.Received += onReceived;
          channel.BasicConsume(queue: "inverter",
                              autoAck: true,
                              consumer: consumer);

          Console.WriteLine(" Press [enter] to exit.");
          Console.ReadLine();
        }
      }
    }

    private void onReceived(object sender, BasicDeliverEventArgs ea)
    {

      IBasicProperties props = ea.BasicProperties;
      string msgType = "";
      if (props.Headers != null)
      {
        foreach (var key in props.Headers.Keys)
        {
          byte[] b = (byte[])props.Headers[key];
          msgType = Encoding.Default.GetString(b);
          Console.WriteLine(" Header {0}:{1}", key, msgType);
        }
      }
      var body = ea.Body.Span;
      var message = Encoding.UTF8.GetString(body);
      Console.WriteLine(" [x] Received {0}", message);

      EcoMonCMSPost(msgType, message);

    }

    private void EcoMonCMSPost(string msgType, string message)
    {

      if (msgType == "OpProp")
      {
        OperatingProps opProps = JsonConvert.DeserializeObject<OperatingProps>(message);
        string json = JsonConvert.SerializeObject(opProps.inverter, Formatting.Indented);

        string url = $"http://axpi4/input/post?node=emonpi&fulljson={{\"power1\":\"{opProps.inverter.ACOutputActivePower}\"}}&apikey=c84bdc0fc6f3d3cf7b8f88cb0073d622";
        Console.WriteLine(" [x] Sending {0}", url);

        var response = client.GetAsync(url);
      }

    }
  }
}