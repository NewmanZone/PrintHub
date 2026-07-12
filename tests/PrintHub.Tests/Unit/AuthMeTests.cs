using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using PrintHub.Core.Entities;
using PrintHub.Core.Enums;
using PrintHub.Core.Interfaces.Repositories;
using PrintHub.Infrastructure.Repositories;
using Xunit;

namespace PrintHub.Tests.Unit;

public class AuthMeTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    public AuthMeTests(WebApplicationFactory<Program> factory) => _factory = factory.WithWebHostBuilder(_ => { });

    [Fact]
    public async Task AnonymousRequest_IsUnauthorized()
    {
        (await _factory.CreateClient().GetAsync("/auth/me")).StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task FirstAndReturningSignIn_UpsertOneProfile()
    {
        var subject = Guid.NewGuid();
        var client = AuthenticatedClient(subject, "first@example.com", "First Name");
        var first = await client.GetFromJsonAsync<AuthMeResponse>("/auth/me");

        client.DefaultRequestHeaders.Remove("X-User-Email");
        client.DefaultRequestHeaders.Remove("X-User-Name");
        client.DefaultRequestHeaders.Add("X-User-Email", "updated@example.com");
        client.DefaultRequestHeaders.Add("X-User-Name", "Updated Name");
        var returning = await client.GetFromJsonAsync<AuthMeResponse>("/auth/me");

        returning!.User.Id.Should().Be(first!.User.Id);
        returning.User.Email.Should().Be("updated@example.com");
    }

    [Fact]
    public async Task ReturnsOnlyOwnedAndAcceptedActiveMemberships()
    {
        var subject = Guid.NewGuid();
        var client = AuthenticatedClient(subject, "member@example.com", "Member");
        var initial = await client.GetFromJsonAsync<AuthMeResponse>("/auth/me");
        var userId = initial!.User.Id;
        using var scope = _factory.Services.CreateScope();
        var repository = (InMemoryWorkspaceRepository)scope.ServiceProvider.GetRequiredService<IWorkspaceRepository>();
        var owned = new Workspace { Id = Guid.NewGuid(), Name = "Owned", OwnerUserId = userId };
        var accepted = new Workspace { Id = Guid.NewGuid(), Name = "Accepted", OwnerUserId = Guid.NewGuid() };
        var pending = new Workspace { Id = Guid.NewGuid(), Name = "Pending", OwnerUserId = Guid.NewGuid() };
        var removed = new Workspace { Id = Guid.NewGuid(), Name = "Removed", OwnerUserId = Guid.NewGuid() };
        foreach (var workspace in new[] { owned, accepted, pending, removed }) repository.Add(workspace);
        repository.Add(new WorkspaceMember { Id = Guid.NewGuid(), WorkspaceId = accepted.Id, UserId = userId, Role = WorkspaceRole.Viewer, AcceptedAt = DateTime.UtcNow });
        repository.Add(new WorkspaceMember { Id = Guid.NewGuid(), WorkspaceId = pending.Id, UserId = userId });
        repository.Add(new WorkspaceMember { Id = Guid.NewGuid(), WorkspaceId = removed.Id, UserId = userId, AcceptedAt = DateTime.UtcNow, RemovedAt = DateTime.UtcNow });

        var result = await client.GetFromJsonAsync<AuthMeResponse>("/auth/me");

        result!.Workspaces.Select(x => x.Name).Should().BeEquivalentTo("Owned", "Accepted");
        result.Workspaces.Single(x => x.Name == "Accepted").Role.Should().Be("Viewer");
    }

    private HttpClient AuthenticatedClient(Guid subject, string email, string name)
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-User-Id", subject.ToString());
        client.DefaultRequestHeaders.Add("X-User-Email", email);
        client.DefaultRequestHeaders.Add("X-User-Name", name);
        return client;
    }

    public sealed record AuthMeResponse(AuthUser User, List<AuthWorkspace> Workspaces);
    public sealed record AuthUser(Guid Id, string Email, string DisplayName);
    public sealed record AuthWorkspace(Guid Id, string Name, string Role);
}
