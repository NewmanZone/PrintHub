import { beforeAll, describe, expect, it, vi } from 'vitest'
import { fireEvent, render, screen, within } from '@testing-library/react'
import { MemoryRouter } from 'react-router-dom'
import { Bundles, Jobs, Products } from '../pages'

beforeAll(() => {
  Object.defineProperty(URL, 'createObjectURL', {
    configurable: true,
    value: vi.fn(() => 'blob:manifest'),
  })
  Object.defineProperty(URL, 'revokeObjectURL', {
    configurable: true,
    value: vi.fn(),
  })
  HTMLAnchorElement.prototype.click = vi.fn()
})

describe('workspace pages', () => {
  it('filters products by search term and active state', async () => {
    render(
      <MemoryRouter>
        <Products />
      </MemoryRouter>,
    )

    expect(await screen.findByText('Dino Wall Hook')).toBeInTheDocument()
    fireEvent.change(screen.getByLabelText('Search products'), { target: { value: 'cat' } })
    expect(screen.getByText('Cat Wall Hook')).toBeInTheDocument()
    expect(screen.queryByText('Dino Wall Hook')).not.toBeInTheDocument()

    fireEvent.change(screen.getByLabelText('Filter products by status'), { target: { value: 'active' } })
    expect(screen.getByText('No products match')).toBeInTheDocument()
  })

  it('filters jobs by canonical status', () => {
    render(
      <MemoryRouter>
        <Jobs />
      </MemoryRouter>,
    )

    expect(screen.getByText('job_001')).toBeInTheDocument()
    expect(screen.getByText('job_002')).toBeInTheDocument()

    fireEvent.change(screen.getByLabelText('Filter jobs by status'), { target: { value: 'Paused' } })
    const table = screen.getByRole('table')
    expect(within(table).getByText('job_003')).toBeInTheDocument()
    expect(within(table).queryByText('job_001')).not.toBeInTheDocument()
  })

  it('renders preparation bundles for manual download', () => {
    render(
      <MemoryRouter>
        <Bundles />
      </MemoryRouter>,
    )

    expect(screen.getByRole('heading', { name: 'Preparation Bundles' })).toBeInTheDocument()
    expect(screen.getAllByText('bundle_001')).not.toHaveLength(0)
    expect(screen.getAllByRole('button', { name: /Download/i })).not.toHaveLength(0)
  })

  it('filters preparation bundles by search and status', () => {
    render(
      <MemoryRouter>
        <Bundles />
      </MemoryRouter>,
    )

    fireEvent.change(screen.getByLabelText('Search bundles'), { target: { value: 'Mia' } })
    const table = screen.getByRole('table')
    expect(within(table).getByText('bundle_002')).toBeInTheDocument()
    expect(within(table).queryByText('bundle_001')).not.toBeInTheDocument()

    fireEvent.change(screen.getByLabelText('Filter bundles by status'), { target: { value: 'Printed' } })
    expect(screen.getByText('No bundles match')).toBeInTheDocument()
  })

  it('validates and creates a manual preparation bundle', () => {
    render(
      <MemoryRouter>
        <Bundles />
      </MemoryRouter>,
    )

    fireEvent.click(screen.getByRole('button', { name: 'Create manual bundle' }))
    fireEvent.click(screen.getByRole('button', { name: 'Save bundle' }))
    expect(screen.getByRole('alert')).toHaveTextContent('Order ID and customer name are required.')

    fireEvent.change(screen.getByLabelText('Etsy order ID'), { target: { value: 'etsy_order_98770' } })
    fireEvent.change(screen.getByLabelText('Customer'), { target: { value: 'Dad' } })
    fireEvent.change(screen.getByLabelText('Files'), { target: { value: '2' } })
    fireEvent.change(screen.getByLabelText('Items'), { target: { value: '4' } })
    fireEvent.change(screen.getByLabelText('Notes'), { target: { value: 'Two signs with name personalization.' } })
    fireEvent.click(screen.getByRole('button', { name: 'Save bundle' }))

    expect(screen.getByText('bundle_004 is ready to download.')).toBeInTheDocument()
    expect(screen.getByText('etsy_order_98770')).toBeInTheDocument()
    expect(screen.getByText('Dad')).toBeInTheDocument()
  })

  it('prepares a manifest download and lets a bundle be marked printed', () => {
    render(
      <MemoryRouter>
        <Bundles />
      </MemoryRouter>,
    )

    fireEvent.click(screen.getAllByRole('button', { name: 'Download' })[0])
    expect(screen.getByText('bundle_001-manifest.json')).toBeInTheDocument()
    expect(screen.getByText('bundle_001-manifest.json prepared.')).toBeInTheDocument()

    fireEvent.click(screen.getAllByRole('button', { name: 'Mark printed' })[0])
    expect(screen.getByText('bundle_001 marked printed.')).toBeInTheDocument()
    expect(screen.getAllByText('Printed')).not.toHaveLength(0)
  })
})
