using PrintHub.Core.Entities;

namespace PrintHub.Core.Interfaces.Services;

public interface IPrintQueueService
{
    Task<PrintJob> CreateJobAsync(Guid shopId, List<PrintJobItem> items, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PrintJob>> GetJobsByShopIdAsync(Guid shopId, CancellationToken cancellationToken = default);
}
