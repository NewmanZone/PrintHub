using Xunit;
using FluentAssertions;
using Moq;
using PrintHub.Core.Auth;
using PrintHub.Core.Entities;
using PrintHub.Core.Enums;
using PrintHub.Core.Interfaces.Auth;
using PrintHub.Core.Interfaces.Repositories;
using PrintHub.Infrastructure.Auth;

namespace PrintHub.Tests.Unit;

public class WorkspaceAuthorizationTests
{
    private static ICurrentUserContext AuthenticatedUser(Guid userId, string email = "user@example.com")
        => new CurrentUserContext { UserId = userId, Email = email, AuthSubject = $"auth|{userId}" };

    private static ICurrentUserContext AnonymousUser()
        => new CurrentUserContext { Email = string.Empty, AuthSubject = string.Empty };

    private static Mock<IWorkspaceRepository> RepositoryWithWorkspace(Guid workspaceId, Guid ownerId, params (Guid userId, WorkspaceRole role, DateTime? removedAt)[] members)
    {
        var workspace = new Workspace { Id = workspaceId, Name = "Test", OwnerUserId = ownerId };
        var repository = new Mock<IWorkspaceRepository>();

        repository.Setup(r => r.GetByIdAsync(workspaceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(workspace);

        repository.Setup(r => r.GetByIdAsync(It.Is<Guid>(id => id != workspaceId), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Workspace?)null);

        repository.Setup(r => r.GetMemberAsync(workspaceId, It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid _, Guid userId, CancellationToken __) =>
            {
                var match = members.FirstOrDefault(m => m.userId == userId);
                if (match == default)
                    return null;

                return new WorkspaceMember
                {
                    WorkspaceId = workspaceId,
                    UserId = userId,
                    Role = match.role,
                    RemovedAt = match.removedAt
                };
            });

        return repository;
    }

    [Fact]
    public async Task Owner_IsMember_ReturnsTrue()
    {
        var ownerId = Guid.NewGuid();
        var workspaceId = Guid.NewGuid();
        var repo = RepositoryWithWorkspace(workspaceId, ownerId);
        var service = new WorkspaceAuthorizationService(AuthenticatedUser(ownerId), repo.Object);

        var result = await service.IsMemberAsync(workspaceId);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task Contributor_IsMember_ReturnsTrue()
    {
        var ownerId = Guid.NewGuid();
        var contributorId = Guid.NewGuid();
        var workspaceId = Guid.NewGuid();
        var repo = RepositoryWithWorkspace(workspaceId, ownerId, (contributorId, WorkspaceRole.Contributor, null));
        var service = new WorkspaceAuthorizationService(AuthenticatedUser(contributorId), repo.Object);

        var result = await service.IsMemberAsync(workspaceId);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task NonMember_IsMember_ReturnsFalse()
    {
        var ownerId = Guid.NewGuid();
        var strangerId = Guid.NewGuid();
        var workspaceId = Guid.NewGuid();
        var repo = RepositoryWithWorkspace(workspaceId, ownerId);
        var service = new WorkspaceAuthorizationService(AuthenticatedUser(strangerId), repo.Object);

        var result = await service.IsMemberAsync(workspaceId);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task RemovedMember_IsMember_ReturnsFalse()
    {
        var ownerId = Guid.NewGuid();
        var removedId = Guid.NewGuid();
        var workspaceId = Guid.NewGuid();
        var repo = RepositoryWithWorkspace(workspaceId, ownerId, (removedId, WorkspaceRole.Contributor, DateTime.UtcNow));
        var service = new WorkspaceAuthorizationService(AuthenticatedUser(removedId), repo.Object);

        var result = await service.IsMemberAsync(workspaceId);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task AnonymousUser_IsMember_ReturnsFalse()
    {
        var workspaceId = Guid.NewGuid();
        var repo = RepositoryWithWorkspace(workspaceId, Guid.NewGuid());
        var service = new WorkspaceAuthorizationService(AnonymousUser(), repo.Object);

        var result = await service.IsMemberAsync(workspaceId);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task Contributor_CanAccessAsContributor_ReturnsTrue()
    {
        var ownerId = Guid.NewGuid();
        var contributorId = Guid.NewGuid();
        var workspaceId = Guid.NewGuid();
        var repo = RepositoryWithWorkspace(workspaceId, ownerId, (contributorId, WorkspaceRole.Contributor, null));
        var service = new WorkspaceAuthorizationService(AuthenticatedUser(contributorId), repo.Object);

        var result = await service.IsInRoleAsync(workspaceId, WorkspaceRole.Contributor);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task Contributor_CannotAccessAsOwner_ReturnsFalse()
    {
        var ownerId = Guid.NewGuid();
        var contributorId = Guid.NewGuid();
        var workspaceId = Guid.NewGuid();
        var repo = RepositoryWithWorkspace(workspaceId, ownerId, (contributorId, WorkspaceRole.Contributor, null));
        var service = new WorkspaceAuthorizationService(AuthenticatedUser(contributorId), repo.Object);

        var result = await service.IsOwnerAsync(workspaceId);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task Owner_CanAccessAsOwner_ReturnsTrue()
    {
        var ownerId = Guid.NewGuid();
        var workspaceId = Guid.NewGuid();
        var repo = RepositoryWithWorkspace(workspaceId, ownerId);
        var service = new WorkspaceAuthorizationService(AuthenticatedUser(ownerId), repo.Object);

        var result = await service.IsOwnerAsync(workspaceId);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task Owner_CanAccessAsContributor_ReturnsTrue()
    {
        var ownerId = Guid.NewGuid();
        var workspaceId = Guid.NewGuid();
        var repo = RepositoryWithWorkspace(workspaceId, ownerId);
        var service = new WorkspaceAuthorizationService(AuthenticatedUser(ownerId), repo.Object);

        var result = await service.IsInRoleAsync(workspaceId, WorkspaceRole.Contributor);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task MissingWorkspace_AnyCheck_ReturnsFalse()
    {
        var userId = Guid.NewGuid();
        var repo = new Mock<IWorkspaceRepository>();
        repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Workspace?)null);
        var service = new WorkspaceAuthorizationService(AuthenticatedUser(userId), repo.Object);

        var memberResult = await service.IsMemberAsync(Guid.NewGuid());
        var ownerResult = await service.IsOwnerAsync(Guid.NewGuid());
        var existsResult = await service.WorkspaceExistsAsync(Guid.NewGuid());

        memberResult.Should().BeFalse();
        ownerResult.Should().BeFalse();
        existsResult.Should().BeFalse();
    }

    [Fact]
    public async Task ExistingWorkspace_WorkspaceExistsAsync_ReturnsTrue()
    {
        var ownerId = Guid.NewGuid();
        var workspaceId = Guid.NewGuid();
        var repo = RepositoryWithWorkspace(workspaceId, ownerId);
        var service = new WorkspaceAuthorizationService(AuthenticatedUser(ownerId), repo.Object);

        var result = await service.WorkspaceExistsAsync(workspaceId);

        result.Should().BeTrue();
    }
}
