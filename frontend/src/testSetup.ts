import '@testing-library/jest-dom'
import { beforeEach, vi } from 'vitest'

const products = [
  {
    id: 'prod_001',
    externalListingId: 'etsy_1001',
    name: 'Dino Wall Hook',
    description: 'A friendly dinosaur wall hook.',
    etsyPrice: 18.5,
    imageUrl: null,
    isActive: true,
    updatedAt: '2026-05-20T10:30:00.000Z',
  },
  {
    id: 'prod_002',
    externalListingId: 'etsy_1002',
    name: 'Cat Wall Hook',
    description: 'A cat-themed wall hook.',
    etsyPrice: 16,
    imageUrl: null,
    isActive: false,
    updatedAt: '2026-05-19T08:15:00.000Z',
  },
]

const files = [
  {
    id: 'file_001',
    productId: 'prod_001',
    fileName: 'dino-hook.3mf',
    fileType: '.3mf',
    fileSizeBytes: 2048,
    versionNumber: 1,
    uploadedAt: '2026-05-21T09:00:00.000Z',
  },
]

const jsonResponse = (body: unknown, status = 200) =>
  new Response(JSON.stringify(body), {
    status,
    headers: { 'Content-Type': 'application/json' },
  })

beforeEach(() => {
  vi.stubGlobal('fetch', vi.fn(async (input: RequestInfo | URL, init?: RequestInit) => {
    const url = new URL(String(input), 'http://localhost')

    if (url.pathname === '/api/etsy/connection') {
      return jsonResponse({
        shopId: '123456',
        shopName: 'Newman Zone',
        expiresAt: '2026-06-01T00:00:00.000Z',
        connectedAt: '2026-05-20T00:00:00.000Z',
        lastSyncAt: '2026-05-21T00:00:00.000Z',
      })
    }

    if (url.pathname === '/api/etsy/connect') {
      return jsonResponse({ authUrl: 'https://www.etsy.com/oauth/connect?state=test' })
    }

    if (url.pathname === '/api/etsy/sync' && init?.method === 'POST') {
      return jsonResponse({ imported: 1, updated: 1, total: 2, syncedAt: '2026-05-21T00:00:00.000Z' })
    }

    if (url.pathname === '/api/products') {
      return jsonResponse({ products })
    }

    const productMatch = url.pathname.match(/^\/api\/products\/([^/]+)$/)
    if (productMatch) {
      const product = products.find((candidate) => candidate.id === productMatch[1])
      return product ? jsonResponse(product) : jsonResponse({ error: 'Product not found.' }, 404)
    }

    const filesMatch = url.pathname.match(/^\/api\/products\/([^/]+)\/files$/)
    if (filesMatch && init?.method === 'POST') {
      return jsonResponse({
        id: 'file_002',
        productId: filesMatch[1],
        fileName: 'new-source.3mf',
        fileType: '.3mf',
        fileSizeBytes: 4096,
        versionNumber: 2,
        uploadedAt: '2026-05-22T09:00:00.000Z',
      })
    }

    if (filesMatch) {
      return jsonResponse({ files: files.filter((file) => file.productId === filesMatch[1]) })
    }

    return jsonResponse({ error: 'Unhandled test request.' }, 404)
  }))
})
