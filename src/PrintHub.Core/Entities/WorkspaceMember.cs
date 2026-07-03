using PrintHub.Core.Enums;

namespace PrintHub.Core.Entities;

public class WorkspaceMember
{
    public Guid Id { get; set; }
    public Guid WorkspaceId { get; set; }
    public Guid UserId { get; set; }
    public WorkspaceRole Role { get; set; } = WorkspaceRole.Contributor;
    public Guid? InvitedByUserId { get; set; }
    public string? InvitedEmail { get; set; }
    public DateTime? AcceptedAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? RemovedAt { get; set; }
}
