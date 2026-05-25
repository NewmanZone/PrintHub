import React from 'react'
import { Printer } from 'lucide-react'
import { EmptyState } from '../components/ui/EmptyState'
import { Panel } from '../components/ui/Panel'
import { StatusChip } from '../components/ui/StatusChip'

export const Printers: React.FC = () => (
  <div className="ph-page">
    <div className="ph-page-header">
      <div>
        <p className="ph-page-kicker">Printer fleet</p>
        <h1 className="ph-page-title">Printers</h1>
        <p className="ph-page-description">Printer integration is intentionally outside the first Etsy file-workspace release.</p>
      </div>
    </div>
    <Panel title="Printer integration">
      <EmptyState
        icon={<Printer size={24} />}
        title="Manual printing for phase one"
        description="Download product source files from PrintHub and print them in your normal slicer workflow."
        action={<StatusChip status="draft" label="Future phase" />}
      />
    </Panel>
  </div>
)
