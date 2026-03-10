# MicroCommerce — Project Structure

## Root
```
src/
  MicroCommerce.AppHost/          # Aspire orchestrator (entry point)
  MicroCommerce.ApiService/       # Backend API
  MicroCommerce.Gateway/          # YARP reverse proxy
  MicroCommerce.ServiceDefaults/  # Aspire cross-cutting (telemetry, health)
  MicroCommerce.ApiService.Tests/ # xUnit integration + unit tests
  MicroCommerce.Web/              # Next.js frontend
  BuildingBlocks/BuildingBlocks.Common/  # DDD primitives
```

## ApiService Features (vertical slices)
Each feature under `src/MicroCommerce.ApiService/Features/{Name}/`:
- Catalog, Cart, Ordering, Inventory, Profiles, Reviews, Wishlists, Messaging, Coupons

Each feature structure:
```
{Name}Endpoints.cs
Domain/
  Entities/, Events/, ValueObjects/
Application/
  Commands/, Queries/, Consumers/, Saga/, Specifications/
Infrastructure/
  {Name}DbContext.cs, Configurations/, Migrations/, {Name}DataSeeder.cs
```

## Frontend (Next.js)
```
src/MicroCommerce.Web/
  src/app/(storefront)/   # Customer-facing routes
  src/app/admin/          # Admin dashboard
  src/components/         # React components
  src/hooks/              # TanStack Query hooks
  src/lib/                # API client, utilities
  src/auth.ts             # NextAuth.js config
  src/middleware.ts        # Auth route protection
  e2e/                    # Playwright E2E tests
```

## Tests
```
src/MicroCommerce.ApiService.Tests/
  Integration/   # Testcontainers + WebApplicationFactory
  Unit/          # Domain aggregates, value objects, validators, saga
```

## Design Files
`design/` — split .pen files per page (design-system, storefront-*, admin-dashboard, auth-login)
