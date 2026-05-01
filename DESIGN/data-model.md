# PrintHub - Data Model

## Overview

PrintHub uses a Bill of Materials (BOM) approach to manage products and their constituent parts. Parts can be:
- **Generic** — shared across multiple products (e.g., basic hooks, standard connectors)
- **Product-specific** — unique to a single product (e.g., custom character sculpts)

This allows efficient batch printing: printing 10 Product A and 5 Product B with shared parts means the system calculates exact quantities needed rather than naively multiplying.

---

## Core Entities

### User
```
User
├── Id: Guid (PK)
├── Email: string
├── PasswordHash: string
├── CreatedAt: DateTime
├── UpdatedAt: DateTime
└── Shops: List<Shop>
```

### Shop
Represents an Etsy shop connected to PrintHub.
```
Shop
├── Id: Guid (PK)
├── UserId: Guid (FK)
├── Provider: string ("etsy")
├── ExternalId: string (Etsy shop ID)
├── AccessToken: string (encrypted)
├── RefreshToken: string (encrypted)
├── ShopName: string
├── IsActive: bool
├── LastSyncAt: DateTime?
└── Products: List<Product>
```

### Product
A product as it appears on Etsy (or a standalone product not yet on Etsy).
```
Product
├── Id: Guid (PK)
├── ShopId: Guid (FK)
├── ExternalListingId: string? (Etsy listing ID)
├── Name: string
├── Description: string?
├── EtsyPrice: decimal?
├── ImageUrl: string?
├── IsActive: bool
├── PrintCount: int (how many printed historically)
├── InventoryOnHand: int (printed - sold)
├── ReorderPoint: int?
├── ReorderQuantity: int?
├── CostPerPrint: decimal?
├── CreatedAt: DateTime
├── UpdatedAt: DateTime
└── ProductParts: List<ProductPart>
```

### Part
A reusable component that can be printed.
```
Part
├── Id: Guid (PK)
├── ShopId: Guid (FK)
├── Name: string
├── Description: string?
├── IsGeneric: bool (shared across products vs product-specific)
├── CurrentVersionId: Guid? (FK to PrintFileVersion)
├── CostPerUnit: decimal (filament + electricity estimate)
├── InventoryOnHand: int (for generic parts, printed but unassigned)
├── CreatedAt: DateTime
├── UpdatedAt: DateTime
└── PrintFileVersions: List<PrintFileVersion>
```

### ProductPart
Junction table linking Products to Parts with quantities.
```
ProductPart
├── Id: Guid (PK)
├── ProductId: Guid (FK)
├── PartId: Guid (FK)
├── QuantityPerProduct: int
└── SortOrder: int (for display/printing sequence)
```

### PrintFile
A 3D model file (STL, 3MF, OBJ, etc.) with version tracking.
```
PrintFile
├── Id: Guid (PK)
├── PartId: Guid (FK)
├── FileName: string
├── FileType: string (".stl", ".3mf", ".obj")
├── FileSizeBytes: long
├── CurrentVersionNumber: int
├── IsDeleted: bool
├── CreatedAt: DateTime
├── UpdatedAt: DateTime
└── Versions: List<PrintFileVersion>
```

### PrintFileVersion
Each upload creates a new version.
```
PrintFileVersion
├── Id: Guid (PK)
├── PrintFileId: Guid (FK)
├── VersionNumber: int
├── FilePath: string (blob storage path)
├── FileHash: string (SHA-256 for integrity)
├── ThumbnailPath: string?
├── UploadedAt: DateTime
└── Notes: string?
```

### PrintJob
A request to print one or more items.
```
PrintJob
├── Id: Guid (PK)
├── UserId: Guid (FK)
├── ShopId: Guid (FK)
├── Status: PrintJobStatus
├── PrinterTarget: string? (printer serial/IP for non-Bambu)
├── CreatedAt: DateTime
├── StartedAt: DateTime?
├── CompletedAt: DateTime?
├── EstimatedMinutes: int?
├── Notes: string?
└── PrintJobItems: List<PrintJobItem>
```

### PrintJobStatus (enum)
```
Pending
Queued
InProgress
Completed
Failed
Cancelled
```

### PrintJobItem
A single item within a print job, linking to a specific Part version.
```
PrintJobItem
├── Id: Guid (PK)
├── PrintJobId: Guid (FK)
├── PartId: Guid (FK)
├── PrintFileVersionId: Guid (FK)
├── Quantity: int
├── Status: PrintJobItemStatus
├── BambuTaskId: string? (from Bambu Cloud API)
└── Notes: string?
```

### PrintJobItemStatus (enum)
```
Pending
Printing
Completed
Failed
```

### PersonalizedOrder
For orders requiring personalization (custom text, name on product, etc.).
```
PersonalizedOrder
├── Id: Guid (PK)
├── ShopId: Guid (FK)
├── EtsyOrderId: string?
├── EtsyListingId: string?
├── CustomerName: string?
├── PersonalizationData: string (JSON, e.g., {"name": "Mike", "color": "blue"})
├── Status: PersonalizedOrderStatus
├── DueBy: DateTime?
├── Notes: string?
├── CreatedAt: DateTime
└── PrintJobId: Guid? (FK, if queued for printing)
```

### PersonalizedOrderStatus (enum)
```
Received
InPreparation
QueuedForPrint
Printed
Shipped
```

### InventoryMovement
Audit log for inventory changes.
```
InventoryMovement
├── Id: Guid (PK)
├── ShopId: Guid (FK)
├── ProductId: Guid? (FK, null for generic part movements)
├── PartId: Guid (FK)
├── QuantityChange: int (+/-)
├── Reason: string (Printed, Sold, Adjusted, Deleted)
├── Reference: string? (PrintJobId, EtsyOrderId, etc.)
├── CreatedAt: DateTime
```

### CostRecord
Tracks cost data for products and parts.
```
CostRecord
├── Id: Guid (PK)
├── ShopId: Guid (FK)
├── ProductId: Guid? (FK, null for standalone part costs)
├── PartId: Guid? (FK)
├── CostType: string (Filament, Electricity, Labor, Other)
├── Amount: decimal
├── Currency: string
├── RecordedAt: DateTime
└── Notes: string?
```

---

## Relationships Diagram

```
User
  └── Shop (1:many)
        ├── Product (1:many)
        │     └── ProductPart (many:many with Part)
        │           └── Part
        │                 └── PrintFile (1:many)
        │                       └── PrintFileVersion (1:many)
        │
        └── Part (1:many) — generic parts live here too

PrintJob (1:many) PrintJobItem
      │
      └── PrintJobItem → Part
                          → PrintFileVersion

PersonalizedOrder (1:1 or 0:1) PrintJob

InventoryMovement → Part or Product

CostRecord → Part or Product
```

---

## Example: Wall Hook Product

### Data

**Part: Generic Hook**
- Id: `part-001`
- Name: "Basic Wall Hook"
- IsGeneric: true
- CurrentVersionId: `version-001`
- CostPerUnit: $0.15 (filament only)
- InventoryOnHand: 12

**Part: Dino Character**
- Id: `part-002`
- Name: "Dino Character Topper"
- IsGeneric: false
- CurrentVersionId: `version-002`
- CostPerUnit: $0.30
- InventoryOnHand: 0

**Product: Dino Wall Hook**
- Id: `product-001`
- EtsyListingId: `etsy-listing-12345`
- EtsyPrice: $24.99
- InventoryOnHand: 4
- ReorderPoint: 6
- ReorderQuantity: 10

**ProductPart entries:**
| PartId | PartName | QtyPerProduct |
|--------|----------|---------------|
| part-001 | Basic Wall Hook | 1 |
| part-002 | Dino Character Topper | 1 |

### Print Queue Scenario

User queues:
- 5x Dino Wall Hook
- 3x Cat Wall Hook (Cat character is `part-003`)
- 2x Bear Wall Hook (Bear character is `part-004`)

**System calculates:**
- Generic hooks needed: 5 + 3 + 2 = 10
- Current generic hook inventory: 12
- Net inventory after print: 12 - 10 + 10 (printed) = 12 (replenished)

**Print jobs generated:**
| PrintJob | Part | Qty | Notes |
|----------|------|-----|-------|
| Job-001 | Basic Wall Hook | 10 | Single batch print, shared |
| Job-002 | Dino Character | 5 | |
| Job-003 | Cat Character | 3 | |
| Job-004 | Bear Character | 2 | |

User sees consolidated queue showing all items and total print time/filament.

---

## Personalized Order Flow

1. Etsy order received with personalization ("Name: Mike")
2. System creates `PersonalizedOrder` with `PersonalizationData`
3. Background worker generates customized 3MF (or flags for manual prep)
4. `PrintJob` created with the personalized file version
5. Print proceeds
6. Status updates back to Etsy (if API supports)

---

## Versioning Strategy

- Print files are immutable once uploaded
- New uploads create new versions under the same `PrintFile`
- `CurrentVersionId` on `Part` points to active version
- PrintJobs reference specific `PrintFileVersionId`, not the part
- Historical jobs retain their version even if current version advances