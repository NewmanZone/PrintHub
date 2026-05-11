import React, { useEffect, useState } from 'react';
import { api } from '../services/mockData';
import type { DashboardStats, WorkspaceAlert } from '../types';

const Dashboard: React.FC = () => {
  const [stats, setStats] = useState<DashboardStats | null>(null);
  const [alerts, setAlerts] = useState<WorkspaceAlert[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const loadDashboard = async () => {
      try {
        const [statsData, alertsData] = await Promise.all([
          api.getDashboardStats(),
          api.getAlerts(),
        ]);
        setStats(statsData);
        setAlerts(alertsData);
      } catch (error) {
        console.error('Failed to load dashboard:', error);
      } finally {
        setLoading(false);
      }
    };
    loadDashboard();
  }, []);

  if (loading) {
    return (
      <div className="loading">
        <div className="spinner" />
      </div>
    );
  }

  if (!stats) {
    return <div className="empty-state">Failed to load dashboard data.</div>;
  }

  return (
    <div>
      <div className="page-header">
        <h1 className="page-title">Dashboard</h1>
        <p className="page-description">
          Welcome back! Here's your print operations overview.
        </p>
      </div>

      {/* Alerts Section */}
      {alerts.length > 0 && (
        <div className="mb-4">
          {alerts.map((alert) => (
            <div key={alert.id} className={`alert alert-${alert.type}`}>
              <AlertIcon type={alert.type} className="alert-icon" />
              <div className="alert-content">
                <div className="alert-title">{alert.title}</div>
                <div className="alert-message">{alert.message}</div>
                {alert.action && (
                  <div className="alert-action">
                    <a href={alert.action.href} className="btn btn-sm btn-primary">
                      {alert.action.label}
                    </a>
                  </div>
                )}
              </div>
            </div>
          ))}
        </div>
      )}

      {/* Stats Grid */}
      <div className="stats-grid">
        <div className="stat-card">
          <div className="stat-label">Total Products</div>
          <div className="stat-value">{stats.totalProducts}</div>
        </div>
        <div className="stat-card">
          <div className="stat-label">Active Orders</div>
          <div className="stat-value">{stats.activeOrders}</div>
        </div>
        <div className="stat-card">
          <div className="stat-label">Print Queue Items</div>
          <div className="stat-value">{stats.printQueueItems}</div>
        </div>
        <div className="stat-card">
          <div className="stat-label">Low Stock Items</div>
          <div className="stat-value" style={{ color: stats.lowStockCount > 0 ? 'var(--color-warning)' : undefined }}>
            {stats.lowStockCount}
          </div>
        </div>
        <div className="stat-card">
          <div className="stat-label">Printers Online</div>
          <div className="stat-value">{stats.printersOnline}/3</div>
        </div>
        <div className="stat-card">
          <div className="stat-label">Revenue This Month</div>
          <div className="stat-value">${stats.revenueThisMonth.toFixed(2)}</div>
        </div>
      </div>

      {/* Quick Actions */}
      <div className="grid grid-2 mt-4">
        <div className="card">
          <div className="card-header">
            <h3 className="card-title">Quick Actions</h3>
          </div>
          <div className="flex gap-2" style={{ flexWrap: 'wrap' }}>
            <a href="/jobs" className="btn btn-primary">View Print Queue</a>
            <a href="/orders" className="btn btn-secondary">View Orders</a>
            <a href="/printers" className="btn btn-secondary">Printer Status</a>
          </div>
        </div>

        <div className="card">
          <div className="card-header">
            <h3 className="card-title">System Status</h3>
          </div>
          <div className="flex gap-4">
            <div className="flex items-center gap-2">
              <div className="status-dot status-online" />
              <span className="text-sm">Etsy Connected</span>
            </div>
            <div className="flex items-center gap-2">
              <div className="status-dot status-online" />
              <span className="text-sm">Bambu Cloud</span>
            </div>
            <div className="flex items-center gap-2">
              <div className="status-dot status-online" />
              <span className="text-sm">API Healthy</span>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};

const AlertIcon: React.FC<{ type: string; className?: string }> = ({ type, className }) => {
  const icons: Record<string, React.ReactNode> = {
    warning: (
      <svg className={className} viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
        <path d="M10.29 3.86L1.82 18a2 2 0 0 0 1.71 3h16.94a2 2 0 0 0 1.71-3L13.71 3.86a2 2 0 0 0-3.42 0z" />
        <line x1="12" y1="9" x2="12" y2="13" />
        <line x1="12" y1="17" x2="12.01" y2="17" />
      </svg>
    ),
    info: (
      <svg className={className} viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
        <circle cx="12" cy="12" r="10" />
        <line x1="12" y1="16" x2="12" y2="12" />
        <line x1="12" y1="8" x2="12.01" y2="8" />
      </svg>
    ),
    success: (
      <svg className={className} viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
        <path d="M22 11.08V12a10 10 0 1 1-5.93-9.14" />
        <polyline points="22 4 12 14.01 9 11.01" />
      </svg>
    ),
  };
  return <>{icons[type] || icons.info}</>;
};

export default Dashboard;