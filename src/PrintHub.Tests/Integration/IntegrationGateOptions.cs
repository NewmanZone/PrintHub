namespace PrintHub.Tests.Integration;

/// <summary>
/// Configuration for integration readiness gates
/// </summary>
public class IntegrationGateOptions
{
    /// <summary>
    /// If true, all smoke tests must pass for integration to proceed
    /// </summary>
    public bool RequireAllSmokeTestsPass { get; set; } = true;

    /// <summary>
    /// If true, all unit tests must pass
    /// </summary>
    public bool RequireUnitTestsPass { get; set; } = true;

    /// <summary>
    /// If true, visual checks must pass (currently optional)
    /// </summary>
    public bool RequireVisualChecksPass { get; set; } = false;

    /// <summary>
    /// Minimum code coverage percentage required (0 = no requirement)
    /// </summary>
    public int MinCoveragePercent { get; set; } = 0;

    /// <summary>
    /// If true, block integration on security scan findings
    /// </summary>
    public bool BlockOnSecurityScan { get; set; } = true;

    /// <summary>
    /// Paths to security scan results (OWASP ZAP, Snyk, etc.)
    /// </summary>
    public List<string> SecurityScanPaths { get; set; } = new();

    /// <summary>
    /// Maximum acceptable severity for security findings
    /// </summary>
    public SecuritySeverity MaxAcceptableSeverity { get; set; } = SecuritySeverity.Low;
}

public enum SecuritySeverity
{
    None = 0,
    Low = 1,
    Medium = 2,
    High = 3,
    Critical = 4
}