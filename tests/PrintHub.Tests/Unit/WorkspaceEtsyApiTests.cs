using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using PrintHub.Core.Entities;
using PrintHub.Core.Enums;
using PrintHub.Core.Interfaces.Repositories;
using PrintHub.Core.Interfaces.Services;
using PrintHub.Infrastructure.Repositories;
using PrintHub.Infrastructure.Services.Etsy;
using Xunit;

namespace PrintHub.Tests.Unit;

public class WorkspaceEtsyApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public WorkspaceEtsyApiTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<IEtsyService>();
                services.AddSingleton<IEtsyService, FakeEtsyService>();
            });
        });
    }

    [Fact]
    public async Task EtsyWorkflow_IsWorkspaceScoped_EncryptsTokens_AndSyncsIntoProductReads()
    {
        using var owner = AuthenticatedClient(Guid.NewGuid());
        var workspace = await CreateWorkspaceAsync(owner, "Etsy Ops");

        var connect = await owner.PostAsJsonAsync($"/workspaces/{workspace.Id}/shops/connect/etsy", new { returnUrl = "/settings" });
        connect.StatusCode.Should().Be(HttpStatusCode.OK);
        var auth = await connect.Content.ReadFromJsonAsync<ConnectResponse>();
        var state = Query(auth!.AuthUrl)["state"];

        var callback = await owner.PostAsJsonAsync($"/workspaces/{workspace.Id}/shops/etsy/callback", new { code = "oauth-code", state });
        callback.StatusCode.Should().Be(HttpStatusCode.OK);
        var connected = await callback.Content.ReadFromJsonAsync<CallbackResponse>();

        using (var scope = _factory.Services.CreateScope())
        {
            var shopRepository = scope.ServiceProvider.GetRequiredService<IShopRepository>();
            var shop = await shopRepository.GetByIdAsync(connected!.ShopId);
            shop.Should().NotBeNull();
            shop!.WorkspaceId.Should().Be(workspace.Id);
            shop.AccessToken.Should().NotStartWith("fake_access_token");
            shop.RefreshToken.Should().NotStartWith("fake_refresh_token");
        }

        var listed = await owner.GetFromJsonAsync<ShopsResponse>($"/workspaces/{workspace.Id}/shops");
        listed!.Shops.Should().ContainSingle(x => x.Id == connected!.ShopId);

        var sync = await owner.PostAsync($"/workspaces/{workspace.Id}/shops/{connected!.ShopId}/sync", null);
        sync.StatusCode.Should().Be(HttpStatusCode.OK);

        var products = await owner.GetFromJsonAsync<ProductsResponse>($"/workspaces/{workspace.Id}/products");
        products!.Products.Should().Contain(x => x.ExternalListingId == "etsy_listing_001" && x.Name == "Dino Wall Hook");

        var syncAgain = await owner.PostAsync($"/workspaces/{workspace.Id}/shops/{connected.ShopId}/sync", null);
        syncAgain.StatusCode.Should().Be(HttpStatusCode.OK);
        var afterSecondSync = await owner.GetFromJsonAsync<ProductsResponse>($"/workspaces/{workspace.Id}/products");
        afterSecondSync!.Products.Select(x => x.ExternalListingId).Should().OnlyHaveUniqueItems();
    }

    [Fact]
    public async Task EtsyWorkspaceRoutes_EnforceAcceptedMembershipAndOwnerOnlyAdministration()
    {
        using var owner = AuthenticatedClient(Guid.NewGuid());
        var workspace = await CreateWorkspaceAsync(owner, "Secured Etsy");
        var ownerUser = (await owner.GetFromJsonAsync<AuthMeResponse>("/auth/me"))!.User.Id;
        var shopId = Guid.NewGuid();

        var contributorSubject = Guid.NewGuid();
        var pendingSubject = Guid.NewGuid();
        var removedSubject = Guid.NewGuid();
        var strangerSubject = Guid.NewGuid();
        using var contributor = AuthenticatedClient(contributorSubject);
        using var pending = AuthenticatedClient(pendingSubject);
        using var removed = AuthenticatedClient(removedSubject);
        using var stranger = AuthenticatedClient(strangerSubject);

        var contributorUser = (await contributor.GetFromJsonAsync<AuthMeResponse>("/auth/me"))!.User.Id;
        var pendingUser = (await pending.GetFromJsonAsync<AuthMeResponse>("/auth/me"))!.User.Id;
        var removedUser = (await removed.GetFromJsonAsync<AuthMeResponse>("/auth/me"))!.User.Id;

        using (var scope = _factory.Services.CreateScope())
        {
            var workspaces = (InMemoryWorkspaceRepository)scope.ServiceProvider.GetRequiredService<IWorkspaceRepository>();
            workspaces.Add(Member(workspace.Id, contributorUser, accepted: true));
            workspaces.Add(Member(workspace.Id, pendingUser, accepted: false));
            workspaces.Add(Member(workspace.Id, removedUser, accepted: true, removed: true));

            var shops = scope.ServiceProvider.GetRequiredService<IShopRepository>();
            await shops.AddAsync(new Shop
            {
                Id = shopId,
                WorkspaceId = workspace.Id,
                UserId = ownerUser,
                Provider = "etsy",
                ExternalId = "etsy-shop",
                ShopName = "Workspace Shop",
                AccessToken = "encrypted-access",
                RefreshToken = "encrypted-refresh"
            });
        }

        (await contributor.GetAsync($"/workspaces/{workspace.Id}/shops")).StatusCode.Should().Be(HttpStatusCode.OK);
        (await contributor.GetAsync($"/workspaces/{workspace.Id}/products")).StatusCode.Should().Be(HttpStatusCode.OK);
        (await contributor.PostAsJsonAsync($"/workspaces/{workspace.Id}/shops/connect/etsy", new { })).StatusCode.Should().Be(HttpStatusCode.Forbidden);
        (await contributor.PostAsync($"/workspaces/{workspace.Id}/shops/{shopId}/sync", null)).StatusCode.Should().Be(HttpStatusCode.Forbidden);
        (await contributor.DeleteAsync($"/workspaces/{workspace.Id}/shops/{shopId}")).StatusCode.Should().Be(HttpStatusCode.Forbidden);

        (await pending.GetAsync($"/workspaces/{workspace.Id}/shops")).StatusCode.Should().Be(HttpStatusCode.Forbidden);
        (await removed.GetAsync($"/workspaces/{workspace.Id}/shops")).StatusCode.Should().Be(HttpStatusCode.Forbidden);
        (await stranger.GetAsync($"/workspaces/{workspace.Id}/shops")).StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    private async Task<WorkspaceResponse> CreateWorkspaceAsync(HttpClient client, string name)
    {
        var response = await client.PostAsJsonAsync("/workspaces", new { name });
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        return (await response.Content.ReadFromJsonAsync<WorkspaceResponse>())!;
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

    private static Dictionary<string, string> Query(string url) =>
        new Uri(url).Query.TrimStart('?')
            .Split('&', StringSplitOptions.RemoveEmptyEntries)
            .Select(part => part.Split('=', 2))
            .ToDictionary(parts => Uri.UnescapeDataString(parts[0]), parts => Uri.UnescapeDataString(parts[1]));

    private sealed record WorkspaceResponse(Guid Id, string Name, string Role);
    private sealed record AuthMeResponse(AuthUser User);
    private sealed record AuthUser(Guid Id);
    private sealed record ConnectResponse(string AuthUrl);
    private sealed record CallbackResponse(Guid ShopId, string ShopName, bool Connected);
    private sealed record ShopsResponse(List<ShopDto> Shops);
    private sealed record ShopDto(Guid Id, string ShopName);
    private sealed record ProductsResponse(List<ProductDto> Products);
    private sealed record ProductDto(string ExternalListingId, string Name);
}
