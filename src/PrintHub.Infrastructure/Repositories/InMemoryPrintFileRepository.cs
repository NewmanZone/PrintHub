using System.Collections.Concurrent;
using PrintHub.Core.Entities;
using PrintHub.Core.Interfaces.Repositories;

namespace PrintHub.Infrastructure.Repositories;

public class InMemoryPrintFileRepository : IPrintFileRepository
{
    private readonly ConcurrentDictionary<Guid, PrintFile> _files = new();

    public Task<PrintFile?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        _files.TryGetValue(id, out var file);
        return Task.FromResult(file);
    }

    public Task<PrintFile?> GetByIdWithVersionsAsync(Guid id, CancellationToken ct = default)
    {
        _files.TryGetValue(id, out var file);
        return Task.FromResult(file);
    }

    public Task<IEnumerable<PrintFile>> GetByPartIdAsync(Guid partId, CancellationToken ct = default)
    {
        var files = _files.Values.Where(f => f.PartId == partId).ToList();
        return Task.FromResult<IEnumerable<PrintFile>>(files);
    }

    public Task<PrintFile?> GetByPartIdWithVersionsAsync(Guid partId, CancellationToken ct = default)
    {
        _files.TryGetValue(partId, out var file);
        return Task.FromResult(file);
    }

    public Task<PrintFile> AddAsync(PrintFile file, CancellationToken ct = default)
    {
        _files[file.Id] = file;
        return Task.FromResult(file);
    }

    public Task<PrintFile> UpdateAsync(PrintFile file, CancellationToken ct = default)
    {
        _files[file.Id] = file;
        return Task.FromResult(file);
    }

    public Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        _files.TryRemove(id, out _);
        return Task.CompletedTask;
    }
}
