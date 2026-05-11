import React from 'react'
import { Panel } from '../components/ui/Panel'
import { DataTable } from '../components/ui/DataTable'
import { Button } from '../components/ui/Button'
import { EmptyState } from '../components/ui/EmptyState'
import { Package } from 'lucide-react'
import { mockProducts, type MockProduct } from '../mocks'

export const Products: React.FC = () => {
  if (mockProducts.length === 0) {
    return (
      <EmptyState
        icon={<Package size={24} />}
        title="No products yet"
        description="Connect your Etsy shop or add a product manually to get started."
        action={<Button>Add Product</Button>}
      />
    )
  }

  return (
    <div>
      <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', marginBottom: 'var(--space-6)' }}>
        <h1 style={{ fontSize: 'var(--text-2xl)', fontWeight: 'var(--font-bold)', margin: 0 }}>Products</h1>
        <Button size="sm">Add Product</Button>
      </div>

      <Panel>
        <DataTable
          columns={[
            { key: 'name', header: 'Name' },
            { key: 'etsyPrice', header: 'Price', width: '100px' },
            { key: 'printCount', header: 'Printed', width: '90px' },
            { key: 'inventoryOnHand', header: 'Stock', width: '80px' },
            { key: 'reorderPoint', header: 'Reorder', width: '90px' },
            { key: 'costPerPrint', header: 'Cost/Print', width: '110px', render: (p: MockProduct) => `$${p.costPerPrint.toFixed(2)}` },
          ]}
          rows={mockProducts}
          keyExtractor={(p: MockProduct) => p.id}
        />
      </Panel>
    </div>
  )
}
