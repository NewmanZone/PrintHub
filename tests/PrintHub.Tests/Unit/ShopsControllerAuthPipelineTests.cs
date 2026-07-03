using System.Net;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace PrintHub.Tests.Unit;

public class ShopsControllerAuthPipelineTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public ShopsControllerAuthPipelineTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetShops_WithoutAuthentication_ReturnsUnauthorized()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/Shops");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetShops_WithDevelopmentHeaderAuthentication_ReturnsOk()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-User-Id", Guid.NewGuid().ToString());

        var response = await client.GetAsync("/api/Shops");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
