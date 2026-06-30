import React from 'react'
import { ListChecks } from 'lucide-react'
import { EmptyState } from '../components/ui/EmptyState'
import { Panel } from '../components/ui/Panel'
import { StatusChip } from '../components/ui/StatusChip'

export const Queue: React.FC = () => (
  <div className="ph-page">
    <div className="ph-page-header">
      <div>
        <p className="ph-page-kicker">Print preparation</p>
        <h1 className="ph-page-title">Queue</h1>
        <p className="ph-page-description">The automated print queue will come after orders and source files are represented by live data.</p>
      </div>
    </div>
    <Panel title="Print queue">
      <EmptyState
        icon={<ListChecks size={24} />}
        title="No live queue yet"
        description="Use Products to sync Etsy listings and manage downloadable print files."
        action={<StatusChip status="draft" label="Future phase" />}
      />
    </Panel>
  </div>
)
