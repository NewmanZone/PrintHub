using Microsoft.EntityFrameworkCore;
using PrintHub.Core.Entities;
using PrintHub.Core.Interfaces.Repositories;
using PrintHub.Infrastructure.Data;

namespace PrintHub.Infrastructure.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly PrintHubDbContext _context;

    public ProductRepository(PrintHubDbContext context)
    {
        _context = context;
    }

    public async Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Products.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Product>> GetByShopIdAsync(Guid shopId, CancellationToken cancellationToken = default)
    {
        var products = await _context.Products.Where(p => p.ShopId == shopId).ToListAsync(cancellationToken);
        return products.AsReadOnly();
    }

    public async Task<Product> CreateAsync(Product product, CancellationToken cancellationToken = default)
    {
        _context.Products.Add(product);
        await _context.SaveChangesAsync(cancellationToken);
        return product;
    }

    public async Task UpdateAsync(Product product, CancellationToken cancellationToken = default)
    {
        _context.Products.Update(product);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
