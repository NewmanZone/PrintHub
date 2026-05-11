import { describe, it, expect } from 'vitest'
import { render, screen } from '@testing-library/react'
import { StatusChip } from '../components/ui/StatusChip'

describe('StatusChip', () => {
  it('renders label', () => {
    render(<StatusChip status="online" label="Online" />)
    expect(screen.getByText('Online')).toBeInTheDocument()
  })

  it('renders status dot', () => {
    const { container } = render(<StatusChip status="online" label="Online" />)
    expect(container.querySelector('.ph-status-chip__dot')).toBeInTheDocument()
  })
})
