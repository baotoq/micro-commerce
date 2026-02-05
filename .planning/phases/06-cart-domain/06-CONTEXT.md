# Phase 6: Cart Domain - Context

**Gathered:** 2026-02-09
**Status:** Ready for planning

<domain>
## Phase Boundary

Shopping cart with guest support, database persistence, optimistic UI, and cart merge on login. Includes cart page, add-to-cart flow, header badge, and toast notifications. Checkout flow and payment are Phase 7.

</domain>

<decisions>
## Implementation Decisions

### Cart page layout
- Dedicated /cart page (no slide-out drawer)
- Compact row per item: small thumbnail, name, price, quantity stepper, remove button
- Order summary placement: Claude's discretion (sidebar vs bottom based on storefront patterns)
- Quantity changes use optimistic UI with rollback toast on server rejection (e.g., out of stock)

### Add-to-cart experience
- Simple text toast: "Added to cart", auto-dismisses after 3s
- Adding duplicate item silently increments quantity by 1, toast says "Updated quantity"
- Cart badge in header with subtle bounce animation on count change
- "Add to cart" button only on product detail page (not on grid cards)

### Guest cart & merge behavior
- Auto-generated unique buyer ID stored in cookie (7-day TTL on cookie)
- Cart persists across browser restarts via cookie
- On login with existing authenticated cart: combine both carts, sum quantities for same products
- Cart expiration job: 30-day TTL on cart records (from roadmap)

### Cart expiration & empty state
- Toast notification "Your cart has expired" on next visit after expiry
- Empty cart shows "Your cart is empty" message with "Continue shopping" button (no product suggestions)
- Removing last item stays on cart page showing empty state
- Remove item requires confirmation dialog before deleting

### Claude's Discretion
- Order summary layout (right sidebar vs below items)
- Max quantity per item cap strategy
- Exact toast positioning and duration
- Cart page responsive breakpoints
- Loading skeleton design for cart page

</decisions>

<specifics>
## Specific Ideas

- Consistent with Apple Store aesthetic established in storefront (zinc palette, generous whitespace)
- Optimistic UI pattern with React Query mutations (specified in roadmap)
- Cookie-based buyer ID aligns with guest checkout in Phase 7

</specifics>

<deferred>
## Deferred Ideas

None — discussion stayed within phase scope

</deferred>

---

*Phase: 06-cart-domain*
*Context gathered: 2026-02-09*
