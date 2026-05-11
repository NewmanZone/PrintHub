import React from 'react'
import { Panel } from '../components/ui/Panel'
import { DataTable } from '../components/ui/DataTable'
import { Button } from '../components/ui/Button'
import { StatusChip } from '../components/ui/StatusChip'
import { EmptyState } from '../components/ui/EmptyState'
import { Printer } from 'lucide-react'
import { mockPrinters, type MockPrinter } from '../mocks'

const printerStatusMap: Record<MockPrinter['status'], 'online' | 'busy' | 'offline' | 'error'> = {
  Online: 'online',
  Offline: 'offline',
  Busy: 'busy',
}

export const Printers: React.FC = () => {
  if (mockPrinters.length === 0) {
    return (
      <EmptyState
        icon={<Printer size={24} />}
        title="No printers registered"
        description="Add a Bambu or OctoAnywhere printer to start printing."
        action={<Button>Add Printer</Button>}
      />
    )
  }

  return (
    <div>
      <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', marginBottom: 'var(--space-6)' }}>
        <h1 style={{ fontSize: 'var(--text-2xl)', fontWeight: 'var(--font-bold)', margin: 0 }}>Printers</h1>
        <Button size="sm">Add Printer</Button>
      </div>

      <Panel>
        <DataTable
          columns={[
            { key: 'name', header: 'Name' },
            { key: 'type', header: 'Type', width: '100px' },
            { key: 'model', header: 'Model', width: '140px' },
            { key: 'status', header: 'Status', width: '110px', render: (p: MockPrinter) => <StatusChip status={printerStatusMap[p.status]} label={p.status} /> },
            { key: 'isDefault', header: 'Default', width: '90px', render: (p: MockPrinter) => (p.isDefault ? 'Yes' : 'No') },
          ]}
          rows={mockPrinters}
          keyExtractor={(p: MockPrinter) => p.id}
        />
      </Panel>
    </div>
  )
}
