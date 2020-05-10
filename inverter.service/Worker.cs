using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using inverter.service.workers;
using inverter.common.model;

namespace inverter.service
{
  public class Worker : BackgroundService
  {
    private readonly ILogger<Worker> _logger;
    private readonly IOptions<AppSettings> _appConfig;

    ConcurrentBag<Task> tasks = new ConcurrentBag<Task>();
    public Worker(ILogger<Worker> logger, IOptions<AppSettings> appConfig)

    {
      _logger = logger;
      _appConfig = appConfig;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {

      Task t1 = Task.Factory.StartNew(() =>
      {
        var queryWorker = new QueryWorker(stoppingToken, _logger, _appConfig.Value);
        queryWorker.Run();

      }, stoppingToken);
      tasks.Add(t1);

      Task t2 = Task.Factory.StartNew(() =>
      {
        var updateWorker = new UpdateWorker(stoppingToken, _logger, _appConfig.Value);
        updateWorker.Run();

      }, stoppingToken);
      tasks.Add(t2);

      while (!stoppingToken.IsCancellationRequested)
      {
        await Task.Delay(2000, stoppingToken);
      }

      try
      {
        Task.WaitAll(tasks.ToArray());
      }
      catch (AggregateException e)
      {
        _logger.LogError("AggregateException thrown with the following inner exceptions:");
        // Display information about each exception.  
        foreach (var v in e.InnerExceptions)
        {
          if (v is TaskCanceledException)
            _logger.LogError("   TaskCanceledException: Task {0}", ((TaskCanceledException)v).Task.Id);
          else
            _logger.LogError("   Exception: {0}", v.GetType().Name);
        }
      }
    }
  }
}
