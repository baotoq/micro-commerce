# Implementation Plan: Project Skeleton (Walking Skeleton)

**Branch**: `001-project-skeleton` | **Date**: 2026-11-05 | **Spec**: [spec.md](./spec.md)

**Input**: Feature specification from `specs/001-project-skeleton/spec.md`

## Summary

Build a runnable walking skeleton for MicroCommerce:

- One Aspire command starts PostgreSQL, Redis, Temporal, Keycloak, the ApiService with its Dapr
  sidecar, and a React + Vite web app.
- An Account signs in through Keycloak using PKCE, and the web app shows `GET /api/me`.
- A throwaway `SampleProcess` aggregate shows the conventions for later features:
  - DDD in a Domain project that references only Vogen, with Vogen IDs, EF Core persistence and
    outward Mapster mapping.
  - A FastEndpoints endpoint with validation and ProblemDetails.
  - A Temporal timer workflow that survives restarts.
  - A Dapr pub/sub event with an idempotent inbox.
- Three .NET test tiers (unit, functional and AppHost integration) plus web-app unit tests check
  every requirement.

Most patterns come from the codebase as it was before the re-init (research R4, R5, R7 and R10).
What is new: Mapster in place of Mapperly, Dapr in place of Wolverine, NSubstitute, a separate
Domain project, and a Vite SPA in place of Next.js.

## Technical Context

**Language/Version**: C# on .NET 10 (SDK 10.0.400, `global.json`), with `Nullable` and
`TreatWarningsAsErrors` set in `Directory.Build.props`. TypeScript 5 for the web app.

**Primary Dependencies**:

- Aspire 13.6 (AppHost SDK, PostgreSQL, Redis, Keycloak and JavaScript hosting)
- CommunityToolkit.Aspire.Hosting.Dapr 13.0.0
- FastEndpoints 8.3 with FastEndpoints.OpenApi and Scalar
- EF Core 10 with Npgsql (Aspire client)
- Vogen 8.0.7, Mapster 10.0.14, Temporalio 1.21, Dapr.AspNetCore 1.19
- JwtBearer
- Web app: React 19, Vite 8, react-oidc-context and oidc-client-ts (pending approval)

**Storage**: PostgreSQL (`micro-commerce-db`). Redis serves only as the Dapr pub/sub backing store.

**Testing**:

- xUnit v3 on Microsoft.Testing.Platform, with NSubstitute and Shouldly
- FastEndpoints.Testing and Testcontainers (PostgreSQL, Temporal)
- Aspire.Hosting.Testing
- Temporal time-skipping environment
- Vitest and Testing Library for the web app (pending approval)

**Target Platform**: Local development on macOS, Linux and Windows, with Docker or Podman.
Deployment is out of scope.

**Project Type**: Web application (backend service, SPA frontend and orchestrator).

**Performance Goals**:

- Healthy within 2 minutes of the start command, after the first run (SC-002)
- Sign-in round trip under 30 seconds (SC-003)
- Unit tests under 30 seconds (SC-005)
- Full test suite under 5 minutes (SC-006)

**Constraints**:

- No committed secrets, including seeded passwords and the Dapr app token (FR-019, Principle IV)
- Unit tests need no infrastructure
- Integration tests leave nothing running
- No new dependency without approval

**Scale/Scope**:

- 4 production projects, 3 .NET test projects and 1 web app
- 5 HTTP endpoints, plus the Dapr and health endpoints
- 1 aggregate, 1 workflow and 1 event

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Gate | Pre-design | Post-design |
|-----------|------|:-:|:-:|
| I. Ubiquitous Language | Uses Account, Platform Operator and Buyer, never User or Admin. `CONTEXT.md` is unchanged, because `SampleProcess` is technical and not a domain term (see Complexity Tracking). | ✅ | ✅ |
| II. Domain-Centred Design | The rules live in the `SampleProcess` aggregate. The Domain project references only Vogen, so the build enforces this (R1). IDs are typed (R2). Mapping goes outward only (R3). One aggregate changes per transaction. | ✅ | ✅ |
| III. Tests | There are three tiers, and unit tests need no infrastructure. Every endpoint has a success test and a 401 or 403 test. Coverage is mapped in the [quickstart table](./quickstart.md#which-suite-verifies-what). | ✅ | ✅ |
| IV. Secure by Default | All `/api` endpoints require an Account. The Account comes from the token (ADR-50, `ICurrentAccount`). Validators run at the boundary. Seeded passwords and the Dapr token are generated Aspire secret parameters (R6, R7). The `/dapr/*` endpoints use the Dapr app-token scheme, not an Account (justified below). | ✅ | ✅ (two items to verify, below) |
| V. Durable and Idempotent | The timer runs as a Temporal workflow. The `Complete` activity is idempotent. Publishing is at least once, with a deterministic `eventId`, and the inbox de-duplicates (R5, R6). | ✅ | ✅ |
| VI. Observable | ServiceDefaults provides OTel, health checks and the dashboard. Logging is structured through `ILogger<T>`. Tokens and email addresses are never logged. Errors use ProblemDetails. | ✅ | ✅ |
| VII. Simplicity and Dependencies | No router, query library, BFF or Mapster code generation. Unused Redis caching and Testcontainers.Redis are dropped. New npm packages are marked **pending approval** (R12). | ✅ | ✅ |

**Items to verify in the first implementation task**: if any of these fails, use its fallback and add
a row to Complexity Tracking.

1. Keycloak substitutes `${ENV}` placeholders in imported user credentials (R7).
2. The Dapr sidecar resource in CommunityToolkit 13.0.0 accepts `APP_API_TOKEN`, and
   `WithMetadata` works for Redis (R6).
3. `Aspire.Hosting.Testing` 13.6 can run the `resource-restart` command (R5).

## Project Structure

### Documentation (this feature)

```text
specs/001-project-skeleton/
├── plan.md              # This file
├── research.md          # Phase 0: R1–R12 decisions
├── data-model.md        # Phase 1: SampleProcess, inbox, CurrentAccount
├── quickstart.md        # Phase 1: run and validation guide, plus the FR-to-test map
├── contracts/
│   ├── http-api.md      # /api/me, sample-process endpoints, /dapr/*, health
│   └── events.md        # sample-process-completed (SampleProcessCompletedV1)
├── checklists/requirements.md
└── tasks.md             # Phase 2 (/speckit-tasks), not created here
```

### Source Code (repository root)

```text
global.json                          # SDK 10.0.400, Microsoft.Testing.Platform runner
MicroCommerce.slnx
Directory.Build.props                # existing
Directory.Packages.props             # existing; changes in research R12, made during implementation
README.md                            # prerequisites, run and test commands (FR-005)

src/
├── MicroCommerce.AppHost/
│   ├── AppHost.cs                   # postgres, redis, temporal, keycloak, dapr pubsub, apiservice and sidecar, web
│   ├── Realms/micro-commerce-realm.json   # placeholders for passwords; no secrets
│   └── dapr/                        # only if the R6 fallback is needed: pubsub component YAML
├── MicroCommerce.ServiceDefaults/   # OTel, health, service discovery, resilience (old pattern)
├── MicroCommerce.Domain/            # references only Vogen
│   ├── SharedKernel/                # AggregateRoot<TId>, IDomainEvent, DomainRuleViolation, AccountId
│   └── Diagnostics/                 # SampleProcess, SampleProcessId, SampleProcessStatus, SampleProcessCompleted
├── MicroCommerce.ApiService/
│   ├── Program.cs
│   ├── Features/
│   │   ├── Accounts/                # GetMe endpoint, ICurrentAccount, AuthPolicies
│   │   └── Diagnostics/             # StartSampleProcess and GetSampleProcess endpoints and validators,
│   │                                # SampleProcessWorkflow and activities, SampleProcessCompleted subscriber
│   ├── Infrastructure/
│   │   ├── Data/                    # AppDbContext, VogenEfCoreConverters, Migrations/
│   │   ├── Messaging/               # IIntegrationEventPublisher, DaprIntegrationEventPublisher, DaprSubscribe endpoint, inbox
│   │   └── Mapping/                 # Mapster TypeAdapterConfig registration
│   └── appsettings*.json            # no secrets
└── MicroCommerce.Web/               # React + Vite + TS (pnpm)
    ├── vite.config.ts               # /api proxy to the Aspire-discovered apiservice
    └── src/                         # main.tsx (AuthProvider), App.tsx (home, sign in/out, /api/me), api.ts

tests/
├── Directory.Build.props            # xunit.v3, Shouldly, NSubstitute, implicit usings
├── MicroCommerce.UnitTests/         # Domain, validators, Mapster Compile(), handlers and activities with
│                                    # NSubstitute, workflow with time skipping
├── MicroCommerce.FunctionalTests/   # ApiFixture (FastEndpoints.Testing + Testcontainers PG/Temporal), TestIdentity
└── MicroCommerce.IntegrationTests/  # AppHostFixture (Aspire.Hosting.Testing, --EphemeralResources=true)
```

**Structure Decision**: This is the old codebase's layout, with three changes:

- `src/MicroCommerce.Domain` is new and enforces FR-013.
- The web app moves to `src/MicroCommerce.Web` as a Vite SPA.
- `ApiService/Infrastructure/` groups the cross-cutting adapters.

Feature folders in the ApiService stay vertical slices: each one holds its endpoint, validator,
mapping and handlers.

## Phase 0 and 1 Outputs

- [research.md](./research.md): every technical unknown is resolved. No NEEDS CLARIFICATION
  remains. Two npm dependency sets are recommended and pending user approval.
- [data-model.md](./data-model.md), [contracts/http-api.md](./contracts/http-api.md),
  [contracts/events.md](./contracts/events.md) and [quickstart.md](./quickstart.md).

## Risks

| Risk | Mitigation |
|------|------------|
| The SPA holds access tokens in the browser, in `sessionStorage`, which exposes them to XSS. | Keep tokens short-lived, use PKCE, and add no third-party scripts. Revisit with a BFF before any production deployment. |
| CommunityToolkit Dapr 13.0.0 lags Aspire 13.6. The newer APIs are beta only. | Use only APIs confirmed in 13.0.0. The R6 fallbacks are a component YAML and environment variables. |
| The Dapr CLI becomes a prerequisite for developers and for integration tests. | `dapr init --slim` needs no containers. It is documented in the README and quickstart. |
| Podman differences in Aspire, Testcontainers and Ryuk. | Environment variables are documented in quickstart and research R11. |
| `.claude/settings.json` still runs `.claude/hooks/session-start.sh`, which the re-init deleted. | Outside this feature. It is reported to the user. |

## Complexity Tracking

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| An extra `MicroCommerce.Domain` project (Principle VII: no speculative layers) | FR-013 requires a domain with no infrastructure dependencies, and only a project boundary is checked by the build | A `Domain/` folder in the ApiService can't be verified |
| `SampleProcess` is not in `CONTEXT.md` (Principle I) | It is a technical demonstration, not a Marketplace concept, and is removed when a real Lifecycle process lands | Adding it to the glossary would pollute the domain language |
| `/dapr/*` endpoints are not Account-authenticated (Principle IV) | The Dapr sidecar calls them, not an Account | They are still authenticated, with the Dapr app-API-token scheme and a generated secret. There is **no unauthenticated fallback**. If Toolkit 13.0.0 can't give the sidecar the token, implementation stops and the user decides between the 13.6 beta toolkit and configuring `daprd` by hand (T011(b)). |
