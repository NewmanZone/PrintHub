import React from 'react'
import { Boxes } from 'lucide-react'
import { EmptyState } from '../components/ui/EmptyState'
import { Panel } from '../components/ui/Panel'
import { StatusChip } from '../components/ui/StatusChip'

export const Parts: React.FC = () => (
  <div className="ph-page">
    <div className="ph-page-header">
      <div>
        <p className="ph-page-kicker">Catalog setup</p>
        <h1 className="ph-page-title">Parts</h1>
        <p className="ph-page-description">Reusable part libraries are planned after the shared Etsy file workspace is complete.</p>
      </div>
    </div>
    <Panel title="Part library">
      <EmptyState
        icon={<Boxes size={24} />}
        title="No live part library yet"
        description="Attach source files directly to Etsy products for the phase-one workflow."
        action={<StatusChip status="draft" label="Future phase" />}
      />
    </Panel>
  </div>
)
