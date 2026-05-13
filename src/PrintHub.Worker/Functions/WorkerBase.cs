using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace PrintHub.Worker.Functions;

public abstract class WorkerBase : BackgroundService
{
    private static readonly Action<ILogger, string, DateTime, Exception?> LogWorkerStarting =
        LoggerMessage.Define<string, DateTime>(
            LogLevel.Information,
            new EventId(1, nameof(LogWorkerStarting)),
            "{WorkerName} starting at: {Time}");

    private static readonly Action<ILogger, string, Exception?> LogWorkerError =
        LoggerMessage.Define<string>(
            LogLevel.Error,
            new EventId(2, nameof(LogWorkerError)),
            "Error in {WorkerName}");

    private readonly ILogger _logger;

    protected WorkerBase(ILogger logger)
    {
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        LogWorkerStarting(_logger, GetType().Name, DateTime.UtcNow, null);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await DoWorkAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                LogWorkerError(_logger, GetType().Name, ex);
            }

            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }
    }

    protected abstract Task DoWorkAsync(CancellationToken stoppingToken);
}
