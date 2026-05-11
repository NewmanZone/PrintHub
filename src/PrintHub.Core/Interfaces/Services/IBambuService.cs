namespace PrintHub.Core.Interfaces.Services;

/// <summary>
/// Service for Bambu Cloud API operations (authentication, device management).
/// </summary>
public interface IBambuService
{
    /// <summary>
    /// Authenticate with Bambu Cloud using email and password.
    /// </summary>
    Task<BambuAuthResult> AuthenticateAsync(string email, string password, CancellationToken cancellationToken = default);

    /// <summary>
    /// Refresh the access token using the refresh token.
    /// </summary>
    Task<BambuAuthResult> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all devices bound to the user's account.
    /// </summary>
    Task<IEnumerable<BambuDevice>> GetDevicesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get device information including current status.
    /// </summary>
    Task<BambuDevice?> GetDeviceAsync(string deviceId, CancellationToken cancellationToken = default);
}

public record BambuAuthResult(
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAt,
    string UserId,
    string Email);

public record BambuDevice(
    string DeviceId,
    string DeviceName,
    string ModelName,
    bool IsOnline,
    string? FirmwareVersion,
    DateTime? LastOnline);