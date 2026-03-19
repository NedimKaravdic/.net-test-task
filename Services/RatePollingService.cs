using EHSExchangeDashboard.Common;
using EHSExchangeDashboard.Interfaces;

namespace EHSExchangeDashboard.Services;

public class RatePollingService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<RatePollingService> _logger;
    private readonly IHostApplicationLifetime _appLifetime;
    private readonly TimeSpan _pollInterval;

    public RatePollingService(
        IServiceScopeFactory scopeFactory,
        IConfiguration configuration,
        ILogger<RatePollingService> logger,
        IHostApplicationLifetime appLifetime)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _appLifetime = appLifetime;

        var minutes = configuration.GetValue<int>("PollingIntervalMinutes", AppConstants.DefaultPollingInterval);
        _pollInterval = TimeSpan.FromMinutes(minutes);
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
       
        var startupTcs = new TaskCompletionSource();
        using var registration = _appLifetime.ApplicationStarted.Register(() => startupTcs.SetResult());
        
        _logger.LogInformation("Rate polling service waiting for application to start...");
        
       
        await Task.WhenAny(startupTcs.Task, Task.Delay(Timeout.Infinite, cancellationToken));

        if (cancellationToken.IsCancellationRequested) return;

        _logger.LogInformation("Rate polling service started. Poll interval: {Interval}", _pollInterval);

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                _logger.LogInformation("Background Sync: Polling /live endpoint for fresh market data...");
                
                using (var scope = _scopeFactory.CreateScope())
                {
                    var exchangeService = scope.ServiceProvider.GetRequiredService<IExchangeService>();
                    await exchangeService.SyncRatesAsync();
                }

                _logger.LogInformation("Background Sync complete. Market data refreshed in Redis at {Time}", DateTime.Now);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during background market sync.");
            }

            try 
            {
                await Task.Delay(_pollInterval, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }
}
