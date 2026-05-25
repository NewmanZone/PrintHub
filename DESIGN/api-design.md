# PrintHub - API Design

## Overview

The PrintHub API is a RESTful ASP.NET Core 8 Web API. Phase 1 supports OAuth sign-in, shared workspaces, Etsy connection, product/file management, order preparation, and downloadable print bundles.

All endpoints return JSON unless a download endpoint explicitly returns a file stream. Authentication uses JWT bearer tokens issued by the configured OAuth/B2C provider.

**Base URL:** `https://api.printhub.example.com/v1`

---

## Authentication

PrintHub is OAuth-only. The API does not expose password registration, password login, or password reset endpoints.

### GET /auth/me

Return the authenticated user's profile and workspace memberships. The API creates the user profile on first valid OAuth sign-in if it does not already exist.

**Response:** `200 OK`

```json
{
  "user": {
    "id": "usr_a1b2c3d4",
    "email": "seller@example.com",
    "displayName": "Mike"
  },
  "workspaces": [
    {
      "id": "wks_123",
      "name": "Newman Prints",
      "role": "Owner"
    }
  ]
}
```

### POST /auth/logout

Clear API-side session state if present. OAuth provider sign-out is handled by the frontend/provider.

**Response:** `204 No Content`

---

## Workspaces

### GET /workspaces

List workspaces available to the authenticated user.

### POST /workspaces

Create a workspace owned by the authenticated user.

**Request:**

```json
{
  "name": "Newman Prints"
}
```

**Response:** `201 Created`

```json
{
  "id": "wks_123",
  "name": "Newman Prints",
  "role": "Owner"
}
```

### GET /workspaces/{workspaceId}

Get workspace summary, member count, connected shop state, and recent preparation stats.

### GET /workspaces/{workspaceId}/members

List workspace members and pending invites.

### POST /workspaces/{workspaceId}/members/invite

Invite a contributor by email.

**Request:**

```json
{
  "email": "dad@example.com",
  "role": "Contributor"
}
```

**Response:** `202 Accepted`

```json
{
  "inviteId": "inv_123",
  "email": "dad@example.com",
  "role": "Contributor",
  "status": "Pending"
}
```

### PUT /workspaces/{workspaceId}/members/{memberId}

Update a member role. Only owners can change membership.

### DELETE /workspaces/{workspaceId}/members/{memberId}

Remove a contributor from the workspace.

---

## Shops - Etsy Integration

Phase 1 supports one active Etsy shop per workspace.

### GET /workspaces/{workspaceId}/shops

List connected shops for a workspace.

### POST /workspaces/{workspaceId}/shops/connect/etsy

Initiate Etsy OAuth flow.

**Response:** `200 OK`

```json
{
  "authUrl": "https://www.etsy.com/oauth2/authorize?..."
}
```

### POST /workspaces/{workspaceId}/shops/etsy/callback

Complete the Etsy OAuth callback.

**Request:**

```json
{
  "code": "etsy_auth_code",
  "state": "opaque_state_value"
}
```

**Response:** `200 OK`

```json
{
  "shopId": "shop_123",
  "shopName": "NewmanZone",
  "connected": true
}
```

### DELETE /workspaces/{workspaceId}/shops/{shopId}

Disconnect a shop. Owner-only in Phase 1.

### POST /workspaces/{workspaceId}/shops/{shopId}/sync

Manually trigger listing and order sync.

**Response:** `202 Accepted`

```json
{
  "syncJobId": "sync_456",
  "status": "Processing"
}
```

---

## Products

### GET /workspaces/{workspaceId}/products

List products imported from Etsy or created manually.

**Query params:**

- `page`
- `pageSize`
- `search`
- `needsFiles=true`
- `requiresPersonalization=true`

**Response:** `200 OK`

```json
{
  "products": [
    {
      "id": "prod_001",
      "name": "Custom Name Sign",
      "externalListingId": "etsy_listing_12345",
      "imageUrl": "https://...",
      "requiresPersonalization": true,
      "fileCoverage": "Complete",
      "parts": [
        {
          "partId": "part_001",
          "partName": "Sign Base",
          "quantityPerProduct": 1,
          "currentVersionId": "ver_003"
        }
      ]
    }
  ],
  "pagination": {
    "page": 1,
    "pageSize": 20,
    "totalCount": 47,
    "totalPages": 3
  }
}
```

### GET /workspaces/{workspaceId}/products/{productId}

Get full product details, mapped parts, file versions, and recent orders.

### POST /workspaces/{workspaceId}/products

Create a manual product.

### PUT /workspaces/{workspaceId}/products/{productId}

Update product fields, personalization settings, and part mappings.

### DELETE /workspaces/{workspaceId}/products/{productId}

Soft delete a product from PrintHub. This does not delete the Etsy listing.

---

## Parts And Files

### GET /workspaces/{workspaceId}/parts

List reusable parts.

### POST /workspaces/{workspaceId}/parts

Create a part.

**Request:**

```json
{
  "name": "Sign Base",
  "description": "Base 3MF for personalized signs",
  "isGeneric": false
}
```

### GET /workspaces/{workspaceId}/parts/{partId}

Get part detail and version history.

### POST /workspaces/{workspaceId}/parts/{partId}/files

Upload a new STL/3MF source file version.

**Request:** `multipart/form-data`

- `file`: `.stl` or `.3mf`
- `notes`: optional version notes

**Response:** `201 Created`

```json
{
  "printFileId": "file_123",
  "versionId": "ver_011",
  "versionNumber": 2,
  "fileName": "sign-base-v2.3mf",
  "fileSizeBytes": 1245184,
  "uploadedAt": "2026-05-20T10:35:00Z"
}
```

### PUT /workspaces/{workspaceId}/parts/{partId}/current-version

Set a specific version as the current preparation version.

**Request:**

```json
{
  "versionId": "ver_011"
}
```

### GET /workspaces/{workspaceId}/files/{versionId}/download

Download a single file version.

### DELETE /workspaces/{workspaceId}/files/{printFileId}

Soft delete a file and hide it from future preparation.

### DELETE /workspaces/{workspaceId}/files/{printFileId}/purge

Permanently remove source file data. Owner-only. This is optional and should be confirm-gated in the UI.

---

## Orders

### GET /workspaces/{workspaceId}/orders

List synced Etsy orders.

**Query params:**

- `status`
- `preparationStatus`
- `search`
- `from`
- `to`

**Response:** `200 OK`

```json
{
  "orders": [
    {
      "id": "order_001",
      "externalOrderId": "etsy_order_98765",
      "customerName": "Mia Chen",
      "status": "Open",
      "orderedAt": "2026-05-20T15:30:00Z",
      "dueBy": "2026-05-23T23:59:59Z",
      "items": [
        {
          "id": "item_001",
          "productId": "prod_001",
          "productName": "Custom Name Sign",
          "quantity": 2,
          "personalization": { "name": "Mia" },
          "preparationStatus": "NeedsPersonalization"
        }
      ]
    }
  ]
}
```

### GET /workspaces/{workspaceId}/orders/{orderId}

Get order details, mapped products, personalization data, and bundle history.

### PUT /workspaces/{workspaceId}/orders/{orderId}/items/{itemId}

Resolve or adjust item mapping when Etsy data does not match a known product.

**Request:**

```json
{
  "productId": "prod_001",
  "personalization": { "name": "Mia" }
}
```

---

## Preparation Bundles

### POST /workspaces/{workspaceId}/orders/{orderId}/preparation-bundles

Generate or refresh the downloadable file bundle for an order.

**Response:** `201 Created`

```json
{
  "bundleId": "bundle_123",
  "status": "ReadyToDownload",
  "items": [
    {
      "productName": "Custom Name Sign",
      "partName": "Sign Base",
      "fileName": "sign-base-v2.3mf",
      "quantity": 2,
      "requiresManualCustomization": true,
      "notes": "Set customer name to Mia before printing."
    }
  ]
}
```

### POST /workspaces/{workspaceId}/preparation-bundles

Create a manual bundle from selected products and quantities.

**Request:**

```json
{
  "items": [
    {
      "productId": "prod_001",
      "quantity": 3,
      "personalization": { "name": "Sample" }
    }
  ]
}
```

### GET /workspaces/{workspaceId}/preparation-bundles/{bundleId}

Get bundle manifest, blocking issues, and file list.

### GET /workspaces/{workspaceId}/preparation-bundles/{bundleId}/download

Download a ZIP archive containing source/generated files and `manifest.json`.

### PUT /workspaces/{workspaceId}/preparation-bundles/{bundleId}/status

Update bundle status after download or printing.

**Request:**

```json
{
  "status": "Printed"
}
```

---

## Insights

### GET /workspaces/{workspaceId}/insights/dashboard

Dashboard summary for Phase 1: open orders, products missing files, ready bundles, recent downloads, and Etsy sync health.

---

## Later Phase Printer APIs

Printer registration, queue submission, live job progress, pause/resume, and Bambu/OctoEverywhere adapter APIs are intentionally out of Phase 1. Keep old printer-first API ideas in later-phase issues, not in Phase 1 implementation work.

---

## Webhooks

### POST /webhooks/etsy

Etsy webhook endpoint for order updates. The handler validates Etsy signatures, stores the raw payload, and queues workspace-scoped sync work.

---

## Error Responses

All errors follow a consistent format:

```json
{
  "error": {
    "code": "PRODUCT_NOT_FOUND",
    "message": "Product with ID prod_999 was not found",
    "details": {
      "productId": "prod_999"
    }
  }
}
```

| HTTP Status | Error Code | Meaning |
|-------------|------------|---------|
| 400 | VALIDATION_ERROR | Invalid request body |
| 401 | UNAUTHORIZED | Missing or invalid token |
| 403 | FORBIDDEN | Access denied to resource |
| 404 | NOT_FOUND | Resource not found |
| 409 | CONFLICT | Resource already exists |
| 413 | FILE_TOO_LARGE | Uploaded file exceeds limit |
| 415 | UNSUPPORTED_FILE_TYPE | File type is not allowed |
| 429 | RATE_LIMITED | Too many requests |
| 500 | INTERNAL_ERROR | Server error |

---

## Rate Limits

| Tier | Requests/minute | Notes |
|------|-----------------|-------|
| Free | 60 | |
| Pro | 300 | |
| Enterprise | 1000 | |

Rate limit headers returned:

```text
X-RateLimit-Limit: 60
X-RateLimit-Remaining: 45
X-RateLimit-Reset: 1705312800
```
