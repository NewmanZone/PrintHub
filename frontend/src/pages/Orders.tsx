import React from 'react'
import { Panel } from '../components/ui/Panel'
import { DataTable } from '../components/ui/DataTable'
import { EmptyState } from '../components/ui/EmptyState'
import { ShoppingCart } from 'lucide-react'
import { mockOrders, type MockOrder } from '../mocks'

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
    <div>
      <h1 style={{ fontSize: 'var(--text-2xl)', fontWeight: 'var(--font-bold)', marginBottom: 'var(--space-6)' }}>
        Orders
      </h1>

      <Panel>
        <DataTable
          columns={[
            { key: 'etsyOrderId', header: 'Etsy Order' },
            { key: 'productName', header: 'Product' },
            { key: 'customerName', header: 'Customer', width: '130px' },
            { key: 'status', header: 'Status', width: '120px' },
            { key: 'orderedAt', header: 'Ordered', width: '160px', render: (o: MockOrder) => new Date(o.orderedAt).toLocaleDateString() },
            { key: 'dueBy', header: 'Due', width: '160px', render: (o: MockOrder) => new Date(o.dueBy).toLocaleDateString() },
          ]}
          rows={mockOrders}
          keyExtractor={(o: MockOrder) => o.id}
        />
      </Panel>
    </div>
  )
}
