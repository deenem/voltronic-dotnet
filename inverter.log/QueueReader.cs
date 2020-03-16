using System;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.IO;

namespace Inverter.Log
{
  public static class QueueReader
  {

    public static void ReadQueue(string QueueHost)
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
          consumer.Received += (model, ea) =>
          {

            IBasicProperties props = ea.BasicProperties;
            if (props.Headers != null)
            {
              foreach (var key in props.Headers.Keys)
              {
                byte[] b = (byte[])props.Headers[key];
                Console.WriteLine(" Header {0}:{1}", key, Encoding.Default.GetString(b));
              }
            }
            var body = ea.Body;
            var message = Encoding.UTF8.GetString(body);
            Console.WriteLine(" [x] Received {0}", message);
          };
          channel.BasicConsume(queue: "inverter",
                              autoAck: true,
                              consumer: consumer);

          Console.WriteLine(" Press [enter] to exit.");
          Console.ReadLine();
        }
      }
    }
  }
}