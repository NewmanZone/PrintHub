using System.Text.Json;
using FluentAssertions;
using Microsoft.Extensions.Configuration;

namespace PrintHub.Tests.Smoke;

/// <summary>
/// Discovers and runs smoke tests based on configuration
/// </summary>
public class SmokeTestRunner
{
    private readonly SmokeTestOptions _options;
    private readonly List<SmokeTestResult> _results = new();
    private readonly string _outputPath;

    public SmokeTestRunner(SmokeTestOptions options, string? outputPath = null)
    {
        _options = options;
        _outputPath = outputPath ?? "./test-output/smoke-results.json";
    }

    public static SmokeTestOptions LoadFromConfiguration(IConfiguration? configuration = null)
    {
        var config = configuration ?? new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.Testing.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var options = new SmokeTestOptions();
        config.GetSection("PrintHub").Bind(options);
        config.GetSection("SmokeTests").Bind(options);

        return options;
    }

    public async Task<List<SmokeTestResult>> RunAllTestsAsync()
    {
        _results.Clear();

        if (!_options.Enabled)
        {
            Console.WriteLine("⚠️  Smoke tests are disabled in configuration");
            return _results;
        }

        Console.WriteLine($"🚀 Starting smoke tests against {_options.ApiBaseUrl}");
        Console.WriteLine($"⏱️  Timeout: {_options.TimeoutSeconds} seconds per endpoint");
        Console.WriteLine();

        foreach (var endpoint in _options.Endpoints)
        {
            var result = await RunEndpointTestAsync(endpoint);
            _results.Add(result);
            Console.WriteLine(result.ToString());
        }

        await SaveResultsAsync();

        return _results;
    }

    private async Task<SmokeTestResult> RunEndpointTestAsync(SmokeTestEndpoint endpoint)
    {
        var httpClient = new HttpClient
        {
            BaseAddress = new Uri(_options.ApiBaseUrl),
            Timeout = TimeSpan.FromSeconds(_options.TimeoutSeconds)
        };

        try
        {
            var request = new HttpRequestMessage(
                new HttpMethod(endpoint.Method),
                endpoint.Path);

            if (endpoint.Headers != null)
            {
                foreach (var header in endpoint.Headers)
                {
                    request.Headers.TryAddWithoutValidation(header.Key, header.Value);
                }
            }

            if (endpoint.Body != null && endpoint.Method != "GET")
            {
                request.Content = new StringContent(
                    endpoint.Body,
                    System.Text.Encoding.UTF8,
                    "application/json");
            }

            var response = await httpClient.SendAsync(request);
            var statusCode = (int)response.StatusCode;
            var success = statusCode == endpoint.ExpectedStatus;

            return new SmokeTestResult
            {
                TestName = endpoint.Name,
                Endpoint = $"{endpoint.Method} {endpoint.Path}",
                Success = success,
                StatusCode = statusCode,
                ErrorMessage = success
                    ? null
                    : $"Expected {endpoint.ExpectedStatus}, got {statusCode}",
                ResponseBody = await response.Content.ReadAsStringAsync(),
                ExecutedAt = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            return new SmokeTestResult
            {
                TestName = endpoint.Name,
                Endpoint = $"{endpoint.Method} {endpoint.Path}",
                Success = false,
                ErrorMessage = ex.Message,
                ExecutedAt = DateTime.UtcNow
            };
        }
    }

    public SmokeTestSummary GetSummary()
    {
        return new SmokeTestSummary
        {
            TotalTests = _results.Count,
            Passed = _results.Count(r => r.Success),
            Failed = _results.Count(r => !r.Success),
            Results = _results,
            ExecutedAt = DateTime.UtcNow,
            AllPassed = _results.All(r => r.Success)
        };
    }

    private async Task SaveResultsAsync()
    {
        var directory = Path.GetDirectoryName(_outputPath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var json = JsonSerializer.Serialize(GetSummary(), new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await File.WriteAllTextAsync(_outputPath, json);
        Console.WriteLine($"\n📄 Results saved to: {_outputPath}");
    }
}

public class SmokeTestSummary
{
    public int TotalTests { get; set; }
    public int Passed { get; set; }
    public int Failed { get; set; }
    public List<SmokeTestResult> Results { get; set; } = new();
    public DateTime ExecutedAt { get; set; }
    public bool AllPassed { get; set; }

    public double PassRate => TotalTests > 0 ? (double)Passed / TotalTests * 100 : 0;
}
