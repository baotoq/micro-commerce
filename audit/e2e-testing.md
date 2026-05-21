# E2E / Manual QA audit — vs. Playwright + agent-browser skill specs

**Scope:** `src/web/e2e/**`, `src/web/playwright.config.ts`, `src/web/package.json`, `qa/**`.
**Rubric:** `e2e-testing-patterns`, `playwright-cli`, `playwright-explore-website`, `playwright-generate-test`, `playwright-automation-fill-in-form`, `chrome-devtools`, `agent-browser` SKILL.md.

## Summary

The recent fixture restructure (`fixtures/test.ts`, `fixtures/product.ts`, `fixtures/seller.ts`) is solidly aligned with `e2e-testing-patterns` — per-test factory with tracked teardown, unique-SKU keying on `parallelIndex+testId`, and shadcn-aware select helper. The tag taxonomy (`@smoke` / `@regression` / `@seed-dependent` / area tags like `@listings`, `@orders`) is consistent and runnable from the `e2e:smoke` / `e2e:seed-dependent` scripts. The main weak spots are over-reliance on raw `getByText(...).first()` in static-content specs (esp. `marketing`, `promos`, `orders/inbox`), an arbitrary `setTimeout(800)` race in the spinner spec, two leftover content-collision selectors (`page.locator("h1")`, `page.content()` substring matches), the create.spec doing manual cleanup outside the factory, and `trace: "on-first-retry"` skipping local-run traces entirely.

---

## Findings

### [High] Spinner spec races a `setTimeout(800)` against the assertion
- **Where**: `src/web/e2e/seller/listings/spinner.spec.ts:10-26`
- **Issue**: `page.route` handler does `await new Promise(r => setTimeout(r, 800))` then `route.continue()`. The test then asserts two spinners are `toBeVisible()`. If CI is slow the click→render gap can exceed the 800 ms hold, the response resolves before the assertion runs, and both spinners vanish — a textbook flake. The skill explicitly flags fixed timeouts as the #1 flake source.
- **Skill rule**: `e2e-testing-patterns` → "Waiting Strategies" / "Common Pitfalls: Flaky Tests — use proper waits, not fixed timeouts."
- **Fix**: Don't time the route — block it. Replace the `setTimeout` with a manual gate: `let resolve: () => void; const gate = new Promise<void>(r => (resolve = r));` then `await gate; await route.continue();`. Click → assert spinner visible → `resolve!()` → assert spinner hidden + page-2 rows. Deterministic on any hardware.

### [High] `getByText(...).first()` is used as a band-aid for non-unique strings
- **Where**: `src/web/e2e/seller/promos.spec.ts:33-95` (~20 occurrences), `marketing.spec.ts:70,78,109`, `analytics.spec.ts:30-34`, `payouts.spec.ts:34`, `dashboard.spec.ts:11`, `orders/[id]/detail.spec.ts:53-74`, `orders/[id]/pack.spec.ts:32,44`.
- **Issue**: When a copy string appears more than once (label + stat value, sidebar + KPI), `.first()` silently passes even if the intended element disappears, as long as *some* element with the text exists. The assertion no longer verifies what its name claims. The `analytics.spec.ts:27` comment ("use .last() so sidebar's 'Orders' doesn't match") spells out the smell.
- **Skill rule**: `e2e-testing-patterns` → "Use Page Objects / Meaningful Assertions / Test user-visible behavior, not internals" + "Avoid Brittle Selectors."
- **Fix**: Scope by region instead of index — `page.getByRole("region", { name: /KPIs/ }).getByText("Orders")`, `page.getByRole("table").getByRole("row", { name: /SPRING20/ })`, or use `getByRole("heading"/"cell"/"tab")` which inherently exclude sibling text nodes. Reserve `.first()` for genuinely list-of-one cases.

### [High] `create.spec.ts` bypasses the factory teardown — orphaned products on failure
- **Where**: `src/web/e2e/seller/listings/create.spec.ts:42-51`
- **Issue**: The happy-path test creates the SKU through the *UI*, so the `productFactory` isn't tracking it. Cleanup is a single `await request.delete(productEndpoint(sku))` at the end of the test — if any earlier assertion fails, the test exits before cleanup runs. The execution report (`qa/test-runs/2026-05-19-crud-execution-report.md:42`) already confirms this caused real flake: "the orphan was deleted via API" before pagination/listings specs would pass.
- **Skill rule**: `e2e-testing-patterns` → "Clean Up Test Data" + Pattern 2 (fixtures own setup *and* teardown).
- **Fix**: Pre-register the SKU with the factory before the UI submit, e.g. extend the factory with `track(sku: string)` that appends to the same `created[]` array. The Promise.all teardown then handles UI-created SKUs identically to API-created ones, regardless of test outcome.

### [Medium] `page.locator("h1")` / `page.content()` content-substring checks
- **Where**: `src/web/e2e/seller/promos.spec.ts:10`, `marketing.spec.ts:13`, `states/first-sale.spec.ts:15` (`page.locator("h1").first()`); `orders/inbox.spec.ts:65-73` (4× `page.content()` then `.toContain`).
- **Issue**: `page.locator("h1")` is exactly the CSS selector the skill warns against — auto-waits work but the assertion checks *some* h1 exists, not that the page heading rendered. `page.content()` substring matching defeats Playwright's auto-wait entirely (snapshot of HTML at one instant) and reports cryptic failures with no DOM context. Negative assertions like `not.toContain("Mira")` will pass on a half-loaded page.
- **Skill rule**: `e2e-testing-patterns` → "Use Data Attributes / Best Practices #1-2" + "Test user-visible behavior, not internals."
- **Fix**: Replace `locator("h1")` with `getByRole("heading", { level: 1, name: /<expected>/ })` (auto-waits and asserts content in one call). Replace `page.content()` checks with `await expect(page.getByText(/Mira/)).toHaveCount(0)` — that one's auto-waiting and won't false-negative on partial loads.

### [Medium] Specs hard-code seed counts/SKUs without isolation between the dev DB and the test run
- **Where**: `src/web/e2e/seller/listings/index.spec.ts:18-61`, `listings/pagination.spec.ts:6-25,43,57,104`, `marketing.spec.ts:43` ("184 buyers · 96% deliverable"), `promos.spec.ts:23-25,40-103`, `orders/inbox.spec.ts:15,30,56,61`, `dashboard.spec.ts`, `analytics.spec.ts:33-34`.
- **Issue**: `@seed-dependent` correctly fences the listings/pagination specs, but `marketing`, `promos`, `orders/inbox`, `dashboard`, `analytics` assert hard-coded counts (`"42"`, `"47 lifetime · 4 need action"`, `tbody tr` count `10`, `"$12,480.00"`, `"184 buyers"`) and are tagged `@smoke` / `@regression` — i.e. they will fail the moment seller-side mutation tests run beforehand and leave an extra product. The CRUD execution report already documents one such collision (`qa/test-runs/2026-05-19-crud-execution-report.md:42`).
- **Skill rule**: `e2e-testing-patterns` → "Keep Tests Independent" + "Common Pitfalls: Coupled Tests."
- **Fix**: Either (a) confirm those pages are fed from static design-time fixtures (then add a one-line comment per spec stating so), or (b) move the count assertions behind `@seed-dependent` so `e2e:smoke` excludes them — the same gate the listings specs use.

### [Medium] `playwright.config.ts` skips traces on local runs and never captures screenshots
- **Where**: `src/web/playwright.config.ts:28-32`
- **Issue**: `trace: "on-first-retry"` combined with `retries: process.env.CI ? 2 : 0` means **local failures produce no trace** (retries=0 ⇒ no first retry ⇒ no trace). `screenshot:` is unset entirely (defaults to `"off"`), so the only failure artifact locally is the 30-byte stack frame. The skill's reference config recommends `trace: "on-first-retry"` *and* `screenshot: "only-on-failure"` — the latter is missing.
- **Skill rule**: `e2e-testing-patterns` → "Setup and Configuration" reference config; "Debugging Failing Tests."
- **Fix**: Add `screenshot: "only-on-failure"` to `use`. Optionally `trace: process.env.CI ? "on-first-retry" : "retain-on-failure"` so a single local failure yields an inspectable trace without rerunning.

### [Medium] `productFactory.create` only tracks the *user-supplied* SKU, not the one persisted
- **Where**: `src/web/e2e/fixtures/test.ts:27-33` & `fixtures/product.ts:24-39`
- **Issue**: `created.push(sku)` records the SKU pre-POST. The API documented as uppercasing+trimming (`qa/test-cases/TC-S24-seller-listings-create-flow.md:60-67`: `"  qa-lower-…  "` → `QA-LOWER-…`) means a future test that supplies a lowercase/whitespace SKU will track `"  qa-lower-…  "` and try to `DELETE /api/products/%20%20qa-lower-…%20%20` — 404, orphan stays. Today no fixture call hits that path, but the contract is silent and brittle.
- **Skill rule**: `e2e-testing-patterns` → fixture/test data ownership + "Make tests deterministic."
- **Fix**: Have `createProduct` return the canonicalized SKU from the response body (or `body.sku`) and push *that*. One-line change, future-proofs against any SKU normalization.

### [Medium] Marketing & promos specs over-decompose into 20+ single-assertion tests with shared `beforeEach`
- **Where**: `src/web/e2e/seller/marketing.spec.ts` (22 tests, all `await page.goto("/seller/marketing")`), `promos.spec.ts` (22 tests, same pattern), `orders/inbox.spec.ts` (12 tests).
- **Issue**: Each test pays a full Next.js cold-start navigation just to assert one `getByText`. With Aspire-backed SSR this is the slowest unit of work — `marketing.spec.ts` alone is ~22 page loads where a single "renders marketing composer" test (one navigation, many `await expect`s) would do the job in <1/15th the time, with no loss of coverage. The skill's reference specs group related assertions per test.
- **Skill rule**: `e2e-testing-patterns` → "Optimize for Speed" + "What NOT to Test with E2E: edge cases (too slow)."
- **Fix**: Consolidate each route's static-content suite into one `test("renders <route> with all expected sections", …)` containing the existing `expect` blocks. Keep one separate `test` for each behavioural assertion (e.g. the drawer interaction at promos.spec.ts:66-72). Net effect: same coverage, ~4-6× faster suite.

### [Low] `pagination.spec.ts` uses a `window` marker hack to detect "no full reload"
- **Where**: `src/web/e2e/seller/listings/pagination.spec.ts:66-91`
- **Issue**: Stashes a string on `window.__noReloadMark`, clicks Next, asserts the marker survives. Works, but it's an implementation-detail probe rather than user-visible behaviour and easy to break if React strict-mode double-runs anything. Playwright's `page.waitForRequest("**/api/listings*", …)` / counting navigations via `page.on("framenavigated")` expresses the intent directly.
- **Skill rule**: `e2e-testing-patterns` → "Test user behavior, not implementation details."
- **Fix**: Replace marker with `let navCount = 0; page.on("framenavigated", () => navCount++);` then after the click assert `expect(navCount).toBe(0)` (or 1 if you count the initial frame). Same guarantee, no global side effects.

### [Low] Manual QA assets duplicate e2e coverage instead of complementing it
- **Where**: `qa/test-cases/TC-S24-seller-listings-create-flow.md`, `TC-S25-…update-flow.md`, `TC-S26-…delete-flow.md`, `TC-S27-…delete-cache-refresh.md` vs. `src/web/e2e/seller/listings/{create,[sku]/update,[sku]/delete}.spec.ts`.
- **Issue**: The 2026-05-19 manual run re-verified exactly the same flows the Playwright specs cover (happy path, dup SKU, validation, delete confirm/cancel/Escape, cache-warm delete). Per CLAUDE.md the intended split is "manual QA covers visual/a11y/console-error angles those miss" — but TC-S24..27 are functional and now redundant. TC-S27 is the one that *did* deliver new value (cache regression) and that case is now also covered in `delete.spec.ts:72-101` (`@seed-dependent`).
- **Skill rule**: `playwright-explore-website` / `playwright-generate-test` — exploration leads to automation; manual passes should not duplicate the automated tier.
- **Fix**: Mark TC-S24..S27 as "superseded by e2e — keep for design/a11y screenshot evidence only" in their summary blocks, and update `qa/README.md` to point at `src/web/e2e/seller/listings/` for the functional CRUD oracle. Reserve future manual passes for visual regressions, a11y audits with axe, and console-noise sweeps that e2e specs don't catch.

---

## Not flagged (intentional)

- **Chromium-only** (`playwright.config.ts:33-38`): the project decided not to run Firefox/WebKit. Skill's multi-browser matrix is aspirational, not a requirement.
- **Real Catalog API, no MSW/mocking** of `/api/products` in listings specs: by design (Aspire-managed stack, CRUD verification is the point). The one route-mock that exists (`spinner.spec.ts`) is for a UX-only assertion, which is correct usage.
- **Aspire-managed stack & dynamic web port** (`fixtures/api.ts`, `playwright.config.ts:6-9`): the env-var-first resolution chain (`services__*` → `BASE_URL`/`API_URL` → localhost) is exactly the right shape for an AppHost-orchestrated runner.
- **`baseURL` defaulting to `http://localhost:3000` for both web and Catalog API**: when running outside Aspire (`API_URL` unset), the dev assumption is "one process serves both," which matches the CLAUDE.md handbook.
- **`force-dynamic` cache assertions and warm-cache test** in `delete.spec.ts:72-101`: legitimate regression coverage for a known TanStack-Query interaction (documented in TC-S27).
- **CI worker cap at 2** (`playwright.config.ts:13`): matches the skill's parallelism guidance for 2-core runners.
