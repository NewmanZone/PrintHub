import { describe, expect, it } from 'vitest'
import { render, screen, within } from '@testing-library/react'
import { MemoryRouter } from 'react-router-dom'
import App from '../App'

const renderRoute = (route: string) =>
  render(
    <MemoryRouter initialEntries={[route]}>
      <App />
    </MemoryRouter>,
  )

describe('App routes', () => {
  it('renders the public landing page at root without the workspace sidebar', () => {
    renderRoute('/')

    expect(screen.getByRole('heading', { name: 'PrintHub' })).toBeInTheDocument()
    expect(screen.getByText('3D print operations for Etsy sellers')).toBeInTheDocument()
    expect(screen.queryByLabelText('Primary navigation')).not.toBeInTheDocument()
  })

  it('renders the dashboard inside the authenticated app shell', async () => {
    renderRoute('/dashboard')

    expect(screen.getByLabelText('Primary navigation')).toBeInTheDocument()
    expect(screen.getByRole('heading', { name: 'Dashboard' })).toBeInTheDocument()
    expect(screen.getByText('Operations overview')).toBeInTheDocument()
    expect(await screen.findByText('Dino Wall Hook')).toBeInTheDocument()
  })

  it('renders a valid product detail page', async () => {
    renderRoute('/products/prod_001')

    expect(await screen.findByRole('heading', { name: 'Dino Wall Hook' })).toBeInTheDocument()
    expect(screen.getByText('Print files')).toBeInTheDocument()
    expect(screen.getByText('dino-hook.3mf')).toBeInTheDocument()
  })

  it('renders an error state for an unknown product id', async () => {
    renderRoute('/products/missing')

    expect(await screen.findByText('Product not found')).toBeInTheDocument()
    expect(screen.getByText('No product matches ID missing.')).toBeInTheDocument()
  })

  it('renders a valid job detail page with operator controls', () => {
    renderRoute('/jobs/job_001')

    expect(screen.getByRole('heading', { name: 'job_001' })).toBeInTheDocument()
    expect(screen.getByRole('button', { name: 'Pause' })).toBeInTheDocument()
    expect(screen.getByRole('button', { name: 'Resume' })).toBeInTheDocument()
    expect(screen.getByRole('button', { name: 'Cancel' })).toBeInTheDocument()
  })

  it('renders not found for unknown app routes', () => {
    renderRoute('/not-a-real-page')

    expect(screen.getByText('Page not found')).toBeInTheDocument()
    expect(screen.getByRole('link', { name: 'Back to dashboard' })).toHaveAttribute('href', '/dashboard')
  })

  it('renders bundles as a Phase 1 primary route', () => {
    renderRoute('/bundles')

    const nav = screen.getByLabelText('Primary navigation')
    expect(screen.getByRole('heading', { name: 'Preparation Bundles' })).toBeInTheDocument()
    expect(within(nav).getByRole('link', { name: /Bundles/i })).toHaveAttribute('aria-current', 'page')
  })
})
