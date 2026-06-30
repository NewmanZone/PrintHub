import React from 'react'
import { NavLink, useLocation } from 'react-router-dom'
import {
  Archive,
  LayoutDashboard,
  Package,
  Settings,
  ShoppingCart,
} from 'lucide-react'
import './MobileBottomNav.css'

const mobileItems = [
  { to: '/dashboard', label: 'Dashboard', icon: LayoutDashboard },
  { to: '/orders', label: 'Orders', icon: ShoppingCart },
  { to: '/products', label: 'Products', icon: Package },
  { to: '/bundles', label: 'Bundles', icon: Archive },
  { to: '/settings', label: 'More', icon: Settings },
]

export const MobileBottomNav: React.FC = () => {
  const location = useLocation()

  return (
    <nav className="ph-mobile-nav" aria-label="Mobile navigation">
      {mobileItems.map((item) => {
        const isActive = location.pathname === item.to || location.pathname.startsWith(`${item.to}/`)
        const Icon = item.icon
        return (
          <NavLink
            key={item.to}
            to={item.to}
            className={`ph-mobile-nav__item ${isActive ? 'ph-mobile-nav__item--active' : ''}`}
            aria-current={isActive ? 'page' : undefined}
          >
            <span className="ph-mobile-nav__icon">
              <Icon size={20} strokeWidth={1.8} />
            </span>
            <span className="ph-mobile-nav__label">{item.label}</span>
          </NavLink>
        )
      })}
    </nav>
  )
}
