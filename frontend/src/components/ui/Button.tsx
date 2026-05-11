import React from 'react'
import './Button.css'

type ButtonVariant = 'primary' | 'secondary' | 'ghost' | 'danger'
type ButtonSize = 'sm' | 'md' | 'lg'

interface ButtonProps extends React.ButtonHTMLAttributes<HTMLButtonElement> {
  variant?: ButtonVariant
  size?: ButtonSize
  iconLeft?: React.ReactNode
  iconRight?: React.ReactNode
  fullWidth?: boolean
}

export const Button = React.forwardRef<HTMLButtonElement, ButtonProps>(
  (
    { variant = 'primary', size = 'md', iconLeft, iconRight, fullWidth, children, className = '', ...rest },
    ref,
  ) => {
    const cls = [
      'ph-button',
      `ph-button--${variant}`,
      `ph-button--${size}`,
      fullWidth ? 'ph-button--full' : '',
      className,
    ]
      .filter(Boolean)
      .join(' ')

    return (
      <button ref={ref} className={cls} {...rest}>
        {iconLeft && <span className="ph-button__icon ph-button__icon--left">{iconLeft}</span>}
        <span className="ph-button__label">{children}</span>
        {iconRight && <span className="ph-button__icon ph-button__icon--right">{iconRight}</span>}
      </button>
    )
  },
)

Button.displayName = 'Button'
