using PrintHub.Core.Entities;

namespace PrintHub.Core.Interfaces.Repositories;

public interface IPartRepository
{
    Task<Part?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Part>> GetByShopIdAsync(Guid shopId, CancellationToken cancellationToken = default);
    Task<Part> CreateAsync(Part part, CancellationToken cancellationToken = default);
}
