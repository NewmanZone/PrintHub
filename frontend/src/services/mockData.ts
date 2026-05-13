// ============================================================
// PrintHub Mock Data Service
// Simulates API responses for development
// ============================================================

import type {
  User,
  Shop,
  Product,
  Part,
  PrintJob,
  Printer,
  PersonalizedOrder,
  DashboardStats,
  WorkspaceAlert,
} from '../types';

// Mock current user
export const mockUser: User = {
  id: 'usr_mike123',
  email: 'mike@prints.example.com',
  displayName: "Mike's 3D Prints",
  createdAt: '2024-01-15T10:30:00Z',
};

// Mock shops
export const mockShops: Shop[] = [
  {
    id: 'shop_001',
    userId: 'usr_mike123',
    provider: 'etsy',
    externalId: 'etsy_shop_98765',
    shopName: "Mike's 3D Prints",
    isActive: true,
    lastSyncAt: '2025-05-10T14:00:00Z',
  },
  {
    id: 'shop_002',
    userId: 'usr_mike123',
    provider: 'standalone',
    externalId: 'local_001',
    shopName: 'Local Prints',
    isActive: true,
    lastSyncAt: null,
  },
];

// Mock printers
export const mockPrinters: Printer[] = [
  {
    id: 'printer_001',
    name: 'Bambu X1C',
    type: 'bambu',
    model: 'X1 Carbon',
    serialNumber: 'BML-X1C-12345',
    status: 'printing',
    currentJobId: 'job_ongoing_001',
    chamberTemp: 35,
    bedTemp: 55,
    progress: 67,
    nozzleTemp: 215,
    createdAt: '2024-02-01T09:00:00Z',
    updatedAt: '2025-05-10T16:00:00Z',
  },
  {
    id: 'printer_002',
    name: 'Bambu P1S',
    type: 'bambu',
    model: 'P1S',
    serialNumber: 'BML-P1S-67890',
    status: 'online',
    currentJobId: null,
    chamberTemp: 28,
    bedTemp: 45,
    progress: null,
    nozzleTemp: 24,
    createdAt: '2024-03-15T11:00:00Z',
    updatedAt: '2025-05-10T08:00:00Z',
  },
  {
    id: 'printer_003',
    name: 'Voron Printer',
    type: 'klipper',
    model: 'Voron 2.4',
    serialNumber: null,
    status: 'offline',
    currentJobId: null,
    chamberTemp: null,
    bedTemp: null,
    progress: null,
    nozzleTemp: null,
    createdAt: '2024-04-20T14:00:00Z',
    updatedAt: '2025-05-09T22:00:00Z',
  },
];

// Mock products
export const mockProducts: Product[] = [
  {
    id: 'prod_001',
    shopId: 'shop_001',
    externalListingId: 'etsy_listing_12345',
    name: 'Dino Wall Hook',
    description: 'Adorable dinosaur wall hook with vibrant colors',
    etsyPrice: 24.99,
    imageUrl: 'https://picsum.photos/seed/dino/400/400',
    isActive: true,
    printCount: 47,
    inventoryOnHand: 3,
    reorderPoint: 6,
    reorderQuantity: 10,
    costPerPrint: 0.45,
    createdAt: '2024-01-20T10:00:00Z',
    updatedAt: '2025-05-08T15:30:00Z',
    parts: [
      { id: 'pp_001', partId: 'part_001', partName: 'Basic Wall Hook', isGeneric: true, quantityPerProduct: 1 },
      { id: 'pp_002', partId: 'part_002', partName: 'Dino Character', isGeneric: false, quantityPerProduct: 1 },
    ],
  },
  {
    id: 'prod_002',
    shopId: 'shop_001',
    externalListingId: 'etsy_listing_12346',
    name: 'Cat Wall Hook',
    description: 'Cute cat wall hook for cat lovers',
    etsyPrice: 22.99,
    imageUrl: 'https://picsum.photos/seed/cat/400/400',
    isActive: true,
    printCount: 38,
    inventoryOnHand: 8,
    reorderPoint: 10,
    reorderQuantity: 15,
    costPerPrint: 0.42,
    createdAt: '2024-01-25T11:00:00Z',
    updatedAt: '2025-05-07T09:00:00Z',
    parts: [
      { id: 'pp_003', partId: 'part_001', partName: 'Basic Wall Hook', isGeneric: true, quantityPerProduct: 1 },
      { id: 'pp_004', partId: 'part_003', partName: 'Cat Character', isGeneric: false, quantityPerProduct: 1 },
    ],
  },
  {
    id: 'prod_003',
    shopId: 'shop_001',
    externalListingId: 'etsy_listing_12347',
    name: 'Bear Wall Hook',
    description: 'Friendly bear wall hook',
    etsyPrice: 24.99,
    imageUrl: 'https://picsum.photos/seed/bear/400/400',
    isActive: true,
    printCount: 29,
    inventoryOnHand: 5,
    reorderPoint: 8,
    reorderQuantity: 12,
    costPerPrint: 0.48,
    createdAt: '2024-02-01T14:00:00Z',
    updatedAt: '2025-05-06T16:00:00Z',
    parts: [
      { id: 'pp_005', partId: 'part_001', partName: 'Basic Wall Hook', isGeneric: true, quantityPerProduct: 1 },
      { id: 'pp_006', partId: 'part_004', partName: 'Bear Character', isGeneric: false, quantityPerProduct: 1 },
    ],
  },
  {
    id: 'prod_004',
    shopId: 'shop_001',
    externalListingId: 'etsy_listing_12348',
    name: 'Unicorn Wall Hook',
    description: 'Magical unicorn wall hook with rainbow colors',
    etsyPrice: 26.99,
    imageUrl: 'https://picsum.photos/seed/unicorn/400/400',
    isActive: true,
    printCount: 15,
    inventoryOnHand: 0,
    reorderPoint: 5,
    reorderQuantity: 8,
    costPerPrint: 0.52,
    createdAt: '2024-03-10T09:00:00Z',
    updatedAt: '2025-05-09T11:00:00Z',
    parts: [
      { id: 'pp_007', partId: 'part_001', partName: 'Basic Wall Hook', isGeneric: true, quantityPerProduct: 1 },
      { id: 'pp_008', partId: 'part_005', partName: 'Unicorn Character', isGeneric: false, quantityPerProduct: 1 },
    ],
  },
];

// Mock parts
export const mockParts: Part[] = [
  {
    id: 'part_001',
    shopId: 'shop_001',
    name: 'Basic Wall Hook',
    description: 'Standard hook base for all products',
    isGeneric: true,
    currentVersionId: 'ver_hook_001',
    costPerUnit: 0.15,
    inventoryOnHand: 16,
    createdAt: '2024-01-15T08:00:00Z',
    updatedAt: '2025-05-10T10:00:00Z',
  },
  {
    id: 'part_002',
    shopId: 'shop_001',
    name: 'Dino Character',
    description: 'Dinosaur character topper',
    isGeneric: false,
    currentVersionId: 'ver_dino_003',
    costPerUnit: 0.30,
    inventoryOnHand: 3,
    createdAt: '2024-01-15T08:30:00Z',
    updatedAt: '2025-05-08T15:30:00Z',
  },
  {
    id: 'part_003',
    shopId: 'shop_001',
    name: 'Cat Character',
    description: 'Cat character topper',
    isGeneric: false,
    currentVersionId: 'ver_cat_002',
    costPerUnit: 0.27,
    inventoryOnHand: 8,
    createdAt: '2024-01-20T09:00:00Z',
    updatedAt: '2025-05-07T09:00:00Z',
  },
  {
    id: 'part_004',
    shopId: 'shop_001',
    name: 'Bear Character',
    description: 'Bear character topper',
    isGeneric: false,
    currentVersionId: 'ver_bear_002',
    costPerUnit: 0.33,
    inventoryOnHand: 5,
    createdAt: '2024-02-01T10:00:00Z',
    updatedAt: '2025-05-06T16:00:00Z',
  },
  {
    id: 'part_005',
    shopId: 'shop_001',
    name: 'Unicorn Character',
    description: 'Unicorn character topper',
    isGeneric: false,
    currentVersionId: 'ver_unicorn_001',
    costPerUnit: 0.37,
    inventoryOnHand: 0,
    createdAt: '2024-03-10T09:00:00Z',
    updatedAt: '2025-05-09T11:00:00Z',
  },
];

// Mock print jobs
export const mockPrintJobs: PrintJob[] = [
  {
    id: 'job_ongoing_001',
    userId: 'usr_mike123',
    shopId: 'shop_001',
    status: 'InProgress',
    printerTarget: 'printer_001',
    createdAt: '2025-05-10T14:30:00Z',
    startedAt: '2025-05-10T14:45:00Z',
    completedAt: null,
    estimatedMinutes: 180,
    notes: '5x Dino Wall Hook batch print',
    items: [
      {
        id: 'ji_001',
        printJobId: 'job_ongoing_001',
        partId: 'part_001',
        partName: 'Basic Wall Hook',
        printFileVersionId: 'ver_hook_001',
        quantity: 5,
        status: 'Printing',
        bambuTaskId: 'bambu_task_abc123',
        notes: null,
      },
      {
        id: 'ji_002',
        printJobId: 'job_ongoing_001',
        partId: 'part_002',
        partName: 'Dino Character',
        printFileVersionId: 'ver_dino_003',
        quantity: 5,
        status: 'Printing',
        bambuTaskId: 'bambu_task_abc123',
        notes: null,
      },
    ],
  },
  {
    id: 'job_queued_001',
    userId: 'usr_mike123',
    shopId: 'shop_001',
    status: 'Queued',
    printerTarget: 'printer_002',
    createdAt: '2025-05-10T15:00:00Z',
    startedAt: null,
    completedAt: null,
    estimatedMinutes: 120,
    notes: '3x Cat Wall Hook',
    items: [
      {
        id: 'ji_003',
        printJobId: 'job_queued_001',
        partId: 'part_001',
        partName: 'Basic Wall Hook',
        printFileVersionId: 'ver_hook_001',
        quantity: 3,
        status: 'Pending',
        bambuTaskId: null,
        notes: null,
      },
      {
        id: 'ji_004',
        printJobId: 'job_queued_001',
        partId: 'part_003',
        partName: 'Cat Character',
        printFileVersionId: 'ver_cat_002',
        quantity: 3,
        status: 'Pending',
        bambuTaskId: null,
        notes: null,
      },
    ],
  },
  {
    id: 'job_pending_001',
    userId: 'usr_mike123',
    shopId: 'shop_001',
    status: 'Pending',
    printerTarget: null,
    createdAt: '2025-05-10T16:00:00Z',
    startedAt: null,
    completedAt: null,
    estimatedMinutes: 90,
    notes: '8x Unicorn Wall Hook - low stock order',
    items: [
      {
        id: 'ji_005',
        printJobId: 'job_pending_001',
        partId: 'part_001',
        partName: 'Basic Wall Hook',
        printFileVersionId: 'ver_hook_001',
        quantity: 8,
        status: 'Pending',
        bambuTaskId: null,
        notes: null,
      },
      {
        id: 'ji_006',
        printJobId: 'job_pending_001',
        partId: 'part_005',
        partName: 'Unicorn Character',
        printFileVersionId: 'ver_unicorn_001',
        quantity: 8,
        status: 'Pending',
        bambuTaskId: null,
        notes: 'Need to print - inventory is 0',
      },
    ],
  },
  {
    id: 'job_completed_001',
    userId: 'usr_mike123',
    shopId: 'shop_001',
    status: 'Completed',
    printerTarget: 'printer_002',
    createdAt: '2025-05-09T10:00:00Z',
    startedAt: '2025-05-09T10:15:00Z',
    completedAt: '2025-05-09T12:30:00Z',
    estimatedMinutes: 135,
    notes: '2x Bear Wall Hook',
    items: [
      {
        id: 'ji_007',
        printJobId: 'job_completed_001',
        partId: 'part_001',
        partName: 'Basic Wall Hook',
        printFileVersionId: 'ver_hook_001',
        quantity: 2,
        status: 'Completed',
        bambuTaskId: 'bambu_task_xyz789',
        notes: null,
      },
      {
        id: 'ji_008',
        printJobId: 'job_completed_001',
        partId: 'part_004',
        partName: 'Bear Character',
        printFileVersionId: 'ver_bear_002',
        quantity: 2,
        status: 'Completed',
        bambuTaskId: 'bambu_task_xyz789',
        notes: null,
      },
    ],
  },
];

// Mock orders
export const mockOrders: PersonalizedOrder[] = [
  {
    id: 'ord_001',
    shopId: 'shop_001',
    etsyOrderId: 'etsy_order_55555',
    etsyListingId: 'etsy_listing_12345',
    customerName: 'Sarah Johnson',
    personalizationData: { name: 'Sophie', color: 'pink' },
    status: 'QueuedForPrint',
    dueBy: '2025-05-15T23:59:59Z',
    notes: 'Rush order - birthday gift',
    printJobId: 'job_pending_001',
    createdAt: '2025-05-10T11:00:00Z',
  },
  {
    id: 'ord_002',
    shopId: 'shop_001',
    etsyOrderId: 'etsy_order_55556',
    etsyListingId: 'etsy_listing_12346',
    customerName: 'Mike Thompson',
    personalizationData: { name: 'Max', color: 'blue' },
    status: 'InPreparation',
    dueBy: '2025-05-20T23:59:59Z',
    notes: null,
    printJobId: null,
    createdAt: '2025-05-10T09:00:00Z',
  },
  {
    id: 'ord_003',
    shopId: 'shop_001',
    etsyOrderId: 'etsy_order_55557',
    etsyListingId: 'etsy_listing_12347',
    customerName: 'Emily Chen',
    personalizationData: { name: 'Luna', color: 'purple' },
    status: 'Received',
    dueBy: '2025-05-25T23:59:59Z',
    notes: 'First-time customer',
    printJobId: null,
    createdAt: '2025-05-10T07:30:00Z',
  },
  {
    id: 'ord_004',
    shopId: 'shop_001',
    etsyOrderId: 'etsy_order_55554',
    etsyListingId: 'etsy_listing_12345',
    customerName: 'James Wilson',
    personalizationData: { name: 'Buddy', color: 'green' },
    status: 'Printed',
    dueBy: '2025-05-08T23:59:59Z',
    notes: 'Ship by end of week',
    printJobId: 'job_completed_002',
    createdAt: '2025-05-05T14:00:00Z',
  },
  {
    id: 'ord_005',
    shopId: 'shop_001',
    etsyOrderId: 'etsy_order_55553',
    etsyListingId: 'etsy_listing_12348',
    customerName: 'Amanda Lee',
    personalizationData: { name: 'Stella', color: 'rainbow' },
    status: 'Shipped',
    dueBy: '2025-05-05T23:59:59Z',
    notes: 'Tracking: 1Z999AA10123456784',
    printJobId: 'job_completed_003',
    createdAt: '2025-04-30T10:00:00Z',
  },
];

// Mock dashboard stats
export const mockDashboardStats: DashboardStats = {
  totalProducts: 4,
  activeOrders: 3,
  printQueueItems: 16,
  lowStockCount: 2,
  printersOnline: 2,
  revenueThisMonth: 1247.50,
};

// Mock workspace alerts
export const mockAlerts: WorkspaceAlert[] = [
  {
    id: 'alert_001',
    type: 'warning',
    title: 'Low Stock Alert',
    message: 'Unicorn Wall Hook is out of stock. 8 units needed.',
    action: {
      label: 'Print Now',
      href: '/jobs?product=prod_004&qty=8',
    },
    createdAt: '2025-05-10T12:00:00Z',
  },
  {
    id: 'alert_002',
    type: 'info',
    title: 'Reorder Recommendation',
    message: 'Dino Wall Hook stock (3) is below reorder point (6). Print 10 more?',
    action: {
      label: 'Print 10',
      href: '/jobs?product=prod_001&qty=10',
    },
    createdAt: '2025-05-10T10:00:00Z',
  },
  {
    id: 'alert_003',
    type: 'success',
    title: 'Order Shipped',
    message: 'Order #55553 for Amanda Lee has been shipped.',
    action: {
      label: 'View Order',
      href: '/orders?selected=ord_005',
    },
    createdAt: '2025-05-09T16:00:00Z',
  },
];

// Simulated API delay
const delay = (ms: number) => new Promise((resolve) => setTimeout(resolve, ms));

// API Service Functions
export const api = {
  // User
  async getCurrentUser(): Promise<User> {
    await delay(200);
    return mockUser;
  },

  // Shops
  async getShops(): Promise<Shop[]> {
    await delay(300);
    return mockShops;
  },

  async getShop(shopId: string): Promise<Shop | null> {
    await delay(200);
    return mockShops.find((s) => s.id === shopId) || null;
  },

  // Products
  async getProducts(shopId?: string): Promise<Product[]> {
    await delay(300);
    if (shopId) {
      return mockProducts.filter((p) => p.shopId === shopId);
    }
    return mockProducts;
  },

  async getProduct(productId: string): Promise<Product | null> {
    await delay(200);
    return mockProducts.find((p) => p.id === productId) || null;
  },

  // Parts
  async getParts(shopId?: string): Promise<Part[]> {
    await delay(300);
    if (shopId) {
      return mockParts.filter((p) => p.shopId === shopId);
    }
    return mockParts;
  },

  // Printers
  async getPrinters(): Promise<Printer[]> {
    await delay(300);
    return mockPrinters;
  },

  async getPrinter(printerId: string): Promise<Printer | null> {
    await delay(200);
    return mockPrinters.find((p) => p.id === printerId) || null;
  },

  async updatePrinter(printerId: string, updates: Partial<Printer>): Promise<Printer | null> {
    await delay(300);
    const index = mockPrinters.findIndex((p) => p.id === printerId);
    if (index === -1) return null;
    mockPrinters[index] = { ...mockPrinters[index], ...updates };
    return mockPrinters[index];
  },

  // Print Jobs
  async getPrintJobs(shopId?: string): Promise<PrintJob[]> {
    await delay(300);
    if (shopId) {
      return mockPrintJobs.filter((j) => j.shopId === shopId);
    }
    return mockPrintJobs;
  },

  async getPrintJob(jobId: string): Promise<PrintJob | null> {
    await delay(200);
    return mockPrintJobs.find((j) => j.id === jobId) || null;
  },

  async createPrintJob(job: Omit<PrintJob, 'id' | 'createdAt'>): Promise<PrintJob> {
    await delay(400);
    const newJob: PrintJob = {
      ...job,
      id: `job_${Date.now()}`,
      createdAt: new Date().toISOString(),
    };
    mockPrintJobs.push(newJob);
    return newJob;
  },

  async updatePrintJobStatus(jobId: string, status: PrintJob['status']): Promise<PrintJob | null> {
    await delay(300);
    const job = mockPrintJobs.find((j) => j.id === jobId);
    if (!job) return null;
    job.status = status;
    if (status === 'InProgress') job.startedAt = new Date().toISOString();
    if (status === 'Completed' || status === 'Failed' || status === 'Cancelled') {
      job.completedAt = new Date().toISOString();
    }
    return job;
  },

  // Orders
  async getOrders(shopId?: string): Promise<PersonalizedOrder[]> {
    await delay(300);
    if (shopId) {
      return mockOrders.filter((o) => o.shopId === shopId);
    }
    return mockOrders;
  },

  async getOrder(orderId: string): Promise<PersonalizedOrder | null> {
    await delay(200);
    return mockOrders.find((o) => o.id === orderId) || null;
  },

  async updateOrderStatus(orderId: string, status: PersonalizedOrder['status']): Promise<PersonalizedOrder | null> {
    await delay(300);
    const order = mockOrders.find((o) => o.id === orderId);
    if (!order) return null;
    order.status = status;
    return order;
  },

  // Dashboard
  async getDashboardStats(): Promise<DashboardStats> {
    await delay(400);
    return mockDashboardStats;
  },

  async getAlerts(): Promise<WorkspaceAlert[]> {
    await delay(300);
    return mockAlerts;
  },
};

export default api;
