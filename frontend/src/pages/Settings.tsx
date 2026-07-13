import React from 'react'
import { Bell, CreditCard, KeyRound, RefreshCw, Store } from 'lucide-react'
import { api, type EtsyConnection } from '../api'
import { Button } from '../components/ui/Button'
import { Panel } from '../components/ui/Panel'
import { StatusChip } from '../components/ui/StatusChip'

export const Settings: React.FC = () => {
  const [connection, setConnection] = React.useState<EtsyConnection | null>(null)
  const [loading, setLoading] = React.useState(true)
  const [busy, setBusy] = React.useState(false)
  const [message, setMessage] = React.useState('')
  const [error, setError] = React.useState('')

  const loadConnection = React.useCallback(async () => {
    setLoading(true)
    setError('')
    try {
      setConnection(await api.getEtsyConnection())
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to load Etsy connection.')
    } finally {
      setLoading(false)
    }
  }, [])

  React.useEffect(() => {
    void loadConnection()
  }, [loadConnection])

  const connectEtsy = async () => {
    setBusy(true)
    setError('')
    try {
      const result = await api.getEtsyConnectUrl()
      window.location.assign(result.authUrl)
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to start Etsy authorization.')
      setBusy(false)
    }
  }

  const syncEtsy = async () => {
    setBusy(true)
    setMessage('')
    setError('')
    try {
      const result = await api.syncEtsy()
      setMessage(result.total == null ? `Etsy sync ${result.status?.toLowerCase() ?? 'started'}.` : `Synced ${result.total} listings from Etsy.`)
      await loadConnection()
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to sync Etsy listings.')
    } finally {
      setBusy(false)
    }
  }

  return (
    <div className="ph-page">
      <div className="ph-page-header">
        <div>
          <p className="ph-page-kicker">Workspace controls</p>
          <h1 className="ph-page-title">Settings</h1>
          <p className="ph-page-description">Shop connections, notification preferences, account identity, and billing placeholders.</p>
        </div>
      </div>

      {message && <div className="ph-alert" role="status">{message}</div>}
      {error && <div className="ph-alert ph-alert--warning" role="alert">{error}</div>}

      <div className="ph-grid ph-grid--2">
        <Panel title="Account" actions={<KeyRound size={18} />}>
          <p className="ph-muted">OAuth-only identity is enabled. Password login, reset, and registration screens are intentionally omitted.</p>
          <StatusChip status="success" label="OAuth only" />
        </Panel>

        <Panel title="Etsy Integration" actions={<Store size={18} />}>
          {loading ? (
            <p className="ph-muted">Checking Etsy connection...</p>
          ) : connection ? (
            <div className="ph-stack">
              <StatusChip status="success" label="Connected" />
              <span><strong>Shop:</strong> {connection.shopName}</span>
              <span><strong>Shop ID:</strong> {connection.externalId ?? connection.shopId}</span>
              <span><strong>Last sync:</strong> {connection.lastSyncAt ? new Date(connection.lastSyncAt).toLocaleString() : 'Never'}</span>
              <div className="ph-inline">
                <Button variant="secondary" size="sm" iconLeft={<RefreshCw size={16} />} onClick={syncEtsy} disabled={busy}>Sync listings</Button>
                <Button variant="ghost" size="sm" onClick={connectEtsy} disabled={busy}>Reconnect</Button>
              </div>
            </div>
          ) : (
            <div className="ph-stack">
              <p className="ph-muted">Connect Etsy to sync listings into PrintHub and attach source files for printing.</p>
              <StatusChip status="warning" label="Not connected" />
              <Button variant="secondary" size="sm" onClick={connectEtsy} disabled={busy}>Connect Etsy shop</Button>
            </div>
          )}
        </Panel>

        <Panel title="Notifications" actions={<Bell size={18} />}>
          <label className="ph-inline"><input type="checkbox" defaultChecked /> Low stock alerts</label>
          <label className="ph-inline"><input type="checkbox" defaultChecked /> Print complete alerts</label>
        </Panel>

        <Panel title="Billing" actions={<CreditCard size={18} />}>
          <p className="ph-muted">Billing settings are reserved for the production plan.</p>
          <StatusChip status="draft" label="Future" />
        </Panel>
      </div>
    </div>
  )
}
