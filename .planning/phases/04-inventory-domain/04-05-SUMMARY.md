---
phase: 04-inventory-domain
plan: 05
subsystem: ui
tags: [react, next.js, storefront, stock-status, inventory, badges]

# Dependency graph
requires:
  - phase: 04-02
    provides: Inventory CQRS API endpoints (GET /api/inventory/stock, GET /api/inventory/stock/{productId})
  - phase: 04-04
    provides: Inventory API client functions (getStockLevels, getStockByProductId) and StockInfoDto type
  - phase: 03-03
    provides: Product grid with infinite scroll and ProductCard component
  - phase: 03-04
    provides: Product detail page with ProductDetail component
provides:
  - Stock status badges on storefront product cards (In Stock, Only X left!, Out of Stock)
  - Stock-aware product detail page with conditional Add to Cart
  - Visual treatment for out-of-stock products in grid
affects: [06-cart-domain]

# Tech tracking
tech-stack:
  added: []
  patterns:
    - "Client-side stock fetching: Catalog SSR + Inventory client-side for clean module boundary"
    - "Batch stock fetch on grid load with Map<productId, StockInfoDto> for efficient lookups"
    - "Threshold-based stock messaging: >10 In Stock, 1-10 Only X left!, 0 Out of Stock"

key-files:
  modified:
    - code/MicroCommerce.Web/src/components/storefront/product-card.tsx
    - code/MicroCommerce.Web/src/components/storefront/product-grid.tsx
    - code/MicroCommerce.Web/src/components/storefront/product-detail.tsx

key-decisions:
  - "Stock badges positioned at top-right of product image for visibility without dominating card"
  - "Out-of-stock cards use reduced opacity (0.6) and grayscale image rather than full card grayscale"
  - "Stock fetched in parallel with product data on detail page for faster perceived load"
  - "isInStock defaults to true while stock is loading to avoid flash of out-of-stock state"

patterns-established:
  - "Inventory data always fetched client-side, separate from Catalog SSR data"
  - "Stock badge component (StockBadge) reusable across card and detail contexts"

# Metrics
duration: 2min
completed: 2026-02-08
---

# Phase 4 Plan 5: Storefront Stock Status Display Summary

**Stock status badges on product cards and detail page with threshold-based messaging and conditional Add to Cart visibility**

## Performance

- **Duration:** 2 min
- **Started:** 2026-02-07T18:20:51Z
- **Completed:** 2026-02-07T18:23:00Z
- **Tasks:** 2
- **Files modified:** 3

## Accomplishments
- Product cards display context-appropriate stock badges: green "In Stock", amber "Only X left!", red "Out of Stock"
- Out-of-stock products in grid have reduced opacity and grayscale image treatment while remaining clickable
- Product detail page shows stock status prominently near price with icons
- Add to Cart button hidden when product is out of stock, replaced with "out of stock" notice
- Stock data fetched via separate Inventory API maintaining clean module boundary

## Task Commits

Each task was committed atomically:

1. **Task 1: Product Grid and Cards with Stock Badges** - `64b0d56b` (feat)
2. **Task 2: Product Detail Page Stock Display** - `6dfb7c61` (feat)

## Files Created/Modified
- `code/MicroCommerce.Web/src/components/storefront/product-card.tsx` - Added StockBadge component, stockInfo prop, out-of-stock visual treatment
- `code/MicroCommerce.Web/src/components/storefront/product-grid.tsx` - Added batch stock fetching via getStockLevels, stockMap state, passes stockInfo to cards
- `code/MicroCommerce.Web/src/components/storefront/product-detail.tsx` - Added StockStatus component, parallel stock fetch, conditional Add to Cart

## Decisions Made
- Stock badges positioned at top-right of product image for visibility without dominating card layout
- Out-of-stock cards use reduced opacity + grayscale on image only (not entire card) for subtle treatment
- Stock fetched in parallel with product data on detail page (two concurrent useEffect fetches)
- When stock is still loading, isInStock defaults to true to avoid flash of out-of-stock content
- Non-critical stock fetch failures are silently handled -- cards render without badges, detail shows default state

## Deviations from Plan

None - plan executed exactly as written.

## Issues Encountered
None

## User Setup Required
None - no external service configuration required.

## Next Phase Readiness
- Storefront stock display complete, ready for Cart domain (Phase 6)
- Add to Cart button is placeholder (shows toast) -- will be wired to Cart API in Phase 6
- Stock badges and conditional display patterns established for reuse

---
*Phase: 04-inventory-domain*
*Completed: 2026-02-08*
