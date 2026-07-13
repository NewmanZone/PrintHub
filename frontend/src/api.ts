export interface EtsyConnection {
  shopId: string
  externalId?: string
  shopName: string
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

const API_BASE = (import.meta.env.VITE_API_BASE_URL ?? '').replace(/\/$/, '')

interface AuthWorkspace {
  id: string
  name: string
  role: string
}

interface AuthMeResponse {
  workspaces: AuthWorkspace[]
}

interface ShopRecord {
  id: string
  externalId?: string
  shopName: string
  lastSyncAt?: string | null
}

let activeWorkspaceId: string | null = null
let activeWorkspaceRequest: Promise<string> | null = null

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

async function currentWorkspaceId() {
  if (activeWorkspaceId) return activeWorkspaceId
  if (activeWorkspaceRequest) return activeWorkspaceRequest

  activeWorkspaceRequest = request<AuthMeResponse>('/auth/me').then((me) => {
    const workspace = me.workspaces[0]
    if (!workspace) throw new Error('No workspace is available for the current user.')
    activeWorkspaceId = workspace.id
    return activeWorkspaceId
  }).finally(() => {
    activeWorkspaceRequest = null
  })
  return activeWorkspaceRequest
}

async function refreshWorkspaceId() {
  const me = await request<AuthMeResponse>('/auth/me')
  const workspace = me.workspaces[0]
  if (!workspace) throw new Error('No workspace is available for the current user.')
  activeWorkspaceId = workspace.id
  return activeWorkspaceId
}

export const api = {
  async getEtsyConnection() {
    const workspaceId = await currentWorkspaceId()
    const response = await request<{ shops: ShopRecord[] }>(`/workspaces/${workspaceId}/shops`)
    const shop = response.shops[0]
    return shop
      ? {
          shopId: shop.id,
          externalId: shop.externalId,
          shopName: shop.shopName,
          lastSyncAt: shop.lastSyncAt,
        }
      : null
  },

  async getEtsyConnectUrl(returnUrl = `${window.location.origin}/settings?etsy=connected`) {
    const workspaceId = await currentWorkspaceId()
    return request<{ authUrl: string }>(`/workspaces/${workspaceId}/shops/connect/etsy`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ returnUrl }),
    })
  },

  async syncEtsy() {
    const workspaceId = await currentWorkspaceId()
    const connection = await this.getEtsyConnection()
    if (!connection) throw new Error('Connect an Etsy shop before syncing.')
    return request<EtsySyncResponse>(`/workspaces/${workspaceId}/shops/${connection.shopId}/sync`, { method: 'POST' })
  },

  async getProducts() {
    const workspaceId = await currentWorkspaceId()
    return request<{ products: ProductRecord[] }>(`/workspaces/${workspaceId}/products`)
  },

  async getProduct(productId: string) {
    const workspaceId = await currentWorkspaceId()
    return request<ProductRecord>(`/workspaces/${workspaceId}/products/${productId}`)
  },

  async getProductFiles(productId: string) {
    const workspaceId = await currentWorkspaceId()
    return request<{ files: ProductFileRecord[] }>(`/workspaces/${workspaceId}/products/${productId}/files`)
  },

  async uploadProductFile(productId: string, file: File) {
    const workspaceId = await currentWorkspaceId()
    const form = new FormData()
    form.append('file', file)
    return request<ProductFileRecord>(`/workspaces/${workspaceId}/products/${productId}/files`, {
      method: 'POST',
      body: form,
    })
  },

  fileDownloadUrl(fileId: string) {
    const workspaceId = activeWorkspaceId
    if (!workspaceId) return '#'
    return `${API_BASE}/workspaces/${workspaceId}/files/${fileId}/download`
  },

  async refreshWorkspace() {
    return refreshWorkspaceId()
  },
}
