# Research: Project Skeleton

**Feature**: [spec.md](./spec.md) | **Plan**: [plan.md](./plan.md) | **Date**: 2026-11-05

Sources: Context7 (Aspire, CommunityToolkit Aspire, Vogen docs), NuGet and npm registries (checked
2026-11-05), and the codebase as it was before the re-init (`git show '51043dfe^:…'`), which is the
repository's existing pattern for most of these decisions.

## R1. Solution layout and DDD enforcement (FR-013, FR-014)

- **Decision**: Use four production projects. `MicroCommerce.Domain` is a class library that
  references only Vogen. `MicroCommerce.ApiService` holds the endpoints, application code and
  infrastructure, organised in feature folders. `MicroCommerce.ServiceDefaults` and
  `MicroCommerce.AppHost` complete the set. The ApiService references the Domain project, but Domain
  references nothing else, so the compiler enforces that the domain stays free of infrastructure.
- **Rationale**: FR-013 must be verifiable. A project boundary is checked by the build, whereas a
  folder convention is not. The old codebase used one ApiService project with feature folders, and
  this plan keeps those feature folders on the ApiService side.
- **Alternatives considered**:
  - A single ApiService project with a `Domain/` folder: rejected because nothing enforces it.
  - Full Clean Architecture (Domain, Application, Infrastructure, Api): rejected as speculative
    layering for a skeleton (Principle VII).

## R2. Typed identifiers with Vogen, without EF Core in the domain

- **Decision**:
  - The Domain project declares IDs as `[ValueObject<Guid>]` structs with
    `Conversions.SystemTextJson | Conversions.TypeConverter`. Both are BCL types, so Domain needs no
    package other than Vogen.
  - The ApiService project declares `[EfCoreConverter<SampleProcessId>]`, and later each new ID,
    on a marker class `VogenEfCoreConverters`, and calls
    `configurationBuilder.RegisterAllInVogenEfCoreConverters()` in `ConfigureConventions`.
  - The OpenAPI schema mapping is a hand-written `VogenIdSchemaTransformer` in the ApiService. It
    maps each registered ID type to `string`/`uuid`.
  - The old codebase's generated `MapVogenTypesIn…` method isn't used. It would require
    `openApiSchemaCustomizations` in Domain's `VogenDefaults`, and that would pull OpenAPI types
    into Domain (analysis finding C1).
- **Rationale**: Vogen's documented approach for Onion-style layering. It keeps the old codebase's
  behaviour, where IDs appear as primitives in JSON, the database and OpenAPI.
- **Alternatives considered**: `Conversions.EfCoreValueConverter` in Domain, as the old
  `VogenDefaults.cs` did. Rejected because it pulls EF Core into Domain.

## R3. Mapping with Mapster (replaces Mapperly; approved 2026-11-04)

- **Decision**:
  - Use `Mapster` 10.0.14 only. `Mapster.DependencyInjection` isn't needed: register a singleton
    `TypeAdapterConfig` by hand.
  - Mapping is outward only, from domain to response DTO. Nothing maps into an aggregate, because
    aggregates change only through their methods (Principle II).
  - Global rules map Vogen IDs to their primitives (`config.NewConfig<SampleProcessId, Guid>()`
    with `MapWith(x => x.Value)`).
  - `RequireExplicitMapping = true` and `RequireDestinationMemberSource = true` are set. A unit test
    calls `config.Compile()`, so a missing mapping fails in CI rather than at runtime.
- **Rationale**: The user chose Mapster. Compile-time validation in a test recovers most of the
  safety Mapperly's source generator gave.
- **Alternatives considered**: Mapster's code generation (`Mapster.Tool`). Rejected for the
  skeleton because it is an extra tool. It can be revisited later if mapping performance matters.

## R4. Local orchestration (FR-001 to FR-005)

- **Decision**:
  - Keep the old AppHost pattern: PostgreSQL, Redis, a Temporal dev-server container
    (`temporalio/temporal`, `server start-dev`) and Keycloak with realm import.
  - Keep the `EphemeralResources` and `FixedPorts` switches. Dev runs use persistent containers and
    volumes. Test runs use throwaway containers and random ports.
  - Add Dapr through `CommunityToolkit.Aspire.Hosting.Dapr`.
  - Add the web app with `AddViteApp(...).WithPnpm()`, with the API reached through the Vite dev
    proxy (R9).
- **Rationale**: The pattern was proven in the old codebase, covering startup ordering, health
  checks and stable issuer ports.
- **Alternatives considered**:
  - A Temporal hosting integration package: rejected as a new dependency when a plain container
    resource works.
  - Docker Compose: rejected because the constitution requires Aspire.

## R5. Durable processes with Temporal (FR-011, FR-012, SC-007)

- **Decision**:
  - Use `Temporalio` and `Temporalio.Extensions.Hosting` 1.21.0, already pinned.
  - A hosted worker runs inside the ApiService.
  - **The workflow owns creating the aggregate** (analysis finding C2). The start endpoint doesn't
    write to the database; it only starts the workflow. This removes the gap where a saved process
    could have no workflow to complete it.
  - `SampleProcessWorkflow` takes the process ID, the delay and `startedAt`, and runs these steps:
    1. Activity `CreateSampleProcess`. It is idempotent: if the row already exists, it does
       nothing.
    2. `Workflow.DelayAsync(delay)`.
    3. Activity `CompleteSampleProcess`. It is idempotent: it loads the aggregate and calls
       `Complete(now)`, which is a no-op if already completed.
    4. Activity `PublishSampleProcessCompleted`. It is at least once, which is safe because
       subscribers de-duplicate.
  - **Trade-off**: right after the `202`, a `GET` can briefly return `404` until step 1 has run,
    usually within milliseconds. The contract documents this, and clients poll.
  - The workflow ID is `sample-process-{id}`, with a reject-duplicate ID reuse policy, so a retried
    start request can't create a second run.
  - Workflow unit tests use `WorkflowEnvironment.StartTimeSkippingAsync()`. That lives in the
    `Temporalio` package (`Temporalio.Testing` namespace), so no extra package is needed.
- **Restart proof (SC-007)**: The AppHost integration test restarts the `apiservice` resource while
  the Temporal container keeps running, using Aspire's resource command `resource-restart`. Then it
  asserts that completion happened exactly once.
  - **Verify in the first task**: confirm that the command is available through
    `Aspire.Hosting.Testing` 13.6.
  - **Fallback**: a FunctionalTests case that disposes and rebuilds the in-process host against the
    same Temporal container.
- **Alternatives considered**: Hangfire or Quartz, rejected because the constitution requires
  Temporal. In-memory timers violate Principle V.

## R6. Asynchronous messaging with Dapr (replaces Wolverine; approved 2026-11-04)

- **Decision**:
  - **Hosting**: `CommunityToolkit.Aspire.Hosting.Dapr`, pinned to **13.0.0**, the latest stable
    release. Its dependency is `Aspire.Hosting >= 13.0.0`, which is compatible with the 13.6 AppHost.
    The 13.6.x versions are beta only.
    - Use the README API that is confirmed in 13.0.0: `AddDaprPubSub("pubsub")` and
      `.WithDaprSidecar(new DaprSidecarOptions { AppId = "apiservice", ... })`, plus
      `.WithReference(pubSub)` on the project.
    - The newer `builder.AddDapr()` and sidecar-scoped `WithReference` shown on aspire.dev aren't
      assumed.
  - **Component**: The pub/sub uses Aspire's own Redis resource, not the `dapr_redis` container
    that `dapr init` creates. Aspire's Redis requires a password, so the component gets `redisHost`
    and `redisPassword` metadata from the Aspire resource.
    - Primary: use `WithMetadata(...)` with the endpoint and the password parameter, if 13.0.0 has
      it.
    - Fallback: a component YAML under `src/MicroCommerce.AppHost/dapr/` loaded through
      `ResourcesPaths`. It reads the password from an environment-variable secret store fed by the
      Aspire parameter.
    - Either way, no password is committed.
  - **Client**: `Dapr.AspNetCore` 1.19.0, which includes `Dapr.Client`.
    - Publishing goes through an `IIntegrationEventPublisher` port with a `DaprIntegrationEventPublisher`
      adapter, so unit and functional tests can substitute it.
  - **Subscriptions**: Use programmatic subscriptions served by a FastEndpoints endpoint at
    `GET /dapr/subscribe` that lists the topic routes.
    - FastEndpoints doesn't use MVC's `[Topic]` attribute, and a declarative Subscription YAML
      would put the route far from its handler.
    - The handler is a FastEndpoints endpoint at `POST /dapr/events/sample-process-completed`.
  - **Securing sidecar-to-app calls (Principle IV)**:
    - The AppHost generates a persisted secret parameter `dapr-app-api-token`. It passes it to the
      app as `APP_API_TOKEN` and to the sidecar's environment.
    - The app authenticates `/dapr/*` endpoints with the `Dapr` authentication scheme from
      `Dapr.AspNetCore` (`AddAuthentication().AddDapr()`), which validates the `dapr-api-token`
      header. These endpoints use `AuthSchemes("Dapr")` and no Account policy.
    - Verify in the first task that the sidecar resource accepts the environment variable. If it
      doesn't, stop and ask the user to choose between the 13.6 beta toolkit and configuring `daprd`
      by hand. An anonymous fallback is not allowed (Principle IV, analysis finding C3).
  - **Idempotency (FR-012b)**: Each subscriber records `(consumer, message_id)` in a
    `processed_events` inbox table that has a unique constraint. It writes the inbox row in the same
    transaction as the handler's effect, so a duplicate delivery becomes a no-op. A thrown exception
    returns a non-2xx status, and Dapr retries the delivery.
  - **Local prerequisite**: the Dapr CLI with `dapr init --slim`. Slim mode installs only the
    `daprd` binary and no containers. Pub/sub needs no placement or scheduler service, which avoids
    Podman friction.
- **Rationale**: These are the user's choices. The port and adapter split keeps the Dapr SDK out
  of the domain and makes it easy to replace in tests.
- **Alternatives considered**:
  - The 13.6 beta of the toolkit: rejected because stable is preferred.
  - Calling Dapr's HTTP API without the SDK: rejected as more code, for no benefit.

## R7. Identity with Keycloak (FR-006 to FR-010)

- **Decision**:
  - Keep the old realm design, with these changes:
    - The web client `micro-commerce-web` is **public** and uses **PKCE S256**, with no client
      secret. Its redirect and post-logout URI is `http://localhost:5173/*` and its web origin is
      `http://localhost:5173`. It has an audience mapper to `micro-commerce-api` and a realm-role
      mapper to the `roles` claim.
    - `micro-commerce-api` is a bearer-only audience.
    - `micro-commerce-tests` is a public client with direct-access grants, used only by AppHost
      integration tests. It exists only in the dev realm.
    - The realm role is `platform-operator`.
    - There are two seeded users, `buyer` and `operator`. The old `merchant` user is dropped
      because Merchants are out of scope.
  - **Seeded passwords are not committed** (Principle IV). The realm file uses Keycloak's
    environment-variable placeholders in import files (`${MC_BUYER_PASSWORD}` and
    `${MC_OPERATOR_PASSWORD}`). The AppHost defines generated, persisted secret parameters
    (`dev-buyer-password` and `dev-operator-password`, stored in user secrets) and passes them to
    Keycloak. Developers read them from the Aspire dashboard's Parameters view, and integration
    tests pass their own values as arguments.
    - **Verify in the first task**: confirm that the pinned Keycloak image substitutes placeholders
      in user credentials during import.
    - **Fallback**: a dev-only `Realms/README` exception, recorded in Complexity Tracking.
  - **API**: Keep the old `AddJwtBearer` settings: `MapInboundClaims = false`, name claim `name`,
    role claim `roles`, and an Authority set by the AppHost.
  - `GET /api/me` reads the Account from claims through an `ICurrentAccount` request-context
    service (ADR-50: commands never carry the Account). Nothing is persisted, as the spec requires.
    The old `CurrentAccount`, which created Accounts in the database, is not carried over.
  - `AccountId` is a Vogen `Guid`, parsed from the Keycloak `sub` claim, which is a UUID. A token
    without a parseable `sub` is rejected with 401.
- **Alternatives considered**:
  - A backend-for-frontend (BFF) that holds tokens in a cookie session: rejected for the skeleton
    because it means more moving parts. Revisit before production; see the risk in plan.md.
  - A confidential client with a committed dev secret, as in the old Next.js app: rejected because
    a SPA can't keep a secret, and committing one breaks the security rule.

## R8. Web app with React and Vite

- **Decision**:
  - Use React 19, Vite 8 and TypeScript in `src/MicroCommerce.Web`, with pnpm as the old codebase
    did (pnpm is installed).
  - For OIDC: `react-oidc-context` 3.3.1 and `oidc-client-ts` 3.5.0 (**pending approval**,
    recommended). User state is stored in `sessionStorage`, the library default, and is never in
    `localStorage`.
  - Don't add a router or a query library. Two screens (home and signed in) and one `/api/me` call
    don't justify them (Principle VII).
- **Alternatives considered**:
  - Hand-rolled PKCE: rejected because of the security risk.
  - `@tanstack/react-query`: rejected as premature.
  - Next.js: rejected because the user chose React with Vite.

## R9. Web app to API connectivity

- **Decision**:
  - The Vite dev server proxies `/api` to the ApiService. `vite.config.ts` reads the URL from the
    service-discovery environment variable that Aspire injects (`services__apiservice__http__0`), so
    the browser stays same-origin and the API needs no CORS policy.
  - FastEndpoints uses route prefix `api` for client-facing endpoints. The `/dapr/*`, `/health` and
    `/alive` endpoints are excluded from the prefix.
  - The OIDC authority (`VITE_OIDC_AUTHORITY`) and client ID (`VITE_OIDC_CLIENT_ID`) are passed by
    the AppHost as `VITE_` environment variables.
- **Alternatives considered**: Direct calls with CORS: rejected as more configuration, with a
  different origin in dev than in production.

## R10. Testing strategy (FR-020 to FR-023, SC-004 to SC-008)

- **Decision**: Use three .NET test projects on xUnit v3 and Microsoft.Testing.Platform. The old
  `global.json` selected the runner. Shared `tests/Directory.Build.props` brings in xUnit v3,
  Shouldly and NSubstitute.

  | Suite | Scope | Infrastructure |
  |-------|-------|----------------|
  | `MicroCommerce.UnitTests` | Domain rules, validators, Mapster config (`Compile()`), endpoint handlers and activities with NSubstitute doubles, workflow logic with Temporal's time-skipping environment | None. Must pass with networking disabled (SC-005). |
  | `MicroCommerce.FunctionalTests` | API in process via `FastEndpoints.Testing`. Auth is faked with `TestIdentity` signed tokens, as in the old codebase. Covers all endpoints' success, validation, 401 and 403 cases (SC-004), the inbox de-duplication by posting the same event twice to the subscriber endpoint, and the workflow running against a real Temporal container | Testcontainers: PostgreSQL and Temporal. The Dapr publisher is replaced with an NSubstitute double. |
  | `MicroCommerce.IntegrationTests` | Whole AppHost via `Aspire.Hosting.Testing` with `--EphemeralResources=true`. Covers the real Keycloak token (password grant through `micro-commerce-tests`) against `/api/me`, the anonymous call refused, the end-to-end sample process with the event received through the real Dapr sidecar, and the restart scenario (SC-007) | Aspire containers, plus the Dapr CLI on the machine |

  - Web app: **Vitest** 5.1.0 with Testing Library (**pending approval**, recommended). The tests
    cover the signed-out and signed-in rendering, with the OIDC context mocked.
  - Browser end-to-end tests (Playwright) are deferred to a later feature. Sign-in through the
    browser is checked manually with the quickstart.
- **Rationale**: This is the old codebase's proven three-tier split, with NSubstitute added as the
  user asked.
- **Alternatives considered**: Running Dapr in FunctionalTests through a Testcontainers `daprd`:
  rejected as complex. The real sidecar is covered once in IntegrationTests.

## R11. Container runtime: Podman on this machine

- **Decision**: Use Podman, documented in the quickstart.
  - **Aspire**: set `ASPIRE_CONTAINER_RUNTIME=podman`. Aspire doesn't see a shell `docker` alias.
  - **Testcontainers**: set `DOCKER_HOST` to the Podman machine socket and
    `TESTCONTAINERS_DOCKER_SOCKET_OVERRIDE=/var/run/docker.sock`.
  - **Dapr**: `dapr init --slim` needs no container runtime.
  - Docker users need none of this.

## R12. Package changes for implementation (not made during planning)

`Directory.Packages.props` changes when the skeleton is implemented. None of these changes is made
in the plan phase.

| Change | Package | Version | Status |
|--------|---------|---------|--------|
| Remove | `Riok.Mapperly` | — | Approved |
| Remove | `WolverineFx`, `WolverineFx.EntityFrameworkCore`, `WolverineFx.Postgresql`, `WolverineFx.RuntimeCompilation` | — | Approved |
| Remove | `Aspire.StackExchange.Redis.DistributedCaching` | — | Recommended. Nothing in the skeleton caches. Redis stays only as the Dapr pub/sub store. |
| Remove | `Testcontainers.Redis` | — | Recommended. Functional tests don't need Redis. |
| Add | `Mapster` | 10.0.14 | Approved |
| Add | `NSubstitute` | 6.2.0 | Approved |
| Add | `Dapr.AspNetCore` | 1.19.0 | Approved (Dapr) |
| Add | `CommunityToolkit.Aspire.Hosting.Dapr` | 13.0.0 | Approved (Dapr) |
| Add (npm) | `react-oidc-context`, `oidc-client-ts` | 3.3.1, 3.5.0 | **Pending approval** |
| Add (npm) | `vitest`, `@testing-library/react`, `jsdom` | latest stable | **Pending approval** |
| Add (npm) | `react`, `react-dom`, `vite`, `@vitejs/plugin-react`, `typescript` | latest stable | Approved (React and Vite) |

## Spike results (T011)

Run on 2026-11-05.

- **(a) Keycloak `${ENV}` placeholders in imported user credentials: confirmed, primary path.**
  A throwaway `quay.io/keycloak/keycloak:26.6` container (the image the Aspire Keycloak integration
  ran on this machine) imported a realm whose user credential was `"${MC_BUYER_PASSWORD}"`, with
  `MC_BUYER_PASSWORD` set on the container. A password grant with the environment value returned
  `200`, and one with the literal placeholder text returned `400`. The realm file commits no passwords.
- **(b) CommunityToolkit.Aspire.Hosting.Dapr 13.0.0: primary path, API surface confirmed; runtime check pending.**
  - `WithMetadata(IResourceBuilder<IDaprComponentResource>, string, string | IValueProvider | ParameterResource)`
    exists, so the `pubsub` component gets `redisHost` and `redisPassword` from the Aspire Redis
    resource. No component YAML is needed.
  - Correction found during T025: `AddDaprPubSub("pubsub")` makes the toolkit write a
    `pubsub.in-memory` component and ignore its metadata (it logs "Using in-memory provider for dapr
    component 'pubsub'"). The AppHost therefore uses `AddDaprComponent("pubsub", "pubsub.redis")`,
    whose generated YAML carries the metadata. A secret parameter or a value provider becomes a
    `secretKeyRef` into the toolkit's `secretstores.local.env` store, fed through the sidecar's
    environment, so the Redis password is never written to a file.
  - `IDaprSidecarResource` is not `IResourceWithEnvironment`, so `WithEnvironment` doesn't compile on
    it. However, the toolkit's lifecycle hook copies every `EnvironmentCallbackAnnotation` from the
    sidecar resource onto the `dapr run` executable it creates
    (`daprCli.Annotations.AddRange(daprSidecar.Annotations.OfType<EnvironmentCallbackAnnotation>())`).
    The AppHost therefore sets `APP_API_TOKEN` for the sidecar with
    `WithDaprSidecar(sidecar => sidecar.WithAnnotation(new EnvironmentCallbackAnnotation(...)))`,
    and for the app with `WithEnvironment`. No beta toolkit, no hand-configured `daprd`, and no
    anonymous `/dapr/*` endpoints.
  - The runtime half (the sidecar really sends `dapr-api-token`) wasn't run, because the Dapr CLI
    isn't installed on this machine yet. `SampleProcessLifecycleTests` covers it once `daprd` is
    available.
- **(c) Restarting a resource from `Aspire.Hosting.Testing` 13.6: primary path, API confirmed.**
  `DistributedApplication.ResourceCommands` exposes `ResourceCommandService.ExecuteCommandAsync(resourceId,
  commandName, ct)`, and `KnownResourceCommands.RestartCommand` names the restart command. The
  restart test calls `app.ResourceCommands.ExecuteCommandAsync("apiservice", KnownResourceCommands.RestartCommand)`.

No fallback was taken, so plan.md Complexity Tracking gets no new rows.
