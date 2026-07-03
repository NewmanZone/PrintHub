using PrintHub.Core.Entities;
using PrintHub.Core.Interfaces.Repositories;
using System.Collections.Concurrent;

namespace PrintHub.Infrastructure.Repositories;

public class InMemoryProductRepository : IProductRepository
{
    private readonly ConcurrentDictionary<Guid, Product> _products = new();

    public Task<IEnumerable<Product>> GetAllAsync(CancellationToken ct = default)
    {
        return Task.FromResult<IEnumerable<Product>>(_products.Values.ToList());
    }

    public Task<Product?> GetByExternalListingIdAsync(string externalListingId, Guid shopId, CancellationToken ct = default)
    {
        var product = _products.Values.FirstOrDefault(p => p.ExternalListingId == externalListingId && p.ShopId == shopId);
        return Task.FromResult(product);
    }

    public Task<Product?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        _products.TryGetValue(id, out var product);
        return Task.FromResult(product);
    }

    public Task<Product?> GetByIdWithPartsAsync(Guid id, CancellationToken ct = default)
    {
        _products.TryGetValue(id, out var product);
        return Task.FromResult(product);
    }

    public Task<IEnumerable<Product>> GetByShopIdAsync(Guid shopId, CancellationToken ct = default)
    {
        var products = _products.Values
            .Where(p => p.ShopId == shopId)
            .ToList();
        return Task.FromResult<IEnumerable<Product>>(products);
    }

    public Task<IEnumerable<Product>> GetByShopIdWithPartsAsync(Guid shopId, CancellationToken ct = default)
    {
        var products = _products.Values
            .Where(p => p.ShopId == shopId)
            .ToList();
        return Task.FromResult<IEnumerable<Product>>(products);
    }

    public Task<IEnumerable<Product>> GetBelowReorderPointAsync(Guid shopId, CancellationToken ct = default)
    {
        var products = _products.Values
            .Where(p => p.ShopId == shopId && p.ReorderPoint.HasValue && p.InventoryOnHand < p.ReorderPoint.Value)
            .ToList();
        return Task.FromResult<IEnumerable<Product>>(products);
    }

    public Task<IEnumerable<Product>> SearchByNameAsync(Guid shopId, string searchTerm, CancellationToken ct = default)
    {
        var products = _products.Values
            .Where(p => p.ShopId == shopId &&
                       p.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
            .ToList();
        return Task.FromResult<IEnumerable<Product>>(products);
    }

    public Task<Product> AddAsync(Product product, CancellationToken ct = default)
    {
        _products[product.Id] = product;
        return Task.FromResult(product);
    }

    public Task<Product> UpdateAsync(Product product, CancellationToken ct = default)
    {
        product.UpdatedAt = DateTime.UtcNow;
        _products[product.Id] = product;
        return Task.FromResult(product);
    }

    public Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        _products.TryRemove(id, out _);
        return Task.CompletedTask;
    }
}
