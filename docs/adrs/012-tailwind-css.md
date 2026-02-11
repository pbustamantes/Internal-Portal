# ADR-012: Tailwind CSS v4 for Styling

**Status:** Accepted
**Date:** 2025-01

## Context

The frontend needs a styling approach that supports rapid UI development, consistent design tokens, and small production bundle sizes without the overhead of a component library like Material-UI.

## Decision

Use **Tailwind CSS v4** with the modern PostCSS plugin:

- **PostCSS plugin** — `@tailwindcss/postcss` (v4 approach, not the legacy `tailwindcss` plugin)
- **CSS import** — `@import "tailwindcss"` in `globals.css` (replaces v3's `@tailwind base/components/utilities` directives)
- **Class merging** — `cn()` utility combining `clsx` (conditional classes) and `tailwind-merge` (conflict resolution)
- **Design tokens** — Blue primary, gray secondary, red danger, green success, yellow warning
- **Font** — Inter from Google Fonts, loaded in root layout
- **Layout** — Fixed sidebar (256px / `ml-64`), fluid content area, consistent `p-8` page padding

### Component Library

Custom UI primitives built with Tailwind:

| Component | Variants |
|---|---|
| Button | primary, secondary, danger, ghost / sm, md, lg |
| Badge | Color mapped by status string |
| Card | CardHeader, CardContent, CardTitle |
| Input | With label, error text, focus ring |
| Modal | Overlay with backdrop close |
| Table | Full-width with header/body/row/cell |
| Loading | Spinning border animation |

## Alternatives Considered

- **Material-UI / Chakra UI** — Opinionated component libraries; heavier bundle, harder to customize, style conflicts
- **CSS Modules** — Good isolation but more boilerplate, no utility-first productivity
- **Styled Components / Emotion** — Runtime CSS-in-JS has performance cost, less readable in JSX
- **Tailwind CSS v3** — Stable but v4's PostCSS plugin is faster and simpler to configure

## Consequences

**Benefits:**
- No context switching between files — styles live in the component JSX
- Utility classes produce small, deterministic CSS bundles (unused classes are purged)
- `cn()` utility prevents class conflicts (e.g., `w-full` vs `w-1/2`)
- Custom components are lightweight wrappers, not heavy abstractions
- Consistent spacing, colors, and typography via Tailwind's design system

**Tradeoffs:**
- Long class strings can reduce JSX readability (mitigated by extracting components)
- No pre-built component library — UI primitives must be built manually
- Tailwind v4 is relatively new; some community plugins may not support it yet
