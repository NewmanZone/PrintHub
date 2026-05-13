using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace PrintHub.Worker.Functions;

public abstract class WorkerBase : BackgroundService
{
    protected readonly ILogger _logger;

    protected WorkerBase(ILogger logger)
    {
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("{WorkerName} starting at: {Time}",
            GetType().Name, DateTime.UtcNow);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await DoWorkAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in {WorkerName}", GetType().Name);
            }

            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }
    }

    protected abstract Task DoWorkAsync(CancellationToken stoppingToken);
}
