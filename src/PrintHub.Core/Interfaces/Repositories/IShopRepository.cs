using PrintHub.Core.Entities;

namespace PrintHub.Core.Interfaces.Repositories;

public interface IShopRepository
{
    Task<Shop?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Shop>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Shop> CreateAsync(Shop shop, CancellationToken cancellationToken = default);
}
