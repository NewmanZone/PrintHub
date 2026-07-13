import { beforeAll, describe, expect, it, vi } from 'vitest'
import { fireEvent, render, screen, waitFor } from '@testing-library/react'
import { MemoryRouter } from 'react-router-dom'
import { Bundles, Products, Settings } from '../pages'

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

  it('completes an Etsy OAuth callback through the workspace API', async () => {
    window.history.pushState({}, '', '/settings?etsy=callback&code=oauth-code&state=oauth-state')

    render(
      <MemoryRouter>
        <Settings />
      </MemoryRouter>,
    )

    expect(await screen.findByText('Newman Zone is connected.')).toBeInTheDocument()
    await waitFor(() => expect(window.location.pathname + window.location.search).toBe('/settings'))
    expect(fetch).toHaveBeenCalledWith(
      '/workspaces/workspace_001/shops/etsy/callback',
      expect.objectContaining({
        method: 'POST',
        body: JSON.stringify({ code: 'oauth-code', state: 'oauth-state' }),
      }),
    )
  })
})
