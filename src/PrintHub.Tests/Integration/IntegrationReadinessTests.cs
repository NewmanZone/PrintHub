using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace PrintHub.Tests.Integration;

/// <summary>
/// Integration tests that verify the complete system meets readiness gates
/// </summary>
[Collection("Integration Tests")]
[Trait("Category", "Integration")]
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
    [Trait("Priority", "High")]
    public void IntegrationGateOptions_AreConfigured()
    {
        // Assert - verify gates are properly configured
        _options.Should().NotBeNull("Integration gates should be configurable");

        // At minimum, smoke tests and unit tests should be required
        _options.RequireAllSmokeTestsPass.Should()
            .BeTrue("Smoke tests should be required for integration");
        _options.RequireUnitTestsPass.Should()
            .BeTrue("Unit tests should be required for integration");
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
    [Trait("Priority", "High")]
    public void IntegrationGateRunner_CanBeInstantiated()
    {
        // Arrange
        var runner = new IntegrationGateRunner(_options);

        // Assert
        runner.Should().NotBeNull();
    }

    [Fact]
    [Trait("Priority", "Medium")]
    public void GateCheckResult_CanBeCreated()
    {
        // Arrange & Act
        var passResult = GateCheckResult.Pass("Test", "Test passed");
        var failResult = GateCheckResult.Fail("Test", "Test failed");

        // Assert
        passResult.Passed.Should().BeTrue();
        passResult.GateName.Should().Be("Test");
        failResult.Passed.Should().BeFalse();
        failResult.GateName.Should().Be("Test");
    }

    [Fact]
    [Trait("Priority", "Low")]
    public void IntegrationReadinessReport_CanBeCreated()
    {
        // Arrange
        var report = new IntegrationReadinessReport();

        // Act
        report.GateResults.Add(GateCheckResult.Pass("SmokeTests", "All passed"));
        report.TotalGates = 1;
        report.PassedGates = 1;

        // Assert
        report.GateResults.Should().HaveCount(1);
        report.TotalGates.Should().Be(1);
        report.PassedGates.Should().Be(1);
    }
}
