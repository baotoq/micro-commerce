# Quickstart and Validation: Project Skeleton

This guide shows how to check that the skeleton meets [spec.md](./spec.md). Endpoint details are in
[contracts/http-api.md](./contracts/http-api.md), and event details are in
[contracts/events.md](./contracts/events.md).

## Prerequisites

| Tool | Version | Check |
|------|---------|-------|
| .NET SDK | 10.0.400 (pinned in `global.json`) | `dotnet --version` |
| Aspire CLI | 13.6 | `aspire --version` |
| Node.js and pnpm | Node 24 or later, pnpm | `node -v && pnpm -v` |
| Container runtime | Docker or Podman | `docker info` or `podman info` |
| Dapr CLI and runtime | 1.15 or later | `dapr --version` |

One-time setup:

```bash
dapr init --slim                      # installs daprd only; no containers
pnpm --dir src/MicroCommerce.Web install
```

**Podman users (research R11)** need these environment variables in their shell profile. Aspire and
Testcontainers don't see a shell `docker` alias.

```bash
export ASPIRE_CONTAINER_RUNTIME=podman
export DOCKER_HOST="unix://$(podman machine inspect --format '{{.ConnectionInfo.PodmanSocket.Path}}')"
export TESTCONTAINERS_DOCKER_SOCKET_OVERRIDE=/var/run/docker.sock
```

## Scenario 1: Start everything (User Story 1, FR-001 to FR-005, SC-001, SC-002)

```bash
aspire run
```

Expected:

- The dashboard opens and shows these resources as **Running/Healthy** within about 2 minutes
  after the first run:
  - `postgres` and `micro-commerce-db`
  - `redis`
  - `temporal`
  - `keycloak`
  - `apiservice`, together with its Dapr sidecar
  - `web`
  - the `pubsub` component
- `apiservice` logs show that migrations were applied.
- Opening the `web` endpoint (`http://localhost:5173`) shows the home page reporting that the API
  is reachable.
- **Negative check**: stop the container runtime and run `aspire run` again. The command fails with
  a message naming the container runtime.

## Scenario 2: Sign in (User Story 2, FR-006 to FR-010, SC-003)

1. In the dashboard, open **Parameters** and copy `dev-buyer-password`.
2. In the web app, choose **Sign in**. On Keycloak, sign in as `buyer` with that password.
3. Expected: you return to the web app, which shows "Bea Buyer", the Account ID and
   "Platform Operator: no", all as reported by `GET /api/me`.
4. Choose **Sign out**. Expected: the protected view is gone, and **Sign in** shows again.
5. Repeat with `operator`, whose password is `dev-operator-password`. Expected: "Platform Operator:
   yes".
6. Anonymous check: `curl -i <apiservice-url>/api/me` returns `401`.

## Scenario 3: Durable process survives a restart (User Story 4, FR-011, FR-012, FR-012a, SC-007)

1. Sign in as `operator`. Get a token from the browser dev tools, or use the Scalar UI at
   `<apiservice-url>/scalar/v1`.
2. Call `POST /api/diagnostics/sample-processes` with `{ "delaySeconds": 30 }`. Note the `id`.
3. Within 30 seconds, restart `apiservice` from the dashboard (**…**, then **Restart**).
4. After 30 seconds, call `GET /api/diagnostics/sample-processes/{id}`. Expected:
   - `status` is `"Completed"`.
   - `completedAt` is at least 30 seconds after `startedAt`.
   - `completionEventReceivedAt` is set.
5. In the Temporal UI (the `temporal` resource), the workflow `sample-process-{id}` shows a single
   completed run.

## Scenario 4: Tests (User Story 3, FR-020 to FR-023, SC-004 to SC-006, SC-008)

```bash
dotnet test --project tests/MicroCommerce.UnitTests   # no infrastructure; under 30 s (SC-005)
dotnet test --solution MicroCommerce.slnx             # all .NET suites; under 5 min (SC-006)
pnpm --dir src/MicroCommerce.Web test                 # web app unit tests
```

Expected:

- Every suite passes.
- Afterwards, `docker ps` or `podman ps` shows no containers left over from the test run.
- Running the unit tests with networking disabled still passes.
- For SC-008, run the full suite 10 times in a row with no changes. The results must be identical:

```bash
for i in $(seq 10); do dotnet test --solution MicroCommerce.slnx || break; done
```

## Which suite verifies what

| Requirement | UnitTests | FunctionalTests | IntegrationTests | Quickstart (manual) |
|-------------|:-:|:-:|:-:|:-:|
| FR-001–FR-004 (one-command start, health, migrations and seeding) | | | ✓ (all resources healthy) | Scenario 1 |
| FR-006, FR-010 (browser sign-in and sign-out) | | | | Scenario 2 |
| FR-007 (anonymous, expired and tampered tokens refused) | | ✓ | ✓ (anonymous) | |
| FR-008, FR-009 (`/api/me` from the token) | ✓ (`ICurrentAccount`) | ✓ | ✓ (real Keycloak token) | Scenario 2 |
| FR-011, FR-012 (durable process, restart) | ✓ (time-skipping workflow) | ✓ (real Temporal) | ✓ (restart, SC-007) | Scenario 3 |
| FR-012a (event round trip through Dapr) | | | ✓ | Scenario 3 |
| FR-012b (retry and duplicates) | ✓ (aggregate `Complete` idempotent) | ✓ (same event posted twice) | | |
| FR-013 (domain has no infrastructure dependencies) | Build: the Domain project references only Vogen | | | |
| FR-014 (typed IDs) | Compiler | | | |
| FR-015, FR-016 (validation, ProblemDetails) | ✓ (validators) | ✓ | | |
| FR-017 (OpenAPI) | | ✓ (document served, and Dapr routes excluded) | | |
| FR-018, FR-019 (logging, no secrets) | | | | Code review and the constitution check |
| SC-004 (every protected endpoint refuses anonymous calls) | | ✓ (one test per endpoint) | | |
