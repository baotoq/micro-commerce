# Research: Product Catalog

**Feature**: [spec.md](./spec.md) | **Plan**: [plan.md](./plan.md) | **Date**: 2026-11-05

Every decision resolves a technical unknown in the plan's Technical Context. Most decisions reuse the
Catalog implementation from before the re-init (git `a2743b4b`, issues #99, #103, #104 and #105). The
decisions that differ are called out: Wolverine is gone, products are versioned, sibling Category
names are unique, and images are new.

Library documentation was checked through Context7 on 2026-11-05: FastEndpoints file handling,
Npgsql EF Core full-text search and index options, and Aspire Azure Storage hosting.

---

## R1. Area boundaries: how Catalog learns about Merchants

**Decision**:

- Two bounded areas.
  - **Accounts** owns Merchant, including the Shop profile and Pickup Address.
  - **Catalog** owns Category, Product, Variant and Product image.
- Catalog gets Merchant facts only through an in-process contract that Accounts publishes:
  `IMerchantDirectory`, in `Features/Accounts/MerchantDirectory.cs`. It has two operations.
  - `FindByAccountAsync(AccountId) → MerchantRef?`: the Merchant Centre uses it to resolve the
    current Merchant.
  - `GetShopsAsync(IReadOnlyCollection<MerchantId>) → IReadOnlyDictionary<MerchantId, ShopRef>`:
    browsing uses it to fetch Shop names in one batch per page.
- `MerchantId` lives in `Domain/SharedKernel`, because both areas use it. Catalog code never injects
  `AccountsDbContext`. Both areas share one assembly, so the referenced-assemblies check can't see
  this. A unit architecture test reflects over the constructor parameters and fields of every type
  in `Features.Catalog` (and the reverse for `Features.Accounts`). It uses plain reflection, not
  NetArchTest, which would be a new dependency.

**Rationale**:

- Principle II allows "a published contract or event". In the skeleton, only a Temporal activity
  calls `IIntegrationEventPublisher`. There is no transactional outbox.
- The old design kept an event-fed Shop read model. That model depended on Wolverine's EF outbox,
  which the constitution has since replaced with Dapr.
- An in-process contract needs no outbox, no inbox and no eventual consistency. Shop renames show up
  on the next view, which satisfies FR-032 and SC-006.

**Alternatives considered**:

- *Event-fed Shop read model in Catalog* (Dapr pub/sub with a new EF outbox and relay). Rejected for
  now, because it is speculative infrastructure (Principle VII).
- **When to revisit**: the Merchant suspension feature. Browsing will then have to filter out a
  Suspended Merchant's Products in search and pagination across pages. A per-page lookup can't do
  that efficiently, so a local Merchant read model will be needed. FR-025 is written so that only
  the visibility rule changes.
- *Catalog queries `accounts.merchants` directly*. Rejected, because it violates Principle II.

## R2. Persistence layout

**Decision**:

- Add two DbContexts beside the skeleton's `AppDbContext`, which stays in the `public` schema for
  Diagnostics and the inbox.
  - `AccountsDbContext`, schema `accounts`, in `Features/Accounts/Data/`.
  - `CatalogDbContext`, schema `catalog`, in `Features/Catalog/Data/`.
- Each context keeps its own `__EFMigrationsHistory` in its own schema, has its own design-time
  factory and has its own `Migrations/` folder.
- Each context applies only the entity configurations in its own namespace. This also fixes
  `AppDbContext`, whose `ApplyConfigurationsFromAssembly` would otherwise pick up the new
  configurations.
- `DatabaseMigrator` migrates all three contexts in order, runs `CatalogSeeder` (Development only),
  and only then calls `MarkApplied()`. `ApiFixture` already waits for that, so no test can create a
  Category before the seed has run.
- Every new Vogen ID gets an `[EfCoreConverter<>]` entry in `VogenEfCoreConverters` and an entry in
  `VogenIdSchemaTransformer`: `MerchantId`, `CategoryId`, `ProductId`, `VariantId` and
  `ProductImageId`.

**Rationale**: Each area owns its data (Principle II). This is the old module layout, and it is a
proven pattern in this repository (Principle VII).

**Alternatives considered**: One shared context with several schemas. Rejected, because one context
lets any feature query any table, so ownership could no longer be checked.

## R3. Merchant rules: Shop name uniqueness and one Merchant per Account

**Decision**:

- `Merchant` is an aggregate in `Domain/Accounts`.
  - `ShopName` is a Vogen string value object: trimmed, 3 to 50 characters.
  - The aggregate keeps a `NormalizedShopName` (`ToUpperInvariant()`) under a unique index.
  - A unique index on `AccountId` enforces one Merchant per Account. There is no Account table,
    because the Account is the Keycloak `sub`, as in 001.
- The open flow checks both rules first, for friendly errors. It then maps a Postgres `23505` to a
  409 by constraint name, which covers concurrent opens. The index names are constants.
- `PickupAddress` is a value object with Street (at most 200 characters), District (at most 100) and
  Province (at most 100), all required. It is stored as EF complex-property columns.
- The old design also had a Ward field, which the spec dropped (FR-004).
- `Merchant.Version` is the concurrency token for Shop edits.

**Rationale**: This is the old behaviour, which the old concurrency tests covered. It avoids
needing a citext extension or a collation.

**Alternatives considered**:

- *citext column*. Rejected, because it needs a Postgres extension and the gain is small.
- *Case-insensitive collation*. Rejected, because Npgsql unique indexes on nondeterministic
  collations have caveats.

## R4. Category tree

**Decision**:

- The tree is an adjacency list: `Category(Id, Name, NormalizedName, ParentId?)`.
  - A Category's parent never changes, because moving Categories is not in the spec.
  - The foreign keys from parent to child and from Product to Category are `Restrict`.
- Sibling names must be unique, ignoring case. This is new (FR-034). It uses a unique index on
  `(ParentId, NormalizedName)` with `AreNullsDistinct(false)`, which emits `NULLS NOT DISTINCT`
  (PostgreSQL 15+, and the local database is PG 17). Top-level siblings are therefore unique too. A
  `23505` maps to a 409.
- The leaf rule (FR-035) under concurrency is the old `SELECT … FOR UPDATE` on the target Category
  row, taken inside an execution-strategy transaction. These operations take it:
  - adding a child (locks the parent)
  - creating a Product, or moving a Product to another Category (locks the target)
  - deleting a Category (locks it)
- The check runs again on a transient retry, so it only reads and stages changes.
- Descendants for the browse filter are computed in memory from every `(Id, ParentId)` pair. The
  tree is kept small by Platform Operators (fewer than 1,000 nodes). It is loaded per request and
  not cached.
- The starter tree is seeded in Development by `CatalogSeeder`, which `DatabaseMigrator` calls after
  migrations and before `MarkApplied()` (R2). It seeds only when no Categories exist. Its contents:
  - Fashion › Men › (T-Shirts, Shirts)
  - Fashion › Women › (Dresses, Tops)
  - Home › (Kitchen, Bedding)
  - Electronics › (Phones, Laptops)

**Rationale**: This reuses the old logic. The row lock serialises exactly the two operations that
can race.

**Alternatives considered**:

- *ltree or a materialised path*. Rejected, because the tree is small and nothing moves.
- *Recursive CTE*. Rejected, because it is more SQL for no measurable gain at this size.
- *Serializable isolation*. Rejected, because it causes more retries and is harder to test.

## R5. Product, options, Variants and versioning

**Decision**:

- `Product` is an aggregate in `Domain/Catalog`, with these parts:
  - `MerchantId`, `Name` (at most 200 characters) and `Description` (at most 5,000, blank becomes
    null)
  - `CategoryId` and `Status` (Draft, Published or Removed)
  - `Options` (at most 2), stored as a jsonb complex collection
  - `Variants`, an owned table
  - `Images`, an owned table
  - `CreatedAt` and `Version`
- **Options and values**:
  - Each option has 1 to 20 values, each at most 50 characters.
  - Option names, and the values within an option, are distinct ignoring case.
  - Control characters are rejected.
- **Variants**:
  - The Variants must be exactly the cartesian product of the option values. Without options there
    is one default Variant.
  - The validator reports missing, duplicate and unknown combinations, and names the field of each.
- **Variant identity (FR-013)**:
  - A combination key is built from the sorted, upper-cased `name=value` pairs. It does not depend
    on order, so reordering options keeps every Variant ID.
  - `Product.SetVariants` reuses the Variant with a matching key, creates new IDs for new
    combinations and drops the rest. A dropped ID is never reused, because IDs are new GUIDs.
- **Money**:
  - A Vogen `long` in whole units of the Marketplace currency.
  - The price must be greater than 0 and at most 9,007,199,254,740,991, so it stays exact in
    JavaScript.
  - Total stock is a `long`.
- **Versioning (FR-019)** is new compared with the old code:
  - `Product.Version` is an `int`, and every mutating method bumps it (edit, publish, unpublish,
    remove, add, reorder or remove an image). It is EF's concurrency token.
  - The client sends `version` in the body of edit, publish, unpublish and image reorder or removal.
    A mismatch, or a `DbUpdateConcurrencyException`, returns **409** with
    `type: product-version-conflict`.
  - Image upload takes no client version. The server loads the Product, adds the image and saves
    under the token. A concurrent change gives a 409, which also enforces the 9-image cap.
- **Status**:
  - Draft ⇄ Published, by the Merchant. Publishing or unpublishing twice is a no-op that does not
    bump the version.
  - Any status → Removed, by a Platform Operator. Removed is final and idempotent.
  - A Removed Product refuses every Merchant mutation with 409 `type: product-removed`.
  - A Platform Operator's removal racing a Merchant's edit is settled by the version token, which
    replaces the old guarded `ExecuteUpdate`.

**Rationale**: `xmin` alone would miss changes that touch only the Variants or Images tables. An
explicit aggregate version covers the whole aggregate (one aggregate per transaction, Principle II).

**Alternatives considered**:

- *`xmin` concurrency token*. Rejected, because of the owned-table blind spot.
- *ETag and `If-Match` headers*. Rejected for now. A `version` field in the body is simpler for the
  SPA and functional tests, and it is easy to move to a header later.

## R6. Browsing and full-text search

**Decision**:

- **Search vector**: a generated shadow column, so the domain type never sees it:
  `Property<NpgsqlTsVector>("SearchVector").IsGeneratedTsVectorColumn("english", ["Name",
  "Description"])`, with a GIN index.
  - Queries use `EF.Property<NpgsqlTsVector>(p, "SearchVector")`.
  - The lambda overload `HasGeneratedTsVectorColumn` needs a CLR property, so it isn't used.
- **Matching**: `p.SearchVector.Matches(EF.Functions.WebSearchToTsQuery("english", q))`. Every word
  must match.
- **Ordering**: by `Rank` descending, then `CreatedAt` descending, then `Id` descending. Without a
  query, by `CreatedAt` descending, then `Id` descending.
- **Interface**: the search sits behind `IProductSearch`, so a dedicated search engine can replace
  Postgres later.
- **Visible Products**: `Status == Published`. This is the single rule FR-025 refers to.
- **Paging**:
  - `page` is 1-based, default 1. `pageSize` defaults to 20. `query` is at most 200 characters.
  - A `pageSize` above 100 is clamped to 100, and a page past the end returns an empty `items` list
    (spec edge cases).
  - Only `page` below 1, `pageSize` below 1, or an overlong `query` return 400.
  - The response shape is `{items, page, pageSize, totalCount}`.
- **Availability** equals `Stock` (FR-030) and is computed in the query as `availability`.
  `available` on a list entry is true when any Variant has Availability above 0.
- **SC-003** (10,000 Products, p95 under 1 s): an xUnit v3 **explicit** test (`[Fact(Explicit =
  true)]`) bulk-inserts 10,000 Published Products and checks the p95 of 50 mixed browse, search
  and filter requests.
  - Explicit tests are skipped by a normal `dotnet test` and run only with the xUnit
    Microsoft.Testing.Platform option `--explicit only`. Confirm the exact flag in the first test
    task.
  - It runs against its own isolated `ApiFixture` subclass, so its 10,000 rows don't affect other
    tests.
- **SC-007** (plurals and singulars): a functional test runs a fixed set of 20 singular and plural
  pairs, such as mug/mugs, dress/dresses, shirt/shirts and box/boxes. Each Product is named with one
  form and searched with the other, and at least 18 of the 20 must be found.

**Rationale**: The provider already in the stack supports all of this (Npgsql EF Core). No new
dependency is needed, and stemming meets SC-007.

**Alternatives considered**:

- *Trigram (`pg_trgm`)*. Rejected, because it doesn't stem and needs an extension.
- *A dedicated engine (Elasticsearch, Meilisearch)*. Rejected, because it is a new dependency and
  not justified at this scale.

## R7. Product images: storage, upload and serving

**Decision**:

- **Store**: Azure Blob Storage, approved by the user on 2026-11-05.
  - In the AppHost: `builder.AddAzureStorage("storage").RunAsEmulator(...)`, which runs Azurite,
    then `.AddBlobs("blobs")`.
  - Outside ephemeral runs, the emulator gets a data volume and a persistent lifetime, matching the
    other resources.
  - The ApiService calls `builder.AddAzureBlobServiceClient("blobs")` (Aspire client integration).
  - Packages: `Aspire.Hosting.Azure.Storage` 13.6.0 and `Aspire.Azure.Storage.Blobs` 13.6.0. Both
    exist on NuGet and match the other Aspire packages.
- **Container**: `product-images`, created by the app with `CreateIfNotExistsAsync` on first use,
  through `ProductImageStore`. Functional tests don't run the AppHost, so the app can't rely on the
  AppHost to create it.
- **Upload**:
  - `POST /api/merchant/products/{id}/images`, multipart with one `file` field.
  - The endpoint uses `AllowFileUploads()` and `MaxRequestBodySize(6 MB)`. The extra 1 MB above the
    5 MB image limit leaves room for multipart overhead, and the 5 MB limit itself is checked on
    the file.
  - **Content check (FR-020)**: the first 12 bytes are compared with known signatures.
    - JPEG: `FF D8 FF`
    - PNG: `89 50 4E 47 0D 0A 1A 0A`
    - WebP: `RIFF` followed by `WEBP` at offset 8
  - The declared content type and file name are ignored. The stored content type comes from the
    sniffed format. This is roughly 20 lines in `ImageFormat.cs`, with no image library.
- **Blob name**: `{productId}/{imageId}`, with `Content-Type` set from the sniffed format.
- **Atomicity (FR-022)**:
  - Upload: write the blob first, then save the aggregate. If the save fails, for example on a
    version conflict or the cap, the blob is deleted on a best-effort basis.
  - Removal: save the aggregate first, then delete the blob on a best-effort basis.
  - An orphaned blob is never referenced, so it is never shown. Orphans are logged at warning level
    with `ProductId` and `ImageId`, and no personal data. Cleaning them up is out of scope.
- **Serving**: images are streamed through the API, never as direct blob or SAS URLs.
  - The public route `GET /api/marketplace/products/{productId}/images/{imageId}` applies the
    visibility rule (FR-025: 404 unless the Product is Published).
  - The Merchant Centre route `GET /api/merchant/products/{productId}/images/{imageId}` is limited
    to the owning Merchant. Platform Operators use `GET /api/ops/products/{productId}/images/{imageId}`,
    which ignores status and ownership.
  - Responses carry `Cache-Control: no-cache` (Merchant and ops routes: `private, no-cache`) and an
    `ETag` equal to the image ID, because an image ID's content never changes.
    - While the image is visible, revalidation is a cheap 304.
    - Once the Product is unpublished or removed, the next request returns 404, so the image leaves
      the Marketplace at once.
  - Every image response also sends `X-Content-Type-Options: nosniff`. Only the first bytes are
    checked, and the images are served from the same origin as the SPA.
- **Cover**: the image at position 0. Positions are dense, 0 to n−1, and are renumbered on removal
  or reorder.

**Rationale**:

- Uploading through the API is the only way to judge content before anything is attached.
- Serving through the API enforces visibility without signed URLs. The repository's history shows
  bugs with Azurite path-style SAS URLs (commit `f4309ef7`).
- It also keeps the browser away from emulator hostnames and CORS.

**Alternatives considered**:

- *Direct-to-blob SAS upload*. Rejected, because the content can't be validated before it is stored
  and because of the SAS bugs above.
- *Postgres `bytea`*. Rejected by the user.
- *MinIO*. Rejected by the user.

## R8. Integration tests against Azurite

**Decision**:

- `ApiFixture` starts an Azurite container next to Postgres and Temporal, and sets
  `ConnectionStrings:blobs` to its connection string, the same way it sets the database connection
  string.
- The **recommended** way to start it is `Testcontainers.Azurite` 4.15.0. It matches the existing
  `Testcontainers.PostgreSql` 4.15.0, and its `GetConnectionString()` keeps the emulator's
  well-known account key out of our code.
- This package is **pending approval**.

**Fallback** if it is not approved: a generic `ContainerBuilder`, like the Temporal container
today, that reads the account key from the Azurite image's documented default. That needs a
constant in test code. It is a public emulator value, not a secret, but the security rule says not
to put key-shaped literals in code, so the package is preferred.

## R9. Merchant Centre scoping and authorisation

**Decision**: There are four FastEndpoints route groups under the `api` prefix.

| Group | Prefix | Auth |
|-------|--------|------|
| Account | `/me` | Signed-in Account |
| Merchant Centre | `/merchant` | Signed-in Account that owns a Merchant |
| Marketplace | `/marketplace` | `AllowAnonymous()` |
| Platform operations | `/ops` | `PlatformOperator` policy |

- The Merchant Centre group's `RequireMerchant` pre-processor resolves `ICurrentMerchant`
  (request-scoped) through `IMerchantDirectory.FindByAccountAsync(currentAccount.Id)`.
  - Anonymous callers get 401. Signed-in Accounts with no Merchant get 403.
- `MerchantCatalog` exposes `Products`, which is `db.Products.Where(p => p.MerchantId ==
  current.Id)`. Every Merchant Centre read and write goes through it, so another Merchant's IDs
  return **404**.
- No route or body ever carries a `merchantId` (ADR-50, Principle IV).
- The `/marketplace` prefix follows the glossary, which avoids "storefront". The old routes were
  `/storefront`.

## R10. Web app routing and pages

**Decision**:

- Add `react-router` 8.4.0, released 2026-09-15, in library ("declarative") mode. It is **pending
  approval**. The 001 plan chose no router, and this reverses that: see Complexity Tracking.
- Pages:
  - Marketplace home: search box, Category filter and Product grid
  - Product page and Shop page
  - Open your Shop
  - Merchant Centre: Shop profile, Product list, and the Product form with an option editor,
    Variant grid and image manager
  - Platform Operator area: Category tree editor, and Product lookup and removal
- Forms use plain React state. Data fetching uses the existing `api.ts` and `fetch` helpers. There
  is no query library.
- **Authenticated images**: the SPA sends its token as a bearer header, and an `<img src>` request
  can't carry that header. The Merchant Centre and Platform Operator areas therefore load images from
  `/api/merchant/...` and `/api/ops/...` through `lib/authImage.ts`. It runs `fetch` with the token
  and shows the result through `URL.createObjectURL`, revoking each URL when the component unmounts.
  Only `/api/marketplace/...` image URLs are used directly as `src`.
- Vitest covers the pure helpers: the Variant grid builder (cartesian product), the price-range
  format, the image reorder logic, and `authImage` (with `fetch` stubbed).

**Fallback** if `react-router` is not approved: a hand-written router of about 60 lines on the
History API (`pathname` matching plus `popstate`), in `src/router.tsx`.

## R11. Seeding and identity

**Decision**:

- Add a `merchant` user to `Realms/micro-commerce-realm.json`, with password placeholder
  `${MC_MERCHANT_PASSWORD}`. The AppHost generates the secret parameter `dev-merchant-password` and
  passes it through `WithEnvironment`. This user starts without a Merchant and opens one through
  the flow.
- Developers with an existing persistent Keycloak volume must delete it once, so the realm is
  imported again. Quickstart documents this.
- `GET /api/me` gains a nullable `merchant: { merchantId, shopName }`. The change is additive, so
  001's `MeEndpointTests` keep passing.

## R12. Mapping and projections

**Decision**:

- Mapster maps aggregates to responses outward, as in 001 R3.
- List queries that aggregate over Variants project with explicit LINQ `Select`, so the database
  computes `MinPrice`, `MaxPrice`, `TotalStock`, `Available` and `CoverImageId`. Mapster
  `ProjectToType` can't express these aggregates.
- `MappingConfigTests` keeps compiling every registered mapping.

## R13. Dependencies and governance

| Item | Status |
|------|--------|
| `Aspire.Hosting.Azure.Storage` 13.6.0 (AppHost) | Approved 2026-11-05 (user chose Azure Blob through Azurite) |
| `Aspire.Azure.Storage.Blobs` 13.6.0 (ApiService) | Approved 2026-11-05 |
| `Testcontainers.Azurite` 4.15.0 (FunctionalTests) | **Pending approval** (R8 fallback exists) |
| `react-router` 8.4.0 (web) | **Pending approval** (R10 fallback exists) |
| Constitution 1.0.0 → **1.1.0** (MINOR): Technology Constraints gains "Object storage: Azure Blob Storage, with Azurite locally through Aspire" | Done in its own task, in the same pull request |
| `CONTEXT.md`: add **Option**, **Option value** and **Product image**; mark **Pickup Address** as a term (street, district, province); remove "option" from **Variant**'s _Avoid_ list, which would otherwise clash with Option | Done in the same change (Principle I) |

## R14. Test strategy (Principle III)

**Decision**:

- **Unit tests** need no infrastructure. They cover:
  - `ShopName`, `PickupAddress`, `Merchant`, `Category` rules and `Money`
  - `Product`: status transitions, version bumps, the 9-image cap, image order, Variant
    combinations and ID stability
  - `ImageFormat` sniffing
  - every validator
  - Mapster compilation
  - architecture rules: the Domain project, and Catalog not touching `AccountsDbContext`
- **Functional tests** use `ApiFixture` with Postgres, Temporal and Azurite.
  - Every operation gets a success test and an authorisation-refusal test (401, and 403 or 404).
  - Marketplace operations are public by design (Principle IV's explicit public marking). Their
    "refusal" test is the visibility test instead: Draft or Removed returns 404 or is absent.
  - Concurrency tests:
    - concurrent Merchant opens (the same Account, and the same name)
    - a Product create racing an add-child on the same Category
    - two Product edits with the same version, where one gets 409
    - two uploads at 8 images, where one gets 409
  - Data is unique per test (`$"{prefix} {Guid}"`). Tests never assume an empty Category tree,
    because Development seeds one.
  - Merchant test identities come from `TestActor.Buyer()` plus a helper, `NewMerchantAsync`, that
    opens a Merchant.
- **Integration tests** (AppHost):
  - The seeded `merchant` signs in through Keycloak, opens a Merchant, creates a Product, uploads an
    image and publishes the Product.
  - An anonymous client then finds the Product by search and downloads its image.
- **Web tests**: Vitest for the helpers in R10.
- **Performance**: the SC-003 test from R6, opt-in.
