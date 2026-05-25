import React from 'react'
import { Archive, CheckCircle2, Download, Plus, Search } from 'lucide-react'
import { Button } from '../components/ui/Button'
import { DataTable } from '../components/ui/DataTable'
import { EmptyState } from '../components/ui/EmptyState'
import { Panel } from '../components/ui/Panel'
import { StatusChip, type Status } from '../components/ui/StatusChip'
import { mockBundles, type BundleStatus, type MockBundle } from '../mocks'
import './Bundles.css'

const bundleStatusMap: Record<BundleStatus, Status> = {
  ReadyToDownload: 'success',
  Downloaded: 'queued',
  Printed: 'completed',
  Blocked: 'warning',
}

type BundleForm = {
  orderId: string
  customerName: string
  fileCount: string
  itemCount: string
  notes: string
}

const emptyForm: BundleForm = {
  orderId: '',
  customerName: '',
  fileCount: '1',
  itemCount: '1',
  notes: '',
}

export const buildBundleManifest = (bundle: MockBundle) => ({
  bundleId: bundle.id,
  orderId: bundle.orderId,
  customerName: bundle.customerName,
  status: bundle.status,
  fileCount: bundle.fileCount,
  itemCount: bundle.itemCount,
  notes: bundle.notes,
  generatedAt: new Date().toISOString(),
})

export const Bundles: React.FC = () => {
  const [bundles, setBundles] = React.useState<MockBundle[]>(mockBundles)
  const [search, setSearch] = React.useState('')
  const [status, setStatus] = React.useState<'all' | BundleStatus>('all')
  const [showForm, setShowForm] = React.useState(false)
  const [form, setForm] = React.useState<BundleForm>(emptyForm)
  const [formError, setFormError] = React.useState('')
  const [message, setMessage] = React.useState('')
  const [lastManifest, setLastManifest] = React.useState('')

  const filteredBundles = React.useMemo(() => {
    const query = search.trim().toLowerCase()
    return bundles.filter((bundle) => {
      const matchesStatus = status === 'all' || bundle.status === status
      const matchesSearch = query.length === 0
        || bundle.id.toLowerCase().includes(query)
        || bundle.orderId.toLowerCase().includes(query)
        || bundle.customerName.toLowerCase().includes(query)
      return matchesStatus && matchesSearch
    })
  }, [bundles, search, status])

  const updateForm = (field: keyof BundleForm) => (
    event: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement>
  ) => {
    setForm((current) => ({ ...current, [field]: event.target.value }))
  }

  const createBundle = (event: React.FormEvent<HTMLFormElement>) => {
    event.preventDefault()
    setMessage('')

    const fileCount = Number(form.fileCount)
    const itemCount = Number(form.itemCount)
    if (!form.orderId.trim() || !form.customerName.trim()) {
      setFormError('Order ID and customer name are required.')
      return
    }
    if (!Number.isInteger(fileCount) || fileCount < 1 || !Number.isInteger(itemCount) || itemCount < 1) {
      setFormError('File count and item count must be whole numbers greater than zero.')
      return
    }

    const nextBundle: MockBundle = {
      id: `bundle_${String(bundles.length + 1).padStart(3, '0')}`,
      orderId: form.orderId.trim(),
      customerName: form.customerName.trim(),
      status: 'ReadyToDownload',
      fileCount,
      itemCount,
      createdAt: new Date().toISOString(),
      notes: form.notes.trim() || 'Manual bundle created from selected products and files.',
    }

    setBundles((current) => [nextBundle, ...current])
    setForm(emptyForm)
    setFormError('')
    setShowForm(false)
    setMessage(`${nextBundle.id} is ready to download.`)
  }

  const downloadBundle = (bundle: MockBundle) => {
    const manifest = buildBundleManifest(bundle)
    const manifestText = JSON.stringify(manifest, null, 2)
    const filename = `${bundle.id}-manifest.json`

    if (typeof document !== 'undefined' && typeof URL !== 'undefined' && URL.createObjectURL) {
      const blob = new Blob([manifestText], { type: 'application/json' })
      const url = URL.createObjectURL(blob)
      const anchor = document.createElement('a')
      anchor.href = url
      anchor.download = filename
      anchor.click()
      URL.revokeObjectURL(url)
    }

    setBundles((current) => current.map((item) => (
      item.id === bundle.id && item.status === 'ReadyToDownload'
        ? { ...item, status: 'Downloaded' }
        : item
    )))
    setLastManifest(filename)
    setMessage(`${filename} prepared.`)
  }

  const markPrinted = (bundle: MockBundle) => {
    setBundles((current) => current.map((item) => (
      item.id === bundle.id ? { ...item, status: 'Printed' } : item
    )))
    setMessage(`${bundle.id} marked printed.`)
  }

  if (bundles.length === 0) {
    return (
      <EmptyState
        icon={<Archive size={24} />}
        title="No bundles yet"
        description="Generate a preparation bundle from an Etsy order or manual batch."
      />
    )
  }

  return (
    <div className="ph-page">
      <div className="ph-page-header">
        <div>
          <p className="ph-page-kicker">Manual printing</p>
          <h1 className="ph-page-title">Preparation Bundles</h1>
          <p className="ph-page-description">
            Download-ready source files, manifests, quantities, and personalization notes for manual printing.
          </p>
        </div>
        <div className="ph-page-actions">
          <Button
            variant="primary"
            iconLeft={<Plus size={16} />}
            onClick={() => {
              setShowForm((current) => !current)
              setFormError('')
            }}
          >
            Create manual bundle
          </Button>
        </div>
      </div>

      {message && (
        <div className="ph-alert" role="status">
          <CheckCircle2 size={18} aria-hidden="true" />
          <span>{message}</span>
        </div>
      )}

      {lastManifest && (
        <div className="ph-bundles__manifest" aria-label="Latest prepared manifest">
          <strong>Latest manifest</strong>
          <code>{lastManifest}</code>
        </div>
      )}

      {showForm && (
        <Panel title="Create manual bundle">
          <form className="ph-bundles__form" onSubmit={createBundle}>
            {formError && (
              <div className="ph-alert ph-alert--warning ph-bundles__form-wide" role="alert">
                {formError}
              </div>
            )}
            <label className="ph-bundles__label">
              Etsy order ID
              <input
                className="ph-field"
                value={form.orderId}
                onChange={updateForm('orderId')}
                placeholder="etsy_order_98767"
              />
            </label>
            <label className="ph-bundles__label">
              Customer
              <input
                className="ph-field"
                value={form.customerName}
                onChange={updateForm('customerName')}
                placeholder="Customer name"
              />
            </label>
            <label className="ph-bundles__label">
              Files
              <input
                className="ph-field"
                inputMode="numeric"
                value={form.fileCount}
                onChange={updateForm('fileCount')}
              />
            </label>
            <label className="ph-bundles__label">
              Items
              <input
                className="ph-field"
                inputMode="numeric"
                value={form.itemCount}
                onChange={updateForm('itemCount')}
              />
            </label>
            <label className="ph-bundles__label ph-bundles__form-wide">
              Notes
              <textarea
                className="ph-field ph-bundles__textarea"
                value={form.notes}
                onChange={updateForm('notes')}
                placeholder="Personalization or manual prep notes"
              />
            </label>
            <div className="ph-bundles__form-actions">
              <Button type="button" variant="secondary" onClick={() => setShowForm(false)}>Cancel</Button>
              <Button type="submit" variant="primary">Save bundle</Button>
            </div>
          </form>
        </Panel>
      )}

      <Panel title="Recent bundles">
        <div className="ph-bundles__filters">
          <label className="sr-only" htmlFor="bundle-search">Search bundles</label>
          <div className="ph-inline ph-field">
            <Search size={16} aria-hidden="true" />
            <input
              className="ph-bundles__search-input"
              id="bundle-search"
              aria-label="Search bundles"
              value={search}
              onChange={(event) => setSearch(event.target.value)}
              placeholder="Search bundle, order, or customer"
            />
          </div>
          <label className="sr-only" htmlFor="bundle-status">Filter bundles by status</label>
          <select
            id="bundle-status"
            className="ph-field"
            value={status}
            onChange={(event) => setStatus(event.target.value as 'all' | BundleStatus)}
            aria-label="Filter bundles by status"
          >
            <option value="all">All statuses</option>
            <option value="ReadyToDownload">Ready to download</option>
            <option value="Downloaded">Downloaded</option>
            <option value="Printed">Printed</option>
            <option value="Blocked">Blocked</option>
          </select>
        </div>
        <DataTable
          caption="Preparation bundles"
          columns={[
            { key: 'id', header: 'Bundle' },
            { key: 'orderId', header: 'Etsy order' },
            { key: 'customerName', header: 'Customer', width: '140px' },
            {
              key: 'status',
              header: 'Status',
              width: '170px',
              render: (bundle: MockBundle) => (
                <StatusChip status={bundleStatusMap[bundle.status]} label={bundle.status} />
              ),
            },
            { key: 'fileCount', header: 'Files', width: '90px' },
            { key: 'itemCount', header: 'Items', width: '90px' },
            {
              key: 'createdAt',
              header: 'Created',
              width: '140px',
              render: (bundle: MockBundle) => new Date(bundle.createdAt).toLocaleDateString(),
              sortValue: (bundle) => new Date(bundle.createdAt).getTime(),
            },
            {
              key: 'download',
              header: 'Actions',
              width: '240px',
              sortable: false,
              render: (bundle: MockBundle) => (
                <div className="ph-bundles__actions">
                  <Button
                    variant="secondary"
                    size="sm"
                    iconLeft={<Download size={14} />}
                    disabled={bundle.status === 'Blocked'}
                    onClick={() => downloadBundle(bundle)}
                  >
                    Download
                  </Button>
                  <Button
                    variant="ghost"
                    size="sm"
                    iconLeft={<CheckCircle2 size={14} />}
                    disabled={bundle.status === 'Printed' || bundle.status === 'Blocked'}
                    onClick={() => markPrinted(bundle)}
                  >
                    Mark printed
                  </Button>
                </div>
              ),
            },
          ]}
          rows={filteredBundles}
          keyExtractor={(bundle: MockBundle) => bundle.id}
          emptyState={
            <EmptyState
              icon={<Archive size={24} />}
              title="No bundles match"
              description="Clear the search or status filter to see all preparation bundles."
            />
          }
        />
      </Panel>

      <Panel title="What a bundle contains">
        <div className="ph-stack">
          {bundles.slice(0, 2).map((bundle) => (
            <div className="ph-alert" key={bundle.id}>
              <Archive size={18} aria-hidden="true" />
              <div>
                <strong>{bundle.id}</strong>
                <p className="ph-muted ph-bundles__note">{bundle.notes}</p>
              </div>
            </div>
          ))}
        </div>
      </Panel>
    </div>
  )
}
