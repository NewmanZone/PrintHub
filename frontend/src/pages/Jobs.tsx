import React, { useEffect, useState } from 'react';
import { api } from '../services/mockData';
import type { PrintJob, PrintJobStatus } from '../types';

const Jobs: React.FC = () => {
  const [jobs, setJobs] = useState<PrintJob[]>([]);
  const [loading, setLoading] = useState(true);
  const [filter, setFilter] = useState<PrintJobStatus | 'all'>('all');

  useEffect(() => {
    const loadJobs = async () => {
      try {
        const data = await api.getPrintJobs();
        setJobs(data);
      } catch (error) {
        console.error('Failed to load jobs:', error);
      } finally {
        setLoading(false);
      }
    };
    loadJobs();
  }, []);

  const filteredJobs = filter === 'all'
    ? jobs
    : jobs.filter((j) => j.status === filter);

  const getStatusBadge = (status: PrintJobStatus): { label: string; className: string } => {
    const badges: Record<PrintJobStatus, { label: string; className: string }> = {
      Pending: { label: 'Pending', className: 'badge-neutral' },
      Queued: { label: 'Queued', className: 'badge-info' },
      InProgress: { label: 'Printing', className: 'badge-warning' },
      Completed: { label: 'Completed', className: 'badge-success' },
      Failed: { label: 'Failed', className: 'badge-danger' },
      Cancelled: { label: 'Cancelled', className: 'badge-neutral' },
    };
    return badges[status] || badges.Pending;
  };

  const formatDuration = (start: string | null, end: string | null): string => {
    if (!start) return '—';
    const startDate = new Date(start);
    const endDate = end ? new Date(end) : new Date();
    const mins = Math.round((endDate.getTime() - startDate.getTime()) / 60000);
    if (mins < 60) return `${mins}m`;
    const hours = Math.floor(mins / 60);
    const remainingMins = mins % 60;
    return `${hours}h ${remainingMins}m`;
  };

  const formatTime = (dateString: string | null): string => {
    if (!dateString) return '—';
    return new Date(dateString).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
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
        <h1 className="page-title">Print Jobs</h1>
        <p className="page-description">
          Monitor and manage your print queue.
        </p>
      </div>

      <div className="page-actions">
        <div className="flex gap-2">
          {(['all', 'Pending', 'Queued', 'InProgress', 'Completed', 'Failed'] as const).map((status) => (
            <button
              key={status}
              onClick={() => setFilter(status)}
              className={`btn btn-sm ${filter === status ? 'btn-primary' : 'btn-secondary'}`}
            >
              {status === 'all' ? 'All' : status}
            </button>
          ))}
        </div>
        <button className="btn btn-primary">+ New Print Job</button>
      </div>

      {filteredJobs.length === 0 ? (
        <div className="empty-state">
          <div className="empty-state-icon">
            <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.5">
              <rect x="6" y="2" width="12" height="20" rx="2" />
              <path d="M9 7h6" />
              <path d="M9 11h6" />
              <path d="M9 15h4" />
            </svg>
          </div>
          <h3 className="empty-state-title">No print jobs found</h3>
          <p className="empty-state-description">
            {filter !== 'all' ? 'Try selecting a different status filter.' : 'Create your first print job to get started.'}
          </p>
        </div>
      ) : (
        <div className="card">
          <div className="table-wrapper">
            <table>
              <thead>
                <tr>
                  <th>Job ID</th>
                  <th>Status</th>
                  <th>Printer</th>
                  <th>Items</th>
                  <th>Est. Time</th>
                  <th>Started</th>
                  <th>Duration</th>
                  <th>Actions</th>
                </tr>
              </thead>
              <tbody>
                {filteredJobs.map((job) => {
                  const statusBadge = getStatusBadge(job.status);
                  const printer = job.printerTarget
                    ? `Printer ${job.printerTarget.replace('printer_', '#')}`
                    : 'Unassigned';
                  
                  return (
                    <tr key={job.id}>
                      <td>
                        <div>
                          <div style={{ fontWeight: 500, fontFamily: 'monospace', fontSize: '0.875rem' }}>
                            {job.id.slice(0, 12)}...
                          </div>
                          {job.notes && (
                            <div className="text-sm text-muted" style={{ maxWidth: '200px', overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap' }}>
                              {job.notes}
                            </div>
                          )}
                        </div>
                      </td>
                      <td>
                        {job.status === 'InProgress' && job.items[0] && (
                          <div className="flex items-center gap-2">
                            <span className={`badge ${statusBadge.className}`}>{statusBadge.label}</span>
                            <div style={{ width: '80px' }}>
                              <div className="progress-bar" style={{ height: '4px' }}>
                                <div className="progress-bar-fill" style={{ width: '67%' }} />
                              </div>
                            </div>
                          </div>
                        )}
                        {job.status !== 'InProgress' && (
                          <span className={`badge ${statusBadge.className}`}>{statusBadge.label}</span>
                        )}
                      </td>
                      <td>
                        <span className="text-sm">{printer}</span>
                      </td>
                      <td>
                        <div>
                          {job.items.map((item, idx) => (
                            <div key={item.id} className="text-sm">
                              {item.quantity}x {item.partName}
                              {idx < job.items.length - 1 && (
                                <span className="text-muted">, </span>
                              )}
                            </div>
                          ))}
                        </div>
                      </td>
                      <td>
                        {job.estimatedMinutes
                          ? `${Math.round(job.estimatedMinutes)}m`
                          : '—'}
                      </td>
                      <td>
                        <span className="text-sm">{formatTime(job.startedAt)}</span>
                      </td>
                      <td>
                        <span className="text-sm">{formatDuration(job.startedAt, job.completedAt)}</span>
                      </td>
                      <td>
                        <div className="flex gap-2">
                          <button className="btn btn-sm btn-secondary">Details</button>
                          {job.status === 'Queued' && (
                            <button className="btn btn-sm btn-danger">Cancel</button>
                          )}
                          {job.status === 'InProgress' && (
                            <button className="btn btn-sm btn-secondary">Pause</button>
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

      {/* Print Queue Summary */}
      <div className="card mt-4">
        <div className="card-header">
          <h3 className="card-title">Queue Summary</h3>
        </div>
        <div className="stats-grid" style={{ gridTemplateColumns: 'repeat(5, 1fr)' }}>
          <div className="stat-card" style={{ textAlign: 'center' }}>
            <div className="stat-value">{jobs.filter((j) => j.status === 'Pending').length}</div>
            <div className="stat-label">Pending</div>
          </div>
          <div className="stat-card" style={{ textAlign: 'center' }}>
            <div className="stat-value">{jobs.filter((j) => j.status === 'Queued').length}</div>
            <div className="stat-label">Queued</div>
          </div>
          <div className="stat-card" style={{ textAlign: 'center' }}>
            <div className="stat-value" style={{ color: 'var(--color-warning)' }}>
              {jobs.filter((j) => j.status === 'InProgress').length}
            </div>
            <div className="stat-label">Printing</div>
          </div>
          <div className="stat-card" style={{ textAlign: 'center' }}>
            <div className="stat-value" style={{ color: 'var(--color-success)' }}>
              {jobs.filter((j) => j.status === 'Completed').length}
            </div>
            <div className="stat-label">Completed</div>
          </div>
          <div className="stat-card" style={{ textAlign: 'center' }}>
            <div className="stat-value">
              {jobs.reduce((sum, j) => sum + (j.items.reduce((s, i) => s + i.quantity, 0)), 0)}
            </div>
            <div className="stat-label">Total Items</div>
          </div>
        </div>
      </div>
    </div>
  );
};

export default Jobs;