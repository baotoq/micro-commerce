// web/src/lib/seller/analytics/data.test.ts
import { afterEach, describe, expect, it, vi } from "vitest";

vi.mock("server-only", () => ({}));
vi.mock("next/cache", () => ({ cacheTag: vi.fn() }));
vi.mock("@/lib/catalog/analytics", () => ({
  fetchAnalyticsOverview: vi.fn(),
  fetchDashboardSummary: vi.fn(),
}));

import type { AnalyticsOverviewDto } from "@/lib/catalog/analytics";
import * as analyticsClient from "@/lib/catalog/analytics";
import {
  formatKpiChange,
  getAnalyticsKpis,
  getFunnel,
  getOverviewKpis,
  getRangeOptions,
  getRevenueSeries,
  getSources,
  getTopProducts,
} from "@/lib/seller/analytics/data";

function makeOverview(
  overrides: Partial<AnalyticsOverviewDto> = {},
): AnalyticsOverviewDto {
  return {
    kpiCards: [
      { label: "Revenue · 30 days", value: 4280, change: 12.4, isUp: true },
      { label: "Orders · 30 days", value: 38, change: 8, isUp: true },
      { label: "Storefront views", value: 2304, change: 3.1, isUp: false },
      { label: "Avg. order", value: 94.55, change: 2, isUp: true },
    ],
    revenueSeries: [
      { date: "Apr 1", revenue: 520, orders: 6 },
      { date: "Apr 2", revenue: 680, orders: 8 },
      { date: "Apr 3", revenue: 590, orders: 7 },
      { date: "Apr 4", revenue: 740, orders: 9 },
      { date: "Apr 5", revenue: 820, orders: 10 },
      { date: "Apr 6", revenue: 490, orders: 5 },
      { date: "Apr 7", revenue: 440, orders: 4 },
    ],
    sources: [
      { source: "Direct", pct: 42, revenue: 2184 },
      { source: "Instagram", pct: 28, revenue: 1456 },
      { source: "Email", pct: 18, revenue: 936 },
      { source: "Search", pct: 8, revenue: 416 },
      { source: "Other", pct: 4, revenue: 208 },
    ],
    topProducts: [
      { sku: "MC-VS-001", name: "Persimmon vase", units: 48, revenue: 4128 },
      { sku: "MC-CR-003", name: "Indigo carafe", units: 32, revenue: 3520 },
      { sku: "MC-VS-002", name: "Shadow vase, tall", units: 22, revenue: 2728 },
      { sku: "MC-BW-014", name: "Forest bowl", units: 35, revenue: 2380 },
      { sku: "MC-TB-007", name: "Cream tumbler", units: 41, revenue: 2214 },
    ],
    funnel: [
      { label: "Store visits", count: 1240, rate: 1 },
      { label: "Product views", count: 892, rate: 0.719 },
      { label: "Add to cart", count: 318, rate: 0.257 },
      { label: "Checkout started", count: 127, rate: 0.103 },
      { label: "Purchased", count: 46, rate: 0.037 },
    ],
    ...overrides,
  };
}

describe("formatKpiChange", () => {
  it("formats positive change with leading +", () => {
    expect(formatKpiChange(12.4, true)).toBe("+12.4%");
  });

  it("formats negative change with leading -", () => {
    expect(formatKpiChange(3.1, false)).toBe("-3.1%");
  });
});

describe("getRangeOptions", () => {
  it("returns 7d/30d/90d/Year", () => {
    expect(getRangeOptions()).toEqual(["7d", "30d", "90d", "Year"]);
  });
});

describe("getTopProducts", () => {
  afterEach(() => vi.clearAllMocks());

  it("maps TopProductDto[] preserving order, first is Persimmon vase", async () => {
    vi.mocked(analyticsClient.fetchAnalyticsOverview).mockResolvedValueOnce(
      makeOverview(),
    );

    const top = await getTopProducts();

    expect(top.length).toBeGreaterThanOrEqual(5);
    expect(top[0].name).toBe("Persimmon vase");
    expect(top[0].sku).toBe("MC-VS-001");
    expect(top[0].units).toBe(48);
    expect(top[0].revenue).toBe(4128);
    for (const p of top) expect(p.revenue).toBeGreaterThan(0);
  });
});

describe("getSources", () => {
  afterEach(() => vi.clearAllMocks());

  it("maps SourceBreakdownDto[] with Direct first and share normalized from pct", async () => {
    vi.mocked(analyticsClient.fetchAnalyticsOverview).mockResolvedValueOnce(
      makeOverview(),
    );

    const sources = await getSources();

    expect(sources).toHaveLength(5);
    expect(sources[0].name).toBe("Direct");
    expect(sources[0].visits).toBe(2184);
    expect(sources[0].share).toBeCloseTo(0.42, 5);
    const total = sources.reduce((s, x) => s + x.share, 0);
    expect(total).toBeGreaterThan(0.999);
    expect(total).toBeLessThan(1.001);
  });
});

describe("getFunnel", () => {
  afterEach(() => vi.clearAllMocks());

  it("maps FunnelStageDto[] to 5 monotonically non-increasing stages", async () => {
    vi.mocked(analyticsClient.fetchAnalyticsOverview).mockResolvedValueOnce(
      makeOverview(),
    );

    const stages = await getFunnel();

    expect(stages).toHaveLength(5);
    expect(stages[0].label).toBe("Store visits");
    for (let i = 1; i < stages.length; i++) {
      expect(stages[i].count).toBeLessThanOrEqual(stages[i - 1].count);
    }
  });
});

describe("getRevenueSeries", () => {
  afterEach(() => vi.clearAllMocks());

  it("maps RevenuePointDto[] to {day,amount} pairs", async () => {
    vi.mocked(analyticsClient.fetchAnalyticsOverview).mockResolvedValueOnce(
      makeOverview(),
    );

    const series = await getRevenueSeries();

    expect(series).toHaveLength(7);
    expect(series[0].day).toBe("Apr 1");
    expect(series[0].amount).toBe(520);
    for (const p of series) expect(p.amount).toBeGreaterThanOrEqual(0);
  });
});

describe("getAnalyticsKpis", () => {
  afterEach(() => vi.clearAllMocks());

  it("maps KpiPointDto[] with signed delta and inferred format", async () => {
    vi.mocked(analyticsClient.fetchAnalyticsOverview).mockResolvedValueOnce(
      makeOverview(),
    );

    const kpis = await getAnalyticsKpis();

    expect(kpis).toHaveLength(4);
    expect(kpis[0].label).toBe("Revenue · 30 days");
    expect(kpis[0].value).toBe(4280);
    // signed delta: isUp true -> positive
    expect(kpis[0].delta).toBe(12.4);
    // isUp false -> negative delta
    expect(kpis[2].delta).toBe(-3.1);
    for (const k of kpis) {
      expect(["currency", "number", "percent"]).toContain(k.format);
    }
  });
});

describe("getOverviewKpis", () => {
  afterEach(() => vi.clearAllMocks());

  it("returns the first 3 KPI cards for the dashboard overview", async () => {
    vi.mocked(analyticsClient.fetchAnalyticsOverview).mockResolvedValueOnce(
      makeOverview(),
    );

    const kpis = await getOverviewKpis();

    expect(kpis).toHaveLength(3);
    expect(kpis[0].label).toBe("Revenue · 30 days");
    expect(kpis[2].delta).toBe(-3.1);
    for (const k of kpis) {
      expect(["currency", "number", "percent"]).toContain(k.format);
      expect(k.value).toBeGreaterThanOrEqual(0);
    }
  });
});
