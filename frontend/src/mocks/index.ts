// Mock data layer so pages render without a backend API.

export interface MockProduct {
  id: string
  name: string
  externalListingId: string
  etsyPrice: number
  imageUrl: string
  printCount: number
  inventoryOnHand: number
  reorderPoint: number
  reorderQuantity: number
  costPerPrint: number
  salesVelocity: number
  parts: { partId: string; partName: string; isGeneric: boolean; quantityPerProduct: number }[]
}

export interface MockPart {
  id: string
  name: string
  isGeneric: boolean
  currentVersionId: string
  currentVersionNumber: number
  costPerUnit: number
  inventoryOnHand: number
  inventoryValue: number
  updatedAt: string
}

export type JobStatus = 'Draft' | 'Pending' | 'Queued' | 'InProgress' | 'Paused' | 'Completed' | 'Failed' | 'Cancelled'

export interface MockJob {
  id: string
  status: JobStatus
  printerTarget: string
  createdAt: string
  startedAt?: string
  estimatedCompletionAt?: string
  items: { partId: string; partName: string; quantity: number; status: JobStatus }[]
  progressPercent?: number
  notes?: string
}

export interface MockPrinter {
  id: string
  name: string
  type: 'Bambu' | 'Klipper'
  model: string
  serialNumber?: string
  printerUrl?: string
  status: 'Online' | 'Offline' | 'Busy'
  currentJobId?: string
  isDefault: boolean
  bedTemp?: number
  nozzleTemp?: number
}

export interface MockQueueItem {
  productId: string
  productName: string
  quantity: number
  partsBreakdown: string
  estimatedMinutes: number
  status: 'Pending' | 'Printing' | 'Completed'
}

export interface MockOrder {
  id: string
  etsyOrderId: string
  productId: string
  productName: string
  customerName: string
  status: string
  orderedAt: string
  dueBy: string
}

export type BundleStatus = 'ReadyToDownload' | 'Downloaded' | 'Printed' | 'Blocked'

export interface MockBundle {
  id: string
  orderId: string
  customerName: string
  status: BundleStatus
  fileCount: number
  itemCount: number
  createdAt: string
  notes: string
}

export const mockProducts: MockProduct[] = [
  {
    id: 'prod_001',
    name: 'Dino Wall Hook',
    externalListingId: 'etsy_listing_12345',
    etsyPrice: 24.99,
    imageUrl: 'https://placehold.co/120x120?text=Dino',
    printCount: 47,
    inventoryOnHand: 3,
    reorderPoint: 6,
    reorderQuantity: 10,
    costPerPrint: 0.45,
    salesVelocity: 11,
    parts: [
      { partId: 'part_001', partName: 'Basic Wall Hook', isGeneric: true, quantityPerProduct: 1 },
      { partId: 'part_002', partName: 'Dino Character', isGeneric: false, quantityPerProduct: 1 },
    ],
  },
  {
    id: 'prod_002',
    name: 'Cat Wall Hook',
    externalListingId: 'etsy_listing_12346',
    etsyPrice: 22.99,
    imageUrl: 'https://placehold.co/120x120?text=Cat',
    printCount: 12,
    inventoryOnHand: 8,
    reorderPoint: 5,
    reorderQuantity: 15,
    costPerPrint: 0.4,
    salesVelocity: 6,
    parts: [
      { partId: 'part_001', partName: 'Basic Wall Hook', isGeneric: true, quantityPerProduct: 1 },
      { partId: 'part_003', partName: 'Cat Character', isGeneric: false, quantityPerProduct: 1 },
    ],
  },
  {
    id: 'prod_003',
    name: 'Bear Wall Hook',
    externalListingId: 'etsy_listing_12347',
    etsyPrice: 24.99,
    imageUrl: 'https://placehold.co/120x120?text=Bear',
    printCount: 29,
    inventoryOnHand: 1,
    reorderPoint: 5,
    reorderQuantity: 12,
    costPerPrint: 0.48,
    salesVelocity: 9,
    parts: [
      { partId: 'part_001', partName: 'Basic Wall Hook', isGeneric: true, quantityPerProduct: 1 },
      { partId: 'part_004', partName: 'Bear Character', isGeneric: false, quantityPerProduct: 1 },
    ],
  },
]

export const mockParts: MockPart[] = [
  {
    id: 'part_001',
    name: 'Basic Wall Hook',
    isGeneric: true,
    currentVersionId: 'ver_003',
    currentVersionNumber: 3,
    costPerUnit: 0.15,
    inventoryOnHand: 12,
    inventoryValue: 1.8,
    updatedAt: '2026-05-10T00:00:00Z',
  },
  {
    id: 'part_002',
    name: 'Dino Character',
    isGeneric: false,
    currentVersionId: 'ver_010',
    currentVersionNumber: 1,
    costPerUnit: 0.3,
    inventoryOnHand: 0,
    inventoryValue: 0,
    updatedAt: '2026-05-08T00:00:00Z',
  },
  {
    id: 'part_003',
    name: 'Cat Character',
    isGeneric: false,
    currentVersionId: 'ver_011',
    currentVersionNumber: 1,
    costPerUnit: 0.28,
    inventoryOnHand: 5,
    inventoryValue: 1.4,
    updatedAt: '2026-05-12T00:00:00Z',
  },
  {
    id: 'part_004',
    name: 'Bear Character',
    isGeneric: false,
    currentVersionId: 'ver_012',
    currentVersionNumber: 2,
    costPerUnit: 0.33,
    inventoryOnHand: 1,
    inventoryValue: 0.33,
    updatedAt: '2026-05-15T00:00:00Z',
  },
]

export const mockJobs: MockJob[] = [
  {
    id: 'job_001',
    status: 'InProgress',
    printerTarget: 'P1S - Office',
    createdAt: '2026-05-15T10:00:00Z',
    startedAt: '2026-05-15T10:05:00Z',
    estimatedCompletionAt: '2026-05-15T15:00:00Z',
    items: [
      { partId: 'part_001', partName: 'Basic Wall Hook', quantity: 10, status: 'InProgress' },
      { partId: 'part_002', partName: 'Dino Character', quantity: 5, status: 'Queued' },
    ],
    progressPercent: 35,
    notes: 'Consolidated from three Etsy orders.',
  },
  {
    id: 'job_002',
    status: 'Completed',
    printerTarget: 'P1S - Office',
    createdAt: '2026-05-12T08:00:00Z',
    startedAt: '2026-05-12T08:10:00Z',
    estimatedCompletionAt: '2026-05-12T13:00:00Z',
    items: [
      { partId: 'part_001', partName: 'Basic Wall Hook', quantity: 5, status: 'Completed' },
    ],
    progressPercent: 100,
  },
  {
    id: 'job_003',
    status: 'Paused',
    printerTarget: 'X1C - Studio',
    createdAt: '2026-05-16T09:20:00Z',
    startedAt: '2026-05-16T09:45:00Z',
    items: [
      { partId: 'part_004', partName: 'Bear Character', quantity: 4, status: 'Paused' },
    ],
    progressPercent: 62,
    notes: 'Paused for filament swap.',
  },
]

export const mockPrinters: MockPrinter[] = [
  {
    id: 'printer_001',
    name: 'P1S - Office',
    type: 'Bambu',
    model: 'P1S',
    serialNumber: '01P1234567890ABC',
    status: 'Busy',
    currentJobId: 'job_001',
    isDefault: true,
    bedTemp: 65,
    nozzleTemp: 215,
  },
  {
    id: 'printer_002',
    name: 'X1C - Studio',
    type: 'Bambu',
    model: 'X1 Carbon',
    serialNumber: '01X1234567890ABC',
    status: 'Online',
    isDefault: false,
    bedTemp: 32,
    nozzleTemp: 24,
  },
  {
    id: 'printer_003',
    name: 'Centauri Carbon - Lab',
    type: 'Klipper',
    model: 'Centauri Carbon',
    printerUrl: 'https://centauri.octoeverywhere.com',
    status: 'Offline',
    isDefault: false,
  },
]

export const mockQueue: MockQueueItem[] = [
  { productId: 'prod_001', productName: 'Dino Wall Hook', quantity: 5, partsBreakdown: 'Hook x5, Dino x5', estimatedMinutes: 150, status: 'Pending' },
  { productId: 'prod_002', productName: 'Cat Wall Hook', quantity: 3, partsBreakdown: 'Hook x3, Cat x3', estimatedMinutes: 120, status: 'Pending' },
  { productId: 'prod_003', productName: 'Bear Wall Hook', quantity: 4, partsBreakdown: 'Hook x4, Bear x4', estimatedMinutes: 132, status: 'Printing' },
]

export const mockOrders: MockOrder[] = [
  {
    id: 'order_001',
    etsyOrderId: 'etsy_order_98765',
    productId: 'prod_001',
    productName: 'Dino Wall Hook',
    customerName: 'Mike',
    status: 'NeedsFiles',
    orderedAt: '2026-05-14T15:30:00Z',
    dueBy: '2026-05-17T23:59:59Z',
  },
  {
    id: 'order_002',
    etsyOrderId: 'etsy_order_98766',
    productId: 'prod_003',
    productName: 'Bear Wall Hook',
    customerName: 'Riley',
    status: 'ReadyToDownload',
    orderedAt: '2026-05-15T09:00:00Z',
    dueBy: '2026-05-18T23:59:59Z',
  },
]

export const mockBundles: MockBundle[] = [
  {
    id: 'bundle_001',
    orderId: 'etsy_order_98766',
    customerName: 'Riley',
    status: 'ReadyToDownload',
    fileCount: 2,
    itemCount: 4,
    createdAt: '2026-05-15T10:15:00Z',
    notes: 'Bear wall hook source files and manifest are ready.',
  },
  {
    id: 'bundle_002',
    orderId: 'etsy_order_98764',
    customerName: 'Mia',
    status: 'Downloaded',
    fileCount: 1,
    itemCount: 2,
    createdAt: '2026-05-13T16:30:00Z',
    notes: 'Custom sign requires manual name placement before printing.',
  },
  {
    id: 'bundle_003',
    orderId: 'etsy_order_98762',
    customerName: 'Avery',
    status: 'Printed',
    fileCount: 3,
    itemCount: 3,
    createdAt: '2026-05-12T12:00:00Z',
    notes: 'Downloaded and marked printed by contributor.',
  },
]

export const mockDashboard = {
  thisMonth: {
    productsSold: 34,
    printJobs: 12,
    revenue: 849.66,
    printCost: 18.4,
  },
  vsLastMonth: {
    productsSoldChange: 0.12,
    printJobsChange: 0.0,
    revenueChange: 0.15,
    printCostChange: -0.08,
  },
  alerts: [
    {
      type: 'LowStock',
      severity: 'Warning',
      message: '3 products below reorder point',
      products: ['prod_001', 'prod_003'],
    },
  ],
}

export const getLowStockProducts = (products = mockProducts) =>
  products.filter((product) => product.inventoryOnHand < product.reorderPoint)

export const getProductById = (id: string | undefined) =>
  mockProducts.find((product) => product.id === id)

export const getJobById = (id: string | undefined) =>
  mockJobs.find((job) => job.id === id)
