using PrintHub.Core.Entities;
using PrintHub.Core.Enums;

namespace PrintHub.Core.Interfaces.Repositories;

public interface IPrintJobRepository
{
    Task<PrintJob?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<PrintJob?> GetByIdWithItemsAsync(Guid id, CancellationToken ct = default);
    Task<IEnumerable<PrintJob>> GetByShopIdAsync(Guid shopId, PrintJobStatus? status = null, DateTime? from = null, DateTime? to = null, CancellationToken ct = default);
    Task<PrintJob> AddAsync(PrintJob printJob, CancellationToken ct = default);
    Task<PrintJob> UpdateAsync(PrintJob printJob, CancellationToken ct = default);
}