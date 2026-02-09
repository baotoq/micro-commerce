# Phase 7: Ordering Domain & Checkout - Context

**Gathered:** 2026-02-09
**Status:** Ready for planning

<domain>
## Phase Boundary

Build order creation with multi-step checkout flow, guest checkout support, mock payment simulation, and stock reservation via MassTransit saga. Includes Order aggregate, checkout UI, order confirmation page, and saga orchestration with compensation handlers. Order history and admin management are Phase 8.

</domain>

<decisions>
## Implementation Decisions

### Checkout flow steps
- Single-page accordion layout (Shopify-style), not multi-step wizard
- Sections: Shipping → Payment → (no separate review step)
- Order summary in sticky sidebar on desktop, collapses on mobile
- Shipping info collected: name, email, address
- Payment is the last step — no explicit review step before it
- Order summary always visible alongside the accordion sections

### Guest checkout experience
- Guest identified by email only — no phone, no account creation post-purchase
- Before checkout, show "Continue as Guest" or "Sign In" choice (login prompt)
- Logged-in users get shipping info pre-filled from profile
- No post-purchase account creation prompt — keep it simple

### Payment simulation
- Simple "Pay Now" button with amount displayed — no fake card form
- UI toggle to simulate success or failure (good for demo purposes)
- 1-2 second simulated processing delay with spinner/overlay
- On failure: error banner shown at top of payment section, user can retry immediately without re-entering info

### Order confirmation & feedback
- Full summary on confirmation page: order number, all items with images, shipping address, total breakdown (subtotal, shipping, tax, total)
- Order number format: random alphanumeric with prefix (e.g., MC-A7X9B2)
- "Continue Shopping" button as primary CTA on confirmation page
- No print option — keep it simple

### Claude's Discretion
- Cart clearing timing (on successful payment vs after confirmation loads)
- Saga compensation handler design
- Order status state machine transitions
- Exact accordion animation and UX polish
- Tax/shipping calculation approach (flat rate, mock values)
- Checkout page route structure

</decisions>

<specifics>
## Specific Ideas

- Accordion checkout like Shopify — all sections on one page, expanding one at a time
- Login gate before checkout: "Continue as Guest" or "Sign In" — not a direct jump from cart
- Mock payment toggle clearly visible so demo viewers understand it's intentional
- Brief processing delay to make the payment feel realistic

</specifics>

<deferred>
## Deferred Ideas

None — discussion stayed within phase scope

</deferred>

---

*Phase: 07-ordering-checkout*
*Context gathered: 2026-02-09*
