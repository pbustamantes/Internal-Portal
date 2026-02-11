# ADR-002: CQRS with MediatR

**Status:** Accepted
**Date:** 2025-01

## Context

Controllers need to execute business logic (create events, register users, generate reports). Injecting repositories and services directly into controllers couples them to implementation details and makes cross-cutting concerns (logging, validation, performance monitoring) repetitive.

## Decision

Use **MediatR 12.2.0** to implement the CQRS (Command Query Responsibility Segregation) pattern:

- Every use case is a **command** (writes) or **query** (reads) record implementing `IRequest<TResponse>`
- Each command/query has a dedicated **handler** implementing `IRequestHandler<TRequest, TResponse>`
- Controllers only depend on `IMediator` and dispatch requests
- Cross-cutting concerns are handled by **pipeline behaviors** registered in order:
  1. `UnhandledExceptionBehavior` — catches and logs all exceptions
  2. `ValidationBehavior` — runs FluentValidation validators before handler execution
  3. `PerformanceBehavior` — logs warnings for requests exceeding 500ms
  4. `LoggingBehavior` — logs all incoming/outgoing requests

Domain events (`BaseDomainEvent : INotification`) are published via `IMediator.Publish()` after database save, routing to `INotificationHandler<T>` implementations.

## Alternatives Considered

- **Direct service injection in controllers** — simpler but couples controllers to business logic, no pipeline behaviors
- **Custom mediator** — unnecessary given MediatR's maturity and ecosystem

## Consequences

**Benefits:**
- Controllers are thin (dispatch only), testable without HTTP context
- Pipeline behaviors eliminate boilerplate (try-catch, logging, validation) from every handler
- Adding a new use case = adding a command + handler, no controller changes needed
- Domain events decouple side effects (notifications) from aggregate state changes

**Tradeoffs:**
- Indirection: request → mediator → handler requires navigating through types
- One class per command + one class per handler increases file count
- MediatR.Contracts dependency in Domain layer (lightweight, but still a dependency)
