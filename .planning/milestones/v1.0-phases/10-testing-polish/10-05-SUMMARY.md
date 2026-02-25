---
phase: 10-testing-polish
plan: 05
subsystem: testing
tags: [playwright, e2e, frontend-testing, nextjs]

requires:
  - phase: 03
    plan: 03
    reason: "Product grid with infinite scroll"
  - phase: 06
    plan: 03
    reason: "Cart React Query hooks"

provides:
  - "Playwright E2E test framework configured for Next.js"
  - "7 E2E tests covering product browsing and cart flow"
  - "Resilient test selectors with accessibility-first patterns"

affects:
  - phase: 10
    plan: 06
    reason: "Additional E2E tests can follow same patterns"

tech-stack:
  added:
    - "@playwright/test@1.58.2"
  patterns:
    - "Flexible selector strategy with .or() fallbacks"
    - "Accessibility-first selectors (getByRole, getByPlaceholder)"
    - "No webServer in config (assumes Aspire stack running)"

key-files:
  created:
    - "src/MicroCommerce.Web/playwright.config.ts"
    - "src/MicroCommerce.Web/e2e/critical-path.spec.ts"
    - "src/MicroCommerce.Web/e2e/product-browsing.spec.ts"
  modified:
    - "src/MicroCommerce.Web/package.json"
    - "src/MicroCommerce.Web/.gitignore"

decisions:
  - id: "chromium-only-local"
    choice: "Install only chromium for local dev"
    rationale: "Faster install, CI can install all browsers if needed"
  - id: "no-webserver-config"
    choice: "No webServer in playwright.config.ts"
    rationale: "MicroCommerce requires full Aspire stack (API, DB, Keycloak, Service Bus) which can't be started by Playwright alone"
  - id: "flexible-selectors"
    choice: "Use .or() chaining and multiple fallback selectors"
    rationale: "Tests resilient to minor UI changes, won't break on class name changes"
  - id: "baseurl-env-var"
    choice: "baseURL configurable via BASE_URL environment variable"
    rationale: "Supports local dev and CI environments with different URLs"

metrics:
  duration: "3 minutes"
  completed: "2026-02-12"
---

# Phase 10 Plan 05: Playwright E2E Tests Summary

**One-liner:** Playwright configured with 7 E2E tests covering product browsing, detail pages, search, filtering, and cart flow using accessibility-first selectors.

## What Was Built

Set up Playwright test framework for the Next.js frontend and created comprehensive E2E tests for the critical user paths: product browsing and cart operations.

### Test Coverage

**critical-path.spec.ts (2 tests):**
- Browse products → view detail → add to cart → verify cart page
- Empty cart displays appropriate message

**product-browsing.spec.ts (5 tests):**
- Homepage displays product grid
- Product detail page shows product info
- Search filters products by name
- Category filter narrows product list
- Infinite scroll loads more products

### Key Features

1. **Resilient Test Selectors**
   - Flexible selectors: `page.locator('[data-testid="product-card"], article, .product-card')`
   - Accessibility-first: `getByRole('button', { name: /add to cart/i })`
   - Fallback chains: `getByPlaceholder(/search/i).or(getByRole('searchbox'))`

2. **Environment Configuration**
   - baseURL from env var or defaults to `http://localhost:3000`
   - CI-aware settings: retries, workers, reporters
   - Trace on first retry, screenshot only on failure

3. **Aspire Stack Assumption**
   - No webServer configuration
   - Tests require full stack: `dotnet run --project src/MicroCommerce.AppHost`
   - Clear documentation in test file comments

## Decisions Made

1. **Chromium-only for local development**
   - Faster installation (162MB vs 500MB+ for all browsers)
   - CI can install firefox/webkit if needed
   - Desktop Chrome device emulation

2. **No webServer in Playwright config**
   - MicroCommerce requires full Aspire orchestration
   - Backend API, PostgreSQL, Keycloak, Service Bus all needed
   - Starting only frontend with `npm run dev` would cause API failures
   - Tests document prerequisite: run Aspire stack first

3. **Flexible selector strategy**
   - Multiple selector fallbacks prevent brittle tests
   - Prefer semantic selectors over CSS classes
   - Tests survive UI refactoring (class name changes, DOM restructure)

4. **Configurable baseURL**
   - `BASE_URL` environment variable for different environments
   - Default to localhost:3000 for local dev
   - CI can set to deployed preview URL

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 3 - Blocking] Missing test files from 10-01**
- **Found during:** Task 1 git status check
- **Issue:** Plan 10-01 created test files but didn't commit them all
- **Fix:** Added and committed integration/unit test fixtures before proceeding
- **Files:** `Program.cs`, `ApiWebApplicationFactory.cs`, test files in Unit/Integration folders
- **Commit:** 991e9650

No other deviations - plan executed exactly as written.

## Testing Verification

Tests can be verified without running the full stack:

```bash
cd src/MicroCommerce.Web
npx playwright test --list
# Output: Total: 7 tests in 2 files
```

To run tests (requires Aspire stack):

```bash
# Terminal 1: Start Aspire stack
dotnet run --project src/MicroCommerce.AppHost

# Terminal 2: Run E2E tests
cd src/MicroCommerce.Web
npm run test:e2e           # Headless run
npm run test:e2e:ui        # Interactive UI mode
npm run test:e2e:report    # View last run report
```

## Next Phase Readiness

**Ready for:**
- Additional E2E test scenarios (checkout, admin flows)
- CI/CD pipeline integration
- Visual regression testing with Playwright screenshots

**No blockers or concerns.**

## Technical Notes

1. **Test Execution Context**
   - Tests assume seeded database with products/categories
   - Guest cart uses cookies (context isolation per test)
   - Add to cart may fail if product out of stock

2. **Selector Strategy**
   - First try data-testid attributes (none exist yet, future-proof)
   - Fall back to semantic HTML (article, a[href], etc.)
   - Use accessible queries (getByRole, getByPlaceholder)
   - Avoid fragile selectors (.className.with.many.parts)

3. **Future Enhancements**
   - Add data-testid attributes to key UI elements for more reliable selectors
   - Page Object Model pattern for reusable page interactions
   - API route mocking for isolated frontend testing
   - Visual regression testing with Percy or Playwright snapshots

## Commits

| Hash | Message | Files |
|------|---------|-------|
| 21cc2f6c | chore(10-05): install Playwright and configure for Next.js | playwright.config.ts, package.json, .gitignore |
| c17aec96 | test(10-05): add E2E tests for product browsing and cart flow | critical-path.spec.ts, product-browsing.spec.ts |

**Total:** 2 commits for this plan (plus 1 auto-fix commit for 10-01)
