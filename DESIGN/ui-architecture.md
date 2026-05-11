# PrintHub - UI Architecture

## Overview

PrintHub's web UI is a **single-page application (SPA)** built with React + TypeScript. This document locks the high-level page structure, navigation model, and state boundaries.

## Page Inventory

| Route | Page | Purpose |
|-------|------|---------|
| `/` | Landing | Public marketing page, feature overview, CTA to OAuth login |
| `/dashboard` | Dashboard | KPIs, alerts, insights, top performers, inventory overview |
| `/queue` | Print Queue | Queue management, consolidated view, print execution |
| `/products` | Product List | All products with search, filter, quick actions |
| `/products/:id` | Product Detail | Product info, parts, versions, history, settings |
| `/printers` | Printers | List, add, remove, status for all registered printers |
| `/settings` | Settings | General, shop connections, notifications, billing |
| `/jobs/:id` | Job Detail | Live job progress, logs, controls (pause/resume/cancel) |
| `/insights` | Insights (future) | Full analytics, sales velocity, seasonal trends |

## Navigation Model

- **Top navigation** on all authenticated pages: Home → Dashboard → Queue → Products → Printers → Settings
- **Breadcrumbs** on detail pages: Products → Dino Wall Hook
- **Contextual actions** in page headers (e.g., + Add to Queue, + Add Printer)
- **No sidebar** on mobile; hamburger menu replaces topnav links
- **Side nav** appears at `md` breakpoint (768px) for faster navigation on desktop

## State Boundaries

### Server State (React Query)
- Products, parts, files, versions
- Print queue, jobs, job items
- Printers and their live status
- Shop connections and sync state
- Insights/analytics data

**Stale time:** 30s for lists, 5s for active jobs, 60s for insights.

### Client State (Zustand)
- Authentication session (token, user profile)
- UI preferences (theme, density, timezone)
- Form drafts (add-to-queue wizard, product edit)
- Modal/dialog stack
- Toast/notification queue

### URL State
- Active tab/pane on detail pages (`?tab=history`)
- Filter/sort on lists (`?search=dino&status=low`)
- Pagination (`?page=2`)

## Data Flow

```
User action → Zustand (draft/optimistic) → React Query mutation → API →
  → On success: invalidate queries, clear draft, show toast
  → On error: rollback optimistic, show error toast, log to console
```

## Component Hierarchy

```
App
├── Layout
│   ├── TopNav
│   └── SideNav (md+)
├── Pages
│   ├── LandingPage
│   ├── DashboardPage
│   ├── QueuePage
│   ├── ProductsPage
│   ├── ProductDetailPage
│   ├── PrintersPage
│   ├── SettingsPage
│   └── JobDetailPage
├── Shared
│   ├── Card, Metric, Badge, Button, Input, Select
│   ├── DataTable (sortable, paginated)
│   ├── StatusBadge (canonical 8 statuses)
│   ├── Modal, Drawer, Toast
│   ├── SkeletonLoader
│   └── EmptyState
└── Features
    ├── QueueWizard (AddToQueue multi-step)
    ├── ProductEditor
    ├── PrinterRegistration
    └── JobControls
```

## Mock Data Strategy (Prototype)

The static prototype (`prototypes/printhub-ui/index.html`) uses hardcoded HTML to demonstrate every page above. It is **self-contained**: open `index.html` in any browser without a server or backend.

When backend implementation begins, the same page components will swap HTML for React components that call the real API.

## Responsive Strategy

| Breakpoint | Layout Changes |
|------------|----------------|
| < md | Single column, hamburger nav, metric cards 2×2 |
| md–lg | Side nav appears, tables scroll horizontally |
| lg+ | Full two-column detail pages, side-by-side panels |
| xl | Maximum content width 1280px centered |

## Accessibility

- Route changes announce via live region
- Focus trap in modals
- Skip link to main content
- All interactive elements keyboard reachable

## Lock

- **Do not add new top-level pages without updating the prototype.**
- **Do not mix server and client state in the same store.**
- **Do not introduce a state management library other than React Query + Zustand.**
