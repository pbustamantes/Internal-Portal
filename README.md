# Internal Portal - Event Management Application

A full-stack internal company event management portal built with ASP.NET Core 8 and Next.js 15. Features include event CRUD, user registration/RSVP, calendar view, real-time notifications via SignalR, attendee management, and reporting.

## Tech Stack

**Backend:** C# / ASP.NET Core 8, Entity Framework Core, MediatR (CQRS), FluentValidation, SignalR, JWT Authentication, SQL Server

**Frontend:** Next.js 15 (App Router), TypeScript, React 19, Tailwind CSS v4, TanStack React Query, Zustand, Axios, FullCalendar

**Testing:** xUnit, FluentAssertions, Moq, WebApplicationFactory

## Architecture

The backend follows **Onion Architecture** with strict dependency direction (inner layers never reference outer):

```
src/
├── Core/
│   ├── InternalPortal.Domain/          # Entities, value objects, domain events, interfaces
│   └── InternalPortal.Application/     # CQRS commands/queries, DTOs, validation, pipeline behaviors
│
├── Infrastructure/
│   ├── InternalPortal.Persistence/     # EF Core DbContext, configurations, repositories
│   └── InternalPortal.Infrastructure/  # JWT, identity, SignalR hub, email, notifications
│
├── Presentation/
│   └── InternalPortal.API/             # Controllers, middleware, Program.cs
│
└── Frontend/
    └── internal-portal-web/            # Next.js 15 app
```

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Node.js 18+](https://nodejs.org/) and npm
- [Docker](https://www.docker.com/) (for SQL Server) — or a local SQL Server instance

## Getting Started

### Option A: Docker Compose (Recommended)

The quickest way to run the full stack:

```bash
docker compose up
```

This builds and starts all four services:
- **SQL Server** on port `1433`
- **API** on port `5001` (waits for SQL Server to be healthy before starting)
- **Frontend** on port `3000` (waits for API)
- **Mailpit** — SMTP on port `1025`, web UI on port `8025`

The API automatically runs migrations and seeds sample data on first launch. The frontend is built with `NEXT_PUBLIC_API_URL=http://localhost:5001` so browser API calls reach the exposed API port.

**App:** [http://localhost:3000](http://localhost:3000)
**Swagger UI:** [http://localhost:5001/swagger](http://localhost:5001/swagger)
**Mailpit UI:** [http://localhost:8025](http://localhost:8025)

To run in the background:

```bash
docker compose up -d
```

To rebuild after code changes:

```bash
docker compose up --build
```

### Option B: Local Development

#### 1. Start the Database

```bash
docker compose up -d sqlserver
```

This starts a SQL Server 2022 container on port `1433` with:
- **SA Password:** `YourStrong@Passw0rd`
- **Database:** `InternalPortalDb` (created automatically on first run)

#### 2. Run Database Migrations

```bash
# Create the migration (already included in the repo under Persistence/Migrations)
dotnet ef migrations add InitialCreate \
  --project src/Infrastructure/InternalPortal.Persistence \
  --startup-project src/Presentation/InternalPortal.API \
  --output-dir Migrations

# Apply the migration to create all tables
dotnet ef database update \
  --project src/Infrastructure/InternalPortal.Persistence \
  --startup-project src/Presentation/InternalPortal.API
```

This creates the following tables: `Users`, `Events`, `Registrations`, `Notifications`, `RefreshTokens`, `EventCategories`, and `Venues`.

> **Note:** If you skip this step, the API will automatically run migrations and seed data on first launch in Development mode (see step 3).

#### 3. Run the Backend API

```bash
dotnet run --project src/Presentation/InternalPortal.API
```

The API starts on `http://localhost:5001` (or the default Kestrel ports).

**Swagger UI:** [http://localhost:5001/swagger](http://localhost:5001/swagger)

#### 4. Run the Frontend

```bash
cd src/Frontend/internal-portal-web
npm install
npm run dev
```

The frontend starts on [http://localhost:3000](http://localhost:3000).

### Run Tests

```bash
dotnet test
```

Runs 55 tests across 4 test projects (Domain, Application, API, Integration).

## Seed Data

When the API starts in **Development** mode, `SeedData.InitializeAsync` runs automatically. It calls `MigrateAsync()` (applying any pending migrations) and then populates the database with sample data if no users exist yet.

### What Gets Seeded

**5 Event Categories:**

| Name | Color | Description |
|------|-------|-------------|
| Workshop | Blue | Hands-on learning sessions |
| Social | Green | Team building and social events |
| Training | Amber | Professional development |
| Meeting | Indigo | Company meetings |
| Conference | Pink | Internal conferences |

**3 Venues:**

| Name | Location | Capacity |
|------|----------|----------|
| Main Auditorium | 100 Main St, Austin, TX — HQ, Auditorium | 500 |
| Conference Room A | 100 Main St, Austin, TX — HQ, Room A | 30 |
| Training Lab | 100 Main St, Austin, TX — HQ, Lab 1 | 20 |

**3 Users:**

| Email | Role | Password |
|-------|------|----------|
| `admin@company.com` | Admin | `Admin123!` |
| `organizer@company.com` | Organizer | `Organizer123!` |
| `employee@company.com` | Employee | `Employee123!` |

### Re-Seeding

The seed logic checks `if (await context.Users.AnyAsync()) return;`, so it only runs on an empty database. To re-seed:

```bash
# Drop and recreate the database
dotnet ef database drop \
  --project src/Infrastructure/InternalPortal.Persistence \
  --startup-project src/Presentation/InternalPortal.API \
  --force

dotnet ef database update \
  --project src/Infrastructure/InternalPortal.Persistence \
  --startup-project src/Presentation/InternalPortal.API

# Then start the API — seed data will be inserted automatically
dotnet run --project src/Presentation/InternalPortal.API
```

> **Note:** The seed passwords use pre-computed BCrypt hashes. You can also register new users through the `/api/auth/register` endpoint or the frontend registration page.

## API Endpoints

### Auth
| Method | Route | Auth | Description |
|--------|-------|------|-------------|
| POST | `/api/auth/register` | No | Register a new user |
| POST | `/api/auth/login` | No | Login, returns JWT + refresh token |
| POST | `/api/auth/refresh` | No | Refresh access token (token rotation) |
| POST | `/api/auth/revoke` | Yes | Revoke refresh token (logout) |
| POST | `/api/auth/forgot-password` | No | Send password reset email |
| POST | `/api/auth/reset-password` | No | Reset password with token |
| POST | `/api/auth/change-password` | Yes | Change password |

### Events
| Method | Route | Auth | Description |
|--------|-------|------|-------------|
| GET | `/api/events` | Yes | List events (paginated, searchable) |
| GET | `/api/events/{id}` | Yes | Get event details |
| POST | `/api/events` | Yes | Create event (Organizer/Admin) |
| PUT | `/api/events/{id}` | Yes | Update event (Owner/Admin) |
| DELETE | `/api/events/{id}` | Yes | Delete event (Owner/Admin) |
| POST | `/api/events/{id}/publish` | Yes | Publish draft event |
| POST | `/api/events/{id}/cancel` | Yes | Cancel event |
| GET | `/api/events/{id}/attendees` | Yes | List event attendees |
| GET | `/api/events/upcoming` | Yes | Get upcoming events |
| POST | `/api/events/{eventId}/register` | Yes | Register (RSVP) for event |
| DELETE | `/api/events/{eventId}/register` | Yes | Cancel registration |

### Calendar
| Method | Route | Auth | Description |
|--------|-------|------|-------------|
| GET | `/api/calendar?start=&end=` | Yes | Events in date range |

### Users
| Method | Route | Auth | Description |
|--------|-------|------|-------------|
| GET | `/api/users/me` | Yes | Current user profile |
| PUT | `/api/users/me` | Yes | Update profile |
| GET | `/api/users/me/events` | Yes | My registrations |

### Notifications
| Method | Route | Auth | Description |
|--------|-------|------|-------------|
| GET | `/api/notifications` | Yes | User notifications |
| PUT | `/api/notifications/{id}/read` | Yes | Mark notification as read |

### Reports (Admin only)
| Method | Route | Auth | Description |
|--------|-------|------|-------------|
| GET | `/api/reports/attendance` | Admin | Event attendance report |
| GET | `/api/reports/monthly` | Admin | Monthly events summary |
| GET | `/api/reports/popular` | Admin | Most popular events |

## Frontend Pages

| Route | Description |
|-------|-------------|
| `/login`, `/register` | Authentication (unauthenticated layout) |
| `/dashboard` | Upcoming events, stats, recent notifications |
| `/events` | Event listing with search and pagination |
| `/events/[id]` | Event detail with RSVP and attendee list |
| `/events/create` | Create new event form |
| `/events/[id]/edit` | Edit event form |
| `/calendar` | Monthly calendar view |
| `/my-events` | User's registered events |
| `/notifications` | Notification center |
| `/profile` | User profile editor |
| `/admin/events` | Admin event management |
| `/admin/users` | Admin user management |
| `/admin/reports` | Attendance and popularity reports |

## Email (Mailpit)

The application sends real emails (e.g. password reset links) via SMTP. In development, [Mailpit](https://mailpit.axllent.org/) acts as a local catch-all mail server — it accepts all outgoing emails and lets you view them in a web UI. No emails ever leave your machine.

### Viewing Emails

1. Start the stack with `docker compose up`
2. Trigger an email — for example, use the "Forgot Password" flow on the login page
3. Open **[http://localhost:8025](http://localhost:8025)** in your browser
4. All emails sent by the API appear in the Mailpit inbox, where you can view the full HTML content, click links (e.g. password reset), and inspect headers

### Running Mailpit Standalone

If you're running the API locally (not in Docker), you can start just Mailpit:

```bash
docker compose up -d mailpit
```

The API defaults to `localhost:1025` for SMTP (configured in `appsettings.json` under `Smtp`), so emails will be captured by Mailpit automatically.

### SMTP Configuration

| Setting | Default | Description |
|---------|---------|-------------|
| `Smtp:Host` | `localhost` | SMTP server hostname (`mailpit` in Docker Compose) |
| `Smtp:Port` | `1025` | SMTP server port |
| `Smtp:From` | `noreply@internalportal.local` | Sender address for outgoing emails |

These can be overridden via environment variables (e.g. `Smtp__Host=mailpit`).

## Real-Time Notifications

The app uses **SignalR** for real-time push notifications. The hub is at `/hubs/notifications` and requires JWT authentication (passed via query string). Events like new event published, event cancelled, and registration updates trigger notifications to relevant users.

## Configuration

Key settings in `src/Presentation/InternalPortal.API/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost,1433;Database=InternalPortalDb;..."
  },
  "Jwt": {
    "Secret": "SuperSecretKeyThatIsAtLeast32CharactersLong!",
    "Issuer": "InternalPortal",
    "Audience": "InternalPortalUsers",
    "ExpiryHours": 1
  },
  "Smtp": {
    "Host": "localhost",
    "Port": 1025,
    "From": "noreply@internalportal.local"
  }
}
```

The frontend API URL defaults to `http://localhost:5001` and can be overridden with the `NEXT_PUBLIC_API_URL` environment variable.

## Documentation

Detailed technical documentation is available in the [`docs/`](docs/) directory:

| Document | Description |
|----------|-------------|
| [API Reference](docs/api-reference.md) | Complete reference for all 28 REST endpoints — request/response schemas, validation rules, auth requirements, error handling, SignalR hub, and enums |
| [Database Diagram](docs/database-diagram.md) | ER diagram with all 7 tables, column types, constraints, foreign keys, and a relationship flowchart showing delete behaviors |
| [Class Diagrams](docs/class-diagrams.md) | 13 Mermaid class diagrams covering entities, value objects, repository interfaces, CQRS handlers, DTOs, persistence, infrastructure services, controllers, and architecture overview |
| [Sequence Diagrams](docs/sequence-diagrams.md) | 12 sequence diagrams tracing the main user flows — auth, event lifecycle (create → publish → register → complete), cancellation, error handling, and SignalR notifications |
| [Deployment Guide](docs/deployment-guide.md) | Docker Compose, production Docker builds, manual deployment, database migrations, Nginx reverse proxy config, CORS, security checklist, and troubleshooting |
| [Frontend Architecture](docs/frontend-architecture.md) | Next.js app structure, provider hierarchy, auth flow, API client with interceptors, React Query hooks, Zustand store, SignalR integration, component library, and routing |
| [Architecture Decision Records](docs/adrs/) | 12 ADRs documenting key decisions — Onion Architecture, CQRS/MediatR, JWT auth, EF Core, SignalR, Next.js, React Query, FluentValidation, domain events, error handling, Docker, Tailwind CSS |

All diagrams use [Mermaid](https://mermaid.js.org/) syntax and render natively on GitHub.

## Project Stats

- **153** C# source files across 5 backend projects
- **28** TypeScript/React components across 16 routes
- **55** automated tests (33 Domain + 14 Application + 2 API + 6 Integration)
- **7** domain entities with full EF Core configuration
- **24** REST API endpoints across 6 controllers
