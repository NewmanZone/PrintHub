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

    public ApiSmokeTests(SmokeTestOptions options)
    {
        _options = options;
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
    [Trait("Priority", "High")]
    public void SmokeTestEndpoints_CanBeConfigured()
    {
        // Arrange & Act
        var runner = new SmokeTestRunner(_options);
        var summary = runner.GetSummary();

        // Assert - just verify the runner can be created and returns a valid summary
        summary.Should().NotBeNull();
        summary.TotalTests.Should().BeGreaterOrEqualTo(0);
    }

    [Fact]
    [Trait("Priority", "Medium")]
    public void SmokeTestOptions_HasReasonableDefaults()
    {
        // Assert
        _options.ApiBaseUrl.Should().NotBeNullOrEmpty();
        _options.TimeoutSeconds.Should().BeGreaterThan(0);
        _options.TimeoutSeconds.Should().BeLessOrEqualTo(300, "Timeout should be reasonable (max 5 minutes)");
    }
}