using PrintHub.Core.Entities;

namespace PrintHub.Core.Interfaces.Repositories;

public interface IPrintJobItemRepository
{
    Task<PrintJobItem?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IEnumerable<PrintJobItem>> GetByPrintJobIdAsync(Guid printJobId, CancellationToken ct = default);
    Task<PrintJobItem> AddAsync(PrintJobItem item, CancellationToken ct = default);
    Task<PrintJobItem> UpdateAsync(PrintJobItem item, CancellationToken ct = default);
}