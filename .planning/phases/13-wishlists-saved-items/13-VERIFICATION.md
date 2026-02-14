---
phase: 13-wishlists-saved-items
verified: 2026-02-13T13:49:29Z
status: passed
score: 5/5 truths verified
re_verification: false
---

# Phase 13: Wishlists & Saved Items Verification Report

**Phase Goal:** Users can save products to a persistent wishlist and move items to cart
**Verified:** 2026-02-13T13:49:29Z
**Status:** passed
**Re-verification:** No - initial verification

## Goal Achievement

### Observable Truths

| # | Truth | Status | Evidence |
|---|-------|--------|----------|
| 1 | User can add and remove products from their wishlist | ✓ VERIFIED | AddToWishlistCommand (idempotent upsert) and RemoveFromWishlistCommand exist. WishlistToggleButton component wired to product cards and detail page with optimistic UI. |
| 2 | User can view wishlist page showing all saved products | ✓ VERIFIED | /wishlist page route exists with WishlistGrid showing products. GetUserWishlistQuery with cross-context batch lookups for product details and stock info. |
| 3 | User can move wishlist items to cart with proper stock validation | ✓ VERIFIED | WishlistItemCard has Add to Cart button calling useAddToCart with quantity 1. Out-of-stock items dimmed with disabled button showing "Out of Stock". |
| 4 | Product cards display heart icon indicator when product is in wishlist | ✓ VERIFIED | WishlistToggleButton integrated into product-card.tsx (top-left of image) and product-detail.tsx. Heart filled red when in wishlist, outlined when not. useToggleWishlist with Set-based membership checking. |
| 5 | Wishlist persists across sessions and devices | ✓ VERIFIED | Backend stores wishlist in PostgreSQL with UserId. Composite unique index on (UserId, ProductId) ensures data integrity. All endpoints require authentication. |

**Score:** 5/5 truths verified

### Required Artifacts - Plan 13-01 (Backend Domain)

| Artifact | Expected | Status | Details |
|----------|----------|--------|---------|
| `src/MicroCommerce.ApiService/Features/Wishlists/Domain/Entities/WishlistItem.cs` | WishlistItem entity with Create factory method | ✓ VERIFIED | 57 lines, has Create factory method, UserId, ProductId, AddedAt properties, xmin concurrency |
| `src/MicroCommerce.ApiService/Features/Wishlists/Domain/ValueObjects/WishlistItemId.cs` | Strongly-typed ID with New() method | ✓ VERIFIED | 8 lines, StronglyTypedId record with New() factory |
| `src/MicroCommerce.ApiService/Features/Wishlists/Infrastructure/WishlistsDbContext.cs` | DbContext with 'wishlists' schema | ✓ VERIFIED | 27 lines, HasDefaultSchema("wishlists"), DbSet<WishlistItem> |
| `src/MicroCommerce.ApiService/Features/Wishlists/Infrastructure/Configurations/WishlistItemConfiguration.cs` | EF Core configuration with composite unique index | ✓ VERIFIED | 47 lines, composite unique index on (UserId, ProductId), indexes on UserId and AddedAt descending |
| `src/MicroCommerce.ApiService/Features/Wishlists/Infrastructure/Migrations/20260213133155_InitialWishlists.cs` | Migration creating WishlistItems table | ✓ VERIFIED | Migration exists, creates 'wishlists' schema, WishlistItems table with indexes |

### Required Artifacts - Plan 13-02 (Backend CQRS)

| Artifact | Expected | Status | Details |
|----------|----------|--------|---------|
| `src/MicroCommerce.ApiService/Features/Wishlists/Application/Commands/AddToWishlist/AddToWishlistCommand.cs` | Idempotent add command | ✓ VERIFIED | 42 lines, checks for existing item, returns existing ID if found (idempotent) |
| `src/MicroCommerce.ApiService/Features/Wishlists/Application/Commands/RemoveFromWishlist/RemoveFromWishlistCommand.cs` | Idempotent remove command | ✓ VERIFIED | Handler checks for null and returns (idempotent) |
| `src/MicroCommerce.ApiService/Features/Wishlists/Application/Queries/GetUserWishlist/GetUserWishlistQuery.cs` | Full wishlist query with product details and stock | ✓ VERIFIED | 101 lines, cross-context batch queries to CatalogDbContext and InventoryDbContext, filters deleted products |
| `src/MicroCommerce.ApiService/Features/Wishlists/Application/Queries/GetWishlistProductIds/GetWishlistProductIdsQuery.cs` | Returns list of product IDs for heart icon state | ✓ VERIFIED | Query returns List<Guid> of ProductIds for efficient membership checking |
| `src/MicroCommerce.ApiService/Features/Wishlists/WishlistsEndpoints.cs` | 5 REST API endpoints requiring authentication | ✓ VERIFIED | 142 lines, 5 endpoints (GET list, GET count, GET product-ids, POST add, DELETE remove), RequireAuthorization() on route group |

### Required Artifacts - Plan 13-03 (Frontend)

| Artifact | Expected | Status | Details |
|----------|----------|--------|---------|
| `src/MicroCommerce.Web/src/hooks/use-wishlist.ts` | TanStack Query hooks with optimistic UI | ✓ VERIFIED | 139 lines, 4 hooks: useWishlistProductIds (returns Set<string>), useWishlistCount, useUserWishlist, useToggleWishlist with optimistic updates |
| `src/MicroCommerce.Web/src/components/wishlist/wishlist-toggle-button.tsx` | Heart icon toggle with optimistic updates | ✓ VERIFIED | 45 lines, Heart icon outlined -> filled red, redirects guests to login, disabled while isPending |
| `src/MicroCommerce.Web/src/app/(storefront)/wishlist/page.tsx` | Wishlist page route | ✓ VERIFIED | 54 lines, auth check, loading skeleton, empty state, grid view |
| `src/MicroCommerce.Web/src/components/wishlist/wishlist-grid.tsx` | Grid of wishlist item cards | ✓ VERIFIED | 37 lines, responsive grid (1/2/3/4 columns), skeleton variant |
| `src/MicroCommerce.Web/src/components/wishlist/wishlist-item-card.tsx` | Wishlist item card with add-to-cart | ✓ VERIFIED | 130 lines, out-of-stock handling (dimmed, grayscale, disabled button, badge), toast notification, quantity 1 |
| `src/MicroCommerce.Web/src/components/wishlist/wishlist-empty-state.tsx` | Empty state component | ✓ VERIFIED | 23 lines, "Your wishlist is empty" message, Browse Products CTA |
| `src/MicroCommerce.Web/src/lib/api.ts` | WishlistItemDto type and 5 API functions | ✓ VERIFIED | WishlistItemDto interface defined, getUserWishlist, getWishlistCount, getWishlistProductIds, addToWishlist, removeFromWishlist all with Bearer token auth |

### Key Link Verification

| From | To | Via | Status | Details |
|------|----|----|--------|---------|
| Program.cs | WishlistsDbContext | AddNpgsqlDbContext registration | ✓ WIRED | Line 76: `builder.AddNpgsqlDbContext<WishlistsDbContext>("appdb")` |
| WishlistsEndpoints.cs | Program.cs | MapWishlistsEndpoints() | ✓ WIRED | Line 257: `app.MapWishlistsEndpoints()` |
| GetUserWishlistQuery | CatalogDbContext | Batch product lookup | ✓ WIRED | Handler injects CatalogDbContext, batch lookup at line 61 |
| GetUserWishlistQuery | InventoryDbContext | Batch stock lookup | ✓ WIRED | Handler injects InventoryDbContext, batch lookup at line 66 |
| use-wishlist.ts | /api/wishlist | API functions from api.ts | ✓ WIRED | Import at line 11, calls getUserWishlist, getWishlistCount, etc. |
| product-card.tsx | WishlistToggleButton | Heart icon overlay on image | ✓ WIRED | Import at line 12, rendered at line 88 (top-left of image) |
| header.tsx | /wishlist | Heart icon link with count badge | ✓ WIRED | Import useWishlistCount at line 10, link at line 88, mobile link at line 158 |
| account-sidebar.tsx | /wishlist | Wishlist link | ✓ WIRED | "Wishlist" entry at lines 24-25 with href "/wishlist" |

### Requirements Coverage

| Requirement | Status | Evidence |
|-------------|--------|----------|
| WISH-01: User can add a product to their wishlist | ✓ SATISFIED | AddToWishlistCommand + WishlistToggleButton on product cards and detail page |
| WISH-02: User can remove a product from their wishlist | ✓ SATISFIED | RemoveFromWishlistCommand + WishlistToggleButton toggle behavior |
| WISH-03: User can view their wishlist page with all saved products | ✓ SATISFIED | /wishlist page route + GetUserWishlistQuery with cross-context batch lookups |
| WISH-04: User can move a wishlist item to cart | ✓ SATISFIED | WishlistItemCard with Add to Cart button calling useAddToCart |
| WISH-05: Product cards show wishlist indicator (heart icon) when saved | ✓ SATISFIED | WishlistToggleButton integrated into product-card.tsx and product-detail.tsx, heart filled red when in wishlist |

### Anti-Patterns Found

| File | Line | Pattern | Severity | Impact |
|------|------|---------|----------|--------|
| _None found_ | - | - | - | - |

**Anti-pattern scan results:**
- No TODO/FIXME/HACK/PLACEHOLDER comments found in key files
- No empty implementations (return null, return {}, return [])
- No console.log-only implementations
- All commands and queries have substantive implementations
- Cross-context batch queries prevent N+1 problems
- Idempotent commands handle duplicates gracefully

### Human Verification Required

#### 1. Visual Heart Icon Toggle

**Test:** As authenticated user, click heart icon on any product card or product detail page
**Expected:** 
- Heart icon should toggle from outlined to filled red when adding to wishlist
- Heart icon should toggle from filled red to outlined when removing from wishlist
- Toggle should be instant (optimistic UI) with no visible delay
- If network error occurs, heart should revert to previous state

**Why human:** Visual feedback and animation smoothness require human observation

#### 2. Guest Redirect to Login

**Test:** As unauthenticated user, click heart icon on any product card
**Expected:**
- Should redirect to Keycloak login page
- After login, should return to previous page
- After login, clicking heart again should add to wishlist

**Why human:** Full authentication flow requires human navigation and form interaction

#### 3. Wishlist Persistence Across Sessions

**Test:** 
1. Log in, add 3 products to wishlist
2. Log out
3. Log in again from a different browser/device
4. Navigate to /wishlist

**Expected:**
- All 3 products should still be in wishlist
- Heart icons on those products should be filled red

**Why human:** Cross-device persistence and session management require manual testing

#### 4. Out-of-Stock Handling on Wishlist Page

**Test:** Add a product with availableQuantity=0 to wishlist, view wishlist page
**Expected:**
- Product card should be dimmed (opacity-60)
- Product image should be grayscale
- "Out of Stock" badge should be visible at top-left of image
- Add to Cart button should be disabled and show "Out of Stock" text

**Why human:** Visual styling and disabled state require human observation

#### 5. Add to Cart from Wishlist with Toast

**Test:** From wishlist page, click "Add to Cart" on an in-stock item
**Expected:**
- Toast notification "Added to cart" should appear at bottom of screen
- Cart count badge in header should increment by 1
- Product should remain in wishlist (not auto-removed)

**Why human:** Toast notification timing and visual feedback require human observation

### Phase Completion Summary

**Status:** passed

**Evidence of goal achievement:**

1. **Backend persistence layer complete:**
   - WishlistItem entity with composite unique constraint on (UserId, ProductId)
   - WishlistsDbContext with dedicated 'wishlists' schema
   - Migration applied successfully
   - Registered in Program.cs

2. **Backend CQRS layer complete:**
   - 2 idempotent commands (AddToWishlist returns existing ID, RemoveFromWishlist silent on not found)
   - 3 queries (full wishlist with cross-context batch lookups, count, product IDs for efficient membership checking)
   - 5 authenticated REST API endpoints at /api/wishlist
   - All endpoints registered in Program.cs

3. **Frontend integration complete:**
   - API layer with WishlistItemDto type and 5 API functions with Bearer token auth
   - TanStack Query hooks with optimistic UI (Set-based membership for O(1) lookup)
   - WishlistToggleButton component with heart icon toggle (outlined -> filled red)
   - Wishlist page at /wishlist with grid, empty state, loading skeleton
   - Header shows heart icon with count badge (desktop + mobile)
   - Product cards have heart icon at top-left of image
   - Product detail page has heart icon next to product name
   - Account sidebar has Wishlist link

4. **All 5 success criteria satisfied:**
   - ✓ User can add and remove products from their wishlist
   - ✓ User can view wishlist page showing all saved products
   - ✓ User can move wishlist items to cart with proper stock validation
   - ✓ Product cards display heart icon indicator when product is in wishlist
   - ✓ Wishlist persists across sessions and devices

5. **All 5 requirements satisfied:**
   - ✓ WISH-01: Add to wishlist
   - ✓ WISH-02: Remove from wishlist
   - ✓ WISH-03: View wishlist page
   - ✓ WISH-04: Move to cart
   - ✓ WISH-05: Heart icon indicator

**No blockers or gaps.** Phase goal fully achieved.

---

_Verified: 2026-02-13T13:49:29Z_
_Verifier: Claude (gsd-verifier)_
