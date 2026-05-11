using Microsoft.EntityFrameworkCore;
using PrintHub.Core.Entities;
using PrintHub.Core.Interfaces.Repositories;
using PrintHub.Infrastructure.Data;

namespace PrintHub.Infrastructure.Repositories;

public class PrintJobRepository : IPrintJobRepository
{
    private readonly PrintHubDbContext _context;

    public PrintJobRepository(PrintHubDbContext context)
    {
        _context = context;
    }

    public async Task<PrintJob?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.PrintJobs.FirstOrDefaultAsync(j => j.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<PrintJob>> GetByShopIdAsync(Guid shopId, CancellationToken cancellationToken = default)
    {
        var jobs = await _context.PrintJobs.Where(j => j.ShopId == shopId).ToListAsync(cancellationToken);
        return jobs.AsReadOnly();
    }

    public async Task<PrintJob> CreateAsync(PrintJob job, CancellationToken cancellationToken = default)
    {
        _context.PrintJobs.Add(job);
        await _context.SaveChangesAsync(cancellationToken);
        return job;
    }

    public async Task UpdateAsync(PrintJob job, CancellationToken cancellationToken = default)
    {
        _context.PrintJobs.Update(job);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
