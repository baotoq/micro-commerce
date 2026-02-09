---
phase: 11-user-profiles-auth-flow
plan: 02
subsystem: cart-auth-integration
tags: [cart-merge, authentication, session-handling]
requires: [keycloak-jwt-auth, cart-domain]
provides: [guest-to-auth-cart-merge]
affects: [cart-endpoints, cart-aggregate, nextauth-config]
tech-stack:
  added: []
  patterns: [server-side-cookie-reading, bearer-auth, session-flags]
key-files:
  created:
    - src/MicroCommerce.ApiService/Features/Cart/Application/Commands/MergeCarts/MergeCartsCommand.cs
    - src/MicroCommerce.ApiService/Features/Cart/Application/Commands/MergeCarts/MergeCartsCommandHandler.cs
  modified:
    - src/MicroCommerce.ApiService/Features/Cart/Domain/Entities/Cart.cs
    - src/MicroCommerce.ApiService/Features/Cart/CartEndpoints.cs
    - src/MicroCommerce.Web/src/auth.ts
    - src/MicroCommerce.Web/src/lib/api.ts
    - src/MicroCommerce.Web/src/types/next-auth.d.ts
decisions:
  - title: Server-side cookie reading for merge
    rationale: HttpOnly buyer_id cookie cannot be read client-side, so merge endpoint reads it directly from request cookies
    alternatives: [Pass guest ID from client (impossible with HttpOnly), Make cookie readable (security risk)]
  - title: isNewLogin session flag
    rationale: UI needs to detect fresh login to trigger cart merge; set in jwt callback when account is present
    alternatives: [Events callback (no request context), Client-side detection (unreliable)]
metrics:
  duration: 2
  tasks_completed: 2
  files_created: 2
  files_modified: 5
  commits: 2
  completed_date: 2026-02-13
---

# Phase 11 Plan 02: Guest-to-Authenticated Cart Merge Summary

**One-liner:** Server-side cart merge on login transfers guest cart items to authenticated user with cookie-based identity resolution

## What Was Built

Implemented a seamless cart merge system that transfers guest shopping cart items to a user's authenticated account upon login, preserving the shopping experience without data loss.

**Backend (Cart Module):**
- Added `TransferOwnership(Guid)` method to Cart aggregate for reassigning cart ownership
- Created `MergeCartsCommand` CQSR command with guest and authenticated buyer IDs
- Implemented `MergeCartsCommandHandler` with three merge scenarios:
  1. No guest cart → no-op
  2. No auth cart exists → transfer ownership of guest cart
  3. Both carts exist → merge items (combining quantities) + delete guest cart
- Added POST `/api/cart/merge` endpoint with `RequireAuthorization` attribute
- Endpoint reads `buyer_id` cookie server-side and clears it after successful merge
- Returns 204 No Content on success, handles same-ID edge case

**Frontend (NextAuth Integration):**
- Extended NextAuth JWT callback to set `isNewLogin` flag when fresh login detected
- Added `isNewLogin` to Session and JWT TypeScript interfaces
- Created `mergeCart(accessToken)` API function with bearer auth + credentials mode
- Function ready for UI integration (Plan 04 will trigger it from header component)

**Cart Merge Logic:**
- Guest cart items added to authenticated cart via existing `AddItem` method
- Quantities automatically combine for duplicate products (MaxQuantity=99 enforced)
- Guest cart deleted after merge to prevent stale data
- Cookie deletion ensures clean state post-merge

## Deviations from Plan

None - plan executed exactly as written. The plan anticipated the HttpOnly cookie challenge and designed the server-side reading approach from the start.

## Testing Performed

**Compilation:**
- Backend: `dotnet build` completed with 0 errors
- Frontend: `npx tsc --noEmit` passed type checking
- Verified all Cart module commands compile and register correctly

**Code Review:**
- Confirmed `TransferOwnership` correctly updates BuyerId and LastModifiedAt
- Verified MergeCartsCommandHandler loads carts with `.Include(c => c.Items)`
- Checked endpoint authorization, cookie reading, and deletion logic
- Validated session flag propagation through jwt → session callbacks

**Not Yet Tested:**
- Runtime cart merge flow (requires Plan 04 UI integration)
- Edge cases: concurrent merges, expired guest carts, inventory conflicts
- Integration tests (no test project exists yet per CLAUDE.md)

## Key Decisions

### 1. Server-Side Cookie Reading
**Context:** The `buyer_id` cookie is HttpOnly for security (prevents XSS attacks). Client-side JavaScript cannot read it.

**Decision:** Merge endpoint reads cookie directly from `HttpContext.Request.Cookies` instead of requiring client to pass guest buyer ID in request body.

**Impact:**
- ✅ Maintains security (cookie stays HttpOnly)
- ✅ Simpler API contract (no request body needed)
- ✅ Prevents client-side tampering of buyer ID
- ⚠️ Cookie must be sent with request (`credentials: "include"`)

### 2. Session Flag for Merge Trigger
**Context:** Frontend needs to know when to call the merge API. NextAuth events don't have request context for cookie reading.

**Decision:** Set `token.isNewLogin = true` in jwt callback when `account` is present (fresh login), propagate to session, and UI reads flag to trigger merge.

**Alternatives Considered:**
- **Events.signIn callback:** Doesn't have access to request/response for cookie operations
- **Client-side detection:** Unreliable (page refresh loses state)
- **Middleware redirect:** Too invasive, breaks user flow

**Implementation:** Plan 04 will use the flag in header component to call `mergeCart()` after detecting `session.isNewLogin === true`.

### 3. Merge Strategy: Transfer vs. Merge
**Context:** Three scenarios when user logs in:
1. No guest cart
2. Guest cart exists, no auth cart
3. Both carts exist

**Decision:**
- Scenario 1 → No-op (early return)
- Scenario 2 → Transfer ownership (reassign BuyerId, keep cart intact)
- Scenario 3 → Merge items into auth cart + delete guest cart

**Rationale:**
- Transfer is more efficient than creating new cart + copying items
- Merge preserves both guest and existing auth items
- Deletion prevents stale guest carts accumulating in database

## Outstanding Work

**Plan 04 - Frontend Trigger:**
- Wire `mergeCart()` call in header account icon component
- Detect `session.isNewLogin` and trigger merge on mount
- Handle merge errors gracefully (already logged, don't block UI)

**Future Enhancements (not in current plan):**
- Add domain event `CartMergedEvent` for analytics
- Add integration tests for merge handler scenarios
- Consider background job for cleaning up expired guest carts

## Files Changed

**Created (2):**
- `src/MicroCommerce.ApiService/Features/Cart/Application/Commands/MergeCarts/MergeCartsCommand.cs` (6 lines)
- `src/MicroCommerce.ApiService/Features/Cart/Application/Commands/MergeCarts/MergeCartsCommandHandler.cs` (51 lines)

**Modified (5):**
- `src/MicroCommerce.ApiService/Features/Cart/Domain/Entities/Cart.cs` (+9 lines) - TransferOwnership method
- `src/MicroCommerce.ApiService/Features/Cart/CartEndpoints.cs` (+42 lines) - merge endpoint + handler
- `src/MicroCommerce.Web/src/auth.ts` (+4 lines) - isNewLogin flag
- `src/MicroCommerce.Web/src/lib/api.ts` (+12 lines) - mergeCart function
- `src/MicroCommerce.Web/src/types/next-auth.d.ts` (+2 lines) - type extensions

**Total:** 126 lines added, 0 removed

## Commits

1. **0f754d99** - `feat(11-02): implement guest-to-auth cart merge command`
   - Cart.TransferOwnership method
   - MergeCartsCommand + handler
   - POST /api/cart/merge endpoint

2. **25401e40** - `feat(11-02): add cart merge API client and auth session flag`
   - NextAuth isNewLogin session flag
   - mergeCart API function
   - TypeScript type extensions

## Verification Checklist

- [x] Backend compiles (`dotnet build`)
- [x] Frontend type-checks (`npx tsc --noEmit`)
- [x] Cart.TransferOwnership method exists and updates BuyerId
- [x] MergeCartsCommandHandler merges items with quantity combination
- [x] CartEndpoints has /merge endpoint with RequireAuthorization
- [x] Endpoint reads buyer_id cookie and deletes it after merge
- [x] auth.ts sets isNewLogin flag on fresh login
- [x] api.ts has mergeCart function with bearer auth
- [x] TypeScript types include isNewLogin in Session and JWT

## Self-Check

Verifying all claimed artifacts exist:

**Backend files:**
```bash
[ -f "/Users/baotoq/Work/micro-commerce/src/MicroCommerce.ApiService/Features/Cart/Application/Commands/MergeCarts/MergeCartsCommand.cs" ] && echo "✓ MergeCartsCommand.cs" || echo "✗ MISSING"
[ -f "/Users/baotoq/Work/micro-commerce/src/MicroCommerce.ApiService/Features/Cart/Application/Commands/MergeCarts/MergeCartsCommandHandler.cs" ] && echo "✓ MergeCartsCommandHandler.cs" || echo "✗ MISSING"
```

**Frontend files:**
```bash
grep -q "isNewLogin" /Users/baotoq/Work/micro-commerce/src/MicroCommerce.Web/src/auth.ts && echo "✓ auth.ts has isNewLogin" || echo "✗ MISSING"
grep -q "mergeCart" /Users/baotoq/Work/micro-commerce/src/MicroCommerce.Web/src/lib/api.ts && echo "✓ api.ts has mergeCart" || echo "✗ MISSING"
```

**Commits:**
```bash
git log --oneline --all | grep -q "0f754d99" && echo "✓ Commit 0f754d99" || echo "✗ MISSING"
git log --oneline --all | grep -q "25401e40" && echo "✓ Commit 25401e40" || echo "✗ MISSING"
```

## Self-Check: PASSED

All files created, all commits exist, all verifications passed.
