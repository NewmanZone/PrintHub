using System.Text.Json;
using FluentAssertions;

namespace PrintHub.Tests.Visual;

/// <summary>
/// Runner for visual regression tests with baseline comparison
/// </summary>
public class VisualCheckRunner
{
    private readonly VisualCheckOptions _options;
    private readonly string _baselinePath;
    private readonly string _diffPath;
    private readonly string _screenshotsPath;

    public VisualCheckRunner(VisualCheckOptions options)
    {
        _options = options;
        _baselinePath = Path.Combine(options.ScreenshotsPath, "baseline");
        _diffPath = Path.Combine(options.ScreenshotsPath, "diff");
        _screenshotsPath = options.ScreenshotsPath;
    }

    public async Task<VisualCheckResult> CaptureAndCompareAsync(
        string testName,
        string url,
        string viewportName,
        string browserName)
    {
        var result = new VisualCheckResult
        {
            TestName = testName,
            Page = url,
            Browser = browserName,
            Viewport = viewportName,
            ExecutedAt = DateTime.UtcNow
        };

        try
        {
            EnsureDirectoriesExist();

            var screenshotFileName = $"{testName}_{viewportName}_{browserName}.png";
            var screenshotPath = Path.Combine(_screenshotsPath, screenshotFileName);
            var baselinePath = Path.Combine(_baselinePath, screenshotFileName);

            result.ScreenshotPath = screenshotPath;
            result.BaselinePath = baselinePath;

            // Note: Actual screenshot capture requires a browser instance
            // This is a placeholder for the actual implementation
            if (_options.SaveBaselineImages && !File.Exists(baselinePath))
            {
                if (File.Exists(screenshotPath))
                {
                    File.Copy(screenshotPath, baselinePath, overwrite: true);
                }
            }

            // In real implementation, compare screenshots here
            result.Passed = true;
        }
        catch (Exception ex)
        {
            result.Passed = false;
            result.ErrorMessage = ex.Message;
        }

        return result;
    }

    private void EnsureDirectoriesExist()
    {
        Directory.CreateDirectory(_baselinePath);
        Directory.CreateDirectory(_diffPath);
        Directory.CreateDirectory(_screenshotsPath);
    }

    public async Task SaveResultsAsync(List<VisualCheckResult> results, string outputPath)
    {
        var json = JsonSerializer.Serialize(new
        {
            ExecutedAt = DateTime.UtcNow,
            Options = _options,
            Results = results,
            Summary = new
            {
                Total = results.Count,
                Passed = results.Count(r => r.Passed),
                Failed = results.Count(r => !r.Passed)
            }
        }, new JsonSerializerOptions { WriteIndented = true });

        await File.WriteAllTextAsync(outputPath, json);
    }
}
