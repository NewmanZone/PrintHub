import React from 'react'
import { ShoppingCart } from 'lucide-react'
import { EmptyState } from '../components/ui/EmptyState'
import { Panel } from '../components/ui/Panel'
import { StatusChip } from '../components/ui/StatusChip'

export const Orders: React.FC = () => (
  <div className="ph-page">
    <div className="ph-page-header">
      <div>
        <p className="ph-page-kicker">Etsy operations</p>
        <h1 className="ph-page-title">Orders</h1>
        <p className="ph-page-description">Order import will build on the real Etsy connection after listing and source-file management are stable.</p>
      </div>
    </div>
    <Panel title="Order import">
      <EmptyState
        icon={<ShoppingCart size={24} />}
        title="No live order workflow yet"
        description="Phase one is currently focused on Etsy shop connection, listing sync, and 3MF/STL file attachment."
        action={<StatusChip status="draft" label="Future phase" />}
      />
    </Panel>
  </div>
)
