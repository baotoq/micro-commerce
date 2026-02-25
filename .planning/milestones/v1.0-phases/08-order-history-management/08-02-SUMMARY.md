---
phase: 08-order-history-management
plan: 02
subsystem: frontend-data-layer
tags: [react-query, api-client, dnd-kit, recharts, shadcn-chart, admin-layout]
completed: 2026-02-12
duration: ~3m
dependency-graph:
  requires: []
  provides:
    - "Frontend dependencies: @dnd-kit/core, @dnd-kit/sortable, @dnd-kit/utilities, recharts"
    - "shadcn/ui chart component (chart.tsx)"
    - "Admin layout QueryProvider for React Query hooks"
    - "Admin nav Orders link"
    - "4 new API functions: getOrdersByBuyer, getAllOrders, getOrderDashboard, updateOrderStatus"
    - "5 new React Query hooks: useOrdersByBuyer, useOrderWithPolling, useAllOrders, useOrderDashboard, useUpdateOrderStatus"
  affects:
    - "08-03: Order history page uses useOrdersByBuyer hook"
    - "08-04: Admin dashboard uses useOrderDashboard, useAllOrders, chart component"
    - "08-05: Admin kanban uses useAllOrders, useUpdateOrderStatus, dnd-kit"
tech-stack:
  added:
    - "@dnd-kit/core@6.3.1"
    - "@dnd-kit/sortable@10.0.0"
    - "@dnd-kit/utilities@3.2.2"
    - "recharts@2.15.4"
  patterns:
    - "React Query polling with terminal state detection"
    - "Query key hierarchy for scoped invalidation"
key-files:
  created:
    - src/MicroCommerce.Web/src/components/ui/chart.tsx
    - src/MicroCommerce.Web/src/hooks/use-orders.ts
  modified:
    - src/MicroCommerce.Web/package.json
    - src/MicroCommerce.Web/package-lock.json
    - src/MicroCommerce.Web/src/app/admin/layout.tsx
    - src/MicroCommerce.Web/src/lib/api.ts
decisions:
  - id: "terminal-status-polling-stop"
    decision: "Stop polling for Delivered, Failed, Cancelled statuses"
    rationale: "Terminal states will not change, no need to keep polling"
  - id: "20s-polling-interval"
    decision: "20-second polling interval for order status"
    rationale: "Balance between responsiveness and server load for order processing"
  - id: "scoped-query-key-hierarchy"
    decision: "Query keys: ['orders', 'buyer', params] and ['orders', 'admin', params]"
    rationale: "Allows scoped invalidation - admin mutations only invalidate admin queries"
---

# Phase 08 Plan 02: Frontend Data Layer Infrastructure Summary

**One-liner:** dnd-kit + recharts installed, admin QueryProvider + Orders nav, 4 API functions, 5 React Query hooks with polling

## Tasks Completed

| # | Task | Commit | Key Changes |
|---|------|--------|-------------|
| 1 | Install dependencies, add shadcn/ui chart, QueryProvider to admin layout | `fd96a3f8` | package.json, chart.tsx, admin layout.tsx |
| 2 | Add ordering API functions and React Query hooks | `bec2af5e` | api.ts, use-orders.ts |

## What Was Built

### Dependencies Installed
- **@dnd-kit/core, @dnd-kit/sortable, @dnd-kit/utilities** - Drag-and-drop library for admin kanban board (plan 08-05)
- **recharts** - Charting library for admin dashboard (plan 08-04), installed via shadcn/ui chart component

### shadcn/ui Chart Component
- `src/components/ui/chart.tsx` - ChartContainer, ChartTooltip, ChartTooltipContent, ChartLegend, ChartLegendContent
- Wraps recharts with shadcn/ui styling and theming support

### Admin Layout Updates
- Wrapped children in `<QueryProvider>` so React Query hooks work in all admin pages
- Added "Orders" nav link with ShoppingBag icon between Categories and Dead Letters

### API Functions (api.ts)
| Function | Method | Endpoint | Purpose |
|----------|--------|----------|---------|
| `getOrdersByBuyer` | GET | `/api/ordering/orders/my` | Buyer order history (paginated, status filter) |
| `getAllOrders` | GET | `/api/ordering/orders` | Admin order list (paginated, status filter) |
| `getOrderDashboard` | GET | `/api/ordering/dashboard` | Dashboard statistics with time range |
| `updateOrderStatus` | PATCH | `/api/ordering/orders/{id}/status` | Admin status transitions |

New types: `OrderSummaryDto`, `OrderListDto`, `OrderDashboardDto`, `UpdateOrderStatusRequest`

### React Query Hooks (use-orders.ts)
| Hook | Type | Key Features |
|------|------|--------------|
| `useOrdersByBuyer` | query | Paginated, status filter, key: `["orders", "buyer", params]` |
| `useOrderWithPolling` | query | 20s refetchInterval, stops at terminal states (Delivered/Failed/Cancelled) |
| `useAllOrders` | query | Paginated, status filter, key: `["orders", "admin", params]` |
| `useOrderDashboard` | query | Time range param, key: `["dashboard", timeRange]` |
| `useUpdateOrderStatus` | mutation | Invalidates `["orders", "admin"]` and `["dashboard"]` on success |

## Deviations from Plan

None - plan executed exactly as written.

## Decisions Made

1. **Terminal status polling stop** - `useOrderWithPolling` returns `false` for refetchInterval when order status is Delivered, Failed, or Cancelled. Prevents unnecessary network requests for orders that will not change.

2. **20-second polling interval** - Chosen to balance responsiveness (order processing typically takes seconds) with server load reduction.

3. **Scoped query key hierarchy** - Buyer queries use `["orders", "buyer", ...]` and admin queries use `["orders", "admin", ...]`. This allows `useUpdateOrderStatus` to invalidate only admin queries without triggering refetches of buyer-side data.

## Verification Results

- All packages installed: `npm ls @dnd-kit/core @dnd-kit/sortable @dnd-kit/utilities recharts` passes
- `chart.tsx` exists in `src/components/ui/`
- Admin layout wraps children in `<QueryProvider>`
- Admin nav has Orders link with ShoppingBag icon
- api.ts has 4 new API functions + 4 new types
- use-orders.ts has 5 hooks with polling logic
- `npx tsc --noEmit` passes with zero errors

## Next Phase Readiness

- Plans 08-03 through 08-05 can now consume these hooks and components
- Backend endpoints (08-01) need to be implemented for the API functions to return data
- No blockers for downstream plans
