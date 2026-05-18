// web/src/lib/seller/onboarding/data.test.ts
import { describe, expect, it } from "vitest";
import {
  getDayOneStats,
  getFirstMonthKpis,
  getFirstMonthSeries,
  getFirstMonthTopSellers,
  getFirstOrderStats,
} from "@/lib/seller/onboarding/data";

describe("seller onboarding — day one and first order", () => {
  it("day-one stats return 3 entries with label, value, and subject", () => {
    const stats = getDayOneStats();
    expect(stats).toHaveLength(3);
    for (const s of stats) {
      expect(s.label).toBeTruthy();
      expect(s.value).toBeDefined();
      expect(s.subject).toBeTruthy();
    }
  });

  it("first-order stats return 4 entries each with a non-empty spark array", () => {
    const stats = getFirstOrderStats();
    expect(stats).toHaveLength(4);
    for (const s of stats) {
      expect(s.spark.length).toBeGreaterThan(0);
    }
  });
});

describe("seller onboarding — first-month milestones", () => {
  it("first-month KPIs return 4 entries with non-empty spark arrays", () => {
    const kpis = getFirstMonthKpis();
    expect(kpis).toHaveLength(4);
    for (const k of kpis) {
      expect(k.spark.length).toBeGreaterThan(0);
    }
  });

  it("first-month series has 30 data points with non-negative values", () => {
    const series = getFirstMonthSeries();
    expect(series).toHaveLength(30);
    for (const v of series) expect(v).toBeGreaterThanOrEqual(0);
  });

  it("first-month top sellers have 3 entries with positive revenue", () => {
    const sellers = getFirstMonthTopSellers();
    expect(sellers).toHaveLength(3);
    for (const s of sellers) {
      expect(s.revenue).toBeGreaterThan(0);
      expect(s.soldLabel).toMatch(/sold/);
    }
  });
});
