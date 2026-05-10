# PrintHub Frontend

**React SPA for PrintHub - 3D Print Operations Platform**

## Tech Stack

- React 18 with TypeScript
- Vite for build tooling
- React Router for navigation
- CSS Modules for styling

## Getting Started

```bash
cd frontend
npm install
npm run dev
```

## Project Structure

```
src/
├── components/     # Reusable UI components
├── pages/          # Page-level components
│   ├── Dashboard.tsx
│   ├── Products.tsx
│   ├── Printers.tsx
│   ├── Jobs.tsx
│   ├── Orders.tsx
│   └── Settings.tsx
├── services/       # API service layer (mock data for now)
├── types/          # TypeScript interfaces
├── hooks/          # Custom React hooks
└── styles/         # Global styles and CSS variables
```