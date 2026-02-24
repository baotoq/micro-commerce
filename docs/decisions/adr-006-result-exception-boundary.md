# ADR-006: Result vs Exception Boundary

**Status:** Accepted
**Date:** 2026-02-24

## Context

The codebase has historically used throw-based error handling for all failure scenarios. Every error path — whether input validation failures, not-found lookups, or business rule violations — propagates as an exception and is mapped to HTTP responses by `GlobalExceptionHandler`.

Phase 17 introduces FluentResults to support railway-oriented programming for business rule violations. This creates two co-existing error handling mechanisms in the same codebase. Without a clear boundary, developers will make inconsistent choices about when to use `Result` vs when to throw.

This ADR establishes the canonical decision rule for choosing between `Result` and exceptions.

## Decision

Errors are categorized by their expected handling context:

| Category | Mechanism | HTTP Status | Handler |
|---|---|---|---|
| Business rule violations (expected, recoverable) | `Result.Fail` | 422 Unprocessable Entity | `ToHttpResult()` extension |
| Input validation failures (malformed request) | Throw `ValidationException` via `ValidationBehavior` | 400 Bad Request | `GlobalExceptionHandler` |
| Resource not found (expected to exist) | Throw `NotFoundException` | 404 Not Found | `GlobalExceptionHandler` |
| Concurrency conflicts | Throw `ConflictException` or `DbUpdateConcurrencyException` | 409 Conflict | `GlobalExceptionHandler` |
| Unexpected exceptions (bugs, infrastructure) | Let propagate | 500 Internal Server Error | `GlobalExceptionHandler` |

### Rule Summary

**Use `Result.Fail` when:** The caller can reasonably handle the failure as part of normal business flow. Examples: duplicate email on registration, insufficient stock on reservation, coupon already redeemed, product already reviewed.

**Throw an exception when:** The failure represents an abnormal state that the application cannot meaningfully recover from inline — infrastructure errors, missing resources that should exist, protocol violations.

## Consequences

### Gradual Migration

Existing handlers remain unchanged. The `ResultValidationBehavior` only activates for handlers whose return type implements `IResultBase` (constrained via `where TResponse : IResultBase`). All existing `void`, `Unit`, or non-Result handlers continue to use `ValidationBehavior` → `GlobalExceptionHandler` as before. There is no behavioral change for existing handlers.

### Behavior Registration Order

In `Program.cs`, `ValidationBehavior<,>` is registered first, followed by `ResultValidationBehavior<,>`. Both behaviors examine the same validators for a given request, but:
- `ValidationBehavior` throws `ValidationException` for non-Result handlers
- `ResultValidationBehavior` returns `Result.Fail` for Result-typed handlers
- A handler returning `Result` will only hit `ResultValidationBehavior` because the `IResultBase` constraint prevents `ValidationBehavior` from matching... actually both behaviors run independently. The first to match short-circuits via its own logic; handlers returning `Result` will receive `Result.Fail` from `ResultValidationBehavior` if validation fails, as the cast `(TResponse)(object)Result.Fail(errors)` succeeds for non-generic `Result`.

### Limitations

The `ResultValidationBehavior` short-circuit path uses the cast `(TResponse)(object)Result.Fail(errors)`. This cast works for non-generic `Result` but **not** for `Result<T>` (a `Result` cannot be cast to `Result<string>`, for example). Handlers returning `Result<T>` that have validators will have their validation errors returned as exceptions via `ValidationBehavior` falling through, which may not be desired.

`Result<T>` support in `ResultValidationBehavior` requires reflection or a factory method to construct the correct generic type, and is deferred to ADOPT-05. Pilot handlers using `Result<T>` should not rely on `ResultValidationBehavior` for validation short-circuiting.

### HTTP Mapping

`ResultExtensions.ToHttpResult()` maps Result failures to HTTP 422 Unprocessable Entity with a `ProblemDetails` body:

```json
{
  "status": 422,
  "title": "Business Rule Violation",
  "detail": "error message 1; error message 2"
}
```

This is distinct from HTTP 400 (input validation, `ValidationBehavior`) and is intentional: 422 signals that the request was syntactically valid but semantically rejected by business rules.
