---
phase: 08-order-history-management
plan: 01
subsystem: api
tags: [cqrs, mediatr, ef-core, ordering, pagination, dashboard, status-transitions]

# Dependency graph
requires:
  - phase: 07-ordering-domain-checkout
    provides: Order aggregate, OrderingDbContext, OrderingEndpoints, BuyerIdentity
provides:
  - Extended Order aggregate with Shipped/Delivered lifecycle
  - GetOrdersByBuyer query with pagination and status filter
  - GetAllOrders admin query with pagination and status filter
  - GetOrderDashboard query with time-range aggregated statistics
  - UpdateOrderStatus command for admin Ship/Deliver transitions
  - 4 new API endpoints for order history and admin management
affects: [08-02, 08-03, 08-04, 08-05]

# Tech tracking
tech-stack:
  added: []
  patterns:
    - Shared DTOs across queries (OrderListDto/OrderSummaryDto reused by buyer and admin queries)
    - Server-side GROUP BY for dashboard aggregation
    - Time-range filtering pattern (today/7d/30d/all)
    - Admin-only status transition command with FluentValidation whitelist

key-files:
  created:
    - src/MicroCommerce.ApiService/Features/Ordering/Application/Queries/GetOrdersByBuyer/GetOrdersByBuyerQuery.cs
    - src/MicroCommerce.ApiService/Features/Ordering/Application/Queries/GetOrdersByBuyer/GetOrdersByBuyerQueryHandler.cs
    - src/MicroCommerce.ApiService/Features/Ordering/Application/Queries/GetAllOrders/GetAllOrdersQuery.cs
    - src/MicroCommerce.ApiService/Features/Ordering/Application/Queries/GetAllOrders/GetAllOrdersQueryHandler.cs
    - src/MicroCommerce.ApiService/Features/Ordering/Application/Queries/GetOrderDashboard/GetOrderDashboardQuery.cs
    - src/MicroCommerce.ApiService/Features/Ordering/Application/Queries/GetOrderDashboard/GetOrderDashboardQueryHandler.cs
    - src/MicroCommerce.ApiService/Features/Ordering/Application/Commands/UpdateOrderStatus/UpdateOrderStatusCommand.cs
    - src/MicroCommerce.ApiService/Features/Ordering/Application/Commands/UpdateOrderStatus/UpdateOrderStatusCommandHandler.cs
    - src/MicroCommerce.ApiService/Features/Ordering/Application/Commands/UpdateOrderStatus/UpdateOrderStatusCommandValidator.cs
    - src/MicroCommerce.ApiService/Features/Ordering/Infrastructure/Migrations/20260212130722_AddOrderStatusIndex.cs
  modified:
    - src/MicroCommerce.ApiService/Features/Ordering/Domain/ValueObjects/OrderStatus.cs
    - src/MicroCommerce.ApiService/Features/Ordering/Domain/Entities/Order.cs
    - src/MicroCommerce.ApiService/Features/Ordering/Infrastructure/Configurations/OrderConfiguration.cs
    - src/MicroCommerce.ApiService/Features/Ordering/OrderingEndpoints.cs
    - src/MicroCommerce.ApiService/Common/Exceptions/GlobalExceptionHandler.cs

key-decisions:
  - "Shared OrderListDto/OrderSummaryDto across buyer and admin queries to reduce duplication"
  - "Item thumbnails limited to first 3 per order for preview cards"
  - "Dashboard OrdersPerDay always returns 7 days with zero-fill for missing dates"
  - "InvalidOperationException mapped to 400 Bad Request in GlobalExceptionHandler"
  - "MarkAsFailed guard updated to prevent Shipped/Delivered orders from reverting to Failed"

patterns-established:
  - "Shared DTO pattern: GetAllOrders imports OrderListDto from GetOrdersByBuyer namespace"
  - "Time-range query pattern: string parameter parsed to DateTimeOffset filter"
  - "Dashboard zero-fill pattern: fill missing days with count=0 for consistent chart data"

# Metrics
duration: 5min
completed: 2026-02-12
---

# Phase 8 Plan 1: Order Backend API Summary

**Extended Order aggregate with Shipped/Delivered lifecycle, 3 CQRS queries (buyer history, admin list, dashboard stats), 1 status update command, and 4 new API endpoints with pagination and filtering**

## Performance

- **Duration:** 5 min
- **Started:** 2026-02-12T13:05:34Z
- **Completed:** 2026-02-12T13:10:28Z
- **Tasks:** 2
- **Files modified:** 15

## Accomplishments

- Extended Order aggregate with Shipped and Delivered statuses, completing the full order lifecycle (Submitted -> StockReserved -> Paid -> Confirmed -> Shipped -> Delivered)
- Built 3 paginated queries: buyer order history with status filter, admin order list, and dashboard with time-range aggregation (total orders, revenue, average order value, pending count, daily chart data)
- Added UpdateOrderStatus command with FluentValidation restricting to Shipped/Delivered transitions only, backed by domain guard clauses
- Registered 4 new minimal API endpoints following existing vertical slice patterns

## Task Commits

Each task was committed atomically:

1. **Task 1: Extend Order domain with Shipped/Delivered statuses and transition methods** - `57928777` (feat)
2. **Task 2: Add queries, UpdateOrderStatus command, and extend endpoints** - `1c98db54` (feat)

**Plan metadata:** TBD (docs: complete plan)

## Files Created/Modified

- `Features/Ordering/Domain/ValueObjects/OrderStatus.cs` - Added Shipped, Delivered enum values
- `Features/Ordering/Domain/Entities/Order.cs` - Added Ship(), Deliver() methods with guard clauses; fixed MarkAsFailed guard
- `Features/Ordering/Infrastructure/Configurations/OrderConfiguration.cs` - Added Status column index
- `Features/Ordering/Infrastructure/Migrations/20260212130722_AddOrderStatusIndex.cs` - EF migration for status index
- `Features/Ordering/Application/Queries/GetOrdersByBuyer/` - Buyer order history query with pagination, status filter, item thumbnails
- `Features/Ordering/Application/Queries/GetAllOrders/` - Admin order list query reusing shared DTOs
- `Features/Ordering/Application/Queries/GetOrderDashboard/` - Dashboard stats with time-range filtering and daily order counts
- `Features/Ordering/Application/Commands/UpdateOrderStatus/` - Command, handler, and validator for admin status transitions
- `Features/Ordering/OrderingEndpoints.cs` - 4 new endpoint registrations (GET my, GET all, GET dashboard, PATCH status)
- `Common/Exceptions/GlobalExceptionHandler.cs` - Added InvalidOperationException -> 400 mapping

## Decisions Made

- **Shared DTOs across queries:** GetAllOrders imports OrderListDto/OrderSummaryDto from GetOrdersByBuyer namespace to avoid duplication
- **Item thumbnails capped at 3:** `Items.Take(3).Select(i => i.ImageUrl)` provides preview without loading all items
- **Zero-fill daily chart data:** Dashboard always returns 7 days of data, filling missing dates with count=0 for consistent frontend rendering
- **InvalidOperationException mapping:** Added to GlobalExceptionHandler as 400 Bad Request so domain guard clause violations return proper HTTP status instead of 500

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 1 - Bug] Fixed MarkAsFailed guard to include Shipped/Delivered**
- **Found during:** Task 1 (Order.cs Ship/Deliver methods)
- **Issue:** MarkAsFailed only checked for Confirmed and Failed statuses, allowing Shipped/Delivered orders to be incorrectly marked as Failed
- **Fix:** Added `OrderStatus.Shipped or OrderStatus.Delivered` to the guard clause
- **Files modified:** `src/MicroCommerce.ApiService/Features/Ordering/Domain/Entities/Order.cs`
- **Verification:** Build passes, guard clause covers all terminal states
- **Committed in:** 57928777 (Task 1 commit)

**2. [Rule 2 - Missing Critical] Added InvalidOperationException to GlobalExceptionHandler**
- **Found during:** Task 2 (UpdateOrderStatus command handler)
- **Issue:** Domain guard clauses throw InvalidOperationException but GlobalExceptionHandler did not map it, resulting in 500 Internal Server Error for invalid status transitions
- **Fix:** Added InvalidOperationException case returning 400 Bad Request with the exception message
- **Files modified:** `src/MicroCommerce.ApiService/Common/Exceptions/GlobalExceptionHandler.cs`
- **Verification:** Build passes, exception handler now covers all domain exceptions
- **Committed in:** 1c98db54 (Task 2 commit)

---

**Total deviations:** 2 auto-fixed (1 bug, 1 missing critical)
**Impact on plan:** Both fixes essential for correctness. MarkAsFailed guard prevents invalid state transitions. InvalidOperationException mapping ensures proper HTTP responses for domain errors. No scope creep.

## Issues Encountered

None - plan executed cleanly with only the two deviation fixes noted above.

## User Setup Required

None - no external service configuration required.

## Next Phase Readiness

- All backend API endpoints ready for frontend consumption in plans 08-02 through 08-05
- Endpoint routes: GET /api/ordering/orders/my, GET /api/ordering/orders, GET /api/ordering/dashboard, PATCH /api/ordering/orders/{id}/status
- OrderListDto provides pagination metadata (TotalCount, Page, PageSize) for frontend pagination components
- OrderDashboardDto provides all data needed for admin dashboard stat cards and bar chart
- No blockers or concerns

---
*Phase: 08-order-history-management*
*Completed: 2026-02-12*
