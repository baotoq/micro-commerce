# Phase 4: Inventory Domain - Context

**Gathered:** 2026-02-07
**Status:** Ready for planning

<domain>
## Phase Boundary

Build inventory tracking with stock management and reservation pattern. Three capabilities: stock level tracking per product (INV-01), stock reservation during checkout to prevent overselling (INV-02), and admin stock adjustment UI (ADM-02). Does not include checkout flow itself (Phase 7) or real-time stock update events from orders (INV-03, Phase 7).

</domain>

<decisions>
## Implementation Decisions

### Stock Display
- Threshold-based display: "In Stock" normally, "Only X left!" when stock is 10 or fewer
- Out-of-stock products: hide the Add to Cart button entirely, show an "Out of Stock" notice instead
- Product cards in grid: Claude's discretion on visual treatment (e.g., subtle overlay or badge), matching Apple Store aesthetic
- Available quantity shown accounts for active reservations (not total stock)

### Reservation Behavior
- 15-minute TTL on stock reservations
- Reserve at checkout start (when user clicks "Proceed to Checkout")
- On reservation expiry mid-checkout: silently attempt to re-reserve; only block if truly out of stock
- Displayed stock = total stock minus active reservations (available quantity)

### Admin Stock UI
- Inline stock editing on the existing admin product table (add stock column)
- Relative adjustment mode: admin enters +10 or -5 to adjust from current
- Optional reason field for adjustments (not required)
- Visible adjustment history log: who, when, amount, reason — in collapsible section or modal

### Cross-Module Events
- Auto-create StockItem with quantity 0 when ProductCreated event is consumed
- Stock records persist when product is archived (kept for potential un-archive)
- Admin notification: show badge/alert in admin UI when stock hits low threshold (StockLow event)
- Stock info delivery to frontend: Claude's discretion (include in product response vs separate call, considering module boundary principles)

### Claude's Discretion
- Out-of-stock card visual treatment in the product grid
- Whether stock info is included in catalog product API response or fetched separately
- Exact admin notification UX for low stock alerts
- Optimistic concurrency implementation details

</decisions>

<specifics>
## Specific Ideas

- Low stock threshold is 10 units
- Reservation TTL is 15 minutes
- Relative adjustments feel more natural for warehouse workflows ("received 50 units" vs "set to 73")
- Adjustment history adds accountability — important for a showcase project demonstrating real patterns

</specifics>

<deferred>
## Deferred Ideas

None — discussion stayed within phase scope

</deferred>

---

*Phase: 04-inventory-domain*
*Context gathered: 2026-02-07*
