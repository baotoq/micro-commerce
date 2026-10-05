# MicroCommerce

.NET 10 · Aspire 13.6 · FastEndpoints · EF Core (PostgreSQL) · Redis

## Layout

```
src/
  MicroCommerce.AppHost/          Aspire orchestrator (Postgres + pgAdmin, Redis + RedisInsight, API)
  MicroCommerce.ServiceDefaults/  OpenTelemetry, health checks, service discovery, resilience
  MicroCommerce.ApiService/       FastEndpoints API, EF Core DbContext, Redis distributed cache
  micro-commerce/                 Next.js frontend (pnpm), started by the AppHost with API_URL set
tests/
  MicroCommerce.UnitTests/        Pure unit tests (validators, domain)
  MicroCommerce.FunctionalTests/  In-process API via FastEndpoints.Testing + Testcontainers (Postgres, Redis)
  MicroCommerce.IntegrationTests/ Full AppHost via Aspire.Hosting.Testing
src/micro-commerce/e2e/          Playwright end-to-end tests through the browser (Keycloak sign-in, /api proxy)
```

Package versions are managed centrally in `Directory.Packages.props`.

## Setup

The toolchain (.NET SDK, Aspire CLI, Node, pnpm) is pinned in `mise.toml`. With [mise](https://mise.jdx.dev) and a container runtime (Docker or Podman) installed:

```bash
mise trust && mise install
dotnet tool restore
dotnet restore
pnpm --dir src/micro-commerce install
```

Without mise, install the .NET 10 SDK, the Aspire CLI, Node 24 and pnpm yourself.

## Run

```bash
aspire run
```

The API applies EF Core migrations on startup in Development. Once it's running, the API reference is at `/scalar/v1` and the OpenAPI document is at `/openapi/v1.json`.

## Tests

xUnit v3 on Microsoft.Testing.Platform (selected in `global.json`). Functional and integration tests need a running container runtime.

```bash
dotnet test --solution MicroCommerce.slnx
dotnet test --project tests/MicroCommerce.UnitTests
```

Integration tests start the AppHost with `--EphemeralResources=true`, so they get throwaway containers and never touch your dev data volumes.

The Playwright end-to-end suite (Chromium) runs through the browser against the whole app. It starts `aspire run`, or reuses one that's already running, and runs against your dev data:

```bash
pnpm --dir src/micro-commerce exec playwright install chromium   # once
pnpm --dir src/micro-commerce test:e2e
```

## Migrations

```bash
dotnet tool restore
dotnet ef migrations add <Name> -p src/MicroCommerce.ApiService -o Data/Migrations
```
