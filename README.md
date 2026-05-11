# PrintHub

**SaaS platform for 3D print operations with Etsy integration**

PrintHub connects your Etsy store to your 3D printers, enabling print-on-demand sellers to manage products, track inventory, queue prints, and gain business insights—all from a single dashboard.

---

## Features

### Core
- **Etsy Integration** — Import listings, sync orders, track inventory against sales
- **Product Management** — Organize STL/3MF files by product with version control
- **Smart Print Queue** — Consolidate shared parts across products, batch print efficiently
- **Multi-Printer Support** — Bambu (cloud-native) + Klipper/OctoEverywhere (bridge)

> **Design lock:** Azure AD B2C / OAuth-only auth. React + TypeScript frontend. Source STL/3MF retained by default with user purge; generated files short-lived. Adapter-based printer strategy (Bambu primary, OctoEverywhere bridge, Bambu spike experimental). Cosmos partition by `shopId`.

### Inventory & Business Intelligence
- **Inventory Tracking** — Print count vs. sold count, auto-depletion
- **Cost Per Print** — Filament + electricity estimates, margin calculation
- **Low Stock Alerts** — Proactive notifications when products need replenishment
- **Sales Velocity** — Track what sells, predict reorder timing
- **Reorder Recommendations** — "You sold 12 this month, print 10 more?"

### Personalized Orders
- **Etsy Order Sync** — Pull personalized orders automatically
- **Customization Data** — Names, text, colors attached to each order
- **Personalized Print Jobs** — Generate customized 3MF files per order

---

## Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                    PrintHub Azure                          │
│  ┌─────────────────────────────────────────────────────┐  │
│  │              ASP.NET Core 8 Web API                  │  │
│  └─────────────────────────────────────────────────────┘  │
│  ┌──────────┐  ┌──────────┐  ┌──────────┐  ┌──────────┐  │
│  │Products  │  │  Parts   │  │  Jobs    │  │ Insights │  │
│  └──────────┘  └──────────┘  └──────────┘  └──────────┘  │
│         │                │               │                │
│  ┌──────┴────────────────┴───────────────┴──────────┐   │
│  │  Azure Cosmos DB  │  Azure Blob  │  Azure Funcs  │   │
│  └────────────────────────────────────────────────────┘   │
└────────────────────────────────────────────────────────────┘
        │                    │                    │
  Bambu Cloud           Etsy API           User Browser
```

See [DESIGN/](DESIGN/) for full architecture documentation.

---

## Tech Stack

| Layer | Technology |
|-------|------------|
| Backend API | ASP.NET Core 8 (C#) |
| Database | Azure Cosmos DB |
| File Storage | Azure Blob Storage |
| Background Jobs | Azure Functions |
| Authentication | Azure AD B2C / JWT |
| Printer Integration | Bambu Connect API, OctoEverywhere |

---

## Project Structure

```
PrintHub/
├── DESIGN/                      # Architecture & design docs (locked)
│   ├── architecture.md
│   ├── api-design.md
│   ├── cosmos-design.md
│   ├── data-model.md
│   ├── dotnet-structure.md
│   ├── frontend-design-system.md
│   ├── print-queue.md
│   ├── printer-integrations.md
│   ├── security.md
│   └── ui-architecture.md
├── prototypes/                  # Static UI prototype (no backend needed)
│   └── printhub-ui/
│       └── index.html            # Open in any browser
├── src/
│   ├── PrintHub.API/            # Web API (future)
│   ├── PrintHub.Core/          # Domain entities, interfaces
│   ├── PrintHub.Infrastructure/# Implementations
│   └── PrintHub.Worker/        # Azure Functions
└── PrintHub.sln
```

---

## Status

**Current Phase:** Design locked. Prototype complete. Implementation not yet started.

### Roadmap

- [ ] **Phase 1 — MVP**: Etsy OAuth, listing import, product/part management, Bambu print queue
- [ ] **Phase 2 — Inventory**: Inventory tracking, cost calculation, low stock alerts
- [ ] **Phase 3 — Intelligence**: Sales velocity, reorder recommendations, batch optimization
- [ ] **Phase 4 — Scale**: Personalized orders, multi-printer queue, SOC 2

---

## Getting Started

> Implementation not started yet. These steps will apply once the codebase is built.

```bash
# Clone the repo
git clone https://github.com/mln330/PrintHub.git
cd PrintHub

# Restore dependencies
dotnet restore PrintHub.sln

# Build
dotnet build PrintHub.sln

# Run tests
dotnet test PrintHub.sln

# Run locally
cd src/PrintHub.API
dotnet run
```

---

## Contributing

This is a personal project in early design phase. Design feedback and suggestions welcome.

---

## License

Private - All rights reserved