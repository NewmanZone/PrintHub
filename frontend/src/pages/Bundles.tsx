import React from 'react'
import { Archive } from 'lucide-react'
import { EmptyState } from '../components/ui/EmptyState'
import { Panel } from '../components/ui/Panel'
import { StatusChip } from '../components/ui/StatusChip'

export const Bundles: React.FC = () => (
  <div className="ph-page">
    <div className="ph-page-header">
      <div>
        <p className="ph-page-kicker">Preparation bundles</p>
        <h1 className="ph-page-title">Preparation Bundles</h1>
        <p className="ph-page-description">Bundles will use live Etsy orders and attached source files once order import is added.</p>
      </div>
    </div>
    <Panel title="Bundle generation">
      <EmptyState
        icon={<Archive size={24} />}
        title="No live bundle workflow yet"
        description="For now, open a synced product to upload or download its print-ready source file."
        action={<StatusChip status="draft" label="Future phase" />}
      />
    </Panel>
  </div>
)
