import { describe, expect, it } from "vitest";
import type { PromotionDto } from "@/lib/seller/promos/types";
import {
  computeDiscount,
  computeTotals,
  SHIPPING_OPTIONS,
} from "@/lib/storefront/totals";

const lines = [
  { sku: "MC-VS-001", name: "Persimmon vase", price: 86, qty: 1 },
  { sku: "MC-BW-014", name: "Forest bowl", price: 68, qty: 2 },
]; // subtotal 222

const fixed10: PromotionDto = {
  code: "WELCOME10",
  description: "first order",
  kind: "fixed",
  percentValue: null,
  fixedAmount: 10,
  minOrderAmount: 40,
  startsAt: null,
  endsAt: null,
  status: "active",
};

describe("computeDiscount", () => {
  it("fixed promos cap at the subtotal", () => {
    expect(computeDiscount(222, fixed10)).toBe(10);
    expect(computeDiscount(5, { ...fixed10, minOrderAmount: null })).toBe(5);
  });

  it("percentage promos round to cents", () => {
    const pct: PromotionDto = {
      ...fixed10,
      kind: "percentage",
      percentValue: 15,
      fixedAmount: null,
    };
    expect(computeDiscount(86, pct)).toBe(12.9);
  });

  it("no promo means zero", () => {
    expect(computeDiscount(222, null)).toBe(0);
  });
});

describe("computeTotals", () => {
  it("discount hits the subtotal; tax applies after discount; shipping added last", () => {
    const t = computeTotals(lines, fixed10, "standard");
    expect(t.subtotal).toBe(222);
    expect(t.discount).toBe(10);
    expect(t.shipping).toBe(8);
    expect(t.tax).toBe(18.02); // (222-10) * 0.085
    expect(t.total).toBe(238.02);
  });

  it("pickup ships free", () => {
    expect(computeTotals(lines, null, "pickup").shipping).toBe(0);
  });

  it("exposes the three shipping options", () => {
    expect(SHIPPING_OPTIONS.map((o) => o.id)).toEqual([
      "standard",
      "express",
      "pickup",
    ]);
  });
});
