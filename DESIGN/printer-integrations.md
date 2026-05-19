# PrintHub - Printer Integration Strategy

## Overview

PrintHub supports multiple printer families through an **adapter-based implementation**. This document locks the strategy for Bambu, OctoEverywhere/OctoPrint, and future Bambu expansion.

## Adapter Pattern

All printer integrations implement a common interface:

```csharp
public interface IPrinterAdapter
{
    string AdapterName { get; }
    Task<PrinterStatus> GetStatusAsync(string printerId, CancellationToken ct);
    Task<string> QueueJobAsync(PrintJob job, CancellationToken ct);
    Task PauseJobAsync(string jobId, CancellationToken ct);
    Task ResumeJobAsync(string jobId, CancellationToken ct);
    Task CancelJobAsync(string jobId, CancellationToken ct);
}
```

Adapter registration is keyed by `PrinterType` at runtime.

## Supported Adapters

### 1. Bambu Connect (Primary)

- **Path:** Direct cloud-native integration via Bambu Lab Connect API
- **Printers:** P1S, X1C, A1, A1 Mini (and future Bambu models)
- **Setup:** User provides serial number + access code; PrintHub calls Bambu Cloud
- **Zero local infrastructure** — no Raspberry Pi, no VPN, no port forwarding
- **Capabilities:** Full status, start/stop/pause, camera snapshot, AMS support

### 2. OctoEverywhere / OctoPrint (Bridge)

- **Path:** OctoEverywhere cloud bridge to user's local OctoPrint instance
- **Printers:** Any printer running OctoPrint (Klipper via OctoPrint-Klipper plugin, Marlin, etc.)
- **Setup:** User installs OctoPrint + OctoEverywhere plugin; provides OctoEverywhere share URL
- **Capabilities:** Status polling, job start/stop, basic camera (via OctoEverywhere)
- **Limitations:** Depends on user's local network + OctoPrint uptime; slightly higher latency

### 3. Bambu Spike (Experimental)

- **Status:** Pre-MVP spike to validate LAN-only Bambu communication
- **Goal:** Determine if direct LAN control (without Bambu Cloud) is viable for enterprise/self-hosted scenarios
- **Commitment:** No MVP commitment. If the spike succeeds, it becomes Adapter 1b. If it fails, it stays in `experimental/` and does not block MVP.
- **Owner:** Marked as `experimental` in code; gated behind feature flag

## Adapter Selection Matrix

| Printer Family | Recommended Adapter | Setup Complexity | Cloud Dependency |
|----------------|---------------------|------------------|------------------|
| Bambu P1S/X1C/A1 | Bambu Connect | Low | Bambu Cloud |
| Klipper (Voron, Centauri) | OctoEverywhere | Medium | OctoEverywhere |
| Marlin / Ender | OctoEverywhere | Medium | OctoEverywhere |
| Bambu (LAN-only spike) | Bambu Spike | Low | None (experimental) |

## Registration Flow

```
User → Settings → Printers → Add Printer
  → Select Type (Bambu / OctoEverywhere)
  → If Bambu: enter Serial + Access Code
  → If OctoEverywhere: enter Share URL
  → PrintHub validates connection
  → Printer saved with adapter type persisted
```

## Implementation Notes

- Each adapter lives in `PrintHub.Infrastructure/Services/Printers/`
- Adapters are registered in DI via `IServiceCollection.AddPrinterAdapters()`
- Adapter failures are isolated: one printer's OctoEverywhere timeout does not affect Bambu printers
- Retry policy: 3 retries with exponential backoff for transient cloud errors

## Lock

- **Do not add a third primary adapter before MVP ships.**
- **Do not remove the Bambu Spike experimental gate without team review.**
- **Do not commit to Bambu LAN-only in marketing/docs until spike passes.**
