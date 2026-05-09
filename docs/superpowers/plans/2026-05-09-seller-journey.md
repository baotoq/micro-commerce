# Seller Journey Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Ship the three-page seller admin (`/seller`, `/seller/listings`, `/seller/analytics`) backed by an in-process mock-data module, with full e2e + unit + component test coverage and no tax-maintenance tests.

**Architecture:** Server-Component-first Next.js App Router pages reading from a typed `lib/seller/data.ts` module. Sidebar layout wraps three real pages plus two `Coming soon` stubs. Charts are hand-rolled SVG. shadcn primitives provide buttons/cards/badges/tables/tabs/inputs.

**Tech Stack:** Next.js 16 (App Router, RSC), React 19 (compiler on), Tailwind CSS v4, shadcn (`base-nova`, neutral), Base UI / Radix, lucide-react, Vitest + RTL + jsdom, Playwright (Chromium).

**Spec:** [`docs/superpowers/specs/2026-05-09-seller-journey-design.md`](../specs/2026-05-09-seller-journey-design.md)

**Working directory note:** All commands run from `/Users/baotoq/Work/micro-commerce/web/`.

---

## Conventions used in this plan

- File paths are absolute from repo root for clarity, but `npm` commands assume `cwd = web/`.
- Each task ends with a commit step. Commit messages follow `type(scope): subject` (`test`, `feat`, `chore`, `refactor`).
- "Run e2e" means `npm run e2e` (Playwright auto-starts dev server via `playwright.config.ts`).
- Tests come first; implementation follows. Exception: the data-module shape needs to exist as `types.ts` before either tests or impl.

---

## Task 1: Wipe stale e2e specs

**Files:**
- Delete: `web/e2e/cart.spec.ts`, `web/e2e/checkout.spec.ts`, `web/e2e/home.spec.ts`, `web/e2e/product.spec.ts`, `web/e2e/seller.spec.ts`, `web/e2e/seller-analytics.spec.ts`, `web/e2e/seller-listings.spec.ts`, `web/e2e/shopper-flow.spec.ts`

- [ ] **Step 1: Delete all eight spec files**

```bash
cd web
rm e2e/cart.spec.ts e2e/checkout.spec.ts e2e/home.spec.ts e2e/product.spec.ts \
   e2e/seller.spec.ts e2e/seller-analytics.spec.ts e2e/seller-listings.spec.ts \
   e2e/shopper-flow.spec.ts
```

- [ ] **Step 2: Verify the directory is empty**

```bash
ls e2e/
```
Expected: no `.spec.ts` files (the dir may be empty or contain only README/fixture files if any).

- [ ] **Step 3: Confirm Playwright reports zero tests**

```bash
npm run e2e -- --list
```
Expected: `Total: 0 tests in 0 files`.

- [ ] **Step 4: Commit**

```bash
git add -A
git commit -m "chore(e2e): remove stale specs before TDD rewrite"
```

---

## Task 2: Write fresh `/seller` e2e spec (TDD red)

**Files:**
- Create: `web/e2e/seller.spec.ts`

- [ ] **Step 1: Write the failing e2e spec**

```ts
// web/e2e/seller.spec.ts
import { expect, test } from "@playwright/test";

test.describe("Seller overview", () => {
  test("renders sidebar, greeting, KPIs, today panel, and recent orders", async ({ page }) => {
    await page.goto("/seller");

    // Brand + sidebar nav
    await expect(page.getByText("Micro Commerce").first()).toBeVisible();
    for (const item of ["Overview", "Orders", "Listings", "Analytics", "Customers"]) {
      await expect(page.getByRole("link", { name: item, exact: true })).toBeVisible();
    }

    // Greeting + date
    await expect(page.getByRole("heading", { name: /Good morning, Alex/ })).toBeVisible();
    await expect(page.getByText("Tuesday · April 8")).toBeVisible();

    // KPI labels
    for (const label of ["Revenue · 7 days", "Orders · 7 days", "Storefront views"]) {
      await expect(page.getByText(label, { exact: true })).toBeVisible();
    }

    // Chart heading
    await expect(page.getByRole("heading", { name: /Revenue/ })).toBeVisible();

    // Today panel
    await expect(page.getByRole("heading", { name: "Today", exact: true })).toBeVisible();

    // Recent orders table — five distinct order IDs
    await expect(page.getByRole("heading", { name: "Recent orders" })).toBeVisible();
    for (const id of ["#1042", "#1041", "#1040", "#1039", "#1038"]) {
      await expect(page.getByRole("cell", { name: id, exact: true })).toBeVisible();
    }
  });
});
```

- [ ] **Step 2: Run and confirm failure**

```bash
npm run e2e -- e2e/seller.spec.ts
```
Expected: 1 failure ("Page not found" / 404 — no `/seller` route exists yet).

- [ ] **Step 3: Commit**

```bash
git add e2e/seller.spec.ts
git commit -m "test(seller): add overview e2e spec (red)"
```

---

## Task 3: Write fresh `/seller/listings` e2e spec (TDD red)

**Files:**
- Create: `web/e2e/seller-listings.spec.ts`

- [ ] **Step 1: Write the failing e2e spec**

```ts
// web/e2e/seller-listings.spec.ts
import { expect, test } from "@playwright/test";

test.describe("Seller listings", () => {
  test("renders heading, filter chips, dense table, toolbar, and pagination", async ({ page }) => {
    await page.goto("/seller/listings");

    await expect(page.getByRole("heading", { name: "Listings", exact: true })).toBeVisible();
    await expect(page.getByText("42 listings · 38 published")).toBeVisible();

    for (const chip of [
      "All · 42",
      "Active · 34",
      "Low · 3",
      "Out · 1",
      "Drafts · 4",
    ]) {
      await expect(page.getByText(chip, { exact: true })).toBeVisible();
    }

    // Toolbar buttons (no behavior, must render)
    await expect(page.getByRole("button", { name: /Import CSV/ })).toBeVisible();
    await expect(page.getByRole("button", { name: /New listing/ })).toBeVisible();

    // First-page SKU rows (page size 9)
    for (const sku of [
      "MC-VS-001", "MC-VS-002", "MC-BW-014", "MC-TB-007",
      "MC-CR-003", "MC-PL-022", "MC-MG-041", "MC-SC-008", "MC-VS-031",
    ]) {
      await expect(page.getByRole("cell", { name: sku, exact: true })).toBeVisible();
    }

    // Some named products
    await expect(page.getByText("Persimmon vase")).toBeVisible();

    // Pagination footer
    await expect(page.getByText("9 of 42 shown")).toBeVisible();
  });

  test("the Listings nav item links here from the seller dashboard", async ({ page }) => {
    await page.goto("/seller");
    await page.getByRole("link", { name: "Listings", exact: true }).click();
    await expect(page).toHaveURL("/seller/listings");
  });
});
```

- [ ] **Step 2: Run and confirm failure**

```bash
npm run e2e -- e2e/seller-listings.spec.ts
```
Expected: 2 failures.

- [ ] **Step 3: Commit**

```bash
git add e2e/seller-listings.spec.ts
git commit -m "test(seller): add listings e2e spec (red)"
```

---

## Task 4: Write fresh `/seller/analytics` e2e spec (TDD red)

**Files:**
- Create: `web/e2e/seller-analytics.spec.ts`

- [ ] **Step 1: Write the failing e2e spec**

```ts
// web/e2e/seller-analytics.spec.ts
import { expect, test } from "@playwright/test";

test.describe("Seller analytics", () => {
  test("renders KPIs, range tabs, sources, top products, and conversion funnel", async ({ page }) => {
    await page.goto("/seller/analytics");

    await expect(page.getByRole("heading", { name: "Analytics", exact: true })).toBeVisible();
    await expect(page.getByText("Apr 1 – Apr 30 · vs Mar 1 – Mar 30")).toBeVisible();

    for (const r of ["7d", "30d", "90d", "Year"]) {
      await expect(page.getByRole("button", { name: r, exact: true })).toBeVisible();
    }
    await expect(page.getByRole("button", { name: /Export/ })).toBeVisible();

    // KPI labels (use .last() so sidebar's "Orders" doesn't match)
    for (const label of ["Revenue", "Orders", "Conversion", "Avg. order"]) {
      await expect(page.getByText(label, { exact: true }).last()).toBeVisible();
    }
    await expect(page.getByText("$12,480.00").first()).toBeVisible();
    await expect(page.getByText(/3\.4%/).first()).toBeVisible();

    // Sources card
    await expect(page.getByRole("heading", { name: "Sources" })).toBeVisible();
    await expect(page.getByText("Organic search")).toBeVisible();
    await expect(page.getByText("2,304")).toBeVisible();

    // Top products
    await expect(page.getByRole("heading", { name: "Top products" })).toBeVisible();
    await expect(page.getByText("Persimmon vase")).toBeVisible();

    // Conversion funnel
    await expect(page.getByRole("heading", { name: "Conversion funnel" })).toBeVisible();
    for (const stage of [
      "Storefront views", "Product views", "Added to cart", "Checkout started", "Purchased",
    ]) {
      await expect(page.getByText(stage, { exact: true })).toBeVisible();
    }
  });

  test("the Analytics nav item links here from the seller dashboard", async ({ page }) => {
    await page.goto("/seller");
    await page.getByRole("link", { name: "Analytics", exact: true }).click();
    await expect(page).toHaveURL("/seller/analytics");
  });
});
```

- [ ] **Step 2: Run and confirm failure**

```bash
npm run e2e -- e2e/seller-analytics.spec.ts
```
Expected: 2 failures.

- [ ] **Step 3: Commit**

```bash
git add e2e/seller-analytics.spec.ts
git commit -m "test(seller): add analytics e2e spec (red)"
```

---

## Task 5: Add shadcn UI primitives

**Files:**
- Create: `web/src/components/ui/button.tsx`, `card.tsx`, `badge.tsx`, `table.tsx`, `tabs.tsx`, `separator.tsx`, `input.tsx`, `scroll-area.tsx`

- [ ] **Step 1: Install all primitives in one shadcn call**

```bash
cd web
npx shadcn@latest add button card badge table tabs separator input scroll-area --yes
```
Expected: files appear under `web/src/components/ui/`. New runtime deps may be added to `package.json` (Radix sub-packages, etc.) — that's fine.

- [ ] **Step 2: Sanity-check build**

```bash
npx tsc --noEmit
```
Expected: 0 errors.

- [ ] **Step 3: Commit**

```bash
git add src/components/ui package.json package-lock.json
git commit -m "feat(ui): add shadcn primitives (button, card, badge, table, tabs, separator, input, scroll-area)"
```

---

## Task 6: Create domain types

**Files:**
- Create: `web/src/lib/seller/types.ts`

- [ ] **Step 1: Write `types.ts`**

```ts
// web/src/lib/seller/types.ts
export type ListingStatus = "active" | "low" | "out" | "draft";

export type Listing = {
  sku: string;
  name: string;
  category: string;
  price: number;
  inventory: number;
  status: ListingStatus;
  views7d: number;
};

export type OrderStatus = "paid" | "fulfilled" | "refunded" | "pending";

export type Order = {
  id: string;            // includes leading "#"
  customer: string;
  items: number;
  total: number;
  status: OrderStatus;
  placedAt: string;      // ISO date
};

export type KpiFormat = "currency" | "number" | "percent";

export type KpiPoint = {
  label: string;
  value: number;
  format: KpiFormat;
  delta?: number;        // signed percent vs prior period
};

export type RevenuePoint = { day: string; amount: number };

export type SourceBreakdown = { name: string; visits: number; share: number };

export type FunnelStage = { label: string; count: number };

export type TopProduct = { sku: string; name: string; units: number; revenue: number };

export type TodayItem = { label: string; count?: number };

export type ListingCounts = {
  total: number;
  active: number;
  low: number;
  out: number;
  draft: number;
};

export type Brand = { name: string; owner: string };
```

- [ ] **Step 2: Type-check**

```bash
npx tsc --noEmit
```
Expected: 0 errors.

- [ ] **Step 3: Commit**

```bash
git add src/lib/seller/types.ts
git commit -m "feat(seller): add domain types"
```

---

## Task 7: Write data invariant tests (TDD red)

**Files:**
- Create: `web/src/lib/seller/data.test.ts`

- [ ] **Step 1: Write failing tests**

```ts
// web/src/lib/seller/data.test.ts
import { describe, expect, it } from "vitest";
import {
  BRAND,
  TODAY,
  getAnalyticsKpis,
  getFunnel,
  getListingCounts,
  getListings,
  getOverviewKpis,
  getRangeOptions,
  getRecentOrders,
  getRevenueSeries,
  getSources,
  getTodayItems,
  getTopProducts,
} from "@/lib/seller/data";

describe("seller mock data", () => {
  it("identifies the brand and a fixed today", () => {
    expect(BRAND.name).toBe("Micro Commerce");
    expect(BRAND.owner).toBe("Alex");
    expect(TODAY).toBeInstanceOf(Date);
  });

  it("ships exactly 42 listings", () => {
    expect(getListings()).toHaveLength(42);
  });

  it("has unique listing SKUs", () => {
    const skus = getListings().map((l) => l.sku);
    expect(new Set(skus).size).toBe(skus.length);
  });

  it("listing counts sum to total and match the spec", () => {
    const c = getListingCounts();
    expect(c.total).toBe(42);
    expect(c.active).toBe(34);
    expect(c.low).toBe(3);
    expect(c.out).toBe(1);
    expect(c.draft).toBe(4);
    expect(c.active + c.low + c.out + c.draft).toBe(c.total);
  });

  it("counts derived from listings array equal getListingCounts", () => {
    const listings = getListings();
    const c = getListingCounts();
    expect(listings.filter((l) => l.status === "active").length).toBe(c.active);
    expect(listings.filter((l) => l.status === "low").length).toBe(c.low);
    expect(listings.filter((l) => l.status === "out").length).toBe(c.out);
    expect(listings.filter((l) => l.status === "draft").length).toBe(c.draft);
  });

  it("returns 7 revenue points", () => {
    expect(getRevenueSeries()).toHaveLength(7);
    for (const p of getRevenueSeries()) expect(p.amount).toBeGreaterThanOrEqual(0);
  });

  it("returns 5 recent orders with unique ids", () => {
    const orders = getRecentOrders();
    expect(orders).toHaveLength(5);
    const ids = orders.map((o) => o.id);
    expect(new Set(ids).size).toBe(5);
    for (const o of orders) expect(o.id.startsWith("#")).toBe(true);
  });

  it("returns 3 overview KPIs and 4 analytics KPIs with valid format", () => {
    expect(getOverviewKpis()).toHaveLength(3);
    expect(getAnalyticsKpis()).toHaveLength(4);
    for (const k of [...getOverviewKpis(), ...getAnalyticsKpis()]) {
      expect(["currency", "number", "percent"]).toContain(k.format);
      expect(k.value).toBeGreaterThanOrEqual(0);
    }
  });

  it("range options are 7d/30d/90d/Year", () => {
    expect(getRangeOptions()).toEqual(["7d", "30d", "90d", "Year"]);
  });

  it("conversion funnel has 5 stages and is monotonically non-increasing", () => {
    const stages = getFunnel();
    expect(stages).toHaveLength(5);
    for (let i = 1; i < stages.length; i++) {
      expect(stages[i].count).toBeLessThanOrEqual(stages[i - 1].count);
    }
  });

  it("source shares sum to ~1", () => {
    const total = getSources().reduce((s, x) => s + x.share, 0);
    expect(total).toBeGreaterThan(0.999);
    expect(total).toBeLessThan(1.001);
  });

  it("returns at least 5 top products with positive revenue", () => {
    const top = getTopProducts();
    expect(top.length).toBeGreaterThanOrEqual(5);
    for (const p of top) expect(p.revenue).toBeGreaterThan(0);
  });

  it("today items are non-empty", () => {
    expect(getTodayItems().length).toBeGreaterThan(0);
  });
});
```

- [ ] **Step 2: Run and confirm failure**

```bash
npm test -- src/lib/seller/data.test.ts
```
Expected: failure ("Cannot find module '@/lib/seller/data'").

- [ ] **Step 3: Commit**

```bash
git add src/lib/seller/data.test.ts
git commit -m "test(seller): add data invariant tests (red)"
```

---

## Task 8: Implement mock data module (turn data tests green)

**Files:**
- Create: `web/src/lib/seller/data.ts`

- [ ] **Step 1: Implement `data.ts`**

Write a deterministic mock-data module exporting the API consumed by the test in Task 7. Required content:

- `BRAND = { name: "Micro Commerce", owner: "Alex" }`
- `TODAY = new Date("2026-04-08T09:00:00.000Z")` (a Tuesday — the e2e spec asserts "Tuesday · April 8")
- `getRangeOptions(): ["7d", "30d", "90d", "Year"]`
- `getOverviewKpis()` returns 3 entries: `{ label: "Revenue · 7 days", value: 4280, format: "currency", delta: 12 }`, `{ label: "Orders · 7 days", value: 38, format: "number", delta: 8 }`, `{ label: "Storefront views", value: 2304, format: "number", delta: -3 }`
- `getAnalyticsKpis()` returns 4 entries: Revenue 12480 currency, Orders 132 number, Conversion 3.4 percent, Avg. order 94.55 currency — each with a non-zero delta
- `getRevenueSeries()` returns 7 entries with day labels Mon..Sun and varied amounts that sum near 4280
- `getRecentOrders()` returns 5 orders with ids `"#1042"..."#1038"` (descending)
- `getTodayItems()` returns at least one entry whose `label` reads "Pack 2 orders ready to ship"
- `getListings()` returns **42** listings with mutually exclusive statuses summing to 34/3/1/4. SKUs use prefix `MC-` (e.g. `MC-VS-001`). Names from the ceramics catalog (Persimmon vase, Forest bowl, Cream tumbler, Indigo carafe, Shadow vase tall, Rust mug Nº 04, Sage stoneware plate, Ember tea bowl, etc.). The first 9 SKUs (after sorting by listing-table display order) MUST match the e2e expectation: `MC-VS-001, MC-VS-002, MC-BW-014, MC-TB-007, MC-CR-003, MC-PL-022, MC-MG-041, MC-SC-008, MC-VS-031`. The list MUST contain a listing named "Persimmon vase".
- `getListingCounts()` derives from `getListings()` and equals `{ total: 42, active: 34, low: 3, out: 1, draft: 4 }`
- `getSources()` returns at least four entries; the entry named "Organic search" has `visits: 2304`; shares sum to 1.0
- `getTopProducts()` returns ≥5 entries; one of them is "Persimmon vase"
- `getFunnel()` returns 5 entries with labels `"Storefront views" → "Product views" → "Added to cart" → "Checkout started" → "Purchased"`, monotonically non-increasing counts (e.g. `12000, 8400, 2300, 1100, 410`)

Keep the module a single file. No imports from React, Next, or anything client-side. No `Date.now()`, no `Math.random()`.

Skeleton:

```ts
// web/src/lib/seller/data.ts
import type {
  Brand, FunnelStage, KpiPoint, Listing, ListingCounts, Order,
  RevenuePoint, SourceBreakdown, TodayItem, TopProduct,
} from "./types";

export const BRAND: Brand = { name: "Micro Commerce", owner: "Alex" };
export const TODAY = new Date("2026-04-08T09:00:00.000Z");

const RANGE_OPTIONS = ["7d", "30d", "90d", "Year"] as const;
export function getRangeOptions(): string[] {
  return [...RANGE_OPTIONS];
}

const OVERVIEW_KPIS: KpiPoint[] = [
  { label: "Revenue · 7 days", value: 4280, format: "currency", delta: 12 },
  { label: "Orders · 7 days", value: 38, format: "number", delta: 8 },
  { label: "Storefront views", value: 2304, format: "number", delta: -3 },
];
export function getOverviewKpis(): KpiPoint[] { return OVERVIEW_KPIS; }

// ... (analytics KPIs, revenue series, today items, recent orders, listings catalog,
//      sources, top products, funnel — all hard-coded constants)

const LISTINGS: Listing[] = [
  { sku: "MC-VS-001", name: "Persimmon vase",     category: "Vessels",   price: 86, inventory: 24, status: "active", views7d: 412 },
  { sku: "MC-VS-002", name: "Shadow vase, tall",  category: "Vessels",   price: 124, inventory: 6,  status: "active", views7d: 281 },
  { sku: "MC-BW-014", name: "Forest bowl",        category: "Tableware", price: 68,  inventory: 11, status: "active", views7d: 198 },
  { sku: "MC-TB-007", name: "Cream tumbler · 2 pk", category: "Drinkware", price: 54, inventory: 32, status: "active", views7d: 174 },
  { sku: "MC-CR-003", name: "Indigo carafe",      category: "Drinkware", price: 110, inventory: 9,  status: "active", views7d: 153 },
  { sku: "MC-PL-022", name: "Sage stoneware plate", category: "Tableware", price: 42, inventory: 48, status: "active", views7d: 128 },
  { sku: "MC-MG-041", name: "Rust mug Nº 04",     category: "Drinkware", price: 28,  inventory: 0,  status: "out",    views7d: 117 },
  { sku: "MC-SC-008", name: "Ember tea bowl",     category: "Drinkware", price: 36,  inventory: 2,  status: "low",    views7d: 98 },
  { sku: "MC-VS-031", name: "Persimmon vase, micro", category: "Vessels", price: 48, inventory: 14, status: "active", views7d: 84 },
  // ... 33 more listings to total 42, with statuses mixed to satisfy 34/3/1/4
];
export function getListings(): Listing[] { return LISTINGS; }

export function getListingCounts(): ListingCounts {
  const c = { total: LISTINGS.length, active: 0, low: 0, out: 0, draft: 0 };
  for (const l of LISTINGS) c[l.status] += 1;
  return c;
}

// ... remaining selectors
```

The full 42-listing array, sources, top products, and funnel must be authored in full. Use whichever distribution satisfies the invariants — the test will fail loudly if you drift.

- [ ] **Step 2: Run unit tests**

```bash
npm test -- src/lib/seller/data.test.ts
```
Expected: all green.

- [ ] **Step 3: Run the existing money tests**

```bash
npm test
```
Expected: all green (money + seller).

- [ ] **Step 4: Commit**

```bash
git add src/lib/seller/data.ts
git commit -m "feat(seller): implement deterministic mock data module"
```

---

## Task 9: Sidebar component + active-link client island

**Files:**
- Create: `web/src/components/seller/sidebar.tsx`
- Create: `web/src/components/seller/sidebar-nav.tsx`

- [ ] **Step 1: Write the sidebar (Server Component)**

```tsx
// web/src/components/seller/sidebar.tsx
import { BRAND } from "@/lib/seller/data";
import { SidebarNav } from "@/components/seller/sidebar-nav";

const NAV = [
  { label: "Overview",  href: "/seller" },
  { label: "Orders",    href: "/seller/orders" },
  { label: "Listings",  href: "/seller/listings" },
  { label: "Analytics", href: "/seller/analytics" },
  { label: "Customers", href: "/seller/customers" },
] as const;

export function SellerSidebar() {
  return (
    <aside className="flex h-screen w-60 flex-col border-r border-black/[0.06] bg-[#f5f5f7]">
      <div className="flex h-16 items-center px-6 text-[15px] font-semibold tracking-tight">
        {BRAND.name}
      </div>
      <SidebarNav items={NAV} />
    </aside>
  );
}
```

- [ ] **Step 2: Write the active-link client island**

```tsx
// web/src/components/seller/sidebar-nav.tsx
"use client";

import Link from "next/link";
import { usePathname } from "next/navigation";
import { cn } from "@/lib/utils";

type Item = { label: string; href: string };

export function SidebarNav({ items }: { items: readonly Item[] }) {
  const pathname = usePathname();
  return (
    <nav className="flex flex-1 flex-col gap-0.5 px-3 py-4">
      {items.map((item) => {
        const active =
          item.href === "/seller"
            ? pathname === "/seller"
            : pathname === item.href || pathname.startsWith(`${item.href}/`);
        return (
          <Link
            key={item.href}
            href={item.href}
            className={cn(
              "relative rounded-md px-3 py-2 text-sm text-[#1d1d1f] transition-colors",
              active ? "bg-white font-semibold shadow-[inset_2px_0_0_#0066cc]" : "hover:bg-white/60",
            )}
          >
            {item.label}
          </Link>
        );
      })}
    </nav>
  );
}
```

- [ ] **Step 3: Type-check**

```bash
npx tsc --noEmit
```
Expected: 0 errors.

- [ ] **Step 4: Commit**

```bash
git add src/components/seller/sidebar.tsx src/components/seller/sidebar-nav.tsx
git commit -m "feat(seller): add sidebar shell with active-link island"
```

---

## Task 10: Seller layout + stub routes

**Files:**
- Create: `web/src/app/seller/layout.tsx`
- Create: `web/src/app/seller/orders/page.tsx`
- Create: `web/src/app/seller/customers/page.tsx`

- [ ] **Step 1: Write the layout**

```tsx
// web/src/app/seller/layout.tsx
import { SellerSidebar } from "@/components/seller/sidebar";

export default function SellerLayout({ children }: { children: React.ReactNode }) {
  return (
    <div className="grid min-h-screen grid-cols-[240px_1fr] bg-white text-[#1d1d1f]">
      <SellerSidebar />
      <main className="min-w-0">{children}</main>
    </div>
  );
}
```

- [ ] **Step 2: Write the two stub pages**

```tsx
// web/src/app/seller/orders/page.tsx
export default function OrdersPage() {
  return (
    <section className="p-10">
      <h1 className="text-2xl font-semibold tracking-tight">Orders</h1>
      <p className="mt-2 text-sm text-[#1d1d1f]/70">Coming soon</p>
    </section>
  );
}
```

```tsx
// web/src/app/seller/customers/page.tsx
export default function CustomersPage() {
  return (
    <section className="p-10">
      <h1 className="text-2xl font-semibold tracking-tight">Customers</h1>
      <p className="mt-2 text-sm text-[#1d1d1f]/70">Coming soon</p>
    </section>
  );
}
```

- [ ] **Step 3: Confirm dev server runs the stub routes**

```bash
npm run dev &  # background
sleep 4
curl -s -o /dev/null -w "%{http_code}\n" http://localhost:3000/seller/orders
curl -s -o /dev/null -w "%{http_code}\n" http://localhost:3000/seller/customers
kill %1
```
Expected: `200` for both.

- [ ] **Step 4: Commit**

```bash
git add src/app/seller
git commit -m "feat(seller): add layout shell + Orders/Customers stub routes"
```

---

## Task 11: Overview-page subcomponents

**Files:**
- Create: `web/src/components/seller/kpi-card.tsx`
- Create: `web/src/components/seller/revenue-chart.tsx`
- Create: `web/src/components/seller/today-panel.tsx`
- Create: `web/src/components/seller/recent-orders.tsx`

- [ ] **Step 1: KPI card**

```tsx
// web/src/components/seller/kpi-card.tsx
import type { KpiPoint } from "@/lib/seller/types";
import { money } from "@/lib/money";

function format(value: number, fmt: KpiPoint["format"]) {
  if (fmt === "currency") return money(value);
  if (fmt === "percent") return `${value.toFixed(1)}%`;
  return value.toLocaleString("en-US");
}

export function KpiCard({ kpi }: { kpi: KpiPoint }) {
  const positive = (kpi.delta ?? 0) >= 0;
  return (
    <div className="rounded-lg border border-black/[0.06] bg-white p-5">
      <div className="text-[13px] text-[#1d1d1f]/70">{kpi.label}</div>
      <div className="mt-1 text-2xl font-semibold tracking-tight">{format(kpi.value, kpi.format)}</div>
      {kpi.delta !== undefined && (
        <div className={positive ? "mt-1 text-xs text-[#0066cc]" : "mt-1 text-xs text-[#1d1d1f]/60"}>
          {positive ? "▲" : "▼"} {Math.abs(kpi.delta)}%
        </div>
      )}
    </div>
  );
}
```

- [ ] **Step 2: Revenue chart (SVG)**

```tsx
// web/src/components/seller/revenue-chart.tsx
import type { RevenuePoint } from "@/lib/seller/types";

export function RevenueChart({ points }: { points: RevenuePoint[] }) {
  const max = Math.max(...points.map((p) => p.amount), 1);
  const w = 600;
  const h = 180;
  const barW = w / points.length / 2;
  return (
    <div className="rounded-lg border border-black/[0.06] bg-white p-5">
      <h2 className="text-base font-semibold tracking-tight">Revenue · 7 days</h2>
      <svg
        viewBox={`0 0 ${w} ${h + 30}`}
        role="img"
        aria-label="Revenue last 7 days"
        className="mt-4 w-full"
      >
        {points.map((p, i) => {
          const barH = Math.round((p.amount / max) * h);
          const x = (i + 0.25) * (w / points.length);
          return (
            <g key={p.day}>
              <rect x={x} y={h - barH} width={barW} height={barH} rx={3} fill="#1d1d1f" />
              <text x={x + barW / 2} y={h + 18} textAnchor="middle" fontSize="11" fill="#1d1d1f99">
                {p.day}
              </text>
            </g>
          );
        })}
      </svg>
    </div>
  );
}
```

- [ ] **Step 3: Today panel**

```tsx
// web/src/components/seller/today-panel.tsx
import type { TodayItem } from "@/lib/seller/types";

export function TodayPanel({ items }: { items: TodayItem[] }) {
  return (
    <div className="rounded-lg border border-black/[0.06] bg-white p-5">
      <h2 className="text-base font-semibold tracking-tight">Today</h2>
      <ul className="mt-4 space-y-2 text-sm">
        {items.map((it) => (
          <li key={it.label} className="flex items-start gap-2">
            <span className="mt-1.5 inline-block h-1.5 w-1.5 rounded-full bg-[#0066cc]" />
            <span>{it.label}</span>
          </li>
        ))}
      </ul>
    </div>
  );
}
```

- [ ] **Step 4: Recent orders table**

```tsx
// web/src/components/seller/recent-orders.tsx
import type { Order } from "@/lib/seller/types";
import { money } from "@/lib/money";
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table";

export function RecentOrders({ orders }: { orders: Order[] }) {
  return (
    <div className="rounded-lg border border-black/[0.06] bg-white p-5">
      <h2 className="text-base font-semibold tracking-tight">Recent orders</h2>
      <Table className="mt-4">
        <TableHeader>
          <TableRow>
            <TableHead>Order</TableHead>
            <TableHead>Customer</TableHead>
            <TableHead className="text-right">Items</TableHead>
            <TableHead className="text-right">Total</TableHead>
            <TableHead>Status</TableHead>
          </TableRow>
        </TableHeader>
        <TableBody>
          {orders.map((o) => (
            <TableRow key={o.id}>
              <TableCell className="font-mono">{o.id}</TableCell>
              <TableCell>{o.customer}</TableCell>
              <TableCell className="text-right">{o.items}</TableCell>
              <TableCell className="text-right">{money(o.total)}</TableCell>
              <TableCell className="capitalize">{o.status}</TableCell>
            </TableRow>
          ))}
        </TableBody>
      </Table>
    </div>
  );
}
```

- [ ] **Step 5: Type-check**

```bash
npx tsc --noEmit
```
Expected: 0 errors.

- [ ] **Step 6: Commit**

```bash
git add src/components/seller
git commit -m "feat(seller): add overview subcomponents (KPI, chart, today, recent-orders)"
```

---

## Task 12: Implement `/seller` page → e2e green

**Files:**
- Create: `web/src/app/seller/page.tsx`

- [ ] **Step 1: Write the overview page**

```tsx
// web/src/app/seller/page.tsx
import {
  BRAND, TODAY,
  getOverviewKpis, getRecentOrders, getRevenueSeries, getTodayItems,
} from "@/lib/seller/data";
import { KpiCard } from "@/components/seller/kpi-card";
import { RecentOrders } from "@/components/seller/recent-orders";
import { RevenueChart } from "@/components/seller/revenue-chart";
import { TodayPanel } from "@/components/seller/today-panel";

const dateLabel = (d: Date) =>
  d.toLocaleDateString("en-US", { weekday: "long", month: "long", day: "numeric" })
    .replace(",", " ·");

export default function SellerOverview() {
  const kpis = getOverviewKpis();
  return (
    <section className="px-10 py-8">
      <header className="flex items-center justify-between">
        <div>
          <p className="text-xs uppercase tracking-wider text-[#1d1d1f]/60">{dateLabel(TODAY)}</p>
          <h1 className="mt-1 text-3xl font-semibold tracking-tight">
            Good morning, {BRAND.owner}
          </h1>
        </div>
      </header>

      <div className="mt-8 grid grid-cols-3 gap-4">
        {kpis.map((k) => (<KpiCard key={k.label} kpi={k} />))}
      </div>

      <div className="mt-6 grid grid-cols-3 gap-4">
        <div className="col-span-2"><RevenueChart points={getRevenueSeries()} /></div>
        <TodayPanel items={getTodayItems()} />
      </div>

      <div className="mt-6">
        <RecentOrders orders={getRecentOrders()} />
      </div>
    </section>
  );
}
```

> **Note on the date label.** The e2e spec asserts the literal string `"Tuesday · April 8"`. `toLocaleDateString("en-US", { weekday, month, day })` returns `"Tuesday, April 8"` — the `.replace(",", " ·")` step turns the comma into the design's middle-dot separator. Verify locally that it matches before running the spec.

- [ ] **Step 2: Run the seller overview e2e**

```bash
npm run e2e -- e2e/seller.spec.ts
```
Expected: PASS.

- [ ] **Step 3: Commit**

```bash
git add src/app/seller/page.tsx
git commit -m "feat(seller): implement overview page (e2e green)"
```

---

## Task 13: Filter chips component (TDD red→green)

**Files:**
- Create: `web/src/components/seller/filter-chips.test.tsx`
- Create: `web/src/components/seller/filter-chips.tsx`

- [ ] **Step 1: Write the failing component test**

```tsx
// web/src/components/seller/filter-chips.test.tsx
import { render, screen } from "@testing-library/react";
import { describe, expect, it } from "vitest";
import { FilterChips } from "@/components/seller/filter-chips";

describe("FilterChips", () => {
  const counts = { total: 42, active: 34, low: 3, out: 1, draft: 4 };

  it("renders five chips with exact label/count format", () => {
    render(<FilterChips counts={counts} active="all" />);
    for (const text of ["All · 42", "Active · 34", "Low · 3", "Out · 1", "Drafts · 4"]) {
      expect(screen.getByText(text)).toBeInTheDocument();
    }
  });

  it("marks the active chip with aria-pressed=true and others with aria-pressed=false", () => {
    render(<FilterChips counts={counts} active="active" />);
    expect(screen.getByRole("button", { name: /Active · 34/ })).toHaveAttribute("aria-pressed", "true");
    expect(screen.getByRole("button", { name: /All · 42/ })).toHaveAttribute("aria-pressed", "false");
  });
});
```

- [ ] **Step 2: Run and confirm failure**

```bash
npm test -- src/components/seller/filter-chips.test.tsx
```
Expected: failure ("Cannot find module").

- [ ] **Step 3: Implement `filter-chips.tsx`**

```tsx
// web/src/components/seller/filter-chips.tsx
import type { ListingCounts } from "@/lib/seller/types";
import { cn } from "@/lib/utils";

export type FilterKey = "all" | "active" | "low" | "out" | "draft";

export function FilterChips({
  counts,
  active,
}: {
  counts: ListingCounts;
  active: FilterKey;
}) {
  const chips: { key: FilterKey; label: string; count: number }[] = [
    { key: "all",    label: "All",     count: counts.total },
    { key: "active", label: "Active",  count: counts.active },
    { key: "low",    label: "Low",     count: counts.low },
    { key: "out",    label: "Out",     count: counts.out },
    { key: "draft",  label: "Drafts",  count: counts.draft },
  ];
  return (
    <div className="flex flex-wrap gap-2">
      {chips.map((c) => {
        const isActive = c.key === active;
        return (
          <button
            key={c.key}
            type="button"
            aria-pressed={isActive}
            className={cn(
              "rounded-full border px-3 py-1 text-xs",
              isActive
                ? "border-[#0066cc] text-[#0066cc] ring-1 ring-[#0066cc]"
                : "border-black/[0.08] text-[#1d1d1f]",
            )}
          >
            {c.label} · {c.count}
          </button>
        );
      })}
    </div>
  );
}
```

- [ ] **Step 4: Run and confirm green**

```bash
npm test -- src/components/seller/filter-chips.test.tsx
```
Expected: PASS.

- [ ] **Step 5: Commit**

```bash
git add src/components/seller/filter-chips.tsx src/components/seller/filter-chips.test.tsx
git commit -m "feat(seller): add filter chips with active state (test green)"
```

---

## Task 14: Listings table component (TDD red→green)

**Files:**
- Create: `web/src/components/seller/listings-table.test.tsx`
- Create: `web/src/components/seller/listings-table.tsx`

- [ ] **Step 1: Write the failing test**

```tsx
// web/src/components/seller/listings-table.test.tsx
import { render, screen } from "@testing-library/react";
import { describe, expect, it } from "vitest";
import { ListingsTable } from "@/components/seller/listings-table";
import type { Listing } from "@/lib/seller/types";

const make = (n: number): Listing[] =>
  Array.from({ length: n }, (_, i) => ({
    sku: `MC-XX-${String(i + 1).padStart(3, "0")}`,
    name: `Item ${i + 1}`,
    category: "Vessels",
    price: 10,
    inventory: 5,
    status: "active" as const,
    views7d: 0,
  }));

describe("ListingsTable", () => {
  it("renders pageSize rows and the 'X of Y shown' footer", () => {
    render(<ListingsTable listings={make(42)} pageSize={9} />);
    expect(screen.getAllByRole("row")).toHaveLength(9 + 1); // 9 body + 1 header
    expect(screen.getByText("9 of 42 shown")).toBeInTheDocument();
  });

  it("clamps pageSize to total when total < pageSize", () => {
    render(<ListingsTable listings={make(5)} pageSize={9} />);
    expect(screen.getAllByRole("row")).toHaveLength(5 + 1);
    expect(screen.getByText("5 of 5 shown")).toBeInTheDocument();
  });
});
```

- [ ] **Step 2: Run and confirm failure**

```bash
npm test -- src/components/seller/listings-table.test.tsx
```
Expected: failure ("Cannot find module").

- [ ] **Step 3: Implement `listings-table.tsx`**

```tsx
// web/src/components/seller/listings-table.tsx
import type { Listing } from "@/lib/seller/types";
import { money } from "@/lib/money";
import { cn } from "@/lib/utils";
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table";

const STATUS_STYLES: Record<Listing["status"], string> = {
  active: "bg-emerald-50 text-emerald-700",
  low:    "bg-amber-50 text-amber-800",
  out:    "bg-rose-50 text-rose-700",
  draft:  "bg-zinc-100 text-zinc-700",
};

export function ListingsTable({
  listings,
  pageSize = 9,
}: {
  listings: Listing[];
  pageSize?: number;
}) {
  const visible = Math.min(pageSize, listings.length);
  const rows = listings.slice(0, visible);
  return (
    <div className="rounded-lg border border-black/[0.06] bg-white">
      <Table>
        <TableHeader>
          <TableRow>
            <TableHead>SKU</TableHead>
            <TableHead>Product</TableHead>
            <TableHead>Category</TableHead>
            <TableHead className="text-right">Price</TableHead>
            <TableHead className="text-right">Inv.</TableHead>
            <TableHead>Status</TableHead>
            <TableHead className="text-right">Views (7d)</TableHead>
          </TableRow>
        </TableHeader>
        <TableBody>
          {rows.map((l) => (
            <TableRow key={l.sku}>
              <TableCell className="font-mono">{l.sku}</TableCell>
              <TableCell>{l.name}</TableCell>
              <TableCell>{l.category}</TableCell>
              <TableCell className="text-right">{money(l.price)}</TableCell>
              <TableCell className="text-right">{l.inventory}</TableCell>
              <TableCell>
                <span className={cn("rounded-full px-2 py-0.5 text-xs capitalize", STATUS_STYLES[l.status])}>
                  {l.status}
                </span>
              </TableCell>
              <TableCell className="text-right">{l.views7d}</TableCell>
            </TableRow>
          ))}
        </TableBody>
      </Table>
      <div className="border-t border-black/[0.06] px-4 py-3 text-xs text-[#1d1d1f]/60">
        {visible} of {listings.length} shown
      </div>
    </div>
  );
}
```

- [ ] **Step 4: Run and confirm green**

```bash
npm test -- src/components/seller/listings-table.test.tsx
```
Expected: PASS.

- [ ] **Step 5: Commit**

```bash
git add src/components/seller/listings-table.tsx src/components/seller/listings-table.test.tsx
git commit -m "feat(seller): add listings table with pageSize footer (test green)"
```

---

## Task 15: Implement `/seller/listings` page → e2e green

**Files:**
- Create: `web/src/app/seller/listings/page.tsx`

- [ ] **Step 1: Write the page**

```tsx
// web/src/app/seller/listings/page.tsx
import { Button } from "@/components/ui/button";
import { FilterChips } from "@/components/seller/filter-chips";
import { ListingsTable } from "@/components/seller/listings-table";
import { getListingCounts, getListings } from "@/lib/seller/data";

export default function ListingsPage() {
  const counts = getListingCounts();
  const published = counts.active + counts.low + counts.out;
  return (
    <section className="px-10 py-8">
      <header className="flex flex-wrap items-end justify-between gap-4">
        <div>
          <h1 className="text-3xl font-semibold tracking-tight">Listings</h1>
          <p className="mt-1 text-sm text-[#1d1d1f]/70">
            {counts.total} listings · {published} published
          </p>
        </div>
        <div className="flex gap-2">
          <Button variant="outline" className="rounded-full">Import CSV</Button>
          <Button className="rounded-full">New listing</Button>
        </div>
      </header>

      <div className="mt-6">
        <FilterChips counts={counts} active="all" />
      </div>

      <div className="mt-6">
        <ListingsTable listings={getListings()} pageSize={9} />
      </div>
    </section>
  );
}
```

- [ ] **Step 2: Run the listings e2e**

```bash
npm run e2e -- e2e/seller-listings.spec.ts
```
Expected: both tests PASS.

- [ ] **Step 3: Commit**

```bash
git add src/app/seller/listings/page.tsx
git commit -m "feat(seller): implement listings page (e2e green)"
```

---

## Task 16: Conversion funnel component (TDD red→green)

**Files:**
- Create: `web/src/components/seller/conversion-funnel.test.tsx`
- Create: `web/src/components/seller/conversion-funnel.tsx`

- [ ] **Step 1: Write the failing test**

```tsx
// web/src/components/seller/conversion-funnel.test.tsx
import { render, screen, within } from "@testing-library/react";
import { describe, expect, it } from "vitest";
import { ConversionFunnel } from "@/components/seller/conversion-funnel";

describe("ConversionFunnel", () => {
  const stages = [
    { label: "Storefront views",  count: 12000 },
    { label: "Product views",     count: 8400 },
    { label: "Added to cart",     count: 2300 },
    { label: "Checkout started",  count: 1100 },
    { label: "Purchased",         count: 410 },
  ];

  it("renders one row per stage in order", () => {
    render(<ConversionFunnel stages={stages} />);
    const list = screen.getByRole("list");
    const items = within(list).getAllByRole("listitem");
    expect(items).toHaveLength(5);
    expect(items[0]).toHaveTextContent("Storefront views");
    expect(items[4]).toHaveTextContent("Purchased");
  });
});
```

- [ ] **Step 2: Run and confirm failure**

```bash
npm test -- src/components/seller/conversion-funnel.test.tsx
```
Expected: failure ("Cannot find module").

- [ ] **Step 3: Implement `conversion-funnel.tsx`**

```tsx
// web/src/components/seller/conversion-funnel.tsx
import type { FunnelStage } from "@/lib/seller/types";

export function ConversionFunnel({ stages }: { stages: FunnelStage[] }) {
  const top = stages[0]?.count || 1;
  return (
    <div className="rounded-lg border border-black/[0.06] bg-white p-5">
      <h2 className="text-base font-semibold tracking-tight">Conversion funnel</h2>
      <ul className="mt-4 space-y-3">
        {stages.map((s) => {
          const pct = (s.count / top) * 100;
          return (
            <li key={s.label}>
              <div className="flex items-baseline justify-between text-sm">
                <span>{s.label}</span>
                <span className="tabular-nums text-[#1d1d1f]/70">
                  {s.count.toLocaleString("en-US")}
                </span>
              </div>
              <div className="mt-1 h-2 w-full rounded-full bg-black/[0.05]">
                <div
                  className="h-2 rounded-full bg-[#1d1d1f]"
                  style={{ width: `${pct}%` }}
                  aria-hidden
                />
              </div>
            </li>
          );
        })}
      </ul>
    </div>
  );
}
```

- [ ] **Step 4: Run and confirm green**

```bash
npm test -- src/components/seller/conversion-funnel.test.tsx
```
Expected: PASS.

- [ ] **Step 5: Commit**

```bash
git add src/components/seller/conversion-funnel.tsx src/components/seller/conversion-funnel.test.tsx
git commit -m "feat(seller): add conversion funnel (test green)"
```

---

## Task 17: Remaining analytics subcomponents

**Files:**
- Create: `web/src/components/seller/range-tabs.tsx`
- Create: `web/src/components/seller/sources-donut.tsx`
- Create: `web/src/components/seller/top-products.tsx`
- Create: `web/src/components/seller/analytics-kpi-row.tsx`

- [ ] **Step 1: Range tabs (Client island for default-active selection)**

```tsx
// web/src/components/seller/range-tabs.tsx
"use client";

import { useState } from "react";
import { cn } from "@/lib/utils";

export function RangeTabs({ options, defaultValue }: { options: string[]; defaultValue: string }) {
  const [active, setActive] = useState(defaultValue);
  return (
    <div className="inline-flex rounded-full border border-black/[0.08] p-0.5">
      {options.map((opt) => (
        <button
          key={opt}
          type="button"
          onClick={() => setActive(opt)}
          aria-pressed={active === opt}
          className={cn(
            "rounded-full px-3 py-1 text-xs",
            active === opt ? "bg-[#1d1d1f] text-white" : "text-[#1d1d1f]/70",
          )}
        >
          {opt}
        </button>
      ))}
    </div>
  );
}
```

- [ ] **Step 2: Sources donut (SVG)**

```tsx
// web/src/components/seller/sources-donut.tsx
import type { SourceBreakdown } from "@/lib/seller/types";

const PALETTE = ["#1d1d1f", "#0066cc", "#7a7a7a", "#cccccc", "#aacbe7"];

export function SourcesDonut({ sources }: { sources: SourceBreakdown[] }) {
  const total = sources.reduce((s, x) => s + x.visits, 0);
  let acc = 0;
  const r = 60;
  const c = 2 * Math.PI * r;

  return (
    <div className="rounded-lg border border-black/[0.06] bg-white p-5">
      <h2 className="text-base font-semibold tracking-tight">Sources</h2>
      <div className="mt-4 flex items-center gap-6">
        <svg viewBox="-80 -80 160 160" className="h-40 w-40">
          {sources.map((s, i) => {
            const len = s.share * c;
            const dasharray = `${len} ${c - len}`;
            const dashoffset = -acc;
            acc += len;
            return (
              <circle
                key={s.name}
                r={r}
                cx={0}
                cy={0}
                fill="transparent"
                stroke={PALETTE[i % PALETTE.length]}
                strokeWidth={18}
                strokeDasharray={dasharray}
                strokeDashoffset={dashoffset}
                transform="rotate(-90)"
              />
            );
          })}
        </svg>
        <ul className="space-y-2 text-sm">
          {sources.map((s, i) => (
            <li key={s.name} className="flex items-center gap-3">
              <span
                aria-hidden
                className="inline-block h-2.5 w-2.5 rounded-full"
                style={{ background: PALETTE[i % PALETTE.length] }}
              />
              <span className="min-w-[8rem]">{s.name}</span>
              <span className="tabular-nums text-[#1d1d1f]/70">{s.visits.toLocaleString("en-US")}</span>
            </li>
          ))}
        </ul>
      </div>
      <p className="mt-3 text-xs text-[#1d1d1f]/60">{total.toLocaleString("en-US")} visits</p>
    </div>
  );
}
```

- [ ] **Step 3: Top products list**

```tsx
// web/src/components/seller/top-products.tsx
import type { TopProduct } from "@/lib/seller/types";
import { money } from "@/lib/money";

export function TopProducts({ products }: { products: TopProduct[] }) {
  const max = Math.max(...products.map((p) => p.revenue), 1);
  return (
    <div className="rounded-lg border border-black/[0.06] bg-white p-5">
      <h2 className="text-base font-semibold tracking-tight">Top products</h2>
      <ul className="mt-4 space-y-3">
        {products.map((p) => {
          const pct = (p.revenue / max) * 100;
          return (
            <li key={p.sku}>
              <div className="flex items-baseline justify-between text-sm">
                <span>{p.name}</span>
                <span className="tabular-nums text-[#1d1d1f]/70">
                  {p.units} · {money(p.revenue)}
                </span>
              </div>
              <div className="mt-1 h-1.5 w-full rounded-full bg-black/[0.05]">
                <div className="h-1.5 rounded-full bg-[#1d1d1f]" style={{ width: `${pct}%` }} aria-hidden />
              </div>
            </li>
          );
        })}
      </ul>
    </div>
  );
}
```

- [ ] **Step 4: Analytics KPI row**

```tsx
// web/src/components/seller/analytics-kpi-row.tsx
import type { KpiPoint } from "@/lib/seller/types";
import { KpiCard } from "@/components/seller/kpi-card";

export function AnalyticsKpiRow({ kpis }: { kpis: KpiPoint[] }) {
  return (
    <div className="grid grid-cols-2 gap-4 md:grid-cols-4">
      {kpis.map((k) => (<KpiCard key={k.label} kpi={k} />))}
    </div>
  );
}
```

- [ ] **Step 5: Type-check**

```bash
npx tsc --noEmit
```
Expected: 0 errors.

- [ ] **Step 6: Commit**

```bash
git add src/components/seller
git commit -m "feat(seller): add analytics subcomponents (range, donut, top products, KPI row)"
```

---

## Task 18: Implement `/seller/analytics` page → e2e green

**Files:**
- Create: `web/src/app/seller/analytics/page.tsx`

- [ ] **Step 1: Write the page**

```tsx
// web/src/app/seller/analytics/page.tsx
import { Button } from "@/components/ui/button";
import { AnalyticsKpiRow } from "@/components/seller/analytics-kpi-row";
import { ConversionFunnel } from "@/components/seller/conversion-funnel";
import { RangeTabs } from "@/components/seller/range-tabs";
import { SourcesDonut } from "@/components/seller/sources-donut";
import { TopProducts } from "@/components/seller/top-products";
import {
  getAnalyticsKpis, getFunnel, getRangeOptions, getSources, getTopProducts,
} from "@/lib/seller/data";

export default function AnalyticsPage() {
  return (
    <section className="px-10 py-8">
      <header className="flex flex-wrap items-end justify-between gap-4">
        <div>
          <h1 className="text-3xl font-semibold tracking-tight">Analytics</h1>
          <p className="mt-1 text-sm text-[#1d1d1f]/70">Apr 1 – Apr 30 · vs Mar 1 – Mar 30</p>
        </div>
        <div className="flex items-center gap-3">
          <RangeTabs options={getRangeOptions()} defaultValue="30d" />
          <Button variant="outline" className="rounded-full">Export</Button>
        </div>
      </header>

      <div className="mt-8">
        <AnalyticsKpiRow kpis={getAnalyticsKpis()} />
      </div>

      <div className="mt-6 grid gap-4 md:grid-cols-2">
        <SourcesDonut sources={getSources()} />
        <TopProducts products={getTopProducts()} />
      </div>

      <div className="mt-6">
        <ConversionFunnel stages={getFunnel()} />
      </div>
    </section>
  );
}
```

- [ ] **Step 2: Run analytics e2e**

```bash
npm run e2e -- e2e/seller-analytics.spec.ts
```
Expected: both tests PASS.

- [ ] **Step 3: Commit**

```bash
git add src/app/seller/analytics/page.tsx
git commit -m "feat(seller): implement analytics page (e2e green)"
```

---

## Task 19: Final verification gates

**Files:** none

- [ ] **Step 1: Lint**

```bash
cd web
npm run lint
```
Expected: 0 errors / 0 warnings. Fix anything Biome complains about — most commonly `import` order, since Biome auto-organizes.

- [ ] **Step 2: Production build (this is also the typecheck gate)**

```bash
npm run build
```
Expected: build succeeds. Treat any type error as a fix-loop entry.

- [ ] **Step 3: Full unit suite**

```bash
npm test
```
Expected: all green. (money + seller-data + filter-chips + listings-table + conversion-funnel.)

- [ ] **Step 4: Full e2e suite**

```bash
npm run e2e
```
Expected: 5 tests pass (1 in seller, 2 in listings, 2 in analytics).

- [ ] **Step 5: Commit any cleanup from steps 1–4**

```bash
git status
# if there are changes:
git add -A
git commit -m "chore(seller): final lint and build cleanup"
```

---

## Self-Review

**Spec coverage:**
- Delete 8 e2e specs → Task 1 ✓
- Three fresh seller e2e specs (TDD red) → Tasks 2–4 ✓
- shadcn primitives → Task 5 ✓
- Domain types → Task 6 ✓
- Data invariant tests + impl → Tasks 7–8 ✓
- Sidebar + active-link island → Task 9 ✓
- Layout + Orders/Customers stubs → Task 10 ✓
- Overview page + 4 subcomponents → Tasks 11–12 ✓
- FilterChips component test + impl → Task 13 ✓
- ListingsTable component test + impl → Task 14 ✓
- Listings page → Task 15 ✓
- ConversionFunnel component test + impl → Task 16 ✓
- Remaining analytics subcomponents → Task 17 ✓
- Analytics page → Task 18 ✓
- Lint + build + full suites → Task 19 ✓
- No tax tests anywhere ✓

**Type/name consistency:**
- `KpiPoint`, `Listing`, `Order`, `FunnelStage`, `SourceBreakdown`, `TopProduct`, `RevenuePoint`, `TodayItem`, `ListingCounts`, `Brand` defined in Task 6, used identically in all later tasks.
- Selectors `getOverviewKpis`, `getAnalyticsKpis`, `getRevenueSeries`, `getRecentOrders`, `getTodayItems`, `getListings`, `getListingCounts`, `getRangeOptions`, `getSources`, `getTopProducts`, `getFunnel` named identically in test (Task 7), impl (Task 8), and consumers (Tasks 12, 15, 18).
- Filter chip labels identical in component test (Task 13) and e2e spec (Task 3).

**Placeholder scan:** Task 8's data implementation lists the 9 deterministic SKUs and gives concrete distribution targets (34/3/1/4) — the remaining 33 listings are deferred to the engineer's catalog with explicit invariants. This is acceptable because the invariants are tested. No "TBD" / "TODO" / "implement later" strings.

**Risk hot-spot:** Task 12's date label depends on `toLocaleDateString` returning `"Tuesday, April 8"` then `.replace(",", " ·")`. If the runtime locale renders differently (very unlikely on Node 20+ Chromium playwright), substitute a hand-formatted helper. The note in Task 12 flags this.
