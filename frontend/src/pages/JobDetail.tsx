import React from 'react'
import { Link, useParams } from 'react-router-dom'
import { ArrowLeft, Pause, Play, XCircle } from 'lucide-react'
import { Button } from '../components/ui/Button'
import { DataTable } from '../components/ui/DataTable'
import { ErrorState } from '../components/ui/ErrorState'
import { Panel } from '../components/ui/Panel'
import { StatusChip } from '../components/ui/StatusChip'
import { getJobById } from '../mocks'
import { jobStatusMap } from './Jobs'

export const JobDetail: React.FC = () => {
  const { id } = useParams<{ id: string }>()
  const job = getJobById(id)

  if (!job) {
    return <ErrorState title="Job not found" message={`No print job matches ID ${id ?? 'unknown'}.`} />
  }

  return (
    <div className="ph-page">
      <div className="ph-page-header">
        <div>
          <Link to="/jobs" className="ph-inline ph-muted"><ArrowLeft size={16} /> Jobs</Link>
          <h1 className="ph-page-title">{job.id}</h1>
          <p className="ph-page-description">Live job progress, printer target, item states, and operator controls.</p>
        </div>
        <div className="ph-page-actions">
          <Button size="sm" variant="secondary" iconLeft={<Pause size={16} />}>Pause</Button>
          <Button size="sm" iconLeft={<Play size={16} />}>Resume</Button>
          <Button size="sm" variant="danger" iconLeft={<XCircle size={16} />}>Cancel</Button>
        </div>
      </div>

      <div className="ph-grid ph-grid--3">
        <Panel title="Status"><StatusChip status={jobStatusMap[job.status]} label={job.status} /></Panel>
        <Panel title="Printer"><strong>{job.printerTarget}</strong><p className="ph-muted">Target device</p></Panel>
        <Panel title="Progress">
          <strong>{job.progressPercent ?? 0}%</strong>
          <div className="ph-progress"><div className="ph-progress__bar" style={{ width: `${job.progressPercent ?? 0}%` }} /></div>
        </Panel>
      </div>

      {job.notes && <Panel title="Operator notes"><p className="ph-muted">{job.notes}</p></Panel>}

      <Panel title="Job items">
        <DataTable
          caption="Job items"
          columns={[
            { key: 'partName', header: 'Part' },
            { key: 'quantity', header: 'Qty', width: '80px', sortValue: (item) => item.quantity },
            { key: 'status', header: 'Status', width: '140px', render: (item) => <StatusChip status={jobStatusMap[item.status]} label={item.status} /> },
          ]}
          rows={job.items}
          keyExtractor={(item) => item.partId}
        />
      </Panel>
    </div>
  )
}
