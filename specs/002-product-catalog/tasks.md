---
description: "Task list for the MicroCommerce Product Catalog"
---

# Tasks: Product Catalog

**Input**: Design documents in `specs/002-product-catalog/`: plan.md, spec.md, research.md,
data-model.md, contracts/http-api.md and quickstart.md.

**Tests**: Tests are included, because Constitution Principle III is non-negotiable. Every operation
needs a success test and an authorisation-refusal test. Public Marketplace operations get a
visibility test instead. In each story, write the tests first and confirm they fail before
implementing.

**Organization**: One phase per spec user story, in priority order:

| Story | Priority | Scope |
|-------|----------|-------|
| US1 | P1 | Open a Merchant |
| US2 | P1 | List a Product |
| US3 | P1 | Browse |
| US4 | P2 | Options and Variants |
| US5 | P2 | Images |
| US6 | P2 | Category tree |
| US7 | P3 | Removal |

**Approvals**: Every new package is approved: Aspire Azure Storage (hosting and client),
`Testcontainers.Azurite` 4.15.0 and `react-router` 8.4.0, all on 2026-11-05. The constitution
amendment to 1.1.0 is part of this feature (T003).

**Reference code**: "Adapt from `a2743b4b`" means read the old file with
`git show 'a2743b4b:<path>'` (quote it, because zsh mangles `^:`) and adapt it to this plan. The plan
differs from the old code in these ways:

- no Wolverine or events
- no Redis cache
- no suspension or Reservations
- a `Version` concurrency token on Product
- `/marketplace` routes instead of `/storefront`
- unique sibling Category names
- the Vogen and Mapster conventions from 001

The research summary of the old code is in `research.md`.

**Conventions to keep from 001**:

- Endpoint, request, response and validator live in one file per operation.
- Validation errors are `HttpValidationProblemDetails` with camelCased field names.
- Domain rule breaks throw `DomainRuleViolation`.
- Handlers get the Account from `ICurrentAccount`, never from the body (ADR-50).
- Logging uses `ILogger<T>` message templates, and logs IDs only.
- Table names are snake_case.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependency on an unfinished task)
- **[Story]**: US1–US7, for user-story phases only

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Packages, governance documents and AppHost resources. Then verify the plan's three
risky assumptions before anything depends on them.

- [X] T001 Update `Directory.Packages.props`:
  - In "Aspire hosting", add `Aspire.Hosting.Azure.Storage` 13.6.0.
  - In "Aspire client integrations", add `Aspire.Azure.Storage.Blobs` 13.6.0.
  - In "Testing", add `Testcontainers.Azurite` 4.15.0.
  - Add `PackageReference`s for them in these projects:
    - `src/MicroCommerce.AppHost/MicroCommerce.AppHost.csproj`
    - `src/MicroCommerce.ApiService/MicroCommerce.ApiService.csproj`
    - `tests/MicroCommerce.FunctionalTests/MicroCommerce.FunctionalTests.csproj`
  - Run `dotnet build MicroCommerce.slnx`. It must succeed with warnings treated as errors.
- [X] T002 [P] Add `"react-router": "8.4.0"` to `dependencies` in `src/MicroCommerce.Web/package.json`
  with an exact pin, then run `pnpm --dir src/MicroCommerce.Web install` so `pnpm-lock.yaml` updates.
- [X] T003 [P] Amend `.specify/memory/constitution.md` to **1.1.0** (MINOR):
  - In Technology Constraints, add the bullet "**Object storage**: Azure Blob Storage, run locally
    as the Azurite emulator through Aspire. Product images only until a spec justifies more."
  - Update the version line to `1.1.0`, with Last Amended `2026-11-05`.
  - Add a short Sync Impact note at the top: why (002 Product images), and that no templates were
    affected.
- [X] T004 [P] Update `CONTEXT.md` (data-model "Glossary additions"):
  - Under Catalog, add **Option** ("A named way a Product varies (e.g. Colour), with a list of
    Option values." _Avoid_: attribute, variation) and **Option value** ("One allowed value of an
    Option (e.g. Red). A Variant picks one value per Option.").
  - Add **Product image** ("A picture of a Product at a position in its ordered images; the first is
    the cover. Images belong to the Product, not to a Variant.").
  - Change Variant's _Avoid_ line to "SKU, model. A Variant is one combination of Option values, not
    an Option."
  - Under Parties, add **Pickup Address** ("Where a Merchant's parcels ship from: street, district
    and province.").
- [X] T005 Update `src/MicroCommerce.AppHost/AppHost.cs` (research R7, R11):
  - Add `var merchantPassword = AddSecret("dev-merchant-password");` and pass
    `.WithEnvironment("MC_MERCHANT_PASSWORD", merchantPassword)` to keycloak.
  - Add `var storage = builder.AddAzureStorage("storage").RunAsEmulator(e => { if (!ephemeral)
    e.WithDataVolume().WithLifetime(ContainerLifetime.Persistent); });` and
    `var blobs = storage.AddBlobs("blobs");`.
  - Add `.WithReference(blobs).WaitFor(blobs)` to `apiservice`.
  - **Note (2026-11-05)**: the emulator image is pinned with `emulator.WithImageTag("3.37.0")`. Aspire 13.6.0 defaults to Azurite 3.35.0, which rejects the Blob SDK's API version `2026-06-06`.
- [X] T006 [P] Add a `merchant` user to `src/MicroCommerce.AppHost/Realms/micro-commerce-realm.json`.
  - Copy the `buyer` entry: username `merchant`, firstName `Mia`, lastName `Merchant`, email
    `merchant@micro-commerce.local`, `emailVerified: true`, and a password credential
    `${MC_MERCHANT_PASSWORD}`.
  - Give it no realm roles beyond the defaults.
- [X] T007 Verify the three plan assumptions, and record the outcome of each in this task's line
  (or in Complexity Tracking if a fallback is needed):
  - (a) Run `aspire run`. The `storage` and `blobs` resources must reach Running. Then run the
    AppHost with `--EphemeralResources=true` and confirm that no Azurite volume is created.
  - (b) In a scratch EF model, `Property<NpgsqlTsVector>("SearchVector").IsGeneratedTsVectorColumn("english", ["Name", "Description"])`
    must generate a `GENERATED ALWAYS AS (to_tsvector(...)) STORED` column, and
    `EF.Property<NpgsqlTsVector>(p, "SearchVector").Matches(...)` must translate.
  - (c) `HasIndex(...).IsUnique().AreNullsDistinct(false)` must emit `NULLS NOT DISTINCT`.
  - Delete the scratch code afterwards.
  - **Outcome (2026-11-05)**:
    - (a) The AppHost compiles with `storage` and `blobs`, and `StartupTests` now covers both.
      Running it is blocked on this machine: the Dapr CLI isn't installed (`Unable to locate the
      Dapr CLI`), which is a 001 prerequisite. It needs a run where `dapr init --slim` has been
      done.
    - (b) Verified in T050 against the real Product model.
    - (c) Verified: the `InitialCatalog` script emits
      `CREATE UNIQUE INDEX ix_categories_parent_id_normalized_name … NULLS NOT DISTINCT`.

**Checkpoint**: Packages build, the governance documents are updated, Azurite runs, and the
assumptions are confirmed.

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Typed IDs, the two new DbContexts, migrations, seeding, the Category aggregate (every
Product needs a leaf), the route groups, and the test fixture. Each of these blocks several stories.

**⚠️ CRITICAL**: No user-story work can begin until this phase is complete.

### Typed IDs and shared kernel

- [X] T008 [P] Create `src/MicroCommerce.Domain/SharedKernel/MerchantId.cs`:
  `[ValueObject<Guid>] public readonly partial struct MerchantId;`. Its doc comment says that
  Accounts and Catalog share it (research R1).
- [X] T009 [P] Create the Catalog IDs in `src/MicroCommerce.Domain/Catalog/`: `CategoryId.cs`,
  `ProductId.cs`, `VariantId.cs` and `ProductImageId.cs`, each `[ValueObject<Guid>]`, following
  `SampleProcessId`.
- [X] T010 Register the 5 new IDs (`MerchantId`, `CategoryId`, `ProductId`, `VariantId` and
  `ProductImageId`):
  - as `[EfCoreConverter<…>]` lines in
    `src/MicroCommerce.ApiService/Infrastructure/Data/VogenEfCoreConverters.cs`
  - as `typeof(…)` entries in
    `src/MicroCommerce.ApiService/Infrastructure/OpenApi/VogenIdSchemaTransformer.cs`
  - Extend `tests/MicroCommerce.UnitTests/Infrastructure/VogenIdSchemaTransformerTests.cs` so it
    covers them.

### Category aggregate (needed by US2, US3 and US6)

- [X] T011 [P] Write unit tests in `tests/MicroCommerce.UnitTests/Domain/Catalog/CategoryTests.cs`:
  - `Name` is trimmed, 1–100 characters, and blank is refused.
  - `NormalizedName` is the upper-case invariant of `Name`.
  - `Rename` changes both.
  - `CreateChild` sets `ParentId`. A Category's parent never changes.
- [X] T012 [P] Create `src/MicroCommerce.Domain/Catalog/Category.cs` as an aggregate with `Id`,
  `Name` ("Trimmed, 1–100 characters"), `NormalizedName`, and `ParentId` ("Set at creation and
  never changed").
  - Factories `CreateTopLevel(id, name)` and `CreateChild(id, name, parentId)`, and a `Rename(name)`
    method.
  - Each throws `DomainRuleViolation` for an invalid name.
- [X] T013 Create `src/MicroCommerce.ApiService/Features/Catalog/Data/CatalogDbContext.cs`:
  - Schema `catalog`. `DbSet<Category>` for now; Products are added in US2.
  - `ApplyConfigurationsFromAssembly` filtered to types whose namespace starts with
    `MicroCommerce.ApiService.Features.Catalog`.
  - `ConfigureConventions` calls `RegisterAllInVogenEfCoreConverters()`.
  - Migrations history table `__EFMigrationsHistory` in schema `catalog`, set through
    `AddNpgsqlDbContext`'s `configureDbContextOptions`: `UseNpgsql(o =>
    o.MigrationsHistoryTable("__EFMigrationsHistory", "catalog"))`.
  - Add `CatalogDbContextFactory.cs`, a design-time factory that mirrors `AppDbContextFactory`.
- [X] T014 Create `src/MicroCommerce.ApiService/Features/Catalog/Data/CategoryConfiguration.cs`:
  - Table `categories`. Name max 100, NormalizedName max 100.
  - Self foreign key on `ParentId` with `DeleteBehavior.Restrict`.
  - Unique index `(ParentId, NormalizedName)` named `ix_categories_parent_id_normalized_name`, with
    `.AreNullsDistinct(false)`. Expose the name as a constant, `CategoryIndexes.SiblingName`.
- [X] T015 [P] Create `src/MicroCommerce.ApiService/Features/Accounts/Data/AccountsDbContext.cs` and
  `AccountsDbContextFactory.cs`:
  - Schema `accounts`. Leave the DbSets empty; Merchants are added in US1.
  - Configurations filtered to the `MicroCommerce.ApiService.Features.Accounts` namespace.
  - Vogen conventions, and the migrations history in schema `accounts`.
- [X] T016 Change `src/MicroCommerce.ApiService/Infrastructure/Data/AppDbContext.cs` so that
  `OnModelCreating` applies only configurations in the
  `MicroCommerce.ApiService.Features.Diagnostics` and `MicroCommerce.ApiService.Infrastructure`
  namespaces (research R2). Then run
  `dotnet ef migrations has-pending-model-changes --context AppDbContext`, which must report none.
- [X] T017 Register both contexts in `src/MicroCommerce.ApiService/Program.cs` with
  `builder.AddNpgsqlDbContext<AccountsDbContext>("micro-commerce-db", configureDbContextOptions: …)`
  and the same for `CatalogDbContext`, each with its schema's migrations history table (T013).
- [X] T018 Create `src/MicroCommerce.ApiService/Features/Catalog/Data/CatalogSeeder.cs`:
  - When `catalog.categories` is empty, insert the starter tree from research R4:
    - Fashion › Men › (T-Shirts, Shirts)
    - Fashion › Women › (Dresses, Tops)
    - Home › (Kitchen, Bedding)
    - Electronics › (Phones, Laptops)
  - Adapt from `a2743b4b:src/MicroCommerce.ApiService/Features/Catalog/Data/StarterCategories.cs`.
- [X] T019 Update `src/MicroCommerce.ApiService/Infrastructure/Data/DatabaseMigrator.cs`:
  - In one scope, call `MigrateAsync` on `AppDbContext`, then `AccountsDbContext`, then
    `CatalogDbContext`.
  - Then call `CatalogSeeder.SeedAsync` (Development only; the migrator already runs only in
    Development).
  - Only then call `status.MarkApplied()`, so `ApiFixture` never races the seed (research R2, R4).
- [X] T020 Add the initial migrations, then run `dotnet ef migrations has-pending-model-changes`
  for both contexts:
  - `dotnet ef migrations add InitialAccounts --context AccountsDbContext --output-dir Features/Accounts/Data/Migrations`
  - `dotnet ef migrations add InitialCatalog --context CatalogDbContext --output-dir Features/Catalog/Data/Migrations`,
    which creates `catalog.categories` with the `NULLS NOT DISTINCT` index.
- [X] T021 Create `src/MicroCommerce.ApiService/Features/Catalog/Categories/CategoryRules.cs`:
  - `SaveWithCategoryLockedAsync(CatalogDbContext db, CategoryId id, Func<Category?, CancellationToken, Task> check, CancellationToken ct)`.
  - It runs inside `db.Database.CreateExecutionStrategy().ExecuteAsync` with a transaction.
  - It locks the Category with
    `db.Categories.FromSql($"SELECT * FROM catalog.categories WHERE id = {id} FOR UPDATE")`.
  - Then it calls `check`, then `SaveChangesAsync(acceptAllChangesOnSuccess: false)`, commits, and
    calls `AcceptAllChanges()`.
  - Doc comment: `check` must only read and stage changes, because it runs again on a transient
    retry.
  - Adapt from `a2743b4b`.

### Route groups, Merchant context and errors

- [X] T022 [P] Create `src/MicroCommerce.ApiService/Features/Catalog/Marketplace/MarketplaceGroup.cs`,
  a FastEndpoints `Group` with prefix `marketplace` and `AllowAnonymous()`. Also create
  `src/MicroCommerce.ApiService/Features/Catalog/Moderation/OpsGroup.cs` with prefix `ops` and
  `Policies(AuthPolicies.PlatformOperator)`.
- [X] T023 Create `src/MicroCommerce.ApiService/Infrastructure/ConflictProblems.cs`:
  - A helper that sends a 409 ProblemDetails with a `type` from the contract's conflict table:
    `merchant-exists`, `shop-name-taken`, `shop-version-conflict`, `product-version-conflict`,
    `product-removed`, `category-has-products`, `category-has-children`, `category-name-taken` and
    `image-limit-reached`.
  - Also a `merchant-required` helper for 403.
  - Unit-test both in `tests/MicroCommerce.UnitTests/Infrastructure/ConflictProblemsTests.cs`.
- [X] T024 Add `GET /marketplace/categories` in
  `src/MicroCommerce.ApiService/Features/Catalog/Marketplace/GetCategoryTree.cs`. Every page and US6
  need it:
  - Load every Category with `AsNoTracking`, then build `CategoryNode { id, name, children[] }` in
    memory.
  - Sort siblings by name using `StringComparer.OrdinalIgnoreCase`.
  - The endpoint is in `MarketplaceGroup`.

### Test fixture and architecture

- [X] T025 Extend `tests/MicroCommerce.FunctionalTests/ApiFixture.cs`:
  - Start `new AzuriteBuilder("mcr.microsoft.com/azure-storage/azurite:3.37.0").Build()` (3.35.0 rejects the Blob SDK's API version `2026-06-06`) in
    `PreSetupAsync`, in parallel with Postgres and Temporal.
  - In `ConfigureApp`, call `a.UseSetting("ConnectionStrings:blobs", _azurite.GetConnectionString())`.
  - Add a `CatalogTestData` helper class in `tests/MicroCommerce.FunctionalTests/CatalogTestData.cs`:
    - `Unique(prefix)`, which returns `$"{prefix} {Guid.NewGuid():N}"`.
    - `SeededLeafCategoryAsync(HttpClient)`, which reads `/api/marketplace/categories` and returns
      the ID of `Home › Kitchen`.
  - Tests never assume an empty tree.
- [X] T026 [P] Add the area-boundary architecture test in
  `tests/MicroCommerce.UnitTests/Architecture/AreaBoundaryTests.cs`:
  - Use plain reflection. No type in the namespace `MicroCommerce.ApiService.Features.Catalog` may
    have a constructor parameter, field or property of type `AccountsDbContext`.
  - No type in `…Features.Accounts` may use `CatalogDbContext`.
- [X] T027 [P] Write a functional test in
  `tests/MicroCommerce.FunctionalTests/Features/Catalog/CategoryTreeTests.cs`:
  - An anonymous `GET /api/marketplace/categories` returns 200.
  - The result contains the starter tree, nested and sorted, with Home › Kitchen as a leaf.
- [X] T028 [P] Set up web routing in `src/MicroCommerce.Web/src/router.tsx` (react-router 8.4.0,
  declarative mode):
  - Wrap `App` in `BrowserRouter` in `src/MicroCommerce.Web/src/main.tsx`.
  - Define routes for `/`, `/products/:id`, `/shops/:id`, `/open-shop`, `/merchant/*` and `/ops/*`,
    with placeholder components.
  - Keep the 001 home content (sign in, sign out and `/api/me`) in a header shown on every route.
  - Update `src/MicroCommerce.Web/src/App.test.tsx` so it still passes.
- [X] T029 [P] Extend `src/MicroCommerce.Web/src/api.ts`:
  - `getJson`, `sendJson` (POST, PUT and DELETE) and `uploadFile` (multipart) helpers. They attach
    the OIDC access token when signed in and throw a typed `ApiError { status, type, errors }`
    parsed from ProblemDetails.
  - Create `src/MicroCommerce.Web/src/lib/authImage.ts`: a `useAuthImage(url)` hook that fetches
    with the token, returns `URL.createObjectURL(blob)` and revokes it on unmount (research R10).
  - Write Vitest tests in `src/MicroCommerce.Web/src/lib/authImage.test.ts` with `fetch` stubbed.

**Checkpoint**: Three schemas migrate and the starter tree is seeded before the app reports healthy.
The tree is readable anonymously. The fixture runs Azurite. The SPA has routes.

---

## Phase 3: User Story 1: An Account opens a Merchant (Priority: P1) 🎯 MVP

**Goal**: A signed-in Account opens exactly one Merchant, with a unique Shop name and a Pickup
Address, and can edit it. `/me` reports the Merchant.

**Independent Test**:

1. Open a Merchant as a fresh Account, and confirm `/me` shows it.
2. A second open returns 409 `merchant-exists`.
3. Another Account trying the same name in different letter case gets 409 `shop-name-taken`.

### Tests for User Story 1 ⚠️ (write first, see them fail)

- [X] T030 [P] [US1] Write unit tests in `tests/MicroCommerce.UnitTests/Domain/Accounts/`:
  - `ShopNameTests.cs`: trimmed, 3–50 characters, blank refused.
  - `PickupAddressTests.cs`: street required and at most 200 characters; district and province
    required and at most 100 characters.
  - `MerchantTests.cs`:
    - `Open` sets the fields, the normalized name and `Version = 1`.
    - `ChangeProfile` bumps `Version`, and a case-only rename keeps the same normalized name.
    - A blank description becomes null, and a description over 2,000 characters is refused.
- [X] T031 [P] [US1] Write validator unit tests in
  `tests/MicroCommerce.UnitTests/Features/Accounts/OpenMerchantValidatorTests.cs` and
  `UpdateShopValidatorTests.cs`. Each rule from the contract must fail on its camelCased field:
  `shopName`, `description`, `pickupAddress.street`, `pickupAddress.district`,
  `pickupAddress.province` and `version`.
- [X] T032 [US1] Write functional tests in
  `tests/MicroCommerce.FunctionalTests/Features/Accounts/MerchantEndpointsTests.cs`. Adapt the names
  from `a2743b4b`:
  - `POST /api/me/merchant`:
    - returns 201 with `Location` `/api/marketplace/shops/{id}`, and `/me` then shows
      `merchant.shopName`
    - trims the name
    - 400 per invalid field
    - 401 anonymous
    - 409 `merchant-exists` for a second open
    - 409 `shop-name-taken` when the name differs only in letter case
    - 10 parallel opens by the same Account create exactly one Merchant
    - 10 parallel opens with the same name by different Accounts create exactly one Merchant
  - `GET /api/merchant/shop`: 200 for the owner (including the Pickup Address), 403
    `merchant-required` for an Account without a Merchant, 401 anonymous.
  - `PUT /api/merchant/shop`:
    - 200, and the version is bumped
    - a case-only rename of the Merchant's own name is allowed
    - 409 `shop-name-taken` for a name owned by another Merchant
    - 409 `shop-version-conflict` for a stale version
    - 400, 401 and 403 as for the other operations
  - Add a `NewMerchantAsync(this ApiFixture app, TestActor? actor = null)` helper in
    `tests/MicroCommerce.FunctionalTests/MerchantHelpers.cs`. It opens a Merchant with a unique name
    and returns `(HttpClient client, Guid merchantId)`.
- [X] T033 [P] [US1] Extend `tests/MicroCommerce.FunctionalTests/Features/Accounts/MeEndpointTests.cs`:
  - `merchant` is null for a fresh Account.
  - It is set after `POST /api/me/merchant`.
  - The existing 001 assertions are unchanged (an additive field).

### Implementation for User Story 1

- [X] T034 [P] [US1] Create `src/MicroCommerce.Domain/Accounts/ShopName.cs` and
  `src/MicroCommerce.Domain/Accounts/PickupAddress.cs`:
  - `ShopName` is a Vogen string value object: "Trimmed, 3–50 characters". `Normalize` trims, and
    `Validate` checks the length.
  - `PickupAddress` is a sealed record. Its constructor validates "Street: Required, at most 200
    characters", "District: Required, at most 100 characters" and "Province: Required, at most 100
    characters", and throws `DomainRuleViolation`.
- [X] T035 [US1] Create `src/MicroCommerce.Domain/Accounts/Merchant.cs` as an
  `AggregateRoot<MerchantId>` with:
  - `AccountId`, `ShopName`, `NormalizedShopName`
  - `Description` ("At most 2,000 characters. Blank becomes null")
  - `PickupAddress`, `CreatedAt` and `Version`
  - `static Open(id, accountId, shopName, description, pickupAddress, now)`
  - `ChangeProfile(shopName, description, pickupAddress)`, which bumps `Version`
- [X] T036 [US1] Create `src/MicroCommerce.ApiService/Features/Accounts/Data/MerchantConfiguration.cs`:
  - Table `merchants`.
  - `ShopName` stored with a Vogen converter, max 50. `NormalizedShopName` max 50.
  - `PickupAddress` as a `ComplexProperty` with the column lengths 200, 100 and 100.
  - `Version` as `IsConcurrencyToken()`.
  - Unique indexes `ix_merchants_account_id` and `ix_merchants_normalized_shop_name`, both exposed
    as constants in `MerchantIndexes`.
  - Add `DbSet<Merchant> Merchants` to `AccountsDbContext`, and run
    `dotnet ef migrations add AddMerchants --context AccountsDbContext --output-dir Features/Accounts/Data/Migrations`.
- [X] T037 [US1] Create `src/MicroCommerce.ApiService/Features/Accounts/MerchantDirectory.cs`:
  - `IMerchantDirectory` with `FindByAccountAsync(AccountId, ct) → MerchantRef?` and
    `GetShopsAsync(IReadOnlyCollection<MerchantId>, ct) → IReadOnlyDictionary<MerchantId, ShopRef>`.
  - `MerchantRef(MerchantId Id, string ShopName)` and
    `ShopRef(MerchantId Id, string ShopName, string? Description)`.
  - The `EfMerchantDirectory` implementation over `AccountsDbContext`, with `AsNoTracking`.
  - Register it as scoped in `Program.cs` (research R1).
- [X] T038 [US1] Create `src/MicroCommerce.ApiService/Features/Accounts/CurrentMerchant.cs` and
  `MerchantCentreGroup.cs`:
  - `ICurrentMerchant`: a request-scoped holder of `MerchantRef`.
  - `MerchantCentreGroup`: prefix `merchant`, authenticated.
  - A `RequireMerchant` global pre-processor for the group. It resolves `ICurrentAccount`, then
    `IMerchantDirectory.FindByAccountAsync`.
    - No Account: 401.
    - No Merchant: 403 `merchant-required` (T023).
    - Otherwise it sets `ICurrentMerchant`.
  - The Merchant is never read from the route or body (ADR-50).
- [X] T039 [US1] Create `src/MicroCommerce.ApiService/Features/Accounts/OpenMerchant.cs`
  (`POST /me/merchant`):
  - The request, a validator per the contract, and the endpoint.
  - It checks the existing-Merchant rule (409 `merchant-exists`) and the name rule (409
    `shop-name-taken`) first. On save, it catches `DbUpdateException` with `PostgresException`
    `23505` and maps `ConstraintName` through `MerchantIndexes` to the same two conflicts.
  - It returns 201 `ShopProfileResponse` with `Location: /api/marketplace/shops/{id}`.
  - It uses `TimeProvider` for `CreatedAt`.
  - It logs `"Merchant {MerchantId} opened for Account {AccountId}"`.
  - Reuse `Infrastructure/Data/PostgresErrors.cs`.
- [X] T040 [P] [US1] Create `src/MicroCommerce.ApiService/Features/Accounts/GetShop.cs`
  (`GET /merchant/shop`) and `UpdateShop.cs` (`PUT /merchant/shop`, with `version`):
  - A stale version, or `DbUpdateConcurrencyException`, returns 409 `shop-version-conflict`.
  - A taken name returns 409 `shop-name-taken` through the same `23505` mapping.
  - Both endpoints are in `MerchantCentreGroup`.
- [X] T041 [US1] Extend `src/MicroCommerce.ApiService/Features/Accounts/GetMe.cs`:
  - Add the nullable `MerchantSummary? Merchant` (`merchantId`, `shopName`) to `MeResponse`,
    resolved through `IMerchantDirectory`.
  - Update the Mapster config in `Infrastructure/Mapping/MappingConfig.cs`, and keep
    `MappingConfigTests` green.
- [X] T042 [P] [US1] Create the web pages for this story:
  - `src/MicroCommerce.Web/src/merchant/OpenShop.tsx`: a form for the Shop name, description,
    street, district and province. It shows field errors from `ApiError.errors` and a message for
    each 409 `type`. On success it navigates to `/merchant`.
  - `src/MicroCommerce.Web/src/merchant/ShopProfile.tsx`: edits with `version`. A 409
    `shop-version-conflict` shows "changed elsewhere, reload".
  - The header shows "Open your Shop" or "Merchant Centre", based on `/api/me`'s `merchant`.

**Checkpoint**: US1 tests pass. Quickstart Scenario 2 works in the browser.

---

## Phase 4: User Story 2: A Merchant lists a Product in the Merchant Centre (Priority: P1)

**Goal**: A Merchant creates single-Variant Products (Draft), edits them under version control,
publishes and unpublishes them, and lists its own Products. Every other Merchant's Product is a 404.

**Independent Test**:

1. Merchant A creates, lists and publishes a Product.
2. Merchant B gets 404 for every operation on A's Product.
3. An Account without a Merchant gets 403. An anonymous caller gets 401.

### Tests for User Story 2 ⚠️

- [X] T043 [P] [US2] Write unit tests in `tests/MicroCommerce.UnitTests/Domain/Catalog/MoneyTests.cs`:
  - Greater than 0 and at most 9,007,199,254,740,991 are valid.
  - 0, negative values and values above that limit are refused.
- [X] T044 [P] [US2] Write unit tests in `tests/MicroCommerce.UnitTests/Domain/Catalog/ProductTests.cs`:
  - `Create` gives Draft, `Version = 1` and one default Variant with empty `OptionValues`.
  - `Publish`: Draft → Published, and the version is bumped. Publishing twice is a no-op with no
    bump.
  - `Unpublish` is the reverse.
  - `Edit` keeps the status and the default Variant's ID, and bumps the version.
  - The name is trimmed, 1–200 characters. A blank description becomes null, and a description
    over 5,000 characters is refused.
  - Stock below 0 is refused, and a weight of 0 or less is refused.
- [X] T045 [P] [US2] Write validator unit tests in
  `tests/MicroCommerce.UnitTests/Features/Catalog/ProductRequestValidatorTests.cs`. Each rule fails
  on its field: `name`, `description`, `categoryId`, `variants[0].price`, `variants[0].stock`,
  `variants[0].weightGrams` and `version`.
- [X] T046 [US2] Write functional tests in
  `tests/MicroCommerce.FunctionalTests/Features/Catalog/MerchantProductEndpointsTests.cs`, with two
  Merchants created through `NewMerchantAsync`:
  - **Create**: 201 with `Location`, a Draft with one default Variant. 400 per invalid field. 400
    on `categoryId` for an unknown Category and for a non-leaf Category (`Home`).
  - **Get**: 200 for the owner.
  - **Update**: 200, the status is kept, the Variant ID is kept, and `version` increments. Moving
    to a non-leaf Category returns 400.
  - **Publish and unpublish**: the transitions work.
  - **Stale version**: a stale `version` on PUT or publish returns 409 `product-version-conflict`.
  - **Parallel saves**: two parallel PUTs with the same version give exactly one 200 and one 409.
  - **List**: shows only the Merchant's own Products, newest first, with `status`, `variantCount`,
    `minPrice`, `maxPrice`, `totalStock` (a `long`) and `coverImageUrl: null`.
  - **Cross-Merchant**: Merchant B gets 404 on get, put, publish and unpublish of A's Product, and
    nothing changes.
  - **Unknown ID**: 404.
  - **No Merchant**: 403 `merchant-required` on every route.
  - **Anonymous**: 401 on every route.
- [X] T047 [P] [US2] Write a functional test in
  `tests/MicroCommerce.FunctionalTests/Features/Catalog/CategoryConcurrencyTests.cs`:
  - A Product create into a fresh leaf, racing an operator adding a child to the same leaf (run in
    parallel 20 times), never leaves a Product in a non-leaf Category.
  - This test creates its own Categories through the `CatalogDbContext` from `App.Services`, so it
    doesn't depend on US6's endpoints.

### Implementation for User Story 2

- [X] T048 [P] [US2] Create the Product building blocks in `src/MicroCommerce.Domain/Catalog/`:
  - `Money.cs`: a Vogen `long`. `Validate` requires greater than 0 and at most
    9,007,199,254,740,991.
  - `ProductStatus.cs`: an enum with `Draft`, `Published` and `Removed`.
  - `Variant.cs`: `Id`, `Position`, `OptionValues` (`IReadOnlyList<string>`), `Price`, `Stock`
    ("0 or more") and `WeightGrams` ("Greater than 0").
  - `VariantSpec.cs`: a record `(IReadOnlyList<string> OptionValues, Money Price, int Stock,
    int WeightGrams)`, used as the input to Create and Edit.
- [X] T049 [US2] Create `src/MicroCommerce.Domain/Catalog/Product.cs` as an
  `AggregateRoot<ProductId>` with these members:
  - `MerchantId`, `Name` ("Trimmed, 1–200 characters") and `Description` ("At most 5,000
    characters. Blank becomes null")
  - `CategoryId`, `Status`, `CreatedAt` and `Version`
  - `Options` (`IReadOnlyList<ProductOption>`, an empty list in this story)
  - `Variants` and `Images` (an empty list in this story)
  - `static Create(id, merchantId, name, description, categoryId, IReadOnlyList<ProductOption> options, IReadOnlyList<VariantSpec> variants, now)`
  - `Edit(...)`, `Publish()` and `Unpublish()`: every change bumps `Version`, and a Removed Product
    refuses with `DomainRuleViolation`.
  - For this story, accept only an empty options list with exactly one spec whose `OptionValues` is
    empty. US4 generalises this.
- [X] T050 [US2] Create `src/MicroCommerce.ApiService/Features/Catalog/Data/ProductConfiguration.cs`:
  - Table `products`. `Name` max 200, `Description` max 5,000.
  - `Status` as a string, `Version` as `IsConcurrencyToken()`.
  - `Options` as a jsonb `ComplexCollection`.
  - `Variants` as `OwnsMany` to table `variants`: `Id` `ValueGeneratedNever`, `OptionValues` as
    `text[]`, `Price` with a Vogen converter.
  - `Images` as `OwnsMany` to table `product_images`, with the columns from data-model; no
    endpoints yet.
  - Foreign key to `categories` with `Restrict`.
  - Indexes `(MerchantId, CreatedAt DESC)`, `(Status, CreatedAt DESC)` and `(CategoryId)`.
  - Shadow `SearchVector` per T007(b): `IsGeneratedTsVectorColumn("english", ["Name", "Description"])`
    with a GIN index.
  - Add `DbSet<Product> Products` to `CatalogDbContext`, and run
    `dotnet ef migrations add AddProducts --context CatalogDbContext --output-dir Features/Catalog/Data/Migrations`.
- [X] T051 [US2] Create `src/MicroCommerce.ApiService/Features/Catalog/MerchantProducts/MerchantCatalog.cs`,
  a scoped service:
  - `Products`, which is `db.Products.Where(p => p.MerchantId == currentMerchant.Id)`.
  - A doc comment: every Merchant Centre read and write goes through this, never `db.Products`
    (FR-016).
  - Also add `ProductResponses.cs` with `MerchantProductResponse`, `MerchantProductSummary`,
    `VariantResponse` and `ImageRef`, plus the Mapster outward config in `MappingConfig.cs`.
  - **Outcome (2026-11-05)**: responses are built by static `From` factories on the response
    records (`MerchantProductResponse.From`, and also `ShopProfileResponse.From` and
    `OperatorProductResponse.FromAsync`), not by Mapster. The factories need the Product ID to
    build image URLs, and `OperatorProductResponse` needs the Merchant directory. Mapster still
    maps 001's responses (`MeResponse`, `SampleProcessResponse`). See research R12.
- [X] T052 [US2] Create
  `src/MicroCommerce.ApiService/Features/Catalog/MerchantProducts/ProductRequestValidator.cs`:
  - The shared `ProductRequest` (name, description, categoryId, options, variants) with
    FluentValidation rules exactly as in the contract's "Validation (400)" list.
  - The options and combination rules are left for US4. Here, require `options` to be empty and
    exactly one variant with `optionValues: []`.
- [X] T053 [US2] Create `src/MicroCommerce.ApiService/Features/Catalog/MerchantProducts/CreateProduct.cs`
  (`POST /merchant/products`):
  - Uses `CategoryRules.SaveWithCategoryLockedAsync` on the target Category.
    - An unknown Category, or one with children, adds a validation failure on `categoryId`, which
      returns 400.
  - Creates through `Product.Create` with `merchantId` from `ICurrentMerchant`.
  - Returns 201 with `Location: /api/merchant/products/{id}`.
  - Logs `"Product {ProductId} created by Merchant {MerchantId}"`.
- [X] T054 [P] [US2] Create `GetProduct.cs` and `ListProducts.cs` in
  `src/MicroCommerce.ApiService/Features/Catalog/MerchantProducts/`:
  - **List**: paging per the contract, newest first. A LINQ `Select` projection computes
    `variantCount`, `minPrice`, `maxPrice`, `totalStock` (`long`) and `coverImageUrl` (position 0,
    or null) in SQL (research R12). It reads through `MerchantCatalog`.
  - **Get**: returns 404 when `MerchantCatalog` has no match.
- [X] T055 [US2] Create `UpdateProduct.cs` (`PUT /merchant/products/{id}`) in
  `src/MicroCommerce.ApiService/Features/Catalog/MerchantProducts/`:
  - Loads through `MerchantCatalog` (404 if missing).
  - If `request.Version != product.Version`, returns 409 `product-version-conflict`.
  - If the Product is Removed, returns 409 `product-removed`.
  - A Category change goes through `CategoryRules` on the new Category.
  - A `DbUpdateConcurrencyException` also returns 409 `product-version-conflict`.
- [X] T056 [P] [US2] Create `PublishProduct.cs` and `UnpublishProduct.cs` (body `{ version }`) in
  `src/MicroCommerce.ApiService/Features/Catalog/MerchantProducts/`, with the same 404 and 409
  rules as T055.
- [X] T057 [P] [US2] Create the web pages for this story:
  - `src/MicroCommerce.Web/src/merchant/ProductList.tsx`: a table of status, price range, total
    stock and Publish/Unpublish buttons.
  - `src/MicroCommerce.Web/src/merchant/ProductForm.tsx`: name, description, a leaf-only Category
    picker from `/api/marketplace/categories`, and a single price, stock and weight. It sends
    `version` on edit. A 409 shows "This Product changed elsewhere. Reload."
  - Add `src/MicroCommerce.Web/src/lib/price.ts` (`formatPriceRange(min, max)`) with Vitest tests
    in `price.test.ts`.

**Checkpoint**: US1 and US2 work. Quickstart Scenario 3, steps 1 (without options), 5 and 6, works.

---

## Phase 5: User Story 3: Buyers browse, search and open Products across Merchants (Priority: P1)

**Goal**: Public, paginated browsing of Published Products across Merchants, with stemmed search,
a Category filter that includes descendants, Product and Shop pages, Availability (never stock),
and no Pickup Address.

**Independent Test**:

1. With Published and Draft Products from two Merchants, an anonymous visitor sees only Published
   ones.
2. Search for `mugs` finds a Product named with "mug".
3. Filtering by `Home` includes `Home › Kitchen`.
4. A Product's detail shows `availability` but no stock or weight.
5. Opening a Draft Product directly returns 404.

### Tests for User Story 3 ⚠️

- [X] T058 [US3] Write functional tests in
  `tests/MicroCommerce.FunctionalTests/Features/Catalog/MarketplaceEndpointsTests.cs`. Every
  request uses the anonymous `App.Client`.
  - **List**: spans two Merchants. Each card has `minPrice`, `maxPrice`, `available` and
    `shop.shopName`. The JSON contains no `stock`, `totalStock`, `weightGrams` or `pickupAddress`
    property (SC-004).
  - **Order and paging**: newest first; `page` and `pageSize` paginate; `pageSize=500` is clamped
    to 100; a page past the end gives empty `items` with the real `totalCount`; `page=0` returns
    400; a `query` over 200 characters returns 400.
  - **Visibility**: Draft Products and unpublished Products are absent from the list, search and
    `shopId`, and detail returns 404. A Product shows after it is published.
  - **Search**: "mugs" finds "Ceramic coffee mug"; every word must match; the description is
    searched too.
  - **Category filter**: includes descendants; an unknown `categoryId` gives an empty page; query,
    Category and paging combine.
  - **Detail**: the Category name, options, Variants with `availability` equal to stock, and
    `images: []`.
  - **Availability**: a Variant with stock 0 has `availability: 0`. A Product whose Variants are
    all 0 has `available: false`.
  - **Shop page**: 200 with `shopName` and `description` and no `pickupAddress`. An unknown Shop
    returns 404. A Shop rename shows at once on the cards (FR-032).
  - **SC-007**: a fixed set of 20 singular and plural pairs, such as mug/mugs, dress/dresses,
    shirt/shirts, box/boxes, bottle/bottles, glass/glasses, knife/knives, lamp/lamps,
    pillow/pillows, cup/cups, bag/bags, watch/watches, shoe/shoes, sock/socks, phone/phones,
    laptop/laptops, plate/plates, bowl/bowls, towel/towels and brush/brushes. Each Product is named
    with one form and searched with the other; at least 18 must be found.

### Implementation for User Story 3

- [X] T059 [US3] Create `src/MicroCommerce.ApiService/Features/Catalog/Marketplace/ProductSearch.cs`:
  - `IProductSearch` with `SearchAsync(MarketplaceQuery q, ct) → (IReadOnlyList<ProductCardRow>, int totalCount)`.
  - `PostgresProductSearch` (research R6):
    - **Visible Products**: a single `VisibleProducts` expression, `Status == Published` (FR-025).
    - **Text query**: `EF.Property<NpgsqlTsVector>(p, "SearchVector").Matches(EF.Functions.WebSearchToTsQuery("english", q))`,
      ordered by `Rank` descending, then `CreatedAt` descending, then `Id` descending. Without a
      query, `CreatedAt` descending, then `Id` descending.
    - **Category filter**: load every `(Id, ParentId)` pair and compute descendants breadth-first in
      memory.
    - **Shop filter**: `shopId` filters by `MerchantId`.
    - **Paging**: `pageSize` clamped to 100.
    - **Projection**: computes `minPrice`, `maxPrice`, `available` (any Variant with stock above 0)
      and the cover image ID.
  - Register it as scoped.
- [X] T060 [US3] Create `src/MicroCommerce.ApiService/Features/Catalog/Marketplace/ListProducts.cs`
  (`GET /marketplace/products`):
  - The validator: `page` at least 1, `pageSize` at least 1, `query` at most 200 characters.
  - Calls `IProductSearch`, then `IMerchantDirectory.GetShopsAsync` once for the page's distinct
    `MerchantId`s.
  - Maps to `ProductCard`, with `coverImageUrl` `/api/marketplace/products/{id}/images/{imageId}`
    or null.
- [X] T061 [P] [US3] Create `src/MicroCommerce.ApiService/Features/Catalog/Marketplace/GetProduct.cs`
  (`GET /marketplace/products/{id}`):
  - Uses `VisibleProducts` and returns 404 otherwise.
  - The `ProductDetail` has the Category `{id, name}`, the options, Variants with `availability`
    (equal to `Stock`, FR-030) and no stock or weight, images in position order, and the Shop from
    `IMerchantDirectory`.
- [X] T062 [P] [US3] Create `src/MicroCommerce.ApiService/Features/Catalog/Marketplace/GetShopPage.cs`
  (`GET /marketplace/shops/{id}`):
  - Looks up the Shop through `IMerchantDirectory.GetShopsAsync([id])`, and returns 404 if it is
    missing.
  - The response is `ShopPage { id, shopName, description }` only.
- [X] T063 [P] [US3] Create the Marketplace web pages in `src/MicroCommerce.Web/src/marketplace/`:
  - `Home.tsx`: a search box, a Category filter (tree select), a Product grid of cards (cover
    image, or a placeholder when `coverImageUrl` is null; price range; "Sold out" when
    `available` is false; a Shop link), and paging.
  - `ProductPage.tsx`: images, description, Category, options, and a Variant table with price and
    availability.
  - `ShopPage.tsx`: the header plus the grid filtered by `shopId`.

**Checkpoint**: The MVP (US1–US3) is complete. Quickstart Scenarios 2, 3 (without options or images)
and 4 work.

---

## Phase 6: User Story 4: A Merchant sells option combinations as Variants (Priority: P2)

**Goal**: Up to two options. The Variants are exactly the combinations of their values, and their
identities are stable across edits.

**Independent Test**:

1. Create Colour (Red, Blue) × Size (S, M, L). That gives 6 Variants.
2. Remove L. The 4 surviving Variant IDs are unchanged, and the 2 L Variants are gone.

### Tests for User Story 4 ⚠️

- [X] T064 [P] [US4] Write unit tests in
  `tests/MicroCommerce.UnitTests/Domain/Catalog/VariantCombinationsTests.cs` and extend
  `ProductTests.cs`:
  - The cartesian product order puts the last option fastest.
  - The combination key does not depend on order: swapping options keeps every ID.
  - Removing a value drops exactly its combinations. Adding a value adds new IDs. Removing all
    options leaves one default Variant.
  - Values are re-spelt to the option's spelling.
  - The rules from data-model "ProductOption" are enforced:
    - Option name: "1–50 characters. Distinct across the Product's options, ignoring case. No
      control characters".
    - Values: "1–20 values, each 1–50 characters. Distinct within the option, ignoring case".
    - At most 2 options.
- [X] T065 [P] [US4] Extend `tests/MicroCommerce.UnitTests/Features/Catalog/ProductRequestValidatorTests.cs`:
  - Three options, an empty values list, duplicate values, and duplicate option names each fail on
    `options`.
  - Missing, duplicate and unknown combinations each fail on `variants`, with the combination
    named in the message.
- [X] T066 [US4] Extend `tests/MicroCommerce.FunctionalTests/Features/Catalog/MerchantProductEndpointsTests.cs`:
  - Create a 2×3 Product, which gives 6 Variants in option order even when they are posted in a
    different order.
  - Invalid combinations return 400.
  - An edit that removes a value keeps the surviving IDs.
  - An edit that changes only price and stock keeps every ID.
  - Removing every option leaves the default Variant.
  - The list's `variantCount`, price range and `totalStock` cover every Variant.
  - The Marketplace detail lists every Variant with its option values.

### Implementation for User Story 4

- [X] T067 [P] [US4] Create `src/MicroCommerce.Domain/Catalog/ProductOption.cs`, a record
  `(string Name, IReadOnlyList<string> Values)` with the data-model rules as `DomainRuleViolation`s.
  Also create `src/MicroCommerce.Domain/Catalog/VariantCombinations.cs`:
  - `Expand(options)`, the cartesian product with the last option fastest.
  - `Key(options, values)`, the sorted upper-cased `NAME=VALUE` pairs joined with `\u001f`.
  - `InOptionSpelling`.
  - Adapt from `a2743b4b:src/MicroCommerce.ApiService/Features/Catalog/VariantCombinations.cs`.
- [X] T068 [US4] Generalise `src/MicroCommerce.Domain/Catalog/Product.cs`:
  - `Create` and `Edit` accept 0–2 options.
  - A private `SetVariants(options, specs)` checks that the specs equal `VariantCombinations.Expand`
    exactly.
  - It reuses the existing `Variant` with the same key (its ID is kept), creates new
    `VariantId.From(Guid.NewGuid())` IDs for new keys, and drops the rest.
  - It assigns `Position` in expansion order.
- [X] T069 [US4] Generalise `src/MicroCommerce.ApiService/Features/Catalog/MerchantProducts/ProductRequestValidator.cs`
  to the contract's full option and combination rules. Report them on `options` and `variants`,
  naming each missing, duplicate or unknown combination.
- [X] T070 [P] [US4] Add `src/MicroCommerce.Web/src/lib/variantGrid.ts`:
  - `buildVariantGrid(options, existingRows)` returns one row per combination in expansion order,
    keeping the price, stock and weight already entered for surviving combinations.
  - Write Vitest tests in `variantGrid.test.ts`.
  - Add an option editor (at most 2 options, values as chips) and the generated Variant grid to
    `src/MicroCommerce.Web/src/merchant/ProductForm.tsx`.
  - Add a Variant picker to `src/MicroCommerce.Web/src/marketplace/ProductPage.tsx`.

**Checkpoint**: Quickstart Scenario 3, steps 1 and 4, works with options.

---

## Phase 7: User Story 5: A Merchant adds images to a Product (Priority: P2)

**Goal**: Upload JPEG, PNG or WebP images (at most 5 MB each, at most 9 per Product), judged by
content. Reorder and remove them; the first is the cover. Images are served through the API with
visibility enforced.

**Independent Test**:

1. Upload 3 images, move the third to the front, and remove the second.
2. Publish. An anonymous visitor sees 2 images, with the moved one as the cover.
3. After unpublishing, the image URL returns 404.

### Tests for User Story 5 ⚠️

- [X] T071 [P] [US5] Write unit tests in `tests/MicroCommerce.UnitTests/Domain/Catalog/ImageFormatTests.cs`:
  - JPEG `FF D8 FF`, PNG `89 50 4E 47 0D 0A 1A 0A`, and WebP `RIFF????WEBP` are each detected.
  - A GIF, plain text, an empty file and a file shorter than 12 bytes give null.
- [X] T072 [P] [US5] Extend `tests/MicroCommerce.UnitTests/Domain/Catalog/ProductTests.cs`:
  - `AddImage` appends at the next position, and the 10th image is refused (`DomainRuleViolation`,
    mapped to `image-limit-reached`).
  - `ReorderImages` rejects anything that is not a permutation and renumbers 0..n−1.
  - `RemoveImage` renumbers.
  - Each of these bumps the version.
  - On a Removed Product, each is refused.
  - A size over 5,242,880 bytes is refused.
- [X] T073 [US5] Write functional tests in
  `tests/MicroCommerce.FunctionalTests/Features/Catalog/ProductImageEndpointsTests.cs`, against
  Azurite. Use small generated JPEG, PNG and WebP byte arrays as fixtures in
  `tests/MicroCommerce.FunctionalTests/TestImages.cs`.
  - **Upload**: 201, with the image appended. 400 on `file` for a `.txt` file named `x.jpg`, a
    file just over 5 MB, and a missing file.
  - **Limit**: a 10th image returns 409 `image-limit-reached`. Two parallel uploads at 8 images
    give exactly one 201 and one 409.
  - **Reorder**: 200, and the new order is visible on the Marketplace. A non-permutation returns
    400. A stale `version` returns 409.
  - **Remove**: 200, and the image disappears. An unknown image returns 404.
  - **Failed save**: an upload whose save fails because of the cap leaves the Product's images
    unchanged (FR-022).
  - **Marketplace image**: 200 with the right `Content-Type`, `X-Content-Type-Options: nosniff`,
    and an `ETag`. A second request with `If-None-Match` returns 304. After unpublishing it returns
    404.
  - **Merchant image**: 200 for the owner and 404 for another Merchant.
  - **Refusals**: on every route, Merchant B gets 404, an Account without a Merchant gets 403, and
    an anonymous caller gets 401.
  - **Detail**: the Marketplace detail lists images in order, and the card's `coverImageUrl` points
    to position 0.

### Implementation for User Story 5

- [X] T074 [P] [US5] Create `src/MicroCommerce.Domain/Catalog/ImageFormat.cs` and
  `src/MicroCommerce.Domain/Catalog/ProductImage.cs`:
  - `ImageFormat` is an enum with `Jpeg`, `Png` and `WebP`, a static
    `ImageFormats.Detect(ReadOnlySpan<byte> head) → ImageFormat?`, and `ContentType(format)`.
  - `ProductImage` has `Id`, `Position` ("Dense, 0..n−1. Position 0 is the cover"), `Format`,
    `SizeBytes` ("At most 5,242,880") and `UploadedAt`.
  - Add `AddImage`, `ReorderImages` and `RemoveImage` to `Product.cs` with the T072 rules.
  - Update `ProductConfiguration` and add a migration if the T050 owned-table columns differ.
- [X] T075 [US5] Create `src/MicroCommerce.ApiService/Features/Catalog/ProductImages/ProductImageStore.cs`:
  - `IProductImageStore` with `PutAsync(productId, imageId, Stream, contentType)`,
    `OpenReadAsync(productId, imageId) → (Stream, contentType)?` and
    `DeleteAsync(productId, imageId)` (best effort).
  - `BlobProductImageStore` over `BlobServiceClient`, with container `product-images` created
    through `CreateIfNotExistsAsync` once per process (a lazy `Task`). The blob name is
    `{productId}/{imageId}`.
  - In `Program.cs`, call `builder.AddAzureBlobServiceClient("blobs")` and register the store as a
    singleton.
- [X] T076 [US5] Create `src/MicroCommerce.ApiService/Features/Catalog/ProductImages/UploadImage.cs`
  (`POST /merchant/products/{id}/images`):
  - Calls `AllowFileUploads()` and `MaxRequestBodySize(6 * 1024 * 1024)`.
  - Validates the file: required, `Length` at most 5,242,880, and `ImageFormats.Detect` on the first
    12 bytes is not null. Otherwise it returns 400 on `file`.
  - Loads the Product through `MerchantCatalog` (404). A Removed Product returns 409.
  - Calls `PutAsync` with the sniffed content type, then `AddImage`, then `SaveChangesAsync`.
  - On `DomainRuleViolation` (the cap) or `DbUpdateConcurrencyException`, it calls `DeleteAsync`
    (best effort), then returns 409 `image-limit-reached` or `product-version-conflict` (FR-022).
  - Logs orphan-delete failures with
    `"Orphaned image blob {ProductId}/{ImageId} could not be deleted"`.
- [X] T077 [P] [US5] Create `ReorderImages.cs` (`PUT /merchant/products/{id}/images/order`) and
  `RemoveImage.cs` (`DELETE /merchant/products/{id}/images/{imageId}?version=n`) in
  `src/MicroCommerce.ApiService/Features/Catalog/ProductImages/`:
  - Version check: a mismatch returns 409.
  - Remove saves first, then deletes the blob (best effort, with the warning log).
- [X] T078 [US5] Create `src/MicroCommerce.ApiService/Features/Catalog/ProductImages/GetImage.cs`.
  It has three endpoints sharing one private `StreamImageAsync` helper:
  - **Marketplace** (`GET /marketplace/products/{productId}/images/{imageId}`): uses the
    `VisibleProducts` check from T059, with `Cache-Control: no-cache`.
  - **Merchant Centre** (`GET /merchant/products/{productId}/images/{imageId}`): uses
    `MerchantCatalog`, with `Cache-Control: private, no-cache`.
  - **Ops** (`GET /ops/products/{productId}/images/{imageId}`): any Product, with
    `Cache-Control: private, no-cache`.
  - All three send `ETag: "{imageId}"` (304 on a matching `If-None-Match`),
    `X-Content-Type-Options: nosniff`, and `Content-Type` from the stored format.
  - An unknown image, or one hidden from the caller, returns 404.
- [X] T079 [P] [US5] Create `src/MicroCommerce.Web/src/merchant/ImageManager.tsx`:
  - Upload through `uploadFile`, with client-side type and size pre-checks. The server stays
    authoritative.
  - Thumbnails through `useAuthImage`, move left and right, and delete. It sends `version`, and a
    409 prompts a reload.
  - Add `src/MicroCommerce.Web/src/lib/imageOrder.ts` (`moveImage(ids, from, to)`) with Vitest
    tests.
  - Embed it in `ProductForm.tsx` for existing Products.

**Checkpoint**: Quickstart Scenario 3, steps 2 and 3, works, and Scenario 4 shows images.

---

## Phase 8: User Story 6: Platform Operators maintain the Category tree (Priority: P2)

**Goal**: Platform Operators create, rename and delete Categories. Sibling names are unique
ignoring case. A Category holding Products can't gain children, and a Category with children or
Products can't be deleted.

**Independent Test**:

1. As an operator, create a top-level Category and a child, then rename the child.
2. After a Product is in the child, adding a child and deleting both return 409.
3. A non-operator gets 403.

### Tests for User Story 6 ⚠️

- [X] T080 [P] [US6] Write validator unit tests in
  `tests/MicroCommerce.UnitTests/Features/Catalog/CategoryValidatorTests.cs`: the name is required
  and at most 100 characters on `name`, for both create and rename.
- [X] T081 [US6] Write functional tests in
  `tests/MicroCommerce.FunctionalTests/Features/Catalog/CategoryEndpointsTests.cs`, with unique
  names:
  - **Create**: top level and child return 201 with `Location`. An unknown `parentId` returns 400
    on `parentId`. A blank or overlong name returns 400. A sibling name differing only in letter
    case returns 409 `category-name-taken`, including at top level. Adding a child under a Category
    holding a Product returns 409 `category-has-products`.
  - **Get**: 200, and an unknown Category returns 404.
  - **Rename**: 200; the Product stays in the Category and the Marketplace detail shows the new
    name. A sibling clash returns 409. An unknown Category returns 404.
  - **Delete**: a leaf returns 204. A Category with children returns 409 `category-has-children`.
    A Category with Products returns 409 `category-has-products`. An unknown Category returns 404.
  - **Refusals**: a Buyer or Merchant gets 403 on every route; anonymous gets 401.

### Implementation for User Story 6

- [X] T082 [US6] Create these in `src/MicroCommerce.ApiService/Features/Catalog/Categories/`,
  all in `OpsGroup`:
  - `CreateCategory.cs` (`POST /ops/categories`): for a child, it runs inside
    `CategoryRules.SaveWithCategoryLockedAsync(parentId)`, checking that the parent exists (else
    400 on `parentId`) and holds no Products (else 409 `category-has-products`). A `23505` on
    `CategoryIndexes.SiblingName` returns 409 `category-name-taken`. It returns 201 with
    `Location: /api/ops/categories/{id}`.
  - `GetCategory.cs` (`GET /ops/categories/{id}`).
- [X] T083 [P] [US6] Create `RenameCategory.cs` (`PUT /ops/categories/{id}`, with the same sibling
  `23505` mapping) and `DeleteCategory.cs` (`DELETE /ops/categories/{id}`) in
  `src/MicroCommerce.ApiService/Features/Catalog/Categories/`. Delete runs under the Category's own
  lock, and returns 409 `category-has-children` or `category-has-products`, else 204.
- [X] T084 [P] [US6] Create `src/MicroCommerce.Web/src/ops/CategoryTree.tsx`, shown only when
  `/api/me` has `isPlatformOperator`. It renders the tree with add-child, add-top-level, rename
  and delete actions, and shows a message for each 409 `type`.

**Checkpoint**: Quickstart Scenario 5, steps 1–3, works.

---

## Phase 9: User Story 7: Platform Operators remove prohibited Products (Priority: P3)

**Goal**: Operators view any Product and mark it Removed. That is final: it disappears from the
Marketplace at once, and its Merchant sees it as Removed but can't change it.

**Independent Test**:

1. Remove a Published Product. It is gone from the list, search and its Shop page, and its detail
   and image return 404.
2. The Merchant's list shows it as Removed.
3. A Merchant's edit, publish or image change returns 409 `product-removed`.

### Tests for User Story 7 ⚠️

- [X] T085 [P] [US7] Extend `tests/MicroCommerce.UnitTests/Domain/Catalog/ProductTests.cs`:
  - `Remove` works from Draft and from Published, and bumps the version.
  - Removing a Removed Product is a no-op.
  - After removal, `Edit`, `Publish`, `Unpublish` and the image methods each throw.
- [X] T086 [US7] Write functional tests in
  `tests/MicroCommerce.FunctionalTests/Features/Catalog/ProductModerationTests.cs`:
  - **Operator view**: an operator GETs a Draft of any Merchant (200, with `merchantId` and
    `shopName`). An unknown Product returns 404.
  - **Remove**: returns 200, and repeating it returns 200 (idempotent).
  - **Marketplace**: the Product leaves the list, search and its Shop's results. Its detail and
    image return 404.
  - **Merchant**: the list shows `Removed`. PUT, publish, unpublish, upload, reorder and remove
    image each return 409 `product-removed`.
  - **Refusals**: a Buyer or Merchant gets 403 on every `/ops/products` route; anonymous gets 401.
  - **Race**: a removal racing a Merchant edit always ends Removed.

### Implementation for User Story 7

- [X] T087 [US7] Add `Remove()` to `src/MicroCommerce.Domain/Catalog/Product.cs` (any status, then
  Removed; idempotent; bumps the version). Make every Merchant mutation throw `DomainRuleViolation`
  when the Product is Removed, and map that to 409 `product-removed` in the Merchant endpoints.
- [X] T088 [US7] Create `GetOperatorProduct.cs` (`GET /ops/products/{id}`) and `RemoveProduct.cs`
  (`POST /ops/products/{id}/remove`) in `src/MicroCommerce.ApiService/Features/Catalog/Moderation/`:
  - `OperatorProductResponse` is `MerchantProductResponse` plus `merchantId` and `shopName` (from
    `IMerchantDirectory`), with image URLs under `/api/ops/...`.
  - Remove takes no version. It loads, removes and saves. On a `DbUpdateConcurrencyException` it
    reloads and retries once (research R5).
  - It logs `"Product {ProductId} removed by Platform Operator {AccountId}"`.
- [X] T089 [P] [US7] Create `src/MicroCommerce.Web/src/ops/ProductModeration.tsx`: look up a Product
  by ID, view it (images through `useAuthImage`), and Remove with a confirm step. Show a "Removed by
  a Platform Operator" badge in `merchant/ProductList.tsx` and make `ProductForm.tsx` read-only for
  Removed Products.

**Checkpoint**: All 7 stories work. Quickstart Scenarios 1–5 pass.

---

## Phase 10: Polish & Cross-Cutting Concerns

- [X] T090 [P] Add the explicit performance test in
  `tests/MicroCommerce.FunctionalTests/Performance/MarketplacePerformanceTests.cs` with
  `[Fact(Explicit = true)]` and its own `PerformanceApiFixture : ApiFixture`:
  - Bulk-insert 10,000 Published Products, each with 1–6 Variants, across 50 Merchants, with
    `CatalogDbContext` or `COPY`.
  - Warm up, then time 50 mixed requests (browse, search and Category filter) and assert that
    p95 is under 1 s (SC-003).
  - Confirm the xUnit v3 Microsoft.Testing.Platform flag (`--explicit only`), using the `run-tests`
    skill, and fix the command in `quickstart.md` Scenario 6.
  - **Outcome (2026-11-05)**: passes. The flag is `dotnet test --project tests/MicroCommerce.FunctionalTests --explicit only`, and a normal run reports the test as not run.
- [X] T091 [P] Add `tests/MicroCommerce.IntegrationTests/CatalogSmokeTests.cs`. Follow the
  `MeApiTests` and `KeycloakTokens` pattern, with the AppHost run with `--EphemeralResources=true`:
  - The `merchant` user gets a real Keycloak token. Read the password from the AppHost
    `dev-merchant-password` parameter, as 001 does for `buyer`.
  - It opens a Merchant, creates a Product in Home › Kitchen, uploads a PNG and publishes.
  - An anonymous `HttpClient` then finds the Product with `query` and downloads its image (200).
  - **Outcome (2026-11-05)**: written and compiles, but not run. This machine has no Dapr CLI, so the AppHost can't start (see T007(a)).
- [X] T092 [P] Update `README.md`:
  - The `merchant` dev user and its `dev-merchant-password` parameter.
  - The Azurite `storage` resource.
  - The one-time Keycloak volume reset for developers coming from 001.
  - A link to `specs/002-product-catalog/quickstart.md`.
- [X] T093 Review every new log call for personal data. There must be no Pickup Address, email or
  Shop description, only IDs (Principle VI). Confirm that no secret or connection string appears
  in code, config or tests (Principle IV).
- [ ] T094 Run the full suite: `dotnet test --solution MicroCommerce.slnx`,
  `pnpm --dir src/MicroCommerce.Web test` and `pnpm --dir src/MicroCommerce.Web build`. All must
  pass, and `docker ps` must show no leftover test containers.
  - **Outcome (2026-11-05)**: the solution builds with 0 warnings and 0 errors.
    - 262 of 280 .NET tests pass: 159 unit and 103 functional. One test is skipped: the explicit performance test, which passes when run with `--explicit only`.
    - All 17 failures are AppHost integration tests, and every one fails with `Unable to locate the Dapr CLI`, an environment prerequisite.
    - The web app has 18 of 18 tests passing and `pnpm build` succeeds.
    - No test containers are left behind.
    - **Still open**: re-run the integration suite after `dapr init --slim`.
- [ ] T095 Run quickstart Scenarios 1–5 by hand, including timing a 5 MB upload against SC-008
  (under 10 s), and fix anything that differs. Then run `/speckit-analyze`.
  - **Outcome (2026-11-05)**: not done. `aspire run` needs the Dapr CLI, which isn't installed here. `/speckit-analyze` hasn't been run either.

---

## Dependencies & Execution Order

### Phase dependencies

- **Setup (Phase 1)** has no dependencies. T007 runs after T001 and T005.
- **Foundational (Phase 2)** depends on Setup and blocks every story.
  - T013–T020 run in order (contexts, then configurations, then registration, then the migrator,
    then migrations).
  - T025 runs after T001.
- **User stories**:

  | Story | Depends on | Reason |
  |-------|-----------|--------|
  | US1 | Foundational | Nothing else |
  | US2 | US1 | A Merchant is needed (`ICurrentMerchant`, `NewMerchantAsync`) |
  | US3 | US2 | Products are needed. `IMerchantDirectory.GetShopsAsync` comes from US1 |
  | US4 | US2 | It generalises Product and the validator. It can run in parallel with US3, US5 and US6 |
  | US5 | US2 | The Marketplace image route (T078) uses `VisibleProducts` from US3, so do T078 after T059 |
  | US6 | Foundational | Needs only Foundational (Category and `CategoryRules`). Its "has products" tests need US2 |
  | US7 | US2 | The Marketplace assertions need US3. Image assertions need US5 |

- **Polish (Phase 10)** comes after every story it covers. T091 needs US1, US2, US3 and US5.

### Within each story

- Tests first; confirm they fail.
- Then the domain, then persistence and migrations, then services, then endpoints, then the web
  app.
- Migration tasks (T020, T036, T050 and T074) touch the same `Migrations/` folders, so never run
  them in parallel with each other.

## Parallel Examples

```text
# Phase 1: after T001
T002 (web dependency) ∥ T003 (constitution) ∥ T004 (CONTEXT.md) ∥ T006 (realm)

# Phase 2: domain and test scaffolding alongside the persistence chain
T008 ∥ T009 ∥ T011 ∥ T012 ∥ T015 ∥ T022 ∥ T026 ∥ T028 ∥ T029

# US1 tests together
T030 ∥ T031 ∥ T033   (T032 in its own file)

# After US2: four stories in parallel by different developers
US3 (T058–T063) ∥ US4 (T064–T070) ∥ US5 (T071–T077, T079) ∥ US6 (T080–T084)
```

## Implementation Strategy

### MVP first (US1 → US2 → US3)

1. Phase 1 and Phase 2, including the T007 checks.
2. US1, then validate quickstart Scenario 2.
3. US2, then validate Scenario 3 with single-Variant Products.
4. US3, then validate Scenario 4. **Stop here and demo**: Merchants list Products and Buyers
   browse them.

### Incremental delivery

5. US4 (options), US5 (images) and US6 (Category management), in any order or in parallel.
6. US7 (moderation).
7. Polish: the performance and smoke tests, the README, the log and secret review, and the full
   suite.

## Notes

- Each story's checkpoint is a working, testable increment.
- Commit after each task or small group, on `002-product-catalog`. It is stacked on
  `001-project-skeleton`, so open the pull request against `master` once 001 has merged.
- Never take `merchantId` from a route or body.
- Never read `db.Products` directly in Merchant Centre code; always use `MerchantCatalog`.
- Never return stock, weight or a Pickup Address from a `/marketplace` route.

---

## Phase 11: Convergence

- [X] T096 Make `POST /merchant/products` and `PUT /merchant/products/{id}` reject a missing or null `variants[i].optionValues`, a null `variants[i]` and a null `options[i]` with a 400 on the field, not a 500. Today `ProductRequestRules` treats a null `optionValues` as `[]` for validation (`src/MicroCommerce.ApiService/Features/Catalog/MerchantProducts/ProductRequestValidator.cs:110`), but `ToDomain` passes the null on to `VariantSpec`. `VariantCombinations.Match` then throws (`src/MicroCommerce.Domain/Catalog/VariantCombinations.cs:44`). Null list items also reach `variant.OptionValues` and `o.Name` in the validator. Add validator unit tests and one functional test that posts a default Variant without `optionValues`. Per FR-018 and contracts/http-api.md "Validation (400)" (partial)
