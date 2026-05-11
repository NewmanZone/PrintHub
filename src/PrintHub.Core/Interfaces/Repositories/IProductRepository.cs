using PrintHub.Core.Entities;

namespace PrintHub.Core.Interfaces.Repositories;

public interface IProductRepository
{
    Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Product>> GetByShopIdAsync(Guid shopId, CancellationToken cancellationToken = default);
    Task<Product> CreateAsync(Product product, CancellationToken cancellationToken = default);
    Task UpdateAsync(Product product, CancellationToken cancellationToken = default);
}
