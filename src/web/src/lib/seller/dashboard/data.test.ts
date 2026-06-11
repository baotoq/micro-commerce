// web/src/lib/seller/dashboard/data.test.ts
import { afterEach, describe, expect, it, vi } from "vitest";

vi.mock("server-only", () => ({}));
vi.mock("next/cache", () => ({ cacheTag: vi.fn() }));
vi.mock("@/lib/catalog/analytics", () => ({
  fetchAnalyticsOverview: vi.fn(),
  fetchDashboardSummary: vi.fn(),
}));

import type { DashboardSummaryDto } from "@/lib/catalog/analytics";
import * as analyticsClient from "@/lib/catalog/analytics";
import {
  getDashboardRecentOrders,
  getDateLabel,
  getTodayItems,
} from "@/lib/seller/dashboard/data";

function makeSummary(
  overrides: Partial<DashboardSummaryDto> = {},
): DashboardSummaryDto {
  return {
    dateLabel: "Tuesday · April 8",
    newOrders: 3,
    ordersToShip: 3,
    revenue: 152,
    recentReviews: 3,
    todayItems: [
      { label: "Pack 3 orders", count: 3 },
      { label: "3 new reviews to moderate", count: 3 },
      { label: "Low stock: 2 items", count: 2 },
      { label: "Unread messages: 0", count: 0 },
    ],
    recentOrders: [
      {
        number: 1042,
        placedAt: "2026-04-08T21:14:00Z",
        customerName: "Sasha Leblanc",
        cityState: "San Francisco, CA",
        itemsSummary: "Persimmon vase, Ash budstem",
        qty: 2,
        total: 152,
        shippingMethod: "USPS Priority",
        status: "new",
        starred: true,
      },
      {
        number: 1041,
        placedAt: "2026-04-08T18:08:00Z",
        customerName: "Dev Patel",
        cityState: "Brooklyn, NY",
        itemsSummary: "Forest bowl, lg.",
        qty: 1,
        total: 64,
        shippingMethod: "USPS Ground",
        status: "new",
        starred: false,
      },
    ],
    ...overrides,
  };
}

describe("getTodayItems", () => {
  afterEach(() => vi.clearAllMocks());

  it("maps the first today item to 'Pack 3 orders ready to ship'", async () => {
    vi.mocked(analyticsClient.fetchDashboardSummary).mockResolvedValueOnce(
      makeSummary(),
    );

    const items = await getTodayItems();

    expect(items.length).toBeGreaterThan(0);
    expect(items[0].label).toBe("Pack 3 orders ready to ship");
    expect(items[0].count).toBe(3);
  });
});

describe("getDateLabel", () => {
  afterEach(() => vi.clearAllMocks());

  it("returns the backend dateLabel 'Tuesday · April 8'", async () => {
    vi.mocked(analyticsClient.fetchDashboardSummary).mockResolvedValueOnce(
      makeSummary(),
    );

    expect(await getDateLabel()).toBe("Tuesday · April 8");
  });
});

describe("getDashboardRecentOrders", () => {
  afterEach(() => vi.clearAllMocks());

  it("maps recent OrderInboxRowDto[] to dashboard Order rows with short customer name", async () => {
    vi.mocked(analyticsClient.fetchDashboardSummary).mockResolvedValueOnce(
      makeSummary(),
    );

    const orders = await getDashboardRecentOrders();

    expect(orders).toHaveLength(2);
    expect(orders[0].id).toBe("#1042");
    expect(orders[0].customer).toBe("Sasha L.");
    expect(orders[0].items).toBe(2);
    expect(orders[0].total).toBe(152);
    expect(orders[0].status).toBe("paid");
    expect(orders[1].id).toBe("#1041");
  });
});
