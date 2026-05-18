// web/src/lib/seller/promos/data.test.ts
import { describe, expect, it } from "vitest";
import {
  getPromoStats,
  getPromos,
  getPromoTabs,
} from "@/lib/seller/promos/data";

describe("seller promos data", () => {
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
