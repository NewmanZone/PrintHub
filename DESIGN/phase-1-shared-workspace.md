# PrintHub - Phase 1 Shared Workspace Goal

## Product Goal

Phase 1 is a shared Etsy production workspace for a small shop team. The first real workflow is:

1. Sign in with OAuth.
2. Create or select a workspace.
3. Connect an Etsy shop.
4. Invite a trusted contributor, such as a family member, to the workspace.
5. Import or create Etsy products.
6. Attach the source 3MF/STL files needed to print each product.
7. Map products to their printable parts and personalization templates.
8. See incoming Etsy orders and decide what needs to be printed.
9. Generate or prepare the right print file set for the order.
10. Download the prepared files manually for printing.

Direct Bambu submission is intentionally **not required** for Phase 1.

## Phase 1 User Story

As a shop owner working with a contributor, I want one shared space that contains our Etsy products, source 3MF files, product-to-file mappings, and incoming orders, so either of us can prepare and download the correct files for printing.

## In Scope

- OAuth-only user sign-in.
- Workspace/project ownership with contributor access.
- Etsy OAuth connection for one shop per workspace.
- Etsy listing import.
- Etsy order import or manual refresh.
- Product library with linked Etsy listing IDs.
- Product detail pages with associated design files.
- File upload, versioning, current-version selection, and download.
- Product file rules:
  - Standard product: use selected current 3MF/STL file versions.
  - Quantity greater than one: produce a print-prep bundle that repeats the file instructions/counts.
  - Personalized product: capture personalization fields from Etsy and generate/prepare a per-order output when automation is available.
- Manual fallback for personalization:
  - Show the personalization data clearly.
  - Allow the user to download source files and mark the order as prepared manually.
- Order preparation status:
  - Received
  - NeedsMapping
  - NeedsFiles
  - NeedsPersonalization
  - ReadyToDownload
  - Downloaded
  - Printed
  - Blocked
- Basic audit trail for who uploaded, prepared, downloaded, or marked an order printed.

## Out Of Scope For Phase 1

- Sending jobs directly to Bambu Cloud.
- Live printer status.
- Automatic slicer/printer queue execution.
- Full inventory forecasting.
- Billing.
- Multi-shop workspaces.
- Public marketplace features.

## Roles

### Owner

- Connects or disconnects Etsy.
- Invites and removes contributors.
- Uploads, deletes, and selects current file versions.
- Prepares and downloads order files.
- Updates order preparation status.

### Contributor

- Views workspace products and orders.
- Uploads file versions.
- Prepares and downloads order files.
- Updates order preparation status.
- Cannot disconnect Etsy or remove the owner.

## Core Data Concepts

### Workspace

A shared project for one Etsy production operation. A workspace owns the Etsy shop connection, products, files, orders, and contributor list.

### Contributor

A user with explicit access to a workspace. Contributor access is scoped to that workspace.

### Product

An Etsy-linked or manually created product. Products can be mapped to one or more printable parts/files.

### Design File

An uploaded STL/3MF source file. Source files are retained by default unless a user purges them.

### File Version

Each upload creates an immutable version. Products and parts point to the current version used for preparation.

### Preparation Bundle

A generated record for one order or batch that says exactly which files to download and what personalization/quantity instructions apply. In Phase 1, a bundle can be a manifest plus source files. Later phases can turn this into generated 3MF output.

## Order Preparation Rules

### Standard Quantity

If an order contains a product with quantity `N`, the preparation bundle includes the current file version and quantity `N`.

### Multi-Part Product

If a product is made from multiple parts, the preparation bundle includes each mapped current file version multiplied by the product quantity.

### Personalized Product

If Etsy personalization exists, the preparation bundle includes:

- Original personalization text.
- Product template or source file.
- Required output file name.
- Status of automated generation.
- Manual preparation fallback if automation is unavailable.

Automated personalization can start as a structured placeholder in Phase 1, provided the UI makes the manual workflow clear.

## Phase 1 Success Criteria

- Owner signs in with OAuth.
- Owner creates or opens a workspace.
- Owner connects Etsy.
- Etsy products and orders can be imported or refreshed.
- Owner invites contributor by email.
- Contributor can access the same workspace after OAuth sign-in.
- Either user can upload a 3MF/STL file and associate it with a product.
- Either user can open an order and see the required files and personalization details.
- Either user can download the needed files or bundle manifest.
- Either user can mark the order as prepared/printed.
- Tests cover owner/contributor permissions, file versioning, order preparation, download manifest generation, and edge cases.

## Edge Cases To Design And Test

- Contributor tries to access a workspace without an invite.
- Contributor tries to disconnect Etsy.
- Etsy listing has no matching PrintHub product yet.
- Etsy order references a product with no uploaded file.
- Product has multiple files and one missing current version.
- Quantity is greater than one.
- Personalization text is empty, very long, or contains unsafe filename characters.
- Two users upload new versions around the same time.
- A file is purged after an order bundle was prepared.
- Download is requested for an order that is not ready.
- Etsy token expires and needs reconnect.
- Contributor is removed while signed in.
