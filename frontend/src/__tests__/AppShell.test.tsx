import { describe, it, expect } from 'vitest'
import { render, screen } from '@testing-library/react'
import { MemoryRouter } from 'react-router-dom'
import { AppShell } from '../components/AppShell'

describe('AppShell', () => {
  it('renders sidebar and topbar', () => {
    render(
      <MemoryRouter>
        <AppShell>
          <div>Content</div>
        </AppShell>
      </MemoryRouter>,
    )
    expect(screen.getByText('PrintHub')).toBeInTheDocument()
    expect(screen.getByLabelText('Primary navigation')).toBeInTheDocument()
    expect(screen.getByLabelText('Notifications')).toBeInTheDocument()
  })
})
