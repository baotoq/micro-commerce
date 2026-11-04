---
description: "Task list for the MicroCommerce project skeleton"
---

# Tasks: Project Skeleton (Walking Skeleton)

**Input**: Design documents from `specs/001-project-skeleton/`: plan.md, spec.md, research.md,
data-model.md, contracts/http-api.md, contracts/events.md and quickstart.md.

**Tests**: These are included. The spec requires them (User Story 3, FR-020 to FR-023), and
Constitution Principle III is non-negotiable. In each story, write the tests first and confirm they
fail before implementing.

**Organization**: One phase per user story from spec.md:

- US1 (P1): start everything
- US2 (P1): sign in
- US3 (P2): trustworthy tests
- US4 (P3): durable process and event

Spec User Story 5 was removed during clarification.

**Approval gates**: Tasks marked **⚠ APPROVAL** add packages that the user has not approved yet
(research R12). Don't start them until the user approves. Every other package is approved.

**Reference code**: Several tasks say to "restore … from `51043dfe^`". That means read the old file
with `git show '51043dfe^:<path>'` (quote it; zsh mangles `^:`) and adapt it as described. Don't copy
it blindly.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependency on an unfinished task)
- **[Story]**: US1–US4, for user-story phases only

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Create the solution, projects and package pins. Verify the three risky assumptions
before anything depends on them.

- [ ] T001 Create `global.json` at the repo root with SDK `10.0.400`, `rollForward: latestFeature`
  and `"test": { "runner": "Microsoft.Testing.Platform" }`, restoring it from `51043dfe^:global.json`.
- [ ] T002 Update `Directory.Packages.props` as research R12 lists, approved rows only:
  - Remove `Riok.Mapperly`, all four `WolverineFx*` packages,
    `Aspire.StackExchange.Redis.DistributedCaching` and `Testcontainers.Redis`.
  - Add `Mapster` 10.0.14, `NSubstitute` 6.2.0, `Dapr.AspNetCore` 1.19.0 and
    `CommunityToolkit.Aspire.Hosting.Dapr` 13.0.0, each in the matching labelled `ItemGroup`.
  - Keep the `MessagePack` transitive pin.
- [ ] T003 [P] Create `src/MicroCommerce.ServiceDefaults/MicroCommerce.ServiceDefaults.csproj` and
  `src/MicroCommerce.ServiceDefaults/Extensions.cs`, restoring both from `51043dfe^`. That means
  OTel, health checks (`/health` and `/alive`), service discovery and resilience.
- [ ] T004 [P] Create `src/MicroCommerce.Domain/MicroCommerce.Domain.csproj`, a class library whose
  only `PackageReference` is `Vogen`. It has no `FrameworkReference` and no other projects.
  - Add `src/MicroCommerce.Domain/VogenDefaults.cs` with
    `[assembly: VogenDefaults(conversions: Conversions.SystemTextJson | Conversions.TypeConverter)]`
    (research R2: no EF Core conversion here).
- [ ] T005 [P] Create `src/MicroCommerce.ApiService/MicroCommerce.ApiService.csproj` (Web SDK).
  - Project references: ServiceDefaults and Domain.
  - Packages: `Aspire.Npgsql.EntityFrameworkCore.PostgreSQL`, `FastEndpoints`,
    `FastEndpoints.OpenApi`, `Microsoft.AspNetCore.Authentication.JwtBearer`,
    `Microsoft.EntityFrameworkCore.Design` (PrivateAssets all), `Mapster`, `Scalar.AspNetCore`,
    `Temporalio`, `Temporalio.Extensions.Hosting`, `Vogen` and `Dapr.AspNetCore`.
  - Add `Properties/launchSettings.json` with http and https profiles.
- [ ] T006 [P] Create `src/MicroCommerce.AppHost/MicroCommerce.AppHost.csproj` using
  `Aspire.AppHost.Sdk/13.6.0`, restoring it from `51043dfe^`.
  - Packages: `Aspire.Hosting.JavaScript`, `Aspire.Hosting.Keycloak`, `Aspire.Hosting.PostgreSQL`,
    `Aspire.Hosting.Redis` and `CommunityToolkit.Aspire.Hosting.Dapr`.
  - Project reference: ApiService.
  - Generate a new `UserSecretsId` GUID.
  - Add `Properties/launchSettings.json`.
- [ ] T007 [P] Create `tests/Directory.Build.props`, restoring it from `51043dfe^`.
  - It imports the parent props and sets `OutputType Exe` and `IsTestProject`.
  - It references `xunit.v3`, `Shouldly` and `NSubstitute`, with global usings for `Xunit`,
    `Shouldly` and `NSubstitute`.
- [ ] T008 [P] Create the three test projects.
  - `tests/MicroCommerce.UnitTests/MicroCommerce.UnitTests.csproj` references Domain and ApiService.
  - `tests/MicroCommerce.FunctionalTests/MicroCommerce.FunctionalTests.csproj` uses
    `FastEndpoints.Testing` and `Testcontainers.PostgreSql`, references ApiService, and has global
    usings for `FastEndpoints` and `FastEndpoints.Testing`.
  - `tests/MicroCommerce.IntegrationTests/MicroCommerce.IntegrationTests.csproj` uses
    `Aspire.Hosting.Testing`, references AppHost, and has global usings for `Aspire.Hosting`,
    `Aspire.Hosting.ApplicationModel` and `Aspire.Hosting.Testing`.
  - Restore the shapes from `51043dfe^`.
- [ ] T009 Create `MicroCommerce.slnx` at the repo root with every project from T003–T008. Then run
  `dotnet build MicroCommerce.slnx`; it must succeed with zero warnings.
- [ ] T010 [P] Scaffold the web app in `src/MicroCommerce.Web/` with pnpm: React 19, Vite 8 and
  TypeScript, using only `react`, `react-dom`, `vite`, `@vitejs/plugin-react` and `typescript`.
  - Add `package.json` scripts `dev`, `build`, `preview` and `lint` (`tsc --noEmit`).
  - Remove the template's demo assets.
  - Make sure `.gitignore` covers `node_modules/` and `dist/`.
- [ ] T011 Spike the three items from plan.md "Items to verify" in a throwaway AppHost on a
  scratch branch.
  - (a) The `quay.io/keycloak/keycloak` image used by `Aspire.Hosting.Keycloak` 13.6 substitutes
    `${MC_BUYER_PASSWORD}` in an imported user credential.
  - (b) In CommunityToolkit Dapr 13.0.0, check whether `AddDaprPubSub(...).WithMetadata(...)`
    exists, and whether `APP_API_TOKEN` can be set for both the app and the sidecar.
    - Try `DaprSidecarOptions`, or `WithEnvironment` on the sidecar resource.
    - If neither works, **stop and ask the user** whether to pin the 13.6 beta toolkit or
      configure `daprd` by hand. Never fall back to anonymous `/dapr/*` endpoints
      (constitution Principle IV, analysis finding C3).
  - (c) `Aspire.Hosting.Testing` 13.6 can run the `resource-restart` command on a project resource.
  - Record each result and the chosen primary or fallback path under a new
    "Spike results (T011)" heading in `specs/001-project-skeleton/research.md`.
  - Add a row to the plan.md Complexity Tracking table for each fallback taken.

**Checkpoint**: The solution builds, and the spike has decided the paths for T024, T025 and T060.

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: The domain building blocks, the API host and the test fixtures that every story uses.

**⚠️ CRITICAL**: Don't start any user-story phase until this phase is complete.

- [ ] T012 [P] Create `src/MicroCommerce.Domain/SharedKernel/AggregateRoot.cs`:
  - an abstract `AggregateRoot<TId>` with a `TId Id` property that is init-only;
  - a private `List<IDomainEvent>`, exposed as `IReadOnlyList<IDomainEvent> DomainEvents`;
  - a protected `RaiseDomainEvent(IDomainEvent e)` and a public `ClearDomainEvents()`.
- [ ] T013 [P] Create `src/MicroCommerce.Domain/SharedKernel/IDomainEvent.cs`, with the property
  `DateTimeOffset OccurredAt { get; }`, and
  `src/MicroCommerce.Domain/SharedKernel/DomainRuleViolation.cs`, a `sealed` exception with a
  domain-worded message.
- [ ] T014 [P] Create `src/MicroCommerce.Domain/SharedKernel/AccountId.cs` as a
  `[ValueObject<Guid>] public readonly partial struct AccountId`.
- [ ] T015 Create `src/MicroCommerce.ApiService/Infrastructure/Data/AppDbContext.cs`:
  - an `AppDbContext : DbContext` that applies configurations from its assembly;
  - `ConfigureConventions` calls `configurationBuilder.RegisterAllInVogenEfCoreConverters()`.
  - Also create `Infrastructure/Data/VogenEfCoreConverters.cs`, an `internal sealed partial class`
    marker with no attributes yet. US4 adds them.
  - Also create `Infrastructure/Data/AppDbContextFactory.cs`, an `IDesignTimeDbContextFactory` for
    `dotnet ef`, restored in shape from `51043dfe^:…/Accounts/Data/AccountsDbContextFactory.cs`.
- [ ] T016 [P] Create `src/MicroCommerce.ApiService/Infrastructure/Mapping/MappingConfig.cs`:
  - a static `Create()` that returns a `TypeAdapterConfig` with
    `RequireExplicitMapping = true` and `RequireDestinationMemberSource = true`;
  - a global `NewConfig<AccountId, Guid>().MapWith(x => x.Value)`;
  - a hook where later features add their `Register(config)` calls.
  - Mapping goes outward only, from domain to response (research R3).
- [ ] T017 [P] Create `src/MicroCommerce.ApiService/Infrastructure/DomainRuleViolationHandler.cs`.
  It is an `IExceptionHandler` that turns a `DomainRuleViolation` into a ProblemDetails `422`,
  with `detail` set to the exception message. Other exceptions fall through to the default `500`
  ProblemDetails, without a stack trace (FR-016).
- [ ] T018 Create `src/MicroCommerce.ApiService/Program.cs` with these steps, in order:
  - `AddServiceDefaults()`.
  - `AddNpgsqlDbContext<AppDbContext>("micro-commerce-db")`.
  - `AddProblemDetails()` and `AddExceptionHandler<DomainRuleViolationHandler>()`.
  - Register a singleton `TypeAdapterConfig` from `MappingConfig.Create()`.
  - Register `TimeProvider.System`.
  - Configure JwtBearer exactly as in `51043dfe^:src/MicroCommerce.ApiService/Program.cs`:
    `MapInboundClaims=false`, `NameClaimType "name"`, `RoleClaimType "roles"`, with the Authority
    bound from `Authentication:Schemes:Bearer`.
  - `AddFastEndpoints()` with
    `.OpenApiDocument(o => { o.DocumentName = "v1"; o.ConfigureOpenApi = x => x.AddSchemaTransformer<VogenIdSchemaTransformer>(); })`.
    Don't use Vogen's generated `MapVogenTypesIn…` method: generating it would make the Domain
    project depend on OpenAPI types (research R2).
  - Create `src/MicroCommerce.ApiService/Infrastructure/OpenApi/VogenIdSchemaTransformer.cs`, an
    `IOpenApiSchemaTransformer`. It rewrites the schema of every type in a static
    `IdTypes` set (start with `AccountId`) to `type: string, format: uuid`. Every new ID type is
    added to this set, next to its `[EfCoreConverter<T>]` line.
  - Build the app, then add `UseExceptionHandler()`.
  - In Development, apply `MigrateAsync()`.
  - `UseAuthentication()` and `UseAuthorization()`.
  - `UseFastEndpoints(c => { c.Endpoints.RoutePrefix = "api"; c.Errors.UseProblemDetails(); })`.
  - In Development, `MapOpenApi()` and `MapScalarApiReference(o => o.AddDocument("v1"))`.
  - `MapDefaultEndpoints()`.
  - Ending with `public partial class Program;` is optional, because FastEndpoints.Testing uses
    `AppFixture<Program>`.
- [ ] T019 [P] Create `tests/MicroCommerce.FunctionalTests/TestIdentity.cs`, restoring `TestActor`
  and `TestIdentity` from `51043dfe^`, with these changes:
  - The `Subject` must be a `Guid.NewGuid()` string.
  - Drop Merchant actors.
  - The role claim value is `"platform-operator"`.
  - The audience is `"micro-commerce-api"`.
- [ ] T020 Create `tests/MicroCommerce.FunctionalTests/ApiFixture.cs`, an `AppFixture<Program>`,
  restored from `51043dfe^` and reduced:
  - Use one `PostgreSqlBuilder("postgres:17-alpine")` container.
  - Set the connection string through `ConnectionStrings:micro-commerce-db`.
  - Keep the JwtBearer `PostConfigure` override that trusts `TestIdentity.SigningKey`.
  - Migrate `AppDbContext` under a `SemaphoreSlim`.
  - Keep `ClientFor(TestActor)` and `ClientFor(string token)`.
  - Use the Development environment so OpenAPI is served.
  - No Redis. US4 adds Temporal.
- [ ] T021 Create `tests/MicroCommerce.IntegrationTests/AppHostFixture.cs` and
  `tests/MicroCommerce.IntegrationTests/AssemblyInfo.cs` (with
  `[assembly: AssemblyFixture(typeof(AppHostFixture))]`), restored from `51043dfe^`.
  - Start the AppHost with the args `--EphemeralResources=true`,
    `--Parameters:dev-buyer-password=<random>`, `--Parameters:dev-operator-password=<random>` and
    `--Parameters:dapr-app-api-token=<random>`.
  - Expose the passwords as properties.
  - Wait until `apiservice`, `keycloak` and `temporal` are healthy.
  - Expose `ApiClient` and `KeycloakClient`.
  - Restore the working directory on dispose.

**Checkpoint**: The solution builds, and the API host starts in process from `ApiFixture`.

---

## Phase 3: User Story 1: Start the whole system with one command (Priority: P1) 🎯 MVP

**Goal**: `aspire run` starts PostgreSQL, Redis, Temporal, Keycloak (with the realm seeded), the
ApiService and its Dapr sidecar, and the web app. Everything reports healthy, and the web app's home
page shows that it can reach the API (FR-001 to FR-005).

**Independent Test**: Quickstart Scenario 1. Automated by `StartupTests`.

### Tests for User Story 1 (write first, confirm they fail)

- [ ] T022 [P] [US1] Create `tests/MicroCommerce.IntegrationTests/StartupTests.cs`. Using
  `AppHostFixture`, assert that each of the following reaches **Healthy**, or **Running** where a
  resource has no health check: `postgres`, `micro-commerce-db`, `redis`, `temporal`, `keycloak`,
  `apiservice` and `web`. Also assert that `GET /health` on `apiservice` returns 200, and that
  `GET /` on `web` returns 200 HTML.
- [ ] T023 [P] [US1] Create `tests/MicroCommerce.FunctionalTests/InfrastructureEndpointsTests.cs`.
  Assert that `GET /health` and `GET /alive` return 200 without a token, that `GET /openapi/v1.json`
  returns 200 with a valid OpenAPI document, and that its `paths` contains no `/dapr/` entry
  (FR-017). After US2 lands, also assert that `MeResponse.accountId` has the schema
  `type: string, format: uuid`, which proves `VogenIdSchemaTransformer` works.

### Implementation for User Story 1

- [ ] T024 [US1] Create `src/MicroCommerce.AppHost/Realms/micro-commerce-realm.json` (research R7):
  - Realm: `micro-commerce`, with realm role `platform-operator`.
  - Client `micro-commerce-api`: bearer-only. Set `publicClient false`, `standardFlowEnabled false`
    and `directAccessGrantsEnabled false`.
  - Client `micro-commerce-web`:
    - `publicClient true`, `standardFlowEnabled true` and `directAccessGrantsEnabled false`;
    - attribute `pkce.code.challenge.method: S256`;
    - `redirectUris ["http://localhost:5173/*"]`, `webOrigins ["http://localhost:5173"]` and
      `post.logout.redirect.uris "http://localhost:5173/*"`;
    - protocol mappers `api-audience` (`included.client.audience: micro-commerce-api`) and
      `realm-roles` (`claim.name: roles`, multivalued, access token only).
  - Client `micro-commerce-tests`: `publicClient true`, `directAccessGrantsEnabled true`, with the
    same two mappers.
  - User `buyer`: `buyer@micro-commerce.test`, Bea Buyer, password `${MC_BUYER_PASSWORD}`.
  - User `operator`: `operator@micro-commerce.test`, Olive Operator, password
    `${MC_OPERATOR_PASSWORD}`, with realm role `platform-operator`.
  - Commit no literal passwords. If T011(a) chose the fallback, follow that recorded path instead.
- [ ] T025 [US1] Create `src/MicroCommerce.AppHost/AppHost.cs`, restored from
  `51043dfe^:src/MicroCommerce.AppHost/AppHost.cs`, with these changes:
  - Keep the `EphemeralResources` and `FixedPorts` switches, and the persistent volumes for dev runs.
  - Resources: `postgres` with `.AddDatabase("micro-commerce-db")`, `redis`, the `temporal`
    container (`temporalio/temporal`, `server start-dev --ip 0.0.0.0`, grpc endpoint 7233, plus
    the Temporal UI port 8233 as http endpoint `ui`), and `keycloak` (port 8080 when fixed) with
    `.WithRealmImport("./Realms")`.
  - Add secret parameters `dev-buyer-password`, `dev-operator-password` and `dapr-app-api-token`,
    each with `GenerateParameterDefault { MinLength = 24, Special = false }`, `secret: true` and
    `persist: true`.
  - Pass `MC_BUYER_PASSWORD` and `MC_OPERATOR_PASSWORD` to keycloak with `WithEnvironment`.
  - Add `AddDaprPubSub("pubsub")` with Redis host and password metadata, or the T011(b) fallback
    YAML in `src/MicroCommerce.AppHost/dapr/pubsub.yaml`.
  - `apiservice`:
    - `.WithDaprSidecar(new DaprSidecarOptions { AppId = "apiservice" })`, plus
      `.WithReference(pubsub)` and `APP_API_TOKEN` (per T011(b));
    - `.WithReference(db).WaitFor(db)` and `.WaitFor(redis)`;
    - environment `Temporal__Address`, from the temporal grpc HostAndPort, and
      `Temporal__Namespace=default`, with `.WaitFor(temporal)`;
    - environment `Authentication__Schemes__Bearer__Authority` set to `{keycloak http}/realms/micro-commerce`,
      with `.WaitFor(keycloak)`;
    - `.WithHttpHealthCheck("/health", endpointName: "http")` and `.WithExternalHttpEndpoints()`.
  - `web`:
    - `AddViteApp("web", "../MicroCommerce.Web").WithPnpm()` with `.WithReference(api)`;
    - environment `VITE_OIDC_AUTHORITY` (the issuer) and `VITE_OIDC_CLIENT_ID=micro-commerce-web`;
    - `.WaitFor(api)` and `.WaitFor(keycloak)`;
    - port 5173 when `fixedPorts`, and `.WithExternalHttpEndpoints()`.
  - Drop all of the old `Ordering__*` settings and Next.js/Auth.js environment variables.
- [ ] T026 [P] [US1] Create `src/MicroCommerce.Web/vite.config.ts`.
  - Read the API URL from `process.env.services__apiservice__https__0`, falling back to
    `process.env.services__apiservice__http__0`.
  - Proxy `/api` and `/health` to it, with `changeOrigin: true`.
  - Set `server.port` from `process.env.PORT`, and `strictPort: true`.
- [ ] T027 [US1] Create `src/MicroCommerce.Web/src/App.tsx` and `src/MicroCommerce.Web/src/main.tsx`.
  The home page shows "MicroCommerce" and an API status line: `fetch('/health')` shows
  "API: reachable" on 200 and "API: unreachable" otherwise. There are no auth dependencies yet.
- [ ] T028 [US1] Create `README.md` (FR-005) with these sections:
  - The prerequisites table from quickstart.md, including the Dapr CLI, `dapr init --slim` and
    the Podman environment variables from research R11.
  - Run: `aspire run`, and where to find the dashboard, Scalar and the dev passwords (dashboard,
    then **Parameters**).
  - Layout: the source tree from plan.md.
  - Migrations: `dotnet ef migrations add <Name> -p src/MicroCommerce.ApiService -o Infrastructure/Data/Migrations`.
  - Leave the Tests section as a placeholder; T047 fills it in.
- [ ] T029 [US1] Run `aspire run` and complete quickstart Scenario 1. Then run T022 and T023; they
  must pass.

**Checkpoint**: The MVP. The whole system starts with one command and reports healthy.

---

## Phase 4: User Story 2: Sign in as an Account end to end (Priority: P1)

**Goal**: An Account signs in through Keycloak using PKCE. The web app shows that Account as
returned by `GET /api/me`. Anonymous, expired and tampered tokens are refused (FR-006 to FR-010).

**Independent Test**: Quickstart Scenario 2. Automated by `MeEndpointTests` and `MeApiTests`.

**Depends on**: US1, for the Keycloak realm and AppHost wiring.

### Tests for User Story 2 (write first, confirm they fail)

- [ ] T030 [P] [US2] Create `tests/MicroCommerce.UnitTests/Features/Accounts/HttpCurrentAccountTests.cs`.
  Build a `ClaimsPrincipal` on a `DefaultHttpContext` and assert these mappings:
  - `sub` maps to `AccountId`.
  - `name` maps to `DisplayName`, falling back to `preferred_username`.
  - `email` maps to `Email`.
  - `roles` containing `platform-operator` sets `IsPlatformOperator = true`, and false otherwise.
  - A missing or non-UUID `sub` makes `TryGet` return false.
- [ ] T031 [P] [US2] Create `tests/MicroCommerce.FunctionalTests/Features/Accounts/MeEndpointTests.cs`
  with these cases:
  - Buyer: 200, with `accountId` equal to the subject, plus `displayName`, `email` and
    `isPlatformOperator=false`.
  - Operator: `isPlatformOperator=true`.
  - 401 with no token, an expired token, a token signed with `TestIdentity.NewKey()`, a token with
    audience `other`, and a token with a non-UUID `sub`.
- [ ] T032 [P] [US2] Create `tests/MicroCommerce.IntegrationTests/MeApiTests.cs`.
  - Get a real token from Keycloak with the password grant: `POST
    /realms/micro-commerce/protocol/openid-connect/token`, client `micro-commerce-tests`, user
    `buyer`, password from `AppHostFixture`.
  - Then `GET /api/me` must return 200 with `displayName "Bea Buyer"`.
  - Repeat for `operator`, expecting `isPlatformOperator=true`.
  - An anonymous `GET /api/me` must return 401.

### Implementation for User Story 2

- [ ] T033 [P] [US2] Create `src/MicroCommerce.ApiService/Features/Accounts/AuthPolicies.cs`, with
  the constants `PlatformOperator = "PlatformOperator"` and `PlatformOperatorRole = "platform-operator"`.
  Register the policy in `Program.cs` with
  `AddAuthorizationBuilder().AddPolicy(PlatformOperator, p => p.RequireRole(PlatformOperatorRole))`.
- [ ] T034 [P] [US2] Create `src/MicroCommerce.ApiService/Features/Accounts/CurrentAccount.cs`:
  - a record `CurrentAccount(AccountId Id, string DisplayName, string? Email, bool IsPlatformOperator)`;
  - an interface `ICurrentAccount { bool TryGet(out CurrentAccount account); }`;
  - `HttpCurrentAccount`, which implements it from `IHttpContextAccessor`, following the claim rules
    in data-model.md.
  - Register it as scoped in `Program.cs`, together with `AddHttpContextAccessor()`.
  - It persists nothing; don't port the old database-backed `CurrentAccount` (research R7).
- [ ] T035 [US2] Create `src/MicroCommerce.ApiService/Features/Accounts/GetMe.cs`:
  - `MeResponse(Guid AccountId, string DisplayName, string? Email, bool IsPlatformOperator)`;
  - `GetMeEndpoint : EndpointWithoutRequest<MeResponse>` at `Get("/me")`, authenticated by default;
  - `HandleAsync` sends `401` if `ICurrentAccount.TryGet` fails, and otherwise maps the account
    with the injected `TypeAdapterConfig` (`account.Adapt<MeResponse>(config)`).
  - Register `CurrentAccount → MeResponse` in `MappingConfig`, with
    `Map(d => d.AccountId, s => s.Id.Value)`.
  - Add a `Summary` with a 200 and 401 description.
- [ ] T036 [US2] **⚠ APPROVAL** (`react-oidc-context`, `oidc-client-ts`). Run
  `pnpm --dir src/MicroCommerce.Web add react-oidc-context@3.3.1 oidc-client-ts@3.5.0`.
- [ ] T037 [US2] Update `src/MicroCommerce.Web/src/main.tsx` to wrap `<App/>` in `<AuthProvider>`
  (depends on T036). Configure it with:
  - `authority` from `import.meta.env.VITE_OIDC_AUTHORITY` and `client_id` from
    `VITE_OIDC_CLIENT_ID`;
  - `redirect_uri` and `post_logout_redirect_uri` set to `window.location.origin`;
  - `scope "openid profile email"`;
  - `userStore` with `new WebStorageStateStore({ store: window.sessionStorage })`;
  - `onSigninCallback`, which removes the `code` and `state` query parameters with
    `history.replaceState`.
- [ ] T038 [US2] Update `src/MicroCommerce.Web/src/App.tsx` and create `src/MicroCommerce.Web/src/api.ts`.
  - `api.ts` provides `getMe(token)`, which calls `fetch('/api/me', { headers: { Authorization: Bearer … } })`
    and returns a typed `Me`.
  - When signed out, the page shows a **Sign in** button (`signinRedirect`).
  - When signed in, it shows the display name, `accountId`, `Platform Operator: yes/no` and a
    **Sign out** button (`signoutRedirect`).
  - It handles `isLoading` and `error` states. It shows a retry message while Keycloak is still
    starting, which is the spec's first edge case.
  - Keep the API status line from T027.
- [ ] T039 [US2] Complete quickstart Scenario 2 by hand, then run T030–T032; they must pass.

**Checkpoint**: US1 and US2 deliver a running system with working sign-in.

---

## Phase 5: User Story 3: Trustworthy automated tests from day one (Priority: P2)

**Goal**: One command runs every suite. Unit tests need no infrastructure. The conventions are
enforced by tests, not just by documentation (FR-013, FR-020 to FR-023, SC-005, SC-006, SC-008).

**Independent Test**: Quickstart Scenario 4.

### Tests and harness for User Story 3

- [ ] T040 [P] [US3] Create `tests/MicroCommerce.UnitTests/Architecture/DomainDependencyTests.cs`.
  Assert that no entry in `typeof(AggregateRoot<>).Assembly.GetReferencedAssemblies()` has a name
  starting with `Microsoft.EntityFrameworkCore`, `Microsoft.AspNetCore`, `FastEndpoints`,
  `Temporalio`, `Dapr`, `Mapster` or `Npgsql`. Only BCL (`System.*`) and `Vogen*` references are
  expected. This enforces FR-013.
- [ ] T041 [P] [US3] Create `tests/MicroCommerce.UnitTests/Infrastructure/MappingConfigTests.cs`.
  It asserts that `MappingConfig.Create().Compile()` doesn't throw (research R3), which also covers
  every mapping added later.
- [ ] T042 [P] [US3] Create `tests/MicroCommerce.UnitTests/Infrastructure/DomainRuleViolationHandlerTests.cs`.
  A `DomainRuleViolation` must produce a 422 ProblemDetails whose `detail` is the message. Any other
  exception must not be handled (`TryHandleAsync` returns false). Use an NSubstitute
  `IProblemDetailsService`.
- [ ] T043 [US3] Confirm that `tests/MicroCommerce.UnitTests/MicroCommerce.UnitTests.csproj` has no
  Testcontainers, Aspire.Hosting or FastEndpoints.Testing reference. Then run
  `dotnet test --project tests/MicroCommerce.UnitTests` with networking off, for example Wi-Fi
  disabled. It must pass in under 30 seconds (SC-005). Record the time in the PR description.
- [ ] T044 [US3] **⚠ APPROVAL** (`vitest`, `@testing-library/react`, `jsdom`). Run
  `pnpm --dir src/MicroCommerce.Web add -D vitest @testing-library/react jsdom`, add the `test`
  script `vitest run`, and configure `test.environment = 'jsdom'` in
  `src/MicroCommerce.Web/vite.config.ts`.
- [ ] T045 [P] [US3] Create `src/MicroCommerce.Web/src/App.test.tsx` (depends on T044 and T038).
  Mock `react-oidc-context`'s `useAuth` and `fetch`, then assert three states:
  - signed out: the **Sign in** button is visible;
  - signed in: the display name and `Platform Operator: no` come from the mocked `/api/me`;
  - API unreachable: "API: unreachable" is shown.
- [ ] T046 [US3] Run `dotnet test --solution MicroCommerce.slnx`. It must pass in under 5 minutes
  (SC-006), and afterwards `docker ps` or `podman ps` must show no containers left from the run.
  Then run the 10× loop from quickstart Scenario 4 (SC-008). Fix or remove any flaky test (Principle III).
- [ ] T047 [US3] Fill in the `README.md` **Tests** section:
  - the three suites and what each needs, using the table from research R10;
  - the commands `dotnet test --solution MicroCommerce.slnx`,
    `dotnet test --project tests/MicroCommerce.UnitTests` and
    `pnpm --dir src/MicroCommerce.Web test`;
  - a note that integration tests need the Dapr CLI and a container runtime, and use throwaway
    containers.

**Checkpoint**: The test conventions are in place and enforced.

---

## Phase 6: User Story 4: Durable background process survives restarts (Priority: P3)

**Goal**: A Platform Operator starts a `SampleProcess`. A Temporal workflow completes it after the
delay, exactly once, even across an API restart. A `sample-process-completed` event goes through
Dapr, and an idempotent inbox records it (FR-011, FR-012, FR-012a, FR-012b, SC-007).

**Independent Test**: Quickstart Scenario 3. Automated by `SampleProcessLifecycleTests`.

**Depends on**: US1, for Temporal and the Dapr sidecar, and US2, for the Platform Operator policy
and tokens.

### Tests for User Story 4 (write first, confirm they fail)

- [ ] T048 [P] [US4] Create `tests/MicroCommerce.UnitTests/Domain/SampleProcessTests.cs` with these
  cases:
  - `Start` accepts 1 s and 300 s.
  - `Start` rejects 0 s and 301 s with `DomainRuleViolation("A sample process must wait between 1 and 300 seconds")`.
  - `Complete(now)` before `StartedAt + Delay` throws.
  - `Complete` at or after the due time sets `Status = Completed` and `CompletedAt = now`, and
    raises exactly one `SampleProcessCompleted`.
  - A second `Complete(later)` is a no-op: `CompletedAt` is unchanged and no second event is raised.
- [ ] T049 [P] [US4] Create
  `tests/MicroCommerce.UnitTests/Features/Diagnostics/StartSampleProcessValidatorTests.cs`.
  `delaySeconds` values 1 and 300 are valid, while 0, 301 and a missing value are invalid, with the
  error on `delaySeconds`.
- [ ] T050 [P] [US4] Create
  `tests/MicroCommerce.UnitTests/Features/Diagnostics/SampleProcessWorkflowTests.cs`.
  - Use `await WorkflowEnvironment.StartTimeSkippingAsync()` with a `TemporalWorker` that runs the
    real `SampleProcessWorkflow`.
  - Back the activities with an NSubstitute double of `ISampleProcessActivities`, or with stub
    activity methods.
  - Assert this order: `CreateSampleProcess` runs once, before the timer. `CompleteSampleProcess`
    runs once, only after the delay. `PublishSampleProcessCompleted` runs once, last.
- [ ] T051 [P] [US4] Create
  `tests/MicroCommerce.UnitTests/Features/Diagnostics/SampleProcessActivitiesTests.cs`. Use an
  NSubstitute `IIntegrationEventPublisher` and assert that `PublishSampleProcessCompleted` publishes
  `SampleProcessCompletedV1` with `EventId == $"sample-process-completed:{id}"` to topic
  `sample-process-completed`.
- [ ] T052 [P] [US4] Create
  `tests/MicroCommerce.FunctionalTests/Features/Diagnostics/SampleProcessEndpointsTests.cs` with
  these cases:
  - Operator `POST /api/diagnostics/sample-processes {delaySeconds:1}` returns 202, with a
    `Location` header and `status "Running"`.
  - Polling `GET` until it shows `status "Completed"` takes at most 15 s, and `completedAt` is at
    least 1 s after `startedAt`. Treat a `404` right after the `202` as "not yet created" and keep
    polling.
  - An immediate second activity run doesn't create a duplicate row: calling `CreateSampleProcess`
    twice with the same ID leaves one row.
  - `delaySeconds` values 0 and 301 return 400 with `errors.delaySeconds`.
  - A request without a token returns 401, and a buyer token returns 403, for both endpoints
    (SC-004).
  - An unknown ID returns 404, and a non-UUID ID returns 400.
- [ ] T053 [P] [US4] Create
  `tests/MicroCommerce.FunctionalTests/Features/Diagnostics/SampleProcessCompletedSubscriberTests.cs`
  with these cases:
  - Posting the same CloudEvent twice to `/dapr/events/sample-process-completed`, with a
    `dapr-api-token` header, gives 200 both times and exactly one `processed_events` row. Then
    `GET` shows `completionEventReceivedAt` set.
  - A request without the header, or with a wrong token, returns 401.
  - `GET /dapr/subscribe` with the token returns the subscription in contracts/http-api.md.
- [ ] T054 [P] [US4] Create `tests/MicroCommerce.IntegrationTests/SampleProcessLifecycleTests.cs`.
  - Get an operator token from Keycloak, as in T032.
  - Happy path: `delaySeconds 3`. Poll for up to 30 s until `completionEventReceivedAt` is set,
    which proves the real Dapr round trip (FR-012a).
  - Restart (SC-007): mark this test `[Trait("Category", "Restart")]`. It runs once in the default
    suite, and T073 repeats it 10 times. With `delaySeconds 10`, restart `apiservice` 2 s later
    using the T011(c) method. Wait until it is healthy, then assert that the status is `Completed` with one
    `completedAt`, `completionEventReceivedAt` is set, and the Temporal workflow history for
    `sample-process-{id}` has exactly one `CompleteSampleProcess` completion. Use a
    `TemporalClient` connected to the `temporal` resource's endpoint.

### Implementation for User Story 4

- [ ] T055 [P] [US4] Create `src/MicroCommerce.Domain/Diagnostics/SampleProcessId.cs`, a
  `[ValueObject<Guid>] readonly partial struct` with `static New() => From(Guid.CreateVersion7())`.
  Also create `SampleProcessStatus.cs`, an enum with `Running` and `Completed`.
- [ ] T056 [P] [US4] Create `src/MicroCommerce.Domain/Diagnostics/SampleProcessCompleted.cs`, a
  `sealed record SampleProcessCompleted(SampleProcessId Id, DateTimeOffset OccurredAt) : IDomainEvent`.
- [ ] T057 [US4] Create `src/MicroCommerce.Domain/Diagnostics/SampleProcess.cs`, an
  `AggregateRoot<SampleProcessId>` with private setters and a private parameterless constructor
  for EF.
  - `static Start(SampleProcessId id, TimeSpan delay, DateTimeOffset now)`: "`Delay` must be from 1
    second to 300 seconds inclusive", otherwise
    `DomainRuleViolation("A sample process must wait between 1 and 300 seconds")`.
    It sets `Status Running` and `StartedAt now`.
  - `Complete(DateTimeOffset now)`: if already `Completed`, it does nothing. If
    `now < StartedAt + Delay`, it throws a `DomainRuleViolation`. Otherwise it sets `Completed` and
    `CompletedAt = now`, then calls `RaiseDomainEvent(new SampleProcessCompleted(Id, now))`.
  - Follow data-model.md.
- [ ] T058 [US4] Persistence for `SampleProcess` and the inbox:
  - Add `[EfCoreConverter<SampleProcessId>]` to `Infrastructure/Data/VogenEfCoreConverters.cs`, and
    add `SampleProcessId` to `VogenIdSchemaTransformer.IdTypes`.
  - Create `Features/Diagnostics/Data/SampleProcessConfiguration.cs`. It maps table
    `sample_processes`: `id` uuid PK, `delay` interval, `status` text (string conversion),
    `started_at` timestamptz and `completed_at` nullable timestamptz. It ignores `DomainEvents`.
  - Create `Infrastructure/Messaging/ProcessedEvent.cs` and its configuration. It maps table
    `processed_events`: `consumer` text, `message_id` text, `processed_at` timestamptz, with
    PK (`consumer`, `message_id`).
  - Add `DbSet`s to `AppDbContext`.
  - Use snake_case column names, through explicit `HasColumnName` or a naming convention already in
    the repo. Don't add a new package.
- [ ] T059 [US4] Run
  `dotnet ef migrations add InitialCreate -p src/MicroCommerce.ApiService -o Infrastructure/Data/Migrations`.
  Review the generated SQL so that it matches data-model.md.
- [ ] T060 [US4] Set up messaging in `src/MicroCommerce.ApiService/Infrastructure/Messaging/`:
  - `IIntegrationEventPublisher.cs`, with `Task PublishAsync<T>(string topic, T @event, CancellationToken ct)`.
  - `DaprIntegrationEventPublisher.cs`, which uses `DaprClient.PublishEventAsync("pubsub", topic, @event, ct)`.
  - `Inbox.cs`: `Task<bool> TryRecordAsync(string consumer, string messageId, DateTimeOffset now, CancellationToken ct)`.
    It adds a `ProcessedEvent`, catches the Postgres unique violation (`23505`) and returns false
    for duplicates.
  - `DaprSubscribeEndpoint.cs`: `GET /dapr/subscribe`, with `AuthSchemes("Dapr")`, excluded from
    the route prefix and from OpenAPI (`Options(x => x.ExcludeFromDescription())`, and an
    override of the prefix per the FastEndpoints docs). It returns the list from
    contracts/http-api.md.
  - In `Program.cs`, register `AddDaprClient()`, `AddAuthentication().AddDapr()` (per T011(b)),
    the publisher as a singleton and `Inbox` as scoped.
- [ ] T061 [US4] Create `src/MicroCommerce.ApiService/Features/Diagnostics/SampleProcessCompletedV1.cs`.
  It is a record with `EventId`, `SampleProcessId` (Guid) and `CompletedAt`, plus the constant
  `Topic = "sample-process-completed"`, following contracts/events.md.
- [ ] T062 [US4] Create `src/MicroCommerce.ApiService/Features/Diagnostics/SampleProcessActivities.cs`,
  with `[Activity]` methods on a scoped class that takes `AppDbContext`, `IIntegrationEventPublisher`
  and `TimeProvider`.
  - `CreateSampleProcess(Guid id, TimeSpan delay, DateTimeOffset startedAt)` returns early if a row
    with that ID exists. Otherwise it calls `SampleProcess.Start(...)` and saves. If a concurrent
    retry inserts first, it catches the unique violation (`23505`) and treats it as success.
  - `CompleteSampleProcess(Guid id)` loads the aggregate, calls `Complete(timeProvider.GetUtcNow())`,
    saves and returns `CompletedAt`. It is idempotent, because the aggregate no-ops when already
    completed.
  - `PublishSampleProcessCompleted(Guid id, DateTimeOffset completedAt)` publishes `SampleProcessCompletedV1`
    with `EventId = $"sample-process-completed:{id}"`.
  - Log only the ID, using structured templates (FR-018).
- [ ] T063 [US4] Create `src/MicroCommerce.ApiService/Features/Diagnostics/SampleProcessWorkflow.cs`.
  - It has `[Workflow]` and `[WorkflowRun] RunAsync(Guid id, TimeSpan delay, DateTimeOffset startedAt)`.
  - It runs the `CreateSampleProcess` activity, then `await Workflow.DelayAsync(delay)`, then the
    `CompleteSampleProcess` activity, then the `PublishSampleProcessCompleted` activity.
  - All three activities use `StartToCloseTimeout` 30 s with the default retry policy.
  - Add a constant `TaskQueue = "diagnostics"` and the helper `WorkflowId(id) => $"sample-process-{id}"`.
  - In `Program.cs`, register `AddTemporalClient(...)`, reading `Temporal:Address` and
    `Temporal:Namespace` as in `51043dfe^`, and
    `AddHostedTemporalWorker(TaskQueue).AddScopedActivities<SampleProcessActivities>().AddWorkflow<SampleProcessWorkflow>()`.
- [ ] T064 [US4] Create `src/MicroCommerce.ApiService/Features/Diagnostics/StartSampleProcess.cs`.
  - `StartSampleProcessRequest(int? DelaySeconds)`.
  - A `Validator<StartSampleProcessRequest>`: "`delaySeconds` is required and must be an integer from
    1 to 300".
  - The endpoint is `Post("/diagnostics/sample-processes")` with `Policies(AuthPolicies.PlatformOperator)`.
  - In `HandleAsync`:
    - Don't write to the database here; the workflow creates the aggregate (research R5,
      analysis finding C2).
    - Set `id = SampleProcessId.New()` and `startedAt = timeProvider.GetUtcNow()`.
    - Start the workflow with `ITemporalClient.StartWorkflowAsync(wf => wf.RunAsync(id.Value, delay, startedAt), …)`,
      using the workflow ID from `WorkflowId(id)`, `IdReusePolicy = RejectDuplicate` and task queue
      `diagnostics`.
    - Send `202` with `Location` set to `/api/diagnostics/sample-processes/{id}` and a
      `SampleProcessResponse` built from those values: `Status "Running"`, with no completion
      fields.
- [ ] T065 [US4] Create `src/MicroCommerce.ApiService/Features/Diagnostics/GetSampleProcess.cs`.
  - The endpoint is `Get("/diagnostics/sample-processes/{id}")`, with the
    `PlatformOperator` policy and `id` bound as a `Guid`. A non-UUID ID returns 400.
  - It loads the process with `AsNoTracking`, returning 404 when it isn't found.
  - It looks up `processed_events.processed_at` for consumer
    `sample-process-completed.record-receipt` and message `sample-process-completed:{id}`.
  - It returns `SampleProcessResponse(Guid Id, string Status, int DelaySeconds, DateTimeOffset StartedAt, DateTimeOffset? CompletedAt, DateTimeOffset? CompletionEventReceivedAt)`.
  - Register `SampleProcess → SampleProcessResponse` in `MappingConfig` with explicit maps:
    `Id.Value`, `Status.ToString()` and `(int)Delay.TotalSeconds`, plus
    `.Ignore(d => d.CompletionEventReceivedAt)`. Without the `Ignore`,
    `RequireDestinationMemberSource` makes `Compile()` fail in T041 (analysis finding U1).
  - After mapping, set the value with `response with { CompletionEventReceivedAt = receivedAt }`.
- [ ] T066 [US4] Create
  `src/MicroCommerce.ApiService/Features/Diagnostics/SampleProcessCompletedSubscriber.cs`.
  - The endpoint is `Post("/dapr/events/sample-process-completed")`, with `AuthSchemes("Dapr")`,
    excluded from the prefix and from OpenAPI.
  - It binds the CloudEvent's `data` as `SampleProcessCompletedV1`.
  - It calls `Inbox.TryRecordAsync("sample-process-completed.record-receipt", data.EventId, now, ct)`
    and returns 200 whether the event is new or a duplicate.
  - It lets exceptions return 500 so that Dapr retries (FR-012b).
- [ ] T067 [US4] Extend `tests/MicroCommerce.FunctionalTests/ApiFixture.cs`:
  - Add the Temporal container from `51043dfe^` (`temporalio/temporal:1.5.1`,
    `server start-dev --ip 0.0.0.0`, port 7233 random, waiting for TCP), and set `Temporal:Address`.
  - Replace `IIntegrationEventPublisher` with an NSubstitute substitute, exposed as a property.
  - Set `APP_API_TOKEN` to a fixed test value, exposed as `DaprApiToken`.
- [ ] T068 [US4] Complete quickstart Scenario 3 by hand, then run T048–T054; they must pass.

**Checkpoint**: All four stories work and are verified independently.

---

## Phase 7: Polish & Cross-Cutting Concerns

- [ ] T069 [P] Do a logging and secret review (FR-018, FR-019).
  - Run `git grep -nE '\$"|LogInformation\(\$|password|secret|token' -- src tests`.
  - Confirm there is no string-interpolated logging, no logged tokens or emails, and no committed
    passwords or tokens. The only allowed exceptions are test-generated random values.
- [ ] T070 [P] Do a constitution compliance pass.
  - Run `grep -rn "User\b\|Admin\b\|Seller\b" src/` (Principle I: `CONTEXT.md` terms only).
  - Confirm that every `/api` endpoint has a 401 test and, where it is role-gated, a 403 test
    (Principle III, SC-004).
  - Update plan.md Complexity Tracking with any fallback that the T011 spike chose.
- [ ] T071 Run every quickstart.md scenario from a fresh clone in a temporary directory. Time it
  from clone to running, excluding downloads (SC-001 target: under 15 min), and record the result in
  the PR description.
- [ ] T072 Run `dotnet build MicroCommerce.slnx` (zero warnings) and
  `pnpm --dir src/MicroCommerce.Web lint && pnpm --dir src/MicroCommerce.Web build`.
- [ ] T073 Verify SC-007, analysis finding E1.
  - Run the restart test 10 times in a row:
    `for i in $(seq 10); do dotnet test --project tests/MicroCommerce.IntegrationTests -- --filter-trait "Category=Restart" || break; done`.
    Use the `run-tests` skill to confirm the exact filter syntax for xUnit v3 on Microsoft.Testing.Platform.
  - All 10 runs must pass, each with exactly one `CompleteSampleProcess` completion.
  - Record the result in the PR description.
  - It runs separately because 10 runs of about 15 s each would break the SC-006 limit of 5 minutes
    for the default suite.

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies. T011, the spike, must finish before T024, T025, T060 and
  T054.
- **Foundational (Phase 2)**: Depends on Setup. Blocks all stories.
- **US1 (Phase 3)**: Depends on Foundational. This is the MVP.
- **US2 (Phase 4)**: Depends on US1, for the realm and AppHost.
- **US3 (Phase 5)**:
  - T040–T043 depend only on Foundational, so they can run in parallel with US1 and US2.
  - T045 depends on T038 (US2).
  - T046 runs after every story you plan to ship.
- **US4 (Phase 6)**: Depends on US1, for Temporal and Dapr in the AppHost, and on US2, for
  `AuthPolicies` and operator tokens.
- **Polish (Phase 7)**: Depends on every story. T073 depends on T054.

### Within Each Story

Write tests first and confirm they fail. Then build the domain, then persistence and
infrastructure, then the workflow, then the endpoints. Finish with manual quickstart validation.

### Gated tasks

- T036 and T037 (with T038's auth part) need approval for `react-oidc-context` and `oidc-client-ts`.
  Without it, US2 can still deliver its backend part (T030–T035) but not the browser sign-in.
- T044 and T045 need approval for `vitest`, `@testing-library/react` and `jsdom`.

## Parallel Examples

```text
# Setup, after T002:
T003 ServiceDefaults | T004 Domain csproj | T005 ApiService csproj | T006 AppHost csproj | T007 tests props | T010 web scaffold

# Foundational:
T012 AggregateRoot | T013 IDomainEvent+DomainRuleViolation | T014 AccountId | T016 MappingConfig | T017 exception handler | T019 TestIdentity

# US2 tests and implementation:
T030 | T031 | T032   then   T033 | T034   then T035

# US4 tests (all different files):
T048 | T049 | T050 | T051 | T052 | T053 | T054
# US4 domain:
T055 | T056   then T057
```

## Implementation Strategy

### MVP first (US1)

Do Setup, then Foundational, then US1, then **stop and validate** with quickstart Scenario 1. At
that point the system starts with one command and every component is healthy.

### Incremental delivery

1. US1 starts everything.
2. US2 adds sign-in. This is the highest-risk integration, so it comes next.
3. US3 adds test enforcement. T040–T043 can be done early, in parallel.
4. US4 adds the durable process and the Dapr event.

Each story is its own PR off `master`. The branch is `001-project-skeleton`, or one branch per story
if you prefer smaller reviews. The squash-merge workflow is in the constitution.

## Notes

- `[P]` means different files and no dependency on an unfinished task.
- Commit after each task or logical group, and only when the user asks.
- The domain never reads the clock. Pass `now` from `TimeProvider` or `Workflow.UtcNow`.
- Don't add packages beyond research R12. Anything else needs user approval first.
