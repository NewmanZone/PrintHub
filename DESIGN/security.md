# PrintHub - Security Design

## Overview

PrintHub handles highly sensitive intellectual property: users' STL/3MF files represent unique designs, custom characters, and proprietary products. Security is not an afterthought—it is a core product requirement.

---

## Threat Model

| Threat | Impact | Mitigation |
|--------|--------|------------|
| STL files stolen/downloaded by unauthorized parties | IP theft, competitive harm | Signed URLs, no direct file access |
| Malicious file upload | Virus/malware via sliced files | File validation, sandboxed processing |
| Etsy account compromise | Unauthorized access to shop data | OAuth best practices, token encryption |
| Data breach | User data exposure | Encryption at rest, minimal data retention |
| AI training on user files | IP lost to competitors | Explicit no-AI-training policy, legal terms |

---

## File Security

### Upload Validation

```csharp
// Validate all uploaded files
- Extension allowlist: .stl, .3mf, .obj, .amf
- Magic bytes verification (not just extension)
- Max file size: 100MB
- Virus scan via Azure Defender or ClamAV
- Parse and validate mesh geometry (detect corrupt/malicious files)
```

### Storage

| Layer | Protection |
|-------|------------|
| Azure Blob Storage | Encryption at rest (AES-256) |
| Customer-managed keys | Optional BYOK for enterprise |
| Private endpoints | VNet integration for Azure Storage |
| No public blob URLs | Always use signed URLs |

### Signed URLs

```csharp
// Files served through time-limited SAS URLs
- Expiry: 15 minutes for download
- IP restriction where possible
- Single-use tokens for extra sensitive operations
- Thumbnail generation on upload, stored separately
```

### File Retention Policy

**Source STL/3MF files** — Retained by default. Users can delete or purge at any time via the UI/API.
**Generated/sliced files** — Short-lived by default; deleted after the print job completes.
**Personalized files** — Short-lived by default; deleted 24 hours after the order is fulfilled (configurable per shop).

Users can opt into longer retention for generated files via shop settings.

---

## Authentication & Authorization

### User Authentication

- **Azure Active Directory B2C** for identity
- Social login: Google, Apple (Etsy sellers use varied auth)
- MFA required for shops with >$1000/mo revenue

### Shop Connection (Etsy OAuth)

```
Scopes requested:
- listings_rw (read/write listings)
- transactions (read orders)
- profile (basic shop info)

Token storage:
- Encrypted at rest
- Refresh tokens rotated automatically
- Revocation on user request
```

### Authorization

```
User
  └── Shop (owning)
        ├── Products (CRUD on own shop only)
        ├── Parts
        ├── PrintJobs
        └── Settings

No cross-shop access. User A cannot see User B's data.
```

---

## Data Protection

### Encryption

| Data | At Rest | In Transit |
|------|---------|------------|
| Files (STL/3MF) | AES-256 (Azure Storage) | TLS 1.3 |
| Database | AES-256 (Cosmos DB) | TLS 1.3 |
| Tokens | AES-256 + hash for refresh | TLS 1.3 |
| User PII | AES-256 | TLS 1.3 |

### Token Management

```csharp
// Etsy tokens stored encrypted
public class EncryptedToken
{
    public string EncryptedValue { get; set; }  // AES encrypted
    public string IV { get; set; }              // Initialization vector
    public string Hash { get; set; }            // For validation without decrypt
    public DateTime ExpiresAt { get; set; }
}

// Never store raw tokens
// Key rotation: automated, quarterly
```

### Audit Logging

Every sensitive operation logged:
```json
{
  "timestamp": "2024-01-15T10:30:00Z",
  "userId": "user-123",
  "action": "FILE_DOWNLOAD",
  "fileId": "file-456",
  "ip": "203.0.113.42",
  "userAgent": "Mozilla/5.0...",
  "success": true
}
```

Users can download their own audit logs.

---

## Compute Security

### Slicing/Processing

- **Ephemeral containers** — files processed in isolated, stateless containers
- Containers spun up per-job, destroyed after completion
- No persistent storage access from processing containers
- Network isolation: processing VMs cannot reach other services

### Azure Configuration

```bash
# Recommended Azure security config
- Enable Azure Defender for Storage
- Enable Azure Defender for APIs
- Use Private Endpoints for Cosmos DB and Storage
- VNet integration for App Service/Container Apps
- Web Application Firewall (WAF) in front of API
- Rate limiting: 100 req/min per user
```

---

## Compliance & Privacy

### Policy Commitments

| Commitment | Description |
|------------|-------------|
| No AI Training | User files are never used to train AI/ML models |
| No Data Sharing | User data is never sold or shared with third parties |
| No Competitive Use | We don't use your designs for our own products |
| Deletion on Request | Users can request full data deletion within 30 days |
| Breach Notification | Users notified within 72 hours of confirmed breach |

### Legal

- **Terms of Service** — explicit IP ownership stays with user
- **Privacy Policy** — GDPR, CCPA compliant
- **Data Processing Agreement** — available for enterprise customers

### Future: SOC 2 Type II

Roadmap goal for Year 2:
- Annual audit by third party
- Continuous monitoring
- Incident response procedures
- Penetration testing

---

## Security Checklist

### Pre-Launch
- [ ] Azure Defender enabled on all services
- [ ] WAF configured with OWASP rules
- [ ] All secrets in Azure Key Vault
- [ ] TLS 1.3 enforced everywhere
- [ ] File validation tested
- [ ] Penetration test completed
- [ ] Privacy policy and ToS reviewed by lawyer

### Operational
- [ ] Quarterly key rotation
- [ ] Monthly dependency audits (Dependabot)
- [ ] Annual SOC 2 readiness assessment
- [ ] Incident response plan documented
- [ ] Backup restoration tested quarterly

---

## Reporting Security Issues

security@printhub.example.com — responsible disclosure policy