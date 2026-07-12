using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using PrintHub.Core.Entities;
using PrintHub.Core.Enums;
using PrintHub.Core.Interfaces.Auth;
using PrintHub.Core.Interfaces.Repositories;
using PrintHub.Infrastructure.Repositories;
using Xunit;

namespace PrintHub.Tests.Unit;

public class WorkspaceApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    public WorkspaceApiTests(WebApplicationFactory<Program> factory) => _factory = factory.WithWebHostBuilder(_ => { });

    [Fact]
    public async Task CreateWorkspace_CreatesAcceptedOwnerMembership_AndAppearsInCurrentUser()
    {
        using var client = AuthenticatedClient(Guid.NewGuid());

        var response = await client.PostAsJsonAsync("/workspaces", new { name = "  Production Team  " });
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await response.Content.ReadFromJsonAsync<WorkspaceResponse>();
        created.Should().BeEquivalentTo(new { Name = "Production Team", Role = "Owner" });

        using var scope = _factory.Services.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IWorkspaceRepository>();
        var membership = await repository.GetMemberAsync(created!.Id, (await client.GetFromJsonAsync<AuthMeResponse>("/auth/me"))!.User.Id);
        membership.Should().Match<WorkspaceMember>(x => x.Role == WorkspaceRole.Owner && x.AcceptedAt.HasValue && !x.RemovedAt.HasValue);

        var current = await client.GetFromJsonAsync<AuthMeResponse>("/auth/me");
        current!.Workspaces.Should().ContainSingle(x => x.Id == created.Id && x.Role == "Owner");
    }

    [Fact]
    public async Task WorkspaceRoute_EnforcesAcceptedActiveMembershipThroughRequestPipeline()
    {
        var ownerSubject = Guid.NewGuid();
        using var owner = AuthenticatedClient(ownerSubject);
        var created = await (await owner.PostAsJsonAsync("/workspaces", new { name = "Secured" })).Content.ReadFromJsonAsync<WorkspaceResponse>();
        var contributorSubject = Guid.NewGuid();
        var pendingSubject = Guid.NewGuid();
        var removedSubject = Guid.NewGuid();
        var viewerSubject = Guid.NewGuid();
        using var contributor = AuthenticatedClient(contributorSubject);
        using var pending = AuthenticatedClient(pendingSubject);
        using var removed = AuthenticatedClient(removedSubject);
        using var viewer = AuthenticatedClient(viewerSubject);
        var contributorUser = (await contributor.GetFromJsonAsync<AuthMeResponse>("/auth/me"))!.User.Id;
        var pendingUser = (await pending.GetFromJsonAsync<AuthMeResponse>("/auth/me"))!.User.Id;
        var removedUser = (await removed.GetFromJsonAsync<AuthMeResponse>("/auth/me"))!.User.Id;
        var viewerUser = (await viewer.GetFromJsonAsync<AuthMeResponse>("/auth/me"))!.User.Id;
        using (var scope = _factory.Services.CreateScope())
        {
            var repository = (InMemoryWorkspaceRepository)scope.ServiceProvider.GetRequiredService<IWorkspaceRepository>();
            repository.Add(Member(created!.Id, contributorUser, accepted: true));
            repository.Add(Member(created.Id, pendingUser, accepted: false));
            repository.Add(Member(created.Id, removedUser, accepted: true, removed: true));
            var viewerMembership = Member(created.Id, viewerUser, accepted: true);
            viewerMembership.Role = WorkspaceRole.Viewer;
            repository.Add(viewerMembership);
        }

        (await contributor.GetAsync($"/workspaces/{created!.Id}")).StatusCode.Should().Be(HttpStatusCode.OK);
        (await contributor.PutAsJsonAsync($"/workspaces/{created.Id}", new { name = "Denied" })).StatusCode.Should().Be(HttpStatusCode.Forbidden);
        (await pending.GetAsync($"/workspaces/{created.Id}")).StatusCode.Should().Be(HttpStatusCode.Forbidden);
        (await removed.GetAsync($"/workspaces/{created.Id}")).StatusCode.Should().Be(HttpStatusCode.Forbidden);
        (await viewer.GetAsync($"/workspaces/{created.Id}")).StatusCode.Should().Be(HttpStatusCode.Forbidden);
        using var stranger = AuthenticatedClient(Guid.NewGuid());
        (await stranger.GetAsync($"/workspaces/{created.Id}")).StatusCode.Should().Be(HttpStatusCode.Forbidden);
        var ownerUpdate = await owner.PutAsJsonAsync($"/workspaces/{created.Id}", new { name = "Renamed" });
        ownerUpdate.StatusCode.Should().Be(HttpStatusCode.OK);
        (await ownerUpdate.Content.ReadFromJsonAsync<WorkspaceResponse>())!.Name.Should().Be("Renamed");
    }

    [Fact]
    public void OwnerAndContributorAuthorizationChecks_AreRegisteredThroughDependencyInjection()
    {
        using var scope = _factory.Services.CreateScope();
        scope.ServiceProvider.GetRequiredService<IWorkspaceAuthorizationService>().Should().NotBeNull();
    }

    private HttpClient AuthenticatedClient(Guid subject)
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-User-Id", subject.ToString());
        client.DefaultRequestHeaders.Add("X-User-Email", $"{subject}@example.com");
        return client;
    }

    private static WorkspaceMember Member(Guid workspaceId, Guid userId, bool accepted, bool removed = false) => new()
    {
        Id = Guid.NewGuid(), WorkspaceId = workspaceId, UserId = userId, Role = WorkspaceRole.Contributor,
        AcceptedAt = accepted ? DateTime.UtcNow : null, RemovedAt = removed ? DateTime.UtcNow : null
    };

    private sealed record WorkspaceResponse(Guid Id, string Name, string Role);
    private sealed record AuthMeResponse(AuthUser User, List<WorkspaceResponse> Workspaces);
    private sealed record AuthUser(Guid Id);
}
