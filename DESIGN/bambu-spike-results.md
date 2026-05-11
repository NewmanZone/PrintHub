# Spike: Bambu Integration Feasibility and MVP Scope

> Issue: #4 — "Spike: validate Bambu integration feasibility and MVP scope"
> Date: 2026-05-10
> Author: OpenClaw Subagent

## Executive Summary

**Verdict: ✅ FEASIBLE for MVP with bounded scope.**

Bambu Lab integration is technically viable via two well-documented paths: the **Bambu Cloud HTTP API** (official, authenticated) and the **local MQTT API** (community-documented, works over LAN). Third-party libraries (`bambu-lab-cloud-api` on PyPI, `OpenBambuAPI`, `Bambu-Lab-Cloud-API` on GitHub) provide validated reference implementations. Bambu Lab has publicly stated they are building an SDK and working with integration partners, indicating the ecosystem is maturing.

**Key risk:** The Cloud API is reverse-engineered / unofficial. Bambu Lab has introduced authorization controls (Jan 2025 firmware) that restrict critical operations to authorized software. PrintHub should architect for rapid pivot if the official SDK changes contracts.

---

## Research Methods

1. **GitHub reverse-engineering repos** — `coelacant1/Bambu-Lab-Cloud-API`, `Doridian/OpenBambuAPI`
2. **PyPI library analysis** — `bambu-lab-cloud-api` v1.0.5 (AGPL-3.0, Python ≥3.9)
3. **Bambu Lab official communications** — Wiki third-party integration page, blog posts on authorization controls and ecosystem principles
4. **Cross-reference with PrintHub DESIGN docs** — `architecture.md`, `api-design.md`, `print-queue.md`, `data-model.md`, `dotnet-structure.md`

---

## 1. API Landscape

### 1.1 Bambu Cloud HTTP API

| Attribute | Detail |
|-----------|--------|
| **Base URL** | `https://api.bambulab.com` (global) / `https://api.bambulab.cn` (China) |
| **Auth** | OAuth2-style Bearer token (`accessToken` + `refreshToken`), 3-month expiry |
| **Login** | Email + password OR email + verification code (2FA) |
| **Rate Limits** | ~1000 req/hour authenticated; ~10 req/min per device for status |
| **Response Format** | JSON: `{ code, message, data }` where `code: 0` = success |
| **Error Codes** | `1001` Invalid Token, `1002` Expired, `1003` Device Not Found, `1004` Offline, `1005` Print Job Failed, `1006` File Upload Failed |

**Endpoints confirmed by community repos:**

- `POST /v1/user-service/user/login` — Obtain tokens
- `POST /v1/user-service/user/refresh` — Refresh access token
- `GET /v1/iot-service/api/user/bind` — List bound printers (devices)
- `GET /v1/iot-service/api/user/devices` — Device metadata
- `GET /v1/iot-service/api/user/prints` — Print history
- `POST /v1/iot-service/api/user/print` — Start a print job (cloud push)
- File upload via cloud or local FTP

**Reference:**
- `coelacant1/Bambu-Lab-Cloud-API` (87★, Python, actively maintained)
- `Doridian/OpenBambuAPI` (community endpoint reference)

### 1.2 MQTT API (Real-Time)

| Attribute | Detail |
|-----------|--------|
| **Cloud Broker** | `mqtt://us.mqtt.bambulab.com:8883` (TLS) |
| **LAN Broker** | `mqtt://{PRINTER_IP}:8883` (TLS) |
| **Cloud Auth** | Username `u_{USER_ID}`, Password = access token |
| **LAN Auth** | Username `bblp`, Password = LAN access code (device property) |
| **Message Format** | JSON payload with `sequence_id`, `command`, `result` |
| **Wildcards** | `#` subscription supported |

**Capabilities:**
- Real-time telemetry (temperatures, progress, state)
- Push G-code commands
- Start / pause / stop / resume print
- AMS (Automatic Material System) hub queries
- Camera: RTSP (X1) or JPEG frames (P1/A1)

**Reference:** `Doridian/OpenBambuAPI/mqtt.md`

### 1.3 Official SDK / Partner Program

Bambu Lab blog posts (Jan–Mar 2025) indicate:
- **Authorization Control System** rolling out (firmware 01.08.03.00+ for X1, P/A series later). Critical operations now require official authorization. Unauthorized third-party software is blocked from dangerous commands.
- **SDK is incomplete** — Bambu Lab explicitly states: *"SDK is not complete and desired stability of our API has yet to be attained."*
- **Integration partners** are being onboarded for a smooth migration to the new security framework.
- **Rootable firmware** (Firmware R) exists for X1, allowing custom firmware, but voids warranty.

**Implication for PrintHub:**
- Use **Cloud HTTP API + MQTT** as the primary integration path.
- Do **not** rely on local LAN control for MVP — it requires printer network exposure and is now restricted by firmware auth.
- Monitor Bambu Lab partner program for official SDK release; plan a migration layer.

---

## 2. Feasibility Assessment

### 2.1 Can PrintHub push a 3MF file to a Bambu printer?

| Path | Feasibility | Notes |
|------|-------------|-------|
| **Cloud file upload → cloud print start** | ✅ YES | Community repos demonstrate file upload to Bambu Cloud, then queueing a print job by referencing the uploaded file. |
| **Local FTP → LAN print start** | ⚠️ PARTIAL | Firmware auth controls now block unauthorized local control. Requires user to be on same network. Risky for SaaS. |
| **Via Bambu Studio / Handy bridge** | ❌ NO | Not automatable; requires user interaction. |

**MVP recommendation:** Implement **Cloud Push** only. User uploads 3MF to PrintHub → PrintHub uploads to Bambu Cloud via API → PrintHub calls `print` endpoint targeting the user's registered printer.

### 2.2 Can PrintHub monitor print progress?

| Path | Feasibility | Notes |
|------|-------------|-------|
| **MQTT (cloud broker)** | ✅ YES | Subscribe to device topics for real-time progress, temps, state changes. |
| **HTTP polling** | ✅ YES | Poll device status endpoint (~10 req/min limit). Simpler but less real-time. |

**MVP recommendation:** Start with **HTTP polling** for simplicity. Add **MQTT** as a v1.1 enhancement for real-time dashboards.

### 2.3 Can PrintHub consolidate parts across products and send one job?

The print-queue design (`DESIGN/print-queue.md`) already defines:
- `PrintQueueResolutionService` — consolidates shared parts (e.g., Generic Hook ×8)
- `PrintJob` — targets a specific printer
- `PrintJobItem` — individual parts within a job

**Feasibility:** ✅ YES, but with a caveat. Bambu Cloud expects a single 3MF or STL per print job. PrintHub must **pre-compose** the consolidated bed layout into a single 3MF file before pushing. This is a **gating feature** for the printer adapter (#10), not this spike.

### 2.4 Multi-printer support (Bambu farm)

Bambu Cloud API lists all bound devices per user (`GET /v1/iot-service/api/user/bind`). PrintHub can:
1. Fetch user's Bambu printers.
2. Let user select target printer per job.
3. Push job to selected printer.

**Feasibility:** ✅ YES. Bambu Cloud natively supports multiple printers under one account.

---

## 3. Risks and Mitigations

| Risk | Likelihood | Impact | Mitigation |
|------|------------|--------|------------|
| Cloud API changes or rate limits tighten | Medium | High | Abstract Bambu client behind `IBambuService` interface; make adapter swappable |
| Bambu Lab shuts down or restricts unofficial API access | Low | High | Architect adapter layer to support alternative paths (OctoEverywhere, Klipper, official SDK later) |
| Firmware auth blocks cloud print start for unauthorized apps | Medium | High | Apply for Bambu Lab integration partner program; monitor SDK announcements |
| 3MF composition (multi-part bed layout) is complex | Medium | Medium | Scope bed layout composition to post-MVP (#10). MVP can push one part per job. |
| Token refresh / 2FA UX friction | Medium | Medium | Store encrypted refresh tokens; prompt user for re-auth when tokens expire |
| AGPL-3.0 PyPI library license incompatibility | Low | Medium | Do NOT vendor the Python library. Build a clean-room C# implementation referencing public API docs only. |

---

## 4. MVP Scope Definition

### 4.1 What ships in MVP

| Feature | Priority | Implementation Issue |
|---------|----------|----------------------|
| Register Bambu Cloud account credentials (token-based) | P0 | #10 (Printer Adapter) |
| List user's Bambu printers | P0 | #10 |
| Push a single 3MF file to a selected Bambu printer | P0 | #10 |
| Poll print job status (pending / printing / success / failed) | P0 | #10 |
| Token refresh handling | P1 | #10 |
| Real-time MQTT progress streaming | P2 | Post-MVP |
| Multi-part consolidated bed layout | P2 | Post-MVP |
| AMS material selection per job | P2 | Post-MVP |

### 4.2 What does NOT ship in MVP

- Local LAN control (blocked by firmware auth, not cloud-native)
- Bambu Studio / Handy integration
- Automatic retry on print failure
- Printer farm load balancing (print to least-busy printer)
- Camera streaming in dashboard

### 4.3 Architecture Changes Needed

From `DESIGN/dotnet-structure.md`, the Bambu adapter fits cleanly into `PrintHub.Infrastructure`:

```
PrintHub.Infrastructure/
├── Services/
│   ├── Bambu/
│   │   ├── BambuCloudClient.cs      # HTTP API wrapper
│   │   ├── BambuMqttClient.cs       # MQTT subscriber/publisher (v1.1)
│   │   └── BambuPrinterAdapter.cs   # Implements IPrinterAdapter
```

New interfaces needed in `PrintHub.Core`:
- `IBambuService` — token management, printer list, job submission
- `IPrinterAdapter` — abstraction so Bambu can be swapped for OctoEverywhere/Klipper later

---

## 5. Proof-of-Concept Stubs

This spike includes **optional proof-of-concept C# stubs** in `src/` to validate:
1. Interface design compatibility with Core/Infrastructure layering.
2. Configuration options shape (`BambuOptions`).
3. Error handling patterns for Bambu-specific error codes.

These stubs are **not production code**. They compile conceptually against the planned .NET 8 structure but are intentionally minimal (no HTTP client wiring, no real auth flow).

**Files added:**
- `src/PrintHub.Core/Interfaces/Services/IBambuService.cs`
- `src/PrintHub.Core/Interfaces/Services/IPrinterAdapter.cs`
- `src/PrintHub.Infrastructure/Services/Printers/Bambu/BambuCloudClient.cs`
- `src/PrintHub.Infrastructure/Services/Printers/Bambu/BambuPrinterAdapter.cs`
- `src/PrintHub.Infrastructure/Configuration/BambuOptions.cs`

---

## 6. Recommendations

1. **Accept Bambu integration into MVP scope.** The technical path is validated, community tools exist, and the cloud-native model aligns with PrintHub's "zero local setup" goal.
2. **Bound the scope tightly.** Single-file cloud push + polling is enough for MVP. Do not attempt local LAN control or multi-part bed composition yet.
3. **Build behind an adapter interface.** `IPrinterAdapter` decouples Bambu from the queue engine so OctoEverywhere/Klipper can be added later without queue refactor.
4. **Monitor Bambu Lab SDK announcements.** Subscribe to their blog / partner program. Plan a migration sprint when the official SDK stabilizes.
5. **Apply for integration partner status.** Early partner access may unlock official API credentials and avoid future auth blocks.
6. **Document token security.** Bambu tokens are 3-month bearer tokens with broad device access. Store encrypted at rest (Azure Key Vault). Never log tokens.

---

## 7. References

| Source | URL | Role |
|--------|-----|------|
| Bambu-Lab-Cloud-API (PyPI) | https://pypi.org/project/bambu-lab-cloud-api/ | Validated Python implementation |
| coelacant1/Bambu-Lab-Cloud-API | https://github.com/coelacant1/Bambu-Lab-Cloud-API | Endpoint docs + MQTT reference |
| Doridian/OpenBambuAPI | https://github.com/Doridian/OpenBambuAPI | HTTP + MQTT protocol docs |
| Bambu Lab Wiki — Third-party Integration | https://wiki.bambulab.com/en/software/third-party-integration | Official stance |
| Bambu Lab Blog — Authorization Controls | https://blog.bambulab.com/firmware-update-introducing-new-authorization-control-system-2/ | Firmware auth changes |
| Bambu Lab Blog — Ecosystem Principles | https://blog.bambulab.com/custom-firmware-plan-and-our-principles-on-ecosystem/ | SDK maturity disclaimer |
| bambutools/bambulabs_api | https://bambutools.github.io/bambulabs_api/ | Alternative Python API docs |

---

*End of spike report. Closes #4.*
