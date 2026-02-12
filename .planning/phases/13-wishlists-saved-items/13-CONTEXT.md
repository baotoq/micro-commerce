# Phase 13: Wishlists & Saved Items - Context

**Gathered:** 2026-02-13
**Status:** Ready for planning

<domain>
## Phase Boundary

Users can save products to a persistent wishlist and move items to cart. Includes heart icon toggle on product cards and detail pages, a dedicated wishlist page, and add-to-cart from wishlist with stock validation. Wishlist persists across sessions and devices (requires authentication).

</domain>

<decisions>
## Implementation Decisions

### Heart icon interaction
- Heart icon appears on both product cards (grid) AND product detail page — consistent access everywhere
- Outlined heart → filled red heart on click (instant toggle, no animation)
- Clicking heart as guest redirects to login/register — wishlist requires authentication
- Heart icon position on product cards: Claude's discretion (top-right overlay on image is common pattern)

### Wishlist page layout
- Reuse existing product card grid layout — consistent with storefront browsing experience
- Accessible from both My Account sidebar AND a heart icon in the header nav bar
- Header heart icon shows count badge (number of wishlist items) — similar to cart count
- Empty state: "Your wishlist is empty" message with a "Browse products" CTA button

### Move-to-cart behavior
- "Add to cart" button per item — single item at a time, no batch operations
- Item stays in wishlist after adding to cart — user removes manually if they want
- Always adds quantity of 1 — user adjusts quantity in cart if needed
- Toast notification after adding: "Added to cart" — non-blocking, consistent with existing cart behavior

### Out-of-stock handling
- Out-of-stock items stay visible but dimmed/grayed out with "Out of stock" badge
- "Add to cart" button disabled for out-of-stock items
- Always show current price — no comparison to when item was saved
- Out-of-stock items keep their original position in the grid (not pushed to bottom)
- Removing items: clicking the filled heart on a wishlist card removes it — consistent toggle pattern

### Claude's Discretion
- Heart icon exact position on product cards (top-right of image is suggested)
- Loading states and skeleton design for wishlist page
- Exact styling of the "Out of stock" badge
- Toast notification duration and style
- Sort order of wishlist items (e.g., most recently added first)

</decisions>

<specifics>
## Specific Ideas

- Heart toggle should feel instant — optimistic UI update before server confirms
- Wishlist count badge in header mirrors the cart count badge pattern for visual consistency
- Product cards in wishlist grid should look identical to storefront product cards (plus add-to-cart button)

</specifics>

<deferred>
## Deferred Ideas

None — discussion stayed within phase scope

</deferred>

---

*Phase: 13-wishlists-saved-items*
*Context gathered: 2026-02-13*
