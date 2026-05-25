# PrintHub - Security Design

## Overview

PrintHub handles sensitive intellectual property: users' STL/3MF files, product designs, custom text, and Etsy order data. Security is a core product requirement.

Phase 1 security is workspace-scoped. A user can access data only through an active workspace membership.

---

## Threat Model

| Threat | Impact | Mitigation |
|--------|--------|------------|
| Source files stolen by unauthorized parties | IP theft, competitive harm | Workspace auth, signed URLs, no public blob access |
| Contributor overreach | Accidental or malicious shop changes | Role-based permissions and audit logs |
| Malicious file upload | Malware or parser exploit | File allowlist, size limits, scanning, sandboxed processing |
| Etsy token compromise | Unauthorized shop/order access | OAuth best practices, token encryption, revocation |
| Data breach | User/order data exposure | Encryption at rest, least privilege, minimal secrets |
| AI training on user files | Loss of user IP control | Explicit no-AI-training policy |

---

## Authentication

- OAuth-only.
- Azure AD B2C or equivalent provider.
- No PrintHub password registration, password login, password reset, or password hash storage.
- API receives and validates JWT bearer tokens.
- User profile is bootstrapped on first valid sign-in.

---

## Authorization

```text
User
  `-- WorkspaceMember
        `-- Workspace
              |-- Etsy Shop Connection
              |-- Products
              |-- Parts
              |-- Files
              |-- Orders
              |-- Preparation Bundles
              `-- Settings
```

Every protected API request must:

1. Validate the bearer token.
2. Resolve the current user.
3. Confirm active membership in `workspaceId`.
4. Check role permission for the requested action.

### Roles

| Role | Permissions |
|------|-------------|
| Owner | Manage workspace, Etsy connection, members, products, files, orders, bundles, and purge operations |
| Contributor | Manage products, files, order preparation, and downloads |
| Viewer | Read-only access if enabled |

Contributor restrictions in Phase 1:

- Cannot disconnect Etsy.
- Cannot invite/remove members.
- Cannot purge source files unless explicitly elevated later.
- Cannot delete the workspace.

---

## Etsy Connection

Requested scopes should be the minimum required for listing/order import and future status updates.

Token storage:

- Encrypt access and refresh tokens at rest.
- Rotate refresh tokens when Etsy supports it.
- Revoke or delete tokens on shop disconnect.
- Log connection, refresh failure, and disconnect events.

---

## File Security

### Upload Validation

```text
- Extension allowlist: .stl and .3mf for Phase 1
- Magic bytes or structured validation where practical
- Max file size: 100MB initially
- Virus scan through Azure Defender or equivalent
- Store file hash for integrity and duplicate detection
```

### Storage

| Layer | Protection |
|-------|------------|
| Azure Blob Storage | Encryption at rest |
| No public blob URLs | Files served by authenticated API stream or signed URL |
| Private endpoints | Preferred for production storage access |
| Thumbnails | Stored separately from source files |

### File Retention

- Source STL/3MF files are retained by default.
- Users can soft delete files.
- Owners can purge files after an explicit confirmation flow.
- Generated bundle archives are short-lived by default.
- Personalized generated files should be deleted after fulfillment unless the user opts into retention.

---

## Data Protection

| Data | At Rest | In Transit |
|------|---------|------------|
| Source files | Azure Storage encryption | TLS |
| Database records | Cosmos DB encryption | TLS |
| Etsy tokens | Application encryption plus platform encryption | TLS |
| User/order PII | Platform encryption | TLS |

---

## Audit Logging

Log sensitive operations:

```json
{
  "timestamp": "2026-05-20T10:30:00Z",
  "workspaceId": "wks_123",
  "userId": "usr_123",
  "action": "FILE_DOWNLOAD",
  "entityType": "PrintFileVersion",
  "entityId": "ver_456",
  "success": true
}
```

Audit events required for Phase 1:

- Etsy connected/disconnected.
- Member invited, accepted, role changed, removed.
- File uploaded, current version changed, deleted, purged, downloaded.
- Preparation bundle generated, downloaded, marked printed.
- Permission-denied attempts for sensitive actions.

---

## Compute Security

Phase 1 may not need slicing or automated file modification. If generated personalization is introduced:

- Run processing in isolated workers or containers.
- Avoid persistent local storage.
- Delete temporary files after processing.
- Do not let processing workers access unrelated workspace data.

---

## Compliance And Privacy

| Commitment | Description |
|------------|-------------|
| No AI Training | User files are never used to train AI/ML models |
| No Data Sharing | User data is never sold or shared with third parties |
| No Competitive Use | PrintHub does not use customer designs for its own products |
| Deletion on Request | Users can request full data deletion within 30 days |
| Breach Notification | Users notified within 72 hours of confirmed breach |

---

## Security Checklist

### Pre-Launch

- [ ] All protected endpoints enforce workspace membership.
- [ ] Role checks covered by unit/integration tests.
- [ ] Etsy tokens encrypted at rest.
- [ ] Blob containers are private.
- [ ] File upload validation tested.
- [ ] File download authorization tested.
- [ ] Audit logging implemented for sensitive operations.
- [ ] Privacy policy and terms reviewed before public launch.

### Operational

- [ ] Quarterly key rotation.
- [ ] Dependency audits enabled.
- [ ] Backup restoration tested.
- [ ] Incident response plan documented.

---

## Reporting Security Issues

Use a private responsible-disclosure channel before public launch. Public contact details can be added once the product has a production domain.
