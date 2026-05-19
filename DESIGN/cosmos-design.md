# PrintHub - Cosmos DB Design & Partition Strategy

## Overview

PrintHub uses **Azure Cosmos DB** as the primary database. This document locks the tenant, container, and partition-key strategy.

## Tenant Strategy

- **One Cosmos account per environment** (dev, staging, prod).
- **One database per application** (`PrintHub`).
- **No multi-tenant isolation at the database level** — all shops share the same database.
- **Row-level security** enforced by the API layer: every query includes `ShopId` as a mandatory filter.

## Container Strategy

| Container | Partition Key | Description |
|-----------|---------------|-------------|
| `Shops` | `/userId` | One doc per shop. Partitioned by owner for user-centric lookups. |
| `Products` | `/shopId` | One doc per product. Partitioned by shop for shop-scoped queries. |
| `Parts` | `/shopId` | One doc per part. Partitioned by shop. |
| `PrintFileVersions` | `/shopId` | One doc per file version. Partitioned by shop. |
| `PrintJobs` | `/shopId` | One doc per job. Partitioned by shop. |
| `PrintJobItems` | `/shopId` | Child items of a job. Partitioned by shop (not by jobId) to keep all shop data collocated. |
| `InventoryMovements` | `/shopId` | Audit trail. Partitioned by shop. |
| `PersonalizedOrders` | `/shopId` | One doc per order. Partitioned by shop. |
| `CostRecords` | `/shopId` | One doc per cost record. Partitioned by shop. |

## Partition Key Rationale

1. **Shop-scoped queries dominate** — 90%+ of reads are within a single shop (dashboard, queue, product list).
2. **Avoid hot partitions** — `shopId` has natural cardinality (one partition per shop).
3. **No cross-shop queries in prod** — admin/insight aggregations run in the API/service layer, not as cross-partition Cosmos queries.
4. **Cross-partition queries allowed only for:**
   - Background analytics (Azure Functions with rate-limited execution)
   - Admin dashboards (with explicit pagination and timeouts)

## Throughput

| Environment | Mode | RU/s |
|-------------|------|------|
| Dev | Serverless | Pay-per-use |
| Staging | Serverless | Pay-per-use |
| Prod (startup) | Serverless or Autoscale 400–4000 | Scale with demand |

## Indexing Policy

Default automatic indexing. Explicit composite indexes for:
- `ShopId` + `CreatedAt` (DESC) — list queries ordered by newest first
- `ShopId` + `Status` + `CreatedAt` — filtered job queues

## TTL

- `InventoryMovements`: 365 days
- `CostRecords`: 730 days
- `PrintFileVersions` (if soft-deleted): 90 days

## Lock

- **Do not change partition keys without a migration plan reviewed by the team.**
- **Do not introduce cross-partition queries in the hot path.**
- **Do not create per-shop databases or containers.**
