using Microsoft.Playwright;

namespace PrintHub.Tests.Helpers;

/// <summary>
/// Helper utilities for test execution
/// </summary>
public static class TestHelpers
{
    /// <summary>
    /// Waits for a condition with polling
    /// </summary>
    public static async Task<bool> WaitForAsync(
        Func<Task<bool>> condition,
        int maxWaitMs = 5000,
        int pollIntervalMs = 100)
    {
        var elapsed = 0;
        while (elapsed < maxWaitMs)
        {
            if (await condition())
            {
                return true;
            }
            await Task.Delay(pollIntervalMs);
            elapsed += pollIntervalMs;
        }
        return false;
    }

    /// <summary>
    /// Captures a screenshot with automatic naming
    /// </summary>
    public static async Task<string> CaptureScreenshotAsync(
        IPage page,
        string testName,
        string outputPath)
    {
        var fileName = $"{testName}_{DateTime.UtcNow:yyyyMMdd_HHmmss}.png";
        var fullPath = Path.Combine(outputPath, fileName);
        
        Directory.CreateDirectory(outputPath);
        await page.ScreenshotAsync(fullPath);
        
        return fullPath;
    }

    /// <summary>
    /// Creates a test user context with common setup
    /// </summary>
    public static TestUserContext CreateTestUser(string? id = null)
    {
        return new TestUserContext
        {
            Id = id ?? Guid.NewGuid().ToString(),
            Email = $"test-{Guid.NewGuid()}@example.com",
            CreatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Generates test product data
    /// </summary>
    public static ProductTestData CreateTestProduct(string? name = null)
    {
        return new ProductTestData
        {
            Id = Guid.NewGuid().ToString(),
            Name = name ?? $"Test Product {Guid.NewGuid():N}",
            Description = "Test description",
            CreatedAt = DateTime.UtcNow
        };
    }
}

public class TestUserContext
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class ProductTestData
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}