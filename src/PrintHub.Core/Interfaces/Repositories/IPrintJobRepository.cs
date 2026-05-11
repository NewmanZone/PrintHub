using PrintHub.Core.Entities;

namespace PrintHub.Core.Interfaces.Repositories;

public interface IPrintJobRepository
{
    Task<PrintJob?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PrintJob>> GetByShopIdAsync(Guid shopId, CancellationToken cancellationToken = default);
    Task<PrintJob> CreateAsync(PrintJob job, CancellationToken cancellationToken = default);
    Task UpdateAsync(PrintJob job, CancellationToken cancellationToken = default);
}
