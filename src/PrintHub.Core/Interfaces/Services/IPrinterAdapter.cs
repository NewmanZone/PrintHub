namespace PrintHub.Core.Interfaces.Services;

/// <summary>
/// Abstraction over printer control so Bambu, OctoAnywhere, or Klipper
/// can be swapped without changing queue / job logic.
/// </summary>
public interface IPrinterAdapter
{
    string Provider { get; }

    /// <summary>
    /// Returns true if the adapter can reach the printer (online check).
    /// </summary>
    Task<bool> IsOnlineAsync(string printerId, CancellationToken ct = default);

    /// <summary>
    /// Push a single 3MF / STL file to the printer and start printing.
    /// Returns a provider-specific job identifier.
    /// </summary>
    Task<string> StartPrintAsync(
        string printerId,
        string filePath,
        string? fileName = null,
        CancellationToken ct = default);

    /// <summary>
    /// Poll the printer for current job status.
    /// </summary>
    Task<PrintJobStatus> GetJobStatusAsync(
        string printerId,
        string providerJobId,
        CancellationToken ct = default);

    /// <summary>
    /// Cancel the current or specified job.
    /// </summary>
    Task<bool> CancelJobAsync(
        string printerId,
        string providerJobId,
        CancellationToken ct = default);
}

/// <summary>
/// Normalized job status across all printer providers.
/// </summary>
public enum PrintJobStatus
{
    Unknown,
    Pending,
    Printing,
    Paused,
    Success,
    Failed,
    Cancelled
}
