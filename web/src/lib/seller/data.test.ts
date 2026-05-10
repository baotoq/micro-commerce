// web/src/lib/seller/data.test.ts
import { describe, expect, it } from "vitest";
import {
  BRAND,
  DATE_LABEL,
  getAnalyticsKpis,
  getDayOneStats,
  getFirstMonthKpis,
  getFirstMonthSeries,
  getFirstMonthTopSellers,
  getFirstOrderStats,
  getFunnel,
  getLaunchChecklist,
  getLedgerEntries,
  getListingBySku,
  getListingCounts,
  getListings,
  getMarketingDraft,
  getOrderDetail,
  getOrderDetailFull,
  getOrderInbox,
  getOrderInboxSummary,
  getOrderInboxTabs,
  getOverviewKpis,
  getPayoutSummary,
  getPromoStats,
  getPromos,
  getPromoTabs,
  getRangeOptions,
  getRecentOrders,
  getRevenueSeries,
  getSetupSteps,
  getShippingOptions,
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

describe("seller management data — orders inbox", () => {
  it("returns 10 inbox rows with #1042 starred at the top", () => {
    const rows = getOrderInbox();
    expect(rows).toHaveLength(10);
    expect(rows[0].id).toBe("#1042");
    expect(rows[0].starred).toBe(true);
    expect(rows[0].customer).toBe("Sasha L.");
    expect(rows[0].items).toBe("Persimmon vase, Ash budstem");
    expect(rows[0].total).toBe(152);
    expect(rows[0].tone).toBe("warn");
  });

  it("inbox tabs total 47 and Needs action is the active tab", () => {
    const tabs = getOrderInboxTabs();
    expect(tabs).toHaveLength(6);
    const all = tabs.find((t) => t.label === "All");
    expect(all?.count).toBe(47);
    const needs = tabs.find((t) => t.label === "Needs action");
    expect(needs?.count).toBe(4);
    expect(needs?.on).toBe(true);
  });

  it("inbox summary reports lifetime + needs-action counts", () => {
    const s = getOrderInboxSummary();
    expect(s.totalLifetime).toBe(47);
    expect(s.needAction).toBe(4);
  });
});

describe("seller management data — order detail #1042", () => {
  it("returns null for unknown order ids", () => {
    expect(getOrderDetailFull("9999")).toBeNull();
  });

  it("returns the Sasha Leblanc partial-fulfillment detail for 1042", () => {
    const d = getOrderDetailFull("1042");
    expect(d).not.toBeNull();
    if (!d) return;
    expect(d.id).toBe("#1042");
    expect(d.status).toBe("Partially fulfilled");
    expect(d.customer.name).toBe("Sasha Leblanc");
    expect(d.fulfillments).toHaveLength(2);
    expect(d.fulfillments[0].status).toBe("shipped");
    expect(d.fulfillments[0].productName).toBe("Persimmon vase");
    expect(d.fulfillments[1].status).toBe("awaiting");
    expect(d.fulfillments[1].productName).toBe("Ash budstem");
    expect(d.refund.total).toBe(30);
    expect(d.refund.lastFour).toBe("4421");
    expect(d.summary.paid).toBe(152);
    expect(d.summary.fee).toBe(6.08);
    expect(d.summary.labelCost).toBe(9.84);
    expect(d.summary.net).toBe(136.08);
    expect(d.timeline).toHaveLength(5);
    expect(d.customerTags).toEqual(["VIP", "Repeat buyer", "Gift"]);
  });
});

describe("seller management data — promos", () => {
  it("returns 5 promo codes with STUDIO15 highlighted", () => {
    const codes = getPromos();
    expect(codes).toHaveLength(5);
    expect(codes.map((c) => c.code)).toEqual([
      "SPRING20",
      "WELCOME10",
      "STUDIO15",
      "BLOOM",
      "FRIENDS",
    ]);
    const studio = codes.find((c) => c.code === "STUDIO15");
    expect(studio?.highlight).toBe(true);
    expect(studio?.status).toBe("Active");
  });

  it("returns 4 promo stat cards", () => {
    const stats = getPromoStats();
    expect(stats).toHaveLength(4);
    expect(stats[0].label).toBe("Driven revenue");
    expect(stats[3].sub).toBe("from WELCOME10");
  });

  it("returns 3 promo tabs with Promotions active", () => {
    const tabs = getPromoTabs();
    expect(tabs).toHaveLength(3);
    expect(tabs[0].label).toBe("Promotions");
    expect(tabs[0].on).toBe(true);
    expect(tabs[0].count).toBe(5);
  });
});

describe("seller management data — marketing draft", () => {
  it("returns the persimmon-vase restock draft branded to BRAND.name", () => {
    const d = getMarketingDraft();
    expect(d.subject).toBe("The persimmon vase is back · just 8 this batch");
    expect(d.previewText).toBe(
      "A small restock — three glaze variations this round.",
    );
    expect(d.audiences).toHaveLength(4);
    expect(d.audiences[0].count).toBe(47);
    expect(d.audiences.filter((a) => a.on)).toHaveLength(3);
    expect(d.templates.find((t) => t.label === "Restock")?.on).toBe(true);
    expect(d.recipientCount).toBe(184);
    expect(d.openRateForecast).toContain("32%");
    expect(d.productName).toBe("Persimmon vase");
    expect(d.productPrice).toBe(86);
  });

  it("does NOT mention 'Mira' anywhere in marketing draft copy", () => {
    const d = getMarketingDraft();
    const blob = JSON.stringify(d);
    expect(blob).not.toMatch(/Mira/);
    expect(blob).toContain(BRAND.name);
  });
});

describe("seller data — setup and launch", () => {
  it("DATE_LABEL matches the fixed today date", () => {
    expect(DATE_LABEL).toBe("Tuesday · April 8");
  });

  it("setup steps include done, active, and pending states", () => {
    const steps = getSetupSteps();
    expect(steps.length).toBeGreaterThan(0);
    const statuses = steps.map((s) => s.status);
    expect(statuses).toContain("done");
    expect(statuses).toContain("active");
    expect(statuses).toContain("pending");
  });

  it("launch checklist has at least one done and one undone task", () => {
    const tasks = getLaunchChecklist();
    expect(tasks.some((t) => t.done)).toBe(true);
    expect(tasks.some((t) => !t.done)).toBe(true);
  });

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

describe("seller data — shipping options", () => {
  it("returns 3 shipping options with exactly one selected", () => {
    const opts = getShippingOptions();
    expect(opts).toHaveLength(3);
    const selected = opts.filter((o) => o.selected);
    expect(selected).toHaveLength(1);
  });

  it("all shipping options have positive prices", () => {
    for (const o of getShippingOptions()) {
      expect(o.price).toBeGreaterThan(0);
    }
  });
});

describe("seller data — order detail (legacy #1001)", () => {
  it("returns null for an unknown order id", () => {
    expect(getOrderDetail("9999")).toBeNull();
  });

  it("returns Sasha Leblanc detail for order 1001", () => {
    const d = getOrderDetail("1001");
    expect(d).not.toBeNull();
    if (!d) return;
    expect(d.id).toBe("#1001");
    expect(d.customer).toBe("Sasha Leblanc");
    expect(d.subtotal).toBe(86);
    expect(d.net).toBeGreaterThan(0);
    expect(d.feePct).toBe(4);
  });
});

describe("seller data — payout and ledger", () => {
  it("payout summary has positive lifetime revenue", () => {
    const p = getPayoutSummary();
    expect(p.lifetime).toBeGreaterThan(0);
    expect(p.lifetimeOrders).toBeGreaterThan(0);
  });

  it("ledger entries include at least one sale and one payout entry", () => {
    const entries = getLedgerEntries();
    expect(entries.some((e) => e.type === "sale")).toBe(true);
    expect(entries.some((e) => e.type === "payout")).toBe(true);
  });

  it("payout entries have positive amounts, fee entries are negative", () => {
    const entries = getLedgerEntries();
    for (const e of entries.filter((e) => e.type === "payout")) {
      expect(e.amount).toBeGreaterThan(0);
    }
    for (const e of entries.filter((e) => e.type === "fee")) {
      expect(e.amount).toBeLessThan(0);
    }
  });
});

describe("seller data — first-month milestones", () => {
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
