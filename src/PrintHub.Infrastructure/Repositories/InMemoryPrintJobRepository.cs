using PrintHub.Core.Entities;
using PrintHub.Core.Enums;
using PrintHub.Core.Interfaces.Repositories;

namespace PrintHub.Infrastructure.Repositories;

/// <summary>
/// In-memory implementation of print job repository for development and testing.
/// </summary>
public class InMemoryPrintJobRepository : IPrintJobRepository
{
    private readonly Dictionary<Guid, PrintJob> _jobs = new();
    private readonly object _lock = new();

    public Task<PrintJob?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        lock (_lock)
        {
            return Task.FromResult(_jobs.GetValueOrDefault(id));
        }
    }

    public Task<PrintJob?> GetByIdWithItemsAsync(Guid id, CancellationToken ct = default)
    {
        return GetByIdAsync(id, ct);
    }

    public Task<IEnumerable<PrintJob>> GetByShopIdAsync(Guid shopId, PrintJobStatus? status = null, DateTime? from = null, DateTime? to = null, CancellationToken ct = default)
    {
        lock (_lock)
        {
            var query = _jobs.Values.Where(j => j.ShopId == shopId);
            
            if (status.HasValue)
            {
                var statusStr = status.Value.ToString().ToLower();
                query = query.Where(j => j.Status.ToLower() == statusStr);
            }
            
            if (from.HasValue)
            {
                query = query.Where(j => j.CreatedAt >= from.Value);
            }
            
            if (to.HasValue)
            {
                query = query.Where(j => j.CreatedAt <= to.Value);
            }
            
            return Task.FromResult<IEnumerable<PrintJob>>(query.ToList());
        }
    }

    public Task<PrintJob> AddAsync(PrintJob printJob, CancellationToken ct = default)
    {
        lock (_lock)
        {
            _jobs[printJob.Id] = printJob;
        }
        return Task.FromResult(printJob);
    }

    public Task<PrintJob> UpdateAsync(PrintJob printJob, CancellationToken ct = default)
    {
        lock (_lock)
        {
            _jobs[printJob.Id] = printJob;
        }
        return Task.FromResult(printJob);
    }
}