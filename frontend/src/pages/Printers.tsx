import React from 'react'
import { Plus, Printer } from 'lucide-react'
import { Button } from '../components/ui/Button'
import { DataTable } from '../components/ui/DataTable'
import { EmptyState } from '../components/ui/EmptyState'
import { Panel } from '../components/ui/Panel'
import { StatusChip, type Status } from '../components/ui/StatusChip'
import { mockPrinters, type MockPrinter } from '../mocks'

const printerStatusMap: Record<MockPrinter['status'], Status> = {
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
        description="Add a Bambu or OctoEverywhere printer to start printing."
        action={<Button>Add printer</Button>}
      />
    )
  }

  return (
    <div className="ph-page">
      <div className="ph-page-header">
        <div>
          <p className="ph-page-kicker">Devices</p>
          <h1 className="ph-page-title">Printers</h1>
          <p className="ph-page-description">Register printer adapters, monitor readiness, and keep default routing clear.</p>
        </div>
        <div className="ph-page-actions">
          <Button size="sm" iconLeft={<Plus size={16} />}>Add printer</Button>
        </div>
      </div>

      <Panel title="Registered printers">
        <DataTable
          caption="Registered printers"
          columns={[
            { key: 'name', header: 'Name' },
            { key: 'type', header: 'Type', width: '110px' },
            { key: 'model', header: 'Model', width: '150px' },
            { key: 'status', header: 'Status', width: '130px', render: (p: MockPrinter) => <StatusChip status={printerStatusMap[p.status]} label={p.status} /> },
            { key: 'temperature', header: 'Temps', width: '150px', render: (p: MockPrinter) => p.nozzleTemp ? `${p.nozzleTemp}C nozzle / ${p.bedTemp}C bed` : 'Unavailable' },
            { key: 'isDefault', header: 'Default', width: '100px', render: (p: MockPrinter) => (p.isDefault ? 'Yes' : 'No') },
          ]}
          rows={mockPrinters}
          keyExtractor={(printer: MockPrinter) => printer.id}
        />
      </Panel>
    </div>
  )
}
