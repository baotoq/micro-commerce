# Phase 8: Order History & Management - Context

**Gathered:** 2026-02-10
**Status:** Ready for planning

<domain>
## Phase Boundary

Customer-facing order history (list + detail) and admin order management (dashboard + kanban). Customers can view past orders and track status. Admins see business metrics and manage order lifecycle. No new order statuses or checkout changes — those are Phase 7.

</domain>

<decisions>
## Implementation Decisions

### Order history display
- Card-based layout for order list (not table)
- Rich preview per card: order number, date, status badge, total, first 2-3 item thumbnails, item count
- Status filter tabs (All, Submitted, Confirmed, Paid, Shipped, Delivered) — no date range filter
- Newest orders first by default
- Clicking a card navigates to full order detail page

### Order detail page (customer)
- Full summary view: status stepper, all items with images/prices, shipping address, payment summary, order totals
- Horizontal stepper showing full lifecycle: Submitted → Confirmed → Paid → Shipped → Delivered
- Failed/cancelled orders: stepper turns red at the failed step with reason displayed below

### Status updates & tracking
- Polling-based status updates (15-30 second interval) — no SignalR
- Full lifecycle stepper progression: Submitted → Confirmed → Paid → Shipped → Delivered (5 steps)
- Failed orders get red stepper treatment with failure reason

### Admin dashboard
- Combined focus: operational + business metrics
- Top row: 4 stat cards — Total Orders, Revenue Today, Average Order Value, Pending Orders
- Simple bar chart: orders per day for last 7 days
- Below stats: recent orders table
- Time range: defaults to today, range picker to switch between today / 7d / 30d / all time

### Admin order management
- Kanban board layout with columns by status (Submitted, Confirmed, Paid, Shipped, Delivered)
- Drag and drop to transition order status between columns
- Strict forward-only transitions — matches domain model guard clauses, no skipping steps
- Clicking an order card opens a full page detail view (not drawer)
- Breadcrumb navigation back to kanban from detail page

### Claude's Discretion
- Polling interval (within 15-30s range)
- Chart library choice for bar chart
- Kanban card content/density
- Empty state designs (no orders yet)
- Loading skeleton patterns
- Pagination/infinite scroll for order lists and kanban columns

</decisions>

<specifics>
## Specific Ideas

- Kanban board for admin order management — visual, intuitive workflow management
- Drag and drop for status transitions gives admin a hands-on feel
- Rich order cards with thumbnails give customers quick visual context without opening detail
- Horizontal stepper is a common e-commerce pattern (Amazon, Shopify) — familiar to users

</specifics>

<deferred>
## Deferred Ideas

None — discussion stayed within phase scope

</deferred>

---

*Phase: 08-order-history-management*
*Context gathered: 2026-02-10*
