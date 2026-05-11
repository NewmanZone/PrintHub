using Microsoft.Extensions.Configuration;
using PrintHub.Tests.Integration;
using PrintHub.Tests.Smoke;
using PrintHub.Tests.Visual;
using Xunit;

namespace PrintHub.Tests.Helpers.Fixtures;

/// <summary>
/// Shared fixture for smoke tests - provides configured HttpClient and options
/// </summary>
public class SmokeTestFixture : IDisposable
{
    public SmokeTestOptions Options { get; }
    public HttpClient HttpClient { get; }

    public SmokeTestFixture()
    {
        Options = SmokeTestRunner.LoadFromConfiguration();
        HttpClient = new HttpClient
        {
            BaseAddress = new Uri(Options.ApiBaseUrl),
            Timeout = TimeSpan.FromSeconds(Options.TimeoutSeconds)
        };
    }

    public void Dispose()
    {
        HttpClient?.Dispose();
    }
}

/// <summary>
/// Shared fixture for visual checks - provides Playwright browser
/// </summary>
public class VisualCheckFixture : IDisposable
{
    public VisualCheckOptions Options { get; }
    public Microsoft.Playwright.IPlaywright? Playwright { get; }
    public Microsoft.Playwright.IBrowser? Browser { get; }

    public VisualCheckFixture()
    {
        Options = LoadOptions();
        
        if (Options.Enabled)
        {
            Playwright = Microsoft.Playwright.Playwright.Create();
            Browser = Playwright.Chromium.LaunchAsync(new Microsoft.Playwright.BrowserTypeLaunchOptions
            {
                Headless = true
            }).GetAwaiter().GetResult();
        }
    }

    private VisualCheckOptions LoadOptions()
    {
        var config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.Testing.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var options = new VisualCheckOptions();
        config.GetSection("VisualChecks").Bind(options);
        
        return options;
    }

    public void Dispose()
    {
        Browser?.CloseAsync().GetAwaiter().GetResult();
        Playwright?.Dispose();
    }
}

/// <summary>
/// Shared fixture for integration tests
/// </summary>
public class IntegrationTestFixture
{
    public IntegrationGateOptions Options { get; }
    public string TestOutputPath { get; }

    public IntegrationTestFixture()
    {
        Options = IntegrationGateRunner.LoadFromConfiguration();
        TestOutputPath = Path.Combine(Path.GetTempPath(), "PrintHubTests", Guid.NewGuid().ToString());
        
        if (!Directory.Exists(TestOutputPath))
        {
            Directory.CreateDirectory(TestOutputPath);
        }
    }
}