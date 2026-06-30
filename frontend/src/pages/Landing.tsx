import React from 'react'
import { Link } from 'react-router-dom'
import { BarChart3, Layers3, Printer, ShieldCheck } from 'lucide-react'
import { Button } from '../components/ui/Button'
import { Panel } from '../components/ui/Panel'

const features = [
  { icon: Layers3, title: 'BOM-aware production', text: 'Collapse Etsy demand into the generic and personalized parts your printers actually need.' },
  { icon: Printer, title: 'Printer operations', text: 'Track Bambu and OctoEverywhere-connected printers from the same production workspace.' },
  { icon: BarChart3, title: 'Inventory signals', text: 'Spot low stock, print cost, velocity, and queue pressure before orders start slipping.' },
]

export const Landing: React.FC = () => {
  return (
    <main className="ph-landing surface-grid">
      <header className="ph-landing__nav">
        <div className="ph-landing__brand">
          <span className="ph-landing__mark">P</span>
          <span>PrintHub</span>
        </div>
        <Link to="/dashboard">
          <Button size="sm">Continue with OAuth</Button>
        </Link>
      </header>

      <section className="ph-landing__hero">
        <p className="ph-page-kicker">3D print operations for Etsy sellers</p>
        <h1>PrintHub</h1>
        <p>
          Convert orders into a clean print queue, protect inventory, and keep every printer job moving without spreadsheet drift.
        </p>
        <div className="ph-landing__actions">
          <Link to="/dashboard"><Button size="lg">Open demo workspace</Button></Link>
          <a href="#features"><Button size="lg" variant="secondary">Explore features</Button></a>
        </div>
      </section>

      <section id="features" className="ph-landing__features">
        {features.map((feature) => {
          const Icon = feature.icon
          return (
            <Panel key={feature.title}>
              <div className="ph-landing__feature-icon"><Icon size={22} /></div>
              <h2>{feature.title}</h2>
              <p>{feature.text}</p>
            </Panel>
          )
        })}
      </section>

      <section className="ph-landing__security">
        <ShieldCheck size={20} />
        <span>OAuth-only sign in. No password-based auth UI.</span>
      </section>
    </main>
  )
}
