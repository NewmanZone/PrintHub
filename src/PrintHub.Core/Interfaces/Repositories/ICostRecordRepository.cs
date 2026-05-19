using PrintHub.Core.Entities;

namespace PrintHub.Core.Interfaces.Repositories;

public interface ICostRecordRepository
{
    Task<IEnumerable<CostRecord>> GetByShopIdAsync(Guid shopId, int limit = 100, CancellationToken ct = default);
    Task<IEnumerable<CostRecord>> GetByProductIdAsync(Guid productId, CancellationToken ct = default);
    Task<IEnumerable<CostRecord>> GetByPartIdAsync(Guid partId, CancellationToken ct = default);
    Task<CostRecord> AddAsync(CostRecord record, CancellationToken ct = default);
}