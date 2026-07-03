using PrintHub.Core.Enums;

namespace PrintHub.Core.Interfaces.Auth;

public interface IWorkspaceAuthorizationService
{
    Task<bool> IsMemberAsync(Guid workspaceId, CancellationToken cancellationToken = default);
    Task<bool> IsInRoleAsync(Guid workspaceId, WorkspaceRole requiredRole, CancellationToken cancellationToken = default);
    Task<bool> IsOwnerAsync(Guid workspaceId, CancellationToken cancellationToken = default);
    Task<bool> WorkspaceExistsAsync(Guid workspaceId, CancellationToken cancellationToken = default);
}
