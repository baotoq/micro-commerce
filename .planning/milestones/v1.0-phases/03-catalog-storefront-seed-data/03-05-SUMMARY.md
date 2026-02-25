---
phase: 03-catalog-storefront-seed-data
plan: 05
subsystem: ui
tags: [react, next.js, search, filters, url-state, debounce, shadcn]

# Dependency graph
requires:
  - phase: 03-03
    provides: Product grid with infinite scroll and product cards
  - phase: 03-01
    provides: Sort support in catalog API (sortBy, sortDirection params)
provides:
  - Category filter chips with active state
  - Debounced search bar in header
  - Sort dropdown with 4 options
  - URL-synced filter state for shareable links
affects: [03-06, cart-domain]

# Tech tracking
tech-stack:
  added: []
  patterns:
    - URL search params as single source of truth for filter state
    - Debounced input with setTimeout/clearTimeout pattern
    - Server component reads searchParams, passes to client components

key-files:
  created:
    - code/MicroCommerce.Web/src/components/storefront/search-bar.tsx
    - code/MicroCommerce.Web/src/components/storefront/product-filters.tsx
  modified:
    - code/MicroCommerce.Web/src/components/storefront/header.tsx
    - code/MicroCommerce.Web/src/app/(storefront)/page.tsx

key-decisions:
  - "URL params as filter state source of truth for shareable links and browser navigation"
  - "300ms debounce on search input to reduce API calls while typing"
  - "Server component page reads searchParams prop, passes parsed values to client ProductGrid"

patterns-established:
  - "URL-synced filters: useSearchParams to read, router.replace to update, preserve other params"
  - "Debounced search: local state for immediate feedback, setTimeout for URL update"

# Metrics
duration: 2min
completed: 2026-02-07
---

# Phase 3 Plan 5: Search, Filter & Sort Controls Summary

**Debounced search bar in header, category chip filters, and sort dropdown all synced to URL search params driving product grid**

## Performance

- **Duration:** 2 min
- **Started:** 2026-02-07T12:20:46Z
- **Completed:** 2026-02-07T12:22:42Z
- **Tasks:** 2
- **Files modified:** 4

## Accomplishments
- Search bar with 300ms debounce in header (desktop center, mobile menu)
- Category filter chips fetched from API with "All" option and active state styling
- Sort dropdown with 4 options (Newest, Price Low/High, Name A-Z)
- All filter state stored in URL search params for shareable links
- Product grid resets and refetches when any filter changes

## Task Commits

Each task was committed atomically:

1. **Task 1: Create search bar with debounce and category/sort filter controls** - `7d22382c` (feat)
2. **Task 2: Wire filter state from URL into product grid** - `025c1a3f` (feat)

## Files Created/Modified
- `src/components/storefront/search-bar.tsx` - Debounced search input with clear button and URL sync
- `src/components/storefront/product-filters.tsx` - Category chips and sort dropdown with URL param management
- `src/components/storefront/header.tsx` - Updated to include SearchBar in desktop center and mobile menu
- `src/app/(storefront)/page.tsx` - Reads URL searchParams, parses sort, passes filters to ProductGrid

## Decisions Made
- Used URL search params as single source of truth for all filter state, enabling shareable links and browser back/forward navigation
- 300ms debounce on search to balance responsiveness with API call reduction
- Sort param encoded as single value ("price-asc", "name-asc") parsed into sortBy/sortDirection on the page
- "Newest" sort is the default and removes the sort param from URL for cleaner URLs

## Deviations from Plan

None - plan executed exactly as written.

## Issues Encountered
None

## User Setup Required
None - no external service configuration required.

## Next Phase Readiness
- Filter controls complete, ready for any remaining storefront polish in 03-06
- ProductGrid already handles filter prop changes with reset and refetch
- All API params (categoryId, search, sortBy, sortDirection) supported by existing backend

---
*Phase: 03-catalog-storefront-seed-data*
*Completed: 2026-02-07*
