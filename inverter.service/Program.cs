using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting.Systemd;
using inverter.service.model;

namespace inverter.service
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
              .UseSystemd()
              .ConfigureAppConfiguration((hostingContext, config) =>
              {
                config.AddJsonFile("appsettings.json", optional: true);
                config.AddEnvironmentVariables();

                if (args != null)
                {
                  config.AddCommandLine(args);
                }
              })            
              .ConfigureServices((hostContext, services) =>
              {
                  services.AddHostedService<Worker>();
                  services.Configure<AppSettings>(hostContext.Configuration.GetSection("AppSettings"));
              });
    }
}
