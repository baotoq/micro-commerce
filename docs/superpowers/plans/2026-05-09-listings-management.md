# End-to-end Listings Management Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add four static visual mockup routes (`/seller/listings/bulk`, `/seller/listings/[sku]/edit`, `/seller/listings/[sku]/preview`, `/seller/listings/published`) covering hi-fi steps `LFlow_02..05` of the listings-management flow.

**Architecture:** Server Components only. Each page is a single `page.tsx` with inline JSX that renders hardcoded hi-fi-faithful content (matching `/seller/listings/new`'s pattern). Edit and preview routes look up a listing via a new `getListingBySku` helper and call `notFound()` on miss. Per-route Playwright e2e specs validate hi-fi-faithful rendering. No client state, no persistence, no new shared components.

**Tech Stack:** Next.js 16 App Router, React 19 (Server Components), Tailwind v4 utility classes, shadcn-style `Button` primitive, Vitest unit tests, Playwright e2e.

**Reference spec:** `docs/superpowers/specs/2026-05-09-listings-management-design.md`.

**Run all commands from `web/` unless noted otherwise.**

---

## File Structure

**Modified:**
- `web/src/lib/seller/data.ts` — add `getListingBySku(sku: string): Listing | null` (one filter call, exported).
- `web/src/lib/seller/data.test.ts` — add tests for the new helper.

**Created:**
- `web/src/app/seller/listings/published/page.tsx` — LFlow_05 (no params).
- `web/src/app/seller/listings/bulk/page.tsx` — LFlow_02 (no params).
- `web/src/app/seller/listings/[sku]/edit/page.tsx` — LFlow_03 (params + `notFound()`).
- `web/src/app/seller/listings/[sku]/preview/page.tsx` — LFlow_04 (params + `notFound()`).
- `web/e2e/seller-listings-published.spec.ts`
- `web/e2e/seller-listings-bulk.spec.ts`
- `web/e2e/seller-listings-edit.spec.ts`
- `web/e2e/seller-listings-preview.spec.ts`

Order: helper → published → bulk → edit → preview. Each route is a self-contained commit.

---

## Task 1: `getListingBySku` data helper

**Files:**
- Modify: `web/src/lib/seller/data.ts` (append new export near `getListings`).
- Modify: `web/src/lib/seller/data.test.ts` (add new `it()` cases).

- [ ] **Step 1: Write the failing test**

Append the following block at the end of the existing top-level `describe("seller mock data", ...)` block in `web/src/lib/seller/data.test.ts` (before its closing `});`). Also add `getListingBySku` to the import list at the top of the file.

```typescript
import {
  BRAND,
  getAnalyticsKpis,
  getFunnel,
  getListingBySku,        // <-- add this
  getListingCounts,
  getListings,
  getOverviewKpis,
  getRangeOptions,
  getRecentOrders,
  getRevenueSeries,
  getSources,
  getTodayItems,
  getTopProducts,
  TODAY,
} from "@/lib/seller/data";
```

```typescript
  it("getListingBySku returns the matching listing", () => {
    const listing = getListingBySku("MC-VS-001");
    expect(listing).not.toBeNull();
    expect(listing?.sku).toBe("MC-VS-001");
    expect(listing?.name).toBe("Persimmon vase");
  });

  it("getListingBySku returns null for an unknown sku", () => {
    expect(getListingBySku("MC-NOPE-404")).toBeNull();
  });
```

- [ ] **Step 2: Run the test to verify it fails**

```bash
cd web && npm test -- src/lib/seller/data.test.ts
```

Expected: 2 failures complaining that `getListingBySku` is not exported (TS or runtime). The other 13 existing tests still pass.

- [ ] **Step 3: Implement the helper**

Edit `web/src/lib/seller/data.ts`. Find the existing `getListings` export and add the new helper directly after it:

```typescript
export function getListings(): Listing[] {
  return LISTINGS;
}

export function getListingBySku(sku: string): Listing | null {
  return LISTINGS.find((l) => l.sku === sku) ?? null;
}
```

- [ ] **Step 4: Run the test to verify it passes**

```bash
cd web && npm test -- src/lib/seller/data.test.ts
```

Expected: all tests pass (existing 13 + 2 new).

- [ ] **Step 5: Lint & build**

```bash
cd web && npm run lint && npm run build
```

Expected: Biome clean; Next build succeeds (no type errors).

- [ ] **Step 6: Commit**

```bash
git add web/src/lib/seller/data.ts web/src/lib/seller/data.test.ts
git commit -m "feat(seller): add getListingBySku helper"
```

---

## Task 2: `/seller/listings/published` route (LFlow_05)

**Files:**
- Create: `web/src/app/seller/listings/published/page.tsx`
- Create: `web/e2e/seller-listings-published.spec.ts`

- [ ] **Step 1: Write the failing e2e spec**

Create `web/e2e/seller-listings-published.spec.ts` with:

```typescript
// web/e2e/seller-listings-published.spec.ts
import { expect, test } from "@playwright/test";

test.describe("Seller listings — published", () => {
  test("renders success banner, KPI cards, and activity log", async ({
    page,
  }) => {
    await page.goto("/seller/listings/published");

    // Topbar
    await expect(
      page.getByRole("heading", { name: "Listings", exact: true }),
    ).toBeVisible();
    await expect(page.getByText("42 products · 39 active")).toBeVisible();
    await expect(
      page.getByRole("button", { name: "+ New listing" }),
    ).toBeVisible();

    // Success banner
    await expect(
      page.getByText(
        "Persimmon vase published · 2 variants updated, 1 went live.",
      ),
    ).toBeVisible();
    await expect(
      page.getByRole("button", { name: "View shop →" }),
    ).toBeVisible();
    await expect(page.getByRole("button", { name: "Undo" })).toBeVisible();

    // KPI cards
    for (const label of [
      "Active listings",
      "Variants in stock",
      "Out-of-stock items",
    ]) {
      await expect(page.getByText(label, { exact: true })).toBeVisible();
    }
    await expect(page.getByText("39", { exact: true })).toBeVisible();
    await expect(page.getByText("128", { exact: true })).toBeVisible();

    // Activity log heading + link
    await expect(
      page.getByRole("heading", { name: "What just changed" }),
    ).toBeVisible();
    await expect(
      page.getByText("Activity log →", { exact: true }),
    ).toBeVisible();

    // Activity rows
    for (const item of [
      "Persimmon vase · Medium",
      "Persimmon vase · Small",
      "Persimmon vase · Large",
      "Ember tea bowl",
      "Peat serving bowl",
    ]) {
      await expect(
        page.getByRole("cell", { name: item, exact: true }),
      ).toBeVisible();
    }
  });
});
```

- [ ] **Step 2: Run the e2e to verify it fails**

```bash
cd web && npm run e2e -- seller-listings-published
```

Expected: navigation 404 / route does not exist; assertions fail.

- [ ] **Step 3: Implement the page**

Create `web/src/app/seller/listings/published/page.tsx` with:

```tsx
// web/src/app/seller/listings/published/page.tsx
import { SellerTopbar } from "@/components/seller/seller-topbar";
import { Button } from "@/components/ui/button";

const KPIS = [
  { label: "Active listings", value: "39", delta: "+1" },
  { label: "Variants in stock", value: "128", delta: "+2" },
  { label: "Out-of-stock items", value: "1", delta: "−2" },
];

const ACTIVITY = [
  {
    when: "just now",
    item: "Persimmon vase · Medium",
    change: "Price · $86 → $95",
    by: "You",
  },
  {
    when: "just now",
    item: "Persimmon vase · Small",
    change: "Price · $64 → $70",
    by: "You",
  },
  {
    when: "just now",
    item: "Persimmon vase · Large",
    change: "Status · Out → still out (no stock)",
    by: "You",
  },
  {
    when: "12 min ago",
    item: "Ember tea bowl",
    change: "Price · $36 → $39.60",
    by: "You · bulk",
  },
  {
    when: "12 min ago",
    item: "Peat serving bowl",
    change: "Price · $90 → $99",
    by: "You · bulk",
  },
];

export default function ListingsPublishedPage() {
  return (
    <div className="flex min-h-screen flex-col">
      <SellerTopbar
        title="Listings"
        subtitle="42 products · 39 active"
        actions={<Button className="rounded-full">+ New listing</Button>}
      />

      <div className="border-b border-black/[0.06] bg-[#DDEDE1] px-7 py-3.5">
        <div className="flex items-center gap-3">
          <span
            aria-hidden="true"
            className="flex h-[26px] w-[26px] shrink-0 items-center justify-center rounded-full bg-emerald-600 text-sm text-white"
          >
            ✓
          </span>
          <span className="flex-1 text-[13.5px] font-semibold text-[#1d1d1f]">
            Persimmon vase published · 2 variants updated, 1 went live.
          </span>
          <Button variant="ghost" size="sm">
            View shop →
          </Button>
          <Button variant="ghost" size="sm">
            Undo
          </Button>
        </div>
      </div>

      <div className="flex-1 overflow-auto px-7 py-6">
        <div className="mb-5 grid grid-cols-3 gap-3">
          {KPIS.map((k) => (
            <div
              key={k.label}
              className="rounded-xl border border-black/[0.06] bg-white p-[18px]"
            >
              <div className="text-[11px] text-[#1d1d1f]/50">{k.label}</div>
              <div className="mt-1.5 flex items-end gap-2">
                <div className="text-[28px] font-semibold leading-none tabular-nums text-[#1d1d1f]">
                  {k.value}
                </div>
                <span className="mb-1 text-[11px] font-semibold text-emerald-600">
                  ↑ {k.delta}
                </span>
              </div>
            </div>
          ))}
        </div>

        <div className="rounded-xl border border-black/[0.06] bg-white">
          <div className="flex items-center justify-between border-b border-black/[0.06] px-5 py-3.5">
            <h2 className="text-[15px] font-semibold text-[#1d1d1f]">
              What just changed
            </h2>
            <span className="text-[13px] font-medium text-[#1d1d1f]/70">
              Activity log →
            </span>
          </div>
          <table className="w-full text-sm">
            <thead>
              <tr className="text-left text-[11px] uppercase tracking-wider text-[#1d1d1f]/50">
                <th className="px-5 py-2.5 font-medium">When</th>
                <th className="px-5 py-2.5 font-medium">Item</th>
                <th className="px-5 py-2.5 font-medium">Change</th>
                <th className="px-5 py-2.5 font-medium">By</th>
              </tr>
            </thead>
            <tbody>
              {ACTIVITY.map((r) => (
                <tr key={r.item} className="border-t border-black/[0.04]">
                  <td className="px-5 py-3 text-[11px] text-[#1d1d1f]/60">
                    {r.when}
                  </td>
                  <td className="px-5 py-3 font-medium text-[#1d1d1f]">
                    {r.item}
                  </td>
                  <td className="px-5 py-3 text-[#1d1d1f]/70">{r.change}</td>
                  <td className="px-5 py-3 text-[13px]">{r.by}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </div>
    </div>
  );
}
```

- [ ] **Step 4: Run the e2e to verify it passes**

```bash
cd web && npm run e2e -- seller-listings-published
```

Expected: all assertions pass.

- [ ] **Step 5: Lint & build**

```bash
cd web && npm run lint && npm run build
```

Expected: Biome clean; Next build succeeds.

- [ ] **Step 6: Commit**

```bash
git add web/src/app/seller/listings/published/page.tsx web/e2e/seller-listings-published.spec.ts
git commit -m "feat(seller): add listings published route (LFlow_05)"
```

---

## Task 3: `/seller/listings/bulk` route (LFlow_02)

**Files:**
- Create: `web/src/app/seller/listings/bulk/page.tsx`
- Create: `web/e2e/seller-listings-bulk.spec.ts`

- [ ] **Step 1: Write the failing e2e spec**

Create `web/e2e/seller-listings-bulk.spec.ts` with:

```typescript
// web/e2e/seller-listings-bulk.spec.ts
import { expect, test } from "@playwright/test";

test.describe("Seller listings — bulk", () => {
  test("renders filtered catalog, dark bulk-action bar, table, and bulk-edit drawer", async ({
    page,
  }) => {
    await page.goto("/seller/listings/bulk");

    // Topbar
    await expect(
      page.getByRole("heading", { name: "Listings", exact: true }),
    ).toBeVisible();
    await expect(
      page.getByText(/low & out of stock/i),
    ).toBeVisible();

    // Filter chips
    for (const chip of [
      "All · 42",
      "Active · 38",
      "Low · 3",
      "Out · 1",
      "Drafts · 4",
    ]) {
      await expect(page.getByText(chip, { exact: true })).toBeVisible();
    }

    // Bulk action bar
    await expect(page.getByText("3 of 4 selected")).toBeVisible();
    for (const action of ["Edit price", "Adjust stock", "Move to draft"]) {
      await expect(page.getByRole("button", { name: action })).toBeVisible();
    }
    await expect(page.getByRole("button", { name: "Apply →" })).toBeVisible();

    // Table rows
    for (const product of [
      "Rust mug Nº 04",
      "Ember tea bowl",
      "Peat serving bowl",
      "Mist tumbler",
    ]) {
      await expect(
        page.getByRole("cell", { name: product, exact: true }),
      ).toBeVisible();
    }
    for (const sku of [
      "MC-MG-041",
      "MC-SC-008",
      "MC-BW-051",
      "MC-TB-038",
    ]) {
      await expect(
        page.getByRole("cell", { name: sku, exact: true }),
      ).toBeVisible();
    }

    // Drawer
    await expect(page.getByText("Bulk edit · 3 items")).toBeVisible();
    await expect(
      page.getByRole("heading", { name: "Adjust price" }),
    ).toBeVisible();
    for (const seg of ["Set to", "Increase", "Decrease"]) {
      await expect(page.getByRole("button", { name: seg })).toBeVisible();
    }
    await expect(
      page.getByRole("button", { name: "Apply to 3 items" }),
    ).toBeVisible();
    await expect(page.getByRole("button", { name: "Cancel" })).toBeVisible();
  });
});
```

- [ ] **Step 2: Run the e2e to verify it fails**

```bash
cd web && npm run e2e -- seller-listings-bulk
```

Expected: 404 / assertions fail.

- [ ] **Step 3: Implement the page**

Create `web/src/app/seller/listings/bulk/page.tsx` with:

```tsx
// web/src/app/seller/listings/bulk/page.tsx
import { SellerTopbar } from "@/components/seller/seller-topbar";
import { Button } from "@/components/ui/button";

const CHIPS = [
  { label: "All · 42", active: false },
  { label: "Active · 38", active: false },
  { label: "Low · 3", active: true },
  { label: "Out · 1", active: true },
  { label: "Drafts · 4", active: false },
];

type Row = {
  name: string;
  sku: string;
  status: "Out" | "Low";
  stock: number;
  price: number;
  newPrice: number | null;
  selected: boolean;
};

const ROWS: Row[] = [
  {
    name: "Rust mug Nº 04",
    sku: "MC-MG-041",
    status: "Out",
    stock: 0,
    price: 28,
    newPrice: 30.8,
    selected: true,
  },
  {
    name: "Ember tea bowl",
    sku: "MC-SC-008",
    status: "Low",
    stock: 2,
    price: 36,
    newPrice: 39.6,
    selected: true,
  },
  {
    name: "Peat serving bowl",
    sku: "MC-BW-051",
    status: "Low",
    stock: 4,
    price: 90,
    newPrice: 99,
    selected: true,
  },
  {
    name: "Mist tumbler",
    sku: "MC-TB-038",
    status: "Low",
    stock: 3,
    price: 40,
    newPrice: null,
    selected: false,
  },
];

const money = (n: number) =>
  `$${n.toLocaleString("en-US", { minimumFractionDigits: n % 1 === 0 ? 0 : 2, maximumFractionDigits: 2 })}`;

const SELECTED = ROWS.filter((r) => r.selected);

export default function ListingsBulkPage() {
  return (
    <div className="flex min-h-screen flex-col">
      <SellerTopbar
        title="Listings"
        subtitle="Filtered · low & out of stock"
        actions={<Button className="rounded-full">+ New listing</Button>}
      />

      {/* Filter chips */}
      <div className="border-b border-black/[0.06] px-7 py-3.5">
        <div className="flex items-center gap-2">
          {CHIPS.map((c) => (
            <span
              key={c.label}
              className={
                c.active
                  ? "rounded-full bg-[#1d1d1f] px-3 py-1 text-[12px] font-medium text-white"
                  : "rounded-full bg-[#f5f5f7] px-3 py-1 text-[12px] font-medium text-[#1d1d1f]/70"
              }
            >
              {c.label}
            </span>
          ))}
        </div>
      </div>

      {/* Dark bulk-action bar */}
      <div className="flex items-center justify-between bg-[#1d1d1f] px-7 py-3 text-white">
        <div className="flex items-center gap-3">
          <span
            aria-hidden="true"
            className="flex h-[18px] w-[18px] items-center justify-center rounded-[3px] bg-white text-[11px] text-[#1d1d1f]"
          >
            ✓
          </span>
          <span className="text-[13.5px] font-semibold">3 of 4 selected</span>
        </div>
        <div className="flex gap-2">
          {["Edit price", "Adjust stock", "Move to draft"].map((label) => (
            <Button
              key={label}
              variant="ghost"
              size="sm"
              className="border border-white/20 bg-white/[0.12] text-white hover:bg-white/20 hover:text-white"
            >
              {label}
            </Button>
          ))}
          <Button
            size="sm"
            className="bg-white text-[#1d1d1f] hover:bg-white/90"
          >
            Apply →
          </Button>
        </div>
      </div>

      {/* Body: table + drawer */}
      <div className="flex flex-1 overflow-hidden">
        <div className="flex-1 overflow-auto">
          <table className="w-full text-sm">
            <thead>
              <tr className="border-b border-black/[0.06] text-left text-[11px] uppercase tracking-wider text-[#1d1d1f]/50">
                <th className="w-[32px] px-7 py-2.5"></th>
                <th className="px-2 py-2.5 font-medium">Product</th>
                <th className="px-2 py-2.5 font-medium">SKU</th>
                <th className="px-2 py-2.5 font-medium">Status</th>
                <th className="px-2 py-2.5 font-medium">Stock</th>
                <th className="px-2 py-2.5 font-medium">Price</th>
                <th className="px-7 py-2.5 font-medium">New price</th>
              </tr>
            </thead>
            <tbody>
              {ROWS.map((r) => (
                <tr
                  key={r.sku}
                  className="border-b border-black/[0.04]"
                  style={
                    r.selected
                      ? { background: "rgba(194,65,12,0.04)" }
                      : undefined
                  }
                >
                  <td className="px-7 py-3">
                    <span
                      aria-hidden="true"
                      className={
                        r.selected
                          ? "flex h-[14px] w-[14px] items-center justify-center rounded-[3px] bg-[#1d1d1f] text-[9px] text-white"
                          : "flex h-[14px] w-[14px] items-center justify-center rounded-[3px] border-[1.5px] border-[#1d1d1f]/40"
                      }
                    >
                      {r.selected ? "✓" : ""}
                    </span>
                  </td>
                  <td className="px-2 py-3 font-medium text-[#1d1d1f]">
                    {r.name}
                  </td>
                  <td className="px-2 py-3 font-mono text-[12px] text-[#1d1d1f]/60">
                    {r.sku}
                  </td>
                  <td className="px-2 py-3">
                    <span
                      className={
                        r.status === "Out"
                          ? "rounded-full bg-red-50 px-2 py-0.5 text-[11px] font-medium text-red-700"
                          : "rounded-full bg-amber-50 px-2 py-0.5 text-[11px] font-medium text-amber-700"
                      }
                    >
                      {r.status}
                    </span>
                  </td>
                  <td
                    className="px-2 py-3 tabular-nums"
                    style={{
                      color: r.stock === 0 ? "#dc2626" : "#b45309",
                      fontWeight: 500,
                    }}
                  >
                    {r.stock}
                  </td>
                  <td
                    className="px-2 py-3 tabular-nums text-[#1d1d1f]/60"
                    style={{
                      textDecoration: r.selected ? "line-through" : "none",
                    }}
                  >
                    {money(r.price)}
                  </td>
                  <td
                    className="px-7 py-3 tabular-nums"
                    style={{
                      color: r.selected ? "#16a34a" : "#1d1d1f40",
                      fontWeight: 600,
                    }}
                  >
                    {r.newPrice !== null ? money(r.newPrice) : "—"}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>

        {/* Right drawer */}
        <aside className="w-[340px] shrink-0 border-l border-black/[0.06] bg-white p-[22px]">
          <div className="mb-1.5 text-[11px] uppercase tracking-wider text-[#1d1d1f]/50">
            Bulk edit · 3 items
          </div>
          <h2 className="mb-[18px] text-[20px] font-semibold tracking-tight text-[#1d1d1f]">
            Adjust price
          </h2>

          <div className="mb-4 flex gap-1 rounded-lg bg-[#f5f5f7] p-[3px]">
            {["Set to", "Increase", "Decrease"].map((s, i) => (
              <Button
                key={s}
                variant="ghost"
                size="sm"
                className={
                  i === 1
                    ? "flex-1 bg-white text-[#1d1d1f] shadow-sm hover:bg-white"
                    : "flex-1 bg-transparent text-[#1d1d1f] hover:bg-transparent"
                }
              >
                {s}
              </Button>
            ))}
          </div>

          <div className="mb-4 flex gap-2">
            <div className="flex-1">
              <div className="mb-1 text-[11px] text-[#1d1d1f]/50">Amount</div>
              <div
                className="flex items-center rounded-lg px-3.5"
                style={{ height: 44, border: "1.5px solid #1d1d1f" }}
              >
                <span className="text-[16px] font-semibold tabular-nums text-[#1d1d1f]">
                  10
                </span>
              </div>
            </div>
            <div style={{ width: 90 }}>
              <div className="mb-1 text-[11px] text-[#1d1d1f]/50">Unit</div>
              <div className="flex h-[44px] items-center justify-between rounded-lg border border-black/[0.1] px-3">
                <span className="text-[16px] font-semibold text-[#1d1d1f]">
                  %
                </span>
                <span className="text-[10px] text-[#1d1d1f]/50">▾</span>
              </div>
            </div>
          </div>

          <div className="mb-[18px] rounded-lg bg-[#f5f5f7] p-3.5">
            <div className="mb-1.5 text-[11px] text-[#1d1d1f]/50">
              Preview · 3 items
            </div>
            <div className="flex flex-col gap-1.5 text-[13px]">
              {SELECTED.map((r) => (
                <div key={r.sku} className="flex justify-between">
                  <span className="truncate text-[#1d1d1f]/60">{r.name}</span>
                  <span className="tabular-nums">
                    <span className="text-[#1d1d1f]/50 line-through">
                      {money(r.price)}
                    </span>{" "}
                    <span className="font-semibold text-emerald-600">
                      {r.newPrice !== null ? money(r.newPrice) : ""}
                    </span>
                  </span>
                </div>
              ))}
            </div>
          </div>

          <Button className="h-[42px] w-full rounded-lg">Apply to 3 items</Button>
          <Button variant="ghost" className="mt-1.5 w-full">
            Cancel
          </Button>
        </aside>
      </div>
    </div>
  );
}
```

- [ ] **Step 4: Run the e2e to verify it passes**

```bash
cd web && npm run e2e -- seller-listings-bulk
```

Expected: all assertions pass.

- [ ] **Step 5: Lint & build**

```bash
cd web && npm run lint && npm run build
```

Expected: clean.

- [ ] **Step 6: Commit**

```bash
git add web/src/app/seller/listings/bulk/page.tsx web/e2e/seller-listings-bulk.spec.ts
git commit -m "feat(seller): add listings bulk-edit route (LFlow_02)"
```

---

## Task 4: `/seller/listings/[sku]/edit` route (LFlow_03)

**Files:**
- Create: `web/src/app/seller/listings/[sku]/edit/page.tsx`
- Create: `web/e2e/seller-listings-edit.spec.ts`

- [ ] **Step 1: Write the failing e2e spec**

Create `web/e2e/seller-listings-edit.spec.ts` with:

```typescript
// web/e2e/seller-listings-edit.spec.ts
import { expect, test } from "@playwright/test";

test.describe("Seller listings — edit", () => {
  test("renders editor for an existing SKU with variant matrix and status tabs", async ({
    page,
  }) => {
    await page.goto("/seller/listings/MC-VS-001/edit");

    // Header
    await expect(page.getByText("Listings · Vessels")).toBeVisible();
    await expect(
      page.getByRole("heading", { name: "Persimmon vase", exact: true }),
    ).toBeVisible();
    await expect(page.getByText("Unsaved changes")).toBeVisible();
    for (const action of ["Discard", "Save draft", "Publish →"]) {
      await expect(page.getByRole("button", { name: action })).toBeVisible();
    }

    // Variant matrix
    await expect(
      page.getByRole("heading", { name: "Variant matrix" }),
    ).toBeVisible();
    await expect(
      page.getByText("Size × Glaze · 6 combinations"),
    ).toBeVisible();
    for (const variant of [
      "Small · Persimmon",
      "Medium · Persimmon",
      "Large · Persimmon",
      "Small · Cream",
      "Medium · Cream",
      "Large · Cream",
    ]) {
      await expect(
        page.getByRole("cell", { name: variant, exact: true }),
      ).toBeVisible();
    }
    await expect(
      page.getByText(/2 variants updated · prices \+10%/),
    ).toBeVisible();

    // Photos card
    await expect(page.getByText("Photos · 4 of 8")).toBeVisible();

    // Status segmented + slug caption
    for (const s of ["Active", "Draft", "Archived"]) {
      await expect(page.getByRole("button", { name: s })).toBeVisible();
    }
    await expect(page.getByText("/persimmon-vase")).toBeVisible();
  });

  test("returns 404 for unknown SKU", async ({ page }) => {
    const response = await page.goto("/seller/listings/MC-NOPE-404/edit");
    expect(response?.status()).toBe(404);
  });
});
```

- [ ] **Step 2: Run the e2e to verify it fails**

```bash
cd web && npm run e2e -- seller-listings-edit
```

Expected: route does not exist; assertions fail.

- [ ] **Step 3: Implement the page**

Create `web/src/app/seller/listings/[sku]/edit/page.tsx` with:

```tsx
// web/src/app/seller/listings/[sku]/edit/page.tsx
import { notFound } from "next/navigation";
import { Button } from "@/components/ui/button";
import { getListingBySku } from "@/lib/seller/data";

type Variant = {
  label: string;
  sku: string;
  price: number;
  stock: number;
  status: "Active" | "Low" | "Out";
  changed?: boolean;
};

const VARIANTS: Variant[] = [
  {
    label: "Small · Persimmon",
    sku: "MS-VS-001-S",
    price: 70,
    stock: 6,
    status: "Active",
    changed: true,
  },
  {
    label: "Medium · Persimmon",
    sku: "MS-VS-001-M",
    price: 95,
    stock: 4,
    status: "Active",
    changed: true,
  },
  {
    label: "Large · Persimmon",
    sku: "MS-VS-001-L",
    price: 136,
    stock: 0,
    status: "Out",
  },
  {
    label: "Small · Cream",
    sku: "MS-VS-001-SC",
    price: 70,
    stock: 8,
    status: "Active",
  },
  {
    label: "Medium · Cream",
    sku: "MS-VS-001-MC",
    price: 95,
    stock: 5,
    status: "Active",
  },
  {
    label: "Large · Cream",
    sku: "MS-VS-001-LC",
    price: 136,
    stock: 2,
    status: "Low",
  },
];

const PHOTO_TONES = ["#e2d5c8", "#efe8d9", "#cfc7c2", "#d8c0a8"];

const STATUS_CHIP: Record<Variant["status"], string> = {
  Active:
    "rounded-full bg-emerald-50 px-2 py-0.5 text-[11px] font-medium text-emerald-700",
  Low: "rounded-full bg-amber-50 px-2 py-0.5 text-[11px] font-medium text-amber-700",
  Out: "rounded-full bg-red-50 px-2 py-0.5 text-[11px] font-medium text-red-700",
};

const money = (n: number) => `$${n}`;

const slugify = (s: string) =>
  s
    .toLowerCase()
    .replace(/[^a-z0-9]+/g, "-")
    .replace(/(^-|-$)/g, "");

export default async function ListingEditPage({
  params,
}: {
  params: Promise<{ sku: string }>;
}) {
  const { sku } = await params;
  const listing = getListingBySku(sku);
  if (!listing) notFound();

  const slug = slugify(listing.name);

  return (
    <div className="flex min-h-screen flex-col">
      {/* Header */}
      <div className="flex items-center justify-between border-b border-black/[0.06] bg-white px-7 py-4">
        <div className="flex items-center gap-3">
          <button
            type="button"
            aria-label="Back"
            className="flex h-8 w-8 items-center justify-center rounded-md hover:bg-[#f5f5f7]"
          >
            ‹
          </button>
          <div>
            <p className="text-[11px] uppercase tracking-wider text-[#1d1d1f]/60">
              Listings · {listing.category}
            </p>
            <h1 className="mt-0.5 text-[24px] font-semibold leading-none tracking-tight text-[#1d1d1f]">
              {listing.name}
            </h1>
          </div>
          <span className="ml-2 inline-flex items-center gap-1.5 rounded-full bg-amber-50 px-2.5 py-1 text-[11px] font-medium text-amber-700">
            <span className="h-1.5 w-1.5 rounded-full bg-amber-500" />
            Unsaved changes
          </span>
        </div>
        <div className="flex gap-2">
          <Button variant="ghost">Discard</Button>
          <Button variant="outline">Save draft</Button>
          <Button>Publish →</Button>
        </div>
      </div>

      {/* Body */}
      <div className="flex-1 overflow-auto px-7 py-6">
        <div
          className="grid gap-3"
          style={{ gridTemplateColumns: "1.5fr 1fr" }}
        >
          {/* Variant matrix */}
          <div className="rounded-xl border border-black/[0.06] bg-white p-5">
            <div className="mb-3.5 flex items-end justify-between">
              <div>
                <h2 className="text-[15px] font-semibold text-[#1d1d1f]">
                  Variant matrix
                </h2>
                <p className="text-[11px] text-[#1d1d1f]/60">
                  Size × Glaze · 6 combinations
                </p>
              </div>
              <Button variant="outline" size="sm">
                + Add option
              </Button>
            </div>

            <table className="w-full text-sm">
              <thead>
                <tr className="border-b border-black/[0.06] text-left text-[11px] uppercase tracking-wider text-[#1d1d1f]/50">
                  <th className="py-2.5 pr-2 font-medium">Variant</th>
                  <th className="px-2 py-2.5 font-medium">SKU</th>
                  <th className="px-2 py-2.5 font-medium">Price</th>
                  <th className="px-2 py-2.5 font-medium">Stock</th>
                  <th className="px-2 py-2.5 font-medium">Status</th>
                </tr>
              </thead>
              <tbody>
                {VARIANTS.map((v) => (
                  <tr
                    key={v.sku}
                    className="border-b border-black/[0.04]"
                    style={
                      v.changed
                        ? { background: "rgba(27,94,63,0.04)" }
                        : undefined
                    }
                  >
                    <td className="py-3 pr-2 font-medium text-[#1d1d1f]">
                      <span className="inline-flex items-center gap-2">
                        {v.changed && (
                          <span className="h-1.5 w-1.5 rounded-full bg-emerald-500" />
                        )}
                        {v.label}
                      </span>
                    </td>
                    <td className="px-2 py-3 font-mono text-[12px] text-[#1d1d1f]/60">
                      {v.sku}
                    </td>
                    <td
                      className="px-2 py-3 tabular-nums"
                      style={{
                        fontWeight: v.changed ? 600 : 400,
                        color: v.changed ? "#16a34a" : "#1d1d1f",
                      }}
                    >
                      {money(v.price)}
                    </td>
                    <td
                      className="px-2 py-3 tabular-nums"
                      style={{
                        color:
                          v.stock === 0
                            ? "#dc2626"
                            : v.stock < 5
                              ? "#b45309"
                              : "#1d1d1f",
                        fontWeight: 500,
                      }}
                    >
                      {v.stock}
                    </td>
                    <td className="px-2 py-3">
                      <span className={STATUS_CHIP[v.status]}>{v.status}</span>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
            <p className="mt-3.5 text-[11px] text-[#1d1d1f]/60">
              ● 2 variants updated · prices +10%
            </p>
          </div>

          {/* Right column */}
          <div className="flex flex-col gap-3">
            <div className="rounded-xl border border-black/[0.06] bg-white p-[18px]">
              <h3 className="mb-3 text-[13.5px] font-semibold text-[#1d1d1f]">
                Photos · 4 of 8
              </h3>
              <div className="grid grid-cols-3 gap-2">
                {PHOTO_TONES.map((tone, i) => (
                  <div
                    key={i}
                    className="rounded-md"
                    style={{ height: 88, background: tone }}
                  />
                ))}
                <div
                  className="flex items-center justify-center rounded-md border-[1.5px] border-dashed border-black/20 text-[#1d1d1f]/40"
                  style={{ height: 88 }}
                >
                  +
                </div>
              </div>
            </div>

            <div className="rounded-xl border border-black/[0.06] bg-white p-[18px]">
              <h3 className="mb-2.5 text-[13.5px] font-semibold text-[#1d1d1f]">
                Status
              </h3>
              <div className="flex gap-1 rounded-lg bg-[#f5f5f7] p-[3px]">
                {["Active", "Draft", "Archived"].map((s, i) => (
                  <Button
                    key={s}
                    variant="ghost"
                    size="sm"
                    className={
                      i === 0
                        ? "flex-1 bg-white text-[#1d1d1f] shadow-sm hover:bg-white"
                        : "flex-1 bg-transparent text-[#1d1d1f] hover:bg-transparent"
                    }
                  >
                    {s}
                  </Button>
                ))}
              </div>
              <p className="mt-2.5 text-[11px] text-[#1d1d1f]/60">
                Visible at{" "}
                <span className="font-mono text-[#1d1d1f]">/{slug}</span>
              </p>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}
```

- [ ] **Step 4: Run the e2e to verify it passes**

```bash
cd web && npm run e2e -- seller-listings-edit
```

Expected: both tests pass (rendering + 404).

- [ ] **Step 5: Lint & build**

```bash
cd web && npm run lint && npm run build
```

Expected: clean.

- [ ] **Step 6: Commit**

```bash
git add web/src/app/seller/listings/\[sku\]/edit/page.tsx web/e2e/seller-listings-edit.spec.ts
git commit -m "feat(seller): add listings edit route (LFlow_03)"
```

---

## Task 5: `/seller/listings/[sku]/preview` route (LFlow_04)

**Files:**
- Create: `web/src/app/seller/listings/[sku]/preview/page.tsx`
- Create: `web/e2e/seller-listings-preview.spec.ts`

- [ ] **Step 1: Write the failing e2e spec**

Create `web/e2e/seller-listings-preview.spec.ts` with:

```typescript
// web/e2e/seller-listings-preview.spec.ts
import { expect, test } from "@playwright/test";

test.describe("Seller listings — preview", () => {
  test("renders desktop browser preview and listing-health rail", async ({
    page,
  }) => {
    await page.goto("/seller/listings/MC-VS-001/preview");

    // Header
    await expect(page.getByText("Preview · Persimmon vase")).toBeVisible();
    await expect(
      page.getByRole("heading", { name: "How shoppers will see it" }),
    ).toBeVisible();
    for (const b of ["Desktop", "Mobile", "Back to edit", "Publish now →"]) {
      await expect(page.getByRole("button", { name: b })).toBeVisible();
    }

    // Browser-chrome url & alex-studio brand
    await expect(
      page.getByText("alex-studio.micro.shop/persimmon-vase"),
    ).toBeVisible();
    await expect(page.getByText("Alex Studio · Vessels")).toBeVisible();

    // Product details
    await expect(
      page.getByRole("heading", { name: "Persimmon vase", exact: true }),
    ).toBeVisible();
    await expect(
      page.getByText("Medium · Persimmon · 4 in stock"),
    ).toBeVisible();
    for (const chip of ["Small", "Medium", "Large · out"]) {
      await expect(page.getByText(chip, { exact: true })).toBeVisible();
    }
    await expect(
      page.getByRole("button", { name: "Add to bag · $95" }),
    ).toBeVisible();

    // Listing-health rail
    await expect(
      page.getByRole("heading", { name: "Listing health" }),
    ).toBeVisible();
    await expect(page.getByText("96", { exact: true })).toBeVisible();
    for (const label of [
      "Title under 60 chars",
      "Description over 100 chars",
      "4 photos",
      "Variants in stock",
      "Tagged & categorized",
    ]) {
      await expect(page.getByText(label, { exact: true })).toBeVisible();
    }
  });

  test("returns 404 for unknown SKU", async ({ page }) => {
    const response = await page.goto("/seller/listings/MC-NOPE-404/preview");
    expect(response?.status()).toBe(404);
  });
});
```

- [ ] **Step 2: Run the e2e to verify it fails**

```bash
cd web && npm run e2e -- seller-listings-preview
```

Expected: route does not exist; assertions fail.

- [ ] **Step 3: Implement the page**

Create `web/src/app/seller/listings/[sku]/preview/page.tsx` with:

```tsx
// web/src/app/seller/listings/[sku]/preview/page.tsx
import { notFound } from "next/navigation";
import { Button } from "@/components/ui/button";
import { getListingBySku } from "@/lib/seller/data";

type Check = { label: string; sub: string; tone: "good" | "warn" };

const CHECKS: Check[] = [
  { label: "Title under 60 chars", sub: "14 / 60", tone: "good" },
  { label: "Description over 100 chars", sub: "208 / 800", tone: "good" },
  { label: "4 photos", sub: "recommend 6+", tone: "warn" },
  { label: "Variants in stock", sub: "5 of 6 active", tone: "good" },
  { label: "Tagged & categorized", sub: "4 tags · Vessels", tone: "good" },
];

const slugify = (s: string) =>
  s
    .toLowerCase()
    .replace(/[^a-z0-9]+/g, "-")
    .replace(/(^-|-$)/g, "");

export default async function ListingPreviewPage({
  params,
}: {
  params: Promise<{ sku: string }>;
}) {
  const { sku } = await params;
  const listing = getListingBySku(sku);
  if (!listing) notFound();

  const slug = slugify(listing.name);

  return (
    <div className="flex min-h-screen flex-col">
      {/* Header */}
      <div className="flex items-center justify-between border-b border-black/[0.06] bg-white px-7 py-4">
        <div className="flex items-center gap-3">
          <button
            type="button"
            aria-label="Back"
            className="flex h-8 w-8 items-center justify-center rounded-md hover:bg-[#f5f5f7]"
          >
            ‹
          </button>
          <div>
            <p className="text-[11px] uppercase tracking-wider text-[#1d1d1f]/60">
              Preview · {listing.name}
            </p>
            <h1 className="mt-0.5 text-[22px] font-semibold leading-none tracking-tight text-[#1d1d1f]">
              How shoppers will see it
            </h1>
          </div>
        </div>
        <div className="flex items-center gap-2">
          <div className="flex gap-1 rounded-lg bg-[#f5f5f7] p-[3px]">
            <Button
              variant="ghost"
              size="sm"
              className="bg-white text-[#1d1d1f] shadow-sm hover:bg-white"
            >
              Desktop
            </Button>
            <Button
              variant="ghost"
              size="sm"
              className="bg-transparent text-[#1d1d1f] hover:bg-transparent"
            >
              Mobile
            </Button>
          </div>
          <Button variant="outline">Back to edit</Button>
          <Button>Publish now →</Button>
        </div>
      </div>

      {/* Body */}
      <div className="flex flex-1 overflow-hidden bg-[#f5f5f7]">
        {/* Browser preview */}
        <div className="flex flex-1 items-center justify-center p-6">
          <div
            className="overflow-hidden rounded-xl border border-black/[0.06] bg-white shadow-2xl"
            style={{ width: "100%", maxWidth: 720, height: 560 }}
          >
            <div
              className="flex items-center gap-2 border-b border-black/[0.06] px-3"
              style={{ height: 30, background: "#E8E3D8" }}
            >
              {["#FF6058", "#FFBD2E", "#28C941"].map((c) => (
                <span
                  key={c}
                  aria-hidden="true"
                  className="h-[9px] w-[9px] rounded-full"
                  style={{ background: c }}
                />
              ))}
              <div
                className="ml-2 flex flex-1 items-center gap-1 rounded bg-white px-2"
                style={{ height: 18 }}
              >
                <span aria-hidden="true" className="text-[10px]">
                  🔒
                </span>
                <span className="text-[11px] text-[#1d1d1f]/60">
                  alex-studio.micro.shop/{slug}
                </span>
              </div>
            </div>
            <div
              className="grid"
              style={{
                gridTemplateColumns: "1.4fr 1fr",
                height: "calc(100% - 30px)",
              }}
            >
              <div style={{ background: "#e2d5c8" }} />
              <div className="flex flex-col p-7">
                <p className="text-[11px] uppercase tracking-wider text-[#1d1d1f]/60">
                  Alex Studio · {listing.category}
                </p>
                <h2 className="mt-1.5 text-[32px] font-semibold leading-none tracking-tight text-[#1d1d1f]">
                  {listing.name}
                </h2>
                <p className="mt-2.5 text-[22px] font-semibold tabular-nums text-[#1d1d1f]">
                  $95
                </p>
                <p className="mt-1 text-[11px] text-[#1d1d1f]/60">
                  Medium · Persimmon · 4 in stock
                </p>

                <p className="mt-3.5 mb-1.5 text-[11px] font-medium text-[#1d1d1f]">
                  SIZE
                </p>
                <div className="flex gap-1">
                  <span className="rounded-full bg-[#f5f5f7] px-3 py-1 text-[12px] font-medium text-[#1d1d1f]">
                    Small
                  </span>
                  <span className="rounded-full bg-[#1d1d1f] px-3 py-1 text-[12px] font-medium text-white">
                    Medium
                  </span>
                  <span className="rounded-full bg-[#f5f5f7] px-3 py-1 text-[12px] font-medium text-[#1d1d1f]/50">
                    Large · out
                  </span>
                </div>

                <p className="mt-3.5 mb-1.5 text-[11px] font-medium text-[#1d1d1f]">
                  GLAZE
                </p>
                <div className="flex gap-2">
                  <span
                    className="h-6 w-6 rounded-full"
                    style={{ background: "#c2410c", border: "2px solid #1d1d1f" }}
                  />
                  <span
                    className="h-6 w-6 rounded-full border border-black/[0.1]"
                    style={{ background: "#F0E8D7" }}
                  />
                </div>

                <div className="flex-1" />
                <Button className="mt-3 h-11 w-full rounded-lg">
                  Add to bag · $95
                </Button>
              </div>
            </div>
          </div>
        </div>

        {/* Health rail */}
        <aside className="w-[320px] shrink-0 overflow-auto border-l border-black/[0.06] bg-white p-[22px]">
          <div className="mb-2 flex items-center justify-between">
            <h2 className="text-[13.5px] font-semibold text-[#1d1d1f]">
              Listing health
            </h2>
            <span className="text-[13.5px] font-semibold tabular-nums text-emerald-600">
              96
            </span>
          </div>
          <div className="mb-4 h-1.5 overflow-hidden rounded-full bg-black/[0.06]">
            <div className="h-full w-[96%] rounded-full bg-emerald-500" />
          </div>
          <div className="flex flex-col gap-3">
            {CHECKS.map((c) => (
              <div key={c.label} className="flex items-start gap-2">
                <span
                  aria-hidden="true"
                  className="mt-0.5 flex h-[18px] w-[18px] shrink-0 items-center justify-center rounded-full text-white"
                  style={{
                    background: c.tone === "good" ? "#16a34a" : "#d97706",
                  }}
                >
                  <span className="text-[10px] font-bold">
                    {c.tone === "good" ? "✓" : "i"}
                  </span>
                </span>
                <div className="flex-1">
                  <div className="text-[12.5px] font-medium text-[#1d1d1f]">
                    {c.label}
                  </div>
                  <div className="text-[11px] text-[#1d1d1f]/60">{c.sub}</div>
                </div>
              </div>
            ))}
          </div>
        </aside>
      </div>
    </div>
  );
}
```

- [ ] **Step 4: Run the e2e to verify it passes**

```bash
cd web && npm run e2e -- seller-listings-preview
```

Expected: both tests pass.

- [ ] **Step 5: Lint & build**

```bash
cd web && npm run lint && npm run build
```

Expected: clean.

- [ ] **Step 6: Commit**

```bash
git add web/src/app/seller/listings/\[sku\]/preview/page.tsx web/e2e/seller-listings-preview.spec.ts
git commit -m "feat(seller): add listings preview route (LFlow_04)"
```

---

## Task 6: Final regression check

- [ ] **Step 1: Full e2e suite**

```bash
cd web && npm run e2e
```

Expected: all specs pass — the four new ones plus existing `seller-listings*`, `seller-listings-new`, etc. No regressions.

- [ ] **Step 2: Full unit suite**

```bash
cd web && npm test
```

Expected: all tests pass (existing + the 2 new `getListingBySku` tests).

- [ ] **Step 3: Lint & build**

```bash
cd web && npm run lint && npm run build
```

Expected: Biome clean, build succeeds, no type errors.

- [ ] **Step 4: Smoke-check the dev server (manual)**

```bash
cd web && npm run dev
```

Visit each route and confirm rendering matches the hi-fi reference (`design/project/hifi-flow-listings.jsx`):

- http://localhost:3000/seller/listings/published
- http://localhost:3000/seller/listings/bulk
- http://localhost:3000/seller/listings/MC-VS-001/edit
- http://localhost:3000/seller/listings/MC-VS-001/preview
- http://localhost:3000/seller/listings/MC-NOPE-404/edit (expect 404)

Stop the dev server when done.

- [ ] **Step 5: No commit needed**

If all checks pass, the work is done. If any regression appeared, file the fix as its own commit.

---

## Self-Review Notes

- All four spec sections have a corresponding implementation task plus an e2e test.
- The `getListingBySku` helper is introduced in Task 1 before any consumer; signature `(sku: string) => Listing | null` matches its use in Tasks 4 and 5.
- Variant SKUs in Task 4 use the `MS-` design prefix (display-only, intentional per spec "Brand-name leak" / variants-not-in-data discussion).
- Preview page uses `Alex Studio` and `alex-studio.micro.shop`, not `Mira Studio` (per CLAUDE.md brand rule and spec).
- Bulk page uses real Low/Out listings from `data.ts` (`MC-MG-041`, `MC-SC-008`, `MC-BW-051`, `MC-TB-038`), and the published page's bulk activity rows reference two of those by name (`Ember tea bowl`, `Peat serving bowl`) — naming consistent across pages.
- 404 cases asserted via `response.status() === 404` (not just visible text), per spec risk note.
