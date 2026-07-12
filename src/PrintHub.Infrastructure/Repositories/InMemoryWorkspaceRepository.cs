using System.Collections.Concurrent;
using PrintHub.Core.Entities;
using PrintHub.Core.Interfaces.Repositories;

namespace PrintHub.Infrastructure.Repositories;

public sealed class InMemoryWorkspaceRepository : IWorkspaceRepository
{
    private readonly object _createLock = new();
    private readonly ConcurrentDictionary<Guid, Workspace> _workspaces = new();
    private readonly ConcurrentDictionary<Guid, WorkspaceMember> _members = new();

    public Task CreateAsync(Workspace workspace, WorkspaceMember ownerMembership, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(workspace);
        ArgumentNullException.ThrowIfNull(ownerMembership);
        if (ownerMembership.WorkspaceId != workspace.Id || ownerMembership.UserId != workspace.OwnerUserId)
            throw new ArgumentException("The owner membership must belong to the workspace owner.", nameof(ownerMembership));

        lock (_createLock)
        {
            if (!_workspaces.TryAdd(workspace.Id, workspace))
                throw new InvalidOperationException("A workspace with this ID already exists.");
            if (!_members.TryAdd(ownerMembership.Id, ownerMembership))
            {
                _workspaces.TryRemove(workspace.Id, out _);
                throw new InvalidOperationException("A membership with this ID already exists.");
            }
        }

        return Task.CompletedTask;
    }

    public Task<Workspace?> GetByIdAsync(Guid workspaceId, CancellationToken cancellationToken = default) =>
        Task.FromResult(_workspaces.GetValueOrDefault(workspaceId));

    public Task UpdateAsync(Workspace workspace, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(workspace);
        if (!_workspaces.ContainsKey(workspace.Id))
            throw new InvalidOperationException("The workspace does not exist.");
        _workspaces[workspace.Id] = workspace;
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<WorkspaceMember>> GetMembersAsync(Guid workspaceId, CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<WorkspaceMember>>(_members.Values.Where(x => x.WorkspaceId == workspaceId).ToList());

    public Task<WorkspaceMember?> GetMemberAsync(Guid workspaceId, Guid userId, CancellationToken cancellationToken = default) =>
        Task.FromResult(_members.Values.FirstOrDefault(x => x.WorkspaceId == workspaceId && x.UserId == userId));

    public Task<IReadOnlyList<WorkspaceMember>> GetMembershipsForUserAsync(Guid userId, CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<WorkspaceMember>>(_members.Values.Where(x => x.UserId == userId).ToList());

    public Task<IReadOnlyList<Workspace>> GetOwnedByUserAsync(Guid userId, CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<Workspace>>(_workspaces.Values.Where(x => x.OwnerUserId == userId).ToList());

    public void Add(Workspace workspace) => _workspaces[workspace.Id] = workspace;
    public void Add(WorkspaceMember member) => _members[member.Id] = member;
}
