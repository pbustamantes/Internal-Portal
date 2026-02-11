# ADR-003: JWT Authentication with Refresh Tokens

**Status:** Accepted
**Date:** 2025-01

## Context

The application is a SPA (Next.js) communicating with a REST API. We need stateless authentication that works across browser tabs, supports role-based access control, and integrates with SignalR WebSocket connections.

## Decision

Use **JWT Bearer tokens** with a **refresh token rotation** scheme:

- **Access tokens** — Short-lived (1 hour, configurable via `Jwt:ExpiryHours`), signed with HS256 symmetric key
- **Refresh tokens** — Long-lived (7 days), stored in the database as `RefreshToken` entities, 64-byte random Base64 strings
- **Token rotation** — On refresh, the old token is revoked (`RevokedAtUtc` set) and replaced (`ReplacedByToken` set), a new pair is issued
- **Claims** — `NameIdentifier` (user ID), `Email`, `GivenName`, `Surname`, `Role`, `Jti` (unique token ID)
- **ClockSkew** — Set to `TimeSpan.Zero` (strict expiration, no grace period)
- **SignalR** — JWT extracted from `?access_token` query parameter via `OnMessageReceived` event (WebSocket connections cannot send headers)
- **Storage** — Tokens stored in `localStorage` on the frontend

### Password Hashing

BCrypt.Net with **work factor 11** for password hashing. `BCrypt.Verify()` provides time-constant comparison to prevent timing attacks.

## Alternatives Considered

- **Session cookies** — Simpler but stateful, doesn't work well with SPA + separate API server, CSRF concerns
- **OAuth 2.0 / OpenID Connect** — Overkill for an internal portal with no third-party auth providers
- **ASP.NET Identity** — Heavy framework with many features we don't need; custom implementation is simpler and more transparent
- **Argon2 over BCrypt** — Argon2 is newer but BCrypt is well-established, widely supported, and sufficient for this use case

## Consequences

**Benefits:**
- Stateless API — no server-side session storage, horizontal scaling friendly
- Refresh token rotation limits the window of a stolen token
- Role claims enable attribute-based authorization (`[Authorize(Roles = "Admin")]`)
- Works seamlessly with SignalR via query string

**Tradeoffs:**
- `localStorage` is vulnerable to XSS (mitigated by input sanitization and CSP headers)
- Token revocation requires database lookup for refresh tokens (access tokens are not revocable until expiry)
- Symmetric key (HS256) means the API is both issuer and validator — acceptable for a single-service deployment
