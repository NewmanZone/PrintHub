import React from 'react'
import { AlertTriangle } from 'lucide-react'
import './ErrorState.css'

interface ErrorStateProps {
  title?: string
  message?: string
  onRetry?: () => void
  className?: string
}

export const ErrorState: React.FC<ErrorStateProps> = ({
  title = 'Something went wrong',
  message = 'We could not load the data. Please try again later.',
  onRetry,
  className = '',
}) => {
  return (
    <div className={`ph-error-state ${className}`}>
      <div className="ph-error-state__icon">
        <AlertTriangle size={28} strokeWidth={1.8} />
      </div>
      <h4 className="ph-error-state__title">{title}</h4>
      <p className="ph-error-state__desc">{message}</p>
      {onRetry && (
        <button className="ph-error-state__retry" onClick={onRetry}>
          Retry
        </button>
      )}
    </div>
  )
}
