import React from 'react'
import { Link, useParams } from 'react-router-dom'
import { ArrowLeft, Box, Download, UploadCloud } from 'lucide-react'
import { api, type ProductFileRecord, type ProductRecord } from '../api'
import { Button } from '../components/ui/Button'
import { DataTable } from '../components/ui/DataTable'
import { EmptyState } from '../components/ui/EmptyState'
import { ErrorState } from '../components/ui/ErrorState'
import { Panel } from '../components/ui/Panel'
import { StatusChip } from '../components/ui/StatusChip'

const formatBytes = (bytes: number) => {
  if (bytes < 1024) return `${bytes} B`
  if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(1)} KB`
  return `${(bytes / 1024 / 1024).toFixed(1)} MB`
}

export const ProductDetail: React.FC = () => {
  const { id } = useParams<{ id: string }>()
  const [product, setProduct] = React.useState<ProductRecord | null>(null)
  const [files, setFiles] = React.useState<ProductFileRecord[]>([])
  const [selectedFile, setSelectedFile] = React.useState<File | null>(null)
  const [loading, setLoading] = React.useState(true)
  const [uploading, setUploading] = React.useState(false)
  const [error, setError] = React.useState('')
  const [message, setMessage] = React.useState('')

  const loadProduct = React.useCallback(async () => {
    if (!id) return
    setLoading(true)
    setError('')
    try {
      const [productResponse, filesResponse] = await Promise.all([
        api.getProduct(id),
        api.getProductFiles(id),
      ])
      setProduct(productResponse)
      setFiles(filesResponse.files)
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to load product.')
    } finally {
      setLoading(false)
    }
  }, [id])

  React.useEffect(() => {
    void loadProduct()
  }, [loadProduct])

  const uploadFile = async (event: React.FormEvent) => {
    event.preventDefault()
    if (!id || !selectedFile) return
    setUploading(true)
    setError('')
    setMessage('')
    try {
      await api.uploadProductFile(id, selectedFile)
      setSelectedFile(null)
      setMessage(`${selectedFile.name} uploaded.`)
      await loadProduct()
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to upload file.')
    } finally {
      setUploading(false)
    }
  }

  if (loading) {
    return <div className="ph-page"><p className="ph-muted">Loading product...</p></div>
  }

  if (!product || error.includes('404')) {
    return <ErrorState title="Product not found" message={`No product matches ID ${id ?? 'unknown'}.`} />
  }

  return (
    <div className="ph-page">
      <div className="ph-page-header">
        <div>
          <Link to="/products" className="ph-inline ph-muted"><ArrowLeft size={16} /> Products</Link>
          <h1 className="ph-page-title">{product.name}</h1>
          <p className="ph-page-description">Etsy listing {product.externalListingId} with source files ready for your family print workflow.</p>
        </div>
        <div className="ph-page-actions">
          <StatusChip status={product.isActive ? 'success' : 'draft'} label={product.isActive ? 'Active Etsy listing' : 'Inactive Etsy listing'} />
        </div>
      </div>

      {message && <div className="ph-alert" role="status">{message}</div>}
      {error && !error.includes('404') && <div className="ph-alert ph-alert--warning" role="alert">{error}</div>}

      <div className="ph-grid ph-grid--3">
        <Panel title="Overview">
          <div className="ph-stack">
            <span><strong>Price:</strong> {product.etsyPrice == null ? 'Not provided' : `$${product.etsyPrice.toFixed(2)}`}</span>
            <span><strong>Listing ID:</strong> {product.externalListingId}</span>
            <span><strong>Last updated:</strong> {new Date(product.updatedAt).toLocaleString()}</span>
          </div>
        </Panel>
        <Panel title="Files">
          <div className="ph-stack">
            <span><strong>Attached:</strong> {files.length}</span>
            <span><strong>Latest version:</strong> {files[0]?.versionNumber ?? 'None'}</span>
            <StatusChip status={files.length > 0 ? 'success' : 'warning'} label={files.length > 0 ? 'Ready to print' : 'Needs source file'} />
          </div>
        </Panel>
        <Panel title="Thumbnail">
          <div className="ph-product-thumb surface-grid" aria-label="Product thumbnail">
            {product.imageUrl ? <img src={product.imageUrl} alt="" /> : <Box size={48} />}
          </div>
        </Panel>
      </div>

      <Panel title="Upload print source">
        <form className="ph-toolbar" onSubmit={uploadFile}>
          <input
            className="ph-field"
            type="file"
            accept=".3mf,.stl"
            onChange={(event) => setSelectedFile(event.target.files?.[0] ?? null)}
            aria-label="Choose 3MF or STL file"
          />
          <Button type="submit" size="sm" iconLeft={<UploadCloud size={16} />} disabled={!selectedFile || uploading}>
            {uploading ? 'Uploading' : 'Upload file'}
          </Button>
        </form>
      </Panel>

      <Panel title="Print files">
        <DataTable
          caption="Product files"
          columns={[
            { key: 'fileName', header: 'File' },
            { key: 'fileType', header: 'Type', width: '90px' },
            { key: 'fileSizeBytes', header: 'Size', width: '110px', render: (file: ProductFileRecord) => formatBytes(file.fileSizeBytes), sortValue: (file) => file.fileSizeBytes },
            { key: 'versionNumber', header: 'Version', width: '110px' },
            { key: 'uploadedAt', header: 'Uploaded', width: '150px', render: (file: ProductFileRecord) => new Date(file.uploadedAt).toLocaleDateString(), sortValue: (file) => new Date(file.uploadedAt).getTime() },
            { key: 'download', header: '', width: '90px', sortable: false, render: (file: ProductFileRecord) => <a className="ph-inline" href={api.fileDownloadUrl(file.id)}><Download size={16} /> Download</a> },
          ]}
          rows={files}
          keyExtractor={(file) => file.id}
          emptyState={
            <EmptyState
              icon={<UploadCloud size={24} />}
              title="No source files attached"
              description="Upload the 3MF or STL you use to print this Etsy listing."
            />
          }
        />
      </Panel>
    </div>
  )
}
