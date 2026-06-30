import { beforeAll, describe, expect, it, vi } from 'vitest'
import { fireEvent, render, screen } from '@testing-library/react'
import { MemoryRouter } from 'react-router-dom'
import { Bundles, Products } from '../pages'

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

  it('renders preparation bundles as a future live workflow', () => {
    render(
      <MemoryRouter>
        <Bundles />
      </MemoryRouter>,
    )

    expect(screen.getByRole('heading', { name: 'Preparation Bundles' })).toBeInTheDocument()
    expect(screen.getByText('No live bundle workflow yet')).toBeInTheDocument()
  })
})
