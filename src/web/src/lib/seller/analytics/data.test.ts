// web/src/lib/seller/analytics/data.test.ts
import { describe, expect, it } from "vitest";
import {
  getAnalyticsKpis,
  getFunnel,
  getOverviewKpis,
  getRangeOptions,
  getRevenueSeries,
  getSources,
  getTopProducts,
} from "@/lib/seller/analytics/data";

describe("seller analytics data", () => {
  it("returns 7 revenue points", () => {
    expect(getRevenueSeries()).toHaveLength(7);
    for (const p of getRevenueSeries())
      expect(p.amount).toBeGreaterThanOrEqual(0);
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
});
