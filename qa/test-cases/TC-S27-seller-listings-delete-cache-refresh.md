# TC-S27 — Listings table refreshes after delete (client cache regression)

**Route:** `/seller/listings/{sku}/edit` → `/seller/listings`
**Batch:** E
**Worker:** manual
**Executed:** 2026-05-19

---

## Why this test exists

The seller listings table (`ListingsTable`) is a client component backed by TanStack Query with a shared `QueryClient` and `staleTime: 30_000`. The page-level `revalidatePath("/seller/listings")` and `force-dynamic` settings keep the **server** data fresh, but TanStack Query's cache is the source of truth on the client — if a cache entry already exists, `initialData` from the freshly rendered page is **ignored**.

The bug therefore only reproduces when the user **first lands on `/seller/listings`** (which primes the cache) before clicking through to the edit page and deleting. The existing e2e (`seller-listings-delete.spec.ts → shows confirm modal and deletes listing on confirm`) navigates directly to `/edit`, so it never primes the cache and never trips the bug. TC-S27 covers the gap.

The fix invalidates the `["listings"]` cache from `DeleteListingButton.handleConfirm` before `router.push("/seller/listings")`.

---

## Preconditions

- Full Aspire stack running (web + Catalog API + Postgres). Discover the web port via `mcp__aspire__list_resources`.
- Catalog API reachable. Seed data present.
- A throwaway fixture SKU `QA-DELETE-CACHE-<ts>` created via `POST /api/products` for each pass.

---

## Test cases

### S27-01 — Deleted listing disappears from the table when the index was visited first

**Steps:**

1. Create a fixture product via the Catalog API:
   ```bash
   curl -sX POST "$API/api/products" -H 'Content-Type: application/json' \
     -d "{\"sku\":\"QA-DELETE-CACHE-$(date +%s)\",\"name\":\"Cache Stale Repro\",\"category\":\"Ceramics\",\"price\":49.99,\"inventory\":5,\"status\":\"active\"}"
   ```
2. With the same browser session:
   - `agent-browser ... open http://<web>/seller/listings` (primes TanStack Query cache for `["listings", 1, 9, "all"]`).
   - Verify the fixture SKU is visible in the table snapshot.
3. Navigate to `/seller/listings/<sku>/edit`.
4. Click **Delete listing**, then **Delete** in the alert dialog.
5. After the redirect to `/seller/listings`, snapshot the table.

**Expected:**

- URL is `/seller/listings`.
- The fixture SKU is **absent** from the table on first paint — no 30s wait, no manual reload.
- `GET /api/products/<sku>` returns **404**.
- No console errors (especially no React Query warnings about stale data).

**Result:** [ ] pass / [ ] fail

**Evidence:** `evidence/screenshots/S27-01.png`

---

### S27-02 — Listings count badges (`counts.total`, `counts.active`) reflect the deletion

**Steps:**

1. Same setup as S27-01, but capture the topbar subtitle (`"{total} products · {active} active"`) **before** the delete.
2. After redirect, capture it again.

**Expected:** Both counters decrease by 1. (`counts` is fetched server-side, so this asserts the server RSC re-rendered — independent of the TanStack Query fix.)

**Result:** [ ] pass / [ ] fail

**Evidence:** `evidence/screenshots/S27-02.png`

---

### S27-03 — Regression check: cold-cache delete still works

**Steps:**

1. Open a fresh incognito/clean session.
2. Navigate **directly** to `/seller/listings/<sku>/edit` (skip the index — cache stays empty).
3. Delete and confirm.

**Expected:** Same outcome as S27-01. This is the path the existing e2e covers — it must not regress.

**Result:** [ ] pass / [ ] fail

**Evidence:** `evidence/screenshots/S27-03.png`

---

## Summary

| Case   | Description                                                          | Result |
|--------|----------------------------------------------------------------------|--------|
| S27-01 | Warm-cache delete: row disappears immediately after redirect         | [ ]    |
| S27-02 | Topbar counters decrement after delete                               | [ ]    |
| S27-03 | Cold-cache delete still works (no regression)                        | [ ]    |

**TC-S27 status: pending execution against running Aspire stack.**

> **Status: Superseded by e2e suite.** The warm-cache regression is now
> covered by the `@seed-dependent` test in
> `src/web/e2e/seller/listings/[sku]/delete.spec.ts` ("listings table
> refreshes when user visited the index before deleting"). Keep this test
> case for design/a11y screenshot evidence and console-error sweeps only —
> do not re-run the functional cases on every release.
