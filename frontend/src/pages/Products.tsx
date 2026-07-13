import React from 'react'
import { Link } from 'react-router-dom'
import { Package, RefreshCw } from 'lucide-react'
import { api, type ProductRecord } from '../api'
import { Button } from '../components/ui/Button'
import { DataTable } from '../components/ui/DataTable'
import { EmptyState } from '../components/ui/EmptyState'
import { Panel } from '../components/ui/Panel'
import { StatusChip } from '../components/ui/StatusChip'

export const Products: React.FC = () => {
  const [products, setProducts] = React.useState<ProductRecord[]>([])
  const [search, setSearch] = React.useState('')
  const [statusFilter, setStatusFilter] = React.useState<'all' | 'active'>('all')
  const [loading, setLoading] = React.useState(true)
  const [error, setError] = React.useState('')
  const [message, setMessage] = React.useState('')

  const loadProducts = React.useCallback(async () => {
    setLoading(true)
    setError('')
    try {
      const response = await api.getProducts()
      setProducts(response.products)
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to load products.')
    } finally {
      setLoading(false)
    }
  }, [])

  React.useEffect(() => {
    void loadProducts()
  }, [loadProducts])

  const syncEtsy = async () => {
    setMessage('')
    setError('')
    try {
      const result = await api.syncEtsy()
      setMessage(result.total == null
        ? `Etsy sync ${result.status?.toLowerCase() ?? 'started'}.`
        : `Synced ${result.total} listings (${result.imported ?? 0} imported, ${result.updated ?? 0} updated).`)
      await loadProducts()
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to sync Etsy.')
    }
  }

  const filteredProducts = products.filter((product) => {
    const matchesSearch = product.name.toLowerCase().includes(search.toLowerCase())
    const matchesStatus = statusFilter === 'all' || product.isActive
    return matchesSearch && matchesStatus
  })

  return (
    <div className="ph-page">
      <div className="ph-page-header">
        <div>
          <p className="ph-page-kicker">Catalog</p>
          <h1 className="ph-page-title">Products</h1>
          <p className="ph-page-description">Search Etsy listings, verify file coverage, and upload the 3MF/STL source files needed for printing.</p>
        </div>
        <div className="ph-page-actions">
          <Button size="sm" iconLeft={<RefreshCw size={16} />} onClick={syncEtsy}>Sync Etsy</Button>
        </div>
      </div>

      {message && <div className="ph-alert" role="status">{message}</div>}
      {error && <div className="ph-alert ph-alert--warning" role="alert">{error}</div>}

      <Panel
        title="Product catalog"
        actions={
          <div className="ph-toolbar">
            <input className="ph-field" value={search} onChange={(event) => setSearch(event.target.value)} placeholder="Search products" aria-label="Search products" />
            <select className="ph-field" value={statusFilter} onChange={(event) => setStatusFilter(event.target.value as 'all' | 'active')} aria-label="Filter products by status">
              <option value="all">All listings</option>
              <option value="active">Active only</option>
            </select>
          </div>
        }
      >
        {loading ? (
          <p className="ph-muted">Loading products...</p>
        ) : (
          <DataTable
          caption="Products"
          columns={[
            { key: 'name', header: 'Name', render: (p: ProductRecord) => <Link to={`/products/${p.id}`}>{p.name}</Link> },
            { key: 'externalListingId', header: 'Etsy listing', width: '150px' },
            { key: 'etsyPrice', header: 'Price', width: '100px', render: (p: ProductRecord) => p.etsyPrice == null ? '-' : `$${p.etsyPrice.toFixed(2)}`, sortValue: (p) => p.etsyPrice ?? 0 },
            { key: 'isActive', header: 'State', width: '130px', render: (p: ProductRecord) => <StatusChip status={p.isActive ? 'success' : 'draft'} label={p.isActive ? 'Active' : 'Inactive'} /> },
            { key: 'updatedAt', header: 'Updated', width: '140px', render: (p: ProductRecord) => new Date(p.updatedAt).toLocaleDateString(), sortValue: (p) => new Date(p.updatedAt).getTime() },
          ]}
          rows={filteredProducts}
          keyExtractor={(product: ProductRecord) => product.id}
          emptyState={
            <EmptyState
              icon={<Package size={24} />}
              title={products.length === 0 ? 'No Etsy listings synced' : 'No products match'}
              description={products.length === 0 ? 'Connect Etsy in Settings, then sync listings.' : 'Try a different search term or clear the status filter.'}
            />
          }
          />
        )}
      </Panel>
    </div>
  )
}
