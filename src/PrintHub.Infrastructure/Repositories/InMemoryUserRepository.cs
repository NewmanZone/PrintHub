using System.Collections.Concurrent;
using PrintHub.Core.Entities;
using PrintHub.Core.Interfaces.Repositories;

namespace PrintHub.Infrastructure.Repositories;

public sealed class InMemoryUserRepository : IUserRepository
{
    private readonly ConcurrentDictionary<Guid, User> _users = new();
    private readonly ConcurrentDictionary<string, User> _usersBySubject = new(StringComparer.Ordinal);

    public Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        Task.FromResult(_users.GetValueOrDefault(id));

    public Task<User?> GetByEmailAsync(string email, CancellationToken ct = default) =>
        Task.FromResult(_users.Values.FirstOrDefault(x => string.Equals(x.Email, email, StringComparison.OrdinalIgnoreCase)));

    public Task<User?> GetByExternalAuthSubjectAsync(string subject, CancellationToken ct = default) =>
        Task.FromResult(_usersBySubject.GetValueOrDefault(subject));

    public Task<User> UpsertByExternalAuthSubjectAsync(User user, CancellationToken ct = default)
    {
        var stored = _usersBySubject.GetOrAdd(user.ExternalAuthSubject, user);
        if (ReferenceEquals(stored, user))
        {
            _users[user.Id] = user;
        }
        else
        {
            lock (stored)
            {
                stored.Email = user.Email;
                stored.DisplayName = user.DisplayName;
                stored.UpdatedAt = user.UpdatedAt;
            }
        }

        return Task.FromResult(stored);
    }

    public Task<User> AddAsync(User user, CancellationToken ct = default)
    {
        if (!_usersBySubject.TryAdd(user.ExternalAuthSubject, user)) throw new InvalidOperationException("A user with that external auth subject already exists.");
        if (!_users.TryAdd(user.Id, user))
        {
            _usersBySubject.TryRemove(new KeyValuePair<string, User>(user.ExternalAuthSubject, user));
            throw new InvalidOperationException("A user with that id already exists.");
        }
        return Task.FromResult(user);
    }

    public Task<User> UpdateAsync(User user, CancellationToken ct = default)
    {
        _users[user.Id] = user;
        return Task.FromResult(user);
    }
}
