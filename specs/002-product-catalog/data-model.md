# Data Model: Product Catalog

**Feature**: [spec.md](./spec.md) | **Research**: [research.md](./research.md)

The domain types live in `src/MicroCommerce.Domain`, which references only Vogen. Table names are
snake_case, like 001's `sample_processes` and `processed_events`: `accounts.merchants`,
`catalog.categories`, `catalog.products`, `catalog.variants` and `catalog.product_images`. Index-name
constants that the code maps `23505` from (for example `ix_merchants_normalized_shop_name`) follow
the same style. EF Core mapping
lives in each area's `Data/` folder in the ApiService. Every ID is a Vogen value object over `Guid`
(Principle II). In the tables below, a "VO" column is a Vogen value object.

## Shared kernel (`Domain/SharedKernel`)

| Type | Kind | Notes |
|------|------|-------|
| `AccountId` | Vogen `Guid` | Exists in 001. It is the Keycloak `sub`. |
| `MerchantId` | Vogen `Guid` | New. Both Accounts and Catalog use it (research R1). |

---

## Accounts area (`Domain/Accounts`, schema `accounts`)

### Merchant (aggregate)

| Field | Type | Rules |
|-------|------|-------|
| `Id` | `MerchantId` | Generated on open |
| `AccountId` | `AccountId` | The owner. **Unique** (`ix_merchants_account_id`, FR-002) |
| `ShopName` | `ShopName` (VO, string) | Trimmed, 3–50 characters (FR-003) |
| `NormalizedShopName` | string | `ShopName.ToUpperInvariant()`. **Unique** (`ix_merchants_normalized_shop_name`) |
| `Description` | string? | At most 2,000 characters. Blank becomes null |
| `PickupAddress` | `PickupAddress` (VO, complex property) | Required |
| `CreatedAt` | `DateTimeOffset` | From the injected `TimeProvider` |
| `Version` | int | Concurrency token. Bumped by `ChangeProfile` |

**Methods**:

- `Merchant.Open(id, accountId, shopName, description, pickupAddress, now)`
- `ChangeProfile(shopName, description, pickupAddress)`: changing only the letter case of the
  Merchant's own name is allowed.

The uniqueness of the Shop name and of the owning Account is checked by the handler, and finally
by the unique indexes, whose `23505` maps to 409 (research R3).

### PickupAddress (value object)

| Field | Rules |
|-------|-------|
| `Street` | Required, at most 200 characters |
| `District` | Required, at most 100 characters |
| `Province` | Required, at most 100 characters. Later used for Shipping Fees |

The Pickup Address is never returned by any Marketplace operation (FR-031).

### Published contract (`Features/Accounts/MerchantDirectory.cs`)

```text
IMerchantDirectory
  FindByAccountAsync(AccountId) → MerchantRef?            // MerchantRef(MerchantId Id, string ShopName)
  GetShopsAsync(IReadOnlyCollection<MerchantId>) → IReadOnlyDictionary<MerchantId, ShopRef>
                                                         // ShopRef(MerchantId Id, string ShopName, string? Description)
```

This is the only way Catalog code learns about Merchants (research R1).

---

## Catalog area (`Domain/Catalog`, schema `catalog`)

### Category (aggregate)

| Field | Type | Rules |
|-------|------|-------|
| `Id` | `CategoryId` | |
| `Name` | string | Trimmed, 1–100 characters |
| `NormalizedName` | string | Upper-case invariant |
| `ParentId` | `CategoryId?` | Set at creation and never changed. The foreign key is `Restrict` |

**Indexes**: unique `(ParentId, NormalizedName)` with `NULLS NOT DISTINCT`, so sibling names are
unique, including top-level siblings (FR-034).

**Rules** (FR-033–036): every rule that reads Products runs under a `SELECT … FOR UPDATE` row lock
(research R4).

- A child may not be added to a Category that holds Products.
- A Category with children or Products may not be deleted.
- A Product may sit only in a leaf.

### Product (aggregate)

| Field | Type | Rules |
|-------|------|-------|
| `Id` | `ProductId` | |
| `MerchantId` | `MerchantId` | Indexed. Never taken from the request (FR-016) |
| `Name` | string | Trimmed, 1–200 characters. Not unique |
| `Description` | string? | At most 5,000 characters. Blank becomes null |
| `CategoryId` | `CategoryId` | Must be a leaf. The foreign key is `Restrict` |
| `Status` | `ProductStatus` | `Draft`, `Published` or `Removed` |
| `Options` | `List<ProductOption>` | jsonb. 0–2 options |
| `Variants` | `List<Variant>` | Owned table `catalog.variants`. At least 1 |
| `Images` | `List<ProductImage>` | Owned table `catalog.product_images`. 0–9 |
| `CreatedAt` | `DateTimeOffset` | |
| `Version` | int | Concurrency token. Bumped by every mutating method (FR-019) |
| `SearchVector` | `NpgsqlTsVector` | **Generated** from Name and Description with the `english` config. GIN index. Persistence only: it is a shadow property, so the domain never sees it |

**Indexes**:

- `(MerchantId, CreatedAt DESC)` for the Merchant's own list
- `(Status, CreatedAt DESC)` for browsing
- `(CategoryId)`
- GIN on `SearchVector`

### ProductOption (value object, stored as jsonb)

| Field | Rules |
|-------|-------|
| `Name` | 1–50 characters. Distinct across the Product's options, ignoring case. No control characters |
| `Values` | 1–20 values, each 1–50 characters. Distinct within the option, ignoring case |

### Variant (entity inside Product)

| Field | Type | Rules |
|-------|------|-------|
| `Id` | `VariantId` | Stable while its combination survives an edit (FR-013) |
| `Position` | int | Option order, with the last option varying fastest |
| `OptionValues` | `string[]` (`text[]`) | One value per option, spelt as the option spells it. Empty for the default Variant |
| `Price` | `Money` (VO, long) | Greater than 0 and at most 9,007,199,254,740,991. Whole currency units (FR-012) |
| `Stock` | int | 0 or more |
| `WeightGrams` | int | Greater than 0 |

**Combination rule (FR-011)**: the set of `OptionValues` equals the cartesian product of the option
values, with each combination present once. The combination key is the sorted upper-cased
`NAME=VALUE` pairs, so it does not depend on order (research R5).

### ProductImage (entity inside Product)

| Field | Type | Rules |
|-------|------|-------|
| `Id` | `ProductImageId` | It is also part of the blob name `{productId}/{imageId}` |
| `Position` | int | Dense, 0..n−1. Position 0 is the cover (FR-021) |
| `Format` | `ImageFormat` | `Jpeg`, `Png` or `WebP`, judged from the content (FR-020) |
| `SizeBytes` | long | At most 5,242,880 |
| `UploadedAt` | `DateTimeOffset` | |

### Product state transitions

```text
          create
            │
            ▼
        ┌───────┐  Publish (Merchant)   ┌───────────┐
        │ Draft │ ────────────────────▶ │ Published │
        │       │ ◀──────────────────── │           │
        └───┬───┘  Unpublish (Merchant) └─────┬─────┘
            │ Remove (Platform Operator)      │ Remove (Platform Operator)
            ▼                                 ▼
                       ┌─────────┐
                       │ Removed │  final: every Merchant mutation → 409 product-removed
                       └─────────┘
```

| Method | Allowed from | Effect |
|--------|--------------|--------|
| `Product.Create(...)` | | Draft, version 1, one default Variant or the given combinations |
| `Edit(name, description, categoryId, options, variants)` | Draft, Published | Keeps the status and the surviving Variant IDs. Bumps the version |
| `Publish()` | Draft (Published is a no-op) | Becomes Published. Bumps the version |
| `Unpublish()` | Published (Draft is a no-op) | Becomes Draft. Bumps the version |
| `AddImage(id, format, size, now)` | Draft, Published | Fewer than 9 images. Appended at the end. Bumps the version |
| `ReorderImages(orderedIds)` | Draft, Published | Must be a permutation of the current IDs. Bumps the version |
| `RemoveImage(id)` | Draft, Published | Renumbers the positions. Bumps the version |
| `Remove()` | Any (Removed is a no-op) | Becomes Removed. Bumps the version |

A broken rule throws `DomainRuleViolation`. The existing `DomainRuleViolationHandler` turns it into a
ProblemDetails response (the contract lists the status codes).

### Derived values (never stored)

| Value | Definition |
|-------|------------|
| `Availability(Variant)` | `Stock`, until the Reservations feature subtracts active Reservations (FR-030) |
| `available(Product)` | Any Variant has Availability above 0 |
| `minPrice`, `maxPrice` | Lowest and highest Variant `Price` |
| `totalStock` (`long`) | Sum of Variant `Stock`. Shown only in the Merchant Centre |
| `coverImageId` | The Image at position 0, or null when there are none (the web app shows a placeholder) |
| Visible to Buyers | `Status == Published` (FR-025). The Merchant suspension feature will add a filter on the Merchant |

## Blob storage (`product-images` container)

| Blob name | Content type | Lifetime |
|-----------|--------------|----------|
| `{productId}/{imageId}` | From the sniffed format (`image/jpeg`, `image/png` or `image/webp`) | Written before the aggregate is saved. Deleted after an image is removed, or after a failed save, on a best-effort basis. Orphans are logged, never served (research R7) |

## Glossary additions (`CONTEXT.md`, same change)

- **Option**: a named way a Product varies (for example Colour), with a list of option values.
  _Avoid_: attribute, variation.
- **Option value**: one allowed value of an Option (for example Red). A Variant picks one value per
  Option.
- **Product image**: a picture of a Product, at a position in the Product's ordered images. The
  first is the cover. Images belong to the Product, not to a Variant.
- **Pickup Address**: becomes its own term (street, district and province). It is already
  mentioned under Merchant.
- **Variant** (edited): its _Avoid_ list currently includes "option", which would clash with the new
  term. It becomes "_Avoid_: SKU, model. A Variant is one combination of Option values, not an
  Option."
