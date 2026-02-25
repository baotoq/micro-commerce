# Phase 17: Result Pattern - Research

**Researched:** 2026-02-24
**Domain:** Railway-oriented programming with FluentResults in a MediatR + Minimal API codebase
**Confidence:** HIGH

---

## Summary

Phase 17 introduces the Result pattern via the FluentResults library into BuildingBlocks.Common and pilots it on two command handlers. The current codebase uses throw-based error handling exclusively: domain methods throw `InvalidOperationException` for business rule violations, and command handlers propagate these to the `GlobalExceptionHandler` which maps them to HTTP 422/400 responses. The phase creates an explicit boundary — business-rule failures return `Result`, programmer errors and infrastructure failures still throw.

The FluentResults library (latest stable: 4.0.0, released June 2025) is the mandated choice per PRIM-05. It ships `Result` (non-generic) and `Result<T>` (generic) with fluent error aggregation, custom error types, and `Result.Merge` for combining multiple failures. The key integration challenge is wiring a `ResultValidationBehavior` into the MediatR pipeline that only activates for requests whose response type is `Result` or `Result<T>`, coexisting with the existing `ValidationBehavior` which activates for all requests.

No CONTEXT.md was found for this phase, meaning no locked user decisions exist. All design choices are at Claude's discretion, informed by PRIM-05/PRIM-06 requirements and this research.

**Primary recommendation:** Add `FluentResults 4.0.0` to `BuildingBlocks.Common.csproj`, add extension methods for `IResult` HTTP mapping on the `ApiService` side, implement `ResultValidationBehavior<TRequest, TResponse> where TResponse : IResultBase`, and pilot on `ReserveStockCommandHandler` (insufficient stock — multiple failure reasons) and `UpdateOrderStatusCommandHandler` (invalid status transitions — error aggregation opportunity).

---

<phase_requirements>

## Phase Requirements

| ID | Description | Research Support |
|----|-------------|-----------------|
| PRIM-05 | Result type (FluentResults) integrated into BuildingBlocks with Result extensions for HTTP responses | FluentResults 4.0.0 API confirmed; extension method pattern for Minimal API `IResult` mapping researched; placement in BuildingBlocks.Common and HTTP extension methods in ApiService Common |
| PRIM-06 | ResultValidationBehavior for MediatR pipeline coexisting with existing ValidationBehavior | MediatR `IPipelineBehavior<TRequest, TResponse> where TResponse : IResultBase` constraint pattern confirmed; existing ValidationBehavior throws exception — new behavior returns Result failures for Result-typed handlers |

</phase_requirements>

---

## Standard Stack

### Core

| Library | Version | Purpose | Why Standard |
|---------|---------|---------|--------------|
| FluentResults | 4.0.0 | `Result`/`Result<T>` types, error aggregation, `Result.Merge` | Only maintained .NET library with object-oriented error hierarchies, multiple error accumulation, and `IResultBase` interface for generic constraints. Mandated by PRIM-05. |

### Supporting (Already Present)

| Library | Version | Purpose | When to Use |
|---------|---------|---------|-------------|
| MediatR | 13.1.0 | Pipeline behaviors, `IPipelineBehavior<TRequest, TResponse>` | `ResultValidationBehavior` uses MediatR's pipeline constraint mechanism |
| FluentValidation | 12.1.1 | Validators used by `ValidationBehavior` | Existing behavior remains; `ResultValidationBehavior` reads validation failures for Result-typed handlers |
| ASP.NET Core Minimal APIs | .NET 10 | `IResult` HTTP responses via `Results.Problem(...)` | HTTP extension methods map `Result` errors to `Results.Problem(statusCode: 422)` |

### Alternatives Considered

| Instead of | Could Use | Tradeoff |
|------------|-----------|----------|
| FluentResults | Custom Result<T> class | Rolling custom requires ongoing maintenance; FluentResults has `IResultBase` interface (needed for MediatR constraint), `Result.Merge`, `WithError` chaining, and established NuGet ecosystem |
| FluentResults | OneOf / ErrorOr | OneOf is union-type discriminated; ErrorOr is simpler but lacks `IResultBase` interface crucial for MediatR generic constraint |
| FluentResults 4.0.0 | FluentResults 3.15.x | v4 has breaking Deconstruct operator changes and strict empty-list validation; since this is first adoption there is no migration cost — use 4.0.0 directly |

**Installation:**

```bash
# In BuildingBlocks.Common
dotnet add /Users/baotoq/Work/micro-commerce/src/BuildingBlocks/BuildingBlocks.Common/BuildingBlocks.Common.csproj package FluentResults --version 4.0.0
```

FluentResults goes in `BuildingBlocks.Common` (already referenced by `ApiService`). HTTP extension methods live in `ApiService` because they depend on `Microsoft.AspNetCore` which must NOT be in BuildingBlocks.

---

## Architecture Patterns

### Recommended Project Structure

```
BuildingBlocks/Common/
  Results/
    (No new files needed — FluentResults ships Result and Result<T> directly)

src/MicroCommerce.ApiService/
  Common/
    Extensions/
      ResultExtensions.cs          # IResult HTTP mapping (ToHttpResult, ToProblemDetails)
    Behaviors/
      ValidationBehavior.cs        # EXISTING - throws ValidationException (unchanged)
      ResultValidationBehavior.cs  # NEW - returns Result.Fail for Result-typed handlers
  Features/
    Inventory/Application/Commands/ReserveStock/
      ReserveStockCommandHandler.cs  # PILOT: returns Result (insufficient stock)
    Ordering/Application/Commands/UpdateOrderStatus/
      UpdateOrderStatusCommandHandler.cs  # PILOT: returns Result (invalid transitions)

docs/decisions/
  adr-006-result-exception-boundary.md  # ADR: when Result vs exception
```

### Pattern 1: FluentResults Basic API

**What:** Create and inspect Result objects.
**When to use:** All Result-returning command handlers.

```csharp
// Source: https://github.com/altmann/fluentresults/blob/master/README.md

// Non-generic Result (command with no return value)
Result successResult = Result.Ok();
Result failResult = Result.Fail("Insufficient stock.");
Result failWithMultiple = Result.Fail("Reason 1").WithError("Reason 2");

// Generic Result<T> (command returning a value on success)
Result<Guid> successWithValue = Result.Ok(Guid.NewGuid());
Result<Guid> failedResult = Result.Fail<Guid>("Not found");

// Inspection
if (result.IsFailed)
{
    IEnumerable<IError> errors = result.Errors;
}

// Accumulate errors before returning
var result = Result.Ok();
if (stockItem.AvailableQuantity < request.Quantity)
    result = result.WithError($"Insufficient stock. Available: {stockItem.AvailableQuantity}, requested: {request.Quantity}");
if (stockItem.IsDiscontinued)
    result = result.WithError("Product is discontinued and cannot be reserved.");
if (result.IsFailed)
    return result;
```

### Pattern 2: Custom Error Types

**What:** Domain-specific typed errors for structured error information.
**When to use:** When the caller needs to distinguish error categories (e.g., not-found vs. business-rule failure).

```csharp
// In BuildingBlocks.Common or per-feature Errors class
// Source: https://github.com/altmann/fluentresults/blob/master/README.md

public sealed class InsufficientStockError : Error
{
    public InsufficientStockError(int available, int requested)
        : base($"Insufficient stock. Available: {available}, Requested: {requested}")
    {
        Metadata["Available"] = available;
        Metadata["Requested"] = requested;
    }
}

// Usage
return Result.Fail(new InsufficientStockError(stockItem.AvailableQuantity, request.Quantity));
```

> For the pilot scope (two handlers), simple `Error` strings with `WithError` chaining are sufficient. Custom typed errors can be introduced in ADOPT-05 (full adoption phase).

### Pattern 3: ResultValidationBehavior — MediatR Pipeline Constraint

**What:** A MediatR pipeline behavior that converts FluentValidation failures into `Result.Fail` instead of throwing `ValidationException`. It coexists with the existing `ValidationBehavior` by using a different generic constraint.

**Critical constraint mechanism:**
- Existing `ValidationBehavior<TRequest, TResponse>` has `where TRequest : notnull` — applies to ALL handlers
- New `ResultValidationBehavior<TRequest, TResponse>` constrains `where TResponse : IResultBase` — only applies when TResponse is `Result` or `Result<T>`
- Both are registered in the MediatR pipeline; MediatR resolves which behaviors apply at runtime based on constraints
- Registration ORDER matters: `ValidationBehavior` first, `ResultValidationBehavior` second (innermost)

```csharp
// Source: Pattern derived from MediatR IPipelineBehavior docs +
//         https://github.com/altmann/FluentResults/issues/54

using FluentResults;
using FluentValidation;
using MediatR;

namespace MicroCommerce.ApiService.Common.Behaviors;

// Only activates for handlers returning Result or Result<T>
public sealed class ResultValidationBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
    where TResponse : IResultBase
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ResultValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!_validators.Any())
            return await next();

        ValidationContext<TRequest> context = new(request);

        IEnumerable<FluentValidation.Results.ValidationFailure> failures =
            (await Task.WhenAll(_validators.Select(v => v.ValidateAsync(context, cancellationToken))))
            .SelectMany(r => r.Errors)
            .Where(f => f != null);

        List<FluentValidation.Results.ValidationFailure> failureList = failures.ToList();

        if (failureList.Count == 0)
            return await next();

        // Build failed Result with all validation errors as reasons
        List<IError> errors = failureList
            .Select(f => new Error($"{f.PropertyName}: {f.ErrorMessage}"))
            .ToList<IError>();

        // Result.Fail(errors) creates a failed IResultBase-compatible result
        // We need to handle both Result and Result<T>
        Result failed = Result.Fail(errors);

        // Convert to TResponse (works because TResponse : IResultBase)
        return (TResponse)(object)failed;
    }
}
```

> **Note:** The `(TResponse)(object)failed` cast is the only practical mechanism without reflection. It works correctly when TResponse is `Result` (non-generic). For `Result<T>`, a separate behavior or `Result.Fail<T>` created via reflection is needed. The pilot scope only uses non-generic `Result`, so this cast is safe. See Open Questions section.

### Pattern 4: HTTP Extension Methods for Minimal APIs

**What:** Extension methods that convert `Result` to Minimal API `IResult` HTTP responses.
**When to use:** In every endpoint handler that calls a Result-returning command.

```csharp
// File: src/MicroCommerce.ApiService/Common/Extensions/ResultExtensions.cs

using FluentResults;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MicroCommerce.ApiService.Common.Extensions;

public static class ResultExtensions
{
    /// <summary>
    /// Maps a non-generic Result to an HTTP IResult.
    /// Success: 204 No Content (or caller-provided success result).
    /// Failure: 422 Unprocessable Entity with ProblemDetails.
    /// </summary>
    public static IResult ToHttpResult(this Result result, Func<IResult>? onSuccess = null)
    {
        if (result.IsSuccess)
            return onSuccess?.Invoke() ?? Results.NoContent();

        return result.ToFailureProblem();
    }

    /// <summary>
    /// Maps a Result<T> to an HTTP IResult.
    /// Success: caller-provided success result using the value.
    /// Failure: 422 Unprocessable Entity with ProblemDetails.
    /// </summary>
    public static IResult ToHttpResult<T>(this Result<T> result, Func<T, IResult> onSuccess)
    {
        if (result.IsSuccess)
            return onSuccess(result.Value);

        return result.ToFailureProblem();
    }

    private static IResult ToFailureProblem(this IResultBase result)
    {
        string detail = string.Join("; ", result.Errors.Select(e => e.Message));

        ProblemDetails problemDetails = new()
        {
            Status = StatusCodes.Status422UnprocessableEntity,
            Title = "Business Rule Violation",
            Detail = detail
        };

        return Results.Problem(problemDetails);
    }
}
```

**Usage in endpoint:**

```csharp
private static async Task<IResult> UpdateOrderStatus(
    Guid id,
    UpdateOrderStatusRequest request,
    ISender sender,
    CancellationToken cancellationToken)
{
    UpdateOrderStatusCommand command = new(id, request.NewStatus);
    Result result = await sender.Send(command, cancellationToken);
    return result.ToHttpResult(); // 204 on success, 422 on business rule violation
}
```

### Pattern 5: Pilot Handler — Result-Returning Command

**What:** Convert a handler from throw-based to Result-based for business rule violations only.

```csharp
// File: ReserveStockCommandHandler.cs (AFTER migration)
// Demonstrates multiple error aggregation

public sealed class ReserveStockCommandHandler
    : IRequestHandler<ReserveStockCommand, Result>
{
    private readonly InventoryDbContext _context;

    public ReserveStockCommandHandler(InventoryDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(
        ReserveStockCommand request,
        CancellationToken cancellationToken)
    {
        var stockItem = await _context.StockItems
            .Include(s => s.Reservations.Where(r => r.ExpiresAt > DateTimeOffset.UtcNow))
            .FirstOrDefaultAsync(s => s.ProductId == request.ProductId, cancellationToken);

        // Infrastructure/not-found case: still throws (NotFoundException caught by GlobalExceptionHandler)
        if (stockItem is null)
            throw new NotFoundException($"Stock item for product {request.ProductId} not found.");

        // Business rule violations: now return Result failures
        Result validation = Result.Ok();

        if (request.Quantity <= 0)
            validation = validation.WithError("Reservation quantity must be positive.");

        if (stockItem.AvailableQuantity < request.Quantity)
            validation = validation.WithError(
                $"Insufficient stock. Available: {stockItem.AvailableQuantity}, Requested: {request.Quantity}");

        if (validation.IsFailed)
            return validation;

        ReservationId reservationId = stockItem.Reserve(request.Quantity);

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new ConflictException("Stock was modified concurrently. Please retry.");
        }

        return Result.Ok();
    }
}
```

> **Note:** The pilot command signature changes from `IRequest<Guid>` to `IRequest<Result>`. `ReserveStockCommand` currently returns `Guid` (reservation ID). For the pilot, either return `Result<Guid>` to preserve the value or change callers to not need the return value. Given that `ReserveStockCommandHandler` is called from a MassTransit consumer (not an HTTP endpoint directly), it may be simpler to pilot `UpdateOrderStatusCommandHandler` instead since it already returns `Task` (non-generic).

### Anti-Patterns to Avoid

- **Using Result for NotFoundException:** `NotFoundException` is an infrastructure/unexpected failure (resource should exist), not a business rule — keep it as exception caught by `GlobalExceptionHandler`.
- **Using Result for validation input errors:** `ValidationBehavior` already handles FluentValidation failures as `ValidationException` (400 Bad Request). Don't use `ResultValidationBehavior` as a replacement — it is additive.
- **Returning Result from domain methods in the pilot:** The phase says "pilot adoption in command handlers" — domain aggregate methods (`Order.MarkAsPaid`, `StockItem.Reserve`) keep throwing for now. The handler wraps the throw in a try-catch or pre-validates before calling the domain method.
- **Logging in ResultValidationBehavior:** Unlike the exception path (logged in `GlobalExceptionHandler`), Result failures are explicit flow — do not log them as errors in the behavior.
- **Registering ResultValidationBehavior before ValidationBehavior:** The existing `ValidationBehavior` must run first for non-Result handlers. For Result handlers, both run (validation first throws, so `ResultValidationBehavior` never reaches its logic). This means `ResultValidationBehavior` effectively only adds value for Result handlers that also have validators — which is fine for the pilot.

---

## Don't Hand-Roll

| Problem | Don't Build | Use Instead | Why |
|---------|-------------|-------------|-----|
| Result type with errors list | `class Result<T> { bool IsSuccess; List<Error> Errors; }` | FluentResults 4.0.0 | Chaining (`WithError`), `Merge`, `IResultBase` for MediatR constraints, `Result.Try`, custom error types — all production-tested |
| HTTP Problem mapping | Custom middleware | `ResultExtensions.ToHttpResult()` extension method | Thin extension is sufficient; FluentResults.Extensions.AspNetCore NuGet is MVC-based (controllers), not Minimal API |
| Validation-to-Result bridge | A full custom behavior framework | `ResultValidationBehavior<TRequest, TResponse>` with `where TResponse : IResultBase` | The constraint mechanism is all that's needed |

**Key insight:** The `IResultBase` interface from FluentResults is what makes the MediatR generic constraint pattern work. Without it (custom Result class), you'd need reflection (`Activator.CreateInstance`) or string-based type detection to build a correct `TResponse` at runtime.

---

## Common Pitfalls

### Pitfall 1: ResultValidationBehavior Applies to ALL Handlers (Duplicate Validation)

**What goes wrong:** Both `ValidationBehavior` and `ResultValidationBehavior` run for Result-returning handlers, causing validation to happen twice.

**Why it happens:** MediatR applies all registered behaviors that match the TRequest/TResponse constraints. If a handler returns `Result` AND has a `IValidator<TRequest>`, both behaviors activate.

**How to avoid:** The `ValidationBehavior` for non-Result handlers throws `ValidationException` which is caught by `GlobalExceptionHandler`. For Result handlers, `ResultValidationBehavior` should be registered as the ONLY validation behavior that acts on them, OR accept the double validation (it is harmless since Result-returning handlers will never reach their inner logic if `ValidationBehavior` throws first).

**Recommended resolution:** Register `ValidationBehavior` to skip when `TResponse : IResultBase` by adding a type check at the start, OR accept double-validation (ValidationBehavior always runs first and throws, so ResultValidationBehavior is never needed for Result handlers that have validators — the pilot handlers may not need validators at all). For pilot scope: accept the behavior as-is and document in the ADR.

**Warning signs:** Both behaviors appear in logs for the same request.

### Pitfall 2: Casting `Result` to `Result<T>` Fails at Runtime

**What goes wrong:** `(TResponse)(object)Result.Fail(errors)` throws `InvalidCastException` when TResponse is `Result<SomeType>` (generic).

**Why it happens:** `Result.Fail(errors)` returns non-generic `Result`. `Result<T>` is a different type despite both implementing `IResultBase`.

**How to avoid:** In the pilot, only use non-generic `Result` as the command return type. Document this as a known limitation in the ADR. Full fix for ADOPT-05 requires either `Result.Fail<T>(errors)` via reflection, or separate `ResultValidationBehavior` variants.

**Warning signs:** Handlers returning `Result<Guid>` or `Result<SomeDto>` throw cast exceptions when validation fails.

### Pitfall 3: Endpoint Does Not Handle Result Return Type

**What goes wrong:** After changing a command handler to return `Result`, the endpoint still calls `await sender.Send(command)` and ignores the return value (or casts it to `Unit`), so failures are silently swallowed.

**Why it happens:** The endpoint was written for `Task` or `Task<Guid>` return. When the command now returns `Result`, the endpoint must change its signature and call `ToHttpResult()`.

**How to avoid:** For every pilot handler migration, the corresponding endpoint handler MUST be updated in the same plan wave.

**Warning signs:** Commands return failures but the API always returns 204/200.

### Pitfall 4: FluentResults 4.0.0 Breaking Change — Empty Error List

**What goes wrong:** `Result.Fail(new List<IError>())` throws an exception in v4.0.0.

**Why it happens:** v4 added stricter validation: calling Fail with an empty collection is a bug, not a success.

**How to avoid:** Always verify the errors list is non-empty before calling `Result.Fail(errorList)`. Use `if (errors.Count > 0) return Result.Fail(errors); return Result.Ok();` pattern.

**Warning signs:** Tests fail with exception on `Result.Fail(new List<IError>())`.

### Pitfall 5: Mixing HTTP Status Codes Between Result and Exception Paths

**What goes wrong:** `GlobalExceptionHandler` maps `InvalidOperationException` to 400, but `ToHttpResult()` maps Result failures to 422. The same business rule violation can return different status codes depending on whether it was migrated.

**Why it happens:** The two paths (exception vs. Result) have different HTTP mapping.

**How to avoid:** The ADR should document the boundary clearly: business rule violations that are expected to be returned to clients should use 422 via Result. The pilot should pick handlers that previously returned 400 (InvalidOperationException) and now will return 422.

**Warning signs:** API clients receiving different status codes for logically equivalent scenarios.

---

## Code Examples

Verified patterns from official sources and codebase analysis:

### FluentResults Basic Creation and Checking

```csharp
// Source: https://github.com/altmann/fluentresults/blob/master/README.md

Result successResult = Result.Ok();
Result errorResult = Result.Fail("My error message");
Result multipleErrors = Result.Fail("error 1").WithError("error 2").WithError("error 3");

Result<int> successWithValue = Result.Ok(42);
Result<int> failedWithType = Result.Fail<int>("Error");

if (result.IsFailed)
{
    IEnumerable<IError> errors = result.Errors;
    // errors[0].Message == "My error message"
}

var value = result.Value;          // throws if failed
var safeVal = result.ValueOrDefault; // returns default if failed
```

### Merging Multiple Results

```csharp
// Source: https://github.com/altmann/fluentresults/blob/master/README.md

Result result1 = Result.Ok();
Result result2 = Result.Fail("stock unavailable");
Result result3 = Result.Ok();

Result merged = Result.Merge(result1, result2, result3);
// merged.IsFailed == true, merged.Errors has "stock unavailable"
```

### ResultValidationBehavior Registration

```csharp
// In Program.cs — add AFTER ValidationBehavior registration

builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssemblyContaining<Program>();

    // Existing: throws ValidationException for ALL handlers
    cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));

    // New: returns Result.Fail for Result-typed handlers
    cfg.AddOpenBehavior(typeof(ResultValidationBehavior<,>));
});
```

### Pilot Command Change (ReserveStock)

```csharp
// BEFORE (command signature):
public sealed record ReserveStockCommand(...) : IRequest<Guid>;

// AFTER:
public sealed record ReserveStockCommand(...) : IRequest<Result>;
// Note: if the Guid reservation ID is needed by callers, use IRequest<Result<Guid>>
// but be aware of the generic cast pitfall above — pilot should use non-generic Result
```

### ADR Boundary Decision (Documentation Artifact)

```markdown
# ADR-006: Result vs Exception Boundary

## Decision
- Business rule violations that are expected by callers → Return `Result.Fail`
- Not-found infrastructure errors → Throw `NotFoundException` (caught by GlobalExceptionHandler → 404)
- Concurrency conflicts → Throw `ConflictException` (caught by GlobalExceptionHandler → 409)
- Input validation failures → Throw `ValidationException` via ValidationBehavior (caught → 400)
- Unexpected exceptions (bugs) → Let propagate to GlobalExceptionHandler → 500

## Rationale
Railway-oriented programming for expected domain failures, exceptions for unexpected/infrastructure failures.
```

---

## State of the Art

| Old Approach | Current Approach | When Changed | Impact |
|--------------|------------------|--------------|--------|
| Hand-rolled `Result<T>` classes | FluentResults with `IResultBase` | Ongoing — FluentResults v1 was 2019, v4 June 2025 | `IResultBase` enables MediatR generic constraints without reflection |
| `throw` for all business rule violations | Result pattern for expected domain failures | v4 ecosystem standard | Explicit error types, multiple error aggregation, no try-catch in callers |
| MVC `ActionResult` HTTP mapping | Minimal API `IResult` via extension methods | ASP.NET Core Minimal APIs (2021+) | No `ActionResultExtensions`; hand-write thin `ToHttpResult()` extension |
| Single `ValidationBehavior` for everything | Dual behavior: `ValidationBehavior` (exceptions) + `ResultValidationBehavior` (Result) | Phase 17 — pilot adoption | Gradual migration without breaking existing handlers |

**Deprecated/outdated:**
- `FluentResults.Extensions.AspNetCore`: NuGet package exists but targets MVC Controllers (returns `ActionResult`). Minimal API does not use `ActionResult`. Do NOT use this package — write custom `ToHttpResult()` extension instead.

---

## Open Questions

1. **Pilot Handler Selection: ReserveStock vs. UpdateOrderStatus**
   - What we know: `ReserveStockCommandHandler` currently returns `Guid` (reservation ID) — changing to `Result` loses the return value. The handler is called from a MassTransit consumer, not an HTTP endpoint.
   - What's unclear: Whether the MassTransit consumer uses the returned Guid. If yes, the handler should return `Result<Guid>` (which triggers the generic cast pitfall).
   - Recommendation: Use `UpdateOrderStatusCommandHandler` (already returns `Task`, maps to `Result` cleanly) and `AdjustStockCommandHandler` (returns `Unit`, maps to `Result` cleanly with multiple validation reasons: negative quantity, quantity exceeding on-hand). Both avoid the generic cast problem.

2. **ResultValidationBehavior Generic Cast for `Result<T>`**
   - What we know: `(TResponse)(object)Result.Fail(errors)` fails for generic `Result<T>`.
   - What's unclear: Whether the pilot should include `Result<T>` handlers.
   - Recommendation: Pilot uses only non-generic `Result` handlers. Document the `Result<T>` limitation in the ADR. Full fix deferred to ADOPT-05.

3. **Duplicate Validation for Result Handlers**
   - What we know: If a handler returns `Result` AND has a `IValidator<TRequest>` registered, both `ValidationBehavior` (throws) and `ResultValidationBehavior` (returns Result) run, but `ValidationBehavior` always wins by throwing first.
   - What's unclear: Should `ValidationBehavior` be modified to skip Result handlers to allow `ResultValidationBehavior` to handle validation gracefully?
   - Recommendation: For the pilot, pilot handlers should NOT have validators (or remove the validator if one exists). Keep both behaviors simple for now. Document in ADR that input validation always uses exceptions (400), never Result.

4. **Where Does FluentResults Live: BuildingBlocks vs. ApiService?**
   - What we know: PRIM-05 says "integrated into BuildingBlocks.Common". HTTP extension methods must stay in ApiService (ASP.NET Core dependency). The `Result` type itself is from FluentResults, not hand-rolled.
   - Recommendation: Add `FluentResults` PackageReference to `BuildingBlocks.Common.csproj`. The Result types are then available project-wide via the existing project reference chain. HTTP extension methods go in `src/MicroCommerce.ApiService/Common/Extensions/ResultExtensions.cs`.

---

## Validation Architecture

> `workflow.nyquist_validation` is not set in `.planning/config.json` (only `research`, `plan_check`, `verifier` are listed). Skipping this section.

---

## Sources

### Primary (HIGH confidence)

- `/altmann/fluentresults` (Context7) — FluentResults API: `Result.Ok`, `Result.Fail`, `Result<T>`, `IResultBase`, `WithError`, `Merge`, custom Error types, `Result.Try`
- [https://www.nuget.org/packages/FluentResults/](https://www.nuget.org/packages/FluentResults/) — Confirmed v4.0.0 latest stable, released June 29 2025; .NET 8/9 targets; breaking changes documented
- Codebase analysis — `ValidationBehavior.cs`, `GlobalExceptionHandler.cs`, `Program.cs`, `Order.cs`, `StockItem.cs`, `ReserveStockCommandHandler.cs`, `UpdateOrderStatusCommandHandler.cs`, `AdjustStockCommandHandler.cs`, `BuildingBlocks.Common.csproj`, `MicroCommerce.ApiService.csproj` — all read directly

### Secondary (MEDIUM confidence)

- [https://github.com/altmann/FluentResults/issues/54](https://github.com/altmann/FluentResults/issues/54) — MediatR+FluentResults pipeline pattern; `IResultBase` constraint; `(TResponse)(object)` cast mechanism
- [WebSearch] MediatR `IPipelineBehavior` coexistence pattern — `where TResponse : IResultBase` constraint confirmed as the standard approach in community implementations
- [https://github.com/altmann/FluentResults/releases](https://github.com/altmann/FluentResults/releases) — v4.0.0 breaking changes: deconstruct operators, empty Fail list now throws, ReadOnlyList type

### Tertiary (LOW confidence)

- [WebSearch] FluentResults.Extensions.AspNetCore for MVC Controllers — confirmed NOT suitable for Minimal APIs (LOW confidence that it works with Minimal API IResult); using custom extension method instead

---

## Metadata

**Confidence breakdown:**
- Standard stack: HIGH — FluentResults 4.0.0 confirmed on NuGet, Context7 API verified
- Architecture: HIGH — MediatR constraint pattern verified via issue #54, codebase structure fully read
- Pitfalls: HIGH — Empty-list breaking change confirmed in v4 release notes; generic cast problem is a known constraint of C# generics; others derived from codebase analysis
- Pilot handler selection: MEDIUM — Dependent on whether ReserveStock Guid return value is used by MassTransit consumer (recommend AdjustStock + UpdateOrderStatus to avoid ambiguity)

**Research date:** 2026-02-24
**Valid until:** 2026-05-24 (FluentResults 4.0.x is stable; MediatR 13.x is stable)
