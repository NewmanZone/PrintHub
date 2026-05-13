using System.Net;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using PrintHub.Tests.Smoke;
using Xunit;

namespace PrintHub.Tests.Smoke;

/// <summary>
/// Base class for smoke tests that verify API endpoints respond correctly
/// </summary>
[Trait("Category", "Smoke")]
public abstract class SmokeTestBase
{
    protected readonly SmokeTestOptions Options;
    protected readonly HttpClient HttpClient;

    protected SmokeTestBase(SmokeTestOptions options)
    {
        Options = options;
        HttpClient = new HttpClient
        {
            BaseAddress = new Uri(options.ApiBaseUrl),
            Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds)
        };
    }

    protected async Task<SmokeTestResult> TestEndpointAsync(
        string name,
        string path,
        HttpMethod method,
        int expectedStatusCode,
        string? body = null,
        Dictionary<string, string>? headers = null)
    {
        var result = new SmokeTestResult
        {
            TestName = name,
            Endpoint = $"{method} {path}",
            ExecutedAt = DateTime.UtcNow
        };

        try
        {
            var request = new HttpRequestMessage(method, path);

            if (headers != null)
            {
                foreach (var header in headers)
                {
                    request.Headers.TryAddWithoutValidation(header.Key, header.Value);
                }
            }

            if (body != null && method != HttpMethod.Get)
            {
                request.Content = new StringContent(body, Encoding.UTF8, "application/json");
            }

            var response = await HttpClient.SendAsync(request);
            result.StatusCode = (int)response.StatusCode;
            result.Success = result.StatusCode == expectedStatusCode;

            if (!result.Success)
            {
                result.ErrorMessage = $"Expected status {expectedStatusCode}, got {result.StatusCode}";
            }

            result.ResponseBody = await response.Content.ReadAsStringAsync();
        }
        catch (HttpRequestException ex)
        {
            result.Success = false;
            result.ErrorMessage = $"HTTP request failed: {ex.Message}";
        }
        catch (TaskCanceledException)
        {
            result.Success = false;
            result.ErrorMessage = $"Request timed out after {Options.TimeoutSeconds} seconds";
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.ErrorMessage = $"Unexpected error: {ex.Message}";
        }

        return result;
    }

    protected async Task<SmokeTestResult> TestHealthEndpointAsync()
    {
        return await TestEndpointAsync("Health Check", "/health", HttpMethod.Get, 200);
    }

    protected async Task<SmokeTestResult> TestUnauthenticatedEndpointAsync(
        string path,
        int expectedStatus)
    {
        return await TestEndpointAsync(
            $"Unauthenticated Access to {path}",
            path,
            HttpMethod.Get,
            expectedStatus);
    }
}

public class SmokeTestResult
{
    public string TestName { get; set; } = string.Empty;
    public string Endpoint { get; set; } = string.Empty;
    public bool Success { get; set; }
    public int StatusCode { get; set; }
    public string? ErrorMessage { get; set; }
    public string? ResponseBody { get; set; }
    public DateTime ExecutedAt { get; set; }

    public override string ToString()
    {
        var status = Success ? "✅ PASS" : "❌ FAIL";
        return $"{status} [{StatusCode}] {TestName} - {Endpoint}" +
               (ErrorMessage != null ? $"\n   Error: {ErrorMessage}" : string.Empty);
    }
}
