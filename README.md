# MicroCommerce

A showcase e-commerce platform demonstrating modern .NET microservices architecture with best practices.

> [!IMPORTANT]
> **Since 2026, zero lines of code have been written by a human.**
>
> Before 2026, I wrote code like a normal person. Then [Claude Code](https://docs.anthropic.com/en/docs/claude-code) happened.
>
> Now I mass-type `y` to approve tool permissions while Claude Code does all the actual work.
>
> **My contributions (2026–present):**
> - Mass-typing `y`
> - Mass-typing `y` faster
> - Mass-typing `y` with increasing confidence
>
> **Job title:** Senior LGTM Engineer | Chief `y` Officer
>
> If it works — I mass-typed `y` really well. If it doesn't — I probably typed `n` once by accident.

| Project | Tests | Sonar |
|---------|-------|-------|
| Backend | ![Test Result](https://github.com/baotoq/micro-commerce/actions/workflows/dotnet-test.yml/badge.svg) | [![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=baotoq_micro-commerce&metric=alert_status)](https://sonarcloud.io/summary/new_code?id=baotoq_micro-commerce) [![Lines of Code](https://sonarcloud.io/api/project_badges/measure?project=baotoq_micro-commerce&metric=ncloc)](https://sonarcloud.io/summary/new_code?id=baotoq_micro-commerce) |

Inspired by **[Microsoft eShop](https://github.com/dotnet/eShop)**, this project showcases the latest .NET stack with a focus on:

- **Modular Monolith → Microservices** — Start simple, extract when needed
- **DDD & CQRS** — Clean domain-driven architecture with MediatR
- **Event-Driven** — MassTransit with transactional outbox for reliable messaging
- **Cloud-Native** — .NET Aspire orchestration, Kubernetes-ready

## Table of Contents

- [Quick Start](#quick-start)
- [Architecture](#architecture)
- [Tech Stack](#tech-stack)
- [Project Structure](#project-structure)
- [Getting Started](#getting-started)
- [Feature Modules](#feature-modules)
- [Key Patterns](#key-patterns)
- [API Reference](#api-reference)
- [Testing](#testing)
- [Environment Variables](#environment-variables)
- [Available Commands](#available-commands)
- [CI/CD](#cicd)
- [Troubleshooting](#troubleshooting)
- [Star History](#star-history)
- [License](#license)

## Quick Start

```bash
# Run with .NET Aspire (starts all services + infrastructure)
dotnet run --project src/MicroCommerce.AppHost

# Open Aspire dashboard at https://localhost:17225
# Frontend at http://localhost:3000
```

**Requirements:** .NET 10 SDK, Docker Desktop, Node.js 20+

## Architecture

```
┌───────────────────────────────────────────────────────────────────┐
│                       Next.js Frontend (:3000)                    │
│              NextAuth.js · TanStack Query · Radix UI              │
└───────────────────────────┬───────────────────────────────────────┘
                            │
                            ▼
┌───────────────────────────────────────────────────────────────────┐
│                      API Gateway (YARP)                           │
│           CORS · Rate Limiting · JWT Auth · X-Request-ID          │
└───────────────────────────┬───────────────────────────────────────┘
                            │
    ┌───────────┬───────────┼───────────┬───────────┬───────────┐
    ▼           ▼           ▼           ▼           ▼           ▼
┌────────┐ ┌────────┐ ┌──────────┐ ┌──────────┐ ┌────────┐ ┌────────┐
│Catalog │ │  Cart  │ │ Ordering │ │Inventory │ │Profiles│ │Reviews │ ...
│ Module │ │ Module │ │  Module  │ │  Module  │ │ Module │ │ Module │
└───┬────┘ └───┬────┘ └────┬─────┘ └────┬─────┘ └───┬────┘ └───┬────┘
    │          │           │            │            │          │
    └──────────┴───────────┼────────────┴────────────┴──────────┘
                           ▼
             ┌──────────────────────────┐
             │    Azure Service Bus     │
             │  (Domain Events + Saga)  │
             └──────────────────────────┘
                           │
    ┌──────────────────────┼──────────────────────┐
    ▼                      ▼                      ▼
┌────────┐         ┌─────────────┐         ┌──────────┐
│PostgreSQL│        │  Keycloak   │         │Azure Blob│
│ (appdb) │        │  (:8101)    │         │ Storage  │
└─────────┘        └─────────────┘         └──────────┘
```

**Request flow:** Browser → Next.js → YARP Gateway → API Service → PostgreSQL

All infrastructure runs locally via .NET Aspire with Docker containers. No separate Docker Compose needed.

### Aspire Topology

| Resource | Purpose | Notes |
|----------|---------|-------|
| PostgreSQL | Primary database (shared `appdb`, schema-per-module) | Persistent volume, includes PgAdmin |
| Azure Service Bus | Domain events, saga commands | Emulator for local dev |
| Azure Blob Storage | Product images, avatars | Emulator for local dev |
| Keycloak | Identity provider (JWT + OIDC) | Port 8101, persistent volume, auto-imports realm |
| API Service | Backend API (all feature modules) | Health check at `/health` |
| Gateway | YARP reverse proxy | CORS, rate limiting, auth |
| Frontend | Next.js app | Port 3000 |

## Tech Stack

### Backend

| Technology | Version | Purpose |
|------------|---------|---------|
| .NET | 10 | Runtime |
| ASP.NET Core | 10 | Minimal APIs |
| .NET Aspire | 13.1.0 | Cloud-native orchestration |
| Entity Framework Core | 10 | ORM with PostgreSQL |
| MediatR | 13.1.0 | CQRS pipeline |
| MassTransit | 9.0.0 | Messaging, saga, outbox |
| FluentValidation | 12.1.1 | Request validation |
| FluentResults | 4.0.0 | Railway-oriented error handling |
| Vogen | 8.0.4 | Strongly typed IDs |
| Ardalis.SmartEnum | 8.2.0 | Domain enumerations |
| Ardalis.Specification | 9.3.1 | Query specifications |
| YARP | 2.2.0 | Reverse proxy gateway |
| SixLabors.ImageSharp | 3.1.6 | Image processing |

### Frontend

| Technology | Version | Purpose |
|------------|---------|---------|
| Next.js | 16 | React framework |
| React | 19 | UI library |
| TypeScript | 5 | Type safety |
| Tailwind CSS | 4 | Styling |
| TanStack React Query | 5 | Client-side data fetching |
| Radix UI | — | Accessible component primitives |
| NextAuth.js | 5 (beta) | Authentication (Keycloak provider) |
| Recharts | — | Admin dashboard charts |
| DnD Kit | — | Drag and drop |
| Playwright | — | E2E testing |
| Biome | 2.2.0 | Linting and formatting |

### Infrastructure

| Technology | Purpose |
|------------|---------|
| PostgreSQL | Primary database |
| Azure Service Bus | Message broker (emulator for dev) |
| Azure Blob Storage | File storage (emulator for dev) |
| Keycloak | Identity provider |

## Project Structure

```
src/
├── MicroCommerce.AppHost/              # Aspire orchestrator (entry point)
│   └── Realms/                         # Keycloak realm config
├── MicroCommerce.ApiService/           # Backend API
│   ├── Features/                       # Vertical slice modules
│   │   ├── Catalog/                    # Products, categories, images
│   │   ├── Cart/                       # Shopping cart (guest + auth)
│   │   ├── Ordering/                   # Checkout, orders, saga
│   │   ├── Inventory/                  # Stock management
│   │   ├── Profiles/                   # User profiles, addresses, avatars
│   │   ├── Reviews/                    # Product reviews (verified purchase)
│   │   ├── Wishlists/                  # Authenticated user wishlists
│   │   └── Messaging/                  # Dead letter queue admin UI
│   └── Common/                         # Shared infrastructure
│       ├── Behaviors/                  # MediatR pipeline behaviors
│       ├── Persistence/                # BaseDbContext, interceptors, conventions
│       ├── Exceptions/                 # Global exception handling
│       ├── Extensions/                 # FluentResults → HTTP mapping
│       ├── Messaging/                  # MassTransit filters
│       └── OpenApi/                    # Vogen/SmartEnum schema transformers
├── MicroCommerce.Gateway/              # YARP reverse proxy
├── MicroCommerce.ServiceDefaults/      # Aspire cross-cutting (telemetry, health)
├── MicroCommerce.ApiService.Tests/     # xUnit integration + unit tests
│   ├── Integration/                    # Testcontainers + WebApplicationFactory
│   └── Unit/                           # Domain logic unit tests
├── MicroCommerce.Web/                  # Next.js frontend
│   ├── src/
│   │   ├── app/(storefront)/           # Customer-facing routes
│   │   ├── app/admin/                  # Admin dashboard
│   │   ├── components/                 # React components
│   │   ├── hooks/                      # TanStack Query hooks
│   │   ├── lib/                        # API client, auth, utilities
│   │   └── types/                      # Type augmentations
│   └── e2e/                            # Playwright E2E tests
└── BuildingBlocks/
    └── BuildingBlocks.Common/          # DDD primitives (aggregates, events, value objects)
```

### Feature Module Structure

Each backend feature follows vertical slice architecture:

```
Features/{Name}/
  {Name}Endpoints.cs              # Minimal API route mapping
  Domain/
    Entities/                     # Aggregate roots and entities
    Events/                       # Domain events
  Application/
    Commands/                     # Write operations (MediatR IRequest)
    Queries/                      # Read operations (MediatR IRequest)
    Consumers/                    # MassTransit message consumers
    Saga/                         # MassTransit state machines (Ordering)
    Specifications/               # Ardalis.Specification query specs
  Infrastructure/
    {Name}DbContext.cs            # Owned DbContext (schema-isolated)
    Configurations/               # EF Core entity configs
    Migrations/                   # Feature-specific EF migrations
```

## Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Docker Desktop](https://www.docker.com/products/docker-desktop) (for PostgreSQL, Keycloak, Service Bus emulator)
- [Node.js 20+](https://nodejs.org/) (for frontend)

### 1. Clone the Repository

```bash
git clone https://github.com/baotoq/micro-commerce.git
cd micro-commerce
```

### 2. Run with Aspire

This single command starts all backend services, the frontend, and all infrastructure (PostgreSQL, Keycloak, Service Bus emulator, Blob Storage emulator):

```bash
dotnet run --project src/MicroCommerce.AppHost
```

On first run, Docker will pull container images for PostgreSQL, Keycloak, and the Azure emulators. This may take a few minutes.

### 3. Access the Application

| Service | URL |
|---------|-----|
| Aspire Dashboard | `https://localhost:17225` |
| Frontend | `http://localhost:3000` |
| Keycloak Admin | `http://localhost:8101` |
| PgAdmin | See Aspire dashboard for port |
| OpenAPI Spec | `http://localhost:5000/openapi/v1.json` |

### 4. Frontend-Only Development

If you only want to work on the frontend (backend must be running separately):

```bash
cd src/MicroCommerce.Web
npm install
npm run dev
```

### 5. Backend-Only Development

```bash
dotnet run --project src/MicroCommerce.ApiService
```

> **Note:** The backend requires PostgreSQL, Service Bus, and Keycloak to be running. Use Aspire for the full stack or start dependencies manually.

### 6. Environment Setup

The frontend `.env` file is pre-configured for local development:

| Variable | Description | Default |
|----------|-------------|---------|
| `AUTH_SECRET` | NextAuth.js secret | Pre-set for dev |
| `KEYCLOAK_CLIENT_ID` | Keycloak OIDC client | `nextjs-app` |
| `KEYCLOAK_CLIENT_SECRET` | Keycloak client secret | `nextjs-app-secret-change-in-production` |
| `KEYCLOAK_ISSUER` | Keycloak realm URL | `http://localhost:8101/realms/micro-commerce` |
| `NEXT_PUBLIC_KEYCLOAK_ISSUER` | Client-side Keycloak URL | Same as above |

Aspire automatically injects `services__gateway__https__0` for the frontend to discover the Gateway URL.

## Feature Modules

### Catalog

Products and categories with image upload, search, filtering, and status management (Draft → Published → Archived).

**Domain:** `Product` aggregate (with `ProductName`, `Money`, `ProductStatus` value objects), `Category` entity.

**Events:** `ProductCreatedDomainEvent`, `ProductUpdatedDomainEvent`, `ProductStatusChangedDomainEvent`, `ProductArchivedDomainEvent`

### Cart

Cookie-based guest cart with authenticated cart merge on login. 30-day TTL with automatic expiration cleanup.

**Domain:** `Cart` aggregate with `CartItem` collection. Max 99 quantity per item. Supports `AddItem`, `UpdateItemQuantity`, `RemoveItem`, `TransferOwnership`.

### Ordering

Saga-based checkout orchestrating stock reservation, payment, and order confirmation across modules.

**Checkout Saga flow:**
1. `CheckoutStarted` → Reserve stock
2. `StockReservationCompleted` → Wait for payment
3. `PaymentCompleted` → Confirm order + deduct stock + clear cart
4. Any failure → Compensate (release reservations, mark order failed)

**Domain:** `Order` aggregate with statuses: Submitted → StockReserved → Paid → Confirmed → Shipped → Delivered (or Failed at any step).

### Inventory

Stock management with reservations (15-minute TTL), adjustment history, and low-stock alerts (threshold: 10 units).

**Domain:** `StockItem` aggregate with `StockReservation` and `StockAdjustment` children. Computed `AvailableQuantity` accounts for active reservations.

**Events:** `StockAdjustedDomainEvent`, `StockLowDomainEvent`, `StockReservedDomainEvent`, `StockReleasedDomainEvent`

### Profiles

User profiles with display names, avatar upload (max 5MB, processed via ImageSharp), and address management with default address invariant.

**Domain:** `UserProfile` aggregate with owned `Address` collection. Auto-created on first access.

### Reviews

Product reviews with verified purchase enforcement. Only users who have completed an order containing the product can leave a review.

**Domain:** `Review` aggregate with `Rating` (1-5) and `ReviewText` value objects.

### Wishlists

Authenticated user wishlists for saving products.

### Messaging (DLQ)

Admin UI for managing dead-lettered messages from Azure Service Bus. Supports viewing, retrying, and purging DLQ messages.

## Key Patterns

### CQRS with MediatR

Commands and queries are separate MediatR requests processed through a pipeline:
1. `ValidationBehavior` — FluentValidation rules (throws on failure)
2. `ResultValidationBehavior` — FluentResults validation (returns 422 on failure)

### Domain Events

Aggregate roots collect domain events, which are published via MassTransit after `SaveChanges`. The `DomainEventInterceptor` scans the EF Core change tracker for pending events and dispatches them through the transactional outbox.

### Transactional Outbox

MassTransit's EF Core outbox (`OutboxDbContext` in `outbox` schema) ensures domain events are published reliably — events are persisted in the same transaction as the domain changes, then delivered asynchronously.

### Schema-per-Feature Database

All modules share a single PostgreSQL database (`appdb`) but are isolated into separate schemas:

| Module | Schema |
|--------|--------|
| Catalog | `catalog` |
| Cart | `cart` |
| Ordering | `ordering` |
| Inventory | `inventory` |
| Profiles | `profiles` |
| Reviews | `reviews` |
| Wishlists | `wishlists` |
| Outbox | `outbox` |

Each module has its own `DbContext`, migrations, and `__EFMigrationsHistory` table.

### Strongly Typed IDs (Vogen)

All entity IDs use Vogen value objects with UUID v7 for sortable generation:

```csharp
[ValueObject<Guid>(conversions: Conversions.EfCoreValueConverter | Conversions.SystemTextJson)]
public partial record struct ProductId
{
    public static Validation Validate(Guid value) =>
        value != Guid.Empty ? Validation.Ok : Validation.Invalid("ProductId cannot be empty.");

    public static ProductId New() => From(Guid.CreateVersion7());
}
```

### EF Core Interceptors & Conventions

**Interceptors** (applied to all DbContexts):
- `AuditInterceptor` — auto-sets `CreatedAt`/`UpdatedAt` on `IAuditable` entities
- `ConcurrencyInterceptor` — auto-increments `Version` on `IConcurrencyToken` entities
- `SoftDeleteInterceptor` — converts delete to soft-delete on `ISoftDeletable` entities
- `DomainEventInterceptor` — publishes domain events after SaveChanges

**Model Conventions:**
- `AuditableConvention` — configures audit column types
- `ConcurrencyTokenConvention` — configures version as concurrency token
- `SoftDeletableConvention` — applies global `WHERE is_deleted = false` query filter

### YARP Gateway

The gateway centralizes cross-cutting concerns:
- **CORS** — allows `localhost:3000` with credentials
- **Rate Limiting** — sliding window: 30 req/min anonymous, 100 req/min authenticated
- **JWT Auth** — Keycloak JWT validation, `authenticated` policy for write endpoints
- **X-Request-ID** — injected into every proxied request
- **Route Authorization** — read endpoints public, write endpoints require auth

### Resilience

MassTransit endpoints are configured with:
- **Retry** — exponential backoff at 1s, 5s, 25s intervals (skips `PermanentException`)
- **Circuit breaker** — 1-minute tracking window, trips at 15% failure rate, 5-minute reset

## API Reference

### Catalog (`/api/catalog`)

| Method | Path | Auth | Description |
|--------|------|------|-------------|
| GET | `/products` | No | List products (paginated, filterable) |
| GET | `/products/{id}` | No | Get product by ID |
| POST | `/products` | Gateway | Create product |
| PUT | `/products/{id}` | Gateway | Update product |
| PATCH | `/products/{id}/status` | Gateway | Change product status |
| DELETE | `/products/{id}` | Gateway | Archive product (soft delete) |
| POST | `/images` | Gateway | Upload product image |
| GET | `/categories` | No | List categories |
| GET | `/categories/{id}` | No | Get category |
| POST | `/categories` | Gateway | Create category |
| PUT | `/categories/{id}` | Gateway | Update category |
| DELETE | `/categories/{id}` | Gateway | Delete category |

### Cart (`/api/cart`)

| Method | Path | Auth | Description |
|--------|------|------|-------------|
| GET | `/` | No | Get cart (cookie-based identity) |
| POST | `/items` | No | Add item to cart |
| PUT | `/items/{itemId}` | No | Update item quantity |
| DELETE | `/items/{itemId}` | No | Remove item |
| GET | `/count` | No | Get item count |
| POST | `/merge` | JWT | Merge guest cart into authenticated cart |

### Ordering (`/api/ordering`)

| Method | Path | Auth | Description |
|--------|------|------|-------------|
| POST | `/checkout` | No | Submit order (starts saga) |
| POST | `/orders/{id}/pay` | No | Simulate payment |
| GET | `/orders/{id}` | No | Get order by ID |
| GET | `/orders/my` | No | Get current buyer's orders |
| GET | `/orders` | No | List all orders (admin) |
| GET | `/dashboard` | No | Order dashboard stats (admin) |
| PATCH | `/orders/{id}/status` | No | Update order status (admin) |

### Inventory (`/api/inventory`)

| Method | Path | Auth | Description |
|--------|------|------|-------------|
| GET | `/stock/{productId}` | No | Get stock info |
| GET | `/stock` | No | Bulk stock levels (`?productIds=a,b,c`) |
| POST | `/stock/{productId}/adjust` | Gateway | Adjust stock |
| GET | `/stock/{productId}/adjustments` | No | Adjustment history |

### Profiles (`/api/profiles`)

| Method | Path | Auth | Description |
|--------|------|------|-------------|
| GET | `/me` | JWT | Get or auto-create profile |
| PUT | `/me` | JWT | Update display name |
| POST | `/me/avatar` | JWT | Upload avatar (max 5MB) |
| DELETE | `/me/avatar` | JWT | Remove avatar |
| POST | `/me/addresses` | JWT | Add address |
| PUT | `/me/addresses/{id}` | JWT | Update address |
| DELETE | `/me/addresses/{id}` | JWT | Delete address |
| PATCH | `/me/addresses/{id}/default` | JWT | Set default address |

### Reviews (`/api/reviews`)

| Method | Path | Auth | Description |
|--------|------|------|-------------|
| GET | `/products/{productId}` | No | Get product reviews |
| GET | `/products/{productId}/mine` | JWT | Get user's review |
| GET | `/products/{productId}/can-review` | JWT | Check eligibility |
| POST | `/products/{productId}` | JWT | Create review (verified purchase) |
| PUT | `/{reviewId}` | JWT | Update review |
| DELETE | `/{reviewId}` | JWT | Delete review |

### Wishlists (`/api/wishlist`)

| Method | Path | Auth | Description |
|--------|------|------|-------------|
| GET | `/` | JWT | Get wishlist |
| GET | `/count` | JWT | Get item count |
| GET | `/product-ids` | JWT | Get wishlisted product IDs |
| POST | `/{productId}` | JWT | Add to wishlist |
| DELETE | `/{productId}` | JWT | Remove from wishlist |

### Messaging (`/api/messaging`)

| Method | Path | Auth | Description |
|--------|------|------|-------------|
| GET | `/dead-letters` | JWT | List dead-lettered messages |
| POST | `/dead-letters/retry` | JWT | Retry a DLQ message |
| POST | `/dead-letters/purge` | JWT | Purge DLQ messages |

### System Endpoints

| Path | Description |
|------|-------------|
| `/health` | Readiness check |
| `/alive` | Liveness check |
| `/openapi/v1.json` | OpenAPI spec (dev only) |

## Testing

### Backend Tests

```bash
# Run all tests (unit + integration)
dotnet test src/MicroCommerce.ApiService.Tests

# Run with coverage
dotnet test src/MicroCommerce.ApiService.Tests --collect:"XPlat Code Coverage" --settings src/MicroCommerce.ApiService.Tests/coverlet.runsettings
```

**Integration tests** use Testcontainers to spin up a real PostgreSQL container and MassTransit's in-memory test harness. The `ApiWebApplicationFactory` + `IntegrationTestBase` provide:
- Shared PostgreSQL container across tests via `ICollectionFixture`
- `FakeAuthenticationHandler` that injects claims from `X-Test-UserId` header
- `ResetDatabase()` for per-test schema isolation

**Test coverage:** Cart, Catalog, Inventory, Ordering, Profiles, Reviews, Wishlists, Interceptors (integration); Cart, Catalog, Inventory, Ordering aggregates + validators (unit).

### E2E Tests

```bash
# Run Playwright E2E tests (requires full Aspire stack running)
cd src/MicroCommerce.Web
npx playwright test

# Run with UI mode
npx playwright test --ui

# View test report
npx playwright show-report
```

E2E specs: `critical-path.spec.ts`, `product-browsing.spec.ts`, `user-features.spec.ts`

## Environment Variables

### Frontend (`src/MicroCommerce.Web/.env`)

| Variable | Required | Description |
|----------|----------|-------------|
| `AUTH_SECRET` | Yes | NextAuth.js encryption secret |
| `KEYCLOAK_CLIENT_ID` | Yes | Keycloak OIDC client ID |
| `KEYCLOAK_CLIENT_SECRET` | Yes | Keycloak OIDC client secret |
| `KEYCLOAK_ISSUER` | Yes | Keycloak realm issuer URL |
| `NEXT_PUBLIC_KEYCLOAK_ISSUER` | Yes | Client-side Keycloak URL |
| `NEXT_PUBLIC_API_URL` | No | API base URL (default: `http://localhost:5200`) |
| `services__gateway__https__0` | Auto | Aspire-injected gateway URL |

### Backend

Backend configuration is handled by Aspire service discovery and `appsettings.json`. Connection strings for PostgreSQL, Service Bus, Blob Storage, and Keycloak are injected automatically by Aspire.

## Available Commands

### Backend

| Command | Description |
|---------|-------------|
| `dotnet run --project src/MicroCommerce.AppHost` | Start full stack via Aspire |
| `dotnet run --project src/MicroCommerce.ApiService` | Start backend only |
| `dotnet build src/` | Build all projects |
| `dotnet test src/MicroCommerce.ApiService.Tests` | Run tests |
| `dotnet ef migrations add <Name> --context <DbContext> --output-dir Features/<Module>/Infrastructure/Migrations` | Add EF migration |

### Frontend

| Command | Description |
|---------|-------------|
| `npm run dev` | Start dev server |
| `npm run build` | Production build |
| `npm run start` | Start production server |
| `npm run lint` | Run Biome linter |
| `npm run format` | Format with Biome |
| `npm run test:e2e` | Run Playwright tests |
| `npm run test:e2e:ui` | Playwright UI mode |
| `npm run test:e2e:report` | View Playwright report |

## CI/CD

- **GitHub Actions** — `.github/workflows/dotnet-test.yml` runs unit + integration tests on push to master
- **GitHub Actions** — `.github/workflows/release.yml` publishes NuGet packages and Docker images on version tags
- **SonarCloud** — Static analysis and code quality
- **Dependabot** — Weekly NuGet dependency updates

## Troubleshooting

### Docker Containers Not Starting

**Symptom:** Aspire dashboard shows services as unhealthy.

**Solution:** Ensure Docker Desktop is running. On first run, images need to be pulled which can take several minutes. Check Aspire dashboard logs for specific errors.

### Database Migration Errors

**Symptom:** `relation "xyz" does not exist` or migration-related errors.

**Solution:** Migrations run automatically on startup. If the database is in a bad state:
1. Delete the PostgreSQL data volume: stop Aspire, run `docker volume ls` to find the postgres volume, then `docker volume rm <volume-name>`
2. Restart Aspire — it will recreate and seed the database

### Keycloak Not Ready

**Symptom:** Authentication fails or redirects to an error page.

**Solution:** Keycloak can take 30-60 seconds to start. The realm auto-imports from `src/MicroCommerce.AppHost/Realms/`. If the realm is corrupted, delete the Keycloak data volume and restart.

### Frontend Can't Reach Backend

**Symptom:** API calls fail with network errors.

**Solution:**
1. Verify the Gateway is running in the Aspire dashboard
2. Check that `NEXT_PUBLIC_API_URL` or `services__gateway__https__0` is set correctly
3. The frontend routes through the Gateway, not directly to the API service

### Port Conflicts

**Symptom:** `Address already in use` errors.

**Solution:** Default ports are 3000 (frontend), 8101 (Keycloak). Check for other processes using these ports:
```bash
lsof -i :3000
lsof -i :8101
```

## Star History

<a href="https://star-history.com/#baotoq/micro-commerce&Date">
 <picture>
   <source media="(prefers-color-scheme: dark)" srcset="https://api.star-history.com/svg?repos=baotoq/micro-commerce&type=Date&theme=dark" />
   <source media="(prefers-color-scheme: light)" srcset="https://api.star-history.com/svg?repos=baotoq/micro-commerce&type=Date" />
   <img alt="Star History Chart" src="https://api.star-history.com/svg?repos=baotoq/micro-commerce&type=Date" />
 </picture>
</a>

## License

MIT
