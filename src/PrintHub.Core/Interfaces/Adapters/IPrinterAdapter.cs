namespace PrintHub.Core.Interfaces.Adapters;

/// <summary>
/// Abstracted printer adapter for managing printer operations.
/// Supports multiple printer types via adapter implementations.
/// </summary>
public interface IPrinterAdapter
{
    /// <summary>
    /// Gets the printer type this adapter handles (e.g., "bambu", "octoprint").
    /// </summary>
    string PrinterType { get; }

    /// <summary>
    /// Discovers available printers on the network or cloud account.
    /// </summary>
    Task<IEnumerable<PrinterInfo>> DiscoverPrintersAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the current status of a specific printer.
    /// </summary>
    Task<PrinterStatus> GetStatusAsync(string printerId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Starts a print job on the specified printer.
    /// </summary>
    Task<bool> StartPrintAsync(string printerId, PrintJob job, CancellationToken cancellationToken = default);

    /// <summary>
    /// Stops the current print job on the specified printer.
    /// </summary>
    Task<bool> StopPrintAsync(string printerId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Pauses the current print job on the specified printer.
    /// </summary>
    Task<bool> PausePrintAsync(string printerId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Resumes a paused print job on the specified printer.
    /// </summary>
    Task<bool> ResumePrintAsync(string printerId, CancellationToken cancellationToken = default);
}

public record PrinterInfo(
    string PrinterId,
    string Name,
    string Model,
    bool IsOnline,
    string FirmwareVersion);

public record PrintJob(
    string JobId,
    string FileName,
    byte[] FileContent,
    string PrintMode = "normal",
    int? BedTemp = null,
    int? NozzleTemp = null);

public record PrinterStatus(
    string PrinterId,
    PrintState State,
    double Progress,
    int CurrentLayer,
    int TotalLayers,
    double? BedTempCelsius,
    double? NozzleTempCelsius,
    string? CurrentFileName,
    DateTime? EstimatedCompletion,
    string? ErrorMessage);

public enum PrintState
{
    Idle,
    Printing,
    Paused,
    Completed,
    Failed,
    Offline,
    Unknown
}