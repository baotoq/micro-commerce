# CLAUDE.md

## Project Overview

MicroCommerce is a showcase e-commerce platform demonstrating modern .NET microservices architecture. Users can browse products, add to cart, and checkout through a Next.js storefront. The backend is a modular monolith designed for gradual extraction to microservices.

## Tech Stack

- **Backend:** .NET 10, ASP.NET Core Minimal APIs, .NET Aspire 13.1.0
- **Frontend:** Next.js 16, React 19, TypeScript 5
- **Auth:** Keycloak (backend JWT + NextAuth.js frontend)
- **Database:** PostgreSQL (per-service isolation)
- **Messaging:** Azure Service Bus (MassTransit) with emulator for dev
- **Patterns:** CQRS (MediatR), DDD, Event-Driven, Vertical Slice Architecture

## Project Structure

```
src/
  MicroCommerce.AppHost/          # Aspire orchestrator (entry point)
  MicroCommerce.ApiService/       # Backend API
    Features/                     # Vertical slices
      Catalog/                    # Products, categories, images
      Cart/                       # Shopping cart (guest + auth)
      Ordering/                   # Checkout and orders
      Inventory/                  # Stock management
      Messaging/                  # Dead letter queue UI
    Common/                       # Shared: behaviors, persistence, exceptions
  MicroCommerce.ServiceDefaults/  # Aspire cross-cutting (telemetry, health)
  MicroCommerce.Web/              # Next.js frontend
    src/app/(storefront)/         # Customer-facing routes
    src/app/admin/                # Admin dashboard
    src/components/               # React components
    src/hooks/                    # Custom hooks
    src/lib/                      # API client, utilities
  BuildingBlocks/Common/          # DDD primitives (aggregates, events, value objects)
```

## Build & Run

```bash
# Run the full stack via Aspire
dotnet run --project src/MicroCommerce.AppHost

# Frontend only
cd src/MicroCommerce.Web && npm install && npm run dev

# Backend only
dotnet run --project src/MicroCommerce.ApiService
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

## TypeScript/React Conventions

- TypeScript strict mode
- Biome for linting/formatting (2-space indent)
- File names: kebab-case (`auth-button.tsx`)
- Components: PascalCase (`AuthButton`)
- Path alias: `@/*` maps to `./src/*`
- Server Components by default; `"use client"` only when needed
- Prefer server components for data fetching
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
  Infrastructure/
    {Name}DbContext.cs            # Owned DbContext
    Configurations/               # EF Core entity configs
    {Name}DataSeeder.cs           # Dev seed data (BackgroundService)
  DependencyInjection.cs          # Feature service registration
```

## Key Patterns

- **CQRS:** Commands and queries via MediatR with pipeline behaviors (validation, logging)
- **Domain Events:** Published via MassTransit after SaveChanges (DomainEventInterceptor)
- **Strongly Typed IDs:** `record ProductId(Guid Value) : StronglyTypedId<Guid>(Value)`
- **Guest Cart:** Cookie-based buyer identity (`BuyerIdentity.GetOrCreateBuyerId`)
- **Database-per-feature:** Separate DbContext per feature module
- **Aspire Service Discovery:** Frontend references backend via Aspire-injected URLs

## Testing

No test projects exist yet. When adding tests:
- Use xUnit with WebApplicationFactory for integration tests
- Place in `src/MicroCommerce.ApiService.Tests/`
- Use React Testing Library for frontend component tests

## Important Notes

- The Aspire AppHost is the primary entry point for local development
- Keycloak realm config lives in `src/MicroCommerce.AppHost/Realms/`
- Frontend `.env` has auth secrets (AUTH_SECRET, KEYCLOAK_CLIENT_SECRET)
- OpenAPI docs available at `/openapi/v1.json` in development
- Health endpoints: `/health` (readiness), `/alive` (liveness)
