// web/src/lib/seller/data.test.ts
import { describe, expect, it } from "vitest";
import {
  BRAND,
  getAnalyticsKpis,
  getFunnel,
  getListingBySku,
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
    for (const p of getRevenueSeries())
      expect(p.amount).toBeGreaterThanOrEqual(0);
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

  it("getListingBySku returns the matching listing", () => {
    const listing = getListingBySku("MC-VS-001");
    expect(listing).not.toBeNull();
    expect(listing?.sku).toBe("MC-VS-001");
    expect(listing?.name).toBe("Persimmon vase");
  });

  it("getListingBySku returns null for an unknown sku", () => {
    expect(getListingBySku("MC-NOPE-404")).toBeNull();
  });
});
