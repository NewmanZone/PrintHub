# Bambu Integration Spike — PrintHub

**Date:** 2026-05-10  
**Issue:** #4  
**Author:** Spike investigation  
**Status:** Completed  

---

## Executive Summary

Bambu Lab printers are **viable for PrintHub's MVP** via their cloud API at `https://api.bambulab.com`. The official Bambu Connect program requires developer registration, but community documentation provides sufficient detail for a working implementation.

**Recommendation:** Proceed with Bambu integration as specified in Issue #10 (Printer Adapter Contract). No technical blockers identified.

---

## 1. Bambu Ecosystem Overview

### Printer Models
| Model | Series | Cloud Support | Local Network |
|-------|--------|---------------|---------------|
| X1 Carbon | X1 | ✅ Full | ✅ AMS, FTP, MQTT |
| X1E | X1 | ✅ Full | ✅ |
| P1S | P1 | ✅ Full | ✅ Limited |
| P1P | P1 | ✅ Full | ✅ Limited |
| A1 | A1 | ✅ Full | ✅ Limited |
| A1 Mini | A1 | ✅ Full | ✅ Limited |

### Key Capabilities
- **Cloud-native** — printers phone home to Bambu's servers; no port forwarding needed
- **AMS (Auto Material System)** — multi-color printing support
- **Real-time monitoring** — progress, temperatures, video stream
- **Remote control** — start/stop/pause prints, change settings
- **File management** — upload, list, delete files on SD card
- **Task/tray management** — schedule and queue prints

---

## 2. API Landscape Analysis

### 2.1 Official Bambu Connect

Bambu Lab has an official "Bambu Connect" developer program for third-party integration.

**Documentation:** Not publicly accessible; requires registration.  
**Status:** Unknown availability and approval timeline.

### 2.2 Cloud HTTP API (Community Documented)

The `api.bambulab.com` endpoints are used by:
- Bambu Studio (desktop slicer)
- Bambu Handy (mobile app)
- Community projects

**Base URL:** `https://api.bambulab.com`

#### Authentication
- Email + password login → `accessToken` + `refreshToken`
- Tokens valid ~3 months
- All requests require `Authorization: Bearer {accessToken}` header

**Endpoint:** `POST /v1/user-service/user/login`
```json
{
  "account": "email@example.com",
  "password": "hashedpassword"
}
```

#### Key Endpoints for PrintHub

| Method | Path | Purpose |
|--------|------|---------|
| GET | `/v1/iot-service/user/devices` | List user's registered printers |
| GET | `/v1/iot-service/device/{dev_id}` | Get printer status |
| POST | `/v1/iot-service/file/upload` | Upload print file (3MF/STL) |
| POST | `/v1/iot-service/project/upload` | Upload sliced project |
| GET | `/v1/iot-service/project/list` | List user's cloud projects |
| POST | `/v1/iot-service/project/create` | Create new project |
| POST | `/v1/iot-service/project/slice` | Trigger slicing |
| GET | `/v1/iot-service/download/file` | Download files |
| GET | `/v1/user-service/my/profile` | Get user info (for MQTT credentials) |

### 2.3 MQTT Client (Real-Time Updates)

Bambu printers use MQTT for real-time status streaming.

**Broker:** `mqtt.bambulab.com` (port 8883)  
**Credentials:** Derives from user UID (prefix `u_`)  
**Use Cases:** Real-time print progress, temperature updates, AMS status

**Topic Pattern:**
- Device status: `device/{dev_id}/report`
- Upload requests: `device/{dev_id}/upload`

### 2.4 FTP Client

Printer SD card accessible via FTP at printer's IP address.

**Use Cases:** Direct file upload, gcode retrieval (when cloud API limits are hit)

### 2.5 Camera Integration

- **TTCode flow** — get auth token via `POST /v1/iot-service/device/get_thing_token`
- **Stream URL** — RTSP or HLS depending on firmware
- **Not MVP-critical** — can defer to later iterations

---

## 3. Technical Findings

### 3.1 Authentication Flow
1. User provides Bambu account credentials
2. PrintHub calls `/v1/user-service/user/login` 
3. Store `accessToken` + `refreshToken` (encrypted at rest)
4. Include `Authorization: Bearer {token}` on all API calls
5. Implement token refresh before expiry

### 3.2 Device Registration
- Devices are "bound" to user accounts via Bambu Connect
- PrintHub queries bound devices via `/v1/iot-service/user/devices`
- Device info includes: `dev_id`, `name`, `online` status, `dev_model_name`
- Access code (`dev_access_code`) may be needed for local operations

### 3.3 File Upload Flow
1. Upload 3MF/STL to cloud via `POST /v1/iot-service/file/upload`
2. Create/update project via `POST /v1/iot-service/project/create`
3. Assign file to printer task slot (tray)
4. Monitor via MQTT or polling

### 3.4 MQTT Credentials
- **Username:** `u_{uid}` (numeric user ID from user profile)
- **Password:** Session token from login
- **Topic subscribe:** `/device/{dev_id}/report/#`

### 3.5 Print Status Tracking
MQTT messages include:
- `print_status`: "SUCCESS", "FAILED", "FAIL", "RUNNING", "SLICE_WAIT", etc.
- `progress`: 0-100 percentage
- `gcode_state`: Current G-code state
- Temperatures: `bed_temp`, `nozzle_temp`
- Layer info: `cur_layer`, `total_layers`

---

## 4. MVP Scope Recommendation

### MVP (Issue #10, Wave 3)
**Achievable in one sprint:**
1. ✅ User enters Bambu credentials in PrintHub
2. ✅ PrintHub stores encrypted token
3. ✅ List bound printers from Bambu cloud
4. ✅ Show printer status (online/offline)
5. ✅ Send print job (3MF file) to printer
6. ✅ Track print progress via MQTT
7. ✅ Show basic status in UI: printing, progress %, ETA

**Not MVP:**
- Multi-color/AMS optimization
- Camera stream embedding
- Advanced slice settings
- Multiple file uploads per job
- Print history sync

### Phase 2 Additions
- Camera stream integration
- AMS tray management
- Filament usage tracking
- Print cost estimation from Bambu data

---

## 5. Risks & Mitigations

| Risk | Likelihood | Impact | Mitigation |
|------|------------|--------|------------|
| Bambu Connect API closes/restricts | Low | High | Use community documentation; monitor API stability |
| Token refresh failures | Medium | Medium | Implement robust retry with user re-auth prompt |
| MQTT connection instability | Medium | Low | Fallback to HTTP polling for status |
| Device offline during job | Medium | Medium | Queue job; notify user when printer comes online |
| API rate limits | Low | Low | Implement request throttling |

---

## 6. Architectural Implications

### Interface Contract (for Issue #10)

The spike validates that `IPrinterAdapter` should include:

```csharp
public interface IPrinterAdapter
{
    Task<IEnumerable<PrinterInfo>> GetPrintersAsync();
    Task<PrinterStatus> GetStatusAsync(string printerId);
    Task<bool> StartPrintAsync(string printerId, PrintJob job);
    Task<bool> StopPrintAsync(string printerId);
    Task<bool> PausePrintAsync(string printerId);
    Task UploadFileAsync(string printerId, Stream fileStream, string fileName);
    IObservable<PrintProgress> SubscribeProgress(string printerId);
}
```

### Bambu-Specific Implementation

`BambuPrinterAdapter` will:
1. Implement `IBambuCloudClient` for HTTP API calls
2. Implement `IBambuMqttClient` for real-time updates
3. Store access tokens in `BambuCredentials` (encrypted)
4. Map Bambu-specific status to canonical `PrinterStatus`

### Configuration Requirements

```json
{
  "Bambu": {
    "ApiBaseUrl": "https://api.bambulab.com",
    "MqttBroker": "mqtt.bambulab.com",
    "MqttPort": 8883,
    "ClientTimeoutSeconds": 30
  }
}
```

---

## 7. Recommendation

**Proceed with Bambu integration for Issue #10.** The API surface is well-documented by the community, the printer ecosystem supports cloud-native integration, and there are no technical blockers identified.

**MVP Scope for Issue #10:**
- Auth flow with encrypted token storage
- Printer discovery (list bound devices)
- Status polling with MQTT upgrade
- Basic print job dispatch (file upload + start)
- Progress tracking UI

**Defer to Phase 2:**
- Advanced AMS management
- Camera stream embedding
- Print history sync
- Multi-printer optimization

---

## 8. References

- [OpenBambuAPI Community Docs](https://github.com/Doridian/OpenBambuAPI)
- [coelacant1/Bambu-Lab-Cloud-API](https://github.com/coelacant1/Bambu-Lab-Cloud-API)
- [BambuLabs API Documentation](https://bambutools.github.io/bambulabs_api/)