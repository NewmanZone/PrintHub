using PrintHub.Core.Entities;

namespace PrintHub.Core.Interfaces.Repositories;

public interface IPrintFileVersionRepository
{
    Task<PrintFileVersion?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IEnumerable<PrintFileVersion>> GetByPrintFileIdAsync(Guid printFileId, CancellationToken ct = default);
    Task<PrintFileVersion> AddAsync(PrintFileVersion version, CancellationToken ct = default);
    Task UpdateAsync(PrintFileVersion version, CancellationToken ct = default);
}