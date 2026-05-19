import React from 'react'
import './EmptyState.css'

interface EmptyStateProps {
  icon?: React.ReactNode
  title: string
  description?: string
  action?: React.ReactNode
  className?: string
}

export const EmptyState: React.FC<EmptyStateProps> = ({ icon, title, description, action, className = '' }) => {
  return (
    <div className={`ph-empty-state ${className}`}>
      {icon && <div className="ph-empty-state__icon">{icon}</div>}
      <h4 className="ph-empty-state__title">{title}</h4>
      {description && <p className="ph-empty-state__desc">{description}</p>}
      {action && <div className="ph-empty-state__action">{action}</div>}
    </div>
  )
}
