using PrintHub.Core.Entities;

namespace PrintHub.Core.Interfaces.Repositories;

public interface IPrintFileRepository
{
    Task<PrintFile?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<PrintFile?> GetByPartIdWithVersionsAsync(Guid partId, CancellationToken ct = default);
    Task<PrintFile> AddAsync(PrintFile printFile, CancellationToken ct = default);
}