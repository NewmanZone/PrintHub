namespace PrintHub.Tests.Smoke;

/// <summary>
/// Configuration for smoke test execution
/// </summary>
public class SmokeTestOptions
{
    public string ApiBaseUrl { get; set; } = "http://localhost:5000";
    public string FrontendUrl { get; set; } = "http://localhost:3000";
    public int TimeoutSeconds { get; set; } = 30;
    public bool Enabled { get; set; } = true;
    public List<SmokeTestEndpoint> Endpoints { get; set; } = new();
}

public class SmokeTestEndpoint
{
    public string Name { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public int ExpectedStatus { get; set; } = 200;
    public string Method { get; set; } = "GET";
    public Dictionary<string, string>? Headers { get; set; }
    public string? Body { get; set; }
    public bool RequiresAuth { get; set; } = false;
}