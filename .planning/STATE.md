# Project State

**Project:** MicroCommerce
**Current Phase:** Phase 4 - Inventory Domain
**Status:** In Progress
**Last Updated:** 2026-02-08

---

## Current Position

Phase: 4 of 10 (Inventory Domain)
Plan: 3 of ? in current phase
Status: In progress
Last activity: 2026-02-08 - Completed 04-03-PLAN.md (Background Services & Data Seeder)

Progress: █████████░░░░░░░░░░░ 36%

---

## Progress Overview

| Phase | Name | Status | Plans |
|-------|------|--------|-------|
| 1 | Foundation & Project Structure | COMPLETE | 6/6 |
| 2 | Catalog Domain & Admin CRUD | COMPLETE | 7/7 |
| 3 | Catalog Storefront & Seed Data | IN PROGRESS | 5/6 |
| 4 | Inventory Domain | IN PROGRESS | 3/? |
| 5 | Event Bus Infrastructure | NOT STARTED | 0/? |
| 6 | Cart Domain | NOT STARTED | 0/? |
| 7 | Ordering Domain & Checkout | NOT STARTED | 0/? |
| 8 | Order History & Management | NOT STARTED | 0/? |
| 9 | API Gateway | NOT STARTED | 0/? |
| 10 | Testing & Polish | NOT STARTED | 0/? |

**Phases Completed:** 2/10
**Plans Completed (Phase 4):** 3/?

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

---

## Known Issues

| Issue | Severity | Phase to Address |
|-------|----------|------------------|
| NextAuth token refresh not implemented | MEDIUM | Phase 1 |
| .NET 10 is preview | LOW | Monitor for RTM |
| No solution file (MicroCommerce.sln) | LOW | Consider creating for IDE support |

---

## Session Continuity

Last session: 2026-02-08
Stopped at: Completed 04-03-PLAN.md (Background Services & Data Seeder)
Resume file: None

---

## Next Actions

1. **Continue Phase 4** - Execute remaining inventory domain plans (commands, queries, API)
2. Complete Phase 4 and begin Phase 5 (Event Bus Infrastructure)

---
*State file created: 2026-01-29*
*Updated: 2026-02-08*
