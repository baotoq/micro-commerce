# MicroCommerce

A Marketplace built on .NET 10, Aspire, FastEndpoints, EF Core with PostgreSQL, Temporal, Dapr,
Keycloak and a React + Vite web app. The domain vocabulary is in [CONTEXT.md](CONTEXT.md) and the
engineering rules are in [the constitution](.specify/memory/constitution.md).

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

**Podman users** need these environment variables in their shell profile. Aspire and
Testcontainers don't see a shell `docker` alias.

```bash
export ASPIRE_CONTAINER_RUNTIME=podman
export DOCKER_HOST="unix://$(podman machine inspect --format '{{.ConnectionInfo.PodmanSocket.Path}}')"
export TESTCONTAINERS_DOCKER_SOCKET_OVERRIDE=/var/run/docker.sock
```

## Run

```bash
aspire run
```

This starts PostgreSQL, Redis, Temporal, Keycloak (with the `micro-commerce` realm imported), the
ApiService with its Dapr sidecar, and the web app. Then:

- **Dashboard**: the URL that `aspire run` prints. Every resource should become Running or Healthy.
- **Web app**: <http://localhost:5173>.
- **API reference (Scalar)**: `<apiservice-url>/scalar/v1`, in Development only. The OpenAPI
  document is at `<apiservice-url>/openapi/v1.json`.
- **Temporal UI**: the `ui` endpoint of the `temporal` resource.
- **Dev sign-in**: the seeded Accounts are `buyer` and `operator` (a Platform Operator). Their
  passwords are generated on the first run and kept in user secrets. Find them in the dashboard
  under **Parameters**: `dev-buyer-password` and `dev-operator-password`.

Dev runs keep their containers and data volumes between runs. Keycloak imports the realm only when
it doesn't have it yet, so after editing `src/MicroCommerce.AppHost/Realms/micro-commerce-realm.json`,
delete the Keycloak volume.

## Layout

```text
global.json                          # SDK 10.0.400, Microsoft.Testing.Platform runner
MicroCommerce.slnx
Directory.Build.props                # nullable, warnings as errors
Directory.Packages.props             # every package version, pinned centrally

src/
├── MicroCommerce.AppHost/           # Aspire: postgres, redis, temporal, keycloak, dapr pubsub, apiservice + sidecar, web
│   └── Realms/                      # Keycloak dev realm; passwords are ${ENV} placeholders
├── MicroCommerce.ServiceDefaults/   # OpenTelemetry, health checks, service discovery, resilience
├── MicroCommerce.Domain/            # aggregates, value objects and typed IDs; references only Vogen
├── MicroCommerce.ApiService/
│   ├── Features/                    # vertical slices: endpoints, validators, workflows, handlers
│   └── Infrastructure/              # Data (EF Core), Messaging (Dapr), Mapping (Mapster), OpenApi
└── MicroCommerce.Web/               # React + Vite + TypeScript (pnpm)

tests/
├── MicroCommerce.UnitTests/         # no infrastructure
├── MicroCommerce.FunctionalTests/   # API in process, Testcontainers
└── MicroCommerce.IntegrationTests/  # whole AppHost, Aspire.Hosting.Testing
```

## Migrations

Migrations are applied at startup in Development. To add one:

```bash
dotnet ef migrations add <Name> -p src/MicroCommerce.ApiService -o Infrastructure/Data/Migrations
```

## Tests

| Suite | Covers | Needs |
|-------|--------|-------|
| `tests/MicroCommerce.UnitTests` | Domain rules, validators, the Mapster configuration (`Compile()`), request context, exception handling, activities with NSubstitute doubles, and the workflow on Temporal's time-skipping test server | No database, container or running service. The Temporal test server binary is downloaded on first use and cached. |
| `tests/MicroCommerce.FunctionalTests` | Every endpoint in process (FastEndpoints.Testing) with signed test tokens: success, validation, 401 and 403. Also the inbox de-duplication, and the workflow against a real Temporal server | A container runtime (Testcontainers: PostgreSQL and Temporal) |
| `tests/MicroCommerce.IntegrationTests` | The whole AppHost (Aspire.Hosting.Testing): every resource healthy, real Keycloak tokens against `/api/me`, the sample process end to end through the real Dapr sidecar, and an API restart while a process waits | A container runtime and the Dapr CLI (`dapr init --slim`) |
| `src/MicroCommerce.Web` (Vitest) | The home page signed out, signed in, and with the API unreachable | Nothing |

```bash
dotnet test --solution MicroCommerce.slnx             # every .NET suite
dotnet test --project tests/MicroCommerce.UnitTests   # unit tests only
pnpm --dir src/MicroCommerce.Web test                 # web app unit tests
```

Integration tests start the AppHost with `--EphemeralResources=true`: they use throwaway containers
on random ports and random seeded passwords, and remove everything when the run ends. They never
touch your dev containers or data.

The restart test (`Category=Restart`) also runs on its own for the 10-run reliability check:

```bash
for i in $(seq 10); do dotnet test --project tests/MicroCommerce.IntegrationTests -- --filter-trait "Category=Restart" || break; done
```
