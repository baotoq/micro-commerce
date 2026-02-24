---
phase: 17-result-pattern
verified: 2026-02-24T14:00:00Z
status: passed
score: 5/5 must-haves verified
re_verification: false
human_verification:
  - test: "Send HTTP PATCH to /orders/{id}/status with an already-delivered order to trigger Ship() InvalidOperationException"
    expected: "HTTP 422 response with ProblemDetails body containing the domain error message"
    why_human: "Cannot verify runtime domain state transition behavior via static analysis"
  - test: "Send HTTP POST to /inventory/stock/{productId}/adjust with a negative adjustment that would bring stock below zero"
    expected: "HTTP 422 response with ProblemDetails body containing the negative-stock domain error message"
    why_human: "Requires live Postgres + seeded stock data to exercise the domain invariant"
  - test: "Send a request with both ProductId empty AND Adjustment=0 to AdjustStock endpoint"
    expected: "HTTP 400 (input validation via ValidationBehavior) containing both validation error messages, demonstrating dual-behavior coexistence"
    why_human: "Interaction between ValidationBehavior and ResultValidationBehavior for the same Result-typed handler needs runtime confirmation"
---

# Phase 17: Result Pattern Verification Report

**Phase Goal:** Introduce Result type for railway-oriented programming with pilot adoption in command handlers
**Verified:** 2026-02-24T14:00:00Z
**Status:** PASSED
**Re-verification:** No — initial verification

## Goal Achievement

### Observable Truths

| # | Truth | Status | Evidence |
|---|-------|--------|----------|
| 1 | FluentResults Result/Result<T> types available in BuildingBlocks.Common with extension methods for HTTP response mapping | VERIFIED | `FluentResults 4.0.0` in `BuildingBlocks.Common.csproj` line 9; `ResultExtensions.cs` exports `ToHttpResult` for both `Result` and `Result<T>` |
| 2 | ResultValidationBehavior coexists with existing ValidationBehavior in MediatR pipeline based on TResponse constraint | VERIFIED | `Program.cs` lines 189+192 register both behaviors; `ResultValidationBehavior.cs` line 21 carries `where TResponse : IResultBase` constraint |
| 3 | Two command handlers (pilot) return Result instead of throwing exceptions for business rule violations | VERIFIED | `UpdateOrderStatusCommandHandler` and `AdjustStockCommandHandler` both implement `IRequestHandler<TCommand, Result>` and call `Result.Fail(...)` for domain violations |
| 4 | ADR documents Result/Exception boundary: business rules return Result, invalid input throws exception | VERIFIED | `docs/decisions/adr-006-result-exception-boundary.md` exists, Status: Accepted, categorisation table present |
| 5 | Pilot handlers demonstrate clear error aggregation with multiple Result.Failure reasons | VERIFIED | `ResultValidationBehavior.cs` lines 55-59 build `List<IError>` from all validation failures and pass the full list to `Result.Fail(errors)`; `UpdateOrderStatusCommandHandler` has 3 independent `Result.Fail` paths (Ship, Deliver, invalid status string) |

**Score:** 5/5 truths verified

### Required Artifacts

#### Plan 01 Artifacts

| Artifact | Expected | Status | Details |
|----------|----------|--------|---------|
| `src/BuildingBlocks/BuildingBlocks.Common/BuildingBlocks.Common.csproj` | FluentResults 4.0.0 package reference | VERIFIED | `<PackageReference Include="FluentResults" Version="4.0.0" />` present at line 9 |
| `src/MicroCommerce.ApiService/Common/Extensions/ResultExtensions.cs` | HTTP response mapping for Result types | VERIFIED | 56 lines; exports `ToHttpResult(this Result)` and `ToHttpResult<T>(this Result<T>)` with private `ToFailureProblem` producing 422 ProblemDetails |
| `src/MicroCommerce.ApiService/Common/Behaviors/ResultValidationBehavior.cs` | MediatR pipeline behavior for Result-typed handlers | VERIFIED | 61 lines; `sealed class` with `where TResponse : IResultBase` constraint, aggregates `List<IError>` from all validation failures |
| `docs/decisions/adr-006-result-exception-boundary.md` | Architecture decision record for Result vs Exception boundary | VERIFIED | Accepted, dated 2026-02-24, contains categorisation table and consequences including limitations of `Result<T>` in validation short-circuit |

#### Plan 02 Artifacts

| Artifact | Expected | Status | Details |
|----------|----------|--------|---------|
| `src/MicroCommerce.ApiService/Features/Ordering/Application/Commands/UpdateOrderStatus/UpdateOrderStatusCommand.cs` | Command returning `IRequest<Result>` | VERIFIED | Line 7: `public sealed record UpdateOrderStatusCommand(...) : IRequest<Result>;` |
| `src/MicroCommerce.ApiService/Features/Ordering/Application/Commands/UpdateOrderStatus/UpdateOrderStatusCommandHandler.cs` | Handler using `Result.Fail` for invalid transitions | VERIFIED | 3 `Result.Fail` calls (lines 32, 42, 46) covering Ship failure, Deliver failure, invalid status string; `Result.Ok()` at end |
| `src/MicroCommerce.ApiService/Features/Inventory/Application/Commands/AdjustStock/AdjustStockCommand.cs` | Command returning `IRequest<Result>` | VERIFIED | Line 10: `public sealed record AdjustStockCommand(...) : IRequest<Result>;` |
| `src/MicroCommerce.ApiService/Features/Inventory/Application/Commands/AdjustStock/AdjustStockCommandHandler.cs` | Handler using `Result.Fail` for negative stock | VERIFIED | Line 39: `return Result.Fail(ex.Message)` inside `catch (InvalidOperationException)`; `Result.Ok()` at line 60; `ConflictException` still throws (line 57) per ADR boundary |

### Key Link Verification

#### Plan 01 Key Links

| From | To | Via | Status | Details |
|------|----|-----|--------|---------|
| `ResultValidationBehavior.cs` | `FluentResults.IResultBase` | `where TResponse : IResultBase` generic constraint | VERIFIED | Line 21: `where TResponse : IResultBase` — only activates for Result-returning handlers |
| `Program.cs` | `ResultValidationBehavior` | `AddOpenBehavior` after `ValidationBehavior` | VERIFIED | Lines 189+192: `ValidationBehavior<,>` registered first, `ResultValidationBehavior<,>` registered second |
| `ResultExtensions.cs` | `FluentResults.Result` | extension method on `Result` and `Result<T>` | VERIFIED | Line 13: `this Result result`; line 30: `this Result<T> result` — both overloads present |

#### Plan 02 Key Links

| From | To | Via | Status | Details |
|------|----|-----|--------|---------|
| `OrderingEndpoints.cs` | `ResultExtensions.ToHttpResult` | `result.ToHttpResult()` on command Result | VERIFIED | Line 153: `return result.ToHttpResult();` after capturing `Result result = await sender.Send(...)` |
| `InventoryEndpoints.cs` | `ResultExtensions.ToHttpResult` | `result.ToHttpResult()` on command Result | VERIFIED | Line 111: `return result.ToHttpResult();` after capturing `Result result = await sender.Send(...)` |
| `UpdateOrderStatusCommandHandler` | `Order.Ship/Order.Deliver` | try-catch wrapping domain InvalidOperationException | VERIFIED | Lines 26-34 (Ship try-catch), lines 36-44 (Deliver try-catch); both `catch (InvalidOperationException ex)` patterns present |
| `AdjustStockCommandHandler` | `StockItem.AdjustStock` | try-catch wrapping domain InvalidOperationException | VERIFIED | Lines 33-40: `try { stockItem.AdjustStock(...) } catch (InvalidOperationException ex) { return Result.Fail(ex.Message); }` |

### Requirements Coverage

| Requirement | Source Plan | Description | Status | Evidence |
|-------------|-------------|-------------|--------|----------|
| PRIM-05 | 17-01-PLAN.md, 17-02-PLAN.md | Result type (FluentResults) integrated into BuildingBlocks with Result extensions for HTTP responses | SATISFIED | FluentResults 4.0.0 in `BuildingBlocks.Common.csproj`; `ResultExtensions.cs` with `ToHttpResult` mapping success to 204 and failure to 422 ProblemDetails |
| PRIM-06 | 17-01-PLAN.md, 17-02-PLAN.md | ResultValidationBehavior for MediatR pipeline coexisting with existing ValidationBehavior | SATISFIED | Both behaviors registered in `Program.cs`; `IResultBase` constraint ensures non-Result handlers are unaffected; build succeeds with zero errors |

Both requirements are marked complete in `REQUIREMENTS.md` (lines 24-25 and 90-91).

### Anti-Patterns Found

| File | Pattern | Severity | Impact |
|------|---------|----------|--------|
| `ResultValidationBehavior.cs` line 15 | Comment notes `Result<T>` validation short-circuit deferred to ADOPT-05 | Info | Intentional known limitation; documented in ADR-006. Non-generic `Result` pilot is correct scope for Phase 17 |
| Build output | 4 warnings: `SixLabors.ImageSharp` vulnerability (NU1902/NU1903) | Info | Pre-existing, unrelated to Phase 17 changes. Zero error count confirms no regressions |

No blocker or warning-level anti-patterns found in phase 17 files.

### Human Verification Required

#### 1. UpdateOrderStatus 422 on domain violation

**Test:** Start the Aspire stack, ship an order, then attempt to ship it again (already-shipped order cannot be shipped twice via `order.Ship()`)
**Expected:** HTTP 422 Unprocessable Entity response with body `{"status":422,"title":"Business Rule Violation","detail":"<domain error message>"}`
**Why human:** Requires running Postgres + Keycloak + seeded order data; `order.Ship()` guard in the domain entity throws `InvalidOperationException` which handler converts to `Result.Fail`

#### 2. AdjustStock 422 on negative stock

**Test:** Adjust stock for a product by a negative value larger than current stock on hand
**Expected:** HTTP 422 Unprocessable Entity with body containing the negative-stock domain invariant message
**Why human:** Requires live database with seeded stock data to exercise the `StockItem.AdjustStock` invariant path

#### 3. Dual-behavior coexistence with Result-returning handler

**Test:** POST to `/inventory/stock/{productId}/adjust` with empty `productId` (null GUID) and `adjustment: 0` simultaneously
**Expected:** HTTP 400 Bad Request (from `ValidationBehavior`) containing both validation errors, NOT a 422; confirming `ValidationBehavior` still fires for Result-typed handlers when validation throws
**Why human:** The coexistence note in ADR-006 (lines 40-43) acknowledges complexity — a handler returning `Result` technically could be hit by both behaviors. Runtime confirmation ensures the ordering is correct.

### Gaps Summary

No gaps. All 5 success criteria verified against actual codebase:

- FluentResults 4.0.0 is installed and available project-wide.
- `ResultExtensions.cs` maps `Result` success to 204 and failure to 422 ProblemDetails.
- `ResultValidationBehavior` carries `IResultBase` constraint and aggregates multiple errors into a single `Result.Fail` call.
- Both behaviors are registered in `Program.cs` in the correct order.
- `UpdateOrderStatusCommandHandler` and `AdjustStockCommandHandler` return `Result` with explicit `Result.Fail` for business violations and preserve `NotFoundException`/`ConflictException` throws per ADR boundary.
- Both endpoints use `result.ToHttpResult()` and declare `ProducesProblem(422)`.
- ADR-006 is a substantive, accepted document with decision table, rule summary, and consequences.
- Commits `a3bd6101`, `0f51202d`, `da91d2fe`, `c72d9e37` all exist in git history.
- `dotnet build` succeeds with 0 errors, 4 pre-existing unrelated warnings.

Three items are flagged for human verification (runtime behavior) but do not block the goal.

---

_Verified: 2026-02-24T14:00:00Z_
_Verifier: Claude (gsd-verifier)_
