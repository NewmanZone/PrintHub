import { describe, it, expect } from 'vitest'
import { render, screen } from '@testing-library/react'
import { Button } from '../components/ui/Button'

describe('Button', () => {
  it('renders with label', () => {
    render(<Button>Click me</Button>)
    expect(screen.getByText('Click me')).toBeInTheDocument()
  })

  it('is disabled when disabled prop is true', () => {
    render(<Button disabled>Disabled</Button>)
    expect(screen.getByRole('button')).toBeDisabled()
  })

  it('applies fullWidth class', () => {
    const { container } = render(<Button fullWidth>Wide</Button>)
    expect(container.querySelector('.ph-button--full')).toBeInTheDocument()
  })
})
