# CLAUDE.md

## Project Overview

MicroCommerce is a showcase e-commerce platform demonstrating modern .NET microservices architecture. Users can browse products, add to cart, and checkout through a Next.js storefront. The backend is a modular monolith designed for gradual extraction to microservices.

## Tech Stack

- **Backend:** .NET 10, ASP.NET Core Minimal APIs, .NET Aspire 13.1.0
- **Gateway:** YARP reverse proxy (CORS, rate limiting, auth, routing)
- **Frontend:** Next.js 16, React 19, TypeScript 5, Tailwind CSS v4, TanStack React Query v5, Radix UI
- **Auth:** Keycloak (backend JWT + NextAuth.js v5 frontend)
- **Database:** PostgreSQL (schema-per-feature, shared database `appdb`)
- **Messaging:** Azure Service Bus (MassTransit 9.0) with emulator for dev
- **Patterns:** CQRS (MediatR), DDD, Event-Driven, Vertical Slice Architecture
- **Key Libraries:** Vogen (strongly typed IDs), Ardalis.SmartEnum, Ardalis.Specification, FluentValidation, FluentResults

## Project Structure

```
src/
  MicroCommerce.AppHost/              # Aspire orchestrator (entry point)
  MicroCommerce.ApiService/           # Backend API
    Features/                         # Vertical slices
      Catalog/                        # Products, categories, images
      Cart/                           # Shopping cart (guest + auth)
      Ordering/                       # Checkout, orders, saga
      Inventory/                      # Stock management
      Profiles/                       # User profiles, addresses, avatars
      Reviews/                        # Product reviews (verified purchase)
      Wishlists/                      # Authenticated user wishlists
      Messaging/                      # Dead letter queue UI
    Common/                           # Shared infrastructure
      Behaviors/                      # MediatR pipeline (validation, result validation)
      Persistence/                    # BaseDbContext, interceptors, conventions, OutboxDbContext
      Exceptions/                     # Global exception handling
      Extensions/                     # FluentResults -> HTTP mapping
      OpenApi/                        # Vogen/SmartEnum schema transformers
  MicroCommerce.Gateway/              # YARP reverse proxy
  MicroCommerce.ServiceDefaults/      # Aspire cross-cutting (telemetry, health)
  MicroCommerce.ApiService.Tests/     # xUnit integration + unit tests
    Integration/                      # Testcontainers + WebApplicationFactory
    Unit/                             # Domain logic unit tests
  MicroCommerce.Web/                  # Next.js frontend
    src/app/(storefront)/             # Customer-facing routes
    src/app/admin/                    # Admin dashboard
    src/components/                   # React components
    src/hooks/                        # Custom hooks (TanStack Query)
    src/lib/                          # API client, utilities
    src/types/                        # Type augmentations (next-auth.d.ts)
    e2e/                              # Playwright E2E tests
  BuildingBlocks/BuildingBlocks.Common/ # DDD primitives (aggregates, events, value objects)
```

## Build & Run

```bash
# Run the full stack via Aspire
dotnet run --project src/MicroCommerce.AppHost

# Frontend only
cd src/MicroCommerce.Web && npm install && npm run dev

# Backend only
dotnet run --project src/MicroCommerce.ApiService

# Run backend tests
dotnet test src/MicroCommerce.ApiService.Tests

# Frontend linting & formatting
cd src/MicroCommerce.Web && npm run lint
cd src/MicroCommerce.Web && npm run format

# E2E tests (requires full Aspire stack running)
cd src/MicroCommerce.Web && npx playwright test
```

## C# Conventions

- File-scoped namespaces always
- Primary constructors for DI
- Explicit types over `var` (`csharp_style_var_* = false`)
- Collection expressions `[]` over `new List<T>()`
- Private fields: `_camelCase`
- Record types for all DTOs and commands/queries
- FluentValidation for input validation
- Async/await for all I/O (no `.Result` or `.Wait()`)
- `TreatWarningsAsErrors` enabled globally via Directory.Build.props
- Nullable reference types enabled
- UUID v7 for all new entity IDs: `Guid.CreateVersion7()`
- EF Core snake_case naming convention (`UseSnakeCaseNamingConvention()`)
- Strongly typed IDs via Vogen: `[ValueObject<Guid>] public partial record struct ProductId`
- Domain enums via Ardalis.SmartEnum (e.g., `ProductStatus`, `OrderStatus`)
- Complex query filtering via Ardalis.Specification

## TypeScript/React Conventions

- TypeScript strict mode
- Biome for linting/formatting (2-space indent)
- File names: kebab-case (`auth-button.tsx`)
- Components: PascalCase (`AuthButton`)
- Path alias: `@/*` maps to `./src/*`
- Server Components by default; `"use client"` only when needed
- Prefer server components for data fetching
- TanStack React Query for client-side data fetching (hooks in `src/hooks/`)
- Use Suspense boundaries for async operations
- Clean up all effects (return cleanup functions)
- Stable keys for lists (never array index for dynamic lists)
- ARIA labels on interactive elements

## Feature Architecture (Backend)

Each feature follows vertical slice with these layers:
```
Features/{Name}/
  {Name}Endpoints.cs              # Minimal API route mapping
  Domain/
    Entities/                     # Aggregate roots and entities
    Events/                       # Domain events
  Application/
    Commands/                     # Write operations (MediatR IRequest)
    Queries/                      # Read operations (MediatR IRequest)
    Consumers/                    # MassTransit message consumers (Ordering, Inventory)
    Saga/                         # MassTransit state machines (Ordering only)
    Specifications/               # Ardalis.Specification query specs (Catalog, Ordering)
  Infrastructure/
    {Name}DbContext.cs            # Owned DbContext (schema-isolated)
    Configurations/               # EF Core entity configs
    Migrations/                   # Feature-specific EF migrations
    {Name}DataSeeder.cs           # Dev seed data (Catalog, Inventory only)
```

## Key Patterns

- **CQRS:** Commands and queries via MediatR with pipeline behaviors (ValidationBehavior, ResultValidationBehavior)
- **Domain Events:** Published via MassTransit after SaveChanges (DomainEventInterceptor)
- **EF Core Interceptors:** AuditInterceptor (CreatedAt/UpdatedAt), ConcurrencyInterceptor (Version), SoftDeleteInterceptor (soft-delete), DomainEventInterceptor
- **EF Core Model Conventions:** AuditableConvention, ConcurrencyTokenConvention, SoftDeletableConvention — auto-configure columns and global query filters (e.g., `WHERE is_deleted = false`)
- **Strongly Typed IDs:** Vogen `[ValueObject<Guid>]` with EF Core + System.Text.Json conversions
- **Schema-per-Feature:** All DbContexts share one `appdb` PostgreSQL database with separate schemas (catalog, cart, ordering, inventory, profiles, reviews, wishlists, outbox)
- **MassTransit Outbox:** OutboxDbContext in `outbox` schema provides transactional messaging (inbox deduplication + outbox delivery)
- **Checkout Saga:** CheckoutStateMachine orchestrates stock reservation -> payment -> confirm/fail with EF Core-persisted state
- **Circuit Breaker + Retry:** Global MassTransit config with exponential backoff [1s, 5s, 25s], circuit breaker at 15% failure rate
- **Guest Cart:** Cookie-based buyer identity (`BuyerIdentity.GetOrCreateBuyerId`), merges with authenticated cart on login
- **FluentResults:** Railway-oriented error handling in some handlers, processed by ResultValidationBehavior (returns 422 on failure)
- **YARP Gateway:** Centralizes CORS, rate limiting (sliding window), auth, and `X-Request-ID` injection; frontend routes through Gateway, not directly to ApiService
- **Aspire Topology:** PostgreSQL -> ApiService -> Gateway -> Frontend; frontend resolves API via `services__gateway__https__0` env var

## Testing

```bash
# Unit + integration tests
dotnet test src/MicroCommerce.ApiService.Tests

# E2E tests (requires running Aspire stack)
cd src/MicroCommerce.Web && npx playwright test
```

- **Integration tests:** xUnit + Testcontainers (real PostgreSQL) + MassTransit TestFramework (in-memory bus)
- **Test infra:** `ApiWebApplicationFactory` + `IntegrationTestBase` with shared container via `ICollectionFixture`
- **Auth in tests:** `FakeAuthenticationHandler` injects claims from `X-Test-UserId` header
- **DB isolation:** `ResetDatabase()` drops/recreates per-schema between tests
- **Unit tests:** Domain aggregates, value objects, validators, saga logic
- **E2E tests:** Playwright specs in `src/MicroCommerce.Web/e2e/` (critical-path, product-browsing, user-features)

## CI/CD

- `.github/workflows/dotnet-test.yml` — runs unit + integration tests on push to master
- `.github/workflows/release.yml` — publishes NuGet package + Docker images on `v*.*.*` tags
- `.github/dependabot.yml` — weekly NuGet dependency updates

## Important Notes

- The Aspire AppHost is the primary entry point for local development
- Keycloak realm config lives in `src/MicroCommerce.AppHost/Realms/`
- Frontend `.env` has auth secrets (AUTH_SECRET, KEYCLOAK_CLIENT_SECRET, KEYCLOAK_ISSUER)
- Frontend reads gateway URL from Aspire-injected `services__gateway__*` env vars, exposed to client via `/api/config` route
- OpenAPI docs available at `/openapi/v1.json` in development
- Health endpoints: `/health` (readiness), `/alive` (liveness)
