# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Stack

.NET 10 · Aspire 13.6 · FastEndpoints 8 · EF Core (PostgreSQL) · Redis distributed cache · Next.js 16 frontend (pnpm). Toolchain versions are pinned in `mise.toml`; `global.json` is what the SDK resolver actually reads, so keep the two in sync (and pnpm in sync with `packageManager` in `src/micro-commerce/package.json`).

## Commands

`mise tasks ls` lists all shortcuts. The underlying commands:

```bash
aspire run                                    # whole app: Postgres, Redis, Keycloak, API, frontend + Aspire dashboard
dotnet build
dotnet format --verify-no-changes             # .NET lint (mise run lint also runs the frontend lint)
pnpm --dir src/micro-commerce lint            # frontend lint

dotnet test --solution MicroCommerce.slnx     # all tests
dotnet test --project tests/MicroCommerce.UnitTests
dotnet test --project tests/MicroCommerce.UnitTests --filter-method "*Negative_price_fails"
dotnet test --project tests/MicroCommerce.FunctionalTests --filter-class "*ProductEndpointsTests"

dotnet ef migrations add <Name> -p src/MicroCommerce.ApiService -c AppDbContext -o Data/Migrations   # needs dotnet tool restore first
dotnet ef migrations add <Name> -p src/MicroCommerce.ApiService -c AccountsDbContext -o Features/Accounts/Data/Migrations
```

Tests are xUnit v3 on **Microsoft.Testing.Platform in native mode** (set in `global.json`): use `--project`/`--solution` (no positional paths), and xUnit v3 filters (`--filter-class`, `--filter-method`, `--filter-trait`, `--filter-query`) passed directly — not VSTest `--filter` and no `--` separator. Functional and integration tests need a running container runtime.

`TreatWarningsAsErrors` is on for every project (`Directory.Build.props`). Package versions live only in `Directory.Packages.props` (central package management) — `PackageReference` items have no `Version`.

## Architecture

- **`MicroCommerce.AppHost`** (Aspire) wires everything. Resource names here are contracts used elsewhere: the connection names `micro-commerce-db` and `redis` must match `Program.cs` (`AddNpgsqlDbContext` / `AddRedisDistributedCache`) and `ApiFixture`; the resource names `apiservice` and `keycloak` are what integration tests wait on and create clients for. The frontend gets the API address via the `API_URL` env var. Passing `--EphemeralResources=true` skips data volumes, admin UIs, persistent container lifetimes and the fixed ports.
- **Identity (ADR-0002)**: Keycloak imports the `micro-commerce` realm from `src/MicroCommerce.AppHost/Realms/` on start: clients `micro-commerce-frontend` (sign-in, redirect URI `http://localhost:3000/*`), `micro-commerce-api` (the token audience) and `micro-commerce-tests` (password grant, integration tests only); the `platform-operator` realm role, emitted as a flat `roles` claim; seeded users `buyer`, `merchant`, `operator` (password = username). Keycloak runs on port 8080 and the frontend on 3000 so issuer and redirect URIs stay stable. A persistent Keycloak volume skips re-importing an existing realm, so after editing the realm file delete the `keycloak` volume.
- **`MicroCommerce.ServiceDefaults`**: standard Aspire `AddServiceDefaults()` (OpenTelemetry, service discovery, HTTP resilience, `/health` + `/alive` — mapped only in Development).
- **`MicroCommerce.ApiService`** is organized as vertical slices under `Features/<Area>/`:
  - One file per endpoint holding its request record, FastEndpoints `Validator<T>` (FluentValidation), and `Endpoint` class (e.g. `Features/Products/CreateProduct.cs`). Endpoints are discovered automatically by `AddFastEndpoints()`; they inject `AppDbContext` / `IDistributedCache` via primary constructors and reply with `Send.*Async`.
  - The entity file holds the entity, its `IEntityTypeConfiguration<T>` (picked up by `ApplyConfigurationsFromAssembly`), and the response record with a static `From` mapper (`Features/Products/Product.cs`). New entities also need a `DbSet` on their module's context and a migration.
  - Errors use ProblemDetails (`AddProblemDetails` + `c.Errors.UseProblemDetails()`); validation error names come back camelCased.
  - Modules own their tables (ADR-0003). Products still use `AppDbContext` (`public` schema); the Accounts module has `AccountsDbContext` with its own `accounts` schema and migration history. Each context applies only its own feature namespace's entity configurations, so a new module gets its own context, schema and migrations.
  - Auth: JWT bearer with `MapInboundClaims = false` (claims keep their token names: `sub`, `email`, `name`, `roles`). The authority comes from `Authentication:Schemes:Bearer:Authority`, which the AppHost sets. Endpoints require a token unless they call `AllowAnonymous()`. `CurrentAccount.GetOrCreateAsync(User)` maps the token subject to an Account, creating it on first use. `AuthPolicies.PlatformOperator` is the operator policy.
  - In Development the API runs `Database.MigrateAsync()` on startup and serves Scalar at `/scalar/v1` and OpenAPI at `/openapi/v1.json`. `AppDbContextFactory` / `AccountsDbContextFactory` exist only for `dotnet ef` design-time.
- **`src/micro-commerce`** — Next.js 16 / React 19 with the React Compiler and Tailwind 4. `output: "standalone"` in `next.config.ts` is required by Aspire's `AddNextJsApp`. Sign-in uses Auth.js (`next-auth` v5, `src/auth.ts`) with the Keycloak provider. `src/proxy.ts` forwards `/api/*` to the API with the session's access token, except Auth.js's own `/api/auth/*`. Server code calls the API with `session.accessToken`. Its `AGENTS.md` applies: this Next.js version has breaking changes vs. training data, so read the relevant guide in `src/micro-commerce/node_modules/next/dist/docs/` before writing frontend code. `next dev` re-adds that AGENTS.md block — commit it rather than reverting it.

## Test layers

- **UnitTests** — pure tests of validators/domain types; reference the API project directly.
- **FunctionalTests** — API in-process via `FastEndpoints.Testing` (`ApiFixture : AppFixture<Program>`) against Testcontainers Postgres and Redis. Tests derive from `TestBase<ApiFixture>` and call endpoints with typed helpers (`App.Client.POSTAsync<TEndpoint, TRequest, TResponse>`). JWT validation trusts a test signing key instead of Keycloak: `App.ClientFor(TestActor.Buyer())` / `TestActor.PlatformOperator()` gives a client for a fresh subject, and `TestIdentity.TokenFor(...)` mints custom (expired, wrongly signed, other-audience) tokens. `App.Client` is anonymous. The app and containers are shared across the run, so tests must not assume an empty database.
- **IntegrationTests** — boot the full AppHost with `Aspire.Hosting.Testing` (assembly-level `AppHostFixture`, ephemeral resources) and hit the API over plain `HttpClient`. For real tokens, use the password grant against `KeycloakClient` with the `micro-commerce-tests` client and a seeded user.

`tests/Directory.Build.props` makes every test project an `Exe` with xUnit v3 + Shouldly and global `using Xunit; using Shouldly;`. Assertions use Shouldly.

## Agent skills

### Issue tracker

Issues live in GitHub Issues on `baotoq/micro-commerce` (via `gh`). See `docs/agents/issue-tracker.md`.

### Triage labels

Default vocabulary: `needs-triage`, `needs-info`, `ready-for-agent`, `ready-for-human`, `wontfix`. See `docs/agents/triage-labels.md`.

### Domain docs

Single-context: one root `CONTEXT.md` + `docs/adr/` (created lazily). See `docs/agents/domain.md`.
