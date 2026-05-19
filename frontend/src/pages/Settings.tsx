import React from 'react'
import { Panel } from '../components/ui/Panel'
import { Button } from '../components/ui/Button'

export const Settings: React.FC = () => {
  return (
    <div>
      <h1 style={{ fontSize: 'var(--text-2xl)', fontWeight: 'var(--font-bold)', marginBottom: 'var(--space-6)' }}>
        Settings
      </h1>

      <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(300px, 1fr))', gap: 'var(--space-4)' }}>
        <Panel title="Account">
          <p style={{ color: 'var(--muted)' }}>Account settings will appear here once authentication is wired.</p>
        </Panel>

        <Panel title="Etsy Integration">
          <p style={{ color: 'var(--muted)' }}>Connect or disconnect your Etsy shop. OAuth flow coming soon.</p>
          <div style={{ marginTop: 'var(--space-4)' }}>
            <Button variant="secondary" size="sm">Connect Etsy Shop</Button>
          </div>
        </Panel>

        <Panel title="Notifications">
          <p style={{ color: 'var(--muted)' }}>Configure print-complete and low-stock alerts.</p>
        </Panel>
      </div>
    </div>
  )
}
