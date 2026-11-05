# Implementation Plan: Product Catalog

**Branch**: `002-product-catalog` (stacked on `001-project-skeleton`) | **Date**: 2026-11-05 | **Spec**: [spec.md](./spec.md)

**Input**: Feature specification from `specs/002-product-catalog/spec.md`

## Summary

This feature delivers the selling side of the Marketplace on top of the 001 skeleton:

- **Accounts**: an Account opens a Merchant, with a Shop name and a Pickup Address.
- **Catalog**:
  - Platform Operators maintain the Category tree.
  - Merchants list Products with up to two options, Variants and up to 9 images, then publish them.
  - Visitors browse, search with English stemming, filter by Category including descendants, and
    open Products and Shop pages. Buyers see Availability, never stock.
  - Platform Operators can remove Products.

**Approach**:

- Rebuild the earlier implementation (git `a2743b4b`, issues #99, #103, #104 and #105) on the 001
  conventions: Domain project, Vogen, FastEndpoints, Mapster, `ICurrentAccount` and ProblemDetails.
- Catalog learns about Merchants through an in-process contract that Accounts publishes, not through
  events (research R1).
- Products carry an aggregate version for optimistic concurrency (R5).
- Images go to Azure Blob storage (Azurite locally). They are uploaded and served through the API,
  so their content is checked and their visibility is enforced (R7).

## Technical Context

**Language/Version**: C# on .NET 10 (SDK 10.0.400), with nullable reference types and warnings as
errors. TypeScript 5.9 for the web app.

**Primary Dependencies**:

- Existing: FastEndpoints 8.3, EF Core 10 with Npgsql (full-text search included), Vogen 8.0.7,
  Mapster 10.0.14 and Aspire 13.6.
- **New, approved 2026-11-05**: `Aspire.Hosting.Azure.Storage` 13.6.0,
  `Aspire.Azure.Storage.Blobs` 13.6.0, `Testcontainers.Azurite` 4.15.0 and `react-router` 8.4.0.

**Storage**:

- PostgreSQL 17 (`micro-commerce-db`), with schemas `accounts` and `catalog` alongside the existing
  `public`.
- Azure Blob Storage, container `product-images`, through Azurite locally.

**Testing**: xUnit v3, NSubstitute and Shouldly. `ApiFixture` adds an Azurite container. Aspire
AppHost integration tests. Vitest for the web app.

**Target Platform**: Local development (Aspire), as in 001.

**Project Type**: Web application: an API service, an SPA and the AppHost.

**Performance Goals**:

- p95 under 1 s for browse, search and filter with 10,000 Published Products (SC-003).
- Changes visible on the next view (SC-006).
- A 5 MB upload in under 10 s (SC-008).

**Constraints**:

- Buyers never see stock, a Pickup Address, or Draft or Removed Products.
- The Merchant always comes from the token.
- No caching (the constitution allows it only where justified).
- Image content is checked before an image is attached.

**Scale/Scope**:

- 2 bounded areas, 3 aggregates (Merchant, Category and Product), 2 new DbContexts.
- About 30 HTTP operations.
- About 9 web pages or areas.

All unknowns are resolved in [research.md](./research.md) (R1–R14). No NEEDS CLARIFICATION remains.

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Gate | Pre-design | Post-design |
|-----------|------|:-:|:-:|
| I. Ubiquitous Language | The plan uses Merchant, Shop, Product, Variant, Availability, Category, Platform Operator and Merchant Centre. Routes use `/marketplace`, not the avoided "storefront". `CONTEXT.md` gains Option, Option value and Product image, Pickup Address becomes its own term, and Variant's _Avoid_ list drops "option", all in the same change (R13). | ✅ | ✅ |
| II. Domain-Centred Design | Merchant, Category and Product are aggregates in `MicroCommerce.Domain`, which references only Vogen. Typed IDs for `MerchantId`, `CategoryId`, `ProductId`, `VariantId` and `ProductImageId`. One aggregate per transaction. Each area has its own DbContext and schema. Catalog reaches Merchants only through the published `IMerchantDirectory` contract, enforced by an architecture test (R1, R2). | ✅ | ✅ |
| III. Tests | Unit tests for every aggregate and validator. Functional tests give every operation a success test and a refusal test (401, 403 or cross-Merchant 404). Public Marketplace operations get visibility tests instead. Concurrency races are tested. The AppHost smoke test uses a real Keycloak and Azurite. The full map is in the [quickstart table](./quickstart.md#which-suite-verifies-what) (R14). | ✅ | ✅ |
| IV. Secure by Default | Only `/marketplace/*` is public, and it is marked so explicitly. The Merchant is resolved from the token through `ICurrentMerchant` (ADR-50), and no request carries `merchantId`. Validators sit at the boundary. Uploaded image content is checked. Images of hidden Products are never served. The seeded password is a generated Aspire secret (R11). | ✅ | ✅ |
| V. Durable and Idempotent | This feature has no time-based rules and no events (R1). Idempotent operations: publishing twice, unpublishing twice, and removal. Blob cleanup is best effort, and orphans are never referenced. | ✅ | ✅ |
| VI. Observable | Logging is through `ILogger<T>` with templates. Logs carry IDs only, never Pickup Addresses or emails. ProblemDetails `type` values are documented in the contract. Azurite appears in the dashboard. | ✅ | ✅ |
| VII. Simplicity and Dependencies | Reuses the old module patterns. No outbox, read model, cache, search engine or image library. All four new packages were approved by the user on 2026-11-05. The NuGet packages are pinned in `Directory.Packages.props`, and `react-router` is pinned in `package.json`. The deviations are listed below. | ⚠️ justified | ⚠️ justified |
| Technology Constraints | Object storage is not in the approved stack, so the constitution is amended to **1.1.0** (MINOR) in this pull request (R13). | ⚠️ amendment | ⚠️ amendment |

**Items to verify in the first implementation tasks**: if one fails, use its fallback and add a row
to Complexity Tracking.

1. `RunAsEmulator(e => e.WithDataVolume().WithLifetime(ContainerLifetime.Persistent))` works on
   Aspire 13.6 for the `storage` resource. It must stay ephemeral when the AppHost is run with
   `--EphemeralResources=true`.
2. `Property<NpgsqlTsVector>("SearchVector").IsGeneratedTsVectorColumn(...)` works on a shadow
   property and can be queried through `EF.Property`. Fallback: a private `NpgsqlTsVector` field
   mapped by EF. That would put an Npgsql type in the Domain, so the fallback must be recorded here
   if it is used.
3. `AreNullsDistinct(false)` produces `NULLS NOT DISTINCT` in the migration SQL.

## Project Structure

### Documentation (this feature)

```text
specs/002-product-catalog/
├── plan.md              # This file
├── research.md          # Phase 0: R1–R14
├── data-model.md        # Phase 1: aggregates, rules, transitions, blob layout, glossary additions
├── quickstart.md        # Phase 1: run and validation guide, plus the FR-to-suite map
├── contracts/
│   └── http-api.md      # /me, /merchant, /marketplace and /ops operations
├── checklists/requirements.md
└── tasks.md             # Phase 2 (/speckit-tasks), not created here
```

### Source Code (repository root)

```text
CONTEXT.md                                   # + Option, Option value, Product image; Pickup Address as its own term; Variant's Avoid list
.specify/memory/constitution.md              # 1.1.0: object storage in Technology Constraints
Directory.Packages.props                     # + Aspire Azure Storage (hosting and client); + Testcontainers.Azurite

src/
├── MicroCommerce.AppHost/
│   ├── AppHost.cs                           # + storage (Azurite) and blobs; + dev-merchant-password
│   └── Realms/micro-commerce-realm.json     # + merchant user with ${MC_MERCHANT_PASSWORD}
├── MicroCommerce.Domain/
│   ├── SharedKernel/MerchantId.cs
│   ├── Accounts/                            # Merchant, ShopName, PickupAddress
│   └── Catalog/                             # Category, CategoryId, Product, ProductId, ProductStatus,
│                                            # ProductOption, Variant, VariantId, VariantCombinations,
│                                            # Money, ProductImage, ProductImageId, ImageFormat
├── MicroCommerce.ApiService/
│   ├── Program.cs                           # + contexts, blob client, directory, route groups
│   ├── Features/
│   │   ├── Accounts/                        # GetMe (+ merchant), OpenMerchant, GetShop, UpdateShop,
│   │   │   │                                # MerchantDirectory (IMerchantDirectory),
│   │   │   │                                # CurrentMerchant (ICurrentMerchant), MerchantCentreGroup
│   │   │   └── Data/                        # AccountsDbContext, factory, MerchantConfiguration, Migrations/
│   │   └── Catalog/
│   │       ├── Categories/                  # Create, Get, Rename, Delete, GetTree; CategoryRules (row lock)
│   │       ├── MerchantProducts/            # List, Create, Get, Update, Publish, Unpublish; MerchantCatalog
│   │       ├── ProductImages/               # Upload, Reorder, Remove, Get (Merchant, Marketplace and ops);
│   │       │                                # ProductImageStore
│   │       ├── Marketplace/                 # ListProducts, GetProduct, GetShopPage; IProductSearch and
│   │       │                                # PostgresProductSearch; MarketplaceGroup
│   │       ├── Moderation/                  # GetOperatorProduct, RemoveProduct; OpsGroup
│   │       └── Data/                        # CatalogDbContext, factory, configurations, CatalogSeeder
│   │                                        # (starter tree), Migrations/
│   └── Infrastructure/
│       ├── Data/DatabaseMigrator.cs         # migrates all three contexts, then seeds, then MarkApplied()
│       ├── Data/AppDbContext.cs             # applies only its own namespace's configurations
│       ├── Data/VogenEfCoreConverters.cs    # + 5 ID converters
│       └── OpenApi/VogenIdSchemaTransformer.cs   # + 5 ID types
└── MicroCommerce.Web/src/
    ├── router.tsx                           # routes (react-router)
    ├── marketplace/                         # Home (grid, search, Category filter), ProductPage, ShopPage
    ├── merchant/                            # OpenShop, ShopProfile, ProductList, ProductForm (option
    │                                        # editor, Variant grid), ImageManager
    ├── ops/                                 # CategoryTree editor, ProductModeration
    └── lib/                                 # variantGrid.ts, price.ts, imageOrder.ts, authImage.ts (Vitest-covered)

tests/
├── MicroCommerce.UnitTests/
│   ├── Domain/Accounts/                     # Merchant, ShopName, PickupAddress
│   ├── Domain/Catalog/                      # Category, Product (status, version, images, combinations), Money, ImageFormat
│   ├── Features/Accounts/ and Catalog/      # validators
│   └── Architecture/                        # + Catalog must not reference AccountsDbContext
├── MicroCommerce.FunctionalTests/
│   ├── ApiFixture.cs                        # + Azurite container; ConnectionStrings:blobs
│   ├── MerchantHelpers.cs                   # NewMerchantAsync(actor), unique names
│   ├── Features/Accounts/                   # MerchantEndpointsTests, MeEndpointTests (+ merchant)
│   ├── Features/Catalog/                    # Category, MerchantProduct, ProductImage, Marketplace,
│   │                                        # Moderation and Concurrency tests
│   └── Performance/MarketplacePerformanceTests.cs   # [Fact(Explicit = true)], own fixture, SC-003
└── MicroCommerce.IntegrationTests/
    └── CatalogSmokeTests.cs                 # Keycloak merchant → open → create → image → publish → anonymous find
```

**Structure Decision**:

- This keeps 001's layout: the Domain project plus vertical feature slices in the ApiService.
- Catalog has sub-folders per sub-area because it has about 25 endpoints.
- Accounts and Catalog each get their own `Data/` folder with a DbContext and migrations, matching
  the earlier module layout (R2).
- No new .NET projects.

## Phase 0 and 1 Outputs

- [research.md](./research.md): R1–R14. Every technical unknown is resolved, and every new
  dependency is approved.
- [data-model.md](./data-model.md), [contracts/http-api.md](./contracts/http-api.md) and
  [quickstart.md](./quickstart.md).
- Agent context: there is no agent-context file with plan markers. `CLAUDE.md` does not exist and
  none was created.

## Risks

| Risk | Mitigation |
|------|------------|
| A later suspension feature needs to filter Marketplace queries by Merchant status, which the in-process contract can't do across pages | R1 names the trigger. FR-025 isolates the visibility rule to one query (`VisibleProducts`). The read model is added then. |
| Orphaned blobs build up after failed saves or removals | Orphans are never referenced and are logged with IDs. A cleanup job is out of scope and would be a durable workflow (Principle V). |
| Serving images through the API adds load to the API | Browsers revalidate with an ETag, so a visible image costs a cheap 304 and a hidden one returns 404 at once. This is acceptable for local scope. A CDN or SAS URLs can be revisited before production. |
| Persistent Keycloak volumes from 001 lack the `merchant` user | Quickstart documents the one-time volume deletion. The integration tests use ephemeral resources and are unaffected. |
| Aspire's default Azurite image (3.35.0 in Aspire 13.6.0) is older than the Blob SDK's API version, so the emulator rejects every call | The AppHost and `ApiFixture` both pin Azurite 3.37.0 (research R7, R8). When Aspire or the Blob SDK is upgraded, re-check the pin, and drop it once Aspire's default is new enough. The AppHost pin hasn't been run yet (T094, T095) |
| The 001 branch is not merged into `master` yet | This branch is stacked. Its pull request targets `master` after 001 merges, or targets `001-project-skeleton` until then. |

## Complexity Tracking

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| New object storage, Azure Blob through Azurite, with a constitution amendment to 1.1.0 (Technology Constraints, Principle VII) | FR-020–FR-023: the user asked for Merchant image uploads, and chose this store on 2026-11-05 | Postgres `bytea` and MinIO were offered, and the user rejected both |
| Two more DbContexts and schemas (Principle VII: no speculative layers) | Principle II: each bounded area owns its data, and a shared context can't enforce that | One context with several schemas lets any slice query any table |
| `react-router` reverses 001's "no router" decision (approved 2026-11-05) | The feature has about 9 pages, with deep links to Products and Shops | A hand-written router would be more code to maintain for no gain |
| `Testcontainers.Azurite` test dependency (approved 2026-11-05) | `ApiFixture` must provision its own isolated Azurite (Principle III), without putting the emulator key in code | A generic container would need the emulator's well-known key as a constant in code |
| Public `/marketplace/*` operations have no authorisation-refusal test (Principle III: every backend operation tests its success path and its authorisation refusal) | Principle IV explicitly allows public operations, such as browsing the Marketplace (FR-024). They have no authorisation to refuse. Their refusal test is the visibility test instead: a Draft or Removed Product returns 404 or is left out of lists, and its images return 404 (T058, T073, T086) | Adding sign-in to browsing would break FR-024. A constitution PATCH could clarify that the refusal clause applies to protected operations |
| Opt-in (explicit) performance test, outside the default run (it stretches Principle III's fast-test intent and SC-006's 5-minute budget the other way: it would breach that budget if it ran every time) | SC-003 needs 10,000 rows and repeated timing | Leaving SC-003 unverified is worse. It is documented in quickstart Scenario 6 |
