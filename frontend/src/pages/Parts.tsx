import React from 'react'
import { Boxes, Plus } from 'lucide-react'
import { Button } from '../components/ui/Button'
import { DataTable } from '../components/ui/DataTable'
import { EmptyState } from '../components/ui/EmptyState'
import { Panel } from '../components/ui/Panel'
import { StatusChip } from '../components/ui/StatusChip'
import { mockParts, type MockPart } from '../mocks'

export const Parts: React.FC = () => {
  if (mockParts.length === 0) {
    return (
      <EmptyState
        icon={<Boxes size={24} />}
        title="No parts yet"
        description="Add a part and upload a print file to build your product catalog."
        action={<Button>Add part</Button>}
      />
    )
  }

  return (
    <div className="ph-page">
      <div className="ph-page-header">
        <div>
          <p className="ph-page-kicker">Printable inventory</p>
          <h1 className="ph-page-title">Parts</h1>
          <p className="ph-page-description">Track generic and product-specific print files, versions, costs, and on-hand inventory.</p>
        </div>
        <div className="ph-page-actions"><Button size="sm" iconLeft={<Plus size={16} />}>Add part</Button></div>
      </div>

      <Panel title="Part catalog">
        <DataTable
          caption="Parts"
          columns={[
            { key: 'name', header: 'Name' },
            { key: 'isGeneric', header: 'Type', width: '120px', render: (p: MockPart) => <StatusChip status={p.isGeneric ? 'queued' : 'draft'} label={p.isGeneric ? 'Generic' : 'Specific'} /> },
            { key: 'currentVersionNumber', header: 'Version', width: '100px' },
            { key: 'costPerUnit', header: 'Cost', width: '100px', render: (p: MockPart) => `$${p.costPerUnit.toFixed(2)}`, sortValue: (p) => p.costPerUnit },
            { key: 'inventoryOnHand', header: 'Stock', width: '90px', sortValue: (p) => p.inventoryOnHand },
            { key: 'inventoryValue', header: 'Value', width: '100px', render: (p: MockPart) => `$${p.inventoryValue.toFixed(2)}`, sortValue: (p) => p.inventoryValue },
          ]}
          rows={mockParts}
          keyExtractor={(part: MockPart) => part.id}
        />
      </Panel>
    </div>
  )
}
