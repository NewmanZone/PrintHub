using PrintHub.Core.Interfaces.Services;

namespace PrintHub.Infrastructure.Services.Printers.Bambu;

/// <summary>
/// Adapts Bambu Cloud API to the generic IPrinterAdapter interface.
/// Proof-of-concept stub — validates that the adapter pattern works
/// for queue integration and that status mapping is complete.
/// 
/// Full implementation (DI wiring, real HTTP calls, file staging)
/// belongs to issue #10.
/// </summary>
public class BambuPrinterAdapter : IPrinterAdapter
{
    private readonly BambuCloudClient _cloudClient;

    public string Provider => "bambu";

    public BambuPrinterAdapter(BambuCloudClient cloudClient)
    {
        _cloudClient = cloudClient ?? throw new ArgumentNullException(nameof(cloudClient));
    }

    public async Task<bool> IsOnlineAsync(string printerId, CancellationToken ct = default)
    {
        // Stub: in real implementation, call ListDevicesAsync or GetDeviceAsync
        // and check the IsOnline flag.
        throw new NotImplementedException("Implemented in #10");
    }

    public async Task<string> StartPrintAsync(
        string printerId,
        string filePath,
        string? fileName = null,
        CancellationToken ct = default)
    {
        // Stub workflow for MVP:
        // 1. Ensure authenticated (get access token).
        // 2. Upload file to Bambu Cloud → cloudFileId.
        // 3. Start print job referencing cloudFileId + printerId.
        // 4. Return Bambu job identifier.
        throw new NotImplementedException("Implemented in #10");
    }

    public async Task<PrintJobStatus> GetJobStatusAsync(
        string printerId,
        string providerJobId,
        CancellationToken ct = default)
    {
        // Stub: map BambuJobStatusDto.State to normalized PrintJobStatus.
        throw new NotImplementedException("Implemented in #10");
    }

    public async Task<bool> CancelJobAsync(
        string printerId,
        string providerJobId,
        CancellationToken ct = default)
    {
        // Stub: call cloud cancel endpoint.
        throw new NotImplementedException("Implemented in #10");
    }

    // ------------------------------------------------------------------
    // Status Mapping Helper
    // ------------------------------------------------------------------

    /// <summary>
    /// Maps Bambu Cloud job state strings to the normalized PrintJobStatus enum.
    /// </summary>
    protected static PrintJobStatus MapStatus(string? bambuState)
    {
        if (string.IsNullOrWhiteSpace(bambuState))
            return PrintJobStatus.Unknown;

        return bambuState.ToUpperInvariant() switch
        {
            "QUEUED" or "PENDING" => PrintJobStatus.Pending,
            "RUNNING" or "PRINTING" => PrintJobStatus.Printing,
            "PAUSED" => PrintJobStatus.Paused,
            "FINISH" or "SUCCESS" or "COMPLETED" => PrintJobStatus.Success,
            "FAIL" or "FAILED" or "ERROR" => PrintJobStatus.Failed,
            "CANCEL" or "CANCELLED" or "ABORTED" => PrintJobStatus.Cancelled,
            _ => PrintJobStatus.Unknown
        };
    }
}
