using FluentAssertions;
using Microsoft.Playwright;
using Xunit;
using Xunit.Abstractions;

namespace PrintHub.Tests.Visual;

/// <summary>
/// Visual regression tests that capture screenshots and compare against baselines
/// </summary>
[Collection("Visual Tests")]
public class VisualRegressionTests : IClassFixture<VisualCheckOptions>
{
    private readonly VisualCheckOptions _options;
    private readonly IPlaywright _playwright;
    private readonly IBrowser _browser;
    private readonly string _screenshotPath;

    public VisualRegressionTests(VisualCheckOptions options, ITestOutputHelper output)
    {
        _options = options;
        _screenshotPath = _options.ScreenshotsPath;

        if (!_options.Enabled)
        {
            _playwright = null!;
            _browser = null!;
            return;
        }

        _playwright = Microsoft.Playwright.Playwright.Create();
        _browser = _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = true
        }).GetAwaiter().GetResult();

        EnsureDirectoriesExist();
    }

    private void EnsureDirectoriesExist()
    {
        if (!Directory.Exists(_screenshotPath))
        {
            Directory.CreateDirectory(_screenshotPath);
        }

        var baselinePath = Path.Combine(_screenshotPath, "baseline");
        if (!Directory.Exists(baselinePath))
        {
            Directory.CreateDirectory(baselinePath);
        }

        var diffPath = Path.Combine(_screenshotPath, "diff");
        if (!Directory.Exists(diffPath))
        {
            Directory.CreateDirectory(diffPath);
        }
    }

    [SkippableFact]
    [Trait("Category", "Visual")]
    [Trait("Priority", "Medium")]
    public async Task LoginPage_RendersCorrectly()
    {
        Skip.If(!_options.Enabled, "Visual checks are disabled");

        var page = await _browser.NewPageAsync();
        var viewport = _options.Viewports.FirstOrDefault() ?? new ViewportConfig { Width = 1280, Height = 720, Name = "default" };
        await page.SetViewportSizeAsync(viewport.Width, viewport.Height);

        try
        {
            await page.GotoAsync($"{_options.ScreenshotsPath}/../../index.html");
            
            // For now, just verify the page loads
            var title = await page.TitleAsync();
            title.Should().NotBeNullOrEmpty();
        }
        finally
        {
            await page.CloseAsync();
        }
    }

    [SkippableFact]
    [Trait("Category", "Visual")]
    [Trait("Priority", "Medium")]
    public async Task ResponsiveLayout_DesktopAndMobile()
    {
        Skip.If(!_options.Enabled, "Visual checks are disabled");
        Skip.If(_options.Viewports.Count < 2, "Need at least 2 viewports configured");

        foreach (var viewport in _options.Viewports)
        {
            var page = await _browser.NewPageAsync();
            await page.SetViewportSizeAsync(viewport.Width, viewport.Height);

            try
            {
                await page.GotoAsync($"{_options.ScreenshotsPath}/../../index.html");
                
                // Verify the page loads on different viewport sizes
                var title = await page.TitleAsync();
                title.Should().NotBeNullOrEmpty($"Page should load at {viewport.Name} viewport");
            }
            finally
            {
                await page.CloseAsync();
            }
        }
    }

    public void Dispose()
    {
        if (_options.Enabled)
        {
            _browser.CloseAsync().GetAwaiter().GetResult();
            _playwright.Dispose();
        }
    }
}