using PrintHub.Core.Entities;

namespace PrintHub.Core.Interfaces.Repositories;

public interface IProductPartRepository
{
    Task<IEnumerable<ProductPart>> GetByProductIdAsync(Guid productId, CancellationToken ct = default);
    Task<ProductPart?> GetByProductAndPartAsync(Guid productId, Guid partId, CancellationToken ct = default);
    Task AddAsync(ProductPart productPart, CancellationToken ct = default);
    Task UpdateAsync(ProductPart productPart, CancellationToken ct = default);
    Task DeleteAsync(Guid productId, Guid partId, CancellationToken ct = default);
    Task DeleteByProductIdAsync(Guid productId, CancellationToken ct = default);
}