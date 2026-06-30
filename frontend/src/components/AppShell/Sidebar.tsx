import React from 'react'
import { NavLink, useLocation } from 'react-router-dom'
import {
  LayoutDashboard,
  Package,
  Archive,
  Settings,
  ChevronDown,
  Boxes,
  ShoppingCart,
} from 'lucide-react'
import './Sidebar.css'

const navItems = [
  { to: '/dashboard', label: 'Dashboard', icon: LayoutDashboard },
  { to: '/orders', label: 'Orders', icon: ShoppingCart },
  { to: '/products', label: 'Products', icon: Package },
  { to: '/parts', label: 'Parts', icon: Boxes },
  { to: '/bundles', label: 'Bundles', icon: Archive },
  { to: '/settings', label: 'Settings', icon: Settings },
]

interface SidebarProps {
  shopName?: string
  collapsed?: boolean
}

export const Sidebar: React.FC<SidebarProps> = ({ shopName = 'My Print Shop', collapsed = false }) => {
  const location = useLocation()

  return (
    <aside
      className={`ph-sidebar ${collapsed ? 'ph-sidebar--collapsed' : ''}`}
      aria-label="Primary navigation"
    >
      <div className="ph-sidebar__brand">
        <div className="ph-sidebar__brand-mark" aria-hidden="true">P</div>
        {!collapsed && <span className="ph-sidebar__brand-name">PrintHub</span>}
      </div>

      <div className="ph-sidebar__shop">
        {!collapsed && (
          <>
            <span className="ph-sidebar__shop-label">Shop</span>
            <button className="ph-sidebar__shop-toggle">
            <span className="ph-sidebar__shop-name">{shopName}</span>
              <ChevronDown size={14} />
            </button>
          </>
        )}
        {collapsed && <span className="ph-sidebar__shop-dot" aria-hidden="true" />}
      </div>

      <nav className="ph-sidebar__nav">
        {navItems.map((item) => {
          const isActive = location.pathname === item.to || location.pathname.startsWith(`${item.to}/`)
          const Icon = item.icon
          return (
            <NavLink
              key={item.to}
              to={item.to}
              className={`ph-sidebar__link ${isActive ? 'ph-sidebar__link--active' : ''}`}
              aria-current={isActive ? 'page' : undefined}
            >
              <span className="ph-sidebar__link-icon">
                <Icon size={18} strokeWidth={1.8} />
              </span>
              {!collapsed && <span className="ph-sidebar__link-label">{item.label}</span>}
            </NavLink>
          )
        })}
      </nav>
    </aside>
  )
}
