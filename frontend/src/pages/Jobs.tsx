import React from 'react'
import { Link } from 'react-router-dom'
import { Wrench } from 'lucide-react'
import { DataTable } from '../components/ui/DataTable'
import { EmptyState } from '../components/ui/EmptyState'
import { Panel } from '../components/ui/Panel'
import { StatusChip, type Status } from '../components/ui/StatusChip'
import { mockJobs, type JobStatus, type MockJob } from '../mocks'

export const jobStatusMap: Record<JobStatus, Status> = {
  Draft: 'draft',
  Pending: 'pending',
  Queued: 'queued',
  InProgress: 'in-progress',
  Paused: 'paused',
  Completed: 'completed',
  Failed: 'failed',
  Cancelled: 'cancelled',
}

export const Jobs: React.FC = () => {
  const [status, setStatus] = React.useState<'all' | JobStatus>('all')
  const jobs = status === 'all' ? mockJobs : mockJobs.filter((job) => job.status === status)

  return (
    <div className="ph-page">
      <div className="ph-page-header">
        <div>
          <p className="ph-page-kicker">Execution</p>
          <h1 className="ph-page-title">Print Jobs</h1>
          <p className="ph-page-description">Track progress, inspect job items, and keep active printer work under control.</p>
        </div>
      </div>

      <Panel
        title="Job history"
        actions={
          <select className="ph-field" value={status} onChange={(event) => setStatus(event.target.value as 'all' | JobStatus)} aria-label="Filter jobs by status">
            <option value="all">All statuses</option>
            {Object.keys(jobStatusMap).map((item) => <option key={item} value={item}>{item}</option>)}
          </select>
        }
      >
        <DataTable
          caption="Print jobs"
          columns={[
            { key: 'id', header: 'Job ID', width: '120px', render: (job: MockJob) => <Link to={`/jobs/${job.id}`}>{job.id}</Link> },
            { key: 'status', header: 'Status', width: '140px', render: (job: MockJob) => <StatusChip status={jobStatusMap[job.status]} label={job.status} /> },
            { key: 'printerTarget', header: 'Printer' },
            { key: 'progressPercent', header: 'Progress', width: '170px', render: (job: MockJob) => <div className="ph-progress" aria-label={`${job.progressPercent ?? 0}% complete`}><div className="ph-progress__bar" style={{ width: `${job.progressPercent ?? 0}%` }} /></div>, sortValue: (job) => job.progressPercent ?? 0 },
            { key: 'createdAt', header: 'Created', width: '150px', render: (job: MockJob) => new Date(job.createdAt).toLocaleDateString(), sortValue: (job) => new Date(job.createdAt).getTime() },
          ]}
          rows={jobs}
          keyExtractor={(job: MockJob) => job.id}
          emptyState={
            <EmptyState
              icon={<Wrench size={24} />}
              title="No jobs match"
              description="Clear the status filter to see the full print history."
            />
          }
        />
      </Panel>
    </div>
  )
}
