# ADR-004: Entity Framework Core with SQL Server

**Status:** Accepted
**Date:** 2025-01

## Context

The application needs a relational database for structured data (users, events, registrations) with referential integrity, transactions, and complex queries for reporting.

## Decision

Use **Entity Framework Core 8.0.11** with **SQL Server 2022** as the database provider:

- **Code-first** approach with migrations stored in the Persistence project
- **Fluent API** configurations in dedicated `IEntityTypeConfiguration<T>` classes
- **Owned types** for value objects (Address, Capacity, DateTimeRange) — embedded in parent tables
- **Repository pattern** with a generic `RepositoryBase<T>` and specialized repositories (IEventRepository, etc.)
- **Unit of Work** — `IUnitOfWork` wraps `DbContext.SaveChangesAsync()`, registered as Scoped
- **Automatic audit fields** — `CreatedAtUtc` and `UpdatedAtUtc` set in `SaveChangesAsync()` override by inspecting `ChangeTracker` entries
- **Domain event dispatch** — After `base.SaveChangesAsync()`, domain events are extracted from tracked entities and published via MediatR
- **Enums stored as strings** — `HasConversion<string>()` with MaxLength for readability in the database

### Seed Data

`SeedData.InitializeAsync` runs only in Development mode, calls `MigrateAsync()` to apply pending migrations, and seeds sample data if no users exist (idempotent).

## Alternatives Considered

- **Dapper** — Better raw SQL performance but no change tracking, migrations, or owned types
- **PostgreSQL** — Excellent open-source option; SQL Server chosen for enterprise familiarity and Docker availability
- **SQLite** — Too limited for concurrent access and production use
- **NoSQL (MongoDB)** — Relational model better fits the structured event/registration domain

## Consequences

**Benefits:**
- Code-first migrations keep schema in sync with domain model
- Owned types map value objects naturally without separate tables
- Change tracker enables automatic audit field population
- Domain events dispatched after successful save ensures consistency
- Repository abstraction allows swapping to a different ORM/database

**Tradeoffs:**
- EF Core adds startup cost and memory overhead vs raw SQL
- InMemory provider used in tests doesn't perfectly match SQL Server behavior
- Domain event dispatch in `SaveChangesAsync()` couples the DbContext to MediatR
