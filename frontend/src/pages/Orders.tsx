import React from 'react'
import { ShoppingCart } from 'lucide-react'
import { DataTable } from '../components/ui/DataTable'
import { EmptyState } from '../components/ui/EmptyState'
import { Panel } from '../components/ui/Panel'
import { StatusChip } from '../components/ui/StatusChip'
import { mockOrders, type MockOrder } from '../mocks'

const orderStatusMap: Record<string, 'pending' | 'warning' | 'success' | 'queued' | 'completed'> = {
  Received: 'pending',
  NeedsMapping: 'warning',
  NeedsFiles: 'warning',
  NeedsPersonalization: 'warning',
  ReadyToDownload: 'success',
  Downloaded: 'queued',
  Printed: 'completed',
}

export const Orders: React.FC = () => {
  if (mockOrders.length === 0) {
    return (
      <EmptyState
        icon={<ShoppingCart size={24} />}
        title="No orders yet"
        description="Orders from Etsy will appear here once your shop is connected."
      />
    )
  }

  return (
    <div className="ph-page">
      <div className="ph-page-header">
        <div>
          <p className="ph-page-kicker">Etsy intake</p>
          <h1 className="ph-page-title">Orders</h1>
          <p className="ph-page-description">Personalized orders, due dates, file readiness, and bundle preparation status.</p>
        </div>
      </div>

      <Panel title="Recent orders">
        <DataTable
          caption="Orders"
          columns={[
            { key: 'etsyOrderId', header: 'Etsy order' },
            { key: 'productName', header: 'Product' },
            { key: 'customerName', header: 'Customer', width: '130px' },
            {
              key: 'status',
              header: 'Status',
              width: '170px',
              render: (order: MockOrder) => <StatusChip status={orderStatusMap[order.status] ?? 'pending'} label={order.status} />,
            },
            { key: 'orderedAt', header: 'Ordered', width: '140px', render: (order: MockOrder) => new Date(order.orderedAt).toLocaleDateString(), sortValue: (order) => new Date(order.orderedAt).getTime() },
            { key: 'dueBy', header: 'Due', width: '140px', render: (order: MockOrder) => new Date(order.dueBy).toLocaleDateString(), sortValue: (order) => new Date(order.dueBy).getTime() },
          ]}
          rows={mockOrders}
          keyExtractor={(order: MockOrder) => order.id}
        />
      </Panel>
    </div>
  )
}
