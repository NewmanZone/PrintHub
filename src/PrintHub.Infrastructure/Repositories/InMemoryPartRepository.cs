using PrintHub.Core.Entities;
using PrintHub.Core.Interfaces.Repositories;

namespace PrintHub.Infrastructure.Repositories;

/// <summary>
/// In-memory implementation of part repository for development and testing.
/// </summary>
public class InMemoryPartRepository : IPartRepository
{
    private readonly Dictionary<Guid, Part> _parts = new();
    private readonly object _lock = new();

    public Task<Part?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        lock (_lock)
        {
            return Task.FromResult(_parts.GetValueOrDefault(id));
        }
    }

    public Task<Part?> GetByIdWithVersionsAsync(Guid id, CancellationToken ct = default)
    {
        return GetByIdAsync(id, ct);
    }

    public Task<IEnumerable<Part>> GetByShopIdAsync(Guid shopId, CancellationToken ct = default)
    {
        lock (_lock)
        {
            var parts = _parts.Values.Where(p => p.ShopId == shopId).ToList();
            return Task.FromResult<IEnumerable<Part>>(parts);
        }
    }

    public Task<IEnumerable<Part>> GetGenericByShopIdAsync(Guid shopId, CancellationToken ct = default)
    {
        lock (_lock)
        {
            var parts = _parts.Values.Where(p => p.ShopId == shopId && p.IsGeneric).ToList();
            return Task.FromResult<IEnumerable<Part>>(parts);
        }
    }

    public Task<IEnumerable<Part>> GetWithLowStockAsync(Guid shopId, CancellationToken ct = default)
    {
        lock (_lock)
        {
            var parts = _parts.Values.Where(p => p.ShopId == shopId && p.InventoryOnHand <= 0).ToList();
            return Task.FromResult<IEnumerable<Part>>(parts);
        }
    }

    public Task<Part> AddAsync(Part part, CancellationToken ct = default)
    {
        lock (_lock)
        {
            _parts[part.Id] = part;
        }
        return Task.FromResult(part);
    }

    public Task<Part> UpdateAsync(Part part, CancellationToken ct = default)
    {
        lock (_lock)
        {
            part.UpdatedAt = DateTime.UtcNow;
            _parts[part.Id] = part;
        }
        return Task.FromResult(part);
    }

    public Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        lock (_lock)
        {
            _parts.Remove(id);
        }
        return Task.CompletedTask;
    }
}