using PrintHub.Core.Entities;

namespace PrintHub.Core.Interfaces.Repositories;

public interface IWorkspaceRepository
{
    Task CreateAsync(Workspace workspace, WorkspaceMember ownerMembership, CancellationToken cancellationToken = default);
    Task UpdateAsync(Workspace workspace, CancellationToken cancellationToken = default);
    Task<Workspace?> GetByIdAsync(Guid workspaceId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<WorkspaceMember>> GetMembersAsync(Guid workspaceId, CancellationToken cancellationToken = default);
    Task<WorkspaceMember?> GetMemberAsync(Guid workspaceId, Guid userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<WorkspaceMember>> GetMembershipsForUserAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Workspace>> GetOwnedByUserAsync(Guid userId, CancellationToken cancellationToken = default);
}
