import React from 'react'
import { Play } from 'lucide-react'
import { Button } from '../components/ui/Button'
import { DataTable } from '../components/ui/DataTable'
import { Panel } from '../components/ui/Panel'
import { StatusChip } from '../components/ui/StatusChip'
import { mockQueue, type MockQueueItem } from '../mocks'

const queueStatus = {
  Pending: 'pending',
  Printing: 'in-progress',
  Completed: 'completed',
} as const

export const Queue: React.FC = () => {
  const totalMinutes = mockQueue.reduce((sum, item) => sum + item.estimatedMinutes, 0)
  const totalParts = mockQueue.reduce((sum, item) => sum + item.quantity, 0)

  return (
    <div className="ph-page">
      <div className="ph-page-header">
        <div>
          <p className="ph-page-kicker">BOM consolidation</p>
          <h1 className="ph-page-title">Print Queue</h1>
          <p className="ph-page-description">
            Consolidated demand across products, grouped into print-ready part batches.
          </p>
        </div>
        <div className="ph-page-actions">
          <Button iconLeft={<Play size={16} />}>Start print run</Button>
        </div>
      </div>

      <div className="ph-grid ph-grid--3">
        <Panel title="Queue depth"><strong>{mockQueue.length}</strong><p className="ph-muted">product batches waiting</p></Panel>
        <Panel title="Parts needed"><strong>{totalParts}</strong><p className="ph-muted">individual printed components</p></Panel>
        <Panel title="Estimated time"><strong>{Math.round(totalMinutes / 60)}h {totalMinutes % 60}m</strong><p className="ph-muted">based on current profiles</p></Panel>
      </div>

      <Panel title={`${mockQueue.length} queue items`}>
        <DataTable
          caption="Print queue"
          columns={[
            { key: 'productName', header: 'Product' },
            { key: 'quantity', header: 'Qty', width: '80px', sortValue: (i) => i.quantity },
            { key: 'partsBreakdown', header: 'Parts' },
            { key: 'estimatedMinutes', header: 'Est. min', width: '110px', sortValue: (i) => i.estimatedMinutes },
            { key: 'status', header: 'Status', width: '130px', render: (i: MockQueueItem) => <StatusChip status={queueStatus[i.status]} label={i.status} /> },
          ]}
          rows={mockQueue}
          keyExtractor={(item: MockQueueItem) => item.productId}
        />
      </Panel>
    </div>
  )
}
