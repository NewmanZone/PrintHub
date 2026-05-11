import React, { useEffect, useState } from 'react';
import { api } from '../services/mockData';
import type { Shop } from '../types';

const Settings: React.FC = () => {
  const [shops, setShops] = useState<Shop[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const loadShops = async () => {
      try {
        const data = await api.getShops();
        setShops(data);
      } catch (error) {
        console.error('Failed to load shops:', error);
      } finally {
        setLoading(false);
      }
    };
    loadShops();
  }, []);

  const formatDate = (dateString: string | null): string => {
    if (!dateString) return 'Never';
    return new Date(dateString).toLocaleDateString('en-US', {
      month: 'short',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit',
    });
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
        <h1 className="page-title">Settings</h1>
        <p className="page-description">
          Manage your PrintHub configuration, integrations, and preferences.
        </p>
      </div>

      {/* Shop Connections */}
      <div className="card mb-4">
        <div className="card-header">
          <h3 className="card-title">Shop Connections</h3>
          <button className="btn btn-primary btn-sm">+ Connect New Shop</button>
        </div>
        <div className="table-wrapper">
          <table>
            <thead>
              <tr>
                <th>Shop Name</th>
                <th>Provider</th>
                <th>Status</th>
                <th>Last Sync</th>
                <th>Actions</th>
              </tr>
            </thead>
            <tbody>
              {shops.map((shop) => (
                <tr key={shop.id}>
                  <td>
                    <span style={{ fontWeight: 500 }}>{shop.shopName}</span>
                  </td>
                  <td>
                    <span className="badge badge-neutral">
                      {shop.provider === 'etsy' ? 'Etsy' : 'Standalone'}
                    </span>
                  </td>
                  <td>
                    {shop.isActive ? (
                      <span className="badge badge-success">Connected</span>
                    ) : (
                      <span className="badge badge-danger">Disconnected</span>
                    )}
                  </td>
                  <td>
                    <span className="text-sm text-muted">{formatDate(shop.lastSyncAt)}</span>
                  </td>
                  <td>
                    <div className="flex gap-2">
                      <button className="btn btn-sm btn-secondary">Sync Now</button>
                      <button className="btn btn-sm btn-secondary">Configure</button>
                    </div>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </div>

      {/* Print Settings */}
      <div className="grid grid-2">
        <div className="card">
          <div className="card-header">
            <h3 className="card-title">Print Preferences</h3>
          </div>
          <div style={{ display: 'flex', flexDirection: 'column', gap: 'var(--space-4)' }}>
            <div>
              <label style={{ display: 'block', fontWeight: 500, marginBottom: 'var(--space-1)' }}>
                Default Filament Type
              </label>
              <select className="btn btn-secondary" style={{ width: '100%', textAlign: 'left' }}>
                <option>PLA</option>
                <option>PETG</option>
                <option>ABS</option>
                <option>TPU</option>
              </select>
            </div>
            <div>
              <label style={{ display: 'block', fontWeight: 500, marginBottom: 'var(--space-1)' }}>
                Default Print Quality
              </label>
              <select className="btn btn-secondary" style={{ width: '100%', textAlign: 'left' }}>
                <option>Standard (0.2mm)</option>
                <option>High Quality (0.12mm)</option>
                <option>Draft (0.28mm)</option>
              </select>
            </div>
            <div>
              <label style={{ display: 'flex', alignItems: 'center', gap: 'var(--space-2)', cursor: 'pointer' }}>
                <input type="checkbox" defaultChecked />
                Auto-start prints when printer is ready
              </label>
            </div>
            <div>
              <label style={{ display: 'flex', alignItems: 'center', gap: 'var(--space-2)', cursor: 'pointer' }}>
                <input type="checkbox" defaultChecked />
                Send notifications on job completion
              </label>
            </div>
          </div>
          <button className="btn btn-primary mt-4">Save Preferences</button>
        </div>

        <div className="card">
          <div className="card-header">
            <h3 className="card-title">Inventory Alerts</h3>
          </div>
          <div style={{ display: 'flex', flexDirection: 'column', gap: 'var(--space-4)' }}>
            <div>
              <label style={{ display: 'block', fontWeight: 500, marginBottom: 'var(--space-1)' }}>
                Low Stock Threshold
              </label>
              <input
                type="number"
                className="btn btn-secondary"
                defaultValue="5"
                style={{ width: '100%' }}
              />
              <p className="text-sm text-muted mt-1">
                Alert when any product falls below this inventory level
              </p>
            </div>
            <div>
              <label style={{ display: 'flex', alignItems: 'center', gap: 'var(--space-2)', cursor: 'pointer' }}>
                <input type="checkbox" defaultChecked />
                Email alerts for low stock
              </label>
            </div>
            <div>
              <label style={{ display: 'flex', alignItems: 'center', gap: 'var(--space-2)', cursor: 'pointer' }}>
                <input type="checkbox" defaultChecked />
                Reorder recommendations based on sales velocity
              </label>
            </div>
          </div>
          <button className="btn btn-primary mt-4">Save Alerts</button>
        </div>
      </div>

      {/* API & Integrations */}
      <div className="card mt-4">
        <div className="card-header">
          <h3 className="card-title">API & Integrations</h3>
        </div>
        <div className="grid grid-3">
          <div style={{ padding: 'var(--space-4)', background: 'var(--bg-secondary)', borderRadius: 'var(--radius-lg)' }}>
            <div className="flex items-center gap-2 mb-2">
              <span style={{ fontSize: '1.5rem' }}>🖨️</span>
              <span style={{ fontWeight: 600 }}>Bambu Connect</span>
              <span className="badge badge-success">Connected</span>
            </div>
            <p className="text-sm text-muted mb-3">
              Cloud-native integration for Bambu Lab printers.
            </p>
            <button className="btn btn-sm btn-secondary">Configure</button>
          </div>

          <div style={{ padding: 'var(--space-4)', background: 'var(--bg-secondary)', borderRadius: 'var(--radius-lg)' }}>
            <div className="flex items-center gap-2 mb-2">
              <span style={{ fontSize: '1.5rem' }}>🛒</span>
              <span style={{ fontWeight: 600 }}>Etsy API</span>
              <span className="badge badge-success">Connected</span>
            </div>
            <p className="text-sm text-muted mb-3">
              Sync listings, orders, and inventory with your Etsy shop.
            </p>
            <button className="btn btn-sm btn-secondary">Manage</button>
          </div>

          <div style={{ padding: 'var(--space-4)', background: 'var(--bg-secondary)', borderRadius: 'var(--radius-lg)' }}>
            <div className="flex items-center gap-2 mb-2">
              <span style={{ fontSize: '1.5rem' }}>🔧</span>
              <span style={{ fontWeight: 600 }}>OctoAnywhere</span>
              <span className="badge badge-neutral">Not Configured</span>
            </div>
            <p className="text-sm text-muted mb-3">
              Bridge for Klipper/OctoPrint based printers.
            </p>
            <button className="btn btn-sm btn-secondary">Setup</button>
          </div>
        </div>
      </div>

      {/* Account Settings */}
      <div className="card mt-4">
        <div className="card-header">
          <h3 className="card-title">Account</h3>
        </div>
        <div style={{ display: 'flex', flexDirection: 'column', gap: 'var(--space-4)' }}>
          <div>
            <label style={{ display: 'block', fontWeight: 500, marginBottom: 'var(--space-1)' }}>
              Display Name
            </label>
            <input
              type="text"
              className="btn btn-secondary"
              defaultValue="Mike's 3D Prints"
              style={{ width: '100%', maxWidth: '300px' }}
            />
          </div>
          <div>
            <label style={{ display: 'block', fontWeight: 500, marginBottom: 'var(--space-1)' }}>
              Email Address
            </label>
            <input
              type="email"
              className="btn btn-secondary"
              defaultValue="mike@prints.example.com"
              style={{ width: '100%', maxWidth: '300px' }}
            />
          </div>
          <div className="flex gap-2">
            <button className="btn btn-primary">Save Account</button>
            <button className="btn btn-secondary">Change Password</button>
          </div>
        </div>
      </div>

      {/* Danger Zone */}
      <div className="card mt-4" style={{ borderColor: 'var(--color-danger)' }}>
        <div className="card-header">
          <h3 className="card-title" style={{ color: 'var(--color-danger)' }}>Danger Zone</h3>
        </div>
        <p className="text-sm text-muted mb-4">
          These actions are irreversible. Please proceed with caution.
        </p>
        <div className="flex gap-4">
          <button className="btn btn-secondary">Export All Data</button>
          <button className="btn btn-danger">Delete Account</button>
        </div>
      </div>
    </div>
  );
};

export default Settings;