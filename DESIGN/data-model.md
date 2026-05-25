# PrintHub - Data Model

## Overview

PrintHub Phase 1 is a shared Etsy production workspace. The data model is centered on:

- OAuth users who belong to one or more workspaces.
- A workspace that can connect one Etsy shop.
- Products imported from Etsy or created manually.
- Printable parts and versioned STL/3MF source files.
- Etsy orders that are prepared into downloadable file bundles for manual printing.

Direct printer submission, live printer telemetry, and Bambu-specific task IDs are later-phase concerns.

---

## Core Entities

### User

OAuth-backed person using PrintHub.

```text
User
|-- Id: Guid (PK)
|-- Email: string
|-- ExternalAuthSubject: string
|-- DisplayName: string?
|-- CreatedAt: DateTime
|-- UpdatedAt: DateTime
`-- WorkspaceMemberships: List<WorkspaceMember>
```

Auth lock: no `PasswordHash`, password registration, password login, or password reset data exists in PrintHub.

### Workspace

A shared production space for one shop team.

```text
Workspace
|-- Id: Guid (PK)
|-- Name: string
|-- OwnerUserId: Guid (FK)
|-- CreatedAt: DateTime
|-- UpdatedAt: DateTime
|-- Members: List<WorkspaceMember>
|-- Shops: List<Shop>
|-- Products: List<Product>
`-- Parts: List<Part>
```

### WorkspaceMember

Connects users to a workspace with role-based access.

```text
WorkspaceMember
|-- Id: Guid (PK)
|-- WorkspaceId: Guid (FK)
|-- UserId: Guid (FK)
|-- Role: WorkspaceRole
|-- InvitedByUserId: Guid?
|-- InvitedEmail: string?
|-- AcceptedAt: DateTime?
|-- CreatedAt: DateTime
`-- RemovedAt: DateTime?
```

### WorkspaceRole

```text
Owner
Contributor
Viewer
```

Owner can manage Etsy connection, members, products, files, and order preparation. Contributor can manage products, files, and preparation bundles. Viewer is read-only and optional for Phase 1.

### Shop

Represents the Etsy shop connected to a workspace.

```text
Shop
|-- Id: Guid (PK)
|-- WorkspaceId: Guid (FK)
|-- ConnectedByUserId: Guid (FK)
|-- Provider: string ("etsy")
|-- ExternalId: string
|-- AccessToken: string (encrypted)
|-- RefreshToken: string (encrypted)
|-- ShopName: string
|-- IsActive: bool
|-- LastListingSyncAt: DateTime?
|-- LastOrderSyncAt: DateTime?
|-- CreatedAt: DateTime
`-- UpdatedAt: DateTime
```

Phase 1 supports one active Etsy shop per workspace. The model allows additional shops later.

### Product

An Etsy listing or manual product that can be mapped to printable parts.

```text
Product
|-- Id: Guid (PK)
|-- WorkspaceId: Guid (FK)
|-- ShopId: Guid? (FK)
|-- ExternalListingId: string?
|-- Name: string
|-- Description: string?
|-- EtsyPrice: decimal?
|-- ImageUrl: string?
|-- IsActive: bool
|-- RequiresPersonalization: bool
|-- PersonalizationSchemaJson: string?
|-- CreatedAt: DateTime
|-- UpdatedAt: DateTime
`-- ProductParts: List<ProductPart>
```

Inventory and cost fields can be added in Phase 2. Phase 1 should not block file preparation on inventory completeness.

### Part

A reusable printable component.

```text
Part
|-- Id: Guid (PK)
|-- WorkspaceId: Guid (FK)
|-- Name: string
|-- Description: string?
|-- IsGeneric: bool
|-- CurrentVersionId: Guid? (FK to PrintFileVersion)
|-- CreatedAt: DateTime
|-- UpdatedAt: DateTime
`-- PrintFiles: List<PrintFile>
```

### ProductPart

Junction table linking products to parts and quantities.

```text
ProductPart
|-- Id: Guid (PK)
|-- ProductId: Guid (FK)
|-- PartId: Guid (FK)
|-- QuantityPerProduct: int
|-- SortOrder: int
`-- PreparationNotes: string?
```

### PrintFile

A logical source file attached to a part. Each upload creates a version.

```text
PrintFile
|-- Id: Guid (PK)
|-- WorkspaceId: Guid (FK)
|-- PartId: Guid (FK)
|-- FileName: string
|-- FileType: string (".stl", ".3mf")
|-- CurrentVersionNumber: int
|-- IsDeleted: bool
|-- CreatedAt: DateTime
|-- UpdatedAt: DateTime
`-- Versions: List<PrintFileVersion>
```

### PrintFileVersion

Immutable uploaded file version.

```text
PrintFileVersion
|-- Id: Guid (PK)
|-- PrintFileId: Guid (FK)
|-- WorkspaceId: Guid (FK)
|-- VersionNumber: int
|-- FilePath: string
|-- FileHash: string
|-- FileSizeBytes: long
|-- ThumbnailPath: string?
|-- UploadedByUserId: Guid (FK)
|-- UploadedAt: DateTime
`-- Notes: string?
```

Source STL/3MF files are retained by default. Users may delete/purge them from the workspace when desired.

### EtsyOrder

Normalized order record synced from Etsy.

```text
EtsyOrder
|-- Id: Guid (PK)
|-- WorkspaceId: Guid (FK)
|-- ShopId: Guid (FK)
|-- ExternalOrderId: string
|-- CustomerName: string?
|-- Status: EtsyOrderStatus
|-- OrderedAt: DateTime
|-- DueBy: DateTime?
|-- RawPayloadJson: string?
|-- CreatedAt: DateTime
|-- UpdatedAt: DateTime
`-- Items: List<EtsyOrderItem>
```

### EtsyOrderItem

```text
EtsyOrderItem
|-- Id: Guid (PK)
|-- EtsyOrderId: Guid (FK)
|-- ProductId: Guid? (FK)
|-- ExternalListingId: string?
|-- ListingTitle: string
|-- Quantity: int
|-- VariationJson: string?
|-- PersonalizationJson: string?
`-- PreparationStatus: OrderItemPreparationStatus
```

### EtsyOrderStatus

```text
Open
ReadyToPrint
Printed
Completed
Cancelled
```

### OrderItemPreparationStatus

```text
NeedsMapping
NeedsFiles
NeedsPersonalization
Ready
Downloaded
Printed
Blocked
```

### PreparationBundle

A generated record for one order or manual batch that says exactly what to download and print.

```text
PreparationBundle
|-- Id: Guid (PK)
|-- WorkspaceId: Guid (FK)
|-- EtsyOrderId: Guid? (FK)
|-- CreatedByUserId: Guid (FK)
|-- Status: PreparationBundleStatus
|-- ManifestPath: string?
|-- DownloadArchivePath: string?
|-- CreatedAt: DateTime
|-- DownloadedAt: DateTime?
|-- CompletedAt: DateTime?
`-- Items: List<PreparationBundleItem>
```

### PreparationBundleStatus

```text
Draft
Blocked
ReadyToDownload
Downloaded
Printed
Cancelled
```

### PreparationBundleItem

```text
PreparationBundleItem
|-- Id: Guid (PK)
|-- PreparationBundleId: Guid (FK)
|-- ProductId: Guid? (FK)
|-- PartId: Guid (FK)
|-- PrintFileVersionId: Guid (FK)
|-- Quantity: int
|-- PersonalizationJson: string?
|-- RequiresManualCustomization: bool
|-- GeneratedFilePath: string?
`-- Notes: string?
```

For Phase 1, generated output may be a manifest plus source files. Automated 3MF modification can be added incrementally; manual customization must be clearly represented when automation is not available.

### AuditEvent

Workspace-scoped audit trail for important changes.

```text
AuditEvent
|-- Id: Guid (PK)
|-- WorkspaceId: Guid (FK)
|-- ActorUserId: Guid?
|-- EntityType: string
|-- EntityId: Guid?
|-- Action: string
|-- DetailsJson: string?
`-- CreatedAt: DateTime
```

---

## Relationships Diagram

```text
User
  `-- WorkspaceMember
        `-- Workspace
              |-- Shop
              |     `-- EtsyOrder
              |           `-- EtsyOrderItem
              |-- Product
              |     `-- ProductPart
              |           `-- Part
              |                 `-- PrintFile
              |                       `-- PrintFileVersion
              `-- PreparationBundle
                    `-- PreparationBundleItem
                          |-- Product
                          |-- Part
                          `-- PrintFileVersion
```

---

## Example: Personalized Sign Order

1. Etsy sync imports an order for 2 custom signs with personalization `{ "name": "Mia" }`.
2. The Etsy listing maps to Product "Name Sign".
3. Product "Name Sign" maps to one Part "Sign Base", quantity 1 per product.
4. The preparation engine calculates 2 copies of the current file version.
5. If automated personalization is available, the bundle item points to generated 3MF output.
6. If automation is not available, the bundle is marked `Blocked` or `ReadyToDownload` with `RequiresManualCustomization = true`, depending on whether the base file is present.
7. The user downloads the bundle and prints manually.

---

## Versioning Strategy

- Print files are immutable once uploaded.
- New uploads create new versions under the same `PrintFile`.
- `CurrentVersionId` on `Part` points to the active version.
- Preparation bundles reference specific `PrintFileVersionId` values.
- Historical bundles keep their file versions even if a current version changes later.
