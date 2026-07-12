using PrintHub.Core.Entities;
using PrintHub.Core.Enums;

namespace PrintHub.Core.Interfaces.Auth;

public interface ICurrentUserService
{
    Task<CurrentUser?> GetAsync(CancellationToken cancellationToken = default);
}

public sealed record CurrentUser(User User, IReadOnlyList<CurrentUserWorkspace> Workspaces);
public sealed record CurrentUserWorkspace(Guid Id, string Name, WorkspaceRole Role);
