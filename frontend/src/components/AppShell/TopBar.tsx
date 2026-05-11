import React from 'react'
import { Bell, Search, User } from 'lucide-react'
import './TopBar.css'

interface TopBarProps {
  onMenuToggle?: () => void
}

export const TopBar: React.FC<TopBarProps> = ({ onMenuToggle }) => {
  return (
    <header className="ph-topbar">
      <div className="ph-topbar__left">
        <button
          className="ph-topbar__menu-btn"
          onClick={onMenuToggle}
          aria-label="Toggle navigation menu"
        >
          <span />
          <span />
          <span />
        </button>
        <div className="ph-topbar__search">
          <Search size={16} strokeWidth={1.8} />
          <input type="text" placeholder="Search products, parts, jobs..." />
        </div>
      </div>

      <div className="ph-topbar__right">
        <button className="ph-topbar__icon-btn" aria-label="Notifications">
          <Bell size={18} strokeWidth={1.8} />
          <span className="ph-topbar__badge" aria-hidden="true">2</span>
        </button>
        <button className="ph-topbar__avatar" aria-label="Account">
          <User size={18} strokeWidth={1.8} />
        </button>
      </div>
    </header>
  )
}
