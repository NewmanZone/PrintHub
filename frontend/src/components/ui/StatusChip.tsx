import React from 'react'
import './StatusChip.css'

type Status = 'online' | 'busy' | 'offline' | 'error' | 'warning' | 'success'

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
