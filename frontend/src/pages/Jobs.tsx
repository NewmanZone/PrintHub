import React from 'react'
import { ClipboardList } from 'lucide-react'
import { EmptyState } from '../components/ui/EmptyState'
import { Panel } from '../components/ui/Panel'
import { StatusChip } from '../components/ui/StatusChip'

export const Jobs: React.FC = () => (
  <div className="ph-page">
    <div className="ph-page-header">
      <div>
        <p className="ph-page-kicker">Production history</p>
        <h1 className="ph-page-title">Jobs</h1>
        <p className="ph-page-description">Print jobs will be live once printer or order-bundle workflows are implemented.</p>
      </div>
    </div>
    <Panel title="Print jobs">
      <EmptyState
        icon={<ClipboardList size={24} />}
        title="No live job tracking yet"
        description="Phase one avoids simulated printer data and keeps the app centered on Etsy listings and source files."
        action={<StatusChip status="draft" label="Future phase" />}
      />
    </Panel>
  </div>
)
