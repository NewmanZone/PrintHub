import React from 'react'
import { Panel } from '../components/ui/Panel'
import { DataTable } from '../components/ui/DataTable'
import { Button } from '../components/ui/Button'
import { mockQueue, type MockQueueItem } from '../mocks'

export const Queue: React.FC = () => {
  const totalMinutes = mockQueue.reduce((sum, i) => sum + i.estimatedMinutes, 0)

  return (
    <div>
      <h1 style={{ fontSize: 'var(--text-2xl)', fontWeight: 'var(--font-bold)', marginBottom: 'var(--space-6)' }}>
        Print Queue
      </h1>

      <Panel
        title={`${mockQueue.length} items • ~${totalMinutes} min total`}
        actions={<Button size="sm">Start Print</Button>}
      >
        <DataTable
          columns={[
            { key: 'productName', header: 'Product' },
            { key: 'quantity', header: 'Qty', width: '70px' },
            { key: 'partsBreakdown', header: 'Parts' },
            { key: 'estimatedMinutes', header: 'Est. Min', width: '100px' },
            { key: 'status', header: 'Status', width: '100px' },
          ]}
          rows={mockQueue}
          keyExtractor={(i: MockQueueItem) => i.productId}
        />
      </Panel>
    </div>
  )
}
