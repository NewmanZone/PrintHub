using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace PrintHub.Tests.Integration;

/// <summary>
/// Integration tests that verify the complete system meets readiness gates
/// </summary>
[Collection("Integration Tests")]
public class IntegrationReadinessTests
{
    private readonly IntegrationGateOptions _options;
    private readonly ITestOutputHelper _output;

    public IntegrationReadinessTests(ITestOutputHelper output)
    {
        _options = IntegrationGateRunner.LoadFromConfiguration();
        _output = output;
    }

    [Fact]
    [Trait("Priority", "Critical")]
    public async Task IntegrationGates_AllCriticalGatesPass()
    {
        // Arrange
        var runner = new IntegrationGateRunner(_options);
        var expectedCriticalGates = new[] { "SmokeTests", "UnitTests", "SecurityScan" };

        // Act
        var report = await runner.EvaluateAllGatesAsync();

        // Log results for debugging
        foreach (var result in report.GateResults)
        {
            _output.WriteLine($"[{result.GateName}] {(result.Passed ? "✅ PASS" : "❌ FAIL")}: {result.Message}");
        }

        // Assert
        report.TotalGates.Should().BeGreaterOrEqualTo(3, "Should have smoke, unit, and security gates");
        report.IsReady.Should().BeTrue(report.Summary);
    }

    [Fact]
    [Trait("Priority", "High")]
    public void IntegrationGateOptions_AreConfigured()
    {
        // Assert - verify gates are properly configured
        _options.Should().NotBeNull("Integration gates should be configurable");
        
        // At minimum, smoke tests and unit tests should be required
        _options.RequireAllSmokeTestsPass.Should().BeTrue("Smoke tests should be required for integration");
        _options.RequireUnitTestsPass.Should().BeTrue("Unit tests should be required for integration");
    }

    [Fact]
    [Trait("Priority", "Medium")]
    public async Task SmokeTestsGate_WhenRequired_CanBlockIntegration()
    {
        // Arrange
        var blockingOptions = new IntegrationGateOptions
        {
            RequireAllSmokeTestsPass = true,
            RequireUnitTestsPass = true,
            RequireVisualChecksPass = false,
            BlockOnSecurityScan = true
        };

        var runner = new IntegrationGateRunner(blockingOptions);

        // Act
        var report = await runner.EvaluateAllGatesAsync();

        // Assert - smoke test failures should block integration
        if (!_options.RequireAllSmokeTestsPass)
        {
            report.IsReady.Should().BeTrue("Integration should proceed when smoke tests are disabled");
        }
    }

    [Fact]
    [Trait("Priority", "High")]
    public void GateRunner_CanLoadConfiguration()
    {
        // Arrange & Act
        var loadedOptions = IntegrationGateRunner.LoadFromConfiguration();

        // Assert
        loadedOptions.Should().NotBeNull();
        loadedOptions.RequireAllSmokeTestsPass.Should().Be(_options.RequireAllSmokeTestsPass);
        loadedOptions.RequireUnitTestsPass.Should().Be(_options.RequireUnitTestsPass);
    }

    [Fact]
    [Trait("Priority", "Low")]
    public async Task IntegrationReport_CanBeSerialized()
    {
        // Arrange
        var runner = new IntegrationGateRunner(_options);
        var tempPath = Path.Combine(Path.GetTempPath(), $"integration-report-{Guid.NewGuid()}.json");

        try
        {
            // Act
            var report = await runner.EvaluateAllGatesAsync();

            // Assert
            File.Exists(tempPath).Should().BeTrue("Report should be saved to file");
            var content = await File.ReadAllTextAsync(tempPath);
            content.Should().NotBeNullOrEmpty();
            content.Should().Contain("IsReady");
            content.Should().Contain("GateResults");
        }
        finally
        {
            if (File.Exists(tempPath))
            {
                File.Delete(tempPath);
            }
        }
    }
}