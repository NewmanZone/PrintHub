import React from 'react'
import { Panel } from '../components/ui/Panel'
import { DataTable } from '../components/ui/DataTable'
import { Button } from '../components/ui/Button'
import { EmptyState } from '../components/ui/EmptyState'
import { Boxes } from 'lucide-react'
import { mockParts, type MockPart } from '../mocks'

export const Parts: React.FC = () => {
  if (mockParts.length === 0) {
    return (
      <EmptyState
        icon={<Boxes size={24} />}
        title="No parts yet"
        description="Add a part and upload a print file to build your product catalog."
        action={<Button>Add Part</Button>}
      />
    )
  }

  return (
    <div>
      <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', marginBottom: 'var(--space-6)' }}>
        <h1 style={{ fontSize: 'var(--text-2xl)', fontWeight: 'var(--font-bold)', margin: 0 }}>Parts</h1>
        <Button size="sm">Add Part</Button>
      </div>

      <Panel>
        <DataTable
          columns={[
            { key: 'name', header: 'Name' },
            { key: 'isGeneric', header: 'Generic', width: '90px', render: (p: MockPart) => (p.isGeneric ? 'Yes' : 'No') },
            { key: 'currentVersionNumber', header: 'Version', width: '90px' },
            { key: 'costPerUnit', header: 'Cost', width: '90px', render: (p: MockPart) => `$${p.costPerUnit.toFixed(2)}` },
            { key: 'inventoryOnHand', header: 'Stock', width: '80px' },
            { key: 'inventoryValue', header: 'Value', width: '90px', render: (p: MockPart) => `$${p.inventoryValue.toFixed(2)}` },
          ]}
          rows={mockParts}
          keyExtractor={(p: MockPart) => p.id}
        />
      </Panel>
    </div>
  )
}
