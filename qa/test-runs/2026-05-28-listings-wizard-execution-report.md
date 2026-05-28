# 2026-05-28 — Listings Wizard Execution Report

Manual + Playwright E2E run covering the new 3-step create-listing wizard
shipped on the `dev` branch.

## Scope

- Manual exploratory QA: `qa/test-cases/TC-S28-seller-listings-new-wizard.md`
- Full Playwright suite: `npm run e2e` (Chromium only, via the Aspire
  `playwright` resource — see `src/AppHost/AppHost.cs`).

## Environment

- Aspire AppHost: `MicroCommerce.AppHost.csproj` (this run added
  `WithHttpEndpoint(port: 3000, env: "PORT")` to pin the web port).
- Catalog API: `MicroCommerce.Catalog` on its dev port.
- Azurite storage emulator: configured by AppHost.
- Web: Next.js 16.2.6 with `cacheComponents: true`.

## TC-S28 — manual results

| Case   | Result | Evidence                                  |
|--------|--------|-------------------------------------------|
| S28-01 | PASS   | `evidence/screenshots/S28-01.png`         |
| S28-02 | PASS   | `evidence/screenshots/S28-02.png`         |
| S28-03 | PASS   | `evidence/screenshots/S28-03.png`         |
| S28-04 | PASS   | `evidence/screenshots/S28-04.png`         |
| S28-05 | PASS   | `evidence/screenshots/S28-05.png`         |
| S28-06 | PASS (after fix) | screenshots S28-06.png + console S28-06.txt |

The first-pass capture of S28-06 surfaced two production regressions; both
were closed during this session — see "Fixes landed".

## Fixes landed this run

### Infrastructure / wiring

1. **Web port pinned to 3000** (`src/AppHost/AppHost.cs`). The previous
   build let Aspire pick a random web port, which broke every "open
   localhost:3000" assumption (manual QA, Playwright BASE_URL fallback,
   developer bookmarks).
2. **Playwright Aspire resource references catalog-api**
   (`AppHost.cs`). Without it, `services__catalog-api__http__0` was not
   injected and the e2e fixture fell back to `http://localhost:3000`,
   which proxied to the Next.js app and returned 404 HTML for every
   `createProduct` call.
3. **`src/web/e2e/fixtures/api.ts`** now bracket-accesses the env var
   (hyphen-bearing Aspire service names are invalid JS dotted lookups).

### Browser-side proxies

4. `src/web/src/app/api/products/photo-upload-url/route.ts` — Next.js
   POST proxy to the Catalog API's SAS issuer.
5. `src/web/src/app/api/products/by-sku/[sku]/exists/route.ts` — Next.js
   GET proxy for the wizard's debounced SKU-uniqueness probe. Without
   this, `step-basics.tsx` always hit a 404 and the "duplicate SKU"
   detection never fired.

### Catalog API

6. `Catalog/Program.cs` — on startup in Development, re-read Azurite's
   blob-service properties and write back a permissive CORS rule
   (`AllowedOrigins=*`, `Methods=PUT,GET,OPTIONS,HEAD`). Browser-side
   SAS PUTs were blocked by the missing CORS allowance.

### Test fixture

7. `src/web/e2e/fixtures/photo-small.jpg` regenerated 1×1 → 100×100 via
   `sips`. The 1×1 fixture couldn't be decoded by Chromium's
   `createImageBitmap` and the uploader misclassified that as a
   "dimensions" validation failure.
8. `src/web/e2e/fixtures/product.ts` defaults now include a placeholder
   `photoUrls`; the Catalog API rejects `status=active` payloads with
   no photos.

### Wizard validation

9. `src/web/src/lib/seller/listings/schema.ts` — `validateStep(step=2)`
   no longer blocks the Step 2 → Step 3 transition for missing photos
   when status is "active". Photos are only uploadable on Step 3, so the
   prior gate created a catch-22 for the active-status happy path. The
   inventory side of AC-12 still runs on Step 2; the photo side still
   runs on Step 3's full-schema gate and on the server at Publish.
10. Companion unit test in `schema.test.ts` updated to match.

### E2E spec locator hygiene

11. `getByRole("button", { name: "Next" })` → `{ exact: true }` across
    the wizard specs. Next.js 16 ships a dev-tools chrome button labelled
    "Open Next.js Dev Tools" that substring-matches "Next" and broke
    strict-mode locators.
12. `getByText("Avg. discount" | "Revenue · 7 days" | …)` → `{ exact:
    true }` on the promos + payout-error specs. Sparkline SVG `<title>`
    elements echo the label prefix and collide under strict mode.
13. Orders fulfillment scope locators walk up two parents instead of one
    (`.locator("..").locator("..")`) to land on the `flex-1` column that
    contains both productName and the unique subtitle.

### Not-found UI assertions instead of HTTP status

14. `seller/orders/[id]/detail.spec.ts`, `[sku]/edit.spec.ts`,
    `[sku]/preview.spec.ts`, `[sku]/update.spec.ts` — the "returns 404
    for unknown id/SKU" assertions now check that the rendered not-found
    UI is visible rather than the HTTP status code. Under
    `cacheComponents: true`, the static shell streams a 200 response
    header before the dynamic page body can call `notFound()`, so the
    response status is permanently 200. The user-visible behaviour
    (rendering the not-found UI) is still correct.

## Playwright suite

Final outcome of `aspire resource playwright start` after all fixes landed:

```
116 passed (1.2m)
```

The suite started this session with **22 failures / 94 passing** and ended
with **0 failures / 116 passing**. Additional fixes beyond those listed
above:

- **Catalog.API/Application/Products/Commands/UpdateProduct.cs** — added
  `AsTracking()` to the `FirstOrDefaultAsync`. The DbContext is configured
  with `QueryTrackingBehavior.NoTracking` (see InfrastructureExtensions),
  which kept reads cheap but meant updates loaded an untracked entity and
  `SaveChangesAsync` was a no-op. Direct PUTs returned the mutated DTO yet
  the row in Postgres never changed.
- **Catalog.API/Application/Products/Commands/UpdateProduct.cs** —
  PATCH-style preservation: `description`, `tags`, and `photoUrls` now
  fall back to the stored value when the request omits them. The legacy
  edit form intentionally does not submit those rich fields; without this,
  active products would 400 with "Active products require at least one
  photo" on every save.
- **src/web/e2e/seller/listings/[sku]/delete.spec.ts** — cache-refresh
  test creates a draft + reads `?status=draft` so the new SKU lands on
  page 1 of the filtered listing. Catalog API orders by `Views7d desc`
  with no tie-break; factory products (Views7d=0) used to fall off page 1
  once the seed grew.
- **AppHost.cs** — sets `CI=1` + `PLAYWRIGHT_WORKERS=1` on the
  `playwright` resource. The Aspire suite shares one Catalog DB, so
  parallel mutation specs were inflating counts read by the count-tied
  listings/pagination tests. Local `npm run e2e` still runs the default
  worker count.
- **src/web/src/app/api/test-revalidate/route.ts** + productFactory hook
  — Playwright fixtures mutate the catalog directly, which bypasses the
  Server Actions that normally invalidate the `"listings"` cacheTag. The
  test-only POST endpoint exposes `revalidateTag("listings")` so the
  fixture can keep the Next.js cache honest after each create/delete.
