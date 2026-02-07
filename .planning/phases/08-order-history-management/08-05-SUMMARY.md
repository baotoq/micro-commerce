---
phase: 08-order-history-management
plan: 05
subsystem: frontend
tags: [react, dnd-kit, kanban, admin, drag-and-drop, order-management, breadcrumb]

# Dependency graph
requires:
  - phase: 08-order-history-management
    plan: 01
    what: Order backend API with status update endpoint
  - phase: 08-order-history-management
    plan: 02
    what: Frontend data layer (useAllOrders, useUpdateOrderStatus, useOrderWithPolling hooks)
provides:
  - Admin kanban board for visual order management
  - Admin order detail page with status transition buttons
  - Breadcrumb navigation for admin order pages
affects: []

# Tech tracking
tech-stack:
  added: []
  patterns:
    - "@dnd-kit/core drag-and-drop with useDraggable/useDroppable hooks"
    - "Client-side transition validation (forward-only, admin-allowed only)"
    - "DragOverlay for ghost card during drag operations"
    - "Breadcrumb navigation pattern with ChevronRight separator"

# File tracking
key-files:
  created:
    - src/MicroCommerce.Web/src/components/admin/order-kanban-card.tsx
    - src/MicroCommerce.Web/src/components/admin/order-kanban-column.tsx
    - src/MicroCommerce.Web/src/components/admin/order-kanban.tsx
    - src/MicroCommerce.Web/src/app/admin/orders/page.tsx
    - src/MicroCommerce.Web/src/app/admin/orders/[id]/page.tsx
  modified: []

# Decisions
decisions:
  - id: kanban-valid-transitions
    decision: "Only Confirmed->Shipped and Shipped->Delivered are valid DnD transitions"
    rationale: "Matches backend UpdateOrderStatus which only accepts Shipped and Delivered as new statuses"
  - id: pointer-sensor-distance
    decision: "8px activation constraint for PointerSensor"
    rationale: "Prevents accidental drags when clicking cards to navigate to detail page"
  - id: drag-handle-pattern
    decision: "Separate drag handle button on card instead of entire card draggable"
    rationale: "Card is also a Link to detail page; separate handle avoids conflict between drag and click"

# Metrics
metrics:
  duration: ~2 minutes
  completed: 2026-02-12
---

# Phase 08 Plan 05: Admin Order Kanban Board Summary

Admin kanban board with @dnd-kit drag-and-drop for visual order lifecycle management plus admin order detail page with breadcrumb navigation.

## What Was Built

### Task 1: Kanban Board with DnD Columns, Cards, and Status Transition Validation

Created a full kanban board for admin order management:

- **order-kanban-card.tsx**: Draggable card component using `useDraggable` from @dnd-kit/core. Each card shows order number, total price, item count, date, and status badge. Card has a dedicated drag handle (GripVertical icon) separate from the card body which is a Link to the admin detail page. DragOverlay ghost card component also provided.

- **order-kanban-column.tsx**: Droppable column component using `useDroppable`. Shows column header with status name and order count badge. Visual feedback: green border when hovering with valid drop, red border for invalid drop, blue dashed border for potential valid targets. Scrollable body with max-height constraint.

- **order-kanban.tsx**: Main kanban orchestrator with DndContext, PointerSensor (8px activation distance), closestCorners collision detection. Groups orders by status into 5 columns (Submitted, Confirmed, Paid, Shipped, Delivered). Client-side transition validation allows only Confirmed->Shipped and Shipped->Delivered. Invalid transitions show toast error. Valid transitions call `useUpdateOrderStatus` mutation with success toast. Loading skeleton state shows 5 columns with placeholder cards.

- **admin/orders/page.tsx**: Simple page component rendering heading and OrderKanban component.

### Task 2: Admin Order Detail Page with Breadcrumb Navigation

Created a comprehensive admin order detail page:

- **Breadcrumb**: "Orders" (link to /admin/orders) > "Order #{orderNumber}" with ChevronRight icon separator.
- **Header**: Order number, status badge, and conditional action buttons.
- **Status stepper**: Reuses OrderStatusStepper from storefront components.
- **Two-column layout** (large screens): Left column has items table with image, product name, quantity, unit price, line total. Right column has order summary card (subtotal, shipping, tax, total), shipping address card, and buyer email card.
- **Action buttons**: "Mark as Shipped" (Truck icon) visible only for Confirmed orders. "Mark as Delivered" (CheckCircle icon) visible only for Shipped orders. Both use `useUpdateOrderStatus` mutation with loading spinner.
- **Loading state**: Full skeleton layout. **Error state**: "Order not found" with back to kanban link.

## Deviations from Plan

None - plan executed exactly as written.

## Decisions Made

1. **Pointer sensor activation distance of 8px** -- Prevents accidental drags when clicking card links to navigate to detail page.
2. **Separate drag handle from card link** -- GripVertical icon button handles drag, card body is a Link to detail. Avoids DnD/navigation conflict.
3. **Only Confirmed->Shipped and Shipped->Delivered transitions** -- Matches backend constraint where UpdateOrderStatus only accepts "Shipped" and "Delivered" as target statuses.

## Commits

| Hash | Message |
|------|---------|
| f09732ec | feat(08-05): kanban board with DnD columns, cards, and status transition validation |
| 81572462 | feat(08-05): admin order detail page with breadcrumb navigation |
