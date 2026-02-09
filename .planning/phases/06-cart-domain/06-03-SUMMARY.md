---
phase: 06-cart-domain
plan: 03
subsystem: frontend-cart
tags: [react-query, hooks, optimistic-mutations, cart-api]
dependency-graph:
  requires: []
  provides: [react-query-setup, cart-hooks, cart-api-functions]
  affects: [06-04-cart-ui]
tech-stack:
  added: ["@tanstack/react-query@5", "@tanstack/react-query-devtools@5"]
  patterns: [optimistic-mutations, shared-query-key, useState-queryclient]
key-files:
  created:
    - src/MicroCommerce.Web/src/components/providers/query-provider.tsx
    - src/MicroCommerce.Web/src/hooks/use-cart.ts
  modified:
    - src/MicroCommerce.Web/package.json
    - src/MicroCommerce.Web/src/lib/api.ts
    - src/MicroCommerce.Web/src/app/(storefront)/layout.tsx
decisions:
  - useState pattern for QueryClient to prevent SSR state leakage
  - Single ["cart"] query key shared between useCart and useCartItemCount
  - Optimistic mutations with snapshot rollback for update and remove
  - credentials include on all cart fetch calls for cookie-based buyer identity
metrics:
  duration: ~3 minutes
  completed: 2026-02-09
---

# Phase 6 Plan 3: Cart React Query Hooks Summary

React Query v5 installed with QueryProvider wrapping storefront layout, cart API functions with cookie credentials, and 5 cart hooks with optimistic mutations and toast notifications.

## Tasks Completed

### Task 1: Install React Query and create QueryProvider
**Commit:** `498813d6`

- Installed `@tanstack/react-query` and `@tanstack/react-query-devtools`
- Created `QueryProvider` component using `useState(() => new QueryClient({...}))` to avoid SSR state leakage
- Default options: staleTime 60s, retry 1
- Wrapped storefront layout with QueryProvider (outermost wrapper)
- ReactQueryDevtools included with initialIsOpen={false}

### Task 2: Cart API functions and React Query hooks
**Commit:** `645016c7`

- Added cart types: CartDto, CartItemDto, AddToCartRequest, AddToCartResponse, UpdateCartItemRequest
- Added 4 cart API functions all using `credentials: "include"`:
  - `getCart()` - GET /api/cart, returns null on 404
  - `addToCart()` - POST /api/cart/items
  - `updateCartItemQuantity()` - PUT /api/cart/items/{itemId}
  - `removeCartItem()` - DELETE /api/cart/items/{itemId}
- Created 5 React Query hooks:
  - `useCart()` - full cart data query
  - `useCartItemCount()` - derives count from shared ["cart"] cache via select
  - `useAddToCart()` - mutation with invalidation + toast
  - `useUpdateCartItem()` - optimistic update with snapshot/rollback/invalidate
  - `useRemoveCartItem()` - optimistic remove with snapshot/rollback/invalidate

## Deviations from Plan

None - plan executed exactly as written.

## Verification Results

1. `npm run build` succeeds
2. package.json contains @tanstack/react-query and @tanstack/react-query-devtools
3. QueryProvider wraps storefront layout children
4. All cart API functions include `credentials: "include"`
5. useCartItemCount shares ["cart"] query key with useCart
6. useUpdateCartItem and useRemoveCartItem implement optimistic mutation pattern
7. Toast notifications on success and error for all mutations
8. `npx tsc --noEmit` passes with zero errors

## Next Phase Readiness

Plan 06-04 (Cart UI Components) can proceed - all hooks and API functions are available for the cart drawer, cart page, and add-to-cart buttons.
