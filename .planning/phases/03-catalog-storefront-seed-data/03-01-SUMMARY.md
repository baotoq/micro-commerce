---
phase: 03-catalog-storefront-seed-data
plan: 01
subsystem: api
tags: [sorting, seed-data, catalog, electronics, ef-core]

# Dependency graph
requires:
  - phase: 02-catalog-domain-admin-crud
    provides: Product/Category domain models, CatalogDbContext, GetProductsQuery
provides:
  - Sort support for products API (price, name, newest)
  - Catalog data seeder with ~50 electronics products across 8 categories
affects: [03-02 storefront API, 03-03 storefront UI, frontend product browsing]

# Tech tracking
tech-stack:
  added: []
  patterns:
    - "BackgroundService for data seeding with idempotency guard"
    - "Dynamic sorting via switch expression on query parameters"

key-files:
  created:
    - "code/MicroCommerce.ApiService/Features/Catalog/Infrastructure/CatalogDataSeeder.cs"
  modified:
    - "code/MicroCommerce.ApiService/Features/Catalog/Application/Queries/GetProducts/GetProductsQuery.cs"
    - "code/MicroCommerce.ApiService/Features/Catalog/Application/Queries/GetProducts/GetProductsQueryHandler.cs"
    - "code/MicroCommerce.ApiService/Features/Catalog/CatalogEndpoints.cs"
    - "code/MicroCommerce.ApiService/Program.cs"

key-decisions:
  - "BackgroundService for seeding instead of EF UseAsyncSeeding for simplicity"
  - "Development-only seeding with idempotency guard on Categories table"

patterns-established:
  - "Data seeding pattern: BackgroundService with IsDevelopment check and empty-table guard"

# Metrics
duration: 2min
completed: 2026-02-07
---

# Phase 3 Plan 1: Sort Support & Seed Data Summary

**Dynamic sort parameters (price, name, newest) on products API and CatalogDataSeeder with 50 published electronics products across 8 categories**

## Performance

- **Duration:** 2 min
- **Started:** 2026-02-07T08:55:40Z
- **Completed:** 2026-02-07T08:57:40Z
- **Tasks:** 2
- **Files modified:** 5

## Accomplishments
- GetProductsQuery now accepts SortBy and SortDirection parameters with backward-compatible defaults
- Handler applies dynamic ordering for price, name, newest, and default (newest) cases
- CatalogDataSeeder creates 8 categories and 50 published products using domain factory methods
- Seeder is idempotent (checks Categories table emptiness) and Development-only

## Task Commits

Each task was committed atomically:

1. **Task 1: Add sort parameters to GetProductsQuery** - `4db9e1eb` (feat)
2. **Task 2: Create seed data with ~50 electronics products** - `fffac359` (feat)

## Files Created/Modified
- `code/MicroCommerce.ApiService/Features/Catalog/Application/Queries/GetProducts/GetProductsQuery.cs` - Added SortBy and SortDirection parameters
- `code/MicroCommerce.ApiService/Features/Catalog/Application/Queries/GetProducts/GetProductsQueryHandler.cs` - Dynamic sorting logic via switch expression
- `code/MicroCommerce.ApiService/Features/Catalog/CatalogEndpoints.cs` - Bound sortBy and sortDirection query parameters
- `code/MicroCommerce.ApiService/Features/Catalog/Infrastructure/CatalogDataSeeder.cs` - BackgroundService seeding 8 categories and 50 products
- `code/MicroCommerce.ApiService/Program.cs` - Registered CatalogDataSeeder hosted service

## Decisions Made
- Used BackgroundService pattern for seeding (simpler than EF UseAsyncSeeding, works well with Aspire)
- Added 2-second delay before seeding to allow migrations to complete
- Used placehold.co URLs with URL-encoded product names for placeholder images
- SKU prefixes match category abbreviations (LAP, PHN, TAB, AUD, ACC, MON, GAM, WRB)

## Deviations from Plan

None - plan executed exactly as written.

## Issues Encountered
- AppHost has pre-existing build errors (AddAzureStorage/AddBlobs API changes) unrelated to this plan; ApiService builds successfully on its own

## User Setup Required

None - no external service configuration required.

## Next Phase Readiness
- Sort support ready for storefront frontend to offer sort dropdowns
- 50 seeded products available for browsing, filtering, and pagination testing
- Categories populated for category-based filtering

---
*Phase: 03-catalog-storefront-seed-data*
*Completed: 2026-02-07*
