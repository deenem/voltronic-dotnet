using System;
using System.IO;
using Microsoft.Extensions.Configuration;

namespace Inverter.Log
{
  class Program
  {
    static void Main(string[] args)
    {
      var builder = new ConfigurationBuilder()
      .SetBasePath(Directory.GetCurrentDirectory())
      .AddJsonFile("appSettings.json", optional: true, reloadOnChange: true);

      IConfigurationRoot configuration = builder.Build();

      var queueHost = configuration.GetConnectionString("RabbitMQHost");
      if (queueHost == null)
        queueHost = "localhost";

      QueueReader.ReadQueue(queueHost);
    }
  }
}
