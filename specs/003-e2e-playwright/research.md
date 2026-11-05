# Research: Browser End-to-End Tests

**Feature**: [spec.md](spec.md) | **Plan**: [plan.md](plan.md) | **Date**: 2026-11-05

Each decision lists what was chosen, why, and what else was considered.

## R1. Browser test tool and binding

- **Decision**: Playwright for .NET, package **`Microsoft.Playwright` 1.64.1** (published 2026-10-22),
  in a new xUnit v3 project `tests/MicroCommerce.E2ETests`.
  - **What was approved**: On 2026-11-05 the user approved the .NET binding as
    `Microsoft.Playwright.Xunit.v3` 1.64.1.
  - **What the plan uses**: The plan uses `Microsoft.Playwright`, which that adapter itself depends on,
    without the adapter, for the reasons below. This change was reported to the user for confirmation.
  - **Pin and amendment**: The version is pinned in `Directory.Packages.props`, and the constitution's
    Testing line is amended (1.1.0 → 1.2.0) in this change.
- **Rationale**: Every other suite is C# on xUnit v3 and Microsoft Testing Platform. The .NET binding
  can reuse `Aspire.Hosting.Testing` and the per-run random passwords that `AppHostFixture` already
  injects, so the suite starts the Marketplace the same way the integration tests do.
- **Not `Microsoft.Playwright.Xunit.v3`**: Version 1.64.1 of that adapter is built against
  `xunit.v3.extensibility.core` 1.0.1. The repo pins `xunit.v3` 4.0.1, a major version later, and central
  transitive pinning would force the adapter onto it. Its `PageTest` base class also starts tracing
  before the test body runs, which conflicts with R6. A small base class of our own covers what we need:
  one browser per run, and a fresh browser context, page and trace for each test.
- **Alternatives considered**: The Node `@playwright/test` package in `src/MicroCommerce.Web`. It has a
  richer HTML reporter, but it would need a second way to start the AppHost and pass it secrets.
  Rejected by the user.

## R2. Starting the Marketplace

- **Decision**: An assembly fixture, `MarketplaceFixture`, starts the AppHost with
  `DistributedApplicationTestingBuilder`, following `tests/MicroCommerce.IntegrationTests/AppHostFixture.cs`.
  - It passes `--EphemeralResources=true` and `--WebPort=5174`. See R3 for the port.
  - It generates a random value for each parameter: the three development Account passwords, the Dapr
    token and `keycloak-password`, the Keycloak admin password from Aspire's default parameter.
  - Before the first test it waits until `apiservice`, `keycloak`, `temporal` and `web` are healthy, with
    a 5-minute limit.
  - When the run ends it disposes the app.
- **Rationale**: This reuses a pattern that already works (constitution VII). It also covers FR-001 to
  FR-004 and FR-009: resources are throwaway, ports are random except the web app's, and no secret is
  committed.
- **Duplication over sharing**: The fixture is copied rather than shared with the integration tests,
  because it differs from them: it uses the web port, sets the Keycloak admin password, runs
  pre-flight checks and waits for `web`. The constitution prefers duplication to the wrong abstraction.
- **Failure messages**: If a resource isn't healthy within the time limit, the fixture fails with that
  resource's name (FR-003).
- **Web health check**: `AddViteApp` registers no HTTP health check, so `web` would count as healthy as
  soon as its process starts, before Vite serves anything. The AppHost therefore gets
  `.WithHttpHealthCheck("/")` on `web`, which also makes the dev dashboard more accurate. Otherwise the
  first `GotoAsync` could hit a refused connection.
- **Pre-flight checks (FR-005)**: These run before the AppHost starts, and each failure names what is
  missing:
  - `docker info` succeeds: "container runtime not running".
  - `dapr --version` succeeds: "Dapr CLI not installed, run `dapr init --slim`".
  - Port 5174 is free (R3).
- **How a pre-flight failure looks**: In xUnit v3 an assembly fixture that throws fails every test with
  the same exception. A failed check therefore shows as every test failing at once, with that one
  message, without starting the Marketplace or opening a browser.

## R3. Ports and the sign-in redirect

- **Finding**: Keycloak's dev realm lists `http://localhost:5173/*` as the web app's only redirect URI.
  `--FixedPorts=true` pins both 5173 (web) and 8080 (Keycloak), so a suite that used it couldn't run
  while the developer's Marketplace is running.
- **What actually needs fixing**: Only the web port matters. In ephemeral runs the web app and the API
  get the Keycloak issuer from its endpoint, so Keycloak can keep a random port.
- **Decision**: The suite runs alongside the developer's environment.
  - The AppHost gets an optional `WebPort` setting. When it is set, the `web` endpoint uses that port.
    `FixedPorts` keeps its meaning (5173 and 8080 for day-to-day work).
  - The realm's `micro-commerce-web` client gets `http://localhost:5174` as a second redirect URI, web
    origin and post-logout redirect URI. The `post.logout.redirect.uris` attribute is `##`-separated.
  - Nothing else is pinned to 5173:
    - The web app uses `window.location.origin` as its OIDC redirect (`main.tsx`).
    - The API is reached through the Vite proxy, so it needs no CORS policy (`vite.config.ts`).
    - Vite reads its port from `PORT`, with `strictPort`.
  - The suite passes `--WebPort=5174`.
  - The web endpoint is non-proxied when `WebPort` is set. Aspire's testing builder randomizes the
    host port of proxied endpoints, which would move the web app off 5174. Found in implementation,
    T007.
- **Existing dev volumes**: A persistent Keycloak volume keeps its old realm until the volume is reset.
  It doesn't need the new URI anyway, because only throwaway runs use 5174.
- **Port pre-flight**: Before starting the AppHost, the fixture checks that 5174 is free. If it isn't,
  the run stops with "port 5174 is in use" (edge case: port conflicts).
- **Alternatives considered**: Running with `--FixedPorts=true` and failing fast when 5173 or 8080 is
  taken. That is simpler, but you would have to stop your dev environment for every e2e run.

## R4. Browser and its installation

- **Decision**: Chromium only, headless by default, with a 1280×800 viewport (FR-021).
  - The fixture installs Chromium on first use by calling
    `Microsoft.Playwright.Program.Main(["install", "chromium"])`. It does nothing when the browser is
    already in the per-user cache.
  - Setting `HEADED=1` shows the browser for local debugging.
- **Rationale**: SC-001 asks for one command with no other manual step. Playwright's default install
  script needs PowerShell (`pwsh`), which macOS developers often don't have.
- **Download source**: The browser download comes from Playwright's official CDN, which the approved
  package uses.
- **Alternatives considered**: Listing `pwsh` and `playwright.ps1 install` as prerequisites. That is one
  more manual step and a tool nobody else in the repo needs.

## R5. Accounts and sign-in

- **Findings**:
  - The API turns any valid Keycloak token's `sub` into an Account, so it needs no registration step
    (`Features/Accounts/CurrentAccount.cs`).
  - The web app keeps the OIDC session in `sessionStorage` (`main.tsx`). Playwright's saved storage
    state doesn't include `sessionStorage`, so a saved session can't be reused across tests. Only the
    Keycloak cookie would carry over.
  - Only the seeded `merchant` Account starts with no Merchant, and an Account owns at most one
    Merchant.
- **Decision**:
  - **Test Accounts**: Every test that needs an Account owning a Merchant creates its own Account
    through the Keycloak admin REST API (`POST /admin/realms/micro-commerce/users`). It authenticates
    with an admin token from the `master` realm (`admin-cli`, `admin` / `keycloak-password`).
    - **Fully set up**: Each new user is created ready to sign in, so Keycloak shows no "update profile"
      or "update password" page and the password grant doesn't fail with "Account is not fully set up".
      The user has `enabled: true`, `email` and `emailVerified: true`, `firstName` and `lastName`,
      `credentials: [{ type: "password", value: <random>, temporary: false }]`, and no
      `requiredActions`.
    - **Names and passwords**: The username is unique, and the password is random and held only in
      memory.
  - **Platform Operator tests**: These use the seeded `operator` Account, which owns nothing a test
    changes.
  - **Buyer tests**: Browsing is mostly signed out. Where a signed-in Buyer is needed, the seeded
    `buyer` Account is used.
  - **Sign-in**: Every signed-in test signs in through the Keycloak page: choose **Sign in**, fill
    username and password, submit, and wait for the signed-in header. This covers FR-007 and FR-008.
    The spec allows reusing sessions, but the plan doesn't need it. Measure first (SC-002), and add it
    only if the suite runs too slowly.
- **Alternatives considered**: Adding fixed test users to the realm file. Rejected, because a test that
  opens a Shop could only run once per run, which conflicts with FR-014.

## R6. Keeping passwords out of traces

- **Finding**: A Playwright trace records the value passed to `fill()`. A failure trace would then
  contain the Keycloak password, which FR-009 forbids.
- **Decision**: The sign-in helper fills the Keycloak form **before tracing starts**.
  - In a test that signs in, tracing starts after the web app shows the signed-in Account.
  - In a signed-out test, tracing starts at the beginning of the test.
  - If sign-in itself fails, the report still gets a screenshot (R7). Password fields show only
    masking dots.
  - Passwords are never written to test output or assertion messages.
- **Tokens are redacted from traces (constitution IV)**: A trace's network log records the web app's
  `Authorization: Bearer` headers. It may also hold token responses as stored resources. Constitution
  IV says tokens MUST NOT appear in logs, even throwaway ones, so the trace is rewritten before it is
  kept.
  - `TraceRedactor.Redact(zipPath)` rewrites every text entry of the trace zip, including
    `*.trace`, `*.network` and `resources/*`, by checking each for valid UTF-8.
  - Every JWT-shaped value, `eyJ[A-Za-z0-9_-]+\.[A-Za-z0-9_-]+\.[A-Za-z0-9_-]*`, becomes
    `REDACTED`.
  - This covers headers, request bodies and response bodies.
  - The base class saves the trace to a temporary path, redacts it, then moves it to its final name, so
    no unredacted trace is ever left in `TestResults/`.
  - A check in T038 searches the kept trace for `eyJ` and expects zero matches.
- **Alternatives considered**: Stopping and restarting a trace chunk around the password step. It gives
  the same result with more code.

## R7. Failure evidence and report

- **Decision**:
  - **Run report**: xUnit's own HTML and TRX reports (`--report-xunit-html` and `--report-xunit-trx`,
    xUnit 4.0), on by default through the project's MTP arguments. They are written to the project's
    `TestResults` folder, which `.gitignore` already excludes (FR-018, FR-020).
  - **Per-test evidence**: The test base class reads `TestContext.Current.TestState` when it is disposed.
    On failure, it saves a full-page screenshot and the trace zip, named after the test. On success, it
    discards the trace and takes no screenshot (FR-019).
    - Location: `TestResults/e2e/<Class>.<Method>.png|zip`.
    - On failure, the artefact paths are added to the test output, so the HTML report links them.
  - **Opening a trace**: `npx playwright show-trace <zip>`, or https://trace.playwright.dev, which runs
    locally in the browser.
- **Alternatives considered**: The Node reporter, rejected with R1. Keeping traces for every test makes
  reports large and slow to read.

## R8. Arranging test data

- **Decision**: Tests arrange their starting data through the API and use the browser only to act and
  check.
  - Arranged data includes Products in Draft, Published and Removed states, and a second Merchant.
  - Tokens come from the dev realm's existing `micro-commerce-tests` password-grant client, as in
    `KeycloakTokens.cs`.
  - The Merchant journey (User Story 3) goes entirely through the browser, because that journey is what
    it tests.
  - Every name a test creates includes a short unique run suffix, such as `Sunny Mugs 3f9a1c` (FR-015).
- **Rationale**: Arranging data through the browser is slow and tests the same screens repeatedly.
- **Alternatives considered**: Seeding the database directly. That bypasses the Catalog area's own
  rules, which constitution II forbids.

## R9. Waiting and reliability

- **Decision**:
  - Use only Playwright's auto-waiting locators and web-first assertions (`Expect(...).ToBeVisibleAsync()`
    and similar), with a 10-second expectation limit (FR-016).
  - Never use `WaitForTimeoutAsync` or `Task.Delay`.
  - Never retry a failed test (FR-017).
  - Test classes run in parallel, each test with its own browser context, limited to 4 at a time.
  - SC-004's 10-run check is a documented loop, like the integration tests' restart check.
- **Locators**: Locators use role, label and text only (FR-012).
  - An audit of the 002 pages found accessible names everywhere the journeys need them: Shop form
    labels, Product form fields, Option inputs, Variant grid cells (`"Red / M price"`), the image input
    `"Add an image"`, `"Move left"` / `"Move right"`, and Category tree buttons.
  - Buttons that repeat across rows ("Add child", "Delete") are scoped to their row by its text.
  - If implementation finds a control with no accessible name, the fix is a label in
    `src/MicroCommerce.Web`. It is never a test ID.

## R10. Where the suite runs

- **Decision**: The project is added to `MicroCommerce.slnx`, so `dotnet test --solution MicroCommerce.slnx`
  still runs every suite.
  - It needs the same things as the integration tests (a container runtime and the Dapr CLI), plus a
    free port 5174.
  - Chromium installs itself (R4).
  - It has its own command: `dotnet test --project tests/MicroCommerce.E2ETests`.
  - Role groups use the trait `Journey=SignIn|Buyer|Merchant|PlatformOperator` (spec US5 scenario 3).
- **Rationale**: The constitution says `master` must pass all tests. An e2e suite left out of the
  solution would stop being run.
- **Alternatives considered**: Marking the whole suite `Explicit`. That would hide regressions, which
  defeats the point.

## R11. Interrupted runs

- **Finding**: Aspire's orchestrator (DCP) watches the AppHost process, and it removes non-persistent
  containers when that process ends. That includes the test host being killed with Ctrl+C. Ephemeral runs
  create no volumes. This could not be confirmed for every failure mode, such as a killed orchestrator or
  a machine sleeping.
- **Decision**:
  - The quickstart includes a check: interrupt a run, then confirm that `docker ps -a` shows no
    containers from it.
  - FR-004 and SC-005 are reworded. A passed or failed run leaves nothing behind. After an interrupted
    run, the next run still starts clean, because names and ports never collide with leftovers.
