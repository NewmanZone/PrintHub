using Microsoft.EntityFrameworkCore;
using PrintHub.Core.Entities;
using PrintHub.Core.Interfaces.Repositories;
using PrintHub.Infrastructure.Data;

namespace PrintHub.Infrastructure.Repositories;

public class ShopRepository : IShopRepository
{
    private readonly PrintHubDbContext _context;

    public ShopRepository(PrintHubDbContext context)
    {
        _context = context;
    }

    public async Task<Shop?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Shops.FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Shop>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var shops = await _context.Shops.Where(s => s.UserId == userId).ToListAsync(cancellationToken);
        return shops.AsReadOnly();
    }

    public async Task<Shop> CreateAsync(Shop shop, CancellationToken cancellationToken = default)
    {
        _context.Shops.Add(shop);
        await _context.SaveChangesAsync(cancellationToken);
        return shop;
    }
}
