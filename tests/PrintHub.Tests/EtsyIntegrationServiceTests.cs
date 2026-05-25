using System.Net;
using System.Text;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using PrintHub.Api;
using Xunit;

namespace PrintHub.Tests;

public class EtsyIntegrationServiceTests
{
    [Fact]
    public async Task CreateAuthorizationUrlAsync_UsesPkceAndStoresState()
    {
        var store = new InMemoryPrintHubStore();
        var service = CreateService(store, new EtsyHttpHandler(_ => Json("{}")));

        var authUrl = await service.CreateAuthorizationUrlAsync("https://api.printhub.test", "https://app.printhub.test/settings", CancellationToken.None);

        authUrl.Should().StartWith("https://www.etsy.com/oauth/connect?");
        var query = Query(authUrl);
        query["client_id"].Should().Be("etsy-client");
        query["redirect_uri"].Should().Be("https://api.printhub.test/api/etsy/callback");
        query["response_type"].Should().Be("code");
        query["code_challenge_method"].Should().Be("S256");
        query["code_challenge"].Should().NotBeNullOrWhiteSpace();
        query["state"].Should().NotBeNullOrWhiteSpace();

        var state = await store.ReadAsync();
        state.OAuthStates.Should().ContainSingle();
        state.OAuthStates[0].CodeVerifier.Should().NotBeNullOrWhiteSpace();
        state.OAuthStates[0].ReturnUrl.Should().Be("https://app.printhub.test/settings");
    }

    [Fact]
    public async Task SyncListingsAsync_ImportsActiveEtsyListings()
    {
        var store = new InMemoryPrintHubStore
        {
            State =
            {
                EtsyConnection = new EtsyConnectionRecord(
                    "123456",
                    "Newman Zone",
                    "access-token",
                    "refresh-token",
                    DateTimeOffset.UtcNow.AddHours(1),
                    DateTimeOffset.UtcNow.AddDays(-1),
                    null)
            }
        };
        var handler = new EtsyHttpHandler(request =>
        {
            request.Headers.Authorization?.Parameter.Should().Be("access-token");
            request.Headers.GetValues("x-api-key").Should().Contain("etsy-client");
            request.RequestUri!.AbsolutePath.Should().Be("/v3/application/shops/123456/listings/active");
            return Json("""
            {
              "results": [
                {
                  "listing_id": 1001,
                  "title": "Dino Wall Hook",
                  "description": "Ready to print.",
                  "state": "active",
                  "price": { "amount": 1850, "divisor": 100 },
                  "Images": [{ "url_170x135": "https://img.test/dino.jpg" }]
                }
              ]
            }
            """);
        });
        var service = CreateService(store, handler);

        var result = await service.SyncListingsAsync(CancellationToken.None);

        result.Imported.Should().Be(1);
        result.Updated.Should().Be(0);
        result.Total.Should().Be(1);
        store.State.Products.Should().ContainSingle(product =>
            product.ExternalListingId == "1001"
            && product.Name == "Dino Wall Hook"
            && product.EtsyPrice == 18.5m
            && product.ImageUrl == "https://img.test/dino.jpg"
            && product.IsActive);
        store.State.EtsyConnection!.LastSyncAt.Should().NotBeNull();
    }

    [Fact]
    public async Task SyncListingsAsync_RefreshesExpiredTokenBeforeCallingEtsy()
    {
        var store = new InMemoryPrintHubStore
        {
            State =
            {
                EtsyConnection = new EtsyConnectionRecord(
                    "123456",
                    "Newman Zone",
                    "expired-token",
                    "refresh-token",
                    DateTimeOffset.UtcNow.AddMinutes(-5),
                    DateTimeOffset.UtcNow.AddDays(-1),
                    null)
            }
        };
        var calls = new List<string>();
        var handler = new EtsyHttpHandler(request =>
        {
            calls.Add($"{request.Method} {request.RequestUri!.AbsolutePath}");
            if (request.Method == HttpMethod.Post)
            {
                return Json("""{"access_token":"new-access","refresh_token":"new-refresh","expires_in":3600}""");
            }

            request.Headers.Authorization?.Parameter.Should().Be("new-access");
            return Json("""{"results":[]}""");
        });
        var service = CreateService(store, handler);

        await service.SyncListingsAsync(CancellationToken.None);

        calls.Should().Equal("POST /v3/public/oauth/token", "GET /v3/application/shops/123456/listings/active");
        store.State.EtsyConnection!.AccessToken.Should().Be("new-access");
        store.State.EtsyConnection.RefreshToken.Should().Be("new-refresh");
    }

    private static EtsyIntegrationService CreateService(InMemoryPrintHubStore store, HttpMessageHandler handler)
    {
        var client = new HttpClient(handler);
        var factory = new Mock<IHttpClientFactory>();
        factory.Setup(f => f.CreateClient("Etsy")).Returns(client);

        return new EtsyIntegrationService(
            store,
            factory.Object,
            Options.Create(new EtsyOptions
            {
                ClientId = "etsy-client",
                ClientSecret = "etsy-secret",
                StateSigningSecret = "state-secret"
            }),
            NullLogger<EtsyIntegrationService>.Instance);
    }

    private static HttpResponseMessage Json(string body, HttpStatusCode status = HttpStatusCode.OK) =>
        new(status)
        {
            Content = new StringContent(body, Encoding.UTF8, "application/json")
        };

    private static Dictionary<string, string> Query(string url)
    {
        var uri = new Uri(url);
        return uri.Query.TrimStart('?')
            .Split('&', StringSplitOptions.RemoveEmptyEntries)
            .Select(part => part.Split('=', 2))
            .ToDictionary(parts => Uri.UnescapeDataString(parts[0]), parts => Uri.UnescapeDataString(parts[1]));
    }

    private sealed class EtsyHttpHandler(Func<HttpRequestMessage, HttpResponseMessage> responder) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) =>
            Task.FromResult(responder(request));
    }

    private sealed class InMemoryPrintHubStore : IPrintHubStore
    {
        public PrintHubState State { get; } = new();

        public Task<PrintHubState> ReadAsync(CancellationToken ct = default) => Task.FromResult(State);

        public Task WriteAsync(PrintHubState state, CancellationToken ct = default) => Task.CompletedTask;
    }
}
