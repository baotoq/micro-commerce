---
phase: 13-wishlists-saved-items
plan: 03
subsystem: frontend
tags: [react, tanstack-query, optimistic-ui, nextjs, typescript]

# Dependency graph
requires:
  - phase: 13-wishlists-saved-items
    plan: 02
    provides: "5 authenticated REST API endpoints at /api/wishlist"
provides:
  - "WishlistItemDto type and 5 API functions in api.ts"
  - "TanStack Query hooks with optimistic UI (useToggleWishlist)"
  - "WishlistToggleButton with heart icon toggle (outlined -> filled red)"
  - "Complete wishlist page at /wishlist with grid, empty state, loading skeleton"
  - "Wishlist integration in header (count badge), product cards, product detail, account sidebar"
affects: [phase-complete]

# Tech tracking
tech-stack:
  added: []
  patterns:
    - "Optimistic UI with Set-based membership checking for instant heart icon feedback"
    - "TanStack Query hooks with onMutate/onError/onSettled for cache updates"
    - "Guest redirect to login via signIn('keycloak') for protected features"

key-files:
  created:
    - src/MicroCommerce.Web/src/hooks/use-wishlist.ts
    - src/MicroCommerce.Web/src/components/wishlist/wishlist-toggle-button.tsx
    - src/MicroCommerce.Web/src/components/wishlist/wishlist-item-card.tsx
    - src/MicroCommerce.Web/src/components/wishlist/wishlist-grid.tsx
    - src/MicroCommerce.Web/src/components/wishlist/wishlist-empty-state.tsx
    - src/MicroCommerce.Web/src/app/(storefront)/wishlist/page.tsx
  modified:
    - src/MicroCommerce.Web/src/lib/api.ts
    - src/MicroCommerce.Web/src/components/storefront/header.tsx
    - src/MicroCommerce.Web/src/components/storefront/product-card.tsx
    - src/MicroCommerce.Web/src/components/storefront/product-detail.tsx
    - src/MicroCommerce.Web/src/components/account/account-sidebar.tsx

key-decisions:
  - "Set-based membership checking: useWishlistProductIds returns Set<string> for O(1) lookup performance when checking if product is in wishlist"
  - "Optimistic UI: heart icon toggles immediately on click, with rollback on error for instant feedback"
  - "Guest redirect: clicking heart when not logged in redirects to login via signIn('keycloak')"
  - "Heart icon positioning: top-left on product cards (stock badge at top-right), next to product name on detail page"
  - "Out-of-stock handling: dimmed opacity-60, grayscale image, disabled button, Out of Stock badge"
  - "Toast notification: 'Added to cart' toast when adding from wishlist, always quantity 1"
  - "Wishlist page requires authentication: shows sign-in prompt if not logged in"

patterns-established:
  - "Wishlist hooks follow Reviews pattern with session-based auth and TanStack Query"
  - "Optimistic UI pattern for instant feedback on user interactions"
  - "Heart icon toggle pattern (outlined -> filled red) for wishlist state visualization"

# Metrics
duration: 3min
completed: 2026-02-13
---

# Phase 13 Plan 03: Wishlists Frontend Integration Summary

**Complete user-facing wishlist experience with heart icon toggle on products, wishlist page, header count badge, and optimistic UI**

## Performance

- **Duration:** 3 minutes
- **Started:** 2026-02-13T13:40:51Z
- **Completed:** 2026-02-13T13:44:50Z
- **Tasks:** 2
- **Files modified:** 11

## Accomplishments
- Added WishlistItemDto type to api.ts
- Created 5 API functions (getUserWishlist, getWishlistCount, getWishlistProductIds, addToWishlist, removeFromWishlist) with Bearer token auth
- Created use-wishlist.ts with 4 TanStack Query hooks (useWishlistProductIds, useWishlistCount, useUserWishlist, useToggleWishlist)
- Implemented optimistic UI in useToggleWishlist: Set-based membership for O(1) lookup, instant heart toggle with rollback on error
- Created WishlistToggleButton component: heart icon outlined -> filled red, redirects guests to login
- Created WishlistItemCard with add-to-cart (quantity 1, toast notification), out-of-stock handling (dimmed, grayscale, disabled button, badge)
- Created WishlistGrid with responsive 1/2/3/4 column layout and loading skeleton
- Created WishlistEmptyState component with Browse Products CTA
- Created /wishlist page route with authentication check, loading state, empty state, and grid view
- Integrated heart icon with count badge into header (desktop + mobile menu) between orders and cart
- Integrated WishlistToggleButton into product cards (top-left of image, stock badge at top-right)
- Integrated WishlistToggleButton into product detail page (next to product name with hover background)
- Added Wishlist link to account sidebar (between Orders and Security)

## Task Commits

Each task was committed atomically:

1. **Task 1: Create wishlist API functions, hooks, and components** - `4ccc60c8` (feat)
2. **Task 2: Integrate wishlist into header, product cards, product detail, and account sidebar** - `68c141b5` (feat)

## Files Created/Modified
- `src/MicroCommerce.Web/src/lib/api.ts` - Added WishlistItemDto type and 5 API functions with Bearer token auth
- `src/MicroCommerce.Web/src/hooks/use-wishlist.ts` - TanStack Query hooks with optimistic UI and Set-based membership checking
- `src/MicroCommerce.Web/src/components/wishlist/wishlist-toggle-button.tsx` - Heart icon toggle button (outlined -> filled red, guest redirect)
- `src/MicroCommerce.Web/src/components/wishlist/wishlist-item-card.tsx` - Wishlist item card with add-to-cart, out-of-stock handling, rating display
- `src/MicroCommerce.Web/src/components/wishlist/wishlist-grid.tsx` - Responsive grid and loading skeleton
- `src/MicroCommerce.Web/src/components/wishlist/wishlist-empty-state.tsx` - Empty state with Browse Products CTA
- `src/MicroCommerce.Web/src/app/(storefront)/wishlist/page.tsx` - Wishlist page route with auth check, loading, empty, and grid states
- `src/MicroCommerce.Web/src/components/storefront/header.tsx` - Added Heart icon with count badge (desktop + mobile)
- `src/MicroCommerce.Web/src/components/storefront/product-card.tsx` - Added WishlistToggleButton at top-left of image
- `src/MicroCommerce.Web/src/components/storefront/product-detail.tsx` - Added WishlistToggleButton next to product name
- `src/MicroCommerce.Web/src/components/account/account-sidebar.tsx` - Added Wishlist link between Orders and Security

## Decisions Made
- **Set-based membership checking:** useWishlistProductIds returns `Set<string>` instead of array for O(1) lookup performance when checking if a product is in the wishlist. Frontend loads product IDs once and checks membership efficiently.
- **Optimistic UI:** Heart icon toggles immediately on click via optimistic cache updates. On error, rollback to previous state. This provides instant visual feedback while the backend request is in flight.
- **Guest redirect to login:** When unauthenticated user clicks heart icon, redirect to login via `signIn("keycloak")`. No toast message, just redirect per existing auth pattern.
- **Heart icon positioning:** Top-left on product cards (stock badge already occupies top-right), next to product name on detail page with subtle hover background.
- **Out-of-stock handling on wishlist:** Items with availableQuantity=0 are dimmed (opacity-60), have grayscale images, show "Out of Stock" badge, and have disabled "Out of Stock" button text.
- **Add to cart from wishlist:** Always adds quantity 1 per plan requirement. Shows toast notification "Added to cart" on success.
- **Wishlist page auth:** Requires authentication. Shows sign-in prompt with button if not logged in, instead of silently redirecting.

## Deviations from Plan

None - plan executed exactly as written.

## Issues Encountered

None

## User Setup Required

None - wishlist is ready to use once user is authenticated.

## Next Phase Readiness

Phase 13 (Wishlists & Saved Items) complete:
- Backend: Domain entities, CQRS handlers, REST API with idempotent commands
- Frontend: API layer, TanStack Query hooks, UI components, page route, storefront integration
- Optimistic UI provides instant feedback
- Cross-context batch queries prevent N+1 problems
- All 5 phase success criteria achieved:
  1. Heart icon toggle on product cards and detail page ✓
  2. Header shows wishlist count badge ✓
  3. Wishlist page shows saved products with add-to-cart and out-of-stock handling ✓
  4. Empty state displays when wishlist is empty ✓
  5. Account sidebar links to wishlist ✓

No blockers or concerns. Ready for Phase 14 (Admin Dashboard & Analytics).

## Self-Check: PASSED

All created files verified:
- FOUND: src/MicroCommerce.Web/src/lib/api.ts (updated with wishlist types and functions)
- FOUND: src/MicroCommerce.Web/src/hooks/use-wishlist.ts
- FOUND: src/MicroCommerce.Web/src/components/wishlist/wishlist-toggle-button.tsx
- FOUND: src/MicroCommerce.Web/src/components/wishlist/wishlist-item-card.tsx
- FOUND: src/MicroCommerce.Web/src/components/wishlist/wishlist-grid.tsx
- FOUND: src/MicroCommerce.Web/src/components/wishlist/wishlist-empty-state.tsx
- FOUND: src/MicroCommerce.Web/src/app/(storefront)/wishlist/page.tsx
- FOUND: src/MicroCommerce.Web/src/components/storefront/header.tsx (updated)
- FOUND: src/MicroCommerce.Web/src/components/storefront/product-card.tsx (updated)
- FOUND: src/MicroCommerce.Web/src/components/storefront/product-detail.tsx (updated)
- FOUND: src/MicroCommerce.Web/src/components/account/account-sidebar.tsx (updated)

All commits verified:
- FOUND: 4ccc60c8 (Task 1)
- FOUND: 68c141b5 (Task 2)

---
*Phase: 13-wishlists-saved-items*
*Completed: 2026-02-13*
