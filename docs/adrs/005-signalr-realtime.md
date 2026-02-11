# ADR-005: SignalR for Real-Time Notifications

**Status:** Accepted
**Date:** 2025-01

## Context

When events are published, cancelled, or users register, other users should be notified immediately without needing to refresh the page. The notification system needs to support both broadcast (all users) and targeted (specific user) delivery.

## Decision

Use **ASP.NET Core SignalR** for real-time server-to-client push notifications:

- **Hub** — `NotificationHub` at `/hubs/notifications`, requires JWT authentication
- **User groups** — On connect, the user is added to a group named by their `UserId`; removed on disconnect
- **Delivery modes:**
  - `Clients.All.SendAsync()` — Broadcast to all connected users (event published, event cancelled)
  - `Clients.User(userId).SendAsync()` — Targeted to a specific user (registration confirmation)
- **Client event** — `ReceiveNotification` with payload `{ title, message, timestamp }`
- **Integration** — Domain event handlers call `INotificationService`, which uses `IHubContext<NotificationHub>` to send messages
- **Frontend** — `@microsoft/signalr` client connects on auth, receives events, updates Zustand store and shows toast

### Authentication

SignalR WebSocket connections cannot send HTTP headers. JWT is passed via `?access_token` query parameter and extracted in the `OnMessageReceived` event handler in `Program.cs`.

## Alternatives Considered

- **Polling** — Simpler but wastes bandwidth and adds latency (seconds vs milliseconds)
- **Server-Sent Events (SSE)** — Unidirectional only, no built-in .NET integration like SignalR
- **Raw WebSockets** — No automatic fallback, reconnection, or group management
- **Third-party (Pusher, Ably)** — External dependency, cost, unnecessary for an internal app

## Consequences

**Benefits:**
- Sub-second notification delivery
- Automatic WebSocket → Long Polling fallback for restricted networks
- Built-in connection management and auto-reconnect
- User-based groups enable targeted notifications without custom routing
- Tight integration with ASP.NET Core authentication

**Tradeoffs:**
- Persistent connections consume server resources (one per connected user)
- Horizontal scaling requires a backplane (Redis, Azure SignalR Service) — not needed at current scale
- JWT in query string is visible in server logs (mitigated by HTTPS)
