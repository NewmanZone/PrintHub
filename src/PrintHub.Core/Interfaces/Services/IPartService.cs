using PrintHub.Core.Entities;

namespace PrintHub.Core.Interfaces.Services;

public interface IPartService
{
    Task<Part?> GetByIdAsync(Guid partId, CancellationToken ct = default);
    Task<Part?> GetByIdWithVersionsAsync(Guid partId, CancellationToken ct = default);
    Task<IEnumerable<Part>> GetByShopIdAsync(Guid shopId, bool? isGeneric = null, CancellationToken ct = default);
    Task<IEnumerable<Part>> GetWithLowStockAsync(Guid shopId, CancellationToken ct = default);
    Task<Part> CreateAsync(Guid shopId, string name, string? description, bool isGeneric, decimal costPerUnit, CancellationToken ct = default);
    Task<Part> UpdateAsync(Guid partId, string? name = null, string? description = null, decimal? costPerUnit = null, bool? isGeneric = null, CancellationToken ct = default);
    Task DeleteAsync(Guid partId, CancellationToken ct = default);
    Task<PrintFileVersion> UploadFileVersionAsync(Guid partId, string fileName, string fileType, long fileSizeBytes, string filePath, string fileHash, string? notes = null, CancellationToken ct = default);
    Task<Part> SetCurrentVersionAsync(Guid partId, Guid versionId, CancellationToken ct = default);
}