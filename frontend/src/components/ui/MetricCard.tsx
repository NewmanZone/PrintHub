import React from 'react'
import './MetricCard.css'

interface MetricCardProps {
  label: string
  value: string | number
  change?: { direction: 'up' | 'down'; text: string }
  icon?: React.ReactNode
  className?: string
}

export const MetricCard: React.FC<MetricCardProps> = ({ label, value, change, icon, className = '' }) => {
  return (
    <div className={`ph-metric-card ${className}`}>
      <div className="ph-metric-card__top">
        <span className="ph-metric-card__label">{label}</span>
        {icon && <div className="ph-metric-card__icon">{icon}</div>}
      </div>
      <div className="ph-metric-card__value">{value}</div>
      {change && (
        <div className={`ph-metric-card__change ph-metric-card__change--${change.direction}`}>
          {change.direction === 'up' ? 'up' : 'down'} {change.text}
        </div>
      )}
    </div>
  )
}
