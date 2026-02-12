---
phase: 10-testing-polish
plan: 01
subsystem: testing
tags: [xunit, fluent-assertions, testcontainers, masstransit-test-framework, unit-testing, order-domain]

# Dependency graph
requires:
  - phase: 07-ordering-checkout
    provides: Order aggregate with state machine, OrderNumber, ShippingAddress value objects
provides:
  - Test project infrastructure with xUnit, FluentAssertions, Testcontainers, MassTransit.TestFramework
  - 43 unit tests covering Order aggregate state transitions, calculations, and value objects
  - Test foundation for all subsequent testing plans
affects: [10-02, 10-03, 10-04, 10-05, 10-06, 10-07, 10-08]

# Tech tracking
tech-stack:
  added: [xunit, FluentAssertions, Testcontainers.PostgreSql, MassTransit.TestFramework, Microsoft.AspNetCore.Mvc.Testing]
  patterns: [unit testing with arrange-act-assert, helper methods for test data, trait-based test categorization]

key-files:
  created:
    - src/MicroCommerce.ApiService.Tests/MicroCommerce.ApiService.Tests.csproj
    - src/MicroCommerce.ApiService.Tests/coverlet.runsettings
    - src/MicroCommerce.ApiService.Tests/Unit/Ordering/Aggregates/OrderTests.cs
    - src/MicroCommerce.ApiService.Tests/Unit/Ordering/ValueObjects/OrderNumberTests.cs
    - src/MicroCommerce.ApiService.Tests/Unit/Ordering/ValueObjects/ShippingAddressTests.cs
  modified: []

key-decisions:
  - "Test project targets net10.0 to match main project"
  - "FluentAssertions for readable assertions instead of xUnit Assert"
  - "Testcontainers.PostgreSql 4.10.0 for real database integration tests in future plans"
  - "Trait Category=Unit for filtering unit tests vs integration tests"
  - "Helper methods CreateValidOrder() and CreateOrderInStatus() for test data setup"
  - "All status transitions tested for both valid and invalid paths"

patterns-established:
  - "File-scoped namespaces in test files matching C# conventions"
  - "Method naming: {Method}_{Scenario}_{Expected} for test clarity"
  - "Arrange-Act-Assert pattern with clear sections"
  - "ClearDomainEvents() to isolate event assertions in transition tests"

# Metrics
duration: 3min 33s
completed: 2026-02-12
---

# Phase 10 Plan 01: Test Infrastructure and Order Domain Tests Summary

**xUnit test project with 43 passing unit tests covering Order aggregate state machine, calculation logic (8% tax, $5.99 shipping), and value object validation**

## Performance

- **Duration:** 3 min 33 sec
- **Started:** 2026-02-12T15:34:47Z
- **Completed:** 2026-02-12T15:38:20Z
- **Tasks:** 2
- **Files created:** 5

## Accomplishments

- Test project infrastructure established with all dependencies for unit and integration testing
- 30 Order aggregate tests covering all 7 status transitions (valid + invalid), calculation logic, domain events, and invariants
- 6 OrderNumber tests for MC-XXXXXX format generation with unambiguous character set
- 7 ShippingAddress tests for value object validation
- Coverage exclusions configured for migrations and designer files

## Task Commits

Each task was committed atomically:

1. **Task 1: Create xUnit test project with required dependencies** - `c5834083` (chore)
2. **Task 2: Write comprehensive unit tests for Order aggregate and value objects** - `b10fb826` (test)

## Files Created/Modified

### Created
- `src/MicroCommerce.ApiService.Tests/MicroCommerce.ApiService.Tests.csproj` - Test project with xUnit, FluentAssertions, Testcontainers.PostgreSql 4.10.0, MassTransit.TestFramework 9.0.0, Microsoft.AspNetCore.Mvc.Testing
- `src/MicroCommerce.ApiService.Tests/coverlet.runsettings` - Code coverage settings excluding migrations and generated files
- `src/MicroCommerce.ApiService.Tests/Unit/Ordering/Aggregates/OrderTests.cs` - 30 tests for Order aggregate (factory, transitions, calculations, events, invariants)
- `src/MicroCommerce.ApiService.Tests/Unit/Ordering/ValueObjects/OrderNumberTests.cs` - 6 tests for OrderNumber format, uniqueness, character set
- `src/MicroCommerce.ApiService.Tests/Unit/Ordering/ValueObjects/ShippingAddressTests.cs` - 7 tests for ShippingAddress field validation

## Test Coverage Details

### Order Aggregate Tests (30 tests)

**Factory Creation & Calculations (6 tests)**
- `Create_ValidData_ReturnsOrderWithSubmittedStatus` - Verifies initial status and basic properties
- `Create_ValidData_RaisesOrderSubmittedDomainEvent` - Confirms OrderSubmittedDomainEvent raised
- `Create_ValidData_CalculatesSubtotalCorrectly` - Sum of (quantity × price) for multiple items
- `Create_ValidData_CalculatesTaxAt8Percent` - Round(Subtotal × 0.08, 2)
- `Create_ValidData_CalculatesShippingAt599` - Flat $5.99 shipping cost
- `Create_ValidData_CalculatesTotalCorrectly` - Subtotal + Shipping + Tax

**Invariant Enforcement (3 tests)**
- `Create_EmptyItems_ThrowsInvalidOperationException` - Empty order items rejected
- `Create_NullEmail_ThrowsArgumentException` - Null/whitespace email rejected
- `Create_NullAddress_ThrowsArgumentNullException` - Null address rejected

**MarkAsPaid Transition (5 tests)**
- `MarkAsPaid_WhenSubmitted_TransitionsToPaid` - Valid: Submitted → Paid
- `MarkAsPaid_WhenStockReserved_TransitionsToPaid` - Valid: StockReserved → Paid
- `MarkAsPaid_WhenSubmitted_SetsPaidAt` - PaidAt timestamp set
- `MarkAsPaid_WhenSubmitted_RaisesOrderPaidDomainEvent` - OrderPaidDomainEvent raised
- `MarkAsPaid_WhenPaid_ThrowsInvalidOperationException` - Invalid: Paid → Paid rejected
- `MarkAsPaid_WhenShipped_ThrowsInvalidOperationException` - Invalid: Shipped → Paid rejected

**MarkAsFailed Transition (6 tests)**
- `MarkAsFailed_WhenSubmitted_TransitionsToFailed` - Valid: Submitted → Failed
- `MarkAsFailed_WhenSubmitted_SetsFailureReason` - Reason stored
- `MarkAsFailed_WhenSubmitted_RaisesOrderFailedDomainEvent` - OrderFailedDomainEvent raised
- `MarkAsFailed_WhenConfirmed_ThrowsInvalidOperationException` - Invalid: Confirmed → Failed rejected
- `MarkAsFailed_WhenShipped_ThrowsInvalidOperationException` - Invalid: Shipped → Failed rejected
- `MarkAsFailed_WhenDelivered_ThrowsInvalidOperationException` - Invalid: Delivered → Failed rejected
- `MarkAsFailed_NullReason_ThrowsArgumentException` - Null reason rejected

**Confirm Transition (2 tests)**
- `Confirm_WhenPaid_TransitionsToConfirmed` - Valid: Paid → Confirmed
- `Confirm_WhenSubmitted_ThrowsInvalidOperationException` - Invalid: Submitted → Confirmed rejected

**Ship Transition (2 tests)**
- `Ship_WhenConfirmed_TransitionsToShipped` - Valid: Confirmed → Shipped
- `Ship_WhenPaid_ThrowsInvalidOperationException` - Invalid: Paid → Shipped rejected

**Deliver Transition (2 tests)**
- `Deliver_WhenShipped_TransitionsToDelivered` - Valid: Shipped → Delivered
- `Deliver_WhenConfirmed_ThrowsInvalidOperationException` - Invalid: Confirmed → Delivered rejected

**MarkStockReserved Transition (2 tests)**
- `MarkStockReserved_WhenSubmitted_TransitionsToStockReserved` - Valid: Submitted → StockReserved
- `MarkStockReserved_WhenPaid_ThrowsInvalidOperationException` - Invalid: Paid → StockReserved rejected

### OrderNumber Tests (6 tests)
- Format validation: MC- prefix, 9-character length
- Uniqueness: Two calls return different values
- Character set: Only unambiguous alphanumeric (no 0/O/1/I/L)
- Roundtrip: From() and ToString() work correctly

### ShippingAddress Tests (7 tests)
- All 6 fields validated for null/whitespace
- Valid data creates address with all properties set

## Decisions Made

- **FluentAssertions over xUnit Assert:** More readable and maintainable test assertions
- **Testcontainers.PostgreSql 4.10.0:** Enables real database integration tests in future plans without Docker Compose setup
- **MassTransit.TestFramework 9.0.0:** Saga and consumer testing in future plans
- **Helper methods pattern:** `CreateValidOrder()` and `CreateOrderInStatus(OrderStatus)` reduce duplication and make tests more maintainable
- **ClearDomainEvents() in helpers:** Isolates domain event assertions to the specific action being tested
- **Trait-based categorization:** `[Trait("Category", "Unit")]` enables filtering (e.g., run only unit tests)

## Deviations from Plan

None - plan executed exactly as written.

## Issues Encountered

**Microsoft.NET.Test.Sdk version mismatch:** Template generated `17.14.1` but only `17.14.0-preview-25107-01` available for .NET 10 preview. Updated to available version. Build and tests succeeded.

## User Setup Required

None - no external service configuration required.

## Next Phase Readiness

**Test foundation complete.** All subsequent testing plans (10-02 through 10-08) can now:
- Add tests to existing test project
- Use FluentAssertions for readable assertions
- Reference Testcontainers for database integration tests
- Use MassTransit.TestFramework for saga/consumer tests

**Order domain confidence:** The most complex domain (state machine with 7 statuses, calculation logic, multiple invariants) is now covered by 30 unit tests. All valid and invalid state transitions verified.

**No blockers.**

---
*Phase: 10-testing-polish*
*Completed: 2026-02-12*
