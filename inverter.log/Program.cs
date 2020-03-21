using System.IO;
using Microsoft.Extensions.Configuration;

namespace Inverter.Log
{
  class Program
  {

    private static QueueReader queueReader = new QueueReader();
    static void Main(string[] args)
    {
      var builder = new ConfigurationBuilder()
      .SetBasePath(Directory.GetCurrentDirectory())
      .AddJsonFile("appSettings.json", optional: true, reloadOnChange: true);

      IConfigurationRoot configuration = builder.Build();

      var queueHost = configuration.GetConnectionString("RabbitMQHost");
      if (queueHost == null)
        queueHost = "localhost";

      Program.queueReader.ReadQueue(queueHost);
    }
  }
}
