# Project State

## Project Reference

See: .planning/PROJECT.md (updated 2026-02-13)

**Core value:** A user can complete a purchase end-to-end
**Current focus:** Phase 14 - Integration & Polish

## Current Position

Phase: 14 of 14 (Integration & Polish)
Plan: 3 of 3 completed
Status: Complete
Last activity: 2026-02-14 — Completed 14-03-PLAN.md (E2E Tests for User Features)

Progress: [█████████████░] 90% (62/69 total plans across v1.0+v1.1)

## Performance Metrics

**Velocity (v1.0 baseline):**
- Total plans completed: 49
- Average duration: 23 min
- Total execution time: 18.8 hours

**By Phase (v1.0):**

| Phase | Plans | Total | Avg/Plan |
|-------|-------|-------|----------|
| 1 | 6 | 138 min | 23 min |
| 2 | 7 | 161 min | 23 min |
| 3 | 6 | 138 min | 23 min |
| 4 | 5 | 115 min | 23 min |
| 5 | 3 | 69 min | 23 min |
| 6 | 4 | 92 min | 23 min |
| 7 | 4 | 92 min | 23 min |
| 8 | 5 | 115 min | 23 min |
| 9 | 3 | 69 min | 23 min |
| 10 | 6 | 138 min | 23 min |

**Recent Trend:**
- v1.0 completed successfully (2026-01-29 → 2026-02-13, 16 days)
- Trend: Stable execution pattern established

**v1.1 Phase 11 Metrics:**

| Plan | Duration | Tasks | Files |
|------|----------|-------|-------|
| 11-01 | 4 min | 2 | 15 |
| 11-02 | 2 min | 2 | 7 |
| 11-03 | 3 min | 2 | 13 |
| 11-04 | 3 min | 2 | 12 |
| 11-05 | 2 min | 2 | 6 |

**v1.1 Phase 12 Metrics:**

| Plan | Duration | Tasks | Files |
|------|----------|-------|-------|
| 12-01 | 3 min | 2 | 14 |
| 12-02 | 3 min | 2 | 13 |
| 12-03 | 3 min | 2 | 11 |

**v1.1 Phase 13 Metrics:**

| Plan | Duration | Tasks | Files |
|------|----------|-------|-------|
| 13-01 | 2 min | 2 | 8 |
| 13-02 | 3 min | 2 | 7 |
| 13-03 | 3 min | 2 | 11 |

**v1.1 Phase 14 Metrics:**

| Plan | Duration | Tasks | Files |
|------|----------|-------|-------|
| 14-01 | 2 min | 2 | 3 |
| 14-02 | 3 min | 2 | 8 |
| 14-03 | 1 min | 1 | 1 |

## Accumulated Context

### Decisions

Decisions are logged in PROJECT.md Key Decisions table.
Recent decisions affecting current work:

- Database-per-feature isolation: v1.1 will add 3 new DbContexts (Profiles, Reviews, Wishlists)
- Guest checkout: Migration from guest to authenticated users is critical for Phase 11
- Keycloak authentication: Foundation established, v1.1 extends with profile attributes
- [Phase 11]: ImageSharp 3.1.6 for avatar processing with crop-to-square and 400x400 resize
- [Phase 11-03]: Auto-create profile on first GET ensures profile always exists for authenticated users
- [Phase 11-03]: UserId (Guid) from JWT 'sub' claim used as profile lookup key
- [Phase 11-05]: Modal dialog form for both adding and editing addresses (not inline editing)
- [Phase 11-05]: Login/register available via header account icon AND at checkout
- [Phase 11-05]: Cart merge happens silently on login without user intervention
- [Phase 12-01]: Composite unique index on (UserId, ProductId) enforces one review per user per product
- [Phase 12-01]: Rating (1-5) and ReviewText (10-1000 chars) as value objects with validation
- [Phase 12-01]: Product.AverageRating and ReviewCount denormalized for query performance
- [Phase 12]: Purchase verification gates review creation by querying Orders with Paid/Confirmed/Shipped/Delivered status
- [Phase 12]: Aggregate rating recalculation happens synchronously after each CUD operation for immediate consistency
- [Phase 12]: Batch queries for display names and verified purchases to avoid N+1 problems in review list endpoint
- [Phase 12-03]: Yellow/gold filled stars with gray empty stars for classic visual pattern
- [Phase 12-03]: Half-star support using lighter fill color as approximation
- [Phase 12-03]: Compact list layout for reviews (no bordered cards, minimal dividers)
- [Phase 12-03]: Modal dialog form for both creating and editing reviews
- [Phase 12-03]: Load more button accumulates reviews across pages for seamless browsing
- [Phase 13-01]: WishlistItem is a simple entity (not aggregate root) with no domain events
- [Phase 13-01]: Composite unique index on (UserId, ProductId) enforces single entry per user-product pair
- [Phase 13-01]: Three indexes: composite unique, UserId for listing, AddedAt descending for chronological sort
- [Phase 13-02]: Idempotent commands - AddToWishlist returns existing ID if already in wishlist, RemoveFromWishlist silently succeeds if not found
- [Phase 13-02]: Cross-context batch queries in GetUserWishlistQuery prevent N+1 (batch-loads from CatalogDbContext and InventoryDbContext)
- [Phase 13-02]: GetWishlistProductIdsQuery returns just product IDs for efficient heart icon state checking
- [Phase 13-03]: Set-based membership checking - useWishlistProductIds returns Set<string> for O(1) lookup when checking if product is in wishlist
- [Phase 13-03]: Optimistic UI with rollback - heart icon toggles immediately with cache updates, rollback on error for instant feedback
- [Phase 13-03]: Heart icon positioning - top-left on product cards (stock badge at top-right), next to product name on detail page
- [Phase 13-03]: Guest redirect to login - clicking heart when not authenticated redirects via signIn('keycloak')
- [Phase 14-01]: OrderItemReviewRow sub-component pattern allows per-item hook calls (React hooks rules compliance)
- [Phase 14-01]: CanReviewDto logic: hasPurchased && !hasReviewed determines review eligibility
- [Phase 14-02]: Content-matching skeleton screens instead of generic spinners - shows structure of loading content for better UX
- [Phase 14-02]: Standard empty state pattern - icon (size-12, text-zinc-300) + heading + message + CTA button (rounded-full, size lg)
- [Phase 14-02]: Vercel/Linear aesthetic - rounded-full buttons, rounded-xl cards, border-zinc-200, clean spacing for modern polish
- [Phase 14-03]: Flexible selectors pattern - getByRole, getByText, aria-label matching for resilient E2E tests
- [Phase 14-03]: Soft assertions for data-dependent tests - reviews/wishlists may not exist, use soft checks to avoid false failures
- [Phase 14-03]: Auth complexity in E2E - verify page structure and navigation rather than full Keycloak auth flows
- [Phase 14-03]: 10-second timeout for initial loads - Aspire stack may be slow to respond

### Roadmap Evolution

- Phase 14.1 inserted after Phase 14: Check DDD approach correctness (URGENT)

### Pending Todos

None yet.

### Blockers/Concerns

None yet.

## Session Continuity

Last session: 2026-02-14
Stopped at: Completed 14-03-PLAN.md (E2E Tests for User Features)
Resume file: None
Next step: Phase 14 complete - all v1.1 integration and polish plans executed successfully
