using PrintHub.Core.Entities;
using PrintHub.Core.Interfaces.Repositories;

namespace PrintHub.Infrastructure.Repositories;

public class InMemoryShopRepository : IShopRepository
{
    private readonly Dictionary<Guid, Shop> _shops = new();

    public Task<Shop?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        _shops.TryGetValue(id, out var shop);
        return Task.FromResult(shop);
    }

    public Task<Shop?> GetByIdWithProductsAndPartsAsync(Guid id, CancellationToken ct = default)
    {
        _shops.TryGetValue(id, out var shop);
        return Task.FromResult(shop);
    }

    public Task<IEnumerable<Shop>> GetByUserIdAsync(Guid userId, CancellationToken ct = default)
    {
        var shops = _shops.Values.Where(s => s.UserId == userId).ToList();
        return Task.FromResult<IEnumerable<Shop>>(shops);
    }

    public Task<Shop> AddAsync(Shop shop, CancellationToken ct = default)
    {
        _shops[shop.Id] = shop;
        return Task.FromResult(shop);
    }

    public Task<Shop> UpdateAsync(Shop shop, CancellationToken ct = default)
    {
        shop.UpdatedAt = DateTime.UtcNow;
        _shops[shop.Id] = shop;
        return Task.FromResult(shop);
    }

    public Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        _shops.Remove(id);
        return Task.CompletedTask;
    }
}