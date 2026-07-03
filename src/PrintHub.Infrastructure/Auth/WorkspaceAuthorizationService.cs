using PrintHub.Core.Enums;
using PrintHub.Core.Interfaces.Auth;
using PrintHub.Core.Interfaces.Repositories;

namespace PrintHub.Infrastructure.Auth;

public class WorkspaceAuthorizationService : IWorkspaceAuthorizationService
{
    private readonly ICurrentUserContext _currentUser;
    private readonly IWorkspaceRepository _workspaceRepository;

    public WorkspaceAuthorizationService(ICurrentUserContext currentUser, IWorkspaceRepository workspaceRepository)
    {
        _currentUser = currentUser;
        _workspaceRepository = workspaceRepository;
    }

    public async Task<bool> IsMemberAsync(Guid workspaceId, CancellationToken cancellationToken = default)
    {
        if (!_currentUser.IsAuthenticated)
            return false;

        var workspace = await _workspaceRepository.GetByIdAsync(workspaceId, cancellationToken);
        if (workspace is null)
            return false;

        if (workspace.OwnerUserId == _currentUser.UserId!.Value)
            return true;

        var member = await _workspaceRepository.GetMemberAsync(workspaceId, _currentUser.UserId!.Value, cancellationToken);
        return member is { RemovedAt: null };
    }

    public async Task<bool> IsInRoleAsync(Guid workspaceId, WorkspaceRole requiredRole, CancellationToken cancellationToken = default)
    {
        if (!_currentUser.IsAuthenticated)
            return false;

        var workspace = await _workspaceRepository.GetByIdAsync(workspaceId, cancellationToken);
        if (workspace is null)
            return false;

        var userId = _currentUser.UserId!.Value;
        if (workspace.OwnerUserId == userId)
            return true; // Owner implicitly has all roles.

        var member = await _workspaceRepository.GetMemberAsync(workspaceId, userId, cancellationToken);
        if (member is null || member.RemovedAt.HasValue)
            return false;

        return member.Role <= requiredRole;
    }

    public Task<bool> IsOwnerAsync(Guid workspaceId, CancellationToken cancellationToken = default)
        => IsInRoleAsync(workspaceId, WorkspaceRole.Owner, cancellationToken);

    public async Task<bool> WorkspaceExistsAsync(Guid workspaceId, CancellationToken cancellationToken = default)
    {
        var workspace = await _workspaceRepository.GetByIdAsync(workspaceId, cancellationToken);
        return workspace is not null;
    }
}
