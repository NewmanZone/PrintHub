import React, { useEffect, useState } from 'react';
import { api } from '../services/mockData';
import type { Printer, PrinterStatus } from '../types';

const Printers: React.FC = () => {
  const [printers, setPrinters] = useState<Printer[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const loadPrinters = async () => {
      try {
        const data = await api.getPrinters();
        setPrinters(data);
      } catch (error) {
        console.error('Failed to load printers:', error);
      } finally {
        setLoading(false);
      }
    };
    loadPrinters();
  }, []);

  const getStatusClass = (status: PrinterStatus): string => {
    const classes: Record<PrinterStatus, string> = {
      online: 'status-online',
      offline: 'status-offline',
      printing: 'status-printing',
      error: 'status-error',
    };
    return classes[status] || 'status-offline';
  };

  const getStatusBadge = (status: PrinterStatus): { label: string; className: string } => {
    const badges: Record<PrinterStatus, { label: string; className: string }> = {
      online: { label: 'Online', className: 'badge-success' },
      offline: { label: 'Offline', className: 'badge-neutral' },
      printing: { label: 'Printing', className: 'badge-warning' },
      error: { label: 'Error', className: 'badge-danger' },
    };
    return badges[status] || badges.offline;
  };

  const formatPrinterType = (type: string): string => {
    return type.charAt(0).toUpperCase() + type.slice(1);
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
        <h1 className="page-title">Printers</h1>
        <p className="page-description">
          Monitor and manage your connected 3D printers.
        </p>
      </div>

      <div className="page-actions">
        <button className="btn btn-secondary">🔄 Sync Printers</button>
        <button className="btn btn-primary">+ Add Printer</button>
      </div>

      <div className="grid grid-3">
        {printers.map((printer) => {
          const statusBadge = getStatusBadge(printer.status);
          return (
            <div key={printer.id} className="card">
              {/* Printer Header */}
              <div className="flex justify-between items-center mb-4">
                <div>
                  <h3 style={{ marginBottom: '0.25rem' }}>{printer.name}</h3>
                  <span className="text-sm text-muted">{printer.model}</span>
                </div>
                <div className="flex items-center gap-2">
                  <div className={`status-dot ${getStatusClass(printer.status)}`} />
                  <span className={`badge ${statusBadge.className}`}>{statusBadge.label}</span>
                </div>
              </div>

              {/* Printer Type */}
              <div className="flex items-center gap-2 mb-4">
                <span className="badge badge-neutral">{formatPrinterType(printer.type)}</span>
                {printer.serialNumber && (
                  <span className="text-sm text-muted">SN: {printer.serialNumber}</span>
                )}
              </div>

              {/* Temperature Readings */}
              {printer.status !== 'offline' && (
                <div className="grid grid-2 gap-2 mb-4">
                  <div style={{ background: 'var(--bg-secondary)', padding: 'var(--space-3)', borderRadius: 'var(--radius-md)' }}>
                    <div className="text-sm text-muted">Bed Temp</div>
                    <div style={{ fontSize: '1.25rem', fontWeight: 600 }}>
                      {printer.bedTemp !== null ? `${printer.bedTemp}°C` : '—'}
                    </div>
                  </div>
                  <div style={{ background: 'var(--bg-secondary)', padding: 'var(--space-3)', borderRadius: 'var(--radius-md)' }}>
                    <div className="text-sm text-muted">Nozzle Temp</div>
                    <div style={{ fontSize: '1.25rem', fontWeight: 600 }}>
                      {printer.nozzleTemp !== null ? `${printer.nozzleTemp}°C` : '—'}
                    </div>
                  </div>
                </div>
              )}

              {/* Print Progress */}
              {printer.status === 'printing' && printer.progress !== null && (
                <div className="mb-4">
                  <div className="flex justify-between items-center mb-2">
                    <span className="text-sm">Print Progress</span>
                    <span className="text-sm" style={{ fontWeight: 600 }}>{printer.progress}%</span>
                  </div>
                  <div className="progress-bar">
                    <div
                      className="progress-bar-fill"
                      style={{ width: `${printer.progress}%` }}
                    />
                  </div>
                </div>
              )}

              {/* Chamber Temp */}
              {printer.chamberTemp !== null && (
                <div className="text-sm text-muted mb-4">
                  Chamber: {printer.chamberTemp}°C
                </div>
              )}

              {/* Actions */}
              <div className="flex gap-2" style={{ marginTop: 'auto' }}>
                {printer.status === 'online' && (
                  <button className="btn btn-primary btn-sm">Send Job</button>
                )}
                <button className="btn btn-secondary btn-sm">Configure</button>
                <button className="btn btn-secondary btn-sm">Details</button>
              </div>
            </div>
          );
        })}
      </div>

      {/* Printer Stats Summary */}
      <div className="card mt-4">
        <div className="card-header">
          <h3 className="card-title">Printer Overview</h3>
        </div>
        <div className="stats-grid" style={{ gridTemplateColumns: 'repeat(4, 1fr)' }}>
          <div className="stat-card" style={{ textAlign: 'center' }}>
            <div className="stat-value" style={{ color: 'var(--color-success)' }}>
              {printers.filter((p) => p.status === 'online').length}
            </div>
            <div className="stat-label">Online</div>
          </div>
          <div className="stat-card" style={{ textAlign: 'center' }}>
            <div className="stat-value" style={{ color: 'var(--color-warning)' }}>
              {printers.filter((p) => p.status === 'printing').length}
            </div>
            <div className="stat-label">Printing</div>
          </div>
          <div className="stat-card" style={{ textAlign: 'center' }}>
            <div className="stat-value" style={{ color: 'var(--text-muted)' }}>
              {printers.filter((p) => p.status === 'offline').length}
            </div>
            <div className="stat-label">Offline</div>
          </div>
          <div className="stat-card" style={{ textAlign: 'center' }}>
            <div className="stat-value" style={{ color: 'var(--color-danger)' }}>
              {printers.filter((p) => p.status === 'error').length}
            </div>
            <div className="stat-label">Error</div>
          </div>
        </div>
      </div>
    </div>
  );
};

export default Printers;