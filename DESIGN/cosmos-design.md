# PrintHub - Cosmos DB Design & Partition Strategy

## Overview

PrintHub uses Azure Cosmos DB as the primary database. Phase 1 access is workspace-scoped: users may only read or mutate data for workspaces where they have an active membership.

## Tenant Strategy

- One Cosmos account per environment: dev, staging, prod.
- One database per application: `PrintHub`.
- No tenant isolation at the database level in Phase 1.
- Authorization is enforced by the API layer.
- Every hot-path query includes `WorkspaceId` or a partition key derived from workspace scope.

## Container Strategy

| Container | Partition Key | Description |
|-----------|---------------|-------------|
| `Users` | `/id` | OAuth-backed user profiles |
| `Workspaces` | `/id` | Workspace metadata |
| `WorkspaceMembers` | `/workspaceId` | Memberships and pending invites |
| `Shops` | `/workspaceId` | Etsy shop connection metadata |
| `Products` | `/workspaceId` | Etsy/manual products |
| `Parts` | `/workspaceId` | Reusable printable parts |
| `PrintFiles` | `/workspaceId` | Logical file records |
| `PrintFileVersions` | `/workspaceId` | Immutable uploaded file versions |
| `EtsyOrders` | `/workspaceId` | Synced order records and line items |
| `PreparationBundles` | `/workspaceId` | Generated/downloadable bundles |
| `AuditEvents` | `/workspaceId` | Workspace-scoped audit trail |

## Partition Key Rationale

1. Workspace-scoped queries dominate Phase 1: dashboard, products, orders, files, and bundles.
2. Contributors need access to the same shop data, so `userId` is not the right primary partition for products/files/orders.
3. One active Etsy shop per workspace keeps shop-level data naturally colocated without making shop ownership equal user ownership.
4. Cross-workspace queries are not allowed in user-facing hot paths.

Cross-partition queries are allowed only for:

- Background analytics with rate limits.
- Internal admin diagnostics with explicit pagination and timeouts.

## Throughput

| Environment | Mode | RU/s |
|-------------|------|------|
| Dev | Serverless | Pay-per-use |
| Staging | Serverless | Pay-per-use |
| Prod startup | Serverless or autoscale 400-4000 | Scale with demand |

## Indexing Policy

Default automatic indexing is acceptable initially. Add composite indexes for:

- `WorkspaceId` + `CreatedAt` descending for newest-first lists.
- `WorkspaceId` + `Status` + `CreatedAt` for filtered order and bundle views.
- `WorkspaceId` + `ExternalListingId` for Etsy listing mapping.
- `WorkspaceId` + `ExternalOrderId` for Etsy order upserts.

## TTL

- Generated bundle archives: short-lived in Blob Storage by default.
- Audit events: retain at least 365 days unless storage cost requires adjustment.
- Soft-deleted file metadata: retain 90 days.
- Source file blobs: retained by default until user delete/purge.

## Lock

- Do not change partition keys without a migration plan.
- Do not introduce cross-partition queries in user-facing hot paths.
- Do not partition product/order/file data by user; contributors must share the same workspace data.
