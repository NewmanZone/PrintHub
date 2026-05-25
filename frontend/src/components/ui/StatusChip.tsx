import React from 'react'
import './StatusChip.css'

export type Status =
  | 'online'
  | 'busy'
  | 'offline'
  | 'error'
  | 'warning'
  | 'success'
  | 'draft'
  | 'pending'
  | 'queued'
  | 'in-progress'
  | 'paused'
  | 'completed'
  | 'failed'
  | 'cancelled'

interface StatusChipProps {
  status: Status
  label: string
}

export const StatusChip: React.FC<StatusChipProps> = ({ status, label }) => {
  return (
    <span className={`ph-status-chip ph-status-chip--${status}`}>
      <span className="ph-status-chip__dot" aria-hidden="true" />
      {label}
    </span>
  )
}
