export interface EtsyConnection {
  shopId: string
  externalId?: string
  shopName: string
  expiresAt?: string
  connectedAt?: string
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
  jobId?: string
  status?: string
  imported?: number
  updated?: number
  total?: number
  syncedAt?: string
}

interface WorkspaceSummary {
  id: string
  name: string
  role: string
}

interface AuthMeResponse {
  workspaces: WorkspaceSummary[]
}

interface ShopRecord {
  id: string
  provider: string
  externalId: string
  shopName: string
  isActive: boolean
  lastSyncAt?: string | null
}

const API_BASE = (import.meta.env.VITE_API_BASE_URL ?? '').replace(/\/$/, '')

async function request<T>(path: string, init?: RequestInit): Promise<T> {
  const response = await fetch(`${API_BASE}${path}`, init)
  const text = await response.text()
  if (!response.ok) {
    let message = `Request failed with ${response.status}`
    try {
      const body = text ? JSON.parse(text) : null
      message = body.error ?? body.message ?? message
    } catch {
      // Keep status message.
    }
    throw new Error(message)
  }
  return (text ? JSON.parse(text) : null) as T
}

async function getWorkspaceId() {
  const configured = import.meta.env.VITE_WORKSPACE_ID
  if (configured) return configured

  const current = await request<AuthMeResponse>('/auth/me')
  const workspace = current.workspaces[0]
  if (!workspace) throw new Error('No workspace is available for the current user.')
  return workspace.id
}

export const api = {
  async getEtsyConnection() {
    const workspaceId = await getWorkspaceId()
    const response = await request<{ shops: ShopRecord[] }>(`/workspaces/${workspaceId}/shops`)
    const shop = response.shops[0]
    return shop
      ? { shopId: shop.id, externalId: shop.externalId, shopName: shop.shopName, lastSyncAt: shop.lastSyncAt }
      : null
  },

  async getEtsyConnectUrl(returnUrl = `${window.location.origin}/settings?etsy=connected`) {
    const workspaceId = await getWorkspaceId()
    return request<{ authUrl: string }>(`/workspaces/${workspaceId}/shops/connect/etsy`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ returnUrl }),
    })
  },

  async syncEtsy() {
    const workspaceId = await getWorkspaceId()
    const connection = await this.getEtsyConnection()
    if (!connection) throw new Error('Connect an Etsy shop before syncing.')
    return request<EtsySyncResponse>(`/workspaces/${workspaceId}/shops/${connection.shopId}/sync`, { method: 'POST' })
  },

  async getProducts() {
    const workspaceId = await getWorkspaceId()
    return request<{ products: ProductRecord[] }>(`/workspaces/${workspaceId}/products`)
  },

  async getProduct(productId: string) {
    const workspaceId = await getWorkspaceId()
    return request<ProductRecord>(`/workspaces/${workspaceId}/products/${productId}`)
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
