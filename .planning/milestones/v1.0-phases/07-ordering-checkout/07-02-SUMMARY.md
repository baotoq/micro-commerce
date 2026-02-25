---
phase: 07-ordering-checkout
plan: 02
subsystem: ordering
tags: [cqrs, mediatr, minimal-api, fluentvalidation, masstransit]
dependency-graph:
  requires: [07-01]
  provides: [ordering-api-endpoints, submit-order-command, payment-simulation, order-query]
  affects: [07-03, 07-04, 08-01]
tech-stack:
  added: []
  patterns: [cqrs-command-handler, cqrs-query-handler, fluent-validation, minimal-api-endpoints]
key-files:
  created:
    - src/MicroCommerce.ApiService/Features/Ordering/Application/Commands/SubmitOrder/SubmitOrderCommand.cs
    - src/MicroCommerce.ApiService/Features/Ordering/Application/Commands/SubmitOrder/SubmitOrderCommandHandler.cs
    - src/MicroCommerce.ApiService/Features/Ordering/Application/Commands/SubmitOrder/SubmitOrderCommandValidator.cs
    - src/MicroCommerce.ApiService/Features/Ordering/Application/Commands/SimulatePayment/SimulatePaymentCommand.cs
    - src/MicroCommerce.ApiService/Features/Ordering/Application/Commands/SimulatePayment/SimulatePaymentCommandHandler.cs
    - src/MicroCommerce.ApiService/Features/Ordering/Application/Queries/GetOrderById/GetOrderByIdQuery.cs
    - src/MicroCommerce.ApiService/Features/Ordering/Application/Queries/GetOrderById/GetOrderByIdQueryHandler.cs
    - src/MicroCommerce.ApiService/Features/Ordering/Application/Queries/GetOrderById/OrderDto.cs
    - src/MicroCommerce.ApiService/Features/Ordering/OrderingEndpoints.cs
    - src/MicroCommerce.ApiService/Features/Ordering/Contracts.cs
  modified:
    - src/MicroCommerce.ApiService/Program.cs
decisions:
  - PaymentCompleted/PaymentFailed contracts placed in Contracts.cs for saga plan compatibility
metrics:
  duration: ~2 minutes
  completed: 2026-02-10
---

# Phase 07 Plan 02: Ordering CQRS & API Endpoints Summary

Three working API endpoints under /api/ordering with full MediatR CQRS stack, FluentValidation, and MassTransit saga event publishing.

## What Was Built

### Task 1: SubmitOrder and SimulatePayment Commands
- **SubmitOrderCommand** with ShippingAddressRequest, OrderItemRequest, and SubmitOrderResult records
- **SubmitOrderCommandHandler** maps requests to domain value objects, calls Order.Create factory, persists, and returns order ID + number
- **SubmitOrderCommandValidator** validates email format, all shipping address fields, items not empty, each item's productId/name/price/quantity
- **SimulatePaymentCommand** with ShouldSucceed flag and SimulatePaymentResult
- **SimulatePaymentCommandHandler** loads order, calls MarkAsPaid or MarkAsFailed, publishes PaymentCompleted/PaymentFailed via MassTransit
- **Contracts.cs** defines PaymentCompleted and PaymentFailed message contracts for saga consumption

### Task 2: GetOrderById Query and OrderingEndpoints
- **OrderDto** with nested ShippingAddressDto and OrderItemDto for full order representation
- **GetOrderByIdQuery** and handler with eager loading of Items collection
- **OrderingEndpoints** registers three routes:
  - `POST /api/ordering/checkout` - creates order from cart items via BuyerIdentity
  - `POST /api/ordering/orders/{id}/pay` - simulates payment success/failure
  - `GET /api/ordering/orders/{id}` - retrieves full order details
- **CheckoutRequest** and **PaymentRequest** endpoint contract records
- MapOrderingEndpoints registered in Program.cs

## Decisions Made

| Decision | Rationale |
|----------|-----------|
| PaymentCompleted/PaymentFailed in Contracts.cs | Shared location for both CQRS plan (07-02) and saga plan (07-03) to reference |
| BuyerIdentity reused from Cart module | Consistent buyer identification across cart and ordering |
| CheckoutRequest in OrderingEndpoints.cs | Follows CartEndpoints pattern of co-locating request DTOs |

## Deviations from Plan

None - plan executed exactly as written.

## Verification Results

1. `dotnet build` - zero errors, zero warnings
2. OrderingEndpoints registers 3 routes: POST /checkout, POST /orders/{id}/pay, GET /orders/{id}
3. SubmitOrderCommandValidator validates email, shipping fields, items with proper error messages
4. SimulatePaymentCommandHandler publishes PaymentCompleted/PaymentFailed messages
5. GetOrderByIdQueryHandler returns OrderDto with all items, shipping address, and calculated totals

## Commits

| Commit | Type | Description |
|--------|------|-------------|
| 7e682068 | feat | Submit order and simulate payment commands |
| f581eb3c | feat | Order query and ordering API endpoints |

## Next Phase Readiness

- Saga plan (07-03) can consume PaymentCompleted/PaymentFailed from Contracts.cs
- Checkout UI plan (07-04) can call POST /checkout and GET /orders/{id}
- All endpoints ready for frontend integration
