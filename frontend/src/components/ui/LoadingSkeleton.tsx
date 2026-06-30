import React from 'react'
import './LoadingSkeleton.css'

interface LoadingSkeletonProps {
  rows?: number
  columns?: number
  className?: string
}

export const LoadingSkeleton: React.FC<LoadingSkeletonProps> = ({ rows = 4, columns = 4, className = '' }) => {
  return (
    <div className={`ph-skeleton ${className}`} aria-busy="true" aria-label="Loading">
      <div className="ph-skeleton__header">
        {Array.from({ length: columns }).map((_, i) => (
          <div key={i} className="ph-skeleton__cell ph-skeleton__cell--header" />
        ))}
      </div>
      {Array.from({ length: rows }).map((_, r) => (
        <div key={r} className="ph-skeleton__row">
          {Array.from({ length: columns }).map((_, c) => (
            <div key={c} className="ph-skeleton__cell" />
          ))}
        </div>
      ))}
    </div>
  )
}
