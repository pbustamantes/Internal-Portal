# ADR-006: Next.js 15 with App Router

**Status:** Accepted
**Date:** 2025-01

## Context

The frontend needs to be a modern single-page application with client-side routing, authentication, real-time updates, and a rich UI for event management, calendar views, and admin dashboards.

## Decision

Use **Next.js 15** with the **App Router** and **React 19**:

- **App Router** — File-based routing under `src/app/`, layouts, dynamic routes with `[id]` segments
- **Client components** — All pages use `'use client'` since they depend on browser APIs (localStorage, WebSocket, hooks)
- **Standalone output** — `next.config.ts` sets `output: "standalone"` for Docker-optimized production builds
- **TypeScript** — Strict mode enabled with `@/*` path alias for clean imports

### Key Libraries

| Concern | Library | Rationale |
|---|---|---|
| HTTP client | Axios | Request/response interceptors for JWT injection and 401 refresh |
| Server state | TanStack React Query | Caching, background refetch, mutation invalidation |
| Client state | Zustand | Lightweight store for real-time notification state |
| Forms | React Hook Form + Zod | Minimal re-renders, type-safe schema validation |
| Styling | Tailwind CSS v4 | Utility-first, no CSS-in-JS runtime cost |
| Calendar | FullCalendar | Full-featured calendar with day/week/month views |
| Charts | Recharts | Composable React chart components for reports |
| Icons | Lucide React | Tree-shakeable SVG icons |
| Toasts | Sonner | Minimal, accessible toast notifications |
| Real-time | @microsoft/signalr | WebSocket client matching the ASP.NET SignalR server |

## Alternatives Considered

- **Create React App** — No longer maintained, lacks built-in routing and SSR
- **Vite + React Router** — Viable but lacks Next.js conventions (file routing, layouts, image optimization)
- **Pages Router** — Legacy Next.js routing; App Router is the recommended approach
- **Angular / Vue** — React ecosystem better fits the team's experience

## Consequences

**Benefits:**
- File-based routing reduces boilerplate
- Standalone output produces a minimal Node.js server for Docker
- React 19 features (use hook for async params) simplify data loading
- Rich ecosystem of compatible libraries

**Tradeoffs:**
- All pages are client-rendered (`'use client'`), not leveraging Next.js server components — acceptable for an authenticated SPA
- `NEXT_PUBLIC_API_URL` is baked in at build time, requiring a rebuild per environment
- Next.js 15 is relatively new; some library compatibility issues may arise
