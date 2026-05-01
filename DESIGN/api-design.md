# PrintHub - API Design

## Overview

The PrintHub API is a RESTful ASP.NET Core 8 Web API. All endpoints return JSON. Authentication uses JWT Bearer tokens.

**Base URL:** `https://api.printhub.example.com/v1`

---

## Authentication

### POST /auth/register
Register a new user account.

**Request:**
```json
{
  "email": "seller@example.com",
  "password": "SecureP@ssword123",
  "displayName": "Mike's 3D Prints"
}
```

**Response:** `201 Created`
```json
{
  "userId": "usr_a1b2c3d4",
  "email": "seller@example.com",
  "createdAt": "2024-01-15T10:30:00Z"
}
```

### POST /auth/login
Authenticate and receive tokens.

**Request:**
```json
{
  "email": "seller@example.com",
  "password": "SecureP@ssword123"
}
```

**Response:** `200 OK`
```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "rt_abcdef123456...",
  "expiresIn": 3600,
  "tokenType": "Bearer"
}
```

### POST /auth/refresh
Refresh an expired access token.

**Request:**
```json
{
  "refreshToken": "rt_abcdef123456..."
}
```

---

## Shops (Etsy Integration)

### GET /shops
List all connected shops for the authenticated user.

**Response:** `200 OK`
```json
{
  "shops": [
    {
      "id": "shop_123",
      "provider": "etsy",
      "externalId": "etsy_shop_98765",
      "shopName": "Mikes3DPrints",
      "isActive": true,
      "lastSyncAt": "2024-01-15T08:00:00Z"
    }
  ]
}
```

### POST /shops/connect/etsy
Initiate Etsy OAuth flow.

**Response:** `200 OK`
```json
{
  "authUrl": "https://www.etsy.com/oauth2/authorize?..."
}
```

### POST /shops/etsy/callback
Etsy OAuth callback (handled by frontend redirect).

**Request:** Query param `?code=etsy_auth_code`

**Response:** `200 OK`
```json
{
  "shopId": "shop_123",
  "shopName": "Mikes3DPrints",
  "connected": true
}
```

### DELETE /shops/{shopId}
Disconnect a shop.

**Response:** `204 No Content`

### POST /shops/{shopId}/sync
Manually trigger Etsy listing sync.

**Response:** `202 Accepted`
```json
{
  "jobId": "sync_job_456",
  "status": "Processing"
}
```

---

## Products

### GET /shops/{shopId}/products
List all products for a shop.

**Query params:**
- `?page=1&pageSize=20` — pagination
- `?search=dino` — search by name
- `?belowReorderPoint=true` — filter to low stock

**Response:** `200 OK`
```json
{
  "products": [
    {
      "id": "prod_001",
      "name": "Dino Wall Hook",
      "externalListingId": "etsy_listing_12345",
      "etsyPrice": 24.99,
      "imageUrl": "https://...",
      "printCount": 47,
      "inventoryOnHand": 3,
      "reorderPoint": 6,
      "reorderQuantity": 10,
      "costPerPrint": 0.45,
      "parts": [
        {
          "partId": "part_001",
          "partName": "Basic Wall Hook",
          "isGeneric": true,
          "quantityPerProduct": 1
        },
        {
          "partId": "part_002",
          "partName": "Dino Character",
          "isGeneric": false,
          "quantityPerProduct": 1
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

### GET /shops/{shopId}/products/{productId}
Get a single product with full details.

**Response:** `200 OK`
```json
{
  "id": "prod_001",
  "name": "Dino Wall Hook",
  "description": "Adorable dinosaur wall hook...",
  "externalListingId": "etsy_listing_12345",
  "etsyPrice": 24.99,
  "imageUrl": "https://...",
  "printCount": 47,
  "inventoryOnHand": 3,
  "reorderPoint": 6,
  "reorderQuantity": 10,
  "costPerPrint": 0.45,
  "parts": [...],
  "versions": [
    {
      "partId": "part_001",
      "partName": "Basic Wall Hook",
      "versions": [
        { "versionId": "ver_003", "versionNumber": 3, "uploadedAt": "2024-01-10", "isCurrent": true },
        { "versionId": "ver_002", "versionNumber": 2, "uploadedAt": "2023-12-15", "isCurrent": false },
        { "versionId": "ver_001", "versionNumber": 1, "uploadedAt": "2023-11-20", "isCurrent": false }
      ]
    }
  ],
  "printHistory": [
    { "printedAt": "2024-01-12", "quantity": 5, "status": "Completed" },
    { "printedAt": "2024-01-08", "quantity": 3, "status": "Completed" }
  ]
}
```

### POST /shops/{shopId}/products
Create a new standalone product (not yet on Etsy).

**Request:**
```json
{
  "name": "Custom Keychain",
  "description": "Generic keychain design",
  "etsyPrice": 12.99,
  "reorderPoint": 10,
  "reorderQuantity": 25,
  "partIds": ["part_005", "part_006"]
}
```

**Response:** `201 Created`
```json
{
  "id": "prod_002",
  "name": "Custom Keychain",
  "createdAt": "2024-01-15T10:30:00Z"
}
```

### PUT /shops/{shopId}/products/{productId}
Update product settings.

### DELETE /shops/{shopId}/products/{productId}
Delete a product (soft delete).

---

## Parts

### GET /shops/{shopId}/parts
List all parts.

**Query params:**
- `?isGeneric=true` — only shared parts
- `?hasLowStock=true` — parts below inventory threshold

**Response:** `200 OK`
```json
{
  "parts": [
    {
      "id": "part_001",
      "name": "Basic Wall Hook",
      "isGeneric": true,
      "currentVersionId": "ver_003",
      "currentVersionNumber": 3,
      "costPerUnit": 0.15,
      "inventoryOnHand": 12,
      "inventoryValue": 1.80,
      "updatedAt": "2024-01-10T00:00:00Z"
    },
    {
      "id": "part_002",
      "name": "Dino Character",
      "isGeneric": false,
      "currentVersionId": "ver_010",
      "currentVersionNumber": 1,
      "costPerUnit": 0.30,
      "inventoryOnHand": 0,
      "updatedAt": "2023-11-20T00:00:00Z"
    }
  ]
}
```

### GET /shops/{shopId}/parts/{partId}
Get single part with version history.

### POST /shops/{shopId}/parts
Create a new part.

**Request:**
```json
{
  "name": "Cat Character",
  "description": "Cat face for wall hook",
  "isGeneric": false,
  "costPerUnit": 0.30
}
```

### POST /shops/{shopId}/parts/{partId}/files
Upload a new version of a print file.

**Request:** `multipart/form-data`
- `file`: the STL/3MF file
- `notes`: optional version notes

**Response:** `201 Created`
```json
{
  "versionId": "ver_011",
  "versionNumber": 2,
  "fileName": "cat_character_v2.stl",
  "fileSizeBytes": 1245184,
  "uploadedAt": "2024-01-15T10:35:00Z"
}
```

### PUT /shops/{shopId}/parts/{partId}/set-current-version/{versionId}
Set a specific version as the current active version.

---

## Print Queue

### GET /shops/{shopId}/queue
Get current print queue with consolidated view.

**Response:** `200 OK`
```json
{
  "queueItems": [
    {
      "productId": "prod_001",
      "productName": "Dino Wall Hook",
      "quantity": 5,
      "partsBreakdown": "Hook×5, Dino×5",
      "estimatedMinutes": 150,
      "status": "Pending"
    }
  ],
  "consolidatedParts": [
    {
      "partId": "part_001",
      "partName": "Basic Wall Hook",
      "toPrint": 10,
      "onHand": 12,
      "netAfter": 2,
      "status": "Ready"
    },
    {
      "partId": "part_002",
      "partName": "Dino Character",
      "toPrint": 5,
      "onHand": 0,
      "netAfter": -5,
      "status": "Low"
    }
  ],
  "totalEstimatedMinutes": 300,
  "totalFilamentGrams": 180,
  "totalFilamentCost": 2.70
}
```

### POST /shops/{shopId}/queue/items
Add items to the print queue.

**Request:**
```json
{
  "items": [
    { "productId": "prod_001", "quantity": 5 },
    { "productId": "prod_002", "quantity": 3 }
  ]
}
```

**Response:** `201 Created`
```json
{
  "itemsAdded": 2,
  "consolidatedJobCount": 3,
  "estimatedTotalMinutes": 300
}
```

### POST /shops/{shopId}/queue/items/personalized
Add personalized items to queue (e.g., custom names).

**Request:**
```json
{
  "items": [
    {
      "productId": "prod_001",
      "quantity": 1,
      "personalizations": [
        { "customerName": "Mike", "data": { "name": "Mike" } },
        { "customerName": "Sarah", "data": { "name": "Sarah" } }
      ]
    }
  ]
}
```

### DELETE /shops/{shopId}/queue/items/{itemId}
Remove an item from queue.

### PUT /shops/{shopId}/queue/items/{itemId}
Update quantity on a queue item.

### POST /shops/{shopId}/queue/print
Start printing the queue.

**Request:**
```json
{
  "targetPrinterId": "printer_001",
  "options": {
    "notifyOnComplete": true
  }
}
```

**Response:** `202 Accepted`
```json
{
  "printJobId": "job_001",
  "itemsQueued": 3,
  "status": "Queued",
  "estimatedCompletionMinutes": 300
}
```

---

## Print Jobs

### GET /shops/{shopId}/jobs
List print jobs (history).

**Query params:**
- `?status=Completed`
- `?from=2024-01-01&to=2024-01-31`

**Response:** `200 OK`
```json
{
  "jobs": [
    {
      "id": "job_001",
      "status": "InProgress",
      "printerTarget": "P1S - Office",
      "createdAt": "2024-01-15T10:00:00Z",
      "startedAt": "2024-01-15T10:05:00Z",
      "estimatedCompletionAt": "2024-01-15T15:00:00Z",
      "items": [
        { "partId": "part_001", "partName": "Basic Wall Hook", "quantity": 10, "status": "Printing" },
        { "partId": "part_002", "partName": "Dino Character", "quantity": 5, "status": "Pending" }
      ],
      "progress": {
        "percentComplete": 35,
        "currentItem": "Basic Wall Hook (7/10)"
      }
    }
  ]
}
```

### GET /shops/{shopId}/jobs/{jobId}
Get job details.

### POST /shops/{shopId}/jobs/{jobId}/cancel
Cancel a running job.

### POST /shops/{shopId}/jobs/{jobId}/pause
Pause a running job (Bambu only).

### POST /shops/{shopId}/jobs/{jobId}/resume
Resume a paused job.

---

## Printers

### GET /shops/{shopId}/printers
List registered printers.

**Response:** `200 OK`
```json
{
  "printers": [
    {
      "id": "printer_001",
      "name": "P1S - Office",
      "type": "Bambu",
      "model": "P1S",
      "serialNumber": "01P1234567890ABC",
      "status": "Online",
      "currentJobId": "job_001",
      "isDefault": true
    },
    {
      "id": "printer_002",
      "name": "Centauri Carbon - Lab",
      "type": "Klipper",
      "model": "Centauri Carbon",
      "printerUrl": "https://xyz.octoanywhere.com",
      "status": "Online",
      "currentJobId": null,
      "isDefault": false
    }
  ]
}
```

### POST /shops/{shopId}/printers
Register a new printer.

**For Bambu:**
```json
{
  "type": "Bambu",
  "name": "P1S - Office",
  "serialNumber": "01P1234567890ABC",
  "accessCode": "ABC123"
}
```

**For OctoAnywhere/Klipper:**
```json
{
  "type": "Klipper",
  "name": "Centauri Carbon - Lab",
  "octoAnywhereUrl": "https://xyz.octoanywhere.com"
}
```

### DELETE /shops/{shopId}/printers/{printerId}
Unregister a printer.

### PUT /shops/{shopId}/printers/{printerId}
Update printer settings (name, default status).

---

## Insights & Analytics

### GET /shops/{shopId}/insights/dashboard
Dashboard summary data.

**Response:** `200 OK`
```json
{
  "thisMonth": {
    "productsSold": 34,
    "printJobs": 12,
    "revenue": 849.66,
    "printCost": 18.40
  },
  "vsLastMonth": {
    "productsSoldChange": 0.12,
    "printJobsChange": 0.0,
    "revenueChange": 0.15,
    "printCostChange": -0.08
  },
  "alerts": [
    {
      "type": "LowStock",
      "severity": "Warning",
      "message": "3 products below reorder point",
      "products": ["prod_001", "prod_002", "prod_003"],
      "actionLabel": "Print 30 more of each",
      "actionData": { "quantities": { "prod_001": 10, "prod_002": 10, "prod_003": 10 } }
    }
  ],
  "insights": [
    {
      "type": "SeasonalTrend",
      "message": "Heart products sell 3x better in February. Start building inventory in January.",
      "confidence": 0.85
    }
  ],
  "topPerformers": [
    { "productId": "prod_002", "productName": "Cat Wall Hook", "sold": 12, "revenue": 299.76 }
  ]
}
```

### GET /shops/{shopId}/insights/inventory
Detailed inventory report.

### GET /shops/{shopId}/insights/sales-velocity
Sales velocity analysis for reorder predictions.

---

## Personalized Orders (Etsy Sync)

### GET /shops/{shopId}/orders
Get Etsy orders with personalization data.

**Response:** `200 OK`
```json
{
  "orders": [
    {
      "id": "order_001",
      "etsyOrderId": "etsy_order_98765",
      "etsyListingId": "etsy_listing_12345",
      "productId": "prod_001",
      "productName": "Dino Wall Hook",
      "customerName": "Mike",
      "personalization": { "name": "Mike" },
      "status": "Received",
      "orderedAt": "2024-01-14T15:30:00Z",
      "dueBy": "2024-01-17T23:59:59Z"
    }
  ]
}
```

### POST /shops/{shopId}/orders/{orderId}/queue
Queue a personalized order for printing.

**Request:**
```json
{
  "targetPrinterId": "printer_001"
}
```

### PUT /shops/{shopId}/orders/{orderId}/status
Update order status (Received → InPreparation → QueuedForPrint → Printed → Shipped).

---

## Webhooks

### POST /webhooks/etsy
Etsy webhook endpoint for order updates.

**Etsy sends:** `listing_created`, `listing_updated`, `listing_deleted`, `order_created`

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
```
X-RateLimit-Limit: 60
X-RateLimit-Remaining: 45
X-RateLimit-Reset: 1705312800
```