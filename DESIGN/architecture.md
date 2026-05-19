# PrintHub - Architecture Overview

## Overview

PrintHub is a cloud-hosted SaaS platform that bridges Etsy stores with 3D printing operations. It enables print-on-demand sellers to manage products, track inventory, queue prints, and gain business insights—all from a single dashboard.

## Goals

- **Zero local setup** for Bambu printers (cloud-native integration)
- **Guided onboarding** for other printers (OctoEverywhere bridge instructions)
- **Security-first** — STL/3MF files are ephemeral assets
- **Business intelligence** — help sellers optimize inventory and pricing
- **Personalized orders** — handle customizations from Etsy orders seamlessly

---

## High-Level Architecture

```
┌─────────────────────────────────────────────────────────────────────────┐
│                              PrintHub Azure                              │
│                                                                         │
│  ┌─────────────────────────────────────────────────────────────────┐   │
│  │                      ASP.NET Core Web API                        │   │
│  │                                                                   │   │
│  │  ┌──────────┐  ┌──────────┐  ┌──────────┐  ┌──────────────────┐ │   │
│  │  │ Products │  │  Parts   │  │   Jobs   │  │ Insight Engine  │ │   │
│  │  └──────────┘  └──────────┘  └──────────┘  └──────────────────┘ │   │
│  └─────────────────────────────────────────────────────────────────┘   │
│                                │                                          │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐                   │
│  │ Azure Cosmos │  │ Azure Blob   │  │ Azure Funcs  │                   │
│  │     DB       │  │   Storage    │  │  (Jobs/API)  │                   │
│  └──────────────┘  └──────────────┘  └──────────────┘                   │
└──────────────────────────────────────────────────────────────────────────┘
           │                    │                    │
    ┌──────┴──────┐      ┌─────┴─────┐       ┌──────┴──────┐
    │ Bambu Cloud │      │   User     │       │  Etsy API   │
    │  (P1S/X1C)  │      │  Browser   │       │             │
    └─────────────┘      └────────────┘       └─────────────┘
           │
    ┌──────┴──────┐
    │ OctoEverywhere│
    │  (Other     │
    │  Printers)  │
    └─────────────┘
```

---

## Core Components

### 1. PrintHub.API
- **ASP.NET Core 8** Web API
- RESTful endpoints for all operations
- JWT authentication
- Swagger/OpenAPI documentation

### 2. PrintHub.Core
- Domain entities
- Interfaces (repository, services)
- Business logic

### 3. PrintHub.Infrastructure
- Entity Framework Core (Cosmos DB provider)
- Azure Blob Storage integration
- Bambu Connect API client
- Etsy API client
- OctoEverywhere API client

### 4. PrintHub.Worker
- Azure Functions for background jobs
- Etsy order polling
- Inventory sync
- Alert generation

### 5. PrintHub.Frontend (future)
- React or Blazor (separate repo)
- Web dashboard for users

---

## Data Storage

| Data | Storage | Rationale |
|------|---------|-----------|
| User accounts, shops | Azure Cosmos DB | Flexible schema, global distribution |
| Products, parts, versions | Azure Cosmos DB | Fast reads, easy to query |
| STL/3MF files | Azure Blob Storage | Cheaper than DB, signed URLs for security |
| Print job history | Azure Cosmos DB | TTL for auto-expiration if needed |
| Audit logs | Azure Cosmos DB or Table Storage | Queryable, time-ordered |

---

## Security Considerations

- **Source STL/3MF files retained by default** — user-controlled purge; generated files are short-lived by default
- **Encryption at rest** — Azure Storage with customer-managed keys
- **Signed URLs** — time-limited access to STL/3MF files
- **Ephemeral compute** — slicing in isolated containers
- **No AI training** — explicit policy: user files are never used for AI training
- **SOC 2 Type II** — roadmap goal for enterprise trust

See [security.md](./security.md) for full details.

---

## Deployment

### Azure Services
- **Azure Container Apps** or **App Service** — Web API
- **Azure Functions** — Background workers
- **Azure Cosmos DB** — Primary database
- **Azure Blob Storage** — File storage
- **Azure Active Directory** — Authentication
- **Azure Logic Apps** or **SendGrid** — Notifications

### CI/CD
- GitHub Actions for build/deploy
- Infrastructure as Code (Bicep/ARM)

---

## Environment Matrix

| Feature | Bambu (P1S, X1C, etc.) | Klipper (Centauri, Vyper, etc.) |
|---------|------------------------|--------------------------------|
| Cloud-native | ✅ Bambu Connect API | ❌ Requires local bridge |
| Zero local setup | ✅ | ❌ |
| OctoEverywhere bridge | N/A | ✅ Instructions provided |
| Full feature support | ✅ | ✅ (via bridge) |
| Future: custom firmware | N/A | Possible |

---

## Roadmap

### Phase 1 — MVP
- [ ] Etsy OAuth + listing import
- [ ] Product/part management
- [ ] STL/3MF upload + versioning
- [ ] Bambu Connect integration
- [ ] Basic print queue

### Phase 2 — Inventory
- [ ] Inventory tracking
- [ ] Cost per print calculation
- [ ] Low stock alerts
- [ ] Etsy sales sync

### Phase 3 — Intelligence
- [ ] Sales velocity tracking
- [ ] Reorder recommendations
- [ ] Batch print optimization
- [ ] Seasonal trend insights

### Phase 4 — Scale
- [ ] Personalized order handling
- [ ] Multi-printer queue management
- [ ] SOC 2 compliance
- [ ] White-label options