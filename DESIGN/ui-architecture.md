# PrintHub - UI Architecture

## Overview

PrintHub's web UI is a single-page application built with React + TypeScript. Phase 1 is a shared Etsy production workspace, so the UI should optimize for a small team preparing the right print files from real Etsy orders.

The authenticated app should feel like a calm operations console: fast to scan, clear about blocking work, and beautiful without becoming decorative.

## Page Inventory

| Route | Page | Purpose |
|-------|------|---------|
| `/` | Landing | Public page with product value, visual product signal, and OAuth CTA |
| `/dashboard` | Dashboard | Workspace overview: open orders, ready bundles, missing files, Etsy sync health |
| `/orders` | Orders | Etsy order inbox with preparation status, due dates, and blockers |
| `/orders/:id` | Order Detail | Line items, personalization, product mapping, bundle generation, download |
| `/products` | Product List | Etsy/manual products with file coverage and personalization indicators |
| `/products/:id` | Product Detail | Product info, mapped parts, file versions, Etsy listing metadata, prep rules |
| `/parts` | Parts | Reusable printable parts and current source file status |
| `/bundles` | Preparation Bundles | Recent generated/downloaded bundles and manual batch bundles |
| `/bundles/:id` | Bundle Detail | Manifest, files, quantities, manual customization notes, download |
| `/settings` | Settings | Workspace profile, Etsy connection, members, file retention, notifications |
| `/jobs` | Jobs | Later phase print job history; Phase 1 may hide or show as "Coming later" |
| `/printers` | Printers | Later phase printer adapters; Phase 1 may hide or show as "Coming later" |

## Landing Page

The landing page is for people not yet signed up. It should make the Phase 1 promise obvious:

- Sign in with OAuth.
- Connect Etsy.
- Invite a contributor.
- Map products to 3MF/STL files.
- Download the right files for each order.

The first viewport should show PrintHub as the literal product name and include a high-quality visual of an Etsy order being turned into a print-ready file bundle. The visual can be implemented as a coded UI composition now, then upgraded with generated or photographed product/file imagery later.

## Navigation Model

- Authenticated desktop uses a side nav for: Dashboard, Orders, Products, Parts, Bundles, Settings.
- Mobile uses bottom navigation for the highest-frequency pages: Dashboard, Orders, Products, Bundles, Settings.
- Printer and job navigation should not be primary in Phase 1.
- Detail pages use breadcrumbs, for example: Products -> Custom Name Sign.
- Page headers expose one primary action only when possible: Sync Etsy, Upload File, Generate Bundle, Download Bundle.

## State Boundaries

### Server State (React Query)

- Workspaces and memberships
- Etsy shop connection and sync state
- Products, parts, files, versions
- Orders, order items, preparation statuses
- Preparation bundles and download state
- Dashboard insights

Stale time: 30s for lists, 10s for active sync/order prep state, 60s for dashboard summaries.

### Client State (Zustand)

- Authentication session mirror and selected workspace id
- UI preferences such as density and timezone
- Form drafts for product mapping, part editing, and bundle generation
- Modal/dialog stack
- Toast/notification queue

### URL State

- Active tab on detail pages (`?tab=files`)
- Filter/sort on lists (`?search=sign&status=NeedsFiles`)
- Pagination (`?page=2`)

## Data Flow

```text
User action -> local draft or optimistic state -> React Query mutation -> API
  -> Success: invalidate affected queries, clear draft, show toast
  -> Error: rollback optimistic state, preserve draft, show actionable error
```

## Component Hierarchy

```text
App
|-- PublicLayout
|   `-- LandingPage
|-- AppShell
|   |-- SideNav
|   |-- MobileBottomNav
|   `-- WorkspaceSwitcher
|-- Pages
|   |-- DashboardPage
|   |-- OrdersPage
|   |-- OrderDetailPage
|   |-- ProductsPage
|   |-- ProductDetailPage
|   |-- PartsPage
|   |-- BundlesPage
|   |-- BundleDetailPage
|   `-- SettingsPage
|-- Shared UI
|   |-- Button, IconButton, Input, Select, Checkbox, Tabs
|   |-- Panel, MetricCard, DataTable, StatusChip
|   |-- Modal, Drawer, Toast, SkeletonLoader, EmptyState
|   `-- FileDropzone, FileVersionList, ManifestList
`-- Features
    |-- EtsyConnectPanel
    |-- MemberInvitePanel
    |-- ProductMappingEditor
    |-- PreparationBundleWizard
    `-- PersonalizationReview
```

## Phase 1 Primary Workflows

### Workspace Onboarding

1. User signs in with OAuth.
2. User creates or selects a workspace.
3. User connects Etsy.
4. User invites dad as a contributor.
5. Dashboard shows sync state and next setup tasks.

### Product File Setup

1. Product imports from Etsy or is created manually.
2. User maps product to one or more parts.
3. User uploads current 3MF/STL file for each part.
4. Product list shows file coverage as complete, partial, or missing.

### Order Preparation

1. Etsy order appears in Orders.
2. User opens order detail and reviews line items.
3. User resolves missing product mapping or missing files.
4. User generates a preparation bundle.
5. User downloads bundle ZIP with files and manifest.
6. User marks bundle/order printed after manual printing.

## Visual Asset Guidance

Phase 1 implementation can start with coded UI mock visuals, but issues should ask for real assets or generated mockups where beauty matters:

- Product thumbnails: neutral build-plate background, actual product photo/render when available.
- File thumbnails: small 3D model preview or generated placeholder per file type.
- Landing hero visual: order card -> manifest -> 3MF/STL bundle composition, preferably generated as a polished bitmap once the layout is stable.
- Icons: use the configured icon library for actions; avoid hand-drawn SVG except for product/file mock visuals.
- Empty states: compact, practical visuals showing missing file, no Etsy connection, or no orders.

## Responsive Strategy

| Breakpoint | Layout Changes |
|------------|----------------|
| `< md` | Single column, bottom nav, compact tables become list rows |
| `md-lg` | Side nav appears, list/detail pages remain single column where needed |
| `lg+` | Detail pages use two columns: main work area plus status/metadata rail |
| `xl` | Max content width 1440px; dense dashboard and tables |

## Accessibility

- Route changes announce via live region.
- Focus trap in modals and drawers.
- Skip link to main content.
- All interactive elements keyboard reachable.
- File upload controls must be usable without drag-and-drop.
- Color cannot be the only indicator of preparation status.

## Lock

- Do not add password-based auth UI. OAuth only.
- Do not make printer pages required for Phase 1 completion.
- Do not add new top-level pages without updating this document and the frontend prototype/app shell.
- Do not mix server and client state in the same store.
- Do not introduce a state management library other than React Query + Zustand.
