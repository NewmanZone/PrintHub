# PrintHub - Frontend Design System

## Overview

PrintHub's frontend is React + TypeScript. The product should feel like a polished light operations console for a small Etsy production team: calm, precise, fast to scan, and visually memorable.

## Theme Decision

PrintHub uses the light operations theme. Earlier dark graphite explorations are archived in prototype history. Implementation should stay light unless this document changes.

Visual tone:

- Cool mist page canvas.
- Porcelain panels.
- Graphite text.
- Teal primary controls.
- Filament coral alerts and warm emphasis.
- Subtle build-plate grid cues.
- Compact, practical page layouts rather than marketing-card clutter.

## Tech Stack Lock

| Layer | Technology |
|-------|------------|
| Framework | React 18+ |
| Language | TypeScript strict mode |
| Styling | CSS custom properties + scoped component CSS |
| State | React Query for server state, Zustand for client state |
| Routing | React Router v6 |
| Build | Vite |

## Color Tokens

### Graphite

| Token | Hex | Usage |
|-------|-----|-------|
| `--color-graphite-900` | `#111827` | Primary text |
| `--color-graphite-800` | `#1f2937` | Strong text, dark icon states |
| `--color-graphite-700` | `#374151` | Secondary strong text |
| `--color-graphite-600` | `#4b5563` | Body secondary text |
| `--color-graphite-500` | `#6b7280` | Muted text |
| `--color-graphite-400` | `#9ca3af` | Disabled text |
| `--color-graphite-300` | `#d1d5db` | Strong borders |
| `--color-graphite-100` | `#f3f4f6` | Subtle fills |

### Teal

| Token | Hex | Usage |
|-------|-----|-------|
| `--color-teal-700` | `#0f766e` | Primary active controls |
| `--color-teal-600` | `#0d9488` | Primary buttons |
| `--color-teal-500` | `#14b8a6` | Hover and highlights |
| `--color-teal-400` | `#2dd4bf` | Progress and accents |
| `--color-teal-300` | `#5eead4` | Focus affordances |
| `--color-teal-200` | `#99f6e4` | Soft badge border/fill |
| `--color-teal-100` | `#ccfbf1` | Soft icon backgrounds |

### Filament Coral

| Token | Hex | Usage |
|-------|-----|-------|
| `--color-coral-700` | `#c2410c` | Critical/destructive text |
| `--color-coral-600` | `#ea580c` | Warning actions |
| `--color-coral-500` | `#f97316` | Warm emphasis |
| `--color-coral-400` | `#fb923c` | Hover/secondary warm accents |
| `--color-coral-300` | `#fdba74` | Soft warm borders |
| `--color-coral-200` | `#fed7aa` | Warning border |
| `--color-coral-100` | `#ffedd5` | Warning background |

### Semantic Tokens

| Token | Value | Usage |
|-------|-------|-------|
| `--canvas` | `#f5f7fa` | App background |
| `--porcelain` | `#ffffff` | Panels, cards, inputs |
| `--linen` | `#eef2f7` | Secondary panels and fills |
| `--ink` | `var(--color-graphite-900)` | Primary text |
| `--muted` | `var(--color-graphite-500)` | Muted text |
| `--line` | `#d5dbe4` | Default borders |
| `--line-strong` | `#b8c2d0` | Strong borders |
| `--teal` | `var(--color-teal-600)` | Primary brand/control color |
| `--filament-coral` | `var(--color-coral-600)` | Warning/destructive color |

## Typography

| Token | Value | Usage |
|-------|-------|-------|
| `--font-sans` | `Inter, ui-sans-serif, system-ui, sans-serif` | Body and UI |
| `--font-mono` | `SF Mono, ui-monospace, monospace` | IDs, file hashes, metrics |
| `--text-xs` | `0.75rem` | Badges, timestamps |
| `--text-sm` | `0.875rem` | Dense UI body |
| `--text-base` | `1rem` | Standard body |
| `--text-lg` | `1.125rem` | Page descriptions |
| `--text-xl` | `1.25rem` | Panel headings |
| `--text-2xl` | `1.5rem` | Page headings |
| `--text-3xl` | `1.875rem` | Large headings |
| `--text-4xl` | `2.25rem` | Landing headline minimum |

Do not scale font size directly with viewport width. Use responsive layout, not fluid type, except for the landing H1 where `clamp()` is already established.

## Spacing And Shape

| Token | Value |
|-------|-------|
| `--space-1` | `4px` |
| `--space-2` | `8px` |
| `--space-3` | `12px` |
| `--space-4` | `16px` |
| `--space-5` | `20px` |
| `--space-6` | `24px` |
| `--space-8` | `32px` |
| `--space-10` | `40px` |
| `--space-12` | `48px` |
| `--space-16` | `64px` |

| Radius | Value | Usage |
|--------|-------|-------|
| `--radius-sm` | `4px` | Inputs, tags |
| `--radius-md` | `8px` | Buttons, cards |
| `--radius-lg` | `10px` | Panels |
| `--radius-xl` | `14px` | Larger composed surfaces |
| `--radius-full` | `9999px` | Pills, avatars |

Cards should generally stay at 8px radius unless the existing component uses the token above. Avoid cards inside cards.

## Shadows

| Token | Value | Usage |
|-------|-------|-------|
| `--shadow-sm` | `0 1px 2px rgba(15, 23, 42, 0.06)` | Subtle lift |
| `--shadow-md` | `0 8px 20px rgba(15, 23, 42, 0.08)` | Panels and menus |
| `--shadow-lg` | `0 18px 44px rgba(15, 23, 42, 0.12)` | Modals and drawers |
| `--shadow-glow` | `0 0 0 4px rgba(13, 148, 136, 0.14)` | Focus halo |

## Iconography

- Use the configured React icon library already in the app.
- Prefer icons for standard actions: sync, upload, download, invite, edit, search, filter, settings.
- Icon buttons must include accessible names/tooltips.
- Avoid manually drawn SVG icons unless representing a product/file visual that cannot come from the icon library.

## Core Component Patterns

### Buttons

Primary:

```text
background teal, white text, 8px radius, clear hover/active/focus states
```

Secondary:

```text
white or linen background, graphite text, line border
```

Danger:

```text
coral background or coral text depending on severity
```

Ghost:

```text
transparent background, graphite text, linen hover
```

### Panels

```text
porcelain background, line border, 8-10px radius, subtle shadow
```

Use panels for grouped work areas. Do not wrap entire page sections in floating cards when a full-width layout is clearer.

### Inputs

```text
porcelain background, line border, 8px radius, graphite text
focus uses teal outline/glow, errors use coral
```

### Data Tables

Tables are for operations work and should be dense but readable:

- Sticky or visually strong header where useful.
- Sort controls in headers.
- Compact status chips.
- Empty state row.
- Mobile fallback to stacked list rows.

### Status Chips

Canonical Phase 1 statuses:

| Status | Meaning |
|--------|---------|
| Synced | Etsy data is current |
| Needs Mapping | Etsy order/listing is not matched to a product |
| Needs Files | Product or part is missing current source files |
| Needs Personalization | Manual or automated customization required |
| Ready | Order or bundle can be downloaded |
| Downloaded | Bundle has been downloaded |
| Printed | User marked work printed |
| Blocked | Work cannot proceed without user action |

Color cannot be the only indicator of status.

## Build-Plate Grid Cue

Use a subtle grid pattern to evoke a printer build plate:

```css
.surface-grid {
  background-image:
    linear-gradient(var(--grid-line) 1px, transparent 1px),
    linear-gradient(90deg, var(--grid-line) 1px, transparent 1px);
  background-size: 2rem 2rem;
}
```

Apply sparingly to:

- Landing hero visual composition.
- Dashboard background accents.
- Empty states.
- File drop zones.

## Visual Assets And Mockups

To help lower-cost implementation agents produce a beautiful UI, issues should specify the visual assets instead of leaving them vague.

Required asset directions:

- Landing hero: polished bitmap or coded composition showing Etsy order cards, file tiles, and a downloadable 3MF/STL bundle.
- Product thumbnails: actual product photos/renders when available; otherwise neutral build-plate placeholders with clear product initials or file type.
- File thumbnails: small 3D-model preview, file-type badge, version number, upload metadata.
- Empty states: small purpose-built visual for no Etsy connection, no files, no orders, and no contributors.
- Contributor avatars: initials in restrained colored circles.
- No decorative orbs, abstract gradients, or generic stock imagery.

Recommendation: create a coded static prototype first, then generate one landing hero bitmap and a small set of product/file placeholder images once layout and copy are stable.

## Accessibility

- Minimum contrast ratio 4.5:1 for body text.
- Minimum contrast ratio 3:1 for UI components.
- Focus indicators: 2px solid teal with 2px offset or equivalent glow.
- Respect `prefers-reduced-motion`.
- Touch targets: minimum 44px.
- File upload must work through a keyboard-accessible file picker.

## Responsive Breakpoints

| Name | Width | Notes |
|------|-------|-------|
| `sm` | 640px | Single-column defaults |
| `md` | 768px | Side nav appears |
| `lg` | 1024px | Two-column detail layouts |
| `xl` | 1280px | Dashboard density increases |
| `2xl` | 1536px | Max content width 1440px |

## Lock

- Do not introduce a second frontend framework.
- Do not change the core color tokens without updating the prototype and this doc.
- Do not add password-based auth UI. OAuth only.
- Do not make dark theme the default.
- Do not make Bambu/printer controls part of the Phase 1 critical path.
