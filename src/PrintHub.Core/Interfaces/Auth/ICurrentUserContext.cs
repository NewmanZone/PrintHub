namespace PrintHub.Core.Interfaces.Auth;

public interface ICurrentUserContext
{
    Guid? UserId { get; }
    string Email { get; }
    string AuthSubject { get; }
    bool IsAuthenticated { get; }
}
