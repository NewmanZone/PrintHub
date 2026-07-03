using PrintHub.Core.Entities;

namespace PrintHub.Core.Interfaces.Repositories;

public interface IWorkspaceRepository
{
    Task<Workspace?> GetByIdAsync(Guid workspaceId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<WorkspaceMember>> GetMembersAsync(Guid workspaceId, CancellationToken cancellationToken = default);
    Task<WorkspaceMember?> GetMemberAsync(Guid workspaceId, Guid userId, CancellationToken cancellationToken = default);
}
