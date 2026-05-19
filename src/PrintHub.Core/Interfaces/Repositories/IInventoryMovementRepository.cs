using PrintHub.Core.Entities;

namespace PrintHub.Core.Interfaces.Repositories;

public interface IInventoryMovementRepository
{
    Task<IEnumerable<InventoryMovement>> GetByShopIdAsync(Guid shopId, int limit = 100, CancellationToken ct = default);
    Task<IEnumerable<InventoryMovement>> GetByPartIdAsync(Guid partId, int limit = 100, CancellationToken ct = default);
    Task<InventoryMovement> AddAsync(InventoryMovement movement, CancellationToken ct = default);
}