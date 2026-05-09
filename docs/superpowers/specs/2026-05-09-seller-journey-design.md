# End-to-end seller journey — design

**Date:** 2026-05-09
**Scope:** seller-only. Three pages: `/seller`, `/seller/listings`, `/seller/analytics`.
**Out of scope:** shopper-side surfaces (home, product, cart, checkout).

## Goal

Ship the seller side of Micro Commerce as a clean vertical slice — three connected admin pages, fed by a single in-process mock-data module, faithful to `DESIGN.md` at admin volume, fully covered by Playwright e2e and Vitest unit/component tests, with no test-runtime dependency on real APIs.

## Non-goals

- No real backend, persistence, or API routes. Mock data is a typed TS module.
- No tax-maintenance tests. Anywhere.
- No auth, no roles, no settings.
- No shopper-side pages.
- No mutation flows in the UI tests beyond nav clicks (no "create listing" form, no order state changes). Buttons render but do nothing yet.
- `Orders` and `Customers` nav items render `"Coming soon"` placeholder routes — not tested.

## Existing-state notes

- Repo is on a clean canvas: only the home placeholder, `lib/utils.ts`, and `lib/money.ts` (+ its passing test) exist under `web/src/`.
- Test infra is fully wired: vitest + RTL + jsdom for unit/component, Playwright for e2e (Chromium only). No CI yet — locally run via `npm run test` and `npm run e2e`.
- shadcn is configured (`style: base-nova`, `baseColor: neutral`, lucide icons) but no primitives are installed yet.
- All eight existing e2e specs in `web/e2e/` are deleted as the first step (per scope decision). New seller specs are written fresh.

## Architecture

### Directory layout

```
web/src/
├── app/seller/
│   ├── layout.tsx                      # sidebar shell (Server Component)
│   ├── page.tsx                        # /seller — Overview
│   ├── listings/page.tsx               # /seller/listings
│   ├── analytics/page.tsx              # /seller/analytics
│   ├── orders/page.tsx                 # stub: "Coming soon"
│   └── customers/page.tsx              # stub: "Coming soon"
│
├── components/
│   ├── ui/                             # shadcn primitives (added via `npx shadcn add`)
│   │   button, card, badge, table, tabs, separator, input, scroll-area
│   │
│   └── seller/
│       ├── sidebar.tsx                 # brand wordmark + 5 nav links
│       ├── kpi-card.tsx                # label / value / delta / sparkline
│       ├── revenue-chart.tsx           # 7-day SVG bar chart
│       ├── recent-orders.tsx           # orders table (last 5)
│       ├── today-panel.tsx             # "Today" sidebar of action items
│       ├── listings-table.tsx          # SKU table with status chips
│       ├── filter-chips.tsx            # All/Active/Low/Out/Drafts + counts
│       ├── analytics-kpi-row.tsx       # 4-up KPI row with delta
│       ├── range-tabs.tsx              # 7d / 30d / 90d / Year tabs
│       ├── sources-donut.tsx           # SVG donut + legend list
│       ├── top-products.tsx            # ranked product bars
│       └── conversion-funnel.tsx       # 5 horizontal stages
│
└── lib/seller/
    ├── types.ts                        # Order, Listing, KpiPoint, etc.
    ├── data.ts                         # mock fixtures (deterministic)
    └── data.test.ts                    # invariant tests
```

### Component boundaries

- **Server Components by default.** All page files and the layout are Server Components. They import data directly from `@/lib/seller/data`. No `"use client"` until proven necessary.
- **Client Components are tiny and explicit.** Only `range-tabs.tsx` and any chart with hover/tooltip become Client Components. Each one has a clear, narrow interface (props in, no global state).
- **Charts are pure SVG**, no chart library. Each accepts a typed array of points and renders deterministically. This makes them fast to test and zero-dependency.
- **No prop drilling.** Pages compose components and pass slices of data; components don't reach back into the data module.

### Data flow

```
lib/seller/data.ts
        │
        ▼
   page.tsx (Server Component)
        │  imports specific selectors (orders, listings, kpis, ...)
        ▼
   <SellerSidebar /> + page-specific components
        │  receive typed data via props
        ▼
   render (HTML in stream; client islands hydrate where needed)
```

There is exactly one data module. Pages read from it via named exports (e.g. `getRecentOrders()`, `getListings()`, `getAnalyticsKpis()`). The data module is deterministic — no `Date.now()`, no `Math.random()`. Dates are hard-coded ISO strings; the "Today" view uses a fixed `TODAY` constant exported from the module so tests are stable.

## Data shapes (`lib/seller/types.ts`)

```ts
export type ListingStatus = "active" | "low" | "out" | "draft";

export type Listing = {
  sku: string;            // e.g. "MS-VS-001"
  name: string;           // e.g. "Persimmon vase"
  category: string;       // e.g. "Vessels"
  price: number;          // cents-free, dollars as number
  inventory: number;
  status: ListingStatus;
  views7d: number;
};

export type Order = {
  id: string;             // e.g. "#1042"
  customer: string;
  items: number;
  total: number;
  status: "paid" | "fulfilled" | "refunded" | "pending";
  placedAt: string;       // ISO date
};

export type KpiPoint = {
  label: string;
  value: number;
  format: "currency" | "number" | "percent";
  delta?: number;         // signed percent vs previous period
};

export type RevenuePoint = { day: string; amount: number };
export type SourceBreakdown = { name: string; visits: number; share: number };
export type FunnelStage = { label: string; count: number };
export type TopProduct = { sku: string; name: string; units: number; revenue: number };
```

### Selectors (`lib/seller/data.ts`)

```ts
export const TODAY: Date;                           // fixed
export const BRAND = { name: "Micro Commerce", owner: "Alex" };

export function getOverviewKpis(): KpiPoint[];     // 3 items: Revenue/Orders/Storefront views
export function getRevenueSeries(): RevenuePoint[]; // 7 points
export function getRecentOrders(limit?: 5): Order[];
export function getTodayItems(): { label: string; count?: number }[];

export function getListings(): Listing[];           // 42 entries
export function getListingCounts(): {
  total: number; active: number; low: number; out: number; draft: number;
};

export function getAnalyticsKpis(): KpiPoint[];     // 4: Revenue/Orders/Conversion/Avg. order
export function getRangeOptions(): string[];        // ["7d","30d","90d","Year"]
export function getSources(): SourceBreakdown[];
export function getTopProducts(): TopProduct[];
export function getFunnel(): FunnelStage[];         // 5 stages, monotonically descending
```

The fixtures total **42 listings** so that the filter chips read `All · 42 / Active · 38 / Low · 3 / Out · 1 / Drafts · 4` (sum = 42, with Active+Low+Out covering the published 39 minus Drafts=4 → 38+3+1=42 if drafts overlap with status, OR active=34/low=3/out=1/draft=4=42 if status is mutually exclusive). **Decision: status is mutually exclusive**, so chip counts are: `Active 34 / Low 3 / Out 1 / Drafts 4 = 42`. The header line reads `42 listings · 38 published` (where published = active + low + out = 38). This is internally consistent and the unit test enforces it.

> **Note:** earlier draft used `38 active`. Status semantics are: `active` = published & in stock, `low` = published & low stock, `out` = published & out of stock, `draft` = unpublished. The "38 published" headline counts the three published statuses.

## Visual system (`DESIGN.md` at admin volume)

Same vocabulary as `DESIGN.md`, lower volume than the marketing site:

| Aspect | Decision |
|---|---|
| Surfaces | Pure white `#ffffff` page bg; parchment `#f5f5f7` for the sidebar column and cards |
| Ink | `#1d1d1f` for all primary text (oklch equivalent already in `globals.css`) |
| Accent | shadcn neutral primary for fill CTAs; Action Blue `#0066cc` reserved for KPI delta arrows + selected filter chip ring |
| Radius | `rounded-lg` (18px) for cards; `rounded-full` for "New listing" / "Import CSV" pill CTAs; `rounded-md` for inputs |
| Hairlines | `1px rgba(0,0,0,0.08)` borders on cards and table rows |
| Shadows | None on UI. None at all in this slice (no product photography to elevate) |
| Typography | Geist Sans (already loaded). Display weights 600; body 400 at 14–15px (admin density beats the marketing 17px) |
| Spacing | 8px base. Section padding 32–48px (tighter than the marketing 80px) |
| Sidebar | 240px parchment column, 64px header band with brand wordmark, 5 nav rows at 14px/400 with active-row treatment as 600 weight + thin left blue indicator |

The single-accent rule from `DESIGN.md` is preserved: every interactive element is either neutral primary fill or Action Blue text/ring. No third color enters.

## Page contracts (what each page renders)

### `/seller` — Overview

- Top bar: brand wordmark "Micro Commerce", date pill (the fixed `TODAY` formatted, e.g. "Tuesday · April 8"), avatar
- Greeting heading: `"Good morning, Alex"` in display weight
- 3-up KPI row: Revenue · 7 days, Orders · 7 days, Storefront views (each from `getOverviewKpis()`)
- Two-column band: revenue chart (left, 2/3 width) + Today panel (right, 1/3 width)
- Recent orders card: table with columns Order / Customer / Items / Total / Status, 5 rows from `getRecentOrders()`

### `/seller/listings`

- Heading "Listings"
- Subheading: `"42 listings · 38 published"`
- Filter chip row: All · 42 / Active · 34 / Low · 3 / Out · 1 / Drafts · 4
- Toolbar right: "Import CSV" + "New listing" pill buttons (no behaviour)
- Dense table: SKU / Product / Category / Price / Inventory / Status / Views (7d) — 9 visible rows (first page)
- Footer: `"9 of 42 shown"` + paginator (no behaviour beyond rendering)

### `/seller/analytics`

- Heading "Analytics"
- Range subline: `"Apr 1 – Apr 30 · vs Mar 1 – Mar 30"`
- Range tabs row: 7d / 30d / 90d / Year (visual only, default 30d active) + Export pill button
- 4-up KPI row: Revenue / Orders / Conversion / Avg. order — each with delta arrow vs previous period
- Two-column band: Sources donut (left) + Top products bar list (right)
- Conversion funnel card spanning full width: 5 horizontal stages with counts and step-down %

## Routing & navigation

- `app/seller/layout.tsx` renders `<SellerSidebar />` + `<main>{children}</main>` in a CSS grid (`grid-cols-[240px_1fr]`).
- Sidebar links use Next.js `<Link>` to `/seller`, `/seller/orders`, `/seller/listings`, `/seller/analytics`, `/seller/customers`.
- Active link is computed from `usePathname()` (Client Component island for the sidebar nav row only).
- Orders + Customers routes render a single `<Placeholder title="Orders" />` style component — out of scope for tests.

## Error handling

This is a pure mock-data slice with no I/O, no async, no user input. There is nothing meaningful to handle:
- Data shapes are fixed at module load.
- Pages are SSR-static — no loading states, no error boundaries.
- Buttons that don't yet do anything render as enabled but inert. Acceptable because nothing has been promised by the UI.

The data-invariant unit test acts as the only "error" path — if mock data drifts (e.g. listings sum stops matching chip counts), the test fails loudly at CI time, not at runtime in the browser.

## Testing strategy

### Layer 1 — e2e (Playwright, fresh)

Three spec files, one per page, plus the two nav-click flows:

- `web/e2e/seller.spec.ts` — `/seller` renders sidebar / brand / greeting / 3 KPIs / chart heading / Today panel / Recent orders table with five distinct order IDs.
- `web/e2e/seller-listings.spec.ts` — `/seller/listings` renders heading / subheading / 5 filter chips with exact counts / 9 SKU rows / "9 of 42 shown" / Import CSV + New listing buttons. Plus: clicking "Listings" in the sidebar from `/seller` navigates here.
- `web/e2e/seller-analytics.spec.ts` — `/seller/analytics` renders heading / range subline / 4 range tabs / Export / 4 KPI labels / Sources / a top-product name / Conversion funnel with all 5 stage labels. Plus: clicking "Analytics" in the sidebar from `/seller` navigates here.

All assertions key off **stable text content and roles**, not test ids.

### Layer 2 — Vitest unit (`lib/seller/data.test.ts`)

Invariants only — no snapshot of full data:

- `getListings().length === 42`
- counts by status sum to 42 and match `getListingCounts()`
- `getListingCounts().active + .low + .out` === number of "published" listings === 38
- `getRevenueSeries().length === 7`
- `getRecentOrders().length === 5`
- order IDs are unique
- `getFunnel()` is monotonically non-increasing
- `getSources()` shares sum to 1.0 (within ε)
- KPI values are non-negative; `format` is one of the three allowed literals

### Layer 3 — Vitest + RTL component tests

Only for components with real logic:

- `filter-chips.test.tsx` — given counts, renders five chips with the exact label/count format and applies the active state to the prop-selected key.
- `listings-table.test.tsx` — given 42 listings and `pageSize=9`, renders 9 rows and the footer "9 of 42 shown"; given `pageSize=42`, renders 42 rows and "42 of 42 shown".
- `conversion-funnel.test.tsx` — renders one element per stage, in order.

Skipped as redundant with e2e:
- `kpi-card`, `recent-orders`, `today-panel`, `revenue-chart`, `sources-donut`, `top-products`, `range-tabs` — pure presentation; e2e covers visibility.

### Layer 4 — Build & lint gates

- `npm run lint` (Biome) clean
- `npm run build` (Next.js production build) succeeds with no type errors

### Tax tests

None. Anywhere. The cart-side e2e specs that referenced "Estimated tax" are being deleted; nothing in the seller slice mentions tax.

## Build sequence (TDD discipline)

1. Delete all 8 e2e files in `web/e2e/`. Verify `npm run e2e` reports 0 tests.
2. Write the 3 fresh seller e2e specs. Run — they fail (no routes yet).
3. Write `lib/seller/types.ts` and `lib/seller/data.test.ts`. Run unit tests — they fail (no data module).
4. Implement `lib/seller/data.ts`. Unit tests go green.
5. Add shadcn primitives. Build sidebar + `app/seller/layout.tsx`. Add `Coming soon` stubs for `/seller/orders` and `/seller/customers` so the nav has no 404s during interactive testing.
6. Build the three pages page-by-page. After each page, its e2e spec goes green; the others remain red until their turn.
7. Write the three component tests for `filter-chips`, `listings-table`, `conversion-funnel`. Run — they pass.
8. `npm run lint` clean; `npm run build` succeeds.
9. Final `npm run test` + `npm run e2e` — all green.

## Open follow-ups (out of this slice)

- Wire shopper-side surfaces (home/product/cart/checkout) on top of the same mock data layer — explicitly the "next" iteration the user signalled with "first let's…".
- Real interactivity: filter-chip toggling, pagination, listing CRUD, analytics range switching.
- Auth + multi-tenant routing (currently `/seller` is the only seller).
