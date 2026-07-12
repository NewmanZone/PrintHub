using PrintHub.Core.Interfaces.Auth;

namespace PrintHub.Infrastructure.Auth;

public sealed class HttpCurrentUserContext(ICurrentUserService currentUser) : ICurrentUserContext
{
    private CurrentUser? Value => currentUser.GetAsync().GetAwaiter().GetResult();
    public Guid? UserId => Value?.User.Id;
    public string Email => Value?.User.Email ?? string.Empty;
    public string AuthSubject => Value?.User.ExternalAuthSubject ?? string.Empty;
    public bool IsAuthenticated => Value is not null;
}
