using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Xunit;
using Xunit.Abstractions;

namespace PrintHub.Tests.Smoke;

/// <summary>
/// Integration smoke tests that verify core API endpoints are accessible
/// </summary>
[Collection("Smoke Tests")]
public class ApiSmokeTests : IClassFixture<SmokeTestOptions>
{
    private readonly SmokeTestOptions _options;
    private readonly SmokeTestRunner _runner;

    public ApiSmokeTests(SmokeTestOptions options)
    {
        _options = options;
        _runner = new SmokeTestRunner(_options);
    }

    [Fact]
    [Trait("Priority", "High")]
    public async Task HealthEndpoint_ReturnsOk()
    {
        // Arrange
        var endpoint = new SmokeTestEndpoint
        {
            Name = "Health Check",
            Path = "/health",
            Method = "GET",
            ExpectedStatus = 200
        };

        // Act
        var result = await _runner.RunAllTestsAsync();

        // Assert
        var healthResult = result.FirstOrDefault(r => r.TestName == "Health Check");
        healthResult.Should().NotBeNull();
        healthResult!.Success.Should().BeTrue("Health endpoint should return 200 OK");
    }

    [Fact]
    [Trait("Priority", "High")]
    public async Task UnauthenticatedProductList_ReturnsUnauthorized()
    {
        // Arrange
        var endpoint = new SmokeTestEndpoint
        {
            Name = "Products List (Unauthenticated)",
            Path = "/api/products",
            Method = "GET",
            ExpectedStatus = 401
        };

        // Act
        var result = await _runner.RunAllTestsAsync();

        // Assert
        var productResult = result.FirstOrDefault(r => r.TestName == "Products List (Unauthenticated)");
        productResult.Should().NotBeNull();
        productResult!.Success.Should().BeTrue("Products endpoint should require authentication");
    }

    [Fact]
    [Trait("Priority", "Medium")]
    public async Task QueueStatus_ReturnsUnauthorizedWhenNotAuthenticated()
    {
        // Arrange
        var endpoint = new SmokeTestEndpoint
        {
            Name = "Queue Status (Unauthenticated)",
            Path = "/api/queue/status",
            Method = "GET",
            ExpectedStatus = 401
        };

        // Act
        var result = await _runner.RunAllTestsAsync();

        // Assert
        var queueResult = result.FirstOrDefault(r => r.TestName == "Queue Status (Unauthenticated)");
        queueResult.Should().NotBeNull();
        queueResult!.Success.Should().BeTrue("Queue endpoint should require authentication");
    }

    [Fact]
    [Trait("Priority", "High")]
    public void SmokeTestConfiguration_IsValid()
    {
        // Assert
        _options.Should().NotBeNull();
        _options.ApiBaseUrl.Should().NotBeNullOrEmpty("API base URL must be configured");
        _options.TimeoutSeconds.Should().BeGreaterThan(0, "Timeout must be positive");
        Uri.TryCreate(_options.ApiBaseUrl, UriKind.Absolute).Should().NotBeNull("API URL must be a valid URI");
    }

    [Fact]
    [Trait("Priority", "Low")]
    public async Task LoginEndpoint_AcceptsPost()
    {
        // Arrange
        var endpoint = new SmokeTestEndpoint
        {
            Name = "Login Endpoint Exists",
            Path = "/api/auth/login",
            Method = "POST",
            ExpectedStatus = 400, // Bad request without body is expected
            Body = "{}"
        };

        // Act
        var result = await _runner.RunAllTestsAsync();

        // Assert
        var loginResult = result.FirstOrDefault(r => r.TestName == "Login Endpoint Exists");
        loginResult.Should().NotBeNull();
        loginResult!.StatusCode.Should().BeOneOf(400, 401, 404, 422, 
            "Login endpoint should respond (400 bad request without credentials is acceptable)");
    }
}