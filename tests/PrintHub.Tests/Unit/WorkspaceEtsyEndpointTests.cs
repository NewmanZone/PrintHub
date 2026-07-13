using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using PrintHub.Core.Entities;
using PrintHub.Core.Enums;
using PrintHub.Core.Interfaces.Repositories;
using PrintHub.Core.Interfaces.Services;
using PrintHub.Infrastructure.Services;
using PrintHub.Infrastructure.Services.Etsy;
using Xunit;

namespace PrintHub.Tests.Unit;

public sealed class WorkspaceEtsyEndpointTests : IClassFixture<WorkspaceEtsyEndpointTests.Factory>
{
    private readonly Factory _factory;
    private readonly Dictionary<string, Guid> _subjects = new();

    public WorkspaceEtsyEndpointTests(Factory factory) => _factory = factory;

    [Fact]
    public async Task Etsy_routes_require_authentication_and_active_membership()
    {
        var workspaceId = Guid.NewGuid();
        var ownerId = await AddUser("owner");
        Seed(workspaceId, ownerId);

        (await _factory.CreateClient().GetAsync($"/workspaces/{workspaceId}/shops"))
            .StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        foreach (var state in new[] { "non-member", "pending", "removed" })
        {
            var id = await AddUser(state);
            if (state != "non-member") SeedMember(workspaceId, id, accepted: state == "removed", removed: state == "removed");
            (await Client(state).GetAsync($"/workspaces/{workspaceId}/shops"))
                .StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }
    }

    [Fact]
    public async Task Contributor_can_list_and_sync_but_only_owner_can_administer_shop()
    {
        var workspaceId = Guid.NewGuid();
        var ownerId = await AddUser("route-owner");
        var contributorId = await AddUser("route-contributor");
        Seed(workspaceId, ownerId);
        SeedMember(workspaceId, contributorId, accepted: true, removed: false);
        var shopId = Guid.NewGuid();

        (await Client("route-contributor").GetAsync($"/workspaces/{workspaceId}/shops")).StatusCode.Should().Be(HttpStatusCode.OK);
        (await Client("route-contributor").PostAsync($"/workspaces/{workspaceId}/shops/{shopId}/sync", null)).StatusCode.Should().Be(HttpStatusCode.OK);
        (await Client("route-contributor").PostAsJsonAsync($"/workspaces/{workspaceId}/shops/connect/etsy", new { })).StatusCode.Should().Be(HttpStatusCode.Forbidden);
        (await Client("route-contributor").DeleteAsync($"/workspaces/{workspaceId}/shops/{shopId}")).StatusCode.Should().Be(HttpStatusCode.Forbidden);
        (await Client("route-owner").DeleteAsync($"/workspaces/{workspaceId}/shops/{shopId}")).StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    private HttpClient Client(string subject)
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-User-Id", GetSubjectId(subject).ToString());
        client.DefaultRequestHeaders.Add("X-User-Email", $"{subject}@example.test");
        return client;
    }

    private Guid GetSubjectId(string subject)
    {
        if (!_subjects.TryGetValue(subject, out var id))
        {
            id = Guid.NewGuid();
            _subjects[subject] = id;
        }

        return id;
    }

    private async Task<Guid> AddUser(string subject)
    {
        var response = await Client(subject).GetFromJsonAsync<AuthResponse>("/auth/me");
        return response!.User.Id;
    }

    private void Seed(Guid workspaceId, Guid ownerId) =>
        _factory.Services.GetRequiredService<IWorkspaceRepository>().AsMemory().Add(new Workspace { Id = workspaceId, OwnerUserId = ownerId, Name = "Test" });

    private void SeedMember(Guid workspaceId, Guid userId, bool accepted, bool removed) =>
        _factory.Services.GetRequiredService<IWorkspaceRepository>().AsMemory().Add(new WorkspaceMember
        {
            Id = Guid.NewGuid(), WorkspaceId = workspaceId, UserId = userId, Role = WorkspaceRole.Contributor,
            AcceptedAt = accepted ? DateTime.UtcNow : null, RemovedAt = removed ? DateTime.UtcNow : null
        });

    private sealed record AuthResponse(AuthUser User);
    private sealed record AuthUser(Guid Id);

    public sealed class Factory : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder) => builder.ConfigureServices(services =>
        {
            services.RemoveAll<IShopService>();
            services.AddSingleton<IShopService, StubShopService>();
        });
    }

    private sealed class StubShopService : IShopService
    {
        public Task<IEnumerable<ShopResponse>> GetShopsForWorkspaceAsync(Guid id) => Task.FromResult<IEnumerable<ShopResponse>>([]);
        public Task<ConnectResponse> InitiateWorkspaceEtsyConnectAsync(Guid w, Guid u, string? r = null) => Task.FromResult(new ConnectResponse { AuthUrl = "https://etsy.test" });
        public Task<CallbackResponse> HandleWorkspaceEtsyCallbackAsync(Guid w, Guid u, string c, string s) => Task.FromResult(new CallbackResponse());
        public Task DeleteWorkspaceShopAsync(Guid w, Guid s) => Task.CompletedTask;
        public Task<SyncResponse> SyncWorkspaceShopAsync(Guid w, Guid s) => Task.FromResult(new SyncResponse { Status = "Completed" });
        public Task<IEnumerable<ShopResponse>> GetShopsAsync(Guid u) => Task.FromResult<IEnumerable<ShopResponse>>([]);
        public Task<ConnectResponse> InitiateEtsyConnectAsync(Guid u, string? r = null) => throw new NotSupportedException();
        public Task<CallbackResponse> HandleEtsyCallbackAsync(string c, string s) => throw new NotSupportedException();
        public Task DeleteShopAsync(Guid u, Guid s) => throw new NotSupportedException();
        public Task<SyncResponse> InitiateSyncAsync(Guid u, Guid s) => throw new NotSupportedException();
    }
}

internal static class WorkspaceRepositoryTestExtensions
{
    public static PrintHub.Infrastructure.Repositories.InMemoryWorkspaceRepository AsMemory(this IWorkspaceRepository repository) =>
        (PrintHub.Infrastructure.Repositories.InMemoryWorkspaceRepository)repository;
}

public sealed class WorkspaceEtsyRealEndpointTests : IClassFixture<WorkspaceEtsyRealEndpointTests.Factory>
{
    private readonly Factory _factory;
    public WorkspaceEtsyRealEndpointTests(Factory factory) => _factory = factory;

    [Fact]
    public async Task Connect_callback_sync_and_product_reads_use_workspace_scoped_encrypted_state()
    {
        var owner = AuthenticatedClient(Guid.NewGuid());
        var workspace = await (await owner.PostAsJsonAsync("/workspaces", new { name = "Etsy Team" }))
            .Content.ReadFromJsonAsync<WorkspaceResponse>();

        var connect = await owner.PostAsJsonAsync($"/workspaces/{workspace!.Id}/shops/connect/etsy", new { });
        connect.StatusCode.Should().Be(HttpStatusCode.OK);
        var authUrl = (await connect.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("authUrl").GetString();
        var state = ParseQuery(authUrl!)["state"];

        var callback = await owner.PostAsJsonAsync($"/workspaces/{workspace.Id}/shops/etsy/callback", new { code = "valid-code", state });
        callback.StatusCode.Should().Be(HttpStatusCode.OK);
        var callbackBody = await callback.Content.ReadFromJsonAsync<JsonElement>();
        var shopId = callbackBody.GetProperty("shopId").GetGuid();

        using (var scope = _factory.Services.CreateScope())
        {
            var shops = await scope.ServiceProvider.GetRequiredService<IShopRepository>().GetByWorkspaceIdAsync(workspace.Id);
            var shop = shops.Should().ContainSingle().Subject;
            shop.Id.Should().Be(shopId);
            shop.WorkspaceId.Should().Be(workspace.Id);
            var encryption = scope.ServiceProvider.GetRequiredService<ITokenEncryptionService>();
            encryption.Decrypt(shop.AccessToken).Should().StartWith("fake_access_token_");
            encryption.Decrypt(shop.RefreshToken).Should().StartWith("fake_refresh_token_");
            shop.AccessToken.Should().NotStartWith("fake_access_token_");
            shop.RefreshToken.Should().NotStartWith("fake_refresh_token_");
        }

        (await owner.PostAsync($"/workspaces/{workspace.Id}/shops/{shopId}/sync", null)).StatusCode.Should().Be(HttpStatusCode.OK);
        var products = await owner.GetFromJsonAsync<JsonElement>($"/workspaces/{workspace.Id}/products");
        var firstProducts = products.GetProperty("products").EnumerateArray().ToList();
        firstProducts.Should().NotBeEmpty();

        (await owner.PostAsync($"/workspaces/{workspace.Id}/shops/{shopId}/sync", null)).StatusCode.Should().Be(HttpStatusCode.OK);
        products = await owner.GetFromJsonAsync<JsonElement>($"/workspaces/{workspace.Id}/products");
        var secondProducts = products.GetProperty("products").EnumerateArray().ToList();
        secondProducts.Should().HaveCount(firstProducts.Count);

        var productId = firstProducts[0].GetProperty("id").GetGuid();
        (await owner.GetAsync($"/workspaces/{workspace.Id}/products/{productId}")).StatusCode.Should().Be(HttpStatusCode.OK);
        (await AuthenticatedClient(Guid.NewGuid()).GetAsync($"/workspaces/{workspace.Id}/products/{productId}"))
            .StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    private HttpClient AuthenticatedClient(Guid subject)
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-User-Id", subject.ToString());
        client.DefaultRequestHeaders.Add("X-User-Email", $"{subject}@example.test");
        return client;
    }

    private static Dictionary<string, string> ParseQuery(string url)
    {
        var query = new Uri(url).Query.TrimStart('?');
        return query.Split('&', StringSplitOptions.RemoveEmptyEntries)
            .Select(part => part.Split('=', 2))
            .ToDictionary(
                parts => Uri.UnescapeDataString(parts[0]),
                parts => parts.Length > 1 ? Uri.UnescapeDataString(parts[1]) : string.Empty);
    }

    private sealed record WorkspaceResponse(Guid Id, string Name, string Role);

    public sealed class Factory : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder) => builder.ConfigureServices(services =>
        {
            services.RemoveAll<IEtsyService>();
            services.AddSingleton<IEtsyService, FakeEtsyService>();
        });
    }
}
