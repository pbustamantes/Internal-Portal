# Frontend Architecture

## Table of Contents

- [Overview](#overview)
- [Tech Stack](#tech-stack)
- [Project Structure](#project-structure)
- [Application Providers](#application-providers)
- [Authentication](#authentication)
- [API Client](#api-client)
- [State Management](#state-management)
- [Data Fetching (React Query)](#data-fetching-react-query)
- [Custom Hooks](#custom-hooks)
- [Real-time Notifications (SignalR)](#real-time-notifications-signalr)
- [Components](#components)
- [Pages & Routing](#pages--routing)
- [Styling](#styling)
- [Type Definitions](#type-definitions)
- [Build & Configuration](#build--configuration)

---

## Overview

The frontend is a **Next.js 15** application using the **App Router**, **TypeScript**, and **Tailwind CSS v4**. It communicates with the ASP.NET Core API via Axios and receives real-time notifications through a SignalR WebSocket connection.

All pages are client-rendered (`'use client'`) behind an auth guard. Data fetching uses TanStack React Query for caching and automatic invalidation.

---

## Tech Stack

| Library | Version | Purpose |
|---|---|---|
| Next.js | 15.1.0 | Framework (App Router, standalone output) |
| React | 19 | UI rendering |
| TypeScript | 5 | Type safety |
| Tailwind CSS | 4.0.0 | Utility-first styling |
| TanStack React Query | 5.62.0 | Server state, caching, mutations |
| Axios | 1.7.9 | HTTP client with interceptors |
| Zustand | 5.0.2 | Client state management |
| React Hook Form | 7.54.2 | Form state and validation |
| Zod | 3.24.1 | Schema validation |
| @microsoft/signalr | 8.0.7 | Real-time WebSocket notifications |
| FullCalendar | 6.1.15 | Calendar view |
| Recharts | 2.15.0 | Charts for reports |
| Lucide React | 0.468.0 | Icon library |
| Sonner | 1.7.1 | Toast notifications |

---

## Project Structure

```
src/Frontend/internal-portal-web/src/
├── app/                          # Next.js App Router pages
│   ├── layout.tsx                # Root layout (providers, font, metadata)
│   ├── page.tsx                  # Landing page (redirects to /dashboard)
│   ├── globals.css               # Tailwind CSS import
│   ├── login/page.tsx            # Login form
│   ├── register/page.tsx         # Registration form
│   ├── dashboard/page.tsx        # Dashboard with stats and upcoming events
│   ├── events/
│   │   ├── page.tsx              # Event listing with search/pagination
│   │   ├── create/page.tsx       # Create event form
│   │   └── [id]/
│   │       ├── page.tsx          # Event detail with actions
│   │       └── edit/page.tsx     # Edit event form
│   ├── calendar/page.tsx         # FullCalendar month view
│   ├── my-events/page.tsx        # User's registrations
│   ├── notifications/page.tsx    # Notification center
│   ├── profile/page.tsx          # Profile editor + password change
│   └── admin/
│       ├── events/page.tsx       # Admin event management table
│       ├── users/page.tsx        # Admin user management
│       ├── venues/
│       │   ├── page.tsx          # Venue list
│       │   ├── create/page.tsx   # Create venue form
│       │   └── [id]/edit/page.tsx # Edit venue form
│       └── reports/page.tsx      # Attendance and popularity reports
│
├── components/
│   ├── layout/
│   │   ├── auth-guard.tsx        # Protects routes, redirects unauthenticated users
│   │   ├── header.tsx            # Page header with notification bell
│   │   └── sidebar.tsx           # Navigation sidebar with role-based menu
│   └── ui/
│       ├── button.tsx            # Button with variants and sizes
│       ├── input.tsx             # Form input with label and error display
│       ├── card.tsx              # Card, CardHeader, CardContent, CardTitle
│       ├── badge.tsx             # Status badge with color mapping
│       ├── modal.tsx             # Overlay modal dialog
│       ├── loading.tsx           # Spinner animation
│       └── table.tsx             # Table, TableHeader, TableBody, TableRow, etc.
│
├── hooks/
│   ├── use-events.ts             # Event CRUD queries and mutations
│   ├── use-notifications.ts      # Notification queries and mutations
│   ├── use-registrations.ts      # Registration queries and mutations
│   ├── use-reports.ts            # Report queries (admin)
│   └── use-venues.ts             # Venue CRUD queries and mutations
│
├── lib/
│   ├── api.ts                    # Axios instance with auth interceptors
│   ├── auth-context.tsx          # AuthProvider and useAuth hook
│   ├── notification-store.ts     # Zustand store for real-time notifications
│   ├── query-provider.tsx        # React Query client and provider
│   ├── signalr.ts                # SignalR hub connection factory
│   └── utils.ts                  # cn(), formatDate(), formatTimeAgo(), getStatusColor()
│
├── next.config.ts                # output: "standalone"
├── postcss.config.mjs            # @tailwindcss/postcss plugin
└── tsconfig.json                 # Strict mode, @/* path alias
```

---

## Application Providers

The root layout wraps the entire app in a provider hierarchy:

```
<html>
  <body>
    <QueryProvider>          ← React Query client (staleTime: 60s, retry: 1)
      <AuthProvider>         ← Auth context (user, tokens, login/logout)
        <Toaster />          ← Sonner toast container (top-right)
        {children}           ← Page content
      </AuthProvider>
    </QueryProvider>
  </body>
</html>
```

**QueryProvider** configures React Query with:
- `staleTime: 60_000` (1 minute before refetch)
- `retry: 1` (one retry on failure)
- `refetchOnWindowFocus: false`

---

## Authentication

### Auth Context (`lib/auth-context.tsx`)

Manages authentication state via React Context:

```
AuthProvider
├── State: user, isLoading, isAuthenticated
├── Methods: login(), register(), logout(), refreshUser()
└── Storage: localStorage (accessToken, refreshToken)
```

**Flow:**

1. On mount, reads `accessToken` from localStorage
2. If token exists, fetches `/api/users/me` to hydrate user state
3. `login()` and `register()` call the auth API, store tokens, set user
4. `logout()` calls `/api/auth/revoke`, clears tokens, redirects to `/login`
5. On 401 response, attempts token refresh via `/api/auth/refresh`
6. If refresh fails, clears tokens and redirects to `/login`

### Auth Guard (`components/layout/auth-guard.tsx`)

Wraps all protected pages. Checks `isAuthenticated` and `isLoading`:
- While loading → renders `<Loading />` spinner
- If not authenticated → redirects to `/login`
- If authenticated → renders children

Every protected page follows this pattern:

```tsx
<AuthGuard>
  <Sidebar />
  <div className="ml-64">
    <Header title="Page Title" />
    <main className="p-8">{/* page content */}</main>
  </div>
</AuthGuard>
```

---

## API Client

### Axios Instance (`lib/api.ts`)

```
Axios instance (baseURL: NEXT_PUBLIC_API_URL/api)
├── Request interceptor: Attach Bearer token from localStorage
└── Response interceptor: Handle 401 → refresh token → retry original request
```

- Base URL defaults to `http://localhost:5001/api`
- Every request automatically includes `Authorization: Bearer <token>`
- On 401 response:
  1. Reads `refreshToken` from localStorage
  2. Calls `POST /api/auth/refresh`
  3. Stores new tokens
  4. Retries the original failed request
  5. If refresh fails → clears tokens, redirects to `/login`

---

## State Management

The app uses two state management approaches:

### Server State — React Query

All API data (events, notifications, registrations, venues, reports) is managed by React Query through custom hooks. This handles caching, background refetching, loading states, and cache invalidation on mutations.

### Client State — Zustand

Real-time notification state is managed by a Zustand store:

```ts
// lib/notification-store.ts
interface NotificationState {
  notifications: Notification[]
  unreadCount: number
  addNotification(notification)   // Prepends and increments count
  setNotifications(notifications) // Replaces all, recalculates count
  markAsRead(id)                  // Updates single notification
}
```

The Header component reads `unreadCount` to show the notification badge.

---

## Data Fetching (React Query)

All data fetching follows a consistent pattern:

```
Custom Hook → useQuery/useMutation → Axios → API → Cache invalidation
```

### Query Keys

| Hook | Query Key | Data |
|---|---|---|
| `useEvents(page, size, search, categoryId)` | `['events', page, size, search, categoryId]` | Paginated events |
| `useEvent(id)` | `['event', id]` | Single event detail |
| `useUpcomingEvents(count)` | `['events', 'upcoming', count]` | Upcoming events list |
| `useCalendarEvents(start, end)` | `['events', 'calendar', start, end]` | Date range events |
| `useEventAttendees(eventId)` | `['events', eventId, 'attendees']` | Attendee list |
| `useNotifications(unreadOnly)` | `['notifications', unreadOnly]` | User notifications |
| `useMyRegistrations()` | `['registrations', 'me']` | User's registrations |
| `useVenues()` | `['venues']` | All venues |
| `useVenue(id)` | `['venue', id]` | Single venue |
| `useAttendanceReport()` | `['reports', 'attendance']` | Attendance stats |
| `useMonthlyReport(year)` | `['reports', 'monthly', year]` | Monthly stats |
| `usePopularEvents(top)` | `['reports', 'popular', top]` | Popular events |

### Mutation Invalidation

All mutations invalidate related query keys on success to keep the UI in sync:

| Mutation | Invalidates |
|---|---|
| Create/Update/Delete event | `['events']` |
| Publish/Cancel event | `['events']`, `['event', id]` |
| Register/Cancel registration | `['events']`, `['registrations']` |
| Mark notification read | `['notifications']` |
| Create/Update/Delete venue | `['venues']` |

---

## Custom Hooks

### `use-events.ts`

| Hook | Type | Description |
|---|---|---|
| `useEvents(page, pageSize, search?, categoryId?)` | Query | Paginated event listing |
| `useEvent(id)` | Query | Single event with full details |
| `useUpcomingEvents(count)` | Query | Next N upcoming events |
| `useCalendarEvents(start, end)` | Query | Events in a date range |
| `useEventAttendees(eventId)` | Query | List of attendees for an event |
| `useCreateEvent()` | Mutation | Create a new event (Draft) |
| `useUpdateEvent()` | Mutation | Update event fields |
| `useDeleteEvent()` | Mutation | Delete an event |
| `usePublishEvent()` | Mutation | Transition Draft → Published |
| `useCancelEvent()` | Mutation | Cancel an event |

### `use-registrations.ts`

| Hook | Type | Description |
|---|---|---|
| `useRegisterForEvent()` | Mutation | Register current user for an event |
| `useCancelRegistration()` | Mutation | Cancel current user's registration |
| `useMyRegistrations()` | Query | Current user's registrations |

### `use-notifications.ts`

| Hook | Type | Description |
|---|---|---|
| `useNotifications(unreadOnly?)` | Query | Current user's notifications |
| `useMarkNotificationRead()` | Mutation | Mark a notification as read |

### `use-venues.ts`

| Hook | Type | Description |
|---|---|---|
| `useVenues()` | Query | All venues |
| `useVenue(id)` | Query | Single venue by ID |
| `useCreateVenue()` | Mutation | Create venue (Admin) |
| `useUpdateVenue()` | Mutation | Update venue (Admin) |
| `useDeleteVenue()` | Mutation | Delete venue (Admin) |

### `use-reports.ts`

| Hook | Type | Description |
|---|---|---|
| `useAttendanceReport()` | Query | Event attendance statistics |
| `useMonthlyReport(year)` | Query | Monthly event/registration stats |
| `usePopularEvents(top?)` | Query | Most popular events by fill rate |

---

## Real-time Notifications (SignalR)

### Connection (`lib/signalr.ts`)

Creates a SignalR `HubConnection` to `/hubs/notifications`:

- Authenticates via `accessTokenFactory` reading from localStorage
- Auto-reconnect enabled
- Receives `ReceiveNotification` events with `{ title, message, timestamp }`

### Integration Flow

```
SignalR Hub
  → "ReceiveNotification" event
    → Zustand store: addNotification()
      → Header component: re-renders badge with unreadCount
      → Sonner: displays toast notification
```

The connection is established when the user is authenticated and torn down on logout.

---

## Components

### Layout Components

| Component | File | Description |
|---|---|---|
| `AuthGuard` | `components/layout/auth-guard.tsx` | Route protection. Redirects to `/login` if not authenticated, shows spinner while loading |
| `Header` | `components/layout/header.tsx` | Top bar with page title and notification bell. Shows unread count badge from Zustand store |
| `Sidebar` | `components/layout/sidebar.tsx` | Left navigation (fixed, 256px). Shows navigation links, admin section for Admin role, user avatar with profile picture or initials, logout button |

### UI Primitives

| Component | File | Props | Description |
|---|---|---|---|
| `Button` | `ui/button.tsx` | `variant`, `size`, `disabled`, `onClick` | Variants: primary (blue), secondary (gray), danger (red), ghost (transparent). Sizes: sm, md, lg. Forwards ref |
| `Input` | `ui/input.tsx` | `label`, `error`, `type`, `...rest` | Form input with label above and red error text below. Focus ring styling |
| `Card` | `ui/card.tsx` | `children`, `className` | Container with white background, rounded corners, shadow. Sub-components: `CardHeader`, `CardContent`, `CardTitle` |
| `Badge` | `ui/badge.tsx` | `status`, `children` | Color-coded status pill. Uses `getStatusColor()` to map status strings to Tailwind classes |
| `Modal` | `ui/modal.tsx` | `isOpen`, `onClose`, `title`, `children` | Overlay dialog with backdrop click to close. Fixed center positioning |
| `Loading` | `ui/loading.tsx` | — | Centered spinning circle animation |
| `Table` | `ui/table.tsx` | `children` | Full-width table with sub-components: `TableHeader`, `TableBody`, `TableRow`, `TableHead`, `TableCell` |

### Status Color Mapping (`lib/utils.ts`)

```
Draft       → gray
Published   → green
Cancelled   → red
Completed   → blue
Confirmed   → green
Waitlisted  → yellow
Pending     → gray
```

---

## Pages & Routing

### Public Pages (no auth required)

| Route | Page | Description |
|---|---|---|
| `/login` | Login form | Email/password, link to register, toast on error |
| `/register` | Registration form | Name, email, password, optional department |

### Protected Pages (auth required)

| Route | Page | Description |
|---|---|---|
| `/` | Landing | Redirects to `/dashboard` |
| `/dashboard` | Dashboard | Quick stats cards, upcoming events list, recent notifications |
| `/events` | Event listing | Search input, category filter, paginated event cards, "Create Event" button (Organizer/Admin) |
| `/events/[id]` | Event detail | Full event info, attendee table, action buttons (publish, edit, cancel, delete, register). Buttons hidden for past/completed events |
| `/events/create` | Create event | Form with title, description, dates, capacity, location, recurrence, category, venue |
| `/events/[id]/edit` | Edit event | Pre-filled form, same fields as create |
| `/calendar` | Calendar view | FullCalendar month view with event markers, navigation |
| `/my-events` | My registrations | Table of user's event registrations with status and cancel option |
| `/notifications` | Notification center | List of notifications with mark-as-read button |
| `/profile` | Profile editor | Edit name/department, change password, upload/remove profile picture |

### Admin Pages (Admin role required)

| Route | Page | Description |
|---|---|---|
| `/admin/events` | Event management | Table of all events with pagination, edit/delete actions. Buttons hidden for completed events |
| `/admin/users` | User management | User list with role management |
| `/admin/venues` | Venue management | Venue table with create/edit/delete |
| `/admin/venues/create` | Create venue | Form with name, capacity, address fields |
| `/admin/venues/[id]/edit` | Edit venue | Pre-filled venue form |
| `/admin/reports` | Reports | Attendance charts (Recharts), monthly stats, popular events |

### Dynamic Route Parameters

Next.js 15 uses the `Promise<{ id: string }>` pattern for dynamic params:

```tsx
export default function Page({ params }: { params: Promise<{ id: string }> }) {
  const { id } = use(params);
  // ...
}
```

---

## Styling

### Tailwind CSS v4

The project uses Tailwind CSS v4 with the modern import syntax:

```css
/* globals.css */
@import "tailwindcss";
```

PostCSS is configured with the `@tailwindcss/postcss` plugin (not the legacy `tailwindcss` plugin).

### Design System

- **Colors:** Blue primary, gray secondary, red danger, green success, yellow warning
- **Typography:** Inter font (Google Fonts), loaded in root layout
- **Spacing:** Consistent `p-8` page padding, `gap-3`/`gap-4` for flex layouts
- **Shadows:** Cards use subtle shadow (`shadow-sm` or default shadow)
- **Responsive:** Sidebar is fixed at 256px (`ml-64`), content fills remaining width
- **Class merging:** `cn()` utility combines `clsx` and `tailwind-merge` for conflict-free class composition

---

## Type Definitions

Key TypeScript types used across the app:

```typescript
// User & Auth
interface User {
  id: string
  email: string
  firstName: string
  lastName: string
  department?: string
  role: 'Employee' | 'Organizer' | 'Admin'
  profilePictureUrl?: string
}

interface AuthResponse {
  accessToken: string
  refreshToken: string
  expiresAt: string
  user: User
}

// Events
interface EventDto {
  id: string; title: string; description?: string
  startUtc: string; endUtc: string
  minAttendees: number; maxAttendees: number; currentAttendees: number
  status: 'Draft' | 'Published' | 'Cancelled' | 'Completed'
  recurrence: 'None' | 'Daily' | 'Weekly' | 'Monthly'
  locationStreet?: string; locationCity?: string; locationState?: string
  locationZipCode?: string; locationBuilding?: string; locationRoom?: string
  organizerId: string; organizerName: string
  categoryId?: string; categoryName?: string; categoryColor?: string
  venueId?: string; venueName?: string
  createdAtUtc: string
}

interface EventSummaryDto {
  id: string; title: string
  startUtc: string; endUtc: string
  maxAttendees: number; currentAttendees: number
  status: string
  categoryName?: string; categoryColor?: string
  organizerName: string
}

// Pagination
interface PaginatedList<T> {
  items: T[]
  pageNumber: number; totalPages: number; totalCount: number
  hasPreviousPage: boolean; hasNextPage: boolean
}

// Registration
interface RegistrationDto {
  id: string; userId: string; userName: string
  eventId: string; eventTitle: string
  status: 'Pending' | 'Confirmed' | 'Cancelled' | 'Waitlisted'
  registeredAtUtc: string
}

// Notification
interface NotificationDto {
  id: string; title: string; message: string
  type: string; isRead: boolean
  eventId?: string; createdAtUtc: string
}

// Venue
interface VenueDto {
  id: string; name: string; capacity: number
  street: string; city: string; state: string; zipCode: string
  building?: string; room?: string
}
```

---

## Build & Configuration

### next.config.ts

```ts
const nextConfig = {
  output: "standalone"  // Self-contained Node.js server for Docker
};
```

### tsconfig.json

- **Target:** ES2017
- **Strict mode:** Enabled
- **Path alias:** `@/*` → `./src/*`
- **Module:** ESNext
- **JSX:** Preserve

### postcss.config.mjs

```js
const config = {
  plugins: {
    "@tailwindcss/postcss": {}  // Tailwind CSS v4 PostCSS plugin
  }
};
```

### Scripts

```json
{
  "dev": "next dev",           // Development server with hot reload
  "build": "next build",       // Production build
  "start": "next start",       // Start production server
  "lint": "next lint"          // ESLint check
}
```
