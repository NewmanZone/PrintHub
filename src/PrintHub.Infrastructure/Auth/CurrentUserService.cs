using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using PrintHub.Core.Entities;
using PrintHub.Core.Enums;
using PrintHub.Core.Interfaces.Auth;
using PrintHub.Core.Interfaces.Repositories;

namespace PrintHub.Infrastructure.Auth;

public sealed class CurrentUserService(
    IHttpContextAccessor httpContextAccessor,
    IUserRepository users,
    IWorkspaceRepository workspaces) : ICurrentUserService
{
    private CurrentUser? _current;

    public async Task<CurrentUser?> GetAsync(CancellationToken cancellationToken = default)
    {
        if (_current is not null) return _current;
        var principal = httpContextAccessor.HttpContext?.User;
        if (principal?.Identity?.IsAuthenticated != true) return null;

        var subject = principal.FindFirstValue("sub") ?? principal.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(subject)) return null;
        var email = principal.FindFirstValue(ClaimTypes.Email) ?? principal.FindFirstValue("email") ?? string.Empty;
        var displayName = principal.FindFirstValue(ClaimTypes.Name) ?? principal.FindFirstValue("name") ?? email;

        var now = DateTime.UtcNow;
        var user = await users.UpsertByExternalAuthSubjectAsync(new User
        {
            Id = Guid.NewGuid(), ExternalAuthSubject = subject, Email = email,
            DisplayName = displayName, CreatedAt = now, UpdatedAt = now
        }, cancellationToken);

        var owned = await workspaces.GetOwnedByUserAsync(user.Id, cancellationToken);
        var memberships = await workspaces.GetMembershipsForUserAsync(user.Id, cancellationToken);
        var result = owned.Select(x => new CurrentUserWorkspace(x.Id, x.Name, WorkspaceRole.Owner)).ToList();
        foreach (var membership in memberships.Where(x => x.AcceptedAt.HasValue && !x.RemovedAt.HasValue && owned.All(w => w.Id != x.WorkspaceId)))
        {
            var workspace = await workspaces.GetByIdAsync(membership.WorkspaceId, cancellationToken);
            if (workspace is not null) result.Add(new CurrentUserWorkspace(workspace.Id, workspace.Name, membership.Role));
        }

        return _current = new CurrentUser(user, result);
    }
}
