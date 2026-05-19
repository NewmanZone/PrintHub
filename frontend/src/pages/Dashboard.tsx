import React from 'react'
import { Package, Printer, TrendingUp, DollarSign } from 'lucide-react'
import { MetricCard } from '../components/ui/MetricCard'
import { Panel } from '../components/ui/Panel'
import { DataTable } from '../components/ui/DataTable'
import { mockDashboard, mockProducts, type MockProduct } from '../mocks'

export const Dashboard: React.FC = () => {
  const { thisMonth, vsLastMonth, alerts } = mockDashboard

  return (
    <div>
      <h1 style={{ fontSize: 'var(--text-2xl)', fontWeight: 'var(--font-bold)', marginBottom: 'var(--space-6)' }}>
        Dashboard
      </h1>

      <div
        style={{
          display: 'grid',
          gridTemplateColumns: 'repeat(auto-fit, minmax(200px, 1fr))',
          gap: 'var(--space-4)',
          marginBottom: 'var(--space-6)',
        }}
      >
        <MetricCard
          label="Products Sold"
          value={thisMonth.productsSold}
          change={{ direction: 'up', text: `${(vsLastMonth.productsSoldChange * 100).toFixed(0)}% vs last month` }}
          icon={<Package size={18} />}
        />
        <MetricCard
          label="Print Jobs"
          value={thisMonth.printJobs}
          change={{ direction: 'up', text: `${(vsLastMonth.printJobsChange * 100).toFixed(0)}% vs last month` }}
          icon={<Printer size={18} />}
        />
        <MetricCard
          label="Revenue"
          value={`$${thisMonth.revenue.toFixed(2)}`}
          change={{ direction: 'up', text: `${(vsLastMonth.revenueChange * 100).toFixed(0)}% vs last month` }}
          icon={<TrendingUp size={18} />}
        />
        <MetricCard
          label="Print Cost"
          value={`$${thisMonth.printCost.toFixed(2)}`}
          change={{ direction: 'down', text: `${Math.abs(vsLastMonth.printCostChange * 100).toFixed(0)}% vs last month` }}
          icon={<DollarSign size={18} />}
        />
      </div>

      {alerts.length > 0 && (
        <Panel title="Alerts" style={{ marginBottom: 'var(--space-6)' }}>
          <ul style={{ margin: 0, paddingLeft: 'var(--space-5)', color: 'var(--status-error)' }}>
            {alerts.map((a) => (
              <li key={a.message}>{a.message}</li>
            ))}
          </ul>
        </Panel>
      )}

      <Panel title="Products">
        <DataTable
          columns={[
            { key: 'name', header: 'Name' },
            { key: 'etsyPrice', header: 'Price', width: '100px' },
            { key: 'inventoryOnHand', header: 'Stock', width: '80px' },
            { key: 'reorderPoint', header: 'Reorder', width: '90px' },
          ]}
          rows={mockProducts}
          keyExtractor={(p: MockProduct) => p.id}
        />
      </Panel>
    </div>
  )
}
