using System.Text.Json;

namespace PrintHub.Tests.Integration;

/// <summary>
/// Result of an integration readiness gate check
/// </summary>
public class GateCheckResult
{
    public string GateName { get; set; } = string.Empty;
    public bool Passed { get; set; }
    public string? Message { get; set; }
    public Dictionary<string, object> Details { get; set; } = new();
    public DateTime CheckedAt { get; set; } = DateTime.UtcNow;

    public static GateCheckResult Pass(string gateName, string? message = null) => new()
    {
        GateName = gateName,
        Passed = true,
        Message = message ?? $"✅ {gateName} passed"
    };

    public static GateCheckResult Fail(string gateName, string message, Dictionary<string, object>? details = null) => new()
    {
        GateName = gateName,
        Passed = false,
        Message = message,
        Details = details ?? new Dictionary<string, object>()
    };
}

/// <summary>
/// Overall integration readiness assessment
/// </summary>
public class IntegrationReadinessReport
{
    public bool IsReady { get; set; }
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    public List<GateCheckResult> GateResults { get; set; } = new();
    public int TotalGates { get; set; }
    public int PassedGates { get; set; }
    public int FailedGates { get; set; }

    public double PassRate => TotalGates > 0 ? (double)PassedGates / TotalGates * 100 : 0;

    public string Summary => IsReady
        ? $"✅ Integration Ready ({PassedGates}/{TotalGates} gates passed - {PassRate:F1}%)"
        : $"❌ Integration Blocked ({PassedGates}/{TotalGates} gates passed - {FailedGates} blocked)";

    public async Task SaveToFileAsync(string path)
    {
        var json = JsonSerializer.Serialize(this, new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
        await File.WriteAllTextAsync(path, json);
    }
}