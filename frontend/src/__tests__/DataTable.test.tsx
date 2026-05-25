import { describe, expect, it } from 'vitest'
import { fireEvent, render, screen, within } from '@testing-library/react'
import { DataTable } from '../components/ui/DataTable'
import { EmptyState } from '../components/ui/EmptyState'

const rows = [
  { id: 'b', name: 'Beta', count: 2 },
  { id: 'a', name: 'Alpha', count: 10 },
  { id: 'c', name: 'Gamma', count: 1 },
]

describe('DataTable', () => {
  it('renders headers and row cells', () => {
    render(
      <DataTable
        caption="Example table"
        columns={[{ key: 'name', header: 'Name' }, { key: 'count', header: 'Count' }]}
        rows={rows}
        keyExtractor={(row) => row.id}
      />,
    )

    expect(screen.getByText('Name')).toBeInTheDocument()
    expect(screen.getByText('Alpha')).toBeInTheDocument()
    expect(screen.getByText('10')).toBeInTheDocument()
  })

  it('sorts ascending, descending, then resets to original order', () => {
    render(
      <DataTable
        columns={[{ key: 'name', header: 'Name' }, { key: 'count', header: 'Count', sortValue: (row) => row.count }]}
        rows={rows}
        keyExtractor={(row) => row.id}
      />,
    )

    const countSort = screen.getByRole('button', { name: 'Sort by Count' })

    fireEvent.click(countSort)
    let renderedRows = screen.getAllByRole('row').slice(1)
    expect(within(renderedRows[0]).getByText('Gamma')).toBeInTheDocument()
    expect(within(renderedRows[2]).getByText('Alpha')).toBeInTheDocument()

    fireEvent.click(countSort)
    renderedRows = screen.getAllByRole('row').slice(1)
    expect(within(renderedRows[0]).getByText('Alpha')).toBeInTheDocument()
    expect(within(renderedRows[2]).getByText('Gamma')).toBeInTheDocument()

    fireEvent.click(countSort)
    renderedRows = screen.getAllByRole('row').slice(1)
    expect(within(renderedRows[0]).getByText('Beta')).toBeInTheDocument()
  })

  it('renders an empty state when provided rows are empty', () => {
    render(
      <DataTable
        columns={[{ key: 'name', header: 'Name' }]}
        rows={[]}
        keyExtractor={(row: { id: string }) => row.id}
        emptyState={<EmptyState title="Nothing here" description="Try again later." />}
      />,
    )

    expect(screen.getByText('Nothing here')).toBeInTheDocument()
    expect(screen.getByText('Try again later.')).toBeInTheDocument()
    expect(screen.queryByRole('table')).not.toBeInTheDocument()
  })
})
