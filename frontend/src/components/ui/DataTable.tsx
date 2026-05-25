import React from 'react'
import './DataTable.css'

export interface Column<T> {
  key: string
  header: string
  width?: string
  render?: (row: T) => React.ReactNode
  sortValue?: (row: T) => string | number
  sortable?: boolean
}

interface DataTableProps<T> {
  columns: Column<T>[]
  rows: T[]
  keyExtractor: (row: T) => string
  emptyState?: React.ReactNode
  className?: string
  caption?: string
}

type SortState = { key: string; direction: 'asc' | 'desc' }

export function DataTable<T>({
  columns,
  rows,
  keyExtractor,
  emptyState,
  className = '',
  caption,
}: DataTableProps<T>) {
  const [sort, setSort] = React.useState<SortState | null>(null)

  const sortedRows = React.useMemo(() => {
    if (!sort) return rows
    const column = columns.find((col) => col.key === sort.key)
    if (!column) return rows

    const read = (row: T) => {
      if (column.sortValue) return column.sortValue(row)
      const value = (row as Record<string, unknown>)[column.key]
      return typeof value === 'number' || typeof value === 'string' ? value : String(value ?? '')
    }

    return [...rows].sort((a, b) => {
      const av = read(a)
      const bv = read(b)
      const order = typeof av === 'number' && typeof bv === 'number'
        ? av - bv
        : String(av).localeCompare(String(bv), undefined, { numeric: true, sensitivity: 'base' })
      return sort.direction === 'asc' ? order : -order
    })
  }, [columns, rows, sort])

  const updateSort = (key: string) => {
    setSort((current) => {
      if (!current || current.key !== key) return { key, direction: 'asc' }
      if (current.direction === 'asc') return { key, direction: 'desc' }
      return null
    })
  }

  if (rows.length === 0 && emptyState) {
    return <div className={`ph-data-table ph-data-table--empty ${className}`}>{emptyState}</div>
  }

  return (
    <div className={`ph-data-table ${className}`}>
      <table className="ph-data-table__table">
        {caption && <caption className="sr-only">{caption}</caption>}
        <thead>
          <tr>
            {columns.map((col) => (
              <th
                key={col.key}
                className="ph-data-table__th"
                style={col.width ? { width: col.width } : undefined}
              >
                {col.sortable === false || col.header.trim().length === 0 ? (
                  <span className="ph-data-table__static-header">{col.header}</span>
                ) : (
                  <button
                    type="button"
                    className="ph-data-table__sort"
                    onClick={() => updateSort(col.key)}
                    aria-label={`Sort by ${col.header}`}
                  >
                    <span>{col.header}</span>
                  </button>
                )}
              </th>
            ))}
          </tr>
        </thead>
        <tbody>
          {sortedRows.map((row) => (
            <tr key={keyExtractor(row)} className="ph-data-table__tr">
              {columns.map((col) => (
                <td key={col.key} className="ph-data-table__td">
                  {col.render ? col.render(row) : (row as Record<string, unknown>)[col.key] as React.ReactNode}
                </td>
              ))}
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  )
}
