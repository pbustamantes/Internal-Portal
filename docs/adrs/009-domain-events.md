# ADR-009: Domain Events for Side Effects

**Status:** Accepted
**Date:** 2025-01

## Context

When domain state changes occur (event published, user registered, event cancelled), side effects need to happen: notifications sent, emails queued, real-time messages pushed via SignalR. These side effects should not pollute the domain logic or couple aggregates to infrastructure services.

## Decision

Use **domain events** dispatched after database persistence:

### Event Creation
- Domain entities add events via `AddDomainEvent(BaseDomainEvent)` during business operations
- Events are queued in a private list on `BaseEntity`, not dispatched immediately
- Example: `Event.Publish()` adds `EventCreatedDomainEvent(Id, Title)` after setting status to Published

### Event Dispatch
- `ApplicationDbContext.SaveChangesAsync()` handles dispatch:
  1. Sets audit fields (CreatedAtUtc, UpdatedAtUtc)
  2. Calls `base.SaveChangesAsync()` to persist changes to the database
  3. Extracts domain events from all tracked entities via `ChangeTracker`
  4. Publishes each event via `IMediator.Publish(domainEvent)`
  5. Clears events from entities to prevent re-publication

### Event Handling
- Handlers implement `INotificationHandler<TDomainEvent>`:
  - `EventCreatedDomainEventHandler` → broadcasts notification to all users
  - `EventCancelledDomainEventHandler` → broadcasts cancellation notice
  - `UserRegisteredDomainEventHandler` → sends targeted notification to the registering user

### Defined Events

| Event | Raised By | Data |
|---|---|---|
| `EventCreatedDomainEvent` | `Event.Publish()` | EventId, Title |
| `EventCancelledDomainEvent` | `Event.Cancel()` | EventId, Title |
| `UserRegisteredDomainEvent` | `Event.Register()` | UserId, EventId, Status |
| `RegistrationConfirmedDomainEvent` | `Registration.Confirm()` | UserId, EventId |
| `RegistrationCancelledDomainEvent` | `Registration.Cancel()` | UserId, EventId |

## Alternatives Considered

- **Direct service calls in handlers** — Couples command handlers to notification infrastructure
- **Outbox pattern** — More reliable for distributed systems but adds complexity; unnecessary for a single-service deployment
- **Event bus (RabbitMQ, Kafka)** — Overkill for in-process side effects

## Consequences

**Benefits:**
- Domain entities express business intent without knowing about notifications or infrastructure
- Side effects only run after successful database save (consistency)
- Adding new side effects = adding a new handler, no changes to existing code
- Events serve as documentation of what the domain considers significant

**Tradeoffs:**
- Events dispatched in the same transaction; a handler failure could leave partial state (mitigated by handlers doing non-critical work like notifications)
- `SaveChangesAsync()` override couples DbContext to MediatR
- Domain events are not persisted (no event sourcing / audit log from events themselves)
