# Architecture Decision Records (ADRs)

This directory contains the Architecture Decision Records for the Internal Portal project. ADRs document significant architectural decisions, the context that led to them, and the consequences of each choice.

## Index

| ADR | Title | Status |
|-----|-------|--------|
| [001](001-onion-architecture.md) | Onion Architecture with DDD | Accepted |
| [002](002-cqrs-mediatr.md) | CQRS with MediatR | Accepted |
| [003](003-jwt-authentication.md) | JWT Authentication with Refresh Tokens | Accepted |
| [004](004-ef-core-sql-server.md) | Entity Framework Core with SQL Server | Accepted |
| [005](005-signalr-realtime.md) | SignalR for Real-Time Notifications | Accepted |
| [006](006-nextjs-frontend.md) | Next.js 15 with App Router | Accepted |
| [007](007-react-query-state.md) | React Query for Server State, Zustand for Client State | Accepted |
| [008](008-fluentvalidation.md) | FluentValidation over Data Annotations | Accepted |
| [009](009-domain-events.md) | Domain Events for Side Effects | Accepted |
| [010](010-error-handling.md) | Centralized Exception Handling | Accepted |
| [011](011-docker-deployment.md) | Docker with Multi-Stage Builds | Accepted |
| [012](012-tailwind-css.md) | Tailwind CSS v4 for Styling | Accepted |

## Format

Each ADR follows this structure:

- **Status** — Accepted, Superseded, or Deprecated
- **Date** — When the decision was made
- **Context** — The problem or requirement that prompted the decision
- **Decision** — What was decided and how it's implemented
- **Alternatives Considered** — Other options that were evaluated
- **Consequences** — Benefits and tradeoffs of the decision
