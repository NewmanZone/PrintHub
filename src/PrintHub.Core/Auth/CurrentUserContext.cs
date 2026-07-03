using PrintHub.Core.Interfaces.Auth;

namespace PrintHub.Core.Auth;

public class CurrentUserContext : ICurrentUserContext
{
    public Guid? UserId { get; init; }
    public string Email { get; init; } = string.Empty;
    public string AuthSubject { get; init; } = string.Empty;
    public bool IsAuthenticated => UserId.HasValue;
}
