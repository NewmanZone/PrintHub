import React from 'react'
import { FileWarning, Package, RefreshCw, Store, UploadCloud } from 'lucide-react'
import { Link } from 'react-router-dom'
import { api, type EtsyConnection, type ProductRecord } from '../api'
import { DataTable } from '../components/ui/DataTable'
import { MetricCard } from '../components/ui/MetricCard'
import { Panel } from '../components/ui/Panel'
import { StatusChip } from '../components/ui/StatusChip'

export const Dashboard: React.FC = () => {
  const [products, setProducts] = React.useState<ProductRecord[]>([])
  const [connection, setConnection] = React.useState<EtsyConnection | null>(null)
  const [loading, setLoading] = React.useState(true)
  const [error, setError] = React.useState('')

  React.useEffect(() => {
    const load = async () => {
      setLoading(true)
      setError('')
      try {
        const [productsResponse, connectionResponse] = await Promise.all([
          api.getProducts(),
          api.getEtsyConnection(),
        ])
        setProducts(productsResponse.products)
        setConnection(connectionResponse)
      } catch (err) {
        setError(err instanceof Error ? err.message : 'Failed to load dashboard.')
      } finally {
        setLoading(false)
      }
    }
    void load()
  }, [])

  const activeProducts = products.filter((product) => product.isActive)
  const latestProducts = [...products]
    .sort((a, b) => new Date(b.updatedAt).getTime() - new Date(a.updatedAt).getTime())
    .slice(0, 6)

  return (
    <div className="ph-page surface-grid">
      <div className="ph-page-header">
        <div>
          <p className="ph-page-kicker">Operations overview</p>
          <h1 className="ph-page-title">Dashboard</h1>
          <p className="ph-page-description">
            Etsy shop connection, synced listing health, and source-file readiness for the phase one print workflow.
          </p>
        </div>
      </div>

      {error && <div className="ph-alert ph-alert--warning" role="alert">{error}</div>}

      <div className="ph-grid ph-grid--4">
        <MetricCard label="Etsy connection" value={connection ? 'Connected' : 'Needed'} icon={<Store size={18} />} />
        <MetricCard label="Synced listings" value={products.length} icon={<Package size={18} />} />
        <MetricCard label="Active listings" value={activeProducts.length} icon={<RefreshCw size={18} />} />
        <MetricCard label="File coverage" value="Upload in product view" icon={<UploadCloud size={18} />} />
      </div>

      <div className="ph-grid ph-grid--2">
        <Panel title="Phase 1 readiness">
          <div className="ph-stack">
            <div className={connection ? 'ph-alert' : 'ph-alert ph-alert--warning'}>
              <StatusChip status={connection ? 'success' : 'warning'} label={connection ? 'Connected' : 'Action needed'} />
              <span>{connection ? `${connection.shopName} is connected.` : 'Connect Etsy in Settings to import listings.'}</span>
            </div>
            <div className={products.length > 0 ? 'ph-alert' : 'ph-alert ph-alert--warning'}>
              <StatusChip status={products.length > 0 ? 'success' : 'warning'} label={products.length > 0 ? 'Listings synced' : 'No listings'} />
              <span>{products.length > 0 ? 'Products are ready for source file uploads.' : 'Sync Etsy listings after connecting your shop.'}</span>
            </div>
          </div>
        </Panel>

        <Panel title="Next print prep">
          <div className="ph-stack">
            <div className="ph-inline"><StatusChip status="pending" label="Manual printing" /><span>Bambu integration is out of phase one.</span></div>
            <div className="ph-inline"><FileWarning size={16} /><span>Open a product to upload or download its print source file.</span></div>
          </div>
        </Panel>
      </div>

      <Panel title="Recent Etsy listings">
        {loading ? (
          <p className="ph-muted">Loading dashboard...</p>
        ) : (
          <DataTable
            caption="Recent Etsy listings"
            columns={[
              { key: 'name', header: 'Name', render: (product: ProductRecord) => <Link to={`/products/${product.id}`}>{product.name}</Link> },
              { key: 'externalListingId', header: 'Etsy listing', width: '150px' },
              { key: 'etsyPrice', header: 'Price', width: '110px', render: (product: ProductRecord) => product.etsyPrice == null ? '-' : `$${product.etsyPrice.toFixed(2)}`, sortValue: (product) => product.etsyPrice ?? 0 },
              { key: 'isActive', header: 'State', width: '120px', render: (product: ProductRecord) => <StatusChip status={product.isActive ? 'success' : 'draft'} label={product.isActive ? 'Active' : 'Inactive'} /> },
              { key: 'updatedAt', header: 'Updated', width: '140px', render: (product: ProductRecord) => new Date(product.updatedAt).toLocaleDateString(), sortValue: (product) => new Date(product.updatedAt).getTime() },
            ]}
            rows={latestProducts}
            keyExtractor={(product) => product.id}
          />
        )}
      </Panel>
    </div>
  )
}
