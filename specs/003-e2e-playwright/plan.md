# Implementation Plan: Browser End-to-End Tests

**Branch**: `003-e2e-playwright` | **Date**: 2026-11-05 | **Spec**: [spec.md](spec.md)

**Input**: Feature specification from `specs/003-e2e-playwright/spec.md`

## Summary

Add a browser end-to-end suite, `tests/MicroCommerce.E2ETests`. It drives the web app in a real
Chromium browser, against the whole Marketplace started by the AppHost with throwaway resources. It
signs in through Keycloak's real page and covers the Catalog journeys of feature 002 for Buyers,
Merchant owners and Platform Operators.

- **Approach**: Playwright for .NET (`Microsoft.Playwright` 1.64.1, approved by the user on 2026-11-05)
  on xUnit v3. An assembly fixture follows the integration tests' `AppHostFixture` pattern.
- **Running next to development**: A new `WebPort` AppHost setting and a second realm redirect URI let
  the suite run while the developer's own Marketplace is up.
- **Test data**: Tests get unique Accounts from the Keycloak admin API, and arrange their starting data
  through the API.
- **Secrets**: Traces start only after sign-in, so passwords never reach a trace, and every kept trace
  has its tokens redacted.

## Technical Context

**Language/Version**: C# on .NET 10 (SDK 10.0.400), with nullable reference types and warnings as
errors (inherited from `Directory.Build.props`).

**Primary Dependencies**:

- **New**: `Microsoft.Playwright` 1.64.1. The user approved the .NET binding as
  `Microsoft.Playwright.Xunit.v3`. The plan uses its base package without the adapter (R1); see the
  Complexity Tracking table.
- **Reused**:
  - `Aspire.Hosting.Testing` 13.6.0
  - `xunit.v3` 4.0.1 (Microsoft Testing Platform)
  - `Shouldly` 4.3.0

**Storage**: None of its own. The throwaway Marketplace brings PostgreSQL, Redis, Azurite, Keycloak
and Temporal, which are disposed after the run.

**Testing**: The feature is itself a test suite.

- **Run report**: xUnit HTML and TRX reports.
- **Failure evidence**: A Playwright screenshot and trace for each failed test.

**Target Platform**: Developer machines (macOS, Linux and Windows) with a container runtime and the
Dapr CLI. Chromium is installed automatically.

**Project Type**: A test project added to the existing web-service solution, plus small changes to the
AppHost and the Keycloak realm.

**Performance Goals**: The whole run takes under 10 minutes, including Marketplace startup (SC-002).
Startup alone takes about 1 to 2 minutes today, as in the integration tests.

**Constraints**:

- No fixed delays and no retries.
- Locators use only roles, labels and text.
- No secrets in code, traces or reports.
- Port 5174 must be free.

**Scale/Scope**: About 20 browser tests, covering the sign-in check and the 13 acceptance scenarios in
User Stories 2 to 4, plus the refusal check. Tests are grouped by journey trait.

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Check | Result |
|-----------|-------|--------|
| **I. Ubiquitous Language** | Test classes, methods, traits and helper names use glossary terms: Account, Merchant, Shop, Product, Variant, Availability, Category, Platform Operator, Merchant Centre. No new domain concept, so no `CONTEXT.md` change. | Pass |
| **II. Domain-Centred Design** | No domain or business-rule change. Test data is arranged through the published API, never by writing another area's tables (R8). | Pass |
| **III. Test-First, Fast and Isolated** | Unit test rules don't apply. The suite provisions its own throwaway infrastructure and disposes it (R2, R11). Tests create unique data and don't depend on order (R5, R8). There are no retries: a flaky test is fixed or removed (R9). The AppHost change (`WebPort`) is covered by the suite itself, which fails if the web app isn't on 5174. | Pass |
| **IV. Secure by Default** | No secret is committed. Every password, including the Keycloak admin password, is random per run and passed as an AppHost parameter (R2). Passwords never reach traces, screenshots or output, and bearer tokens are redacted from every kept trace (R6). The extra redirect URI is `localhost` only, in the dev realm. One authorisation refusal is checked in the browser (FR-011). | Pass |
| **V. Durable and Idempotent Processes** | Not affected. | N/A |
| **VI. Observable Behaviour** | No new application code. Test helpers write only through xUnit's test output, with no personal data or secrets. | Pass |
| **VII. Simplicity and Deliberate Dependencies** | One new dependency, approved by the user on 2026-11-05 and pinned centrally. The constitution's Technology Constraints are amended in this change (1.1.0 → 1.2.0). The fixture pattern is reused (copied rather than abstracted, R2). Session reuse and multiple browsers are not built (R5, FR-021). | Pass, amendment recorded below |
| **Technology Constraints** | Playwright is not in the approved stack. | Amendment: the Testing line adds "Playwright for .NET for browser end-to-end tests" (MINOR, 1.2.0) |
| **Development Workflow** | The suite joins the solution, so `master` must pass it (R10). The README Tests table and the 001 quickstart are updated in the same PR. | Pass |

**Gate result (pre-research)**: Pass, with the Technology Constraints amendment approved by the user.

**Gate result (post-design)**: Pass. Phase 1 added no further dependencies or abstractions. The
AppHost and realm changes are small and local-only (see Project Structure).

## Project Structure

### Documentation (this feature)

```text
specs/003-e2e-playwright/
├── plan.md              # This file
├── research.md          # Phase 0: decisions R1–R11
├── data-model.md        # Phase 1: test-side entities (nothing persisted)
├── quickstart.md        # Phase 1: how to run and validate the suite
├── checklists/
│   └── requirements.md  # Spec quality checklist
└── tasks.md             # Phase 2 (/speckit-tasks), not created here
```

`contracts/` is not created. The feature adds no API, event or other interface. The only interface
is the test command and its options, which the [quickstart](quickstart.md) documents.

### Source Code (repository root)

```text
tests/MicroCommerce.E2ETests/                 # NEW xUnit v3 project (MTP), in MicroCommerce.slnx
├── MicroCommerce.E2ETests.csproj             # Microsoft.Playwright, Aspire.Hosting.Testing; AppHost project reference;
│                                             # MTP args turn on --report-xunit-html and --report-xunit-trx
├── AssemblyInfo.cs                           # [assembly: AssemblyFixture(typeof(MarketplaceFixture))], max 4 parallel
├── MarketplaceFixture.cs                     # Pre-flight (docker, dapr, port 5174), AppHost start, healthy-wait, Chromium install + launch (R2–R4)
├── BrowserTest.cs                            # Base class: context + page per test, trace after sign-in, evidence on failure (R6, R7)
├── TraceRedactor.cs                          # Replaces every JWT in a saved trace zip with REDACTED (R6)
├── TestAccounts.cs                           # Keycloak admin API: create unique Accounts; password-grant tokens (R5)
├── CatalogArrange.cs                         # API helpers: open Merchant, create/publish/remove Product, unique names (R8)
├── SignIn.cs                                 # Keycloak page sign-in helper (no tracing while filling)
└── Journeys/
    ├── SignInTests.cs                        # Journey=SignIn: US1 scenario 2
    ├── BuyerBrowsingTests.cs                 # Journey=Buyer: US2 scenarios 1–5
    ├── MerchantSellingTests.cs               # Journey=Merchant: US3 scenarios 1–5
    └── PlatformOperatorTests.cs              # Journey=PlatformOperator: US4 scenarios 1–3

src/MicroCommerce.AppHost/AppHost.cs          # CHANGED: optional WebPort setting pins the web endpoint;
                                              # web gets WithHttpHealthCheck("/") (R2)
src/MicroCommerce.AppHost/Realms/micro-commerce-realm.json
                                              # CHANGED: micro-commerce-web gains http://localhost:5174 redirect URI,
                                              # web origin and post-logout redirect URI
src/MicroCommerce.Web/src/**                  # ONLY IF the audit in implementation finds a control without an accessible name (R9)

Directory.Packages.props                      # CHANGED: Microsoft.Playwright 1.64.1 under "Testing"
MicroCommerce.slnx                            # CHANGED: adds the E2ETests project
.specify/memory/constitution.md               # CHANGED: 1.2.0, Testing line adds Playwright for .NET
README.md                                     # CHANGED: Tests table row and e2e commands
specs/001-project-skeleton/quickstart.md      # CHANGED: Scenario 2 points to the automated sign-in test (SC-007)
```

**Structure Decision**: The new project sits next to the other three suites under `tests/` and
inherits `tests/Directory.Build.props`, which brings xUnit v3, Shouldly and NSubstitute. The project
gets its own fixture instead of sharing `IntegrationTests/AppHostFixture.cs`, because the two differ in
port, admin password, pre-flight checks and the resources they wait for (R2).

## Complexity Tracking

No constitution violations. The one new dependency is approved, and the amendment is part of this
change.

| Item | Why needed | Simpler alternative rejected because |
|------|-----------|--------------------------------------|
| `WebPort` AppHost setting and second redirect URI (5174) | The suite can run while the developer's Marketplace is up | `--FixedPorts=true` reuses 5173 and 8080, so you'd have to stop your dev environment for every run (R3) |
| Own Playwright base class instead of `Microsoft.Playwright.Xunit.v3` | The adapter targets xUnit extensibility 1.0.1 (the repo is on 4.0.1), and it starts tracing before sign-in, which would leak passwords | Adopting the adapter risks binary breaks, and its tracing hook would still need overriding (R1, R6) |
