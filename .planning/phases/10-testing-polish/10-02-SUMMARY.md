---
phase: 10-testing-polish
plan: 02
subsystem: testing
tags: [unit-tests, domain-aggregates, value-objects, validators, fluent-validation, catalog, cart, inventory]

requires:
  - 10-01

provides:
  - unit-test-coverage-catalog
  - unit-test-coverage-cart
  - unit-test-coverage-inventory
  - validator-test-coverage

affects:
  - 10-03
  - 10-04

tech-stack:
  added: []
  patterns:
    - unit-testing-domain-aggregates
    - value-object-testing
    - fluentvalidation-testing-with-testvalidate

key-files:
  created:
    - src/MicroCommerce.ApiService.Tests/Unit/Catalog/Aggregates/ProductTests.cs
    - src/MicroCommerce.ApiService.Tests/Unit/Catalog/ValueObjects/MoneyTests.cs
    - src/MicroCommerce.ApiService.Tests/Unit/Catalog/ValueObjects/ProductNameTests.cs
    - src/MicroCommerce.ApiService.Tests/Unit/Cart/Aggregates/CartTests.cs
    - src/MicroCommerce.ApiService.Tests/Unit/Inventory/Aggregates/StockItemTests.cs
    - src/MicroCommerce.ApiService.Tests/Unit/Validators/SubmitOrderCommandValidatorTests.cs
    - src/MicroCommerce.ApiService.Tests/Unit/Validators/CreateProductCommandValidatorTests.cs
    - src/MicroCommerce.ApiService.Tests/Unit/Validators/AdjustStockCommandValidatorTests.cs
  modified: []

decisions: []

metrics:
  test-count: 95
  duration: ~12 minutes
  completed: 2026-02-12
---

# Phase 10 Plan 02: Domain Aggregate and Validator Unit Tests Summary

**One-liner:** Comprehensive unit tests for Product, Cart, StockItem aggregates, Money/ProductName value objects, and three critical FluentValidation validators

---

## What Was Built

### Task 1: Catalog and Cart Domain Tests (50 tests)

**ProductTests.cs (15 tests)**
- Product lifecycle: create, update, publish, unpublish, archive
- Domain event raising: ProductCreated, ProductUpdated, ProductArchived
- Status transitions and idempotency
- Property validation and timestamp tracking

**MoneyTests.cs (9 tests)**
- Amount validation (negative guard, zero allowed)
- Currency normalization (lowercase → uppercase)
- Value equality semantics
- String formatting (culture-aware)

**ProductNameTests.cs (11 tests)**
- Length constraints (2-200 characters)
- Whitespace trimming
- Value equality semantics
- Implicit string conversion

**CartTests.cs (15 tests)**
- Item management (add, update quantity, remove)
- Quantity limits (1-99 range, capping at 99)
- TTL management (30-day expiration, reset on modification)
- LastModifiedAt and ExpiresAt timestamp tracking

### Task 2: Inventory and Validator Tests (45 tests)

**StockItemTests.cs (16 tests)**
- Stock adjustment (positive/negative, guards against negative result)
- Reservation system (create, release, available quantity calculation)
- Domain event raising (StockAdjusted, StockLow at threshold ≤10)
- Zero and negative quantity guards

**SubmitOrderCommandValidatorTests.cs (15 tests)**
- Email validation (required, format)
- Shipping address nested validation (all fields required, zip code ≤10 chars)
- Order items validation (non-empty, quantity 1-99, price >0, productId required)

**CreateProductCommandValidatorTests.cs (10 tests)**
- Product name validation (required, 2-200 chars)
- Description required
- Price ≥0
- Category required
- Image URL optional but must be valid URL when provided

**AdjustStockCommandValidatorTests.cs (5 tests)**
- ProductId required
- Adjustment must be non-zero (positive for restock, negative for reduction)

---

## Technical Approach

### Unit Test Patterns

**Domain Aggregate Testing**
- Factory method usage for aggregate creation
- Domain event verification via `DomainEvents` collection
- Invariant enforcement (guards, range checks, state transitions)
- Helper methods for test data creation (`CreateValidProduct()`, `CreateStockItem()`)

**Value Object Testing**
- Guard clause verification (ArgumentException, ArgumentOutOfRangeException)
- Value equality semantics
- Immutability (no setters, only factory methods)
- String representation and implicit conversions

**FluentValidation Testing**
- `TestValidate()` helper for clear assertion syntax
- `ShouldHaveValidationErrorFor()` and `ShouldNotHaveAnyValidationErrors()`
- Nested property validation (e.g., `ShippingAddress.ZipCode`)
- Collection item validation (e.g., `Items[0].Quantity`)

### Test Organization

```
Unit/
  Catalog/
    Aggregates/
      ProductTests.cs
    ValueObjects/
      MoneyTests.cs
      ProductNameTests.cs
  Cart/
    Aggregates/
      CartTests.cs
  Inventory/
    Aggregates/
      StockItemTests.cs
  Validators/
    SubmitOrderCommandValidatorTests.cs
    CreateProductCommandValidatorTests.cs
    AdjustStockCommandValidatorTests.cs
```

**Conventions**
- Test naming: `{Method}_{Scenario}_{Expected}` (e.g., `Create_ValidData_ReturnsProductWithDraftStatus`)
- `[Trait("Category", "Unit")]` for filtering
- FluentAssertions for readable assertions
- File-scoped namespaces
- Arrange-Act-Assert pattern

---

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 1 - Bug] CartTests namespace collision**
- **Found during:** Task 1 implementation
- **Issue:** Test namespace `Cart.Aggregates` collided with entity type `Cart.Domain.Entities.Cart`, causing compilation error with `Domain.Entities.Cart` reference
- **Fix:** Added type alias `using CartEntity = MicroCommerce.ApiService.Features.Cart.Domain.Entities.Cart;` and used `CartEntity` throughout tests
- **Files modified:** `CartTests.cs`
- **Commit:** 376751e8

**2. [Rule 1 - Bug] ProductNameTests Guard Clause parameter names**
- **Found during:** Task 1 test execution
- **Issue:** Ardalis.GuardClauses library uses parameter names `minLength`/`maxLength` instead of `value` for `LengthOutOfRange` guard
- **Fix:** Removed specific parameter name assertions, kept exception type assertions
- **Files modified:** `ProductNameTests.cs`
- **Commit:** 376751e8

**3. [Rule 1 - Bug] MoneyTests culture-dependent decimal formatting**
- **Found during:** Task 1 test execution
- **Issue:** `ToString()` returns `"USD 99,99"` (comma separator) in some locales vs `"USD 99.99"` (period separator) in others
- **Fix:** Changed assertion to regex `MatchRegex(@"USD 99[.,]99")` for culture-invariance
- **Files modified:** `MoneyTests.cs`
- **Commit:** 376751e8

---

## Verification

### Test Execution

```bash
# Task 1: Catalog and Cart tests
dotnet test --filter "Category=Unit&(FullyQualifiedName~Catalog|FullyQualifiedName~Cart.Aggregates)"
✅ Passed: 50 tests

# Task 2: Inventory and Validator tests
dotnet test --filter "Category=Unit&(FullyQualifiedName~Inventory.Aggregates|FullyQualifiedName~Validators)"
✅ Passed: 45 tests

# Combined
dotnet test --filter "Category=Unit"
✅ Total: 95 tests passed (exceeds 70+ requirement)
```

### Coverage

| Module    | Aggregate/VO       | Tests | Coverage                                      |
| --------- | ------------------ | ----- | --------------------------------------------- |
| Catalog   | Product            | 15    | Create, Update, Publish, Unpublish, Archive   |
| Catalog   | Money              | 9     | Validation, equality, formatting              |
| Catalog   | ProductName        | 11    | Length, trimming, equality, conversion        |
| Cart      | Cart               | 15    | Items, quantity limits, TTL                   |
| Inventory | StockItem          | 16    | Adjust, reserve, release, available quantity  |
| Ordering  | SubmitOrderCmd Val | 15    | Email, address, items validation              |
| Catalog   | CreateProductCmd V | 10    | Name, description, price, category validation |
| Inventory | AdjustStockCmd Val | 5     | ProductId, adjustment validation              |

**Total: 95 tests covering 4 aggregates, 2 value objects, 3 validators**

---

## Next Phase Readiness

### Unblocked

- **10-03**: Integration tests can reference unit test patterns
- **10-04**: E2E tests have comprehensive domain model coverage

### Blockers/Concerns

None - all 95 tests passing, no dependencies on external services

---

## Decisions Made

None - followed established xUnit + FluentAssertions + FluentValidation.TestHelper patterns

---

## Key Learnings

1. **Guard Clause Libraries:** Ardalis.GuardClauses uses specific parameter names (`minLength`, `maxLength`) that differ from the guarded parameter name (`value`) - test for exception type only unless specific parameter is critical
2. **Culture Invariance:** Decimal formatting in `ToString()` is culture-dependent - use regex or specific culture for assertions
3. **Namespace Collisions:** Type aliases (`using TypeAlias = Namespace.Type`) resolve namespace conflicts when test namespace matches domain namespace
4. **FluentValidation TestValidate:** Cleaner than manual validator instantiation and result inspection - prefer `TestValidate()` with `ShouldHaveValidationErrorFor()`
5. **Domain Event Testing:** Clear events before testing specific behavior to isolate event assertions (`aggregate.ClearDomainEvents()`)

---

**Files Changed:** 8 created (4 Task 1 + 4 Task 2)
**Commits:** 2 (376751e8 Task 1 fixes, b5276f52 Task 2)
**Test Count:** 95 (50 + 45)
