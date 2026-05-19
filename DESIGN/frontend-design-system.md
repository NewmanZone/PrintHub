# PrintHub - Frontend Design System

## Overview

PrintHub's web frontend is built with **React + TypeScript**. This document locks the visual direction, component tokens, and theming rules.

## Tech Stack Lock

| Layer | Technology |
|-------|------------|
| Framework | React 18+ |
| Language | TypeScript (strict mode) |
| Styling | CSS Modules + CSS custom properties |
| State | React Query (server state) + Zustand (client state) |
| Routing | React Router v6 |
| Build | Vite |

## Color Palette

### Base

| Token | Hex | Usage |
|-------|-----|-------|
| `--color-graphite-900` | `#0F1419` | Page background, deepest surface |
| `--color-graphite-800` | `#1A2332` | Cards, panels, elevated surfaces |
| `--color-graphite-700` | `#2A3A4F` | Borders, dividers, inactive states |
| `--color-graphite-600` | `#3E5675` | Secondary text, placeholders |
| `--color-graphite-500` | `#5A7A9C` | Tertiary text |
| `--color-graphite-400` | `#8BA3BD` | Body text on dark |
| `--color-graphite-300` | `#B8C9D9` | Headings, primary text |
| `--color-graphite-100` | `#E8EDF2` | Headings on light surfaces |

### Accent: Teal (Controls, Actions)

| Token | Hex | Usage |
|-------|-----|-------|
| `--color-teal-600` | `#0D7377` | Primary buttons, active states |
| `--color-teal-500` | `#14A085` | Primary hover, links |
| `--color-teal-400` | `#2DD4A8` | Success indicators, progress bars |
| `--color-teal-300` | `#6EE7B7` | Highlights, focus rings |
| `--color-teal-200` | `#A7F3D0` | Light backgrounds, badges |
| `--color-teal-100` | `#D1FAE5` | Subtle fills |

### Accent: Filament Coral (Alerts, Warmth)

| Token | Hex | Usage |
|-------|-----|-------|
| `--color-coral-600` | `#C2410C` | Critical alerts, destructive actions |
| `--color-coral-500` | `#EA580C` | Warnings, attention |
| `--color-coral-400` | `#F97316` | Warm accents, filament metaphor |
| `--color-coral-300` | `#FB923C` | Hover on warnings |
| `--color-coral-200` | `#FDBA74` | Subtle warm backgrounds |
| `--color-coral-100` | `#FFEDD5` | Notification pills |

### Semantic

| Token | Value | Usage |
|-------|-------|-------|
| `--color-bg` | `var(--color-graphite-900)` | App background |
| `--color-surface` | `var(--color-graphite-800)` | Cards, panels |
| `--color-surface-elevated` | `var(--color-graphite-700)` | Modals, dropdowns |
| `--color-text-primary` | `var(--color-graphite-300)` | Primary text |
| `--color-text-secondary` | `var(--color-graphite-400)` | Secondary text |
| `--color-text-muted` | `var(--color-graphite-500)` | Tertiary/caption text |
| `--color-border` | `var(--color-graphite-700)` | Default borders |
| `--color-border-focus` | `var(--color-teal-300)` | Focus rings |
| `--color-primary` | `var(--color-teal-500)` | CTA buttons, links |
| `--color-primary-hover` | `var(--color-teal-400)` | CTA hover |
| `--color-danger` | `var(--color-coral-500)` | Destructive actions |
| `--color-success` | `var(--color-teal-400)` | Success states |
| `--color-warning` | `var(--color-coral-400)` | Warning states |

## Typography

| Token | Value | Usage |
|-------|-------|-------|
| `--font-sans` | `Inter, system-ui, sans-serif` | Body, UI |
| `--font-mono` | `JetBrains Mono, ui-monospace, monospace` | Code, serial numbers, metrics |
| `--text-xs` | `0.75rem / 1rem` | Badges, timestamps |
| `--text-sm` | `0.875rem / 1.25rem` | Secondary text, labels |
| `--text-base` | `1rem / 1.5rem` | Body |
| `--text-lg` | `1.125rem / 1.75rem` | Lead paragraphs |
| `--text-xl` | `1.25rem / 1.75rem` | Section headings |
| `--text-2xl` | `1.5rem / 2rem` | Page headings |
| `--text-3xl` | `1.875rem / 2.25rem` | Hero headings |
| `--font-weight-normal` | `400` | Body |
| `--font-weight-medium` | `500` | Labels, buttons |
| `--font-weight-semibold` | `600` | Headings |
| `--font-weight-bold` | `700` | Emphasis |

## Spacing Scale

| Token | Value |
|-------|-------|
| `--space-1` | `0.25rem` (4px) |
| `--space-2` | `0.5rem` (8px) |
| `--space-3` | `0.75rem` (12px) |
| `--space-4` | `1rem` (16px) |
| `--space-5` | `1.25rem` (20px) |
| `--space-6` | `1.5rem` (24px) |
| `--space-8` | `2rem` (32px) |
| `--space-10` | `2.5rem` (40px) |
| `--space-12` | `3rem` (48px) |
| `--space-16` | `4rem` (64px) |
| `--space-20` | `5rem` (80px) |

## Border Radius

| Token | Value | Usage |
|-------|-------|-------|
| `--radius-sm` | `0.25rem` (4px) | Inputs, tags |
| `--radius-md` | `0.5rem` (8px) | Cards, buttons |
| `--radius-lg` | `0.75rem` (12px) | Modals, panels |
| `--radius-xl` | `1rem` (16px) | Hero cards, feature callouts |
| `--radius-full` | `9999px` | Pills, avatars |

## Shadows

| Token | Value | Usage |
|-------|-------|-------|
| `--shadow-sm` | `0 1px 2px rgba(0,0,0,0.3)` | Subtle elevation |
| `--shadow-md` | `0 4px 6px -1px rgba(0,0,0,0.4)` | Cards, dropdowns |
| `--shadow-lg` | `0 10px 15px -3px rgba(0,0,0,0.5)` | Modals, popovers |
| `--shadow-glow` | `0 0 12px rgba(45,212,168,0.25)` | Focus, active CTAs |

## Iconography

- **Library:** Phosphor Icons (React)
- **Size scale:** 16px (sm), 20px (md), 24px (lg), 32px (xl)
- **Weight:** Regular for UI, Bold for navigation
- **Color:** inherits `--color-text-secondary` by default, `--color-primary` for interactive

## Component Patterns

### Button

```
Primary:    bg-teal-600 → hover:bg-teal-500, text-white, radius-md, px-4 py-2
Secondary:  bg-graphite-800 → hover:bg-graphite-700, text-graphite-300, border border-graphite-700
Danger:     bg-coral-600 → hover:bg-coral-500, text-white
Ghost:      transparent → hover:bg-graphite-800, text-graphite-300
```

### Card

```
bg-surface, radius-lg, border border-graphite-700, shadow-md
Header: pb-4 border-b border-graphite-700, text-xl semibold text-primary
Body: p-4
Footer: pt-4 border-t border-graphite-700, flex justify-end gap-2
```

### Input

```
bg-graphite-900, border border-graphite-700, radius-md, px-3 py-2
text-graphite-300, placeholder:text-graphite-600
focus:border-teal-300, focus:ring-1 focus:ring-teal-300
```

### Data Table

```
Header: bg-graphite-800, text-xs uppercase tracking-wider text-graphite-500, py-3 px-4
Row: border-b border-graphite-700, hover:bg-graphite-800/50
Cell: py-3 px-4, text-sm text-graphite-400
Status badge: radius-full, px-2 py-0.5, text-xs font-medium
```

### Status Badges

| Status | Background | Text | Icon |
|--------|-----------|------|------|
| Draft | `bg-graphite-700` | `text-graphite-300` | Circle dashed |
| Pending | `bg-teal-900/40` | `text-teal-300` | Clock |
| Queued | `bg-teal-800/40` | `text-teal-200` | List |
| InProgress | `bg-teal-600/30` | `text-teal-300` | Spinner |
| Paused | `bg-coral-900/30` | `text-coral-300` | Pause |
| Completed | `bg-teal-600/20` | `text-teal-400` | CheckCircle |
| Failed | `bg-coral-900/40` | `text-coral-400` | XCircle |
| Cancelled | `bg-graphite-700/50` | `text-graphite-500` | Slash |

## Build-Plate Grid Cue

The UI uses a subtle grid pattern on primary surfaces to evoke a 3D printer build plate:

```css
.surface-grid {
  background-image:
    linear-gradient(rgba(255,255,255,0.03) 1px, transparent 1px),
    linear-gradient(90deg, rgba(255,255,255,0.03) 1px, transparent 1px);
  background-size: 2rem 2rem;
}
```

Applied sparingly to:
- Dashboard background
- Queue page background
- Empty states

## Thumbnails & Printer Silhouettes

- **Part thumbnails:** 64×64px rounded squares, generated on upload, stored separately from source files
- **Product hero:** 240×180px, object-fit contain on neutral background
- **Printer silhouettes:** 48×48px monochrome icons per printer family (Bambu, Voron, Prusa, Generic)
- **Placeholder:** Skeleton loader with build-plate grid shimmer

## Accessibility

- Minimum contrast ratio 4.5:1 for body text
- Minimum contrast ratio 3:1 for UI components
- Focus indicators: 2px solid `--color-border-focus` with 2px offset
- Reduced motion: respect `prefers-reduced-motion`
- Touch targets: minimum 44×44px

## Responsive Breakpoints

| Name | Width | Notes |
|------|-------|-------|
| `sm` | 640px | Minimal changes |
| `md` | 768px | Side nav appears |
| `lg` | 1024px | Two-column layouts |
| `xl` | 1280px | Full dashboard density |
| `2xl` | 1536px | Max content width 1440px centered |

## Lock

- **Do not introduce a second frontend framework.**
- **Do not change the core color tokens without updating the prototype.**
- **Do not add password-based auth UI (login/register/reset forms).** OAuth-only.
