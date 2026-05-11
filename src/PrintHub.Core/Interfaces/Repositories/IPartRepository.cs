using PrintHub.Core.Entities;

namespace PrintHub.Core.Interfaces.Repositories;

public interface IPartRepository
{
    Task<Part?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Part?> GetByIdWithVersionsAsync(Guid id, CancellationToken ct = default);
    Task<IEnumerable<Part>> GetByShopIdAsync(Guid shopId, CancellationToken ct = default);
    Task<IEnumerable<Part>> GetGenericByShopIdAsync(Guid shopId, CancellationToken ct = default);
    Task<IEnumerable<Part>> GetWithLowStockAsync(Guid shopId, CancellationToken ct = default);
    Task<Part> AddAsync(Part part, CancellationToken ct = default);
    Task<Part> UpdateAsync(Part part, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}