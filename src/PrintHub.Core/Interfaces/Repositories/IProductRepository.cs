using PrintHub.Core.Entities;

namespace PrintHub.Core.Interfaces.Repositories;

public interface IProductRepository
{
    Task<IEnumerable<Product>> GetAllAsync(CancellationToken ct = default) =>
        Task.FromResult(Enumerable.Empty<Product>());
    Task<Product?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Product?> GetByIdWithPartsAsync(Guid id, CancellationToken ct = default);
    Task<Product?> GetByExternalListingIdAsync(string externalListingId, Guid shopId, CancellationToken ct = default);
    Task<IEnumerable<Product>> GetByShopIdAsync(Guid shopId, CancellationToken ct = default);
    Task<IEnumerable<Product>> GetByShopIdWithPartsAsync(Guid shopId, CancellationToken ct = default);
    Task<IEnumerable<Product>> SearchByNameAsync(Guid shopId, string searchTerm, CancellationToken ct = default);
    Task<IEnumerable<Product>> GetBelowReorderPointAsync(Guid shopId, CancellationToken ct = default);
    Task<Product> AddAsync(Product product, CancellationToken ct = default);
    Task<Product> UpdateAsync(Product product, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}
