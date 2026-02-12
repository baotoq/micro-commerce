# Project State

**Project:** MicroCommerce
**Current Phase:** Phase 8 - Order History & Management
**Status:** In Progress
**Last Updated:** 2026-02-12

---

## Current Position

Phase: 8 of 10 (Order History & Management)
Plan: 5 of 5 in current phase
Status: Phase complete
Last activity: 2026-02-12 - Completed 08-05-PLAN.md (Admin Order Kanban Board)

Progress: ██████████████████░░ 88%

---

## Progress Overview

| Phase | Name | Status | Plans |
|-------|------|--------|-------|
| 1 | Foundation & Project Structure | COMPLETE | 6/6 |
| 2 | Catalog Domain & Admin CRUD | COMPLETE | 7/7 |
| 3 | Catalog Storefront & Seed Data | IN PROGRESS | 5/6 |
| 4 | Inventory Domain | COMPLETE | 5/5 |
| 5 | Event Bus Infrastructure | COMPLETE | 3/3 |
| 6 | Cart Domain | IN PROGRESS | 3/4 |
| 7 | Ordering Domain & Checkout | IN PROGRESS | 3/? |
| 8 | Order History & Management | COMPLETE | 5/5 |
| 9 | API Gateway | NOT STARTED | 0/? |
| 10 | Testing & Polish | NOT STARTED | 0/? |

**Phases Completed:** 5/10
**Plans Completed (Phase 8):** 5/5

---

## Completed Plans

### Phase 1
- [x] **01-01**: Infrastructure setup (NuGet packages, Aspire resources) - `141b031`, `f230804`
- [x] **01-02**: Module structure & DbContexts - `439b842`, `e89c3fd`
- [x] **01-03**: MediatR pipeline & validation - `cfeb3c6`, `6533d8f`
- [x] **01-04**: Domain event infrastructure (MassTransit + outbox) - `81c9fa0`, `293f73d`, `f6f5c7e`
- [x] **01-05**: CQRS reference implementation - `46c14b3`, `15d1b9c`, `8db2bf0`, `e7aa61c`
- [x] **01-06**: UAT gap fixes (migration + exception handler) - `830b441`, `e9e56c9`

### Phase 2
- [x] **02-01**: Product Domain Model & CQRS Stack - `4745799c`
- [x] **02-02**: Product CRUD Commands & Queries - `4745799c`
- [x] **02-03**: Azure Blob Storage Image Upload - `4745799c`
- [x] **02-04**: Category CRUD Completion - `4745799c`
- [x] **02-05**: Admin UI - Product List & Table - `4745799c`
- [x] **02-06**: Admin UI - Product Drawer (Create/Edit) - `4745799c`
- [x] **02-07**: Admin UI - Categories Page - `4745799c`

### Phase 3
- [x] **03-01**: Sort support & seed data - `4db9e1eb`, `fffac359`
- [x] **03-02**: Storefront layout & hero banner - `eab9565d`, `a59f959c`
- [x] **03-03**: Product grid & cards with infinite scroll - `c893f6b4`, `be9ba295`
- [x] **03-04**: Product detail page & related products - `ac8dff98`
- [x] **03-05**: Search, filter & sort controls - `7d22382c`, `025c1a3f`

### Phase 4
- [x] **04-01**: Inventory Domain Model - `00f0c76e`, `f7454fea`
- [x] **04-02**: Inventory CQRS & API Endpoints - `ffb1e4ab`, `98698643`
- [x] **04-03**: Background Services & Data Seeder - `d2f9adab`, `879a9d66`
- [x] **04-04**: Admin Stock Management UI - `e151a377`, `ed820b97`
- [x] **04-05**: Storefront Stock Status Display - `64b0d56b`, `6dfb7c61`

### Phase 5
- [x] **05-01**: Global MassTransit Middleware & PermanentException - `8766ddb8`, `cc029c82`
- [x] **05-02**: DLQ Management Backend (service, CQRS, endpoints, fault consumer) - `23b07849`, `8c03d633`
- [x] **05-03**: Admin DLQ Management Page - `b775fad7`, `d9049da4`

### Phase 6
- [x] **06-01**: Cart Domain Model & Persistence - `c8c74c80`, `00700bdd`
- [x] **06-02**: Cart CQRS & API Endpoints - `8d26f1c1`, `b4d6e115`
- [x] **06-03**: Cart React Query Hooks - `498813d6`, `645016c7`

### Phase 7
- [x] **07-01**: Order Domain Model & Persistence - `237a46a0`, `cadb1fbc`
- [x] **07-02**: Ordering CQRS & API Endpoints - `7e682068`, `f581eb3c`
- [x] **07-03**: Checkout Saga State Machine & Consumers - `5e87febf`, `0e1547e6`

### Phase 8
- [x] **08-01**: Order Backend API (domain extensions, queries, endpoints) - `57928777`, `1c98db54`
- [x] **08-02**: Frontend Data Layer Infrastructure - `fd96a3f8`, `bec2af5e`
- [x] **08-03**: Customer Order History & Detail Pages - `2bae9ee4`, `c9f5fab4`
- [x] **08-04**: Admin Order Dashboard (stat cards, bar chart, recent orders) - `930f8eba`, `fe40dabc`
- [x] **08-05**: Admin Order Kanban Board & Detail Page - `f09732ec`, `81572462`

---

## Decisions Made

| Date | Decision | Rationale |
|------|----------|-----------|
| 2026-01-29 | Start with modular monolith | Research shows premature extraction is #1 microservices failure |
| 2026-01-29 | 10 phases for comprehensive depth | Matches project config and ensures thorough implementation |
| 2026-01-29 | Catalog before Inventory | Catalog has no dependencies, foundation for other services |
| 2026-01-29 | Event bus before Cart/Ordering | Reliable messaging required before saga patterns |
| 2026-01-29 | Azure Service Bus emulator for local dev | Avoid cloud costs during development |
| 2026-01-29 | PostgreSQL with pgAdmin | Easy database inspection and management |
| 2026-01-29 | Schema-per-module isolation | Each module gets own PostgreSQL schema for data boundaries |
| 2026-01-29 | Namespace-filtered configurations | DbContexts only apply configs from their module namespace |
| 2026-01-29 | Validation as first pipeline behavior | Fail fast before handler execution |
| 2026-01-29 | Validators auto-discovered from assembly | Zero-config validator registration |
| 2026-01-29 | SavedChangesAsync for event dispatch | Events published after transaction commit, not during |
| 2026-01-29 | Dedicated outbox schema | MassTransit tables isolated in 'outbox' schema |
| 2026-01-29 | Thin domain events with ID only | Consumers query for additional data they need |
| 2026-01-29 | Factory methods for aggregates | Encapsulate creation logic and event raising |
| 2026-01-29 | No repository abstraction | DbContext injected directly in modular monolith |
| 2026-01-30 | shadcn/ui for admin UI | Consistent, accessible components with Tailwind CSS |
| 2026-01-30 | Slide-out drawer for product forms | More space than modal, better UX for complex forms |
| 2026-01-30 | Azure Blob Storage with Azurite emulator | Local dev without cloud costs, production-ready pattern |
| 2026-01-30 | Soft delete for products (Archive) | Preserve data integrity, allow recovery |
| 2026-01-30 | Hard delete for categories | Simple entities, referential integrity via FK constraint |
| 2026-02-07 | BackgroundService for data seeding | Simpler than EF UseAsyncSeeding, works well with Aspire |
| 2026-02-07 | Development-only seeding with idempotency guard | Check Categories table emptiness before seeding |
| 2026-02-07 | Apple Store aesthetic for storefront | Zinc palette, generous whitespace, backdrop blur header |
| 2026-02-07 | Route group layout for storefront | (storefront) wraps customer pages independently of admin |
| 2026-02-07 | Skeleton loading for product placeholders | Consistent with admin loading patterns |
| 2026-02-07 | Intersection Observer for infinite scroll | Native API, no library needed, 200px rootMargin for pre-fetch |
| 2026-02-07 | getStorefrontProducts always filters Published | Storefront never shows Draft/Archived products |
| 2026-02-07 | URL params as filter state source of truth | Shareable links, browser back/forward navigation |
| 2026-02-07 | 300ms debounce on search input | Balance responsiveness with API call reduction |
| 2026-02-08 | Raw Guid for ProductId in Inventory module | Cross-module boundary - Inventory does not reference Catalog types |
| 2026-02-08 | xmin concurrency token via IsRowVersion() | PostgreSQL native optimistic concurrency without extra columns |
| 2026-02-08 | 15-minute TTL for stock reservations | Balance holding stock for checkout vs releasing abandoned carts |
| 2026-02-08 | StockLow threshold at 10 units | Simple threshold for alerts, configurable later |
| 2026-02-08 | preferred_username claim for audit trail | JWT claim used in AdjustStock, falls back to "system" |
| 2026-02-08 | Zero-stock entries for missing product IDs in batch query | Consumer-friendly: always returns entry per requested ID |
| 2026-02-08 | stockItemId as query param for ReleaseReservation | Aggregate must be loaded by its own ID, not reservation ID |
| 2026-02-08 | 1-minute cleanup interval for expired reservations | Balances responsiveness with DB load |
| 2026-02-08 | Seeded Random(42) for inventory data | Reproducible stock quantities across dev environments |
| 2026-02-08 | ~10/20/70 stock distribution (zero/low/normal) | Realistic test data covering edge cases |
| 2026-02-08 | Record<string, StockInfoDto> for stock levels prop | Simpler than Map for React props serialization |
| 2026-02-08 | Batch stock fetch after product list loads | Single API call for all products on page |
| 2026-02-08 | Refetch stock after adjustment (not optimistic) | Ensures accuracy from server state |
| 2026-02-08 | Stock badges at top-right of product image | Visible without dominating card layout |
| 2026-02-08 | Parallel stock fetch on product detail | Catalog SSR + Inventory client-side for clean module boundary |
| 2026-02-08 | isInStock defaults true while loading | Avoid flash of out-of-stock content during fetch |
| 2026-02-09 | Single AddConfigureEndpointsCallback for all middleware | DLQ, circuit breaker, retry, inbox outbox in one callback |
| 2026-02-09 | Middleware order: circuit breaker -> retry -> inbox outbox | Outermost to innermost for correct failure handling |
| 2026-02-09 | MassTransit package in ServiceDefaults | Required for DiagnosticHeaders.DefaultListenerName tracing |
| 2026-02-09 | Aspire AddAzureServiceBusClient for ServiceBusClient DI | Same connection as MassTransit, enables DLQ service |
| 2026-02-09 | Hardcoded known queue names in DLQ service | Extensible list, grows as consumers added |
| 2026-02-09 | Graceful ServiceBusException handling in DLQ service | Emulator may not fully support DLQ sub-queues |
| 2026-02-09 | Generic DomainEventFaultConsumer<T> for fault logging | Auto-discovered by MassTransit assembly scanning |
| 2026-02-09 | RequireAuthorization on messaging route group | Admin-only DLQ management access |
| 2026-02-09 | 30-second auto-refresh for DLQ admin page | Live monitoring without WebSocket complexity |
| 2026-02-09 | Purge requires queue selection | Prevent accidental mass deletion across all queues |
| 2026-02-09 | CartItem is regular class, not aggregate root | Owned by Cart aggregate, not independently accessible |
| 2026-02-09 | 30-day TTL on Cart, resets on modification | Balance cart persistence with cleanup of abandoned carts |
| 2026-02-09 | Max quantity 99 enforced in Cart and CartItem | Practical upper bound for e-commerce quantities |
| 2026-02-09 | Cookie-based buyer identity with 7-day MaxAge | Guest cart persistence without authentication requirement |
| 2026-02-09 | ExecuteDeleteAsync for cart expiration | More efficient bulk delete vs load-then-remove pattern |
| 2026-02-09 | useState pattern for QueryClient | Prevents SSR state leakage in Next.js App Router |
| 2026-02-09 | Single ["cart"] query key for all cart hooks | Cache consistency between useCart and useCartItemCount |
| 2026-02-09 | Optimistic mutations with snapshot rollback | Instant UI feedback, server reconciliation on settle |
| 2026-02-10 | OwnsOne for ShippingAddress mapping | Clearer column naming vs ComplexProperty |
| 2026-02-10 | RandomNumberGenerator for OrderNumber | Cryptographically secure MC-XXXXXX codes |
| 2026-02-10 | Status transitions with guard clauses | Prevent invalid state transitions in Order aggregate |
| 2026-02-10 | Flat $5.99 shipping + 8% tax | Simple calculation, configurable later |
| 2026-02-10 | PaymentCompleted/PaymentFailed in Contracts.cs | Shared location for CQRS and saga plans to reference |
| 2026-02-10 | BuyerIdentity reused from Cart module | Consistent buyer identification across cart and ordering |
| 2026-02-10 | ExistingDbContext for saga persistence | Saga state in same schema as orders, no extra DbContext |
| 2026-02-10 | Optimistic concurrency for saga state | MassTransit auto-retries on conflicts, better throughput |
| 2026-02-10 | JSON serialization for reservation ID map | Simple string serialization, no extra tables needed |
| 2026-02-10 | SetCompletedWhenFinalized for saga cleanup | Auto-removes completed saga rows, prevents table bloat |
| 2026-02-12 | Terminal status polling stop for order hooks | Delivered/Failed/Cancelled orders stop polling to reduce server load |
| 2026-02-12 | 20-second polling interval for order status | Balance responsiveness with server load for order processing |
| 2026-02-12 | Scoped query key hierarchy for orders | buyer vs admin query keys allow targeted cache invalidation |
| 2026-02-12 | Shared OrderListDto across buyer and admin queries | Reduce duplication, consistent response shape |
| 2026-02-12 | Item thumbnails capped at 3 per order card | Preview without loading all items |
| 2026-02-12 | Dashboard zero-fill for missing daily counts | Consistent 7-day chart data for frontend |
| 2026-02-12 | InvalidOperationException mapped to 400 Bad Request | Domain guard clause violations return proper HTTP status |
| 2026-02-12 | MarkAsFailed guard includes Shipped/Delivered | Prevent late-stage orders from reverting to Failed |
| 2026-02-12 | Status badge color mapping pattern for admin order views | Reusable Submitted=yellow, Confirmed=blue, Paid=green, Shipped=purple, Delivered=green, Failed=red |
| 2026-02-12 | 5-step customer lifecycle mapping in stepper | StockReserved mapped to Submitted; Failed/Cancelled shown as error on last step |
| 2026-02-12 | useParams for client-side dynamic route params | Client components use useParams hook instead of page props |
| 2026-02-12 | Orders icon in storefront header | ClipboardList icon + mobile menu link for order history discoverability |
| 2026-02-12 | Only Confirmed->Shipped and Shipped->Delivered DnD transitions | Matches backend UpdateOrderStatus which only accepts Shipped and Delivered |
| 2026-02-12 | 8px PointerSensor activation constraint | Prevents accidental drags when clicking card links |
| 2026-02-12 | Separate drag handle from card link | GripVertical button handles drag; card body is Link to detail page |

---

## Known Issues

| Issue | Severity | Phase to Address |
|-------|----------|------------------|
| NextAuth token refresh not implemented | MEDIUM | Phase 1 |
| .NET 10 is preview | LOW | Monitor for RTM |
| No solution file (MicroCommerce.sln) | LOW | Consider creating for IDE support |

---

## Session Continuity

Last session: 2026-02-12
Stopped at: Completed 08-05-PLAN.md (Admin Order Kanban Board)
Resume file: None

---

## Next Actions

1. **Begin Phase 9** - API Gateway
2. **Begin Phase 10** - Testing & Polish

---
*State file created: 2026-01-29*
*Updated: 2026-02-12 (08-05 complete, Phase 8 complete)*
