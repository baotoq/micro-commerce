# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Stack

.NET 10 · Aspire 13.6 · FastEndpoints 8 · EF Core (PostgreSQL) · Wolverine 6 (outbox and events between modules) · Redis distributed cache · Next.js 16 frontend (pnpm). Toolchain versions are pinned in `mise.toml`; `global.json` is what the SDK resolver actually reads, so keep the two in sync (and pnpm in sync with `packageManager` in `src/micro-commerce/package.json`).

## Commands

`mise install` installs the pinned toolchain (`mise.toml` holds only tool versions and env, no tasks). Common commands:

```bash
aspire run                                    # whole app: Postgres, Redis, Keycloak, API, frontend + Aspire dashboard
dotnet build
dotnet format --verify-no-changes             # .NET lint
pnpm --dir src/micro-commerce lint            # frontend lint

dotnet test --solution MicroCommerce.slnx     # all tests
dotnet test --project tests/MicroCommerce.UnitTests
dotnet test --project tests/MicroCommerce.UnitTests --filter-method "*Negative_price_fails"
dotnet test --project tests/MicroCommerce.FunctionalTests --filter-class "*ProductEndpointsTests"

pnpm --dir src/micro-commerce exec playwright install chromium   # once, before the first E2E run
pnpm --dir src/micro-commerce test:e2e        # Playwright E2E; starts or reuses `aspire run`
pnpm --dir src/micro-commerce test:e2e e2e/merchant-products.spec.ts

dotnet ef migrations add <Name> -p src/MicroCommerce.ApiService -c AccountsDbContext -o Features/Accounts/Data/Migrations   # needs dotnet tool restore first
dotnet ef migrations add <Name> -p src/MicroCommerce.ApiService -c CatalogDbContext -o Features/Catalog/Data/Migrations
dotnet ef migrations add <Name> -p src/MicroCommerce.ApiService -c OrderingDbContext -o Features/Ordering/Data/Migrations
```

Tests are xUnit v3 on **Microsoft.Testing.Platform in native mode** (set in `global.json`): use `--project`/`--solution` (no positional paths), and xUnit v3 filters (`--filter-class`, `--filter-method`, `--filter-trait`, `--filter-query`) passed directly — not VSTest `--filter` and no `--` separator. Functional and integration tests need a running container runtime. `dotnet test` doesn't run the E2E suite; it runs separately.

`TreatWarningsAsErrors` is on for every project (`Directory.Build.props`). Package versions live only in `Directory.Packages.props` (central package management) — `PackageReference` items have no `Version`.

## Architecture

- **`MicroCommerce.AppHost`** (Aspire) wires everything. Resource names here are contracts used elsewhere: the connection names `micro-commerce-db` and `redis` must match `Program.cs` (`AddNpgsqlDbContext` / `AddRedisDistributedCache`) and `ApiFixture`; the resource names `apiservice` and `keycloak` are what integration tests wait on and create clients for. The frontend gets the API address via the `API_URL` env var. Passing `--EphemeralResources=true` skips data volumes, admin UIs, persistent container lifetimes and the fixed ports; adding `--FixedPorts=true` keeps the fixed ports (Keycloak 8080, frontend 3000) with throwaway containers, which is how CI runs E2E.
- **Identity (ADR-0002)**: Keycloak imports the `micro-commerce` realm from `src/MicroCommerce.AppHost/Realms/` on start: clients `micro-commerce-frontend` (sign-in, redirect URI `http://localhost:3000/*`), `micro-commerce-api` (the token audience) and `micro-commerce-tests` (password grant, integration tests only); the `platform-operator` realm role, emitted as a flat `roles` claim; seeded users `buyer`, `merchant`, `operator` (password = username). Keycloak runs on port 8080 and the frontend on 3000 so issuer and redirect URIs stay stable. A persistent Keycloak volume skips re-importing an existing realm, so after editing the realm file delete the `keycloak` volume.
- **`MicroCommerce.ServiceDefaults`**: standard Aspire `AddServiceDefaults()` (OpenTelemetry, service discovery, HTTP resilience, `/health` + `/alive` — mapped only in Development).
- **`MicroCommerce.ApiService`** is organized as vertical slices under `Features/<Area>/`:
  - One file per endpoint holding its request record, FastEndpoints `Validator<T>` (FluentValidation), and `Endpoint` class (e.g. `Features/Catalog/CreateProduct.cs`). Endpoints are discovered automatically by `AddFastEndpoints()`; they inject their module's context (`AccountsDbContext`, `CatalogDbContext`, `OrderingDbContext`) and services via primary constructors and reply with `Send.*Async`.
  - The entity file holds the entity, its `IEntityTypeConfiguration<T>` (picked up by `ApplyConfigurationsFromAssembly`), and the response record (`Features/Catalog/Product.cs`). New entities also need a `DbSet` on their module's context and a migration.
  - Each module maps entities to responses with one Mapperly mapper, `Features/<Area>/<Area>Mapper.cs`: a `[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)] static partial class` of extension methods — `entity.ToResponse()` for loaded entities, `query.ProjectToResponse()` so read queries select only the response's columns (prefer it over materialising entities). Values that don't come from the entity are extra parameters (`account.ToMeResponse(isPlatformOperator, merchant)`). Every response member needs a source, otherwise the build fails; entity members a response doesn't carry are ignored. Don't hand-write mapping methods on response records.
  - Value objects are Vogen types; `VogenDefaults.cs` gives them EF Core and System.Text.Json conversions and a generated OpenAPI schema transformer (wired in `Program.cs`), so they reach JSON, the database and the OpenAPI document as their primitive. Every entity ID is a typed ID in `SharedKernel` (ADR-0005), created with `XxxId.New()` (a version-7 Guid); register a new ID's converter in its context's `ConfigureConventions`. Module-specific value objects such as `ShopName` and `Money` (whole VND, never negative) live in their module. Requests and responses use typed IDs directly (a route ID keeps its `{id:guid}` constraint). Value objects with rules stay primitives in requests, validated by FluentValidation, and handlers convert with `Xxx.From`, so invalid input is a 400, not a Vogen exception.
  - Errors use ProblemDetails (`AddProblemDetails` + `c.Errors.UseProblemDetails()`); validation error names come back camelCased.
  - Modules own their tables (ADR-0003). The Accounts module (Accounts, Merchants) has `AccountsDbContext` with its own `accounts` schema and migration history, and the Catalog module (`Features/Catalog/`: the Category tree, Products and their Variants) has `CatalogDbContext` with the `catalog` schema, and the Ordering module (`Features/Ordering/`: Carts and their lines) has `OrderingDbContext` with the `ordering` schema. Each context applies only its own feature namespace's entity configurations, so a new module gets its own context, schema and migrations.
  - Modules talk through events on **Wolverine** (ADR-0004, `Messaging.cs`): Postgres message storage in the `wolverine` schema (Wolverine creates it on startup; it isn't in any migration), durable local queues, and `TransactionMiddlewareMode.Lightweight`, since Aspire's retrying EF Core execution strategy refuses explicit transactions. Wolverine only carries messages; FastEndpoints stays the HTTP layer. A publishing endpoint takes `IDbContextOutbox<TContext>`, calls `PublishAsync` and then `SaveChangesAndFlushMessagesAsync`, and its context maps Wolverine's envelope tables (`modelBuilder.MapWolverineEnvelopeStorage(Messaging.Schema)`, excluded from migrations), so one `SaveChangesAsync` writes the change and its events. Handlers are public classes named `*Handler`. Delivery is at least once, in no set order, so handlers must be idempotent: Accounts' `MerchantOpened` / `MerchantProfileChanged` carry the Merchant's `ProfileVersion`, and `ShopHandler` upserts only a newer one. A context a handler takes needs `opts.CodeGeneration.AlwaysUseServiceLocationFor<TContext>()`, because Aspire registers contexts through a pooling factory.
  - The Catalog keeps its own `Shop` read model (`catalog.Shops`: Shop name and description) from those events and never reads Accounts' tables. The storefront shows a Product only when it is Published and its Shop has arrived (`StorefrontQueries.VisibleProducts`); storefront endpoints read Products only through that. Storefront lists and search go through the `IProductSearch` interface (`PostgresProductSearch`: Postgres full-text search, `english` config, over a generated, GIN-indexed `SearchVector` shadow column on Products built from name and description), so a dedicated engine can replace it; a Category filter includes its descendants. A Platform Operator can mark any Product Removed (`/ops/products/{id}/remove`), which is final: the Merchant still sees it, but editing, publishing or unpublishing it is a 409, and Merchant status changes are conditional updates so a concurrent removal still wins. Storefront Product detail is cached in Redis (`StorefrontProductCache`, `X-Cache: HIT/MISS`); evict the entry from whatever changes what Buyers see of a Product.
  - Ordering reads Variants only through the Catalog's in-process Lookup contract (ADR-0006, `ICatalogLookup` / `CatalogLookup` in `Features/Catalog/CatalogLookup.cs`): a batch read by Variant ID returning plain `VariantLookup` records (Product name, option values, Shop name, unit price, weight, owning `MerchantId`, available quantity, and `IsBuyable`, derived from `VisibleProducts`); a Variant that no longer exists is absent. It is the one deliberate exception to modules talking only through events. A Cart (`ordering.Carts`, keyed by `AccountId`, `Version` as its concurrency token; lines in `ordering.CartLines` keyed by `(AccountId, VariantId)`) holds no price or stock: `CartView.Build` reads each line live and flags it (`Unavailable`, `InsufficientStock`, `OwnMerchant`, `TotalTooLarge`) instead of dropping it, and the items total uses checked arithmetic against `Product.MaxPrice`. Every Cart edit bumps `Version`, so a concurrent edit is a 409; request validators check a line's quantity (1–99) and `CartValidator` checks the edited Cart (at most 50 lines, 99 of a Variant).
  - Auth: JWT bearer with `MapInboundClaims = false` (claims keep their token names: `sub`, `email`, `name`, `roles`). The authority comes from `Authentication:Schemes:Bearer:Authority`, which the AppHost sets. Endpoints require a token unless they call `AllowAnonymous()`. `CurrentAccount.GetOrCreateAsync(User)` maps the token subject to an Account, creating it on first use. `AuthPolicies.PlatformOperator` is the operator policy.
  - Route groups (FastEndpoints `Group<T>()`): `/merchant/*` (`MerchantCentre`: the signed-in Account's own Merchant, from `CurrentMerchant`, never from the route or body; 403 without one; Catalog endpoints read Products through `MerchantCatalog`, already scoped to that Merchant, so another Merchant's Product is a 404), `/ops/*` (`PlatformOperations`: Platform Operator policy, 403/401 for anyone else), `/storefront/*` (`Storefront`: anonymous), `/cart/*` (`Carts`: any signed-in Account's own Cart, from `BuyerCart`, never from the route or body).
  - In Development the API runs `Database.MigrateAsync()` on startup, seeds a starter Category tree into an empty one (`StarterCategories`), and serves Scalar at `/scalar/v1` and OpenAPI at `/openapi/v1.json`. `AccountsDbContextFactory` / `CatalogDbContextFactory` / `OrderingDbContextFactory` exist only for `dotnet ef` design-time.
- **`src/micro-commerce`** — Next.js 16 / React 19 with the React Compiler and Tailwind 4. `output: "standalone"` in `next.config.ts` is required by Aspire's `AddNextJsApp`. Sign-in uses Auth.js (`next-auth` v5, `src/auth.ts`) with the Keycloak provider. `src/proxy.ts` forwards `/api/*` to the API with the session's access token, except Auth.js's own `/api/auth/*`. Server code calls the API with `session.accessToken`. Its `AGENTS.md` applies: this Next.js version has breaking changes vs. training data, so read the relevant guide in `src/micro-commerce/node_modules/next/dist/docs/` before writing frontend code. `next dev` re-adds that AGENTS.md block — commit it rather than reverting it.

## Test layers

- **UnitTests** — pure tests of validators/domain types; reference the API project directly.
- **FunctionalTests** — API in-process via `FastEndpoints.Testing` (`ApiFixture : AppFixture<Program>`) against Testcontainers Postgres and Redis. Tests derive from `TestBase<ApiFixture>` and call endpoints with typed helpers (`App.Client.POSTAsync<TEndpoint, TRequest, TResponse>`). JWT validation trusts a test signing key instead of Keycloak: `App.ClientFor(TestActor.Buyer())` / `TestActor.PlatformOperator()` gives a client for a fresh subject, and `TestIdentity.TokenFor(...)` mints custom (expired, wrongly signed, other-audience) tokens. `App.Client` is anonymous. The app and containers are shared across the run, so tests must not assume an empty database. What an event changes in another module arrives in the background: poll for it with a timeout (`EventuallyAsync` in `StorefrontEndpointsTests`), never a fixed sleep.
- **IntegrationTests** — boot the full AppHost with `Aspire.Hosting.Testing` (assembly-level `AppHostFixture`, ephemeral resources) and hit the API over plain `HttpClient`. For real tokens, use the password grant against `KeycloakClient` with the `micro-commerce-tests` client and a seeded user.
- **E2E** (`src/micro-commerce/e2e/`) — Playwright, Chromium only, through the browser against the whole app: real Keycloak sign-in, `proxy.ts` token forwarding and the API round trip. `playwright.config.ts`'s `webServer` runs `aspire run` from the repo root and waits for `http://localhost:3000`, reusing an already-running dev session. It needs the fixed ports (the realm only allows the `localhost:3000` redirect URI), so not `aspire run --isolated` or a bare `--EphemeralResources=true`, and locally it runs against dev data: make names unique per run. With `CI` set it never reuses a running app and starts `aspire run -- --EphemeralResources=true --FixedPorts=true` itself. The `setup` project signs in once as the seeded `buyer` and saves `playwright/.auth/buyer.json` as `storageState` for the other tests; a test that signs out must sign in on its own (`test.use({ storageState: { cookies: [], origins: [] } })` + `signInAsBuyer`), since signing out ends that Keycloak session. `api-proxy.spec.ts` (`page.request` against `/api/me`) pins token forwarding on its own; the Merchant Centre and operator specs sign in as the seeded `merchant` / `operator` the same way (`signInAsMerchant`, `signInAsOperator`). Under `aspire run` Keycloak serves `https://localhost:8080` with the ASP.NET dev certificate, which Chromium may not trust (it doesn't on CI), so the config sets `ignoreHTTPSErrors`. Locators are role/label based (`getByRole`, `getByLabel`).

## CI

`.github/workflows/ci.yml` (GitHub Actions, on pushes to `master` and on pull requests) installs the toolchain from `mise.toml` with `jdx/mise-action`, so a version bump there is a CI bump too. Jobs: build and lint (`dotnet build`, `dotnet format --verify-no-changes`, frontend `lint` and `build`); unit, functional and integration tests as a matrix (one `dotnet test --project` each, on the runner's Docker); and E2E (`playwright install --with-deps chromium`, then `test:e2e`, uploading the report and traces on failure).

`tests/Directory.Build.props` makes every test project an `Exe` with xUnit v3 + Shouldly and global `using Xunit; using Shouldly;`. Assertions use Shouldly.

## Pull requests

Fill in `.github/pull_request_template.md`. When a PR changes the UI, capture screenshots of every new or changed page and state, and show them in the PR description. Take them with Playwright against the running app (`aspire run`), signed in as the seeded user the page needs. The GitHub API can't upload attachments, so push the PNGs to the `pr-screenshots` branch (never merged) under `<PR number>/` and embed each as `![what it shows](https://github.com/baotoq/micro-commerce/blob/pr-screenshots/<PR number>/<file>.png?raw=true)`.

## Agent skills

### Issue tracker

Issues live in GitHub Issues on `baotoq/micro-commerce` (via `gh`). See `docs/agents/issue-tracker.md`.

### Triage labels

Default vocabulary: `needs-triage`, `needs-info`, `ready-for-agent`, `ready-for-human`, `wontfix`. See `docs/agents/triage-labels.md`.

### Domain docs

Single-context: one root `CONTEXT.md` + `docs/adr/` (created lazily). See `docs/agents/domain.md`.
