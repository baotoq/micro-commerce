---
phase: 07-ordering-checkout
plan: 03
subsystem: ordering-saga
tags: [masstransit, saga, state-machine, consumers, checkout, compensation]
depends_on: ["07-01"]
provides: ["checkout-saga", "saga-contracts", "stock-consumers", "order-consumers", "cart-clearing"]
affects: ["07-04", "08-01"]
tech-stack:
  added: []
  patterns: ["saga-state-machine", "compensation-handler", "optimistic-concurrency-saga"]
key-files:
  created:
    - src/MicroCommerce.ApiService/Features/Ordering/Application/Saga/Contracts.cs
    - src/MicroCommerce.ApiService/Features/Ordering/Application/Saga/CheckoutState.cs
    - src/MicroCommerce.ApiService/Features/Ordering/Application/Saga/CheckoutStateMachine.cs
    - src/MicroCommerce.ApiService/Features/Ordering/Infrastructure/Configurations/CheckoutStateConfiguration.cs
    - src/MicroCommerce.ApiService/Features/Ordering/Application/Consumers/ReserveStockForOrderConsumer.cs
    - src/MicroCommerce.ApiService/Features/Ordering/Application/Consumers/DeductStockConsumer.cs
    - src/MicroCommerce.ApiService/Features/Ordering/Application/Consumers/ReleaseStockReservationsConsumer.cs
    - src/MicroCommerce.ApiService/Features/Ordering/Application/Consumers/ConfirmOrderConsumer.cs
    - src/MicroCommerce.ApiService/Features/Ordering/Application/Consumers/OrderFailedConsumer.cs
    - src/MicroCommerce.ApiService/Features/Ordering/Application/Consumers/ClearCartConsumer.cs
  modified:
    - src/MicroCommerce.ApiService/Features/Ordering/Infrastructure/OrderingDbContext.cs
    - src/MicroCommerce.ApiService/Program.cs
decisions:
  - ExistingDbContext for saga persistence in OrderingDbContext
  - Optimistic concurrency via PostgreSQL xmin for saga state
  - JSON serialization for reservation ID map (simple, no extra tables)
  - SetCompletedWhenFinalized to auto-remove completed saga instances
metrics:
  duration: ~5 minutes
  completed: 2026-02-10
---

# Phase 7 Plan 3: Checkout Saga State Machine Summary

MassTransit CheckoutStateMachine with EF Core persistence, 11 saga contracts, and 6 integration consumers orchestrating reserve stock -> payment -> confirm/fail with compensation on failure paths.

## What Was Built

### Saga State Machine
- **CheckoutStateMachine**: MassTransitStateMachine<CheckoutState> with 4 states (Submitted, StockReserved, Confirmed, Failed)
- **CheckoutState**: SagaStateMachineInstance persisted in ordering schema with optimistic concurrency
- **Flow**: Initially -> CheckoutStarted -> publish ReserveStockForOrder -> Submitted -> StockReservationCompleted -> StockReserved -> PaymentCompleted -> publish ConfirmOrder + DeductStock + ClearCart -> Confirmed
- **Compensation**: PaymentFailed -> publish ReleaseStockReservations + OrderFailed -> Failed

### Saga Event Contracts (Contracts.cs)
All 11 message contracts in a single file for saga correlation:
- CheckoutStarted, CheckoutItem (saga trigger)
- ReserveStockForOrder, StockReservationCompleted, StockReservationFailed (stock reservation)
- PaymentCompleted, PaymentFailed (payment outcomes)
- ConfirmOrder, OrderFailed (order status updates)
- DeductStock, ReleaseStockReservations (stock finalization/compensation)
- ClearCart (cart cleanup)

### Integration Consumers
1. **ReserveStockForOrderConsumer**: Reserves stock for each item, handles partial failures by rolling back completed reservations
2. **DeductStockConsumer**: Permanently deducts stock (AdjustStock negative) and releases reservations after payment success
3. **ReleaseStockReservationsConsumer**: Compensation handler - releases all reservations on payment failure
4. **ConfirmOrderConsumer**: Sets order status to Confirmed
5. **OrderFailedConsumer**: Sets order status to Failed (idempotent - skips if already failed)
6. **ClearCartConsumer**: Deletes buyer cart after successful checkout using ExecuteDeleteAsync

### Infrastructure
- CheckoutStateConfiguration: EF Core config for CheckoutSagas table with optimistic concurrency
- OrderingDbContext: Added DbSet<CheckoutState> CheckoutSagas
- Program.cs: Registered saga with EntityFrameworkRepository using OrderingDbContext
- Migration: AddCheckoutSaga creates CheckoutSagas table in ordering schema

## Decisions Made

| Decision | Rationale |
|----------|-----------|
| ExistingDbContext<OrderingDbContext> for saga | Keeps saga state in same schema as orders, no extra DbContext |
| JSON string for reservation IDs | Simple serialization, avoids extra junction table for saga-to-reservation mapping |
| Optimistic concurrency (ConcurrencyMode.Optimistic) | MassTransit auto-retries on concurrency conflicts, better throughput than pessimistic |
| SetCompletedWhenFinalized | Auto-removes completed saga rows, prevents table bloat |
| Idempotent OrderFailedConsumer | Prevents errors if OrderFailed message delivered multiple times |

## Deviations from Plan

None - plan executed exactly as written.

## Commits

| Hash | Type | Description |
|------|------|-------------|
| 5e87febf | feat | Saga state machine, contracts, and EF persistence |
| 0e1547e6 | feat | Integration consumers for saga orchestration |

## Verification Results

- Build: zero errors, zero warnings
- CheckoutStateMachine has all transitions: Initially -> Submitted -> StockReserved -> Confirmed/Failed
- Every failure path publishes compensation events (ReleaseStockReservations on PaymentFailed, OrderFailed on StockReservationFailed)
- ReserveStockForOrderConsumer handles partial failures with rollback
- ClearCartConsumer deletes cart by BuyerId using ExecuteDeleteAsync
- Saga registered in Program.cs with EntityFrameworkRepository and optimistic concurrency
- Migration adds CheckoutSagas table to ordering schema

## Next Phase Readiness

- Saga contracts (PaymentCompleted, PaymentFailed) are ready for SimulatePaymentCommandHandler to publish
- CheckoutStarted contract ready for SubmitOrderCommandHandler to publish
- All consumers auto-discovered by MassTransit assembly scanning
- Frontend checkout page can trigger the saga flow via the ordering API endpoints (built in 07-02)
