# ADR-011: Docker with Multi-Stage Builds

**Status:** Accepted
**Date:** 2025-01

## Context

The application needs a reproducible, portable deployment strategy that works consistently across development machines and production environments. Developers should be able to run the full stack (API + database) with a single command.

## Decision

Use **Docker** with a **multi-stage Dockerfile** and **Docker Compose** for orchestration:

### Dockerfile (API)
- **Build stage** — .NET 8 SDK image, restores NuGet packages, publishes a Release build
- **Runtime stage** — ASP.NET 8 runtime image (smaller, no SDK), copies published output, creates `/app/uploads` directory, exposes port 8080

### Docker Compose
- **sqlserver** — SQL Server 2022 with health checks (sqlcmd ping every 10s, 10 retries, 30s start period)
- **api** — Depends on sqlserver with `condition: service_healthy` to prevent startup before database is ready
- **web** — Next.js frontend, depends on api, built with `NEXT_PUBLIC_API_URL=http://localhost:5001` so browser API calls reach the exposed API port
- **Volumes** — `sqlserver-data` for database persistence, `api-uploads` for profile pictures
- **Environment** — `ASPNETCORE_ENVIRONMENT=Development` for local development with seed data

### Frontend Dockerfile
- Multi-stage build: **deps** (npm ci) → **build** (next build with `NEXT_PUBLIC_API_URL` build arg) → **runtime** (standalone Node.js server on port 3000)
- Next.js configured with `output: "standalone"` for optimized Docker builds (self-contained Node.js server without `node_modules`)

## Alternatives Considered

- **Kubernetes** — Overkill for a single-team internal portal; Docker Compose is sufficient
- **Azure App Service / AWS ECS** — Cloud-specific; Docker Compose is cloud-agnostic
- **No containers (direct hosting)** — Less reproducible, "works on my machine" problems
- **Single-stage Docker build** — Larger image (includes SDK), longer startup

## Consequences

**Benefits:**
- `docker compose up` runs the entire stack (database, API, frontend) in one command
- Health checks prevent race conditions between API and database startup
- Multi-stage build produces a minimal runtime image (~200MB vs ~800MB with SDK)
- Persistent volumes survive container restarts
- Same Dockerfile works for development and production

**Tradeoffs:**
- Docker required on developer machines
- SQL Server container uses ~2GB RAM
- Docker Compose doesn't provide orchestration features (scaling, rolling updates) — acceptable at current scale
