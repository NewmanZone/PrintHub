using PrintHub.Core.Entities;

namespace PrintHub.Core.Interfaces.Services;

public interface IInventoryService
{
    Task<int> GetInventoryOnHandForPartAsync(Guid partId, CancellationToken ct = default);
    Task<decimal> GetInventoryValueForShopAsync(Guid shopId, CancellationToken ct = default);
    Task RecordMovementAsync(Guid shopId, Guid partId, Guid? productId, int quantityChange, string reason, string? reference = null, CancellationToken ct = default);
    Task<IEnumerable<InventoryMovement>> GetRecentMovementsAsync(Guid shopId, int limit = 50, CancellationToken ct = default);
    Task UpdatePartInventoryAsync(Guid partId, int newInventoryOnHand, CancellationToken ct = default);
}