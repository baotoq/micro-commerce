# Data Model: Browser End-to-End Tests

**Feature**: [spec.md](spec.md) | **Plan**: [plan.md](plan.md)

This feature persists nothing and adds no domain entity. The entities below exist only in memory
during a test run. They are listed so that tests name and use them consistently.

## Test run

One execution of the suite (`MarketplaceFixture`, one per run).

| Field | Meaning | Rule |
|-------|---------|------|
| Run suffix | Short random hex string (6 chars) | Appended to every name a test creates (FR-015) |
| Parameters | Development Account passwords, `keycloak-password`, Dapr token | Random per run; never logged, never in traces (FR-009, R6) |
| Web address | `http://localhost:5174` | Port must be free before start, or the run stops (R3) |
| Marketplace | The throwaway AppHost | Healthy before the first test; disposed at the end (FR-003, FR-004) |
| Browser | One Chromium instance | Shared; each test gets its own context (R4) |

## Test Account

An Account a test signs in with.

| Kind | Source | Used by |
|------|--------|---------|
| Seeded `buyer` | Dev realm, password from run parameters | Buyer tests that need to be signed in; the US4 refusal check |
| Seeded `operator` (Platform Operator role) | Dev realm | Platform Operator tests |
| Fresh Account | Created per test through the Keycloak admin API, `e2e-<role>-<suffix>-<n>`, random in-memory password | Every test that opens a Merchant, either in the browser (US3) or through the API (arranging US2 and US4 data) |

**Rule**: A fresh Account becomes a Marketplace Account the first time its token is used, because the
API maps the token `sub` to an Account (R5). The seeded `merchant` Account is not used, so no test
depends on it having no Merchant.

## Arranged Catalog data

Created through the API before the browser acts (R8). Uses the Catalog's own rules and published
endpoints.

| Item | States used | Notes |
|------|-------------|-------|
| Merchant | Open | Two Merchants for US2. Shop name `E2E Shop <suffix>-<n>` |
| Product | Draft, Published, Removed | One distinctive word in the name for the search scenario |
| Variant | 1–4 per Product | Price and stock set. Buyers must see Availability, never stock |
| Product image | 1–2 | Small valid PNG from test resources |
| Category | Starter tree from 002 seed (`Home › Kitchen`) | US4 adds a child Category under it |

## Browser test → scenario mapping

Each test carries a `Journey` trait and refers to its scenario in its name or a comment. This is how
SC-003's 100% mapping is checked.

| Journey trait | Spec scenarios | Test class |
|---------------|----------------|-----------|
| `SignIn` | US1 #2 | `SignInTests` |
| `Buyer` | US2 #1–#5 | `BuyerBrowsingTests` |
| `Merchant` | US3 #1–#5 | `MerchantSellingTests` |
| `PlatformOperator` | US4 #1–#3 | `PlatformOperatorTests` |

## Failure evidence

| Artefact | Path | Kept when |
|----------|------|-----------|
| Screenshot (full page) | `tests/MicroCommerce.E2ETests/bin/**/TestResults/e2e/<Class>.<Method>.png` | Test failed |
| Trace (replayable steps) | `…/TestResults/e2e/<Class>.<Method>.zip` | Test failed; starts after sign-in |
| HTML and TRX report | `…/TestResults/` | Every run |
