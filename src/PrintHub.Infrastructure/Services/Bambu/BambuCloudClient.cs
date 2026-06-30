using Microsoft.Extensions.Logging;
using PrintHub.Core.Configuration;
using PrintHub.Core.Interfaces.Services;

namespace PrintHub.Infrastructure.Services.Bambu;

/// <summary>
/// POC stub for Bambu Cloud API client.
/// Full HTTP client wiring, auth flow, and error handling
/// belongs to Issue #10 (Printer Adapter Contract).
/// </summary>
public class BambuCloudClient : IBambuService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<BambuCloudClient> _logger;
    private readonly BambuOptions _options;

    public BambuCloudClient(HttpClient httpClient, ILogger<BambuCloudClient> logger, BambuOptions options)
    {
        _httpClient = httpClient;
        _logger = logger;
        _options = options;
    }

    public Task<BambuAuthResult> AuthenticateAsync(string email, string password, CancellationToken cancellationToken = default)
    {
        // TODO: Implement in Issue #10
        // - POST /v1/user-service/user/login
        // - Hash password according to Bambu requirements
        // - Store tokens securely
        throw new NotImplementedException("Bambu authentication will be implemented in Issue #10");
    }

    public Task<BambuAuthResult> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        // TODO: Implement in Issue #10
        // - POST /v1/user-service/user/refreshToken
        // - Handle token expiration gracefully
        throw new NotImplementedException("Token refresh will be implemented in Issue #10");
    }

    public Task<IEnumerable<BambuDevice>> GetDevicesAsync(CancellationToken cancellationToken = default)
    {
        // TODO: Implement in Issue #10
        // - GET /v1/iot-service/user/devices
        // - Map response to BambuDevice records
        throw new NotImplementedException("Device discovery will be implemented in Issue #10");
    }

    public Task<BambuDevice?> GetDeviceAsync(string deviceId, CancellationToken cancellationToken = default)
    {
        // TODO: Implement in Issue #10
        // - GET /v1/iot-service/device/{dev_id}
        // - Return null if device not found
        throw new NotImplementedException("Device status will be implemented in Issue #10");
    }
}