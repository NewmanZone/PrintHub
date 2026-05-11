using Microsoft.EntityFrameworkCore;
using PrintHub.Core.Entities;
using PrintHub.Core.Interfaces.Repositories;
using PrintHub.Infrastructure.Data;

namespace PrintHub.Infrastructure.Repositories;

public class PartRepository : IPartRepository
{
    private readonly PrintHubDbContext _context;

    public PartRepository(PrintHubDbContext context)
    {
        _context = context;
    }

    public async Task<Part?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Parts.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Part>> GetByShopIdAsync(Guid shopId, CancellationToken cancellationToken = default)
    {
        var parts = await _context.Parts.Where(p => p.ShopId == shopId).ToListAsync(cancellationToken);
        return parts.AsReadOnly();
    }

    public async Task<Part> CreateAsync(Part part, CancellationToken cancellationToken = default)
    {
        _context.Parts.Add(part);
        await _context.SaveChangesAsync(cancellationToken);
        return part;
    }
}
