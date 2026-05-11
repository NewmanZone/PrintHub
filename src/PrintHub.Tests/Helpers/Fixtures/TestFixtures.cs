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
public class VisualCheckFixture : IAsyncDisposable
{
    public VisualCheckOptions Options { get; }
    public Microsoft.Playwright.IPlaywright? Playwright { get; private set; }
    public Microsoft.Playwright.IBrowser? Browser { get; private set; }

    public VisualCheckFixture()
    {
        Options = LoadOptions();
    }

    public async Task InitializeAsync()
    {
        if (Options.Enabled)
        {
            Playwright = await Microsoft.Playwright.Playwright.CreateAsync();
            Browser = await Playwright.Chromium.LaunchAsync(new Microsoft.Playwright.BrowserTypeLaunchOptions
            {
                Headless = true
            });
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

    public async ValueTask DisposeAsync()
    {
        if (Browser != null)
        {
            await Browser.CloseAsync();
        }
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