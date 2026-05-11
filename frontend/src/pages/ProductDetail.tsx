import React from 'react'
import { useParams } from 'react-router-dom'
import { Panel } from '../components/ui/Panel'
import { StatusChip } from '../components/ui/StatusChip'
import { ErrorState } from '../components/ui/ErrorState'
import { mockProducts } from '../mocks'

export const ProductDetail: React.FC = () => {
  const { id } = useParams<{ id: string }>()
  const product = mockProducts.find((p) => p.id === id)

  if (!product) {
    return <ErrorState title="Product not found" message={`No product matches ID ${id}.`} />
  }

  const isLow = product.inventoryOnHand < product.reorderPoint

  return (
    <div>
      <h1 style={{ fontSize: 'var(--text-2xl)', fontWeight: 'var(--font-bold)', marginBottom: 'var(--space-6)' }}>
        {product.name}
      </h1>

      <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(280px, 1fr))', gap: 'var(--space-4)' }}>
        <Panel title="Overview">
          <p>
            <strong>Price:</strong> ${product.etsyPrice.toFixed(2)}
          </p>
          <p>
            <strong>Printed:</strong> {product.printCount} times
          </p>
          <p>
            <strong>Cost per print:</strong> ${product.costPerPrint.toFixed(2)}
          </p>
          <div style={{ marginTop: 'var(--space-4)' }}>
            <StatusChip status={isLow ? 'warning' : 'success'} label={isLow ? 'Low stock' : 'Stock OK'} />
          </div>
        </Panel>

        <Panel title="Parts">
          <ul style={{ margin: 0, paddingLeft: 'var(--space-5)' }}>
            {product.parts.map((part) => (
              <li key={part.partId}>
                {part.partName} {part.isGeneric && <span style={{ color: 'var(--muted)' }}>(generic)</span>} × {part.quantityPerProduct}
              </li>
            ))}
          </ul>
        </Panel>

        <Panel title="Inventory">
          <p>
            <strong>On hand:</strong> {product.inventoryOnHand}
          </p>
          <p>
            <strong>Reorder point:</strong> {product.reorderPoint}
          </p>
          <p>
            <strong>Reorder quantity:</strong> {product.reorderQuantity}
          </p>
        </Panel>
      </div>
    </div>
  )
}
