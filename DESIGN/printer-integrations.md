# PrintHub - Printer Integration Strategy

## Phase

Printer integrations are Phase 3 work. Phase 1 must not depend on Bambu, OctoEverywhere, direct print submission, live printer status, or automatic print queues.

Phase 1 stops at preparation bundles: the user downloads the right files and prints manually.

## Adapter Strategy

When printer execution begins, all printer integrations should sit behind a common adapter interface.

```csharp
public interface IPrinterAdapter
{
    Task<IReadOnlyList<PrinterInfo>> GetPrintersAsync(CancellationToken ct);
    Task<PrinterStatus> GetStatusAsync(string printerId, CancellationToken ct);
    Task<string> QueueJobAsync(PreparedPrintJob job, CancellationToken ct);
    Task CancelJobAsync(string printerId, string externalJobId, CancellationToken ct);
}
```

The Phase 3 implementation should adapt preparation bundles into printer-specific jobs. It should not rewrite Phase 1 product/file/order preparation concepts.

## Candidate Adapters

| Printer Family | Candidate Adapter | Notes |
|----------------|-------------------|-------|
| Bambu P1S/X1C/A1 | Bambu Connect / cloud APIs | Cloud-native, viable from spike, requires careful auth and API stability review |
| OctoPrint/Klipper/Marlin | OctoEverywhere bridge | Useful for non-Bambu printers, requires user setup |
| LAN-only Bambu | Experimental | Not a committed product path until explicitly approved |

## Phase 3 UI Flow

```text
Settings -> Printers -> Add Printer
  -> Select adapter
  -> Connect account/device
  -> Verify reachable status
  -> Save printer

Preparation Bundle -> Send to Printer
  -> Select printer
  -> Confirm file/job details
  -> Submit
  -> Track live status
```

## Implementation Notes

- Printer adapters should live under `PrintHub.Infrastructure/Services/Printers/`.
- Adapter failures must not affect file preparation or downloads.
- A workspace should be able to use PrintHub without any connected printers.
- Keep printer credentials encrypted and auditable.

## Lock

- Do not add printer integration to the Phase 1 critical path.
- Do not require Bambu credentials for onboarding.
- Do not market LAN-only Bambu support until a dedicated spike and product review approve it.
