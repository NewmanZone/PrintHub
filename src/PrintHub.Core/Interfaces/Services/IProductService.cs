using PrintHub.Core.Entities;

namespace PrintHub.Core.Interfaces.Services;

public interface IProductService
{
    Task<Product?> GetByIdAsync(Guid productId, CancellationToken ct = default);
    Task<Product?> GetByIdWithPartsAsync(Guid productId, CancellationToken ct = default);
    Task<IEnumerable<Product>> GetByShopIdAsync(Guid shopId, int page = 1, int pageSize = 20, CancellationToken ct = default);
    Task<IEnumerable<Product>> SearchByNameAsync(Guid shopId, string searchTerm, CancellationToken ct = default);
    Task<IEnumerable<Product>> GetBelowReorderPointAsync(Guid shopId, CancellationToken ct = default);
    Task<Product> CreateAsync(Guid shopId, string name, string? description, decimal? etsyPrice, int? reorderPoint, int? reorderQuantity, IEnumerable<Guid>? partIds = null, CancellationToken ct = default);
    Task<Product> UpdateAsync(Guid productId, string? name = null, string? description = null, decimal? etsyPrice = null, int? reorderPoint = null, int? reorderQuantity = null, bool? isActive = null, CancellationToken ct = default);
    Task DeleteAsync(Guid productId, CancellationToken ct = default);
    Task<Product> SetPartsAsync(Guid productId, IEnumerable<(Guid partId, int quantity)> parts, CancellationToken ct = default);
}