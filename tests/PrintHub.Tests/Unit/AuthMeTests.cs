using System.Net;
using System.Net.Http.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using FluentAssertions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using PrintHub.Core.Entities;
using PrintHub.Core.Enums;
using PrintHub.Core.Interfaces.Repositories;
using PrintHub.Infrastructure.Repositories;
using Xunit;

namespace PrintHub.Tests.Unit;

public class AuthMeTests : IClassFixture<WebApplicationFactory<Program>>
{
    private const string Issuer = "https://issuer.printhub.test";
    private const string Audience = "printhub-api";
    private const string SigningKey = "test-signing-key-that-is-at-least-32-bytes-long";
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
    public async Task ConcurrentFirstSignIns_UpsertOneProfile()
    {
        var subject = Guid.NewGuid();
        var requests = Enumerable.Range(0, 20).Select(async _ =>
        {
            using var client = AuthenticatedClient(subject, "parallel@example.com", "Parallel User");
            return await client.GetFromJsonAsync<AuthMeResponse>("/auth/me");
        });

        var results = await Task.WhenAll(requests);

        results.Select(x => x!.User.Id).Distinct().Should().ContainSingle();
        using var scope = _factory.Services.CreateScope();
        var users = scope.ServiceProvider.GetRequiredService<IUserRepository>();
        (await users.GetByExternalAuthSubjectAsync(subject.ToString()))!.Id.Should().Be(results[0]!.User.Id);
    }

    [Fact]
    public async Task ValidJwt_MapsSubjectEmailAndName()
    {
        using var factory = JwtFactory();
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new("Bearer", CreateToken(Guid.NewGuid().ToString(), "jwt@example.com", "JWT User"));

        var result = await client.GetFromJsonAsync<AuthMeResponse>("/auth/me");

        result!.User.Email.Should().Be("jwt@example.com");
        result.User.DisplayName.Should().Be("JWT User");
    }

    [Fact]
    public async Task InvalidJwt_IsUnauthorized()
    {
        using var factory = JwtFactory();
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new("Bearer", CreateToken(Guid.NewGuid().ToString(), "jwt@example.com", "JWT User", "different-signing-key-that-is-also-long-enough"));

        (await client.GetAsync("/auth/me")).StatusCode.Should().Be(HttpStatusCode.Unauthorized);
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

    private WebApplicationFactory<Program> JwtFactory() => _factory.WithWebHostBuilder(builder =>
    {
        builder.UseSetting("Authentication:Authority", Issuer);
        builder.UseSetting("Authentication:Audience", Audience);
        builder.ConfigureServices(services => services.PostConfigure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, options =>
        {
            options.Configuration = new OpenIdConnectConfiguration { Issuer = Issuer };
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = Issuer,
                ValidateAudience = true,
                ValidAudience = Audience,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SigningKey)),
                ValidateLifetime = true
            };
        }));
    });

    private static string CreateToken(string subject, string email, string name, string signingKey = SigningKey)
    {
        var credentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey)), SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(Issuer, Audience,
            [new Claim(JwtRegisteredClaimNames.Sub, subject), new Claim(JwtRegisteredClaimNames.Email, email), new Claim("name", name)],
            expires: DateTime.UtcNow.AddMinutes(5), signingCredentials: credentials);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public sealed record AuthMeResponse(AuthUser User, List<AuthWorkspace> Workspaces);
    public sealed record AuthUser(Guid Id, string Email, string DisplayName);
    public sealed record AuthWorkspace(Guid Id, string Name, string Role);
}
