# Quickstart and Validation: Product Catalog

This guide shows how to check that the Catalog meets [spec.md](./spec.md). Endpoint details are in
[contracts/http-api.md](./contracts/http-api.md), and the rules are in
[data-model.md](./data-model.md). The prerequisites and one-time setup are the same as in
[001 quickstart](../001-project-skeleton/quickstart.md#prerequisites).

## One-time setup for this feature

```bash
pnpm --dir src/MicroCommerce.Web install          # picks up react-router, if approved (research R10)
```

The dev realm gains a `merchant` user (research R11). Keycloak imports a realm only when it doesn't
already have one, so a developer with a persistent Keycloak volume from 001 must delete the volume
once:

```bash
# with aspire run stopped and the persistent keycloak container removed:
docker volume ls | grep -i keycloak    # find it, then: docker volume rm <name>  (Podman: podman volume …)
```

## Scenario 1: Start everything, with image storage

```bash
aspire run
```

Expected, in addition to 001 Scenario 1:

- The dashboard shows the `storage` resource (Azurite) and `blobs` as Running.
- The `apiservice` logs show migrations applied for the `public`, `accounts` and `catalog` schemas.
- In Development, the starter Category tree is seeded (Fashion, Home and Electronics).
- The dashboard's **Parameters** view lists `dev-merchant-password`.

## Scenario 2: Open a Merchant (User Story 1, FR-001–FR-006)

1. Sign in to the web app as `merchant`, with the password in `dev-merchant-password`.
2. The home page offers **Open your Shop**. Enter the Shop name `Sunny Mugs`, a description and a
   Pickup Address (street, district, province), then submit.
3. Expected: the web app now shows **Merchant Centre**, and `GET /api/me` includes
   `merchant.shopName = "Sunny Mugs"`.
4. Negative checks:
   - Submit **Open your Shop** again. Expected: 409 `merchant-exists`.
   - Sign in as `buyer` and try `sunny mugs`. Expected: 409 `shop-name-taken`.
5. In the Merchant Centre, edit the Shop name. Expected: the Shop page shows the new name at once.

## Scenario 3: List and publish a Product (User Stories 2, 4 and 5)

1. In the Merchant Centre, choose **New Product**. Enter a name and description, and pick
   **Home › Kitchen**.
   - Add the option Colour (Red, Blue) and the option Size (S, M, L). Expected: a grid of 6 Variant
     rows appears.
   - Fill in price, stock and weight for each row and save. Expected: the Product is **Draft**.
2. Upload 3 images: a JPEG, a PNG and a WebP file, each under 5 MB.
   - Drag the third image to the front, and delete the second one.
   - Expected: 2 images remain, and the moved image is the cover.
3. Negative checks:
   - Upload a `.txt` file renamed to `.jpg`. Expected: 400 on `file`.
   - Upload a 6 MB image. Expected: 400.
   - Upload images until there are 10. Expected: the 10th returns 409 `image-limit-reached`.
4. Remove the value L from Size and save. Expected: 4 Variants remain, and their IDs (visible in the
   Scalar response) are unchanged.
5. Open the Product in two tabs, save in tab A, then save in tab B. Expected: tab B gets 409 and
   asks you to reload.
6. Choose **Publish**. Expected: the status is **Published**.

## Scenario 4: Browse as a visitor (User Story 3, FR-024–FR-032)

Sign out, or use a private window.

1. On the Marketplace home, the Product appears with its cover image, its price range, "available"
   and "Sunny Mugs".
2. Search for `mugs` when the Product name contains "mug". Expected: it is found (SC-007).
3. Filter by **Home**. Expected: the Product is listed, because it sits in Home › Kitchen.
4. Open the Product. Expected: the images are in order, and the 4 Variants show price and
   **availability**. No stock or weight is shown.
5. Open the Shop page. Expected: the Shop name and description, with no Pickup Address.
6. In the Merchant Centre, unpublish the Product. Reload the Marketplace. Expected: the Product is
   gone, and both its page and its image URL return 404.

Anonymous API checks:

```bash
curl -s "<apiservice-url>/api/marketplace/products?query=mugs" | jq '.items[0]'   # no stock fields
curl -i "<apiservice-url>/api/merchant/products"                                    # 401
```

## Scenario 5: Platform Operator (User Stories 6 and 7)

1. Sign in as `operator`. In the Platform Operator area, create **Home › Garden**, then rename it
   **Home › Garden & Patio**.
2. Try to create a second top-level Category named `home`. Expected: 409 `category-name-taken`.
3. Try to add a child under **Home › Kitchen**, which holds the Product. Expected: 409
   `category-has-products`.
4. Look up the Product by ID and choose **Remove**. Expected:
   - It disappears from the Marketplace.
   - The Merchant's list shows it as **Removed**.
   - Edit and Publish in the Merchant Centre return 409 `product-removed`.
5. As `buyer`, any `/api/ops/*` call returns 403.

## Scenario 6: Tests

```bash
dotnet test --project tests/MicroCommerce.UnitTests              # no infrastructure
dotnet test --solution MicroCommerce.slnx                        # all .NET suites; Azurite is started by the fixture
pnpm --dir src/MicroCommerce.Web test                            # Variant grid, price range, image order helpers
```

The SC-003 performance check is opt-in: it is an xUnit v3 explicit test. It inserts 10,000
Published Products and reports the p95 latency:

```bash
dotnet test --project tests/MicroCommerce.FunctionalTests --explicit only   # exact flag confirmed in the first test task
```

Expected: every suite passes, the performance test reports a p95 under 1 second, and `docker ps`
shows no leftover containers.

## Which suite verifies what

| Requirement | UnitTests | FunctionalTests | IntegrationTests | Manual |
|-------------|:-:|:-:|:-:|:-:|
| FR-001–FR-006 (open and edit a Merchant, uniqueness, `/me`) | ✓ (`ShopName`, `PickupAddress`, `Merchant`, validators) | ✓ (including concurrent opens) | ✓ (seeded `merchant`) | Scenario 2 |
| FR-007–FR-015 (Product, options, Variants, status, list) | ✓ (`Product`, combinations, `Money`, validators) | ✓ | ✓ (create and publish) | Scenario 3 |
| FR-016, FR-017 (scoping, 401, 403, cross-Merchant 404) | | ✓ (two Merchants; every operation) | | |
| FR-018 (field-named validation errors) | ✓ | ✓ | | |
| FR-019 (version conflict) | ✓ (version bumps) | ✓ (stale version, and parallel saves) | | Scenario 3.5 |
| FR-020–FR-023 (images) | ✓ (`ImageFormat`, cap, order) | ✓ (Azurite: upload, sniff, size, cap race, reorder, remove, failed save leaves the Product unchanged) | ✓ (upload and anonymous download) | Scenario 3 |
| FR-024–FR-032 (browsing, search, filter, Availability, Shop page) | | ✓ (visibility on every operation; stemming; descendants; no stock or Pickup Address in responses) | ✓ (anonymous search) | Scenario 4 |
| FR-033–FR-036 (Category tree) | ✓ (`Category`, validators) | ✓ (rules, sibling uniqueness, add-child versus create-Product race, 403) | | Scenario 5 |
| FR-037–FR-039 (moderation) | ✓ (`Remove` transitions) | ✓ | | Scenario 5 |
| SC-003 (10k Products, p95 under 1 s) | | ✓ (explicit test, opt-in) | | |
| SC-007 (singular and plural search) | | ✓ (fixed 20-pair set, at least 18 found) | | Scenario 4.2 |
| SC-008 (5 MB upload in under 10 s) | | | | Scenario 3.2 (time a 5 MB upload) |
| SC-004, SC-005 (no leaks, every refusal) | | ✓ | | |
| Area boundary (Catalog never uses `AccountsDbContext`) | ✓ (architecture test) | | | |
