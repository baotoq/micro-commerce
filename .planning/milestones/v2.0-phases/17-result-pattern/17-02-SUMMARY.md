---
phase: 17-result-pattern
plan: 02
subsystem: api
tags: [fluent-results, railway-oriented-programming, mediatr, result-pattern, cqrs, domain-driven-design]

# Dependency graph
requires:
  - phase: 17-result-pattern
    plan: 01
    provides: FluentResults infrastructure (ResultExtensions.ToHttpResult, ResultValidationBehavior, ADR-006)

provides:
  - UpdateOrderStatusCommandHandler returning Result instead of void, with try-catch wrapping domain InvalidOperationException
  - AdjustStockCommandHandler returning Result instead of Unit, with try-catch wrapping domain InvalidOperationException
  - Both endpoints using ToHttpResult() for 204 success / 422 business failure HTTP mapping
  - End-to-end proof of the Result pattern pipeline: Command -> ResultValidationBehavior -> Handler -> Result -> ToHttpResult -> HTTP response

affects:
  - Any future handler adoption of Result pattern (demonstrates the pattern for ADOPT-05)
  - Ordering admin workflow (invalid status transitions now 422 not 500)
  - Inventory admin workflow (negative stock adjustments now 422 not 500)

# Tech tracking
tech-stack:
  added: []
  patterns:
    - "Handler pattern: catch InvalidOperationException from domain methods, return Result.Fail(ex.Message)"
    - "ADR-006 boundary in practice: business rule InvalidOperationException -> Result.Fail (422), infrastructure NotFoundException -> throw (404), ConflictException -> throw (409)"
    - "Endpoint pattern: Result result = await sender.Send(command, ct); return result.ToHttpResult();"

key-files:
  created: []
  modified:
    - src/MicroCommerce.ApiService/Features/Ordering/Application/Commands/UpdateOrderStatus/UpdateOrderStatusCommand.cs
    - src/MicroCommerce.ApiService/Features/Ordering/Application/Commands/UpdateOrderStatus/UpdateOrderStatusCommandHandler.cs
    - src/MicroCommerce.ApiService/Features/Ordering/OrderingEndpoints.cs
    - src/MicroCommerce.ApiService/Features/Inventory/Application/Commands/AdjustStock/AdjustStockCommand.cs
    - src/MicroCommerce.ApiService/Features/Inventory/Application/Commands/AdjustStock/AdjustStockCommandHandler.cs
    - src/MicroCommerce.ApiService/Features/Inventory/InventoryEndpoints.cs

key-decisions:
  - "Domain methods (Ship, Deliver, AdjustStock) keep throwing InvalidOperationException — handlers catch and convert to Result.Fail (adapter pattern at handler boundary)"
  - "Invalid status string in UpdateOrderStatus switch default now returns Result.Fail instead of throwing InvalidOperationException (consistent with Result pattern adoption)"
  - "Pre-existing integration test failures (29 tests) caused by masstransit-bus health check duplicate registration in WebApplicationFactory — confirmed pre-existing, unrelated to Result pattern changes"

patterns-established:
  - "Result handler pattern: wrap domain method calls in try-catch InvalidOperationException, return Result.Fail(ex.Message)"
  - "Endpoint Result pattern: capture Result from sender.Send, call result.ToHttpResult() — replaces direct Results.NoContent()"
  - "OpenAPI annotation: add ProducesProblem(422) alongside existing success Produces for commands that return Result"

requirements-completed: [PRIM-05, PRIM-06]

# Metrics
duration: 4min
completed: 2026-02-24
---

# Phase 17 Plan 02: Result Pattern Pilot - UpdateOrderStatus and AdjustStock Summary

**UpdateOrderStatusCommandHandler and AdjustStockCommandHandler now return FluentResults.Result, converting domain InvalidOperationExceptions to Result.Fail at the handler boundary, with endpoints using ToHttpResult() for 204 success / 422 business failure HTTP mapping.**

## Performance

- **Duration:** ~4 min
- **Started:** 2026-02-24T12:59:10Z
- **Completed:** 2026-02-24T13:03:10Z
- **Tasks:** 2
- **Files modified:** 6

## Accomplishments

- UpdateOrderStatusCommand changed from `IRequest` (void) to `IRequest<Result>` with handler catching `InvalidOperationException` from `order.Ship()` and `order.Deliver()`, returning `Result.Fail(ex.Message)` instead of letting exceptions propagate to GlobalExceptionHandler as 500s
- Invalid status string in switch default now returns `Result.Fail` (422) instead of throwing `InvalidOperationException` (500)
- AdjustStockCommand changed from `IRequest<Unit>` to `IRequest<Result>` with handler catching `InvalidOperationException` from `stockItem.AdjustStock()`, returning `Result.Fail(ex.Message)` for negative stock violations
- Both endpoints updated to capture `Result result = await sender.Send(...)` and call `result.ToHttpResult()`, plus `ProducesProblem(422)` added to endpoint registrations
- Full end-to-end pipeline proven: Command -> ResultValidationBehavior -> Handler -> Result -> ToHttpResult -> HTTP response (204 or 422)
- NotFoundException and ConflictException still propagate as exceptions per ADR-006 boundary (infrastructure failures, not business rule violations)

## Task Commits

Each task was committed atomically:

1. **Task 1: Migrate UpdateOrderStatusCommand handler to return Result** - `da91d2fe` (feat)
2. **Task 2: Migrate AdjustStockCommand handler to return Result** - `c72d9e37` (feat)

## Files Created/Modified

- `src/MicroCommerce.ApiService/Features/Ordering/Application/Commands/UpdateOrderStatus/UpdateOrderStatusCommand.cs` - Changed from `IRequest` to `IRequest<Result>`, added FluentResults using
- `src/MicroCommerce.ApiService/Features/Ordering/Application/Commands/UpdateOrderStatus/UpdateOrderStatusCommandHandler.cs` - Changed to `IRequestHandler<UpdateOrderStatusCommand, Result>`, try-catch wrapping Ship/Deliver calls, Result.Fail for invalid status, Result.Ok() at end
- `src/MicroCommerce.ApiService/Features/Ordering/OrderingEndpoints.cs` - Added FluentResults/ResultExtensions usings, changed to capture Result and call ToHttpResult(), added ProducesProblem(422)
- `src/MicroCommerce.ApiService/Features/Inventory/Application/Commands/AdjustStock/AdjustStockCommand.cs` - Changed from `IRequest<Unit>` to `IRequest<Result>`, added FluentResults using
- `src/MicroCommerce.ApiService/Features/Inventory/Application/Commands/AdjustStock/AdjustStockCommandHandler.cs` - Changed to `IRequestHandler<AdjustStockCommand, Result>`, try-catch wrapping AdjustStock call, replaced `return Unit.Value` with `return Result.Ok()`, fixed var to explicit types
- `src/MicroCommerce.ApiService/Features/Inventory/InventoryEndpoints.cs` - Added FluentResults/ResultExtensions usings, changed to capture Result and call ToHttpResult(), added ProducesProblem(422)

## Decisions Made

- Domain methods (`Ship()`, `Deliver()`, `AdjustStock()`) keep throwing `InvalidOperationException` — the handler acts as the adapter layer, catching and converting to `Result.Fail`. This is the "pilot adoption in command handlers — domain methods keep throwing" pattern from the research.
- The invalid status string in the `switch` default case was previously `throw new InvalidOperationException(...)` — changed to `return Result.Fail(...)` for consistency with the Result pattern (it's a business rule violation, not an exceptional condition).
- Pre-existing integration test failures (29 out of 173 tests): confirmed via git stash that the "Duplicate health checks were registered with the name(s): masstransit-bus" error existed before our changes. Out of scope per deviation rules (pre-existing, not caused by our changes).

## Deviations from Plan

None - plan executed exactly as written.

## Issues Encountered

Pre-existing integration test failures (29 tests) caused by `Duplicate health checks registered: masstransit-bus` in `ApiWebApplicationFactory.InitializeAsync`. Confirmed pre-existing via git stash verification. Not caused by our Result pattern changes and out of scope per deviation rules.

## User Setup Required

None - no external service configuration required.

## Next Phase Readiness

- Result pattern fully proven end-to-end with real handlers: Command -> ResultValidationBehavior -> Handler (catch domain exceptions) -> Result -> ToHttpResult -> HTTP 204/422
- Pattern is established and documented for future handler adoption (ADOPT-05)
- ADR-006 boundary in practice: invalid business transitions return 422, not-found returns 404, concurrency returns 409, input validation returns 400
- Phase 17 complete — all 2 plans done

## Self-Check: PASSED

Files verified:
- FOUND: src/MicroCommerce.ApiService/Features/Ordering/Application/Commands/UpdateOrderStatus/UpdateOrderStatusCommand.cs (contains IRequest<Result>)
- FOUND: src/MicroCommerce.ApiService/Features/Ordering/Application/Commands/UpdateOrderStatus/UpdateOrderStatusCommandHandler.cs (contains Result.Fail)
- FOUND: src/MicroCommerce.ApiService/Features/Ordering/OrderingEndpoints.cs (contains ToHttpResult)
- FOUND: src/MicroCommerce.ApiService/Features/Inventory/Application/Commands/AdjustStock/AdjustStockCommand.cs (contains IRequest<Result>)
- FOUND: src/MicroCommerce.ApiService/Features/Inventory/Application/Commands/AdjustStock/AdjustStockCommandHandler.cs (contains Result.Fail)
- FOUND: src/MicroCommerce.ApiService/Features/Inventory/InventoryEndpoints.cs (contains ToHttpResult)

Commits verified:
- da91d2fe: feat(17-02): migrate UpdateOrderStatusCommand handler to return Result
- c72d9e37: feat(17-02): migrate AdjustStockCommand handler to return Result

---
*Phase: 17-result-pattern*
*Completed: 2026-02-24*
