using System.Collections.Concurrent;
using PrintHub.Core.Entities;
using PrintHub.Core.Interfaces.Repositories;

namespace PrintHub.Infrastructure.Repositories;

public sealed class InMemoryUserRepository : IUserRepository
{
    private readonly ConcurrentDictionary<Guid, User> _users = new();

    public Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        Task.FromResult(_users.GetValueOrDefault(id));

    public Task<User?> GetByEmailAsync(string email, CancellationToken ct = default) =>
        Task.FromResult(_users.Values.FirstOrDefault(x => string.Equals(x.Email, email, StringComparison.OrdinalIgnoreCase)));

    public Task<User?> GetByExternalAuthSubjectAsync(string subject, CancellationToken ct = default) =>
        Task.FromResult(_users.Values.FirstOrDefault(x => string.Equals(x.ExternalAuthSubject, subject, StringComparison.Ordinal)));

    public Task<User> AddAsync(User user, CancellationToken ct = default)
    {
        if (!_users.TryAdd(user.Id, user)) throw new InvalidOperationException("A user with that id already exists.");
        return Task.FromResult(user);
    }

    public Task<User> UpdateAsync(User user, CancellationToken ct = default)
    {
        _users[user.Id] = user;
        return Task.FromResult(user);
    }
}
