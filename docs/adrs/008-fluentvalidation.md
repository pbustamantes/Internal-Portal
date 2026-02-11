# ADR-008: FluentValidation over Data Annotations

**Status:** Accepted
**Date:** 2025-01

## Context

Commands and queries need input validation before reaching business logic. Validation rules range from simple (required fields, max length) to complex (end date must be after start date, max attendees must be >= min attendees).

## Decision

Use **FluentValidation 11.9.0** with MediatR pipeline integration:

- Validators are separate classes (`AbstractValidator<TCommand>`) co-located with their commands
- All validators auto-registered via `AddValidatorsFromAssembly()`
- `ValidationBehavior<TRequest, TResponse>` runs all matching validators before the handler executes
- On failure, throws `ValidationException` with a dictionary of field-level errors
- `ExceptionHandlingMiddleware` maps `ValidationException` to 400 Bad Request with the errors dictionary

### Example Validators

- `CreateEventCommandValidator` — Title not empty (max 200), dates in future, EndUtc > StartUtc, MaxAttendees >= MinAttendees
- `RegisterUserCommandValidator` — Email format, password 8-128 chars, name fields not empty
- `CreateVenueCommandValidator` — Name not empty (max 200), capacity > 0, address fields required

## Alternatives Considered

- **Data Annotations** — Simpler but limited to declarative rules, hard to express cross-field validation, pollutes command records with attributes
- **Manual validation in handlers** — No separation of concerns, duplicated validation logic
- **Custom validation middleware** — Reinvents FluentValidation's features

## Consequences

**Benefits:**
- Validation logic is testable independently
- Complex rules expressed as readable fluent chains
- Cross-field validation (e.g., EndUtc > StartUtc) is natural
- Pipeline behavior ensures validation runs consistently for all commands
- Error response includes per-field error messages for frontend display

**Tradeoffs:**
- One more class per command that needs validation
- Validators must be kept in sync with command properties manually
