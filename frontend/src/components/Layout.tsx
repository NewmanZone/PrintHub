import React from 'react';
import { Outlet, NavLink } from 'react-router-dom';

const Logo: React.FC<{ className?: string }> = ({ className }) => (
  <svg
    xmlns="http://www.w3.org/2000/svg"
    viewBox="0 0 24 24"
    fill="none"
    stroke="currentColor"
    strokeWidth="2"
    strokeLinecap="round"
    strokeLinejoin="round"
    className={className}
  >
    <rect x="3" y="3" width="18" height="18" rx="2" />
    <path d="M3 9h18" />
    <path d="M9 21V9" />
    <circle cx="15" cy="15" r="3" />
  </svg>
);

const Layout: React.FC = () => {
  return (
    <div className="page">
      <nav className="nav">
        <div className="nav-inner">
          <NavLink to="/dashboard" className="nav-brand">
            <Logo />
            <span>PrintHub</span>
          </NavLink>

          <div className="nav-links">
            <NavLink to="/dashboard" className={({ isActive }) => `nav-link ${isActive ? 'active' : ''}`}>
              Dashboard
            </NavLink>
            <NavLink to="/products" className={({ isActive }) => `nav-link ${isActive ? 'active' : ''}`}>
              Products
            </NavLink>
            <NavLink to="/printers" className={({ isActive }) => `nav-link ${isActive ? 'active' : ''}`}>
              Printers
            </NavLink>
            <NavLink to="/jobs" className={({ isActive }) => `nav-link ${isActive ? 'active' : ''}`}>
              Jobs
            </NavLink>
            <NavLink to="/orders" className={({ isActive }) => `nav-link ${isActive ? 'active' : ''}`}>
              Orders
            </NavLink>
            <NavLink to="/settings" className={({ isActive }) => `nav-link ${isActive ? 'active' : ''}`}>
              Settings
            </NavLink>
          </div>

          <div className="nav-user">
            <span className="text-sm text-muted">Mike's 3D Prints</span>
            <div className="status-dot status-online" title="All systems operational" />
          </div>
        </div>
      </nav>

      <main className="page-content">
        <div className="container">
          <Outlet />
        </div>
      </main>
    </div>
  );
};

export default Layout;