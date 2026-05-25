# PrintHub - Architecture Overview

## Overview

PrintHub is a cloud-hosted SaaS platform for small Etsy-based 3D print shops. Phase 1 helps a shop owner and trusted contributors keep products, source files, and incoming orders in one shared workspace, then prepare downloadable file bundles for manual printing.

Printer execution is intentionally a later phase.

## Goals

- Shared Etsy production workspace for a shop owner and contributors.
- OAuth-only sign-in with workspace-scoped authorization.
- Etsy listing and order sync.
- Product-to-part-to-file mapping with versioned STL/3MF uploads.
- Order-to-file preparation so users can download the right bundle for each order.
- Clear handling of personalization and manual customization.
- Source file retention by default with user-controlled deletion/purge.
- Later printer execution through Bambu or OctoEverywhere adapters after Phase 1.

---

## High-Level Architecture

```text
                             PrintHub Azure

  +------------------------------------------------------------------+
  |                       ASP.NET Core 8 API                         |
  |                                                                  |
  |  Workspaces  Auth  Etsy Sync  Products  Files  Orders  Bundles   |
  +------------------------------------------------------------------+
        |             |               |               |
        v             v               v               v
  +------------+  +-----------+  +-------------+  +------------------+
  | Cosmos DB  |  | Blob      |  | Azure       |  | OAuth/B2C        |
  |            |  | Storage   |  | Functions   |  | Identity Provider|
  +------------+  +-----------+  +-------------+  +------------------+
        ^                              |
        |                              v
  +-------------+              +---------------+
  | React SPA   |              | Etsy API      |
  | User Browser|              | Listings/Orders|
  +-------------+              +---------------+

Later Phase:
  Preparation Bundles -> Printer Adapter Contracts -> Bambu/OctoEverywhere
```

---

## Core Components

### 1. PrintHub.API

- ASP.NET Core 8 Web API.
- REST endpoints for workspaces, members, Etsy connection, products, parts, files, orders, and preparation bundles.
- JWT bearer authentication from OAuth/B2C.
- Workspace-scoped authorization on every protected endpoint.
- Swagger/OpenAPI documentation.

### 2. PrintHub.Core

- Domain entities and value objects.
- Repository and service interfaces.
- Preparation bundle planning logic.
- Workspace role/permission rules.

### 3. PrintHub.Infrastructure

- Cosmos DB persistence.
- Azure Blob Storage file storage.
- Etsy API client.
- OAuth provider integration.
- Later phase: printer adapter implementations.

### 4. PrintHub.Worker

- Etsy listing/order sync jobs.
- Webhook processing.
- File thumbnail/metadata jobs where needed.
- Bundle archive generation.
- Later phase: printer job monitoring.

### 5. PrintHub.Frontend

- React + TypeScript SPA.
- Public landing page.
- Authenticated workspace app shell.
- Dashboard, orders, products, parts, bundles, and settings.

---

## Data Storage

| Data | Storage | Rationale |
|------|---------|-----------|
| Users, workspaces, memberships | Cosmos DB | Flexible document model and workspace-scoped queries |
| Shops, products, parts, orders | Cosmos DB | Fast workspace-scoped reads |
| Preparation bundle manifests | Cosmos DB + Blob Storage | Query metadata in DB, archive files in Blob |
| Source STL/3MF files | Azure Blob Storage | Cost-effective binary storage, signed URLs |
| Audit logs | Cosmos DB or Table Storage | Time-ordered workspace audit trail |

---

## Security Considerations

- Source STL/3MF files are retained by default and can be deleted or purged by authorized users.
- Generated bundle archives are short-lived by default.
- Etsy tokens and other secrets are encrypted at rest.
- File downloads use time-limited signed URLs or authenticated streams.
- Every query is scoped by workspace authorization.
- User files are never used for AI/ML training.

See [security.md](./security.md) for the broader security model.

---

## Deployment

### Azure Services

- Azure Container Apps or App Service for the Web API.
- Azure Static Web Apps or equivalent static hosting for the React SPA.
- Azure Functions for background workers.
- Azure Cosmos DB for primary data.
- Azure Blob Storage for file storage.
- Azure AD B2C or equivalent OAuth provider for authentication.
- SendGrid or Azure Communication Services for invites/notifications.

### CI/CD

- GitHub Actions for build, test, and deployment.
- Infrastructure as Code through Bicep/ARM/Terraform when infra is introduced.

---

## Roadmap

### Phase 1 - Shared Etsy File Workspace

- [ ] OAuth sign-in.
- [ ] Shared workspace/project with owner and contributor roles.
- [ ] Etsy OAuth plus listing/order import.
- [ ] Product and part management.
- [ ] STL/3MF upload and versioning.
- [ ] Order preparation bundle generation.
- [ ] Download files and manifest for manual printing.

### Phase 2 - Inventory

- [ ] Inventory tracking.
- [ ] Cost per print calculation.
- [ ] Low stock alerts.
- [ ] Etsy sales sync refinement.

### Phase 3 - Printer Execution

- [ ] Printer adapter contract.
- [ ] Bambu Connect integration.
- [ ] OctoEverywhere bridge.
- [ ] Basic print queue submission.
- [ ] Live job state.

### Phase 4 - Intelligence

- [ ] Sales velocity tracking.
- [ ] Reorder recommendations.
- [ ] Batch print optimization.
- [ ] Seasonal trend insights.
