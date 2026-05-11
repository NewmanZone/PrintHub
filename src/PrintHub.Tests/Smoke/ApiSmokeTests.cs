using FluentAssertions;
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
        Uri.TryCreate(_options.ApiBaseUrl, UriKind.Absolute, out _).Should().BeTrue("API URL must be a valid URI");
    }

    [Fact]
    [Trait("Priority", "Low")]
    public async Task LoginEndpoint_AcceptsPost()
    {
        // Act
        var result = await _runner.RunAllTestsAsync();

        // Assert
        var loginResult = result.FirstOrDefault(r => r.TestName == "Login Endpoint Exists");
        loginResult.Should().NotBeNull();
        loginResult!.StatusCode.Should().BeOneOf(400, 401, 404, 422);
    }
}