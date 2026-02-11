# ADR-001: Onion Architecture with DDD

**Status:** Accepted
**Date:** 2025-01

## Context

We needed an architecture for a medium-complexity internal portal with event management, user authentication, notifications, and reporting. The application requires clear separation between business logic, data access, and presentation to support long-term maintainability and testability.

## Decision

Adopt **Onion Architecture** (a variant of Clean Architecture) with Domain-Driven Design principles, organized into five projects with strict inward dependency direction:

```
Domain (innermost) → Application → Persistence / Infrastructure → API (outermost)
```

- **Domain** — Entities, value objects, domain events, repository interfaces. Zero external dependencies (only `MediatR.Contracts` for `INotification`)
- **Application** — CQRS commands/queries/handlers, DTOs, validators, pipeline behaviors. Depends on Domain
- **Persistence** — EF Core DbContext, entity configurations, repository implementations. Depends on Domain + Application
- **Infrastructure** — JWT, identity, SignalR, email services. Depends on Domain + Application
- **API** — Controllers, middleware, startup. References all layers for DI wiring

Each layer exposes an `AddXyz()` extension method on `IServiceCollection` for self-contained DI registration.

## Consequences

**Benefits:**
- Domain logic is testable without any framework dependencies (31+ pure unit tests)
- Swapping persistence (e.g., SQL Server → PostgreSQL) requires changes only in the Persistence project
- Infrastructure services (JWT, email) can be replaced without touching business logic
- Clear ownership boundaries for each project

**Tradeoffs:**
- More projects and files than a simple layered architecture
- Indirect data access through repository interfaces adds a layer of abstraction
- New developers need to understand the dependency direction rules
