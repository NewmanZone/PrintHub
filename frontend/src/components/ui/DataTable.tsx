import React from 'react'
import './DataTable.css'

interface Column<T> {
  key: string
  header: string
  width?: string
  render?: (row: T) => React.ReactNode
}

interface DataTableProps<T> {
  columns: Column<T>[]
  rows: T[]
  keyExtractor: (row: T) => string
  emptyState?: React.ReactNode
  className?: string
}

export function DataTable<T>({ columns, rows, keyExtractor, emptyState, className = '' }: DataTableProps<T>) {
  if (rows.length === 0 && emptyState) {
    return <div className={`ph-data-table ph-data-table--empty ${className}`}>{emptyState}</div>
  }

  return (
    <div className={`ph-data-table ${className}`}>
      <table className="ph-data-table__table">
        <thead>
          <tr>
            {columns.map((col) => (
              <th
                key={col.key}
                className="ph-data-table__th"
                style={col.width ? { width: col.width } : undefined}
              >
                {col.header}
              </th>
            ))}
          </tr>
        </thead>
        <tbody>
          {rows.map((row) => (
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
