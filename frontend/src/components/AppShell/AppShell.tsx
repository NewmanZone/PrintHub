import React from 'react'
import { Sidebar } from './Sidebar'
import { TopBar } from './TopBar'
import { MobileBottomNav } from './MobileBottomNav'
import './AppShell.css'

interface AppShellProps {
  children: React.ReactNode
}

export const AppShell: React.FC<AppShellProps> = ({ children }) => {
  const [mobileMenuOpen, setMobileMenuOpen] = React.useState(false)

  return (
    <div className="ph-app-shell">
      <Sidebar collapsed={false} />
      <TopBar onMenuToggle={() => setMobileMenuOpen((v) => !v)} />
      <main className="ph-app-shell__main">
        <div className="ph-app-shell__content">{children}</div>
      </main>
      <MobileBottomNav />
    </div>
  )
}
