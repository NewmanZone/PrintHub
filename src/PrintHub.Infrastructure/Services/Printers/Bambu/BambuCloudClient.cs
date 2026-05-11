using PrintHub.Infrastructure.Configuration;

namespace PrintHub.Infrastructure.Services.Printers.Bambu;

/// <summary>
/// Low-level HTTP client for the Bambu Cloud API.
/// Proof-of-concept stub — validates the configuration shape,
/// error code handling patterns, and request/response contracts.
/// 
/// Full implementation (HttpClient wiring, auth interceptors,
/// file upload streams, retry policies) belongs to issue #10.
/// </summary>
public class BambuCloudClient
{
    private readonly BambuOptions _options;

    public BambuCloudClient(BambuOptions options)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
    }

    // ------------------------------------------------------------------
    // Authentication
    // ------------------------------------------------------------------

    /// <summary>
    /// POST /v1/user-service/user/login
    /// Body: { account, password, code }
    /// Response: { accessToken, refreshToken, expiresIn, loginType, ... }
    /// </summary>
    public virtual Task<BambuLoginResponse> LoginAsync(
        string account,
        string password,
        string? verificationCode = null,
        CancellationToken ct = default)
    {
        // Stub: validates the contract only.
        throw new NotImplementedException("Login flow implemented in #10");
    }

    /// <summary>
    /// POST /v1/user-service/user/refresh
    /// Body: { refreshToken }
    /// Response: { accessToken, refreshToken, expiresIn }
    /// </summary>
    public virtual Task<BambuLoginResponse> RefreshTokenAsync(
        string refreshToken,
        CancellationToken ct = default)
    {
        throw new NotImplementedException("Token refresh implemented in #10");
    }

    // ------------------------------------------------------------------
    // Devices / Printers
    // ------------------------------------------------------------------

    /// <summary>
    /// GET /v1/iot-service/api/user/bind
    /// Returns the list of printers bound to the authenticated user.
    /// </summary>
    public virtual Task<IReadOnlyList<BambuDeviceDto>> ListDevicesAsync(
        string accessToken,
        CancellationToken ct = default)
    {
        throw new NotImplementedException("Device listing implemented in #10");
    }

    /// <summary>
    /// GET /v1/iot-service/api/user/devices
    /// Returns device metadata (model, serial, status, etc.).
    /// </summary>
    public virtual Task<BambuDeviceDto> GetDeviceAsync(
        string accessToken,
        string deviceId,
        CancellationToken ct = default)
    {
        throw new NotImplementedException("Device detail implemented in #10");
    }

    // ------------------------------------------------------------------
    // Print Jobs
    // ------------------------------------------------------------------

    /// <summary>
    /// POST /v1/iot-service/api/user/print
    /// Start a print job on a device using a cloud-file reference.
    /// </summary>
    public virtual Task<string> StartPrintAsync(
        string accessToken,
        string deviceId,
        string cloudFileId,
        CancellationToken ct = default)
    {
        throw new NotImplementedException("Print start implemented in #10");
    }

    /// <summary>
    /// Poll print job status. Bambu Cloud does not expose a single
    /// "job status" endpoint directly; status comes from device state
    /// or print history. Stub models the eventual shape.
    /// </summary>
    public virtual Task<BambuJobStatusDto> GetPrintStatusAsync(
        string accessToken,
        string deviceId,
        string jobId,
        CancellationToken ct = default)
    {
        throw new NotImplementedException("Status polling implemented in #10");
    }

    /// <summary>
    /// Cancel a running print job via cloud command.
    /// </summary>
    public virtual Task<bool> CancelPrintAsync(
        string accessToken,
        string deviceId,
        string jobId,
        CancellationToken ct = default)
    {
        throw new NotImplementedException("Cancel implemented in #10");
    }

    // ------------------------------------------------------------------
    // File Upload
    // ------------------------------------------------------------------

    /// <summary>
    /// Upload a 3MF or STL file to Bambu Cloud storage.
    /// Returns a cloud file identifier that can be passed to StartPrintAsync.
    /// </summary>
    public virtual Task<string> UploadFileAsync(
        string accessToken,
        Stream fileStream,
        string fileName,
        CancellationToken ct = default)
    {
        throw new NotImplementedException("File upload implemented in #10");
    }

    // ------------------------------------------------------------------
    // Error Handling Helpers
    // ------------------------------------------------------------------

    /// <summary>
    /// Map Bambu Cloud error codes to typed exceptions so the adapter
    /// can decide whether to retry, re-auth, or surface to the user.
    /// </summary>
    protected virtual Exception MapErrorCode(int code, string? message)
    {
        return code switch
        {
            1001 => new BambuAuthException($"Invalid token: {message}"),
            1002 => new BambuAuthException($"Token expired: {message}"),
            1003 => new BambuDeviceException($"Device not found: {message}"),
            1004 => new BambuDeviceException($"Device offline: {message}"),
            1005 => new BambuPrintException($"Print job failed: {message}"),
            1006 => new BambuPrintException($"File upload failed: {message}"),
            429 => new BambuRateLimitException($"Rate limited: {message}"),
            _ => new BambuApiException($"Bambu API error {code}: {message}")
        };
    }
}

// ------------------------------------------------------------------
// DTOs (data-transfer shapes validated by community API docs)
// ------------------------------------------------------------------

public record BambuLoginResponse(
    string AccessToken,
    string RefreshToken,
    long ExpiresIn,
    string? LoginType);

public record BambuDeviceDto(
    string Id,
    string Name,
    string Model,
    string? SerialNumber,
    bool IsOnline,
    string? AccessCode,
    string? Status);

public record BambuJobStatusDto(
    string JobId,
    string State, // "QUEUED", "RUNNING", "PAUSED", "FINISH", "FAIL", "CANCEL"
    int ProgressPercent,
    int? RemainingMinutes,
    string? FailReason);

// ------------------------------------------------------------------
// Typed Exceptions
// ------------------------------------------------------------------

public class BambuApiException : Exception
{
    public BambuApiException(string message) : base(message) { }
}

public class BambuAuthException : BambuApiException
{
    public BambuAuthException(string message) : base(message) { }
}

public class BambuDeviceException : BambuApiException
{
    public BambuDeviceException(string message) : base(message) { }
}

public class BambuPrintException : BambuApiException
{
    public BambuPrintException(string message) : base(message) { }
}

public class BambuRateLimitException : BambuApiException
{
    public BambuRateLimitException(string message) : base(message) { }
}
