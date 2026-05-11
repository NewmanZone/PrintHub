namespace PrintHub.Core.Interfaces.Services;

/// <summary>
/// Bambu Cloud-specific operations. Higher-level than IPrinterAdapter;
/// manages auth, printer discovery, and cloud file lifecycle.
/// </summary>
public interface IBambuService
{
    /// <summary>
    /// Obtain (or refresh) a Bambu Cloud access token from stored credentials.
    /// </summary>
    Task<BambuTokenResult> AuthenticateAsync(CancellationToken ct = default);

    /// <summary>
    /// List all printers bound to the user's Bambu Cloud account.
    /// </summary>
    Task<IReadOnlyList<BambuPrinter>> ListPrintersAsync(CancellationToken ct = default);

    /// <summary>
    /// Upload a 3MF/STL file to Bambu Cloud storage so it can be referenced by a print job.
    /// Returns a cloud file identifier.
    /// </summary>
    Task<string> UploadFileAsync(
        string filePath,
        string? fileName = null,
        CancellationToken ct = default);

    /// <summary>
    /// Start a print job on a specific printer using a previously-uploaded cloud file.
    /// Returns the Bambu Cloud job identifier.
    /// </summary>
    Task<string> StartPrintJobAsync(
        string printerId,
        string cloudFileId,
        CancellationToken ct = default);

    /// <summary>
    /// Poll the status of a cloud print job.
    /// </summary>
    Task<BambuJobStatus> GetJobStatusAsync(
        string printerId,
        string jobId,
        CancellationToken ct = default);

    /// <summary>
    /// Cancel a running or pending cloud print job.
    /// </summary>
    Task<bool> CancelJobAsync(
        string printerId,
        string jobId,
        CancellationToken ct = default);
}

public record BambuTokenResult(string AccessToken, string RefreshToken, long ExpiresInSeconds);

public record BambuPrinter(
    string Id,
    string Name,
    string Model,
    string? SerialNumber,
    bool IsOnline,
    string? AccessCode);

public enum BambuJobStatus
{
    Unknown,
    Queued,
    Running,
    Paused,
    Finished,
    Failed,
    Cancelled
}
