---
phase: 14-integration-polish
plan: 01
subsystem: reviews
tags: [frontend, ui-polish, reviews, orders]
dependency_graph:
  requires: [12-03-review-ui, 10-03-order-detail]
  provides: [consolidated-review-experience]
  affects: [order-detail-page, review-workflow]
tech_stack:
  added: []
  patterns: [sub-component-hooks, per-item-state-management]
key_files:
  created:
    - src/MicroCommerce.Web/src/app/(storefront)/orders/[id]/review/page.tsx
    - src/MicroCommerce.Web/src/components/storefront/order-review-page.tsx
  modified:
    - src/MicroCommerce.Web/src/components/storefront/order-detail.tsx
decisions:
  - "OrderItemReviewRow sub-component pattern allows per-item hook calls (React hooks rules compliance)"
  - "CanReviewDto logic: hasPurchased && !hasReviewed determines review eligibility"
  - "Review Products button only appears for Paid/Confirmed/Shipped/Delivered orders"
  - "Back link navigates to order detail, not orders list"
metrics:
  duration_minutes: 2
  tasks_completed: 2
  files_created: 2
  files_modified: 1
  commits: 2
  completed_date: 2026-02-13
---

# Phase 14 Plan 01: Consolidated Review Experience Summary

**One-liner:** Single "Review Products" button per order leading to dedicated page showing all items with review status and forms.

## What Was Built

Created a consolidated review experience that replaces per-item "Write a Review" links in order details with a single "Review Products" button. The button navigates to a new `/orders/[orderId]/review` page showing all order items with their review status, allowing users to submit or edit reviews for each product in one place.

### Key Components

**Order Review Route** (`orders/[id]/review/page.tsx`)
- Client component extracting orderId from params
- Renders OrderReviewPage component

**OrderReviewPage Component** (`order-review-page.tsx`)
- Fetches order data using useOrderWithPolling hook
- Displays page title "Review Products" with order number
- Back link to order detail
- List of order items with OrderItemReviewRow sub-components
- Loading and error states with appropriate UI feedback

**OrderItemReviewRow Sub-component**
- Per-item hooks: useMyReview and useCanReview for independent state
- Shows product image, name, quantity, and price
- Displays review status:
  - Already reviewed: StarRatingDisplay + "Edit Review" button
  - Not yet reviewed (eligible): "Write a Review" button
  - Not eligible: Purchase verification message
- Opens ReviewFormDialog for both create and edit flows

**Order Detail Updates**
- Removed per-item "Write a Review" links from order items
- Added single "Review Products" button at bottom of items card
- Button appears only for reviewable orders (Paid/Confirmed/Shipped/Delivered status)
- Links to `/orders/{orderId}/review`

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 1 - Bug] Fixed CanReviewDto property access**
- **Found during:** Task 1 TypeScript compilation
- **Issue:** Code tried to access `canReviewData?.canReview` but CanReviewDto has `hasPurchased` and `hasReviewed` properties
- **Fix:** Changed logic to `canReviewData?.hasPurchased && !canReviewData?.hasReviewed`
- **Files modified:** src/MicroCommerce.Web/src/components/storefront/order-review-page.tsx
- **Commit:** 62a1b8d1 (included in Task 1 commit)

## Implementation Highlights

### Sub-Component Hook Pattern
Used OrderItemReviewRow as a sub-component to call useMyReview and useCanReview hooks per item. This approach complies with React hooks rules (hooks at component level) while maintaining per-item state independence.

### Review Eligibility Logic
The component determines review eligibility by combining two conditions:
- `hasPurchased`: User has purchased the product (verified via Orders service)
- `!hasReviewed`: User hasn't already reviewed this product

This logic correctly implements purchase verification gating.

### UI/UX Consistency
- Minimal design with border-b separators between items
- Rounded-full buttons matching existing design system
- Generous spacing (space-y-6) for comfortable reading
- Loading skeletons matching final layout structure
- Error states with actionable "Back to Orders" link

## Success Criteria Met

- [x] Per-item "Write a Review" links removed from order detail
- [x] Single "Review Products" button present on order detail for reviewable orders
- [x] /orders/[orderId]/review page shows all order items with review forms
- [x] Already-reviewed items show their rating with edit capability
- [x] Items not yet reviewed show "Write a Review" button
- [x] ReviewFormDialog integration works for both new and edit review flows
- [x] TypeScript compiles without errors
- [x] Back link navigates to order detail

## Technical Notes

### Hook Organization
Each OrderItemReviewRow independently manages its review state through dedicated hooks. This prevents parent component from making N calls to useMyReview/useCanReview (which would violate hooks rules).

### Route Structure
The `/orders/[id]/review` route follows Next.js App Router dynamic segment pattern. The page component is client-side due to need for useOrderWithPolling hook.

### Loading States
Loading skeletons are provided at both page level (when order is loading) and row level (when review status is loading). This ensures smooth progressive loading experience.

## Files Summary

**Created (2 files, 208 lines)**
- `src/MicroCommerce.Web/src/app/(storefront)/orders/[id]/review/page.tsx` (13 lines)
- `src/MicroCommerce.Web/src/components/storefront/order-review-page.tsx` (195 lines)

**Modified (1 file)**
- `src/MicroCommerce.Web/src/components/storefront/order-detail.tsx` (simplified items rendering, added Review Products button)

## Commits

| Task | Commit | Description |
|------|--------|-------------|
| 1 | 62a1b8d1 | feat(14-01): create order review products page and component |
| 2 | 01c4ac22 | feat(14-01): replace per-item review links with single review products button |

## Self-Check: PASSED

**Files exist:**
```
✓ src/MicroCommerce.Web/src/app/(storefront)/orders/[id]/review/page.tsx
✓ src/MicroCommerce.Web/src/components/storefront/order-review-page.tsx
✓ src/MicroCommerce.Web/src/components/storefront/order-detail.tsx (modified)
```

**Commits exist:**
```
✓ 62a1b8d1: feat(14-01): create order review products page and component
✓ 01c4ac22: feat(14-01): replace per-item review links with single review products button
```

**TypeScript compilation:**
```
✓ No compilation errors
```

**Verification checks:**
```
✓ Per-item "Write a Review" links: 0 occurrences (removed)
✓ Single "Review Products" button: 1 occurrence (present)
✓ Link to /orders/[id]/review: verified
✓ ReviewFormDialog integration: present in order-review-page.tsx
```
