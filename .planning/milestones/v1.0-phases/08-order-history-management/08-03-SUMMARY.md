---
phase: 08-order-history-management
plan: 03
subsystem: storefront-order-pages
tags: [next-auth, react-query, order-history, status-stepper, polling, auth-gate]
completed: 2026-02-12
duration: ~3m
dependency-graph:
  requires:
    - "08-01: Order backend API (getOrdersByBuyer, getOrderById endpoints)"
    - "08-02: Frontend data layer (useOrdersByBuyer, useOrderWithPolling hooks, OrderSummaryDto, OrderDto types)"
  provides:
    - "Customer order history page at /orders with auth gate"
    - "Customer order detail page at /orders/[id] with polling status"
    - "OrderStatusStepper component mapping 5-step lifecycle"
    - "OrderHistoryList component with tab-based status filtering"
    - "OrderDetail component with full order summary"
    - "Header navigation link to orders"
  affects:
    - "08-04: Admin dashboard may reuse status badge patterns"
    - "08-05: Admin kanban may reference status mapping"
tech-stack:
  added: []
  patterns:
    - "Auth-gated client pages with useSession redirect"
    - "Horizontal status stepper with 5-step customer lifecycle mapping"
    - "Tab-based filtering with paginated card list"
    - "useParams for client-side dynamic route params"
key-files:
  created:
    - src/MicroCommerce.Web/src/components/storefront/order-status-stepper.tsx
    - src/MicroCommerce.Web/src/components/storefront/order-history-list.tsx
    - src/MicroCommerce.Web/src/components/storefront/order-detail.tsx
    - src/MicroCommerce.Web/src/app/(storefront)/orders/page.tsx
    - src/MicroCommerce.Web/src/app/(storefront)/orders/[id]/page.tsx
  modified:
    - src/MicroCommerce.Web/src/components/storefront/header.tsx
decisions:
  - id: "5-step-customer-lifecycle"
    decision: "Map internal statuses to 5 customer-visible steps: Submitted, Paid, Confirmed, Shipped, Delivered"
    rationale: "Customers don't need to see StockReserved; Failed/Cancelled shown as error state on last active step"
  - id: "useParams-for-client-page"
    decision: "Use useParams hook instead of page props for dynamic route param"
    rationale: "Client components ('use client') use useParams for cleaner access to route params"
  - id: "orders-icon-in-header"
    decision: "Added ClipboardList icon link to desktop header and text link to mobile menu"
    rationale: "Discoverable navigation to order history from any storefront page"
---

# Phase 08 Plan 03: Customer Order History & Detail Pages Summary

**One-liner:** Auth-gated order history with tab filtering, order detail with horizontal 5-step status stepper and 20s polling

## Tasks Completed

| # | Task | Commit | Key Changes |
|---|------|--------|-------------|
| 1 | Order status stepper and order history list components | `2bae9ee4` | order-status-stepper.tsx, order-history-list.tsx |
| 2 | Order history page, order detail page, header nav update | `c9f5fab4` | orders/page.tsx, orders/[id]/page.tsx, order-detail.tsx, header.tsx |

## What Was Built

### OrderStatusStepper Component
- Horizontal 5-step stepper: Submitted -> Paid -> Confirmed -> Shipped -> Delivered
- `mapToCustomerStep()` maps internal API statuses to customer-visible step index
- Completed steps: green circle with check icon, green connecting line
- Current step: blue pulsing circle with step number
- Failed/Cancelled: red circle with X icon, failure reason displayed below
- Responsive sizing (size-8 mobile, size-10 desktop)

### OrderHistoryList Component
- Tab bar with 6 filters: All, Submitted, Paid, Confirmed, Shipped, Delivered
- Order cards showing: thumbnails (up to 3 stacked circles), order number, status badge, date, item count, total
- Status badge colors: Submitted=yellow, Confirmed=blue, Paid=green, Shipped=purple, Delivered=green, Failed=red, Cancelled=gray
- Pagination with Previous/Next buttons and page indicator
- Loading state: 4 skeleton cards
- Empty state: "No orders yet" with link to browse products
- Cards link to `/orders/{id}` for detail view

### Order History Page (/orders)
- Auth-gated with `useSession` from next-auth/react
- Loading state: full-page skeleton with tab and card placeholders
- Unauthenticated users redirected to `/api/auth/signin`
- Renders heading "My Orders" and `<OrderHistoryList />` component

### Order Detail Page (/orders/[id])
- Breadcrumb navigation: "My Orders" (link) > "Order Details"
- Uses `useParams` hook for client-side route param extraction
- Renders `<OrderDetail />` component with orderId prop

### OrderDetail Component
- Uses `useOrderWithPolling(orderId)` for auto-polling every 20s
- Full order display: header (order number, status badge, date), status stepper, items list, shipping address, order summary
- Items section: 64x64 image thumbnails, product name, quantity x unit price, line total
- Shipping address: name, street, city/state/zip, email
- Order summary: subtotal, shipping, tax, separator, total (bold)
- Error state: "Order not found" with back-to-orders button
- Loading state: skeleton layout matching content structure
- All prices formatted with `Intl.NumberFormat` as USD currency

### Header Navigation Update
- Desktop: ClipboardList icon link to /orders (hidden on mobile, shown with sm:block)
- Mobile menu: "Orders" text link added below "Products"

## Deviations from Plan

None - plan executed exactly as written.

## Decisions Made

1. **5-step customer lifecycle mapping** - Internal statuses like "StockReserved" are mapped to the "Submitted" step since customers don't need to see backend processing details. Failed/Cancelled states render as error on the last active step.

2. **useParams for client page** - Used `useParams<{ id: string }>()` hook instead of `params: Promise<{id: string}>` page props since the order detail page is a client component.

3. **Orders icon in header** - Added ClipboardList icon from lucide-react to the desktop header nav area and a text link in the mobile menu for discoverability.

## Verification Results

- `npx tsc --noEmit` passes with zero errors
- orders/page.tsx checks session status and redirects unauthenticated users
- orders/[id]/page.tsx renders OrderDetail with status stepper and polling via useOrderWithPolling
- Breadcrumb navigates from detail back to /orders
- Status stepper maps all statuses correctly: Submitted/StockReserved->0, Paid->1, Confirmed->2, Shipped->3, Delivered->4
- Failed/Cancelled renders red X icon with failure reason
- Polling stops at terminal states (Delivered, Failed, Cancelled) via useOrderWithPolling hook

## Next Phase Readiness

- All customer-facing order pages are complete
- 08-04 (admin dashboard) and 08-05 (admin kanban) can proceed independently
- No blockers identified
