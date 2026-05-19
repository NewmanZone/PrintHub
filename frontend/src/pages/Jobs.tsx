import React from 'react'
import { Panel } from '../components/ui/Panel'
import { DataTable } from '../components/ui/DataTable'
import { StatusChip } from '../components/ui/StatusChip'
import { EmptyState } from '../components/ui/EmptyState'
import { Wrench } from 'lucide-react'
import { mockJobs, type MockJob } from '../mocks'

const statusMap: Record<MockJob['status'], 'online' | 'busy' | 'offline' | 'error' | 'warning' | 'success'> = {
  Pending: 'warning',
  InProgress: 'busy',
  Completed: 'success',
  Failed: 'error',
  Cancelled: 'offline',
}

export const Jobs: React.FC = () => {
  if (mockJobs.length === 0) {
    return (
      <EmptyState
        icon={<Wrench size={24} />}
        title="No print jobs yet"
        description="Queue items from the print queue to start your first job."
      />
    )
  }

  return (
    <div>
      <h1 style={{ fontSize: 'var(--text-2xl)', fontWeight: 'var(--font-bold)', marginBottom: 'var(--space-6)' }}>
        Print Jobs
      </h1>

      <Panel>
        <DataTable
          columns={[
            { key: 'id', header: 'Job ID', width: '120px' },
            { key: 'status', header: 'Status', width: '120px', render: (j: MockJob) => <StatusChip status={statusMap[j.status]} label={j.status} /> },
            { key: 'printerTarget', header: 'Printer' },
            { key: 'progressPercent', header: 'Progress', width: '110px', render: (j: MockJob) => `${j.progressPercent ?? 0}%` },
            { key: 'createdAt', header: 'Created', width: '180px', render: (j: MockJob) => new Date(j.createdAt).toLocaleDateString() },
          ]}
          rows={mockJobs}
          keyExtractor={(j: MockJob) => j.id}
        />
      </Panel>
    </div>
  )
}
