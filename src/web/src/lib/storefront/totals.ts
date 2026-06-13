import type { PromotionDto } from "@/lib/seller/promos/types";

// Demo constants per spec §7. Single source of truth for shipping + tax.
export const TAX_RATE = 0.085;

export const SHIPPING_OPTIONS = [
  {
    id: "standard",
    label: "Standard",
    detail: "5–7 business days · USPS Ground",
    price: 8,
  },
  {
    id: "express",
    label: "Express",
    detail: "2–3 business days · UPS Saver",
    price: 22,
  },
  {
    id: "pickup",
    label: "Local pickup",
    detail: "Oakland studio · ready Fri 4–7pm",
    price: 0,
  },
] as const;

export type ShippingId = (typeof SHIPPING_OPTIONS)[number]["id"];

export type PricedLine = {
  sku: string;
  name: string;
  price: number;
  qty: number;
};

export type Totals = {
  subtotal: number;
  discount: number;
  shipping: number;
  tax: number;
  total: number;
};

function roundMoney(n: number): number {
  return Math.round(n * 100) / 100;
}

/** Discount applies to the item subtotal only; fixed promos cap at the subtotal (spec §7). */
export function computeDiscount(
  subtotal: number,
  promo: PromotionDto | null,
): number {
  if (!promo) return 0;
  if (promo.kind === "percentage" && promo.percentValue) {
    return roundMoney((subtotal * promo.percentValue) / 100);
  }
  if (promo.kind === "fixed" && promo.fixedAmount) {
    return Math.min(promo.fixedAmount, subtotal);
  }
  return 0;
}

export function computeTotals(
  lines: PricedLine[],
  promo: PromotionDto | null,
  shipping: ShippingId,
): Totals {
  const subtotal = roundMoney(
    lines.reduce((sum, l) => sum + l.price * l.qty, 0),
  );
  const discount = computeDiscount(subtotal, promo);
  const shippingCost =
    SHIPPING_OPTIONS.find((o) => o.id === shipping)?.price ?? 0;
  const tax = roundMoney((subtotal - discount) * TAX_RATE);
  return {
    subtotal,
    discount,
    shipping: shippingCost,
    tax,
    total: roundMoney(subtotal - discount + shippingCost + tax),
  };
}
