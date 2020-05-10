
using System;
using System.Threading;
using Microsoft.Extensions.Logging;
using inverter.common.model;

namespace inverter.service.workers
{
  public class UpdateWorker
  {
    private CancellationToken _token;
    private ILogger<Worker> _logger;
    private AppSettings _appSettings;

    public UpdateWorker(CancellationToken token, ILogger<Worker> logger, AppSettings appSettings)
    {
      _token = token;
      _logger = logger;
      _appSettings = appSettings;
    }

    public void Run()
    {

      while (!_token.IsCancellationRequested)
      {

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

  }
}