# End-to-end listings management — design

**Date:** 2026-05-09
**Scope:** Add the four hi-fi listings-management screens that complete the seller-journey listings flow. Static visual mockups matching existing patterns (`/seller/listings/new`, `/seller/orders/[id]/pack`). No real state, no persistence.
**References:** `design/project/hifi-flow-listings.jsx` (`LFlow_01..05`); aggregated in `design/Micro Commerce Hi-fi _standalone_.html`.

## Goal

Cover the remaining 4 of the 5 hi-fi steps in the listings flow as production routes:

| Hi-fi step | URL | Status |
|---|---|---|
| `LFlow_01_Catalog` | `/seller/listings` | already shipped |
| `LFlow_02_Bulk` | `/seller/listings/bulk` | new |
| `LFlow_03_Edit` | `/seller/listings/[sku]/edit` | new |
| `LFlow_04_Preview` | `/seller/listings/[sku]/preview` | new |
| `LFlow_05_Published` | `/seller/listings/published` | new |

After this work, the QA suite can navigate the full listings-management arc by URL.

## Non-goals

- No persistence layer, no API routes.
- No real selection state, form inputs, drag-and-drop, photo upload.
- No variants in the data layer (`web/src/lib/seller/types.ts` `Listing` is unchanged; variants are hardcoded in the editor/preview pages, matching how `/new` hardcodes "Persimmon vase").
- No inter-page navigation links between the new pages. Each route is reachable directly.

## Architecture

- **Server Components only.** No `"use client"`. All four pages render static markup; this matches every other route under `/seller/`.
- **Inline layouts** in each `page.tsx` (mirrors `/seller/listings/new/page.tsx`). No new shared components extracted — verbatim duplication does not occur across the four pages.
- **Reused primitives:** `Button` (`@/components/ui/button`), `SellerTopbar` (`@/components/seller/seller-topbar`) where the design uses a topbar. `SellerSidebar` is provided by `web/src/app/seller/layout.tsx` and is not rendered per-page.
- **Single data-layer addition:** `getListingBySku(sku: string): Listing | null` exported from `web/src/lib/seller/data.ts`. Used by `[sku]/edit` and `[sku]/preview`. If the helper returns `null`, the page calls `notFound()`.

## Files added

```
web/src/app/seller/listings/
  bulk/page.tsx
  [sku]/edit/page.tsx
  [sku]/preview/page.tsx
  published/page.tsx

web/e2e/
  seller-listings-bulk.spec.ts
  seller-listings-edit.spec.ts
  seller-listings-preview.spec.ts
  seller-listings-published.spec.ts
```

## Files modified

- `web/src/lib/seller/data.ts` — add `getListingBySku`.
- `web/src/lib/seller/data.test.ts` — add a test for `getListingBySku` (returns the listing on hit, `null` on miss).

## Per-screen specs

### 1) `/seller/listings/bulk` (LFlow_02_Bulk)

**Layout (top to bottom inside `<main>`):**

1. Topbar: title `Listings`, subtitle `Filtered · low & out of stock`, right action `+ New listing` (primary).
2. Filter chips row: `All · 42`, `Active · 38`, `Low · 3`, `Out · 1`, `Drafts · 4`. `Low · 3` and `Out · 1` rendered as active chips; others inactive.
3. Dark bulk-action bar (`bg-[#1d1d1f] text-white`):
   - Left: filled checkbox icon + `3 of 4 selected`.
   - Right: outline-on-dark buttons `Edit price`, `Adjust stock`, `Move to draft`, then primary-on-dark `Apply →`.
4. Two-column body:
   - **Left — table** with header columns `Product`, `SKU`, `Status`, `Stock`, `Price`, `New price`. Four hardcoded rows sourced from real Low/Out listings in `data.ts`:
     | Product | SKU | Status | Stock | Price | New price | Selected |
     |---|---|---|---|---|---|---|
     | Rust mug Nº 04 | MC-MG-041 | Out | 0 | $28 | $30.80 | yes |
     | Ember tea bowl | MC-SC-008 | Low | 2 | $36 | $39.60 | yes |
     | Peat serving bowl | MC-BW-051 | Low | 4 | $90 | $99.00 | yes |
     | Mist tumbler | MC-TB-038 | Low | 3 | $40 | — | no |

     Total = 4 rows (1 Out + 3 Low), matching the existing catalog counts `Low · 3, Out · 1`. Selected rows have an orange-tinted background (`rgba(194,65,12,0.04)`), price column struck-through, new-price column rendered in `text-emerald-600`.
   - **Right — drawer (340px wide, left border):**
     - Eyebrow `Bulk edit · 3 items`.
     - Heading `Adjust price`.
     - Segmented control `Set to / Increase / Decrease` (Increase active).
     - Amount field showing `10`, unit field showing `%`.
     - Preview list (3 items) with old (struck-through) → new prices.
     - `Apply to 3 items` primary button (full width), `Cancel` ghost button.

**SKU prefix:** the codebase uses `MC-` (per `seller-listings.spec.ts` and `data.ts`). The hi-fi mockup shows `MS-` — we use `MC-` to stay consistent with the live catalog.

**E2E (`seller-listings-bulk.spec.ts`)** asserts:
- Heading `Listings` and subtitle containing `low & out of stock` (case-insensitive).
- All 5 filter chips visible.
- `3 of 4 selected` text visible.
- Buttons `Edit price`, `Adjust stock`, `Move to draft`, `Apply →` visible.
- Drawer: heading `Adjust price`, all three segmented options, `Apply to 3 items` button.
- All 4 product names (`Rust mug Nº 04`, `Ember tea bowl`, `Peat serving bowl`, `Mist tumbler`) and their SKUs visible as cells.

---

### 2) `/seller/listings/[sku]/edit` (LFlow_03_Edit)

**Lookup behavior:** `await getListingBySku(params.sku)`; if `null`, `notFound()`.

**Substituted fields** (from looked-up listing):
- Header breadcrumb category text: `Listings · {listing.category}`.
- Title: `{listing.name}`.
- Status caption slug: derived from `listing.name` via `name.toLowerCase().replace(/\s+/g, "-")`.

**Hardcoded fields** (independent of which listing was looked up): variant matrix, photo grid, "Unsaved changes" pill, "2 variants updated" caption, "4 of 8" photos count.

**Layout:**

1. Header row (with bottom border):
   - Left: back chevron icon button, then a stack with `Listings · {category}` (eyebrow, muted) and `{name}` (display heading), then a warn pill `Unsaved changes`.
   - Right: ghost `Discard`, outline `Save draft`, primary `Publish →`.
2. Two-column body grid (`1.5fr 1fr`):
   - **Left card — Variant matrix:**
     - Header row: stack of `Variant matrix` (h3) and `Size × Glaze · 6 combinations` (muted), plus outline-sm button `+ Add option`.
     - Table columns: `Variant`, `SKU`, `Price`, `Stock`, `Status`. Six hardcoded rows:
       | Variant | SKU | Price | Stock | Status | Changed? |
       |---|---|---|---|---|---|
       | Small · Persimmon | MS-VS-001-S | $70 | 6 | Active | yes |
       | Medium · Persimmon | MS-VS-001-M | $95 | 4 | Active | yes |
       | Large · Persimmon | MS-VS-001-L | $136 | 0 | Out | no |
       | Small · Cream | MS-VS-001-SC | $70 | 8 | Active | no |
       | Medium · Cream | MS-VS-001-MC | $95 | 5 | Active | no |
       | Large · Cream | MS-VS-001-LC | $136 | 2 | Low | no |

       Variant SKUs intentionally use the `MS-` design prefix because variant SKUs do not exist in the data layer; these are display-only strings copied from the hi-fi.

       "Changed" rows: green dot before the variant label, price in `text-emerald-600` and bold.
     - Below the table: muted caption `● 2 variants updated · prices +10%`.
   - **Right column** (stacked cards):
     - Photos card: heading `Photos · 4 of 8`, 3-col grid of 5 swatches plus a dashed `+` cell (matching the hi-fi tones: clay, bone, shadow, terra).
     - Status card: heading `Status`, segmented `Active / Draft / Archived` (Active active), caption `Visible at /{slug}` in mono.

**E2E (`seller-listings-edit.spec.ts`)** asserts (using `/seller/listings/MC-VS-001/edit`):
- Heading text equals the listing's name (read the expected name from `getListings()` via a test-side import, or hardcode `Persimmon vase` and accept that the spec tracks the data).
- `Listings · {category}` eyebrow visible.
- Buttons `Discard`, `Save draft`, `Publish →` visible.
- `Unsaved changes` pill visible.
- `Variant matrix` heading and `6 combinations` text visible.
- All 6 variant labels visible.
- `2 variants updated` caption visible.
- Status segmented control: `Active`, `Draft`, `Archived` visible.
- `Photos · 4 of 8` visible.
- 404 case: `goto("/seller/listings/NOPE-404/edit")` resolves to a 404 (assert via `await page.goto(...)` returning a response with `status() === 404`).

---

### 3) `/seller/listings/[sku]/preview` (LFlow_04_Preview)

**Lookup behavior:** identical to edit page.

**Substituted fields** (from looked-up listing): breadcrumb `Preview · {listing.name}`, mock URL `alex-studio.micro.shop/{slug}`, eyebrow `Alex Studio · {listing.category}`, product title `{listing.name}`. Other strings (price `$95`, stock caption, size chips, glaze swatches, listing-health checks) are hardcoded to match the hi-fi.

**Brand note:** the hi-fi shows `Mira Studio`, but per `CLAUDE.md` "Mira" outside `/seller/apply` is a copy bug. Use `Alex Studio` (derived from `BRAND.owner = "Alex"`) instead.

**Layout:**

1. Header row:
   - Left: back chevron, eyebrow `Preview · {name}`, title `How shoppers will see it`.
   - Right: segmented `Desktop / Mobile` (Desktop active), outline `Back to edit`, primary `Publish now →`.
2. Body on `bg-[#f5f5f7]`, two regions:
   - **Center — desktop browser preview card** (max-w 720, h 560, drop shadow):
     - Faux browser chrome: 3 traffic-light dots, mock URL bar with lock icon and text `alex-studio.micro.shop/{slug}`.
     - Two-column inner grid (`1.4fr 1fr`):
       - Left: full-bleed product image swatch (`tone="clay"`).
       - Right (padded 28): eyebrow `Alex Studio · {category}`, display title `{name}`, price `$95`, caption `Medium · Persimmon · 4 in stock`, SIZE label, chips `Small / Medium / Large · out`, GLAZE label, two color swatches, then a flex spacer, then primary full-width button `Add to bag · $95`.
   - **Right rail (320px, left border) — Listing health:**
     - Title `Listing health` + score `96` (right-aligned, green).
     - Progress bar at 96% (green fill).
     - 5 check rows:
       | Label | Sub | Tone |
       |---|---|---|
       | Title under 60 chars | 14 / 60 | good |
       | Description over 100 chars | 208 / 800 | good |
       | 4 photos | recommend 6+ | warn |
       | Variants in stock | 5 of 6 active | good |
       | Tagged & categorized | 4 tags · Vessels | good |

       Good rows: green circle with check icon. Warn row: orange circle with info icon.

**E2E (`seller-listings-preview.spec.ts`)** asserts (`/seller/listings/MC-VS-001/preview`):
- Heading `How shoppers will see it` visible.
- Mock URL containing the slug visible.
- Buttons `Back to edit`, `Publish now →` visible; segmented `Desktop` and `Mobile` visible.
- `Listing health` heading and score `96` visible.
- All 5 health-check labels visible.
- Size chips `Small`, `Medium`, `Large · out` visible.
- `Add to bag · $95` button visible.
- 404 case as in edit page.

---

### 4) `/seller/listings/published` (LFlow_05_Published)

**Layout (no `[sku]`, fully static):**

1. Topbar: title `Listings`, subtitle `42 products · 39 active`, primary action `+ New listing`.
2. Success banner (`bg-[#DDEDE1]`, bottom border):
   - Green-circle check icon (left).
   - Text `Persimmon vase published · 2 variants updated, 1 went live.`
   - Right: ghost-sm `View shop →` and ghost-sm `Undo`.
3. Body (overflow-auto, padded):
   - **3-up KPI grid:**
     | Label | Value | Delta | Direction |
     |---|---|---|---|
     | Active listings | 39 | +1 | up |
     | Variants in stock | 128 | +2 | up |
     | Out-of-stock items | 1 | −2 | up (good direction for OOS) |
   - **Activity card** "What just changed":
     - Header: heading `What just changed` + link `Activity log →`.
     - Table columns `When / Item / Change / By`. 5 rows (variant rows narrate the edit-page changes; bulk rows reuse two of the bulk-page fixtures):
       | When | Item | Change | By |
       |---|---|---|---|
       | just now | Persimmon vase · Medium | Price · $86 → $95 | You |
       | just now | Persimmon vase · Small | Price · $64 → $70 | You |
       | just now | Persimmon vase · Large | Status · Out → still out (no stock) | You |
       | 12 min ago | Ember tea bowl | Price · $36 → $39.60 | You · bulk |
       | 12 min ago | Peat serving bowl | Price · $90 → $99 | You · bulk |

**E2E (`seller-listings-published.spec.ts`)** asserts:
- Heading `Listings` + subtitle `42 products · 39 active`.
- Success banner text `Persimmon vase published` and `2 variants updated, 1 went live`.
- `View shop →` and `Undo` buttons visible.
- All three KPI labels and their values (`39`, `128`, `1`) visible.
- `What just changed` heading and `Activity log →` link visible.
- All 5 activity rows visible (assert by item label: 3 Persimmon vase variants, then `Ember tea bowl` and `Peat serving bowl`).

## Build sequence (TDD)

Per project rule (`web/AGENTS.md`, root `CLAUDE.md`): tests first.

1. Add `getListingBySku` test to `data.test.ts` (red).
2. Implement `getListingBySku` in `data.ts` (green).
3. For each of the four screens, in order: write the e2e spec (red — route 404), implement the page (green), run `npm run lint` and `npm run build`.

Recommended screen order (lowest dependency first):
1. `/seller/listings/published` — no params, simplest.
2. `/seller/listings/bulk` — no params, more pieces.
3. `/seller/listings/[sku]/edit` — params + lookup + 404 case.
4. `/seller/listings/[sku]/preview` — same shape as edit.

## Verification

Before declaring done:
- `cd web && npm run build` passes (typecheck gate).
- `cd web && npm run lint` clean.
- `cd web && npm test` passes (covers `data.test.ts`).
- `cd web && npm run e2e` passes for all 4 new specs plus the existing `seller-listings*` specs (no regressions).

## Risks

- **Hi-fi vs. data divergence.** Variant matrix uses `MS-` SKU prefix because variants are not in the data layer. Documented above. If a future task adds variants to the data layer, the editor page should switch to data-driven variants.
- **Brand-name leak (resolved).** The hi-fi shows `Mira Studio` in the preview. Per `CLAUDE.md`, "Mira" outside `/seller/apply` is a copy bug. Spec uses `Alex Studio` and `alex-studio.micro.shop` instead, derived from `BRAND.owner`. The preview e2e asserts against `Alex Studio`, not `Mira`.
- **404 testing.** Playwright's `page.goto` returns the navigation response; the assertion needs to use the response object, not just visible text, to be reliable.
