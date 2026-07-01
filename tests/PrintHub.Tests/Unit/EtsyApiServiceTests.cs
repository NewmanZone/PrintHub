using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using PrintHub.Infrastructure.Services;
using PrintHub.Infrastructure.Services.Etsy;
using PrintHub.Core.Interfaces.Services;
using Xunit;

namespace PrintHub.Tests.Unit;

[Collection("Unit Tests")]
public class EtsyApiServiceTests
{
    private static HttpClient CreateMockHttpClient(Func<HttpRequestMessage, CancellationToken, HttpResponseMessage> responseHandler, out List<HttpRequestMessage> capturedRequests)
    {
        capturedRequests = new List<HttpRequestMessage>();
        var mockHandler = new Mock<HttpMessageHandler>();
        mockHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync((HttpRequestMessage request, CancellationToken ct) =>
            {
                capturedRequests.Add(request);
                return responseHandler(request, ct);
            });
        
        return new HttpClient(mockHandler.Object);
    }

    [Fact]
    public async Task ExchangeCodeForTokenAsync_DeserializesSnakeCaseTokenResponse()
    {
        // Arrange
        var json = @"{
            ""access_token"": ""token_123"",
            ""refresh_token"": ""refresh_456"",
            ""expires_in"": 3600,
            ""token_type"": ""Bearer""
        }";
        
        var httpClient = CreateMockHttpClient((_, __) => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        });
        
        var config = new EtsyConfiguration
        {
            ClientId = "test_client",
            ClientSecret = "test_secret",
            RedirectUri = "https://localhost/callback"
        };
        
        var logger = new Mock<ILogger<EtsyApiService>>();
        var service = new EtsyApiService(httpClient, config, logger.Object);
        
        // Act
        var result = await service.ExchangeCodeForTokenAsync("code", config.RedirectUri, "verifier");
        
        // Assert
        result.Should().NotBeNull();
        result.AccessToken.Should().Be("token_123");
        result.RefreshToken.Should().Be("refresh_456");
        result.ExpiresIn.Should().Be(3600);
        result.TokenType.Should().Be("Bearer");
    }

    [Fact]
    public async Task GetShopInfoAsync_DeserializesSnakeCaseShopResponse_AndUsesCorrectUrlAndHeaders()
    {
        // Arrange
        var json = @"{
            ""results"": [
                {
                    ""shop_id"": 12345,
                    ""shop_name"": ""MyTestShop"",
                    ""email"": ""test@example.com"",
                    ""image_url"": ""https://example.com/image.jpg""
                }
            ]
        }";
        
        var httpClient = CreateMockHttpClient((_, __) => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        }, out var capturedRequests);
        
        var config = new EtsyConfiguration
        {
            ClientId = "test_client_id",
            BaseUrl = "https://openapi.etsy.com/v3"
        };
        var logger = new Mock<ILogger<EtsyApiService>>();
        var service = new EtsyApiService(httpClient, config, logger.Object);
        
        // Act
        var result = await service.GetShopInfoAsync("access_token");
        
        // Assert
        result.Should().NotBeNull();
        result.ShopId.Should().Be("12345");
        result.ShopName.Should().Be("MyTestShop");
        
        capturedRequests.Should().HaveCount(1);
        var request = capturedRequests[0];
        request.RequestUri!.ToString().Should().Contain("/users/__SELF__/shops");
        request.Headers.Authorization!.Scheme.Should().Be("Bearer");
        request.Headers.Authorization.Parameter.Should().Be("access_token");
        request.Headers.Contains("x-api-key").Should().BeTrue();
        request.Headers.GetValues("x-api-key").First().Should().Be("test_client_id");
    }

    [Fact]
    public async Task GetListingsAsync_DeserializesSnakeCaseListingResponse_AndUsesCorrectUrlAndHeaders()
    {
        // Arrange
        var json = @"{
            ""results"": [
                {
                    ""listing_id"": 98765,
                    ""title"": ""Dino Hook"",
                    ""description"": ""A hook"",
                    ""price"": 24.99,
                    ""state"": ""active"",
                    ""creation_date"": ""2026-01-01T00:00:00Z"",
                    ""last_modified_date"": ""2026-06-15T00:00:00Z"",
                    ""main_image"": { ""url_full"": ""https://example.com/img1.jpg"" },
                    ""images"": [{ ""url_full"": ""https://example.com/img2.jpg"" }]
                }
            ],
            ""pagination"": { ""next"": null }
        }";
        
        var httpClient = CreateMockHttpClient((_, __) => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        }, out var capturedRequests);
        
        var config = new EtsyConfiguration
        {
            ClientId = "test_client_id",
            BaseUrl = "https://openapi.etsy.com/v3"
        };
        var logger = new Mock<ILogger<EtsyApiService>>();
        var service = new EtsyApiService(httpClient, config, logger.Object);
        
        // Act
        var result = await service.GetListingsAsync("access_token", "shop_123");
        
        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(1);
        var listing = result.First();
        listing.ListingId.Should().Be("98765");
        listing.Title.Should().Be("Dino Hook");
        listing.Price.Should().Be(24.99m);
        listing.State.Should().Be("active");
        listing.ImageUrl.Should().Be("https://example.com/img1.jpg");
        
        capturedRequests.Should().HaveCount(1);
        var request = capturedRequests[0];
        request.RequestUri!.ToString().Should().Contain("/shops/shop_123/listings/active");
        request.RequestUri!.ToString().Should().Contain("limit=100");
        request.RequestUri!.ToString().Should().Contain("includes=Images");
        request.Headers.Authorization!.Scheme.Should().Be("Bearer");
        request.Headers.Authorization.Parameter.Should().Be("access_token");
        request.Headers.Contains("x-api-key").Should().BeTrue();
        request.Headers.GetValues("x-api-key").First().Should().Be("test_client_id");
    }
}
