# PrintHub

**Shared Etsy production workspace for 3D print file preparation**

PrintHub connects an Etsy shop to a shared product and STL/3MF file library, so a small team can prepare and download the right files for each order.

---

## Phase 1 Goal

Phase 1 is intentionally focused and useful without Bambu integration:

1. Sign in with OAuth.
2. Create or select a workspace.
3. Link the workspace to an Etsy shop.
4. Invite a contributor, such as a family member or production partner.
5. Import or create products.
6. Upload and version product/part STL and 3MF files.
7. Turn each Etsy order into a downloadable file bundle and manifest.
8. Print manually, then mark the bundle/order complete.

Direct printer submission, live printer status, and Bambu/OctoEverywhere integrations are later-phase work.

---

## Features

### Phase 1

- **Etsy Integration** - Import listings, sync orders, and retain Etsy order context.
- **Shared Workspace** - Owners can invite contributors to help manage products and files.
- **Product Management** - Organize products, parts, STL/3MF files, and current versions.
- **Smart File Preparation** - Generate order-specific download bundles and manifests.
- **Personalization Support** - Capture custom names/text from Etsy and flag manual or automated customization steps.

> **Design lock:** OAuth-only auth. React + TypeScript frontend. Light operations theme. Source STL/3MF files are retained by default with user-controlled deletion/purge. Phase 1 does not require Bambu integration.

### Later Phases

- Inventory tracking, cost calculation, and low-stock alerts.
- Bambu/OctoEverywhere printer adapters and direct print submission.
- Sales velocity, reorder recommendations, and batch optimization.

---

## Architecture

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
```

See [DESIGN](./DESIGN) for architecture, API, data model, UI, and design-system documentation.

---

## Tech Stack

| Layer | Technology |
|-------|------------|
| Frontend | React + TypeScript + Vite |
| Backend API | ASP.NET Core 8 |
| Database | Azure Cosmos DB |
| File Storage | Azure Blob Storage |
| Background Jobs | Azure Functions |
| Authentication | Azure AD B2C / OAuth / JWT |
| External Integration | Etsy API |
| Later Printer Integration | Bambu Connect API, OctoEverywhere |

---

## Project Structure

```text
PrintHub/
|-- DESIGN/
|   |-- phase-1-shared-workspace.md
|   |-- architecture.md
|   |-- api-design.md
|   |-- cosmos-design.md
|   |-- data-model.md
|   |-- dotnet-structure.md
|   |-- frontend-design-system.md
|   |-- print-queue.md
|   |-- printer-integrations.md
|   |-- security.md
|   `-- ui-architecture.md
|-- frontend/
|   `-- src/
|-- src/
|   |-- PrintHub.API/
|   |-- PrintHub.Core/
|   |-- PrintHub.Infrastructure/
|   `-- PrintHub.Worker/
|-- ISSUES_EXECUTION_PLAN.md
`-- PrintHub.sln
```

---

## Status

Frontend prototype/app shell and backend scaffolding are in progress. Phase 1 is scoped to the shared Etsy file-preparation workspace rather than direct printer execution.

### Roadmap

- [ ] **Phase 1 - Shared Etsy File Workspace:** OAuth sign-in, workspace contributors, Etsy connection, product import, STL/3MF upload/versioning, order preparation, file bundle download.
- [ ] **Phase 2 - Inventory:** Inventory tracking, cost calculation, low stock alerts.
- [ ] **Phase 3 - Printer Execution:** Bambu/OctoEverywhere adapters, print submission, live job state.
- [ ] **Phase 4 - Intelligence:** Sales velocity, reorder recommendations, batch optimization.

---

## Getting Started

```bash
# Frontend
cd frontend
npm install
npm run dev

# Backend, once local .NET SDK is available
dotnet restore PrintHub.sln
dotnet build PrintHub.sln
dotnet test PrintHub.sln
```

---

## License

Private - All rights reserved
