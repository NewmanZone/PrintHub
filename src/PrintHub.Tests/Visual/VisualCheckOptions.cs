namespace PrintHub.Tests.Visual;

/// <summary>
/// Configuration for visual regression checks
/// </summary>
public class VisualCheckOptions
{
    public bool Enabled { get; set; } = false;
    public string ScreenshotsPath { get; set; } = "./test-output/screenshots";
    public List<string> Browsers { get; set; } = new() { "chromium" };
    public List<ViewportConfig> Viewports { get; set; } = new();
    public double ComparisonThreshold { get; set; } = 0.05; // 5% pixel difference tolerance
    public bool SaveBaselineImages { get; set; } = true;
    public bool UpdateBaselines { get; set; } = false;
}

public class ViewportConfig
{
    public int Width { get; set; }
    public int Height { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class VisualCheckResult
{
    public string TestName { get; set; } = string.Empty;
    public string Page { get; set; } = string.Empty;
    public string Browser { get; set; } = string.Empty;
    public string Viewport { get; set; } = string.Empty;
    public bool Passed { get; set; }
    public string? ScreenshotPath { get; set; }
    public string? BaselinePath { get; set; }
    public double? DifferencePercent { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime ExecutedAt { get; set; } = DateTime.UtcNow;
}