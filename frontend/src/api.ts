export interface EtsyConnection {
  shopId: string
  shopName: string
  expiresAt: string
  connectedAt: string
  lastSyncAt?: string | null
}

export interface ProductRecord {
  id: string
  externalListingId: string
  name: string
  description?: string | null
  etsyPrice?: number | null
  imageUrl?: string | null
  isActive: boolean
  updatedAt: string
}

export interface ProductFileRecord {
  id: string
  productId: string
  fileName: string
  fileType: string
  fileSizeBytes: number
  versionNumber: number
  uploadedAt: string
}

export interface EtsySyncResponse {
  imported: number
  updated: number
  total: number
  syncedAt: string
}

const API_BASE = (import.meta.env.VITE_API_BASE_URL ?? '').replace(/\/$/, '')

async function request<T>(path: string, init?: RequestInit): Promise<T> {
  const response = await fetch(`${API_BASE}${path}`, init)
  if (!response.ok) {
    let message = `Request failed with ${response.status}`
    try {
      const body = await response.json()
      message = body.error ?? body.message ?? message
    } catch {
      // Keep status message.
    }
    throw new Error(message)
  }
  return response.json() as Promise<T>
}

export const api = {
  async getEtsyConnection() {
    return request<EtsyConnection | null>('/api/etsy/connection')
  },

  async getEtsyConnectUrl(returnUrl = `${window.location.origin}/settings?etsy=connected`) {
    return request<{ authUrl: string }>(`/api/etsy/connect?returnUrl=${encodeURIComponent(returnUrl)}`)
  },

  async syncEtsy() {
    return request<EtsySyncResponse>('/api/etsy/sync', { method: 'POST' })
  },

  async getProducts() {
    return request<{ products: ProductRecord[] }>('/api/products')
  },

  async getProduct(productId: string) {
    return request<ProductRecord>(`/api/products/${productId}`)
  },

  async getProductFiles(productId: string) {
    return request<{ files: ProductFileRecord[] }>(`/api/products/${productId}/files`)
  },

  async uploadProductFile(productId: string, file: File) {
    const form = new FormData()
    form.append('file', file)
    return request<ProductFileRecord>(`/api/products/${productId}/files`, {
      method: 'POST',
      body: form,
    })
  },

  fileDownloadUrl(fileId: string) {
    return `${API_BASE}/api/files/${fileId}/download`
  },
}
