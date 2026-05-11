import React, { useEffect, useState } from 'react';
import { api } from '../services/mockData';
import type { PersonalizedOrder, PersonalizedOrderStatus } from '../types';

const Orders: React.FC = () => {
  const [orders, setOrders] = useState<PersonalizedOrder[]>([]);
  const [loading, setLoading] = useState(true);
  const [filter, setFilter] = useState<PersonalizedOrderStatus | 'all'>('all');

  useEffect(() => {
    const loadOrders = async () => {
      try {
        const data = await api.getOrders();
        setOrders(data);
      } catch (error) {
        console.error('Failed to load orders:', error);
      } finally {
        setLoading(false);
      }
    };
    loadOrders();
  }, []);

  const filteredOrders = filter === 'all'
    ? orders
    : orders.filter((o) => o.status === filter);

  const getStatusBadge = (status: PersonalizedOrderStatus): { label: string; className: string } => {
    const badges: Record<PersonalizedOrderStatus, { label: string; className: string }> = {
      Received: { label: 'Received', className: 'badge-info' },
      InPreparation: { label: 'In Prep', className: 'badge-warning' },
      QueuedForPrint: { label: 'Queued', className: 'badge-neutral' },
      Printed: { label: 'Printed', className: 'badge-success' },
      Shipped: { label: 'Shipped', className: 'badge-success' },
    };
    return badges[status] || badges.Received;
  };

  const formatDate = (dateString: string | null): string => {
    if (!dateString) return '—';
    const date = new Date(dateString);
    return date.toLocaleDateString('en-US', {
      month: 'short',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit',
    });
  };

  const formatPersonalization = (data: Record<string, unknown>): string => {
    return Object.entries(data)
      .map(([key, value]) => `${key}: ${String(value)}`)
      .join(', ');
  };

  const isOverdue = (dueBy: string | null): boolean => {
    if (!dueBy) return false;
    return new Date(dueBy) < new Date();
  };

  if (loading) {
    return (
      <div className="loading">
        <div className="spinner" />
      </div>
    );
  }

  return (
    <div>
      <div className="page-header">
        <h1 className="page-title">Orders</h1>
        <p className="page-description">
          Manage personalized orders from Etsy and standalone sales.
        </p>
      </div>

      <div className="page-actions">
        <div className="flex gap-2">
          {(['all', 'Received', 'InPreparation', 'QueuedForPrint', 'Printed', 'Shipped'] as const).map((status) => (
            <button
              key={status}
              onClick={() => setFilter(status)}
              className={`btn btn-sm ${filter === status ? 'btn-primary' : 'btn-secondary'}`}
            >
              {status === 'all' ? 'All' : status.replace(/([A-Z])/g, ' $1').trim()}
            </button>
          ))}
        </div>
        <button className="btn btn-secondary">🔄 Sync Etsy Orders</button>
      </div>

      {filteredOrders.length === 0 ? (
        <div className="empty-state">
          <div className="empty-state-icon">
            <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.5">
              <path d="M9 5H7a2 2 0 00-2 2v12a2 2 0 002 2h10a2 2 0 002-2V7a2 2 0 00-2-2h-2" />
              <rect x="9" y="3" width="6" height="4" rx="2" />
              <path d="M9 12h6" />
              <path d="M9 16h6" />
            </svg>
          </div>
          <h3 className="empty-state-title">No orders found</h3>
          <p className="empty-state-description">
            {filter !== 'all' ? 'Try selecting a different status filter.' : 'Orders will appear here when sync with Etsy.'}
          </p>
        </div>
      ) : (
        <div className="card">
          <div className="table-wrapper">
            <table>
              <thead>
                <tr>
                  <th>Order ID</th>
                  <th>Customer</th>
                  <th>Personalization</th>
                  <th>Status</th>
                  <th>Due By</th>
                  <th>Print Job</th>
                  <th>Created</th>
                  <th>Actions</th>
                </tr>
              </thead>
              <tbody>
                {filteredOrders.map((order) => {
                  const statusBadge = getStatusBadge(order.status);
                  const overdue = isOverdue(order.dueBy) && order.status !== 'Shipped';
                  
                  return (
                    <tr key={order.id}>
                      <td>
                        <div>
                          <div style={{ fontFamily: 'monospace', fontSize: '0.875rem' }}>
                            {order.etsyOrderId || order.id.slice(0, 8)}
                          </div>
                          {order.etsyOrderId && (
                            <div className="text-sm text-muted">Etsy</div>
                          )}
                        </div>
                      </td>
                      <td>
                        <span style={{ fontWeight: 500 }}>{order.customerName || '—'}</span>
                      </td>
                      <td>
                        <span className="text-sm text-muted">
                          {formatPersonalization(order.personalizationData)}
                        </span>
                      </td>
                      <td>
                        <span className={`badge ${statusBadge.className}`}>
                          {statusBadge.label}
                        </span>
                      </td>
                      <td>
                        {order.dueBy ? (
                          <span style={{ color: overdue ? 'var(--color-danger)' : undefined, fontWeight: overdue ? 600 : 400 }}>
                            {new Date(order.dueBy).toLocaleDateString()}
                            {overdue && ' ⚠️'}
                          </span>
                        ) : (
                          <span className="text-muted">—</span>
                        )}
                      </td>
                      <td>
                        {order.printJobId ? (
                          <a href={`/jobs/${order.printJobId}`} className="text-sm">
                            {order.printJobId.slice(0, 12)}...
                          </a>
                        ) : (
                          <button className="btn btn-sm btn-secondary">Create Job</button>
                        )}
                      </td>
                      <td>
                        <span className="text-sm text-muted">{formatDate(order.createdAt)}</span>
                      </td>
                      <td>
                        <div className="flex gap-2">
                          <button className="btn btn-sm btn-secondary">View</button>
                          {order.status === 'Received' && (
                            <button className="btn btn-sm btn-primary">Prepare</button>
                          )}
                          {order.status === 'Printed' && (
                            <button className="btn btn-sm btn-primary">Mark Shipped</button>
                          )}
                        </div>
                      </td>
                    </tr>
                  );
                })}
              </tbody>
            </table>
          </div>
        </div>
      )}

      {/* Order Stats Summary */}
      <div className="card mt-4">
        <div className="card-header">
          <h3 className="card-title">Order Summary</h3>
        </div>
        <div className="stats-grid" style={{ gridTemplateColumns: 'repeat(5, 1fr)' }}>
          <div className="stat-card" style={{ textAlign: 'center' }}>
            <div className="stat-value" style={{ color: 'var(--color-info)' }}>
              {orders.filter((o) => o.status === 'Received').length}
            </div>
            <div className="stat-label">New</div>
          </div>
          <div className="stat-card" style={{ textAlign: 'center' }}>
            <div className="stat-value" style={{ color: 'var(--color-warning)' }}>
              {orders.filter((o) => o.status === 'InPreparation').length}
            </div>
            <div className="stat-label">In Prep</div>
          </div>
          <div className="stat-card" style={{ textAlign: 'center' }}>
            <div className="stat-value">
              {orders.filter((o) => o.status === 'QueuedForPrint').length}
            </div>
            <div className="stat-label">Queued</div>
          </div>
          <div className="stat-card" style={{ textAlign: 'center' }}>
            <div className="stat-value" style={{ color: 'var(--color-success)' }}>
              {orders.filter((o) => o.status === 'Printed' || o.status === 'Shipped').length}
            </div>
            <div className="stat-label">Fulfilled</div>
          </div>
          <div className="stat-card" style={{ textAlign: 'center' }}>
            <div className="stat-value" style={{ color: 'var(--color-danger)' }}>
              {orders.filter((o) => isOverdue(o.dueBy) && o.status !== 'Shipped').length}
            </div>
            <div className="stat-label">Overdue</div>
          </div>
        </div>
      </div>
    </div>
  );
};

export default Orders;