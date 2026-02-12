# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Build & Run Commands

### Full Stack (Docker)
```bash
docker compose up              # SQL Server (1433) + API (5001) + Frontend (3000)
docker compose up --build      # Rebuild after code changes
docker compose up -d sqlserver # Database only (for local dev)
```

### Backend (.NET 8)
```bash
dotnet build                   # Build all projects
dotnet test                    # Run all tests (Domain, Application, API, Infrastructure, Integration)
dotnet test tests/InternalPortal.Domain.Tests              # Run single test project
dotnet test --filter "FullyQualifiedName~UserTests"        # Run specific test class
dotnet run --project src/Presentation/InternalPortal.API   # Run API on port 5001
```

### Frontend (Next.js 15)
```bash
cd src/Frontend/internal-portal-web
npm install
npm run dev     # Dev server on port 3000
npm run build   # Production build
npm run lint    # ESLint
```

### EF Core Migrations
```bash
dotnet ef migrations add <Name> \
  --project src/Infrastructure/InternalPortal.Persistence \
  --startup-project src/Presentation/InternalPortal.API
dotnet ef database update \
  --project src/Infrastructure/InternalPortal.Persistence \
  --startup-project src/Presentation/InternalPortal.API
```

## Architecture

**Onion Architecture** with strict inward dependency direction:

```
Domain (innermost) → Application → Persistence/Infrastructure → API (outermost)
```

- **Domain** (`InternalPortal.Domain`) — Entities inheriting `BaseEntity` (Id, CreatedAtUtc, UpdatedAtUtc, DomainEvents collection), value objects (DateTimeRange, Capacity, Address), domain events inheriting `BaseDomainEvent`, custom exceptions. Uses `MediatR.Contracts` only (not full MediatR).
- **Application** (`InternalPortal.Application`) — CQRS via MediatR. Features organized as `Features/{BoundedContext}/Commands|Queries/`. Each command/query has a handler, validator (FluentValidation), and DTOs. Pipeline behaviors: ValidationBehavior → LoggingBehavior → PerformanceBehavior → UnhandledExceptionBehavior.
- **Persistence** (`InternalPortal.Persistence`) — EF Core `ApplicationDbContext` with Fluent API configurations in `/Configurations/`. Repository pattern with `IRepository<T>` base. `SaveChangesAsync` sets audit fields and dispatches domain events via MediatR.
- **Infrastructure** (`InternalPortal.Infrastructure`) — JWT auth (Bearer + refresh tokens), `ICurrentUserService`, SignalR `NotificationHub` at `/hubs/notifications`, email service. Requires `<FrameworkReference Include="Microsoft.AspNetCore.App" />`.
- **API** (`InternalPortal.API`) — Controllers, `ExceptionHandlingMiddleware` mapping domain exceptions to HTTP status codes, Swagger. DI wired via extension methods: `AddApplication()`, `AddPersistence()`, `AddInfrastructure()`.
- **Frontend** (`internal-portal-web`) — Next.js 15 App Router, all pages `'use client'`. Axios client with JWT interceptors (`lib/api.ts`), React Query hooks in `hooks/use-*.ts`, Zustand store for real-time notification state. `NEXT_PUBLIC_API_URL` baked at build time. Path alias: `@/*` → `./src/*`.

## Key Patterns

- **Entity factory methods**: `User.Create(...)`, `Event.Create(...)` — not public constructors
- **Domain events**: Added to entity's `DomainEvents` collection, dispatched after `SaveChangesAsync`
- **Validation**: FluentValidation validators per command, auto-registered via assembly scan, run in `ValidationBehavior` pipeline
- **Custom exceptions**: `NotFoundException`, `ForbiddenException`, `DomainException`, `ValidationException` — caught by `ExceptionHandlingMiddleware`
- **CORS**: Allows `http://localhost:3000` — configured in `Program.cs`
- **SignalR auth**: JWT passed via `?access_token=` query parameter

## Testing

- **xUnit** + **FluentAssertions** + **Moq**
- Tests require explicit `using Xunit;` (not in implicit usings)
- Integration tests use `WebApplicationFactory<Program>` with in-memory database
- Integration test factory sets environment to `"Testing"` to skip `SeedData` (which calls `MigrateAsync`)

## Seed Data (Development Mode)

Three users seeded on empty database: `admin@company.com` / `Admin123!` (Admin), `organizer@company.com` / `Organizer123!` (Organizer), `employee@company.com` / `Employee123!` (Employee).

## Known Build Constraints

- `Microsoft.Extensions.Logging.Abstractions` must be 8.0.2+ (not 8.0.1) to avoid NU1605 downgrade errors with EF Core 8.0.11
- Frontend: Tailwind CSS v4 uses `@import "tailwindcss"` (not `@tailwind` directives), PostCSS plugin is `@tailwindcss/postcss`
- Frontend: Next.js 15 dynamic params use `Promise<{ id: string }>` pattern with `use()` hook
