using System.Collections.Concurrent;
using PrintHub.Core.Entities;
using PrintHub.Core.Interfaces.Repositories;

namespace PrintHub.Infrastructure.Repositories;

public class InMemoryShopRepository : IShopRepository
{
    private readonly ConcurrentDictionary<Guid, Shop> _shops = new();

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
        // Snapshot to avoid concurrent enumeration issues
        var shops = _shops.Values.ToList().Where(s => s.UserId == userId).ToList();
        return Task.FromResult<IEnumerable<Shop>>(shops);
    }

    public Task<IEnumerable<Shop>> GetByWorkspaceIdAsync(Guid workspaceId, CancellationToken ct = default) =>
        Task.FromResult<IEnumerable<Shop>>(_shops.Values.Where(s => s.WorkspaceId == workspaceId).ToList());

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
        _shops.TryRemove(id, out _);
        return Task.CompletedTask;
    }
}
