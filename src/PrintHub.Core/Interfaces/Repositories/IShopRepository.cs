using PrintHub.Core.Entities;

namespace PrintHub.Core.Interfaces.Repositories;

public interface IShopRepository
{
    Task<Shop?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Shop?> GetByIdWithProductsAndPartsAsync(Guid id, CancellationToken ct = default);
    Task<IEnumerable<Shop>> GetByUserIdAsync(Guid userId, CancellationToken ct = default);
    Task<IEnumerable<Shop>> GetByWorkspaceIdAsync(Guid workspaceId, CancellationToken ct = default);
    Task<Shop?> GetByWorkspaceAndExternalIdAsync(Guid workspaceId, string externalId, CancellationToken ct = default);
    Task<Shop> AddAsync(Shop shop, CancellationToken ct = default);
    Task<Shop> UpdateAsync(Shop shop, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}
