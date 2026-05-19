using PrintHub.Core.Entities;

namespace PrintHub.Core.Interfaces.Repositories;

public interface IPersonalizedOrderRepository
{
    Task<PersonalizedOrder?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IEnumerable<PersonalizedOrder>> GetByShopIdAsync(Guid shopId, CancellationToken ct = default);
    Task<PersonalizedOrder> AddAsync(PersonalizedOrder order, CancellationToken ct = default);
    Task<PersonalizedOrder> UpdateAsync(PersonalizedOrder order, CancellationToken ct = default);
}