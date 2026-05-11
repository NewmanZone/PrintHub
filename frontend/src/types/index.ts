// ============================================================
// PrintHub Type Definitions
// Based on: DESIGN/data-model.md
// ============================================================

// User and Shop
export interface User {
  id: string;
  email: string;
  displayName: string;
  createdAt: string;
}

export interface Shop {
  id: string;
  userId: string;
  provider: 'etsy' | 'standalone';
  externalId: string;
  shopName: string;
  isActive: boolean;
  lastSyncAt: string | null;
}

// Products
export interface ProductPart {
  id: string;
  partId: string;
  partName: string;
  isGeneric: boolean;
  quantityPerProduct: number;
}

export interface Product {
  id: string;
  shopId: string;
  externalListingId: string | null;
  name: string;
  description: string | null;
  etsyPrice: number | null;
  imageUrl: string | null;
  isActive: boolean;
  printCount: number;
  inventoryOnHand: number;
  reorderPoint: number | null;
  reorderQuantity: number | null;
  costPerPrint: number | null;
  createdAt: string;
  updatedAt: string;
  parts: ProductPart[];
}

// Parts
export interface PrintFileVersion {
  id: string;
  printFileId: string;
  versionNumber: number;
  filePath: string;
  fileHash: string;
  thumbnailPath: string | null;
  uploadedAt: string;
  notes: string | null;
}

export interface PrintFile {
  id: string;
  partId: string;
  fileName: string;
  fileType: string;
  fileSizeBytes: number;
  currentVersionNumber: number;
  isDeleted: boolean;
  createdAt: string;
  updatedAt: string;
  versions: PrintFileVersion[];
}

export interface Part {
  id: string;
  shopId: string;
  name: string;
  description: string | null;
  isGeneric: boolean;
  currentVersionId: string | null;
  costPerUnit: number;
  inventoryOnHand: number;
  createdAt: string;
  updatedAt: string;
  printFileVersions?: PrintFileVersion[];
}

// Print Jobs
export type PrintJobStatus = 
  | 'Pending'
  | 'Queued'
  | 'InProgress'
  | 'Completed'
  | 'Failed'
  | 'Cancelled';

export type PrintJobItemStatus = 
  | 'Pending'
  | 'Printing'
  | 'Completed'
  | 'Failed';

export interface PrintJobItem {
  id: string;
  printJobId: string;
  partId: string;
  partName: string;
  printFileVersionId: string;
  quantity: number;
  status: PrintJobItemStatus;
  bambuTaskId: string | null;
  notes: string | null;
}

export interface PrintJob {
  id: string;
  userId: string;
  shopId: string;
  status: PrintJobStatus;
  printerTarget: string | null;
  createdAt: string;
  startedAt: string | null;
  completedAt: string | null;
  estimatedMinutes: number | null;
  notes: string | null;
  items: PrintJobItem[];
}

// Printers
export type PrinterType = 'bambu' | 'klipper' | 'octoprint';
export type PrinterStatus = 'online' | 'offline' | 'printing' | 'error';

export interface Printer {
  id: string;
  name: string;
  type: PrinterType;
  model: string;
  serialNumber: string | null;
  status: PrinterStatus;
  currentJobId: string | null;
  chamberTemp: number | null;
  bedTemp: number | null;
  progress: number | null;
  nozzleTemp: number | null;
  createdAt: string;
  updatedAt: string;
}

// Orders
export type PersonalizedOrderStatus =
  | 'Received'
  | 'InPreparation'
  | 'QueuedForPrint'
  | 'Printed'
  | 'Shipped';

export interface PersonalizedOrder {
  id: string;
  shopId: string;
  etsyOrderId: string | null;
  etsyListingId: string | null;
  customerName: string | null;
  personalizationData: Record<string, unknown>;
  status: PersonalizedOrderStatus;
  dueBy: string | null;
  notes: string | null;
  printJobId: string | null;
  createdAt: string;
}

// Inventory
export interface InventoryMovement {
  id: string;
  shopId: string;
  productId: string | null;
  partId: string;
  quantityChange: number;
  reason: 'Printed' | 'Sold' | 'Adjusted' | 'Deleted';
  reference: string | null;
  createdAt: string;
}

// API Response Types
export interface PaginatedResponse<T> {
  data: T[];
  pagination: {
    page: number;
    pageSize: number;
    totalCount: number;
    totalPages: number;
  };
}

export interface ApiError {
  code: string;
  message: string;
  details?: unknown;
}

// Workspace Types
export interface WorkspaceAlert {
  id: string;
  type: 'warning' | 'info' | 'success';
  title: string;
  message: string;
  action?: {
    label: string;
    href: string;
  };
  createdAt: string;
}

export interface DashboardStats {
  totalProducts: number;
  activeOrders: number;
  printQueueItems: number;
  lowStockCount: number;
  printersOnline: number;
  revenueThisMonth: number;
}