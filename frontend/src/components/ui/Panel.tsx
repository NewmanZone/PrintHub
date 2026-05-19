import React from 'react'
import './Panel.css'

interface PanelProps {
  title?: string
  children: React.ReactNode
  actions?: React.ReactNode
  className?: string
  style?: React.CSSProperties
}

export const Panel: React.FC<PanelProps> = ({ title, children, actions, className = '', style }) => {
  return (
    <div className={`ph-panel ${className}`} style={style}>
      {(title || actions) && (
        <div className="ph-panel__header">
          {title && <h3 className="ph-panel__title">{title}</h3>}
          {actions && <div className="ph-panel__actions">{actions}</div>}
        </div>
      )}
      <div className="ph-panel__body">{children}</div>
    </div>
  )
}
