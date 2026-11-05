# Quickstart: Browser End-to-End Tests

**Feature**: [spec.md](spec.md) | **Plan**: [plan.md](plan.md) | **Decisions**: [research.md](research.md)

These steps prove the suite works and meets the spec. They replace the manual browser sign-in check
in [001's quickstart, Scenario 2](../001-project-skeleton/quickstart.md) (SC-007).

## Prerequisites

The same as the integration tests:

- .NET SDK 10.0.400
- pnpm
- A running container runtime
- The Dapr CLI (`dapr init --slim`)
- Port **5174** free

Chromium downloads itself on the first run into the per-user Playwright cache. Nothing else is
installed by hand.

## Scenario 1: Run the whole suite (US1, SC-001, SC-002)

```bash
dotnet test --project tests/MicroCommerce.E2ETests
```

Expected:

- The Marketplace starts with throwaway resources, and every test runs in headless Chromium.
- The command exits `0`.
- The whole run takes under 10 minutes.
- The HTML and TRX reports are in the repository-root `TestResults/` folder (git-ignored).

## Scenario 2: Runs alongside your dev environment (US1 #3, FR-002)

1. Start your Marketplace as usual: `aspire run`. It uses ports 5173 and 8080 and your persistent data.
2. In another terminal, run Scenario 1.

Expected: the suite passes, and your dev Marketplace's data (Shops, Products) is unchanged.

## Scenario 3: Run one journey or one test (US5 #3)

```bash
dotnet test --project tests/MicroCommerce.E2ETests -- --filter-trait "Journey=Merchant"
```

```bash
dotnet test --project tests/MicroCommerce.E2ETests -- --filter-method "*A_merchant_publishes_a_product*"
```

Set `HEADED=1` to watch the browser while debugging.

## Scenario 4: Failure evidence (US5 #1–#2, FR-019)

1. Temporarily change one expected text in a Buyer test so that it fails.
2. Run `--filter-trait "Journey=Buyer"`.

Expected:

- The report names the failing test and step.
- `TestResults/e2e/` holds `<Class>.<Method>.png` and `.zip` for that test only.
- `npx playwright show-trace <zip>` replays its steps.
- The trace contains no password: search it for the `fill` actions, and none target the Keycloak
  password field (R6).
- The trace contains no token: `unzip -o <zip> -d /tmp/trace && grep -rl "eyJ" /tmp/trace` prints
  nothing.

Revert the change afterwards.

## Scenario 5: Missing prerequisite and port conflict (US1 #5, edge cases)

1. Stop the container runtime and run Scenario 1. Expected: every test fails at once with the same
   message, "container runtime not running". The Marketplace does not start and no browser opens.
2. Occupy the port with `python3 -m http.server 5174`, then run Scenario 1. Expected: it stops with
   "port 5174 is in use".

## Scenario 6: Clean-up (FR-004, SC-005)

1. After Scenario 1, `docker ps -a` and `docker volume ls` show nothing from the run.
2. Start a run and press Ctrl+C during the tests. Within a minute, `docker ps -a` shows no containers
   from it. Run Scenario 1 again. Expected: it passes.

## Scenario 7: Reliability (SC-004)

```bash
for i in $(seq 10); do dotnet test --project tests/MicroCommerce.E2ETests || break; done
```

Expected: all 10 runs pass. No test retries exist, so a pass is a real pass.

## Coverage check (SC-003)

Every row of the [mapping table](data-model.md#browser-test--scenario-mapping) has a test. The
`Journey` trait counts in the report match: SignIn 1, Buyer ≥ 5, Merchant ≥ 5, PlatformOperator ≥ 3.
