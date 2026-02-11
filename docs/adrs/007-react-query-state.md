# ADR-007: React Query for Server State, Zustand for Client State

**Status:** Accepted
**Date:** 2025-01

## Context

The frontend needs to manage two kinds of state:
1. **Server state** — Data fetched from the API (events, users, notifications, reports) that needs caching, background refreshing, and invalidation on mutations
2. **Client state** — UI state like real-time notification counts that change independently of API calls

## Decision

### Server State — TanStack React Query 5

All API data flows through custom hooks built on `useQuery` and `useMutation`:

- **Query keys** — Structured arrays (e.g., `['events', page, size, search, categoryId]`) for granular cache control
- **Stale time** — 60 seconds before background refetch
- **Retry** — 1 automatic retry on failure
- **Window focus refetch** — Disabled to prevent jarring UI updates
- **Mutation invalidation** — Every mutation invalidates related query keys on success (e.g., creating an event invalidates `['events']`)

### Client State — Zustand 5

A single Zustand store (`notification-store.ts`) manages real-time notification state:

- `notifications[]` — Received via SignalR
- `unreadCount` — Computed on add/read
- `addNotification()` — Called by SignalR event handler
- `markAsRead()` — Updates local state

### Authentication State — React Context

Auth state (user, tokens, login/logout) uses React Context because:
- It's app-level singleton state (one user at a time)
- It needs to wrap the entire component tree
- It's simpler than adding another state library for a single concern

## Alternatives Considered

- **Redux** — Too much boilerplate (actions, reducers, selectors) for this app's complexity
- **Zustand for everything** — Lacks React Query's caching, background refetch, and mutation invalidation
- **React Context for server state** — No caching, manual loading/error states, no background refetch
- **SWR** — Similar to React Query but less mature mutation support

## Consequences

**Benefits:**
- Clear separation: React Query owns API data, Zustand owns real-time UI state, Context owns auth
- React Query eliminates manual `useEffect` + `useState` patterns for data fetching
- Automatic cache invalidation keeps UI consistent after mutations
- Zustand has near-zero boilerplate (single `create()` call)

**Tradeoffs:**
- Three state management approaches may confuse new developers
- React Query's query key system requires discipline to keep consistent
- Zustand notification state may drift from server state (acceptable for real-time ephemeral data)
