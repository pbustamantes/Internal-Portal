# Deployment Guide

## Table of Contents

- [Prerequisites](#prerequisites)
- [Environment Variables](#environment-variables)
- [Docker Compose (Development)](#docker-compose-development)
- [Docker Production Deployment](#docker-production-deployment)
- [Manual Deployment](#manual-deployment)
- [Database](#database)
- [Frontend Deployment](#frontend-deployment)
- [Reverse Proxy](#reverse-proxy)
- [Security Checklist](#security-checklist)
- [Troubleshooting](#troubleshooting)

---

## Prerequisites

| Dependency | Version | Purpose |
|---|---|---|
| .NET SDK | 8.0+ | Build and run the API |
| Node.js | 18+ | Build the frontend |
| Docker | 20+ | Containerized deployment |
| SQL Server | 2022 | Database (or use the Docker container) |

---

## Environment Variables

### Backend (API)

Configuration is loaded from `appsettings.json` and can be overridden via environment variables using the `__` (double underscore) separator.

| Variable | Default | Description |
|---|---|---|
| `ASPNETCORE_ENVIRONMENT` | `Production` | `Development` enables Swagger and seed data |
| `ConnectionStrings__DefaultConnection` | *(see below)* | SQL Server connection string |
| `Jwt__Secret` | `SuperSecretKeyThatIsAtLeast32CharactersLong!` | JWT signing key (change in production) |
| `Jwt__Issuer` | `InternalPortal` | JWT issuer claim |
| `Jwt__Audience` | `InternalPortalUsers` | JWT audience claim |
| `Jwt__ExpiryHours` | `1` | Access token lifetime in hours |

**Default connection string:**

```
Server=localhost,1433;Database=InternalPortalDb;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=True
```

### Frontend

| Variable | Default | Description |
|---|---|---|
| `NEXT_PUBLIC_API_URL` | `http://localhost:5001` | Backend API base URL |

Set this at build time or in a `.env.local` file:

```bash
# src/Frontend/internal-portal-web/.env.local
NEXT_PUBLIC_API_URL=https://api.yourcompany.com
```

---

## Docker Compose (Development)

The quickest way to run the full stack locally:

```bash
docker compose up
```

This starts:

| Service | Port | Description |
|---|---|---|
| `sqlserver` | 1433 | SQL Server 2022 with health checks |
| `api` | 5001 | ASP.NET Core API (waits for SQL Server) |
| `web` | 3000 | Next.js frontend (waits for API) |

The API automatically runs migrations and seeds sample data on first launch. The frontend is built with `NEXT_PUBLIC_API_URL=http://localhost:5001` so browser API calls go to the exposed API port.

### Docker Compose Services

```yaml
# Abbreviated — see docker-compose.yml for full config
services:
  sqlserver:
    image: mcr.microsoft.com/mssql/server:2022-latest
    ports: ["1433:1433"]
    environment:
      SA_PASSWORD: "YourStrong@Passw0rd"
      ACCEPT_EULA: "Y"
    volumes:
      - sqlserver-data:/var/opt/mssql  # Persistent database storage
    healthcheck:
      test: /opt/mssql-tools18/bin/sqlcmd ...
      interval: 10s, timeout: 5s, retries: 5

  api:
    build: .
    ports: ["5001:8080"]
    depends_on:
      sqlserver:
        condition: service_healthy
    environment:
      ASPNETCORE_ENVIRONMENT: Development
      ConnectionStrings__DefaultConnection: "Server=sqlserver;..."
    volumes:
      - uploads:/app/uploads  # Persistent profile picture storage

  web:
    build:
      context: src/Frontend/internal-portal-web
      args:
        NEXT_PUBLIC_API_URL: http://localhost:5001
    ports: ["3000:3000"]
    depends_on: [api]
```

### Useful Commands

```bash
# Run in background
docker compose up -d

# Rebuild after code changes
docker compose up --build

# View logs
docker compose logs -f api

# Stop and remove containers
docker compose down

# Stop and remove containers + volumes (resets database)
docker compose down -v
```

---

## Docker Production Deployment

### 1. Build the API Image

The included multi-stage `Dockerfile` produces an optimized production image:

```bash
docker build -t internal-portal-api:latest .
```

**Dockerfile stages:**
1. **Build** — Restores NuGet packages, publishes Release build
2. **Runtime** — ASP.NET 8.0 runtime only, creates `/app/uploads` directory, exposes port 8080

### 2. Run the API Container

```bash
docker run -d \
  --name internal-portal-api \
  -p 5001:8080 \
  -e ASPNETCORE_ENVIRONMENT=Production \
  -e "ConnectionStrings__DefaultConnection=Server=your-sql-server;Database=InternalPortalDb;User Id=sa;Password=YOUR_PROD_PASSWORD;TrustServerCertificate=True" \
  -e "Jwt__Secret=YourProductionSecretKeyThatIsAtLeast32Characters!" \
  -v portal-uploads:/app/uploads \
  internal-portal-api:latest
```

### 3. Build the Frontend Image

Create a production `Dockerfile` for the frontend:

```dockerfile
FROM node:18-alpine AS builder
WORKDIR /app
COPY package*.json ./
RUN npm ci
COPY . .
ENV NEXT_PUBLIC_API_URL=https://api.yourcompany.com
RUN npm run build

FROM node:18-alpine
WORKDIR /app
COPY --from=builder /app/.next/standalone ./
COPY --from=builder /app/.next/static ./.next/static
COPY --from=builder /app/public ./public
EXPOSE 3000
CMD ["node", "server.js"]
```

The Next.js config already sets `output: "standalone"` for optimized Docker builds.

```bash
cd src/Frontend/internal-portal-web
docker build -t internal-portal-web:latest .
docker run -d --name internal-portal-web -p 3000:3000 internal-portal-web:latest
```

---

## Manual Deployment

### Backend

```bash
# 1. Publish a release build
dotnet publish src/Presentation/InternalPortal.API \
  -c Release \
  -o ./publish

# 2. Set environment variables
export ASPNETCORE_ENVIRONMENT=Production
export ConnectionStrings__DefaultConnection="Server=...;Database=InternalPortalDb;..."
export Jwt__Secret="YourProductionSecretKey..."

# 3. Run
cd publish
dotnet InternalPortal.API.dll
```

The API listens on port 8080 by default. Use a reverse proxy (Nginx, Caddy) to terminate TLS on port 443.

### Frontend

```bash
cd src/Frontend/internal-portal-web

# 1. Install dependencies
npm ci

# 2. Set the API URL
export NEXT_PUBLIC_API_URL=https://api.yourcompany.com

# 3. Build
npm run build

# 4. Start (standalone mode)
node .next/standalone/server.js
```

---

## Database

### Migrations

EF Core migrations are included in `src/Infrastructure/InternalPortal.Persistence/Migrations/`.

**Apply migrations manually:**

```bash
dotnet ef database update \
  --project src/Infrastructure/InternalPortal.Persistence \
  --startup-project src/Presentation/InternalPortal.API
```

**In Development mode**, the API runs `MigrateAsync()` automatically on startup via `SeedData.InitializeAsync`.

**In Production**, migrations should be applied explicitly before deploying a new version:

```bash
dotnet ef database update \
  --project src/Infrastructure/InternalPortal.Persistence \
  --startup-project src/Presentation/InternalPortal.API \
  --connection "Server=prod-server;Database=InternalPortalDb;..."
```

### Seed Data

Seed data only runs when `ASPNETCORE_ENVIRONMENT=Development`. It creates:

- 5 event categories (Workshop, Social, Training, Meeting, Conference)
- 3 venues (Main Auditorium, Conference Room A, Training Lab)
- 3 users (admin, organizer, employee)

The seed is idempotent — it checks `if (await context.Users.AnyAsync()) return;` and skips if data exists.

For production, create users via the `/api/auth/register` endpoint or seed manually.

### Backups

For SQL Server running in Docker:

```bash
# Backup
docker exec internal-portal-db /opt/mssql-tools18/bin/sqlcmd \
  -S localhost -U sa -P "YourStrong@Passw0rd" -C \
  -Q "BACKUP DATABASE InternalPortalDb TO DISK='/var/opt/mssql/backup/portal.bak'"

# Restore
docker exec internal-portal-db /opt/mssql-tools18/bin/sqlcmd \
  -S localhost -U sa -P "YourStrong@Passw0rd" -C \
  -Q "RESTORE DATABASE InternalPortalDb FROM DISK='/var/opt/mssql/backup/portal.bak'"
```

---

## Frontend Deployment

### Static Export vs Standalone Server

The frontend uses `output: "standalone"` in `next.config.ts`, which produces a self-contained Node.js server. This is the recommended approach for Docker deployments.

### Build and Preview

```bash
cd src/Frontend/internal-portal-web
npm run build    # Production build
npm run start    # Start production server on port 3000
```

### Environment at Build Time

`NEXT_PUBLIC_API_URL` is embedded at build time (it's a `NEXT_PUBLIC_` variable). You must set it before running `npm run build`:

```bash
NEXT_PUBLIC_API_URL=https://api.yourcompany.com npm run build
```

---

## Reverse Proxy

### Nginx Example

```nginx
server {
    listen 443 ssl;
    server_name portal.yourcompany.com;

    ssl_certificate     /etc/ssl/certs/portal.crt;
    ssl_certificate_key /etc/ssl/private/portal.key;

    # Frontend
    location / {
        proxy_pass http://localhost:3000;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
    }

    # API
    location /api/ {
        proxy_pass http://localhost:5001;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
    }

    # SignalR WebSocket
    location /hubs/ {
        proxy_pass http://localhost:5001;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection "upgrade";
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
    }

    # Uploaded files (profile pictures)
    location /uploads/ {
        proxy_pass http://localhost:5001;
    }
}
```

Key points:
- The `/hubs/` location requires WebSocket upgrade headers for SignalR
- The `/uploads/` location proxies to the API which serves static files from the uploads directory
- CORS is configured in the API to allow the frontend origin — update it when deploying behind a reverse proxy

### CORS Configuration

The API currently allows `http://localhost:3000`. For production, update `Program.cs` or use environment-based configuration:

```csharp
// In Program.cs — CORS policy
options.AddPolicy("AllowFrontend", builder =>
    builder.WithOrigins("https://portal.yourcompany.com")
           .AllowAnyMethod()
           .AllowAnyHeader()
           .AllowCredentials());
```

---

## Security Checklist

Before deploying to production:

- [ ] **Change JWT secret** — Use a strong, random 256-bit key. Never use the default
- [ ] **Change SA password** — Replace `YourStrong@Passw0rd` with a strong password
- [ ] **Set `ASPNETCORE_ENVIRONMENT=Production`** — Disables Swagger UI and seed data
- [ ] **Enable HTTPS** — Terminate TLS at the reverse proxy or configure Kestrel
- [ ] **Update CORS origins** — Replace `localhost:3000` with the actual frontend domain
- [ ] **Restrict database access** — Use a dedicated SQL user instead of `sa`
- [ ] **Secure uploads directory** — Validate file types (already done: .jpg, .jpeg, .png, .gif, .webp) and scan for malware
- [ ] **Set token expiry** — Default is 1 hour for access tokens, 7 days for refresh tokens
- [ ] **Enable logging** — Configure structured logging (Serilog, Application Insights, etc.)
- [ ] **Rate limiting** — Add rate limiting middleware for auth endpoints
- [ ] **Remove seed data credentials** — Don't deploy with default user passwords

---

## Troubleshooting

### API won't start — "Connection refused" to SQL Server

The API depends on SQL Server being healthy. If using Docker Compose, ensure the health check passes:

```bash
docker compose logs sqlserver
```

If running locally, verify SQL Server is accessible on port 1433:

```bash
docker exec internal-portal-db /opt/mssql-tools18/bin/sqlcmd \
  -S localhost -U sa -P "YourStrong@Passw0rd" -C \
  -Q "SELECT 1"
```

### Migrations fail

Ensure the connection string is correct and the database server is running:

```bash
dotnet ef database update \
  --project src/Infrastructure/InternalPortal.Persistence \
  --startup-project src/Presentation/InternalPortal.API \
  --verbose
```

### SignalR connection fails

- Verify the WebSocket upgrade headers are being passed by the reverse proxy
- Check that the JWT token is valid and being passed as `?access_token=` query parameter
- Ensure CORS allows the frontend origin with credentials

### Frontend shows "Network Error"

- Verify `NEXT_PUBLIC_API_URL` was set correctly at build time
- Check browser console for CORS errors
- Ensure the API is running and accessible from the browser

### Profile pictures not loading

- Verify the `/uploads` directory exists and is writable
- Check that the reverse proxy forwards `/uploads/` to the API
- In Docker, ensure the uploads volume is mounted correctly
