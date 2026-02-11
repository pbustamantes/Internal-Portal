# ADR-010: Centralized Exception Handling

**Status:** Accepted
**Date:** 2025-01

## Context

The API needs consistent error responses across all endpoints. Without centralized handling, each controller action would need its own try-catch blocks, leading to inconsistent error formats and duplicated code.

## Decision

Use a **custom exception handling middleware** (`ExceptionHandlingMiddleware`) as the outermost middleware in the pipeline, combined with a **typed exception hierarchy**:

### Exception Types

| Exception | Layer | HTTP Status | Purpose |
|---|---|---|---|
| `DomainException` | Domain | 400 | Business rule violations (e.g., "Past events cannot be modified") |
| `ValidationException` | Application | 400 | Input validation failures with per-field errors |
| `NotFoundException` | Application | 404 | Entity not found by ID |
| `ForbiddenException` | Application | 403 | Insufficient permissions |
| `ApplicationException` | .NET | 400 | General application errors (e.g., invalid credentials) |
| Any other | — | 500 | Unexpected errors (logged, generic message returned) |

### Error Response Format

```json
{
  "title": "Error Type",
  "detail": "Human-readable message",
  "errors": { "fieldName": ["error1"] }
}
```

The `errors` field is only populated for `ValidationException`; it's `null` for all other error types.

### Middleware Behavior
1. Wraps the entire request pipeline in a try-catch
2. Logs all exceptions via `ILogger`
3. Maps exception type → HTTP status code via pattern matching
4. Serializes `ErrorResponse` record to camelCase JSON
5. For 500 errors, returns a generic message (never exposes internal details)

## Alternatives Considered

- **Problem Details (RFC 7807)** — More standardized but heavier; our format is simpler and sufficient
- **Exception filters** — Only work within MVC pipeline, not for middleware-level errors
- **Result pattern (no exceptions)** — Eliminates throw overhead but adds complexity to every handler return type

## Consequences

**Benefits:**
- Every error response has the same structure, simplifying frontend error handling
- Domain and application layers throw exceptions naturally without knowing about HTTP
- Unhandled exceptions never leak stack traces to clients
- Single place to modify error response format

**Tradeoffs:**
- Exceptions for control flow (e.g., `NotFoundException`) have minor performance cost
- All errors are caught at the same level — no per-endpoint customization without additional middleware
