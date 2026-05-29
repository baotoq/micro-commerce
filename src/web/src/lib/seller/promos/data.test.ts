import { afterEach, describe, expect, it, vi } from "vitest";

vi.mock("server-only", () => ({}));
vi.mock("next/cache", () => ({ cacheTag: vi.fn() }));
vi.mock("@/lib/catalog/promotions", () => ({
  fetchPromotions: vi.fn(),
  fetchPromotionStats: vi.fn(),
}));

import * as promotionsClient from "@/lib/catalog/promotions";
import {
  getPromoStats,
  getPromos,
  getPromoTabs,
} from "@/lib/seller/promos/data";
import type {
  PromotionDto,
  PromotionStatsDto,
} from "@/lib/seller/promos/types";

function makeDto(overrides: Partial<PromotionDto> = {}): PromotionDto {
  return {
    code: "SPRING20",
    description: "sitewide",
    kind: "percentage",
    percentValue: 20,
    fixedAmount: null,
    minOrderAmount: null,
    startsAt: "2025-04-01T00:00:00Z",
    endsAt: "2025-05-15T00:00:00Z",
    status: "active",
    ...overrides,
  };
}

describe("getPromos", () => {
  afterEach(() => {
    vi.clearAllMocks();
  });

  it("maps DTO to PromoCode with formatPromoWhat and formatPromoWindow", async () => {
    vi.mocked(promotionsClient.fetchPromotions).mockResolvedValueOnce({
      items: [makeDto()],
      total: 1,
      page: 1,
      pageSize: 20,
    });

    const promos = await getPromos();

    expect(promos).toHaveLength(1);
    expect(promos[0].code).toBe("SPRING20");
    expect(promos[0].what).toBe("20% off · sitewide");
    expect(promos[0].window).toBe("Apr 1 → May 15");
    expect(promos[0].status).toBe("Active");
    expect(promos[0].tone).toBe("good");
    expect(promos[0].redemptions).toBe(0);
    expect(promos[0].drivenRevenue).toBe(0);
  });

  it("sets tone=mute for Draft and Ended promos", async () => {
    vi.mocked(promotionsClient.fetchPromotions).mockResolvedValueOnce({
      items: [
        makeDto({
          code: "FRIENDS",
          status: "draft",
          startsAt: null,
          endsAt: null,
        }),
        makeDto({
          code: "BLOOM",
          status: "ended",
          startsAt: null,
          endsAt: null,
        }),
      ],
      total: 2,
      page: 1,
      pageSize: 20,
    });

    const promos = await getPromos();

    expect(promos[0].tone).toBe("mute");
    expect(promos[0].status).toBe("Draft");
    expect(promos[1].tone).toBe("mute");
    expect(promos[1].status).toBe("Ended");
  });

  it("maps fixed discount with minOrderAmount", async () => {
    vi.mocked(promotionsClient.fetchPromotions).mockResolvedValueOnce({
      items: [
        makeDto({
          code: "WELCOME10",
          kind: "fixed",
          percentValue: null,
          fixedAmount: 10,
          minOrderAmount: 40,
          startsAt: null,
          endsAt: null,
          status: "active",
        }),
      ],
      total: 1,
      page: 1,
      pageSize: 20,
    });

    const promos = await getPromos();

    expect(promos[0].what).toBe("$10 off · sitewide $40+");
    expect(promos[0].window).toBe("Always");
  });

  it("sets window=Drafted for Draft promo without dates", async () => {
    vi.mocked(promotionsClient.fetchPromotions).mockResolvedValueOnce({
      items: [makeDto({ status: "draft", startsAt: null, endsAt: null })],
      total: 1,
      page: 1,
      pageSize: 20,
    });

    const promos = await getPromos();
    expect(promos[0].window).toBe("Drafted");
  });

  it("does not set highlight on any promo (field dropped)", async () => {
    vi.mocked(promotionsClient.fetchPromotions).mockResolvedValueOnce({
      items: [makeDto()],
      total: 1,
      page: 1,
      pageSize: 20,
    });

    const promos = await getPromos();
    expect(promos[0].highlight).toBeUndefined();
  });
});

describe("getPromoStats", () => {
  afterEach(() => {
    vi.clearAllMocks();
  });

  it("maps PromotionStatsDto to 4 stat cards", async () => {
    const statsDto: PromotionStatsDto = {
      active: 3,
      draft: 1,
      ended: 1,
      total: 5,
    };
    vi.mocked(promotionsClient.fetchPromotionStats).mockResolvedValueOnce(
      statsDto,
    );

    const stats = await getPromoStats();

    expect(stats).toHaveLength(4);
    expect(stats[0].label).toBe("Active");
    expect(stats[0].value).toBe("3");
    expect(stats[0].sub).toBe("currently live");
    expect(stats[1].label).toBe("Draft");
    expect(stats[1].value).toBe("1");
    expect(stats[1].sub).toBe("not yet started");
    expect(stats[2].label).toBe("Ended");
    expect(stats[2].value).toBe("1");
    expect(stats[2].sub).toBe("past their window");
    expect(stats[3].label).toBe("Total");
    expect(stats[3].value).toBe("5");
    expect(stats[3].sub).toBe("all-time");
  });

  it("includes spark arrays on each stat card", async () => {
    vi.mocked(promotionsClient.fetchPromotionStats).mockResolvedValueOnce({
      active: 0,
      draft: 0,
      ended: 0,
      total: 0,
    });

    const stats = await getPromoStats();
    for (const stat of stats) {
      expect(Array.isArray(stat.spark)).toBe(true);
      expect(stat.spark.length).toBeGreaterThan(0);
    }
  });
});

describe("getPromoTabs", () => {
  afterEach(() => {
    vi.clearAllMocks();
  });

  it("returns 3 tabs with Promotions active and count from stats total", async () => {
    vi.mocked(promotionsClient.fetchPromotionStats).mockResolvedValueOnce({
      active: 3,
      draft: 1,
      ended: 1,
      total: 5,
    });

    const tabs = await getPromoTabs();

    expect(tabs).toHaveLength(3);
    expect(tabs[0].label).toBe("Promotions");
    expect(tabs[0].on).toBe(true);
    expect(tabs[0].count).toBe(5);
    expect(tabs[1].label).toBe("Automatic");
    expect(tabs[1].count).toBe(1);
    expect(tabs[2].label).toBe("Gift cards");
    expect(tabs[2].count).toBe(0);
  });
});
