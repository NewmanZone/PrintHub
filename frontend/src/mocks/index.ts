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

export interface MockJob {
  id: string
  status: 'Pending' | 'InProgress' | 'Completed' | 'Failed' | 'Cancelled'
  printerTarget: string
  createdAt: string
  startedAt?: string
  estimatedCompletionAt?: string
  items: { partId: string; partName: string; quantity: number; status: string }[]
  progressPercent?: number
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
    costPerPrint: 0.40,
    parts: [
      { partId: 'part_001', partName: 'Basic Wall Hook', isGeneric: true, quantityPerProduct: 1 },
      { partId: 'part_003', partName: 'Cat Character', isGeneric: false, quantityPerProduct: 1 },
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
    inventoryValue: 1.80,
    updatedAt: '2024-01-10T00:00:00Z',
  },
  {
    id: 'part_002',
    name: 'Dino Character',
    isGeneric: false,
    currentVersionId: 'ver_010',
    currentVersionNumber: 1,
    costPerUnit: 0.30,
    inventoryOnHand: 0,
    inventoryValue: 0.00,
    updatedAt: '2023-11-20T00:00:00Z',
  },
  {
    id: 'part_003',
    name: 'Cat Character',
    isGeneric: false,
    currentVersionId: 'ver_011',
    currentVersionNumber: 1,
    costPerUnit: 0.28,
    inventoryOnHand: 5,
    inventoryValue: 1.40,
    updatedAt: '2024-01-15T00:00:00Z',
  },
]

export const mockJobs: MockJob[] = [
  {
    id: 'job_001',
    status: 'InProgress',
    printerTarget: 'P1S - Office',
    createdAt: '2024-01-15T10:00:00Z',
    startedAt: '2024-01-15T10:05:00Z',
    estimatedCompletionAt: '2024-01-15T15:00:00Z',
    items: [
      { partId: 'part_001', partName: 'Basic Wall Hook', quantity: 10, status: 'Printing' },
      { partId: 'part_002', partName: 'Dino Character', quantity: 5, status: 'Pending' },
    ],
    progressPercent: 35,
  },
  {
    id: 'job_002',
    status: 'Completed',
    printerTarget: 'P1S - Office',
    createdAt: '2024-01-12T08:00:00Z',
    startedAt: '2024-01-12T08:10:00Z',
    estimatedCompletionAt: '2024-01-12T13:00:00Z',
    items: [
      { partId: 'part_001', partName: 'Basic Wall Hook', quantity: 5, status: 'Completed' },
    ],
    progressPercent: 100,
  },
]

export const mockPrinters: MockPrinter[] = [
  {
    id: 'printer_001',
    name: 'P1S - Office',
    type: 'Bambu',
    model: 'P1S',
    serialNumber: '01P1234567890ABC',
    status: 'Online',
    currentJobId: 'job_001',
    isDefault: true,
  },
  {
    id: 'printer_002',
    name: 'Centauri Carbon - Lab',
    type: 'Klipper',
    model: 'Centauri Carbon',
    printerUrl: 'https://xyz.octoanywhere.com',
    status: 'Online',
    isDefault: false,
  },
]

export const mockQueue: MockQueueItem[] = [
  { productId: 'prod_001', productName: 'Dino Wall Hook', quantity: 5, partsBreakdown: 'Hook×5, Dino×5', estimatedMinutes: 150, status: 'Pending' },
  { productId: 'prod_002', productName: 'Cat Wall Hook', quantity: 3, partsBreakdown: 'Hook×3, Cat×3', estimatedMinutes: 120, status: 'Pending' },
]

export const mockOrders: MockOrder[] = [
  {
    id: 'order_001',
    etsyOrderId: 'etsy_order_98765',
    productId: 'prod_001',
    productName: 'Dino Wall Hook',
    customerName: 'Mike',
    status: 'Received',
    orderedAt: '2024-01-14T15:30:00Z',
    dueBy: '2024-01-17T23:59:59Z',
  },
]

export const mockDashboard = {
  thisMonth: {
    productsSold: 34,
    printJobs: 12,
    revenue: 849.66,
    printCost: 18.40,
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
      products: ['prod_001', 'prod_002'],
    },
  ],
}
