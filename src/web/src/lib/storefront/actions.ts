"use server";

import { revalidateTag } from "next/cache";
import { redirect } from "next/navigation";
import { fetchProductBySku } from "@/lib/catalog/api";
import {
  type CheckoutError,
  type PlaceOrderPayload,
  placeStorefrontOrder,
} from "@/lib/catalog/checkout";
import { fetchPromotionByCode } from "@/lib/catalog/promotions";
import { addLine, removeLine, setLineQty } from "@/lib/storefront/cart";
import {
  clearCartCookie,
  readCart,
  writeCart,
} from "@/lib/storefront/cart-cookie";
import {
  computeTotals,
  type PricedLine,
  SHIPPING_OPTIONS,
  type ShippingId,
} from "@/lib/storefront/totals";

export type CartActionResult =
  | { ok: true; qty?: number }
  | { ok: false; error: "UNAVAILABLE" | "PROMO_INVALID" };

async function buyableProduct(sku: string) {
  const product = await fetchProductBySku(sku);
  if (!product || product.status === "draft" || product.status === "out")
    return null;
  return product;
}

export async function addToCart(
  sku: string,
  qty: number,
): Promise<CartActionResult> {
  const product = await buyableProduct(sku);
  if (!product || product.inventory < 1)
    return { ok: false, error: "UNAVAILABLE" };

  const cart = await readCart();
  const existing = cart.lines.find((l) => l.sku === sku)?.qty ?? 0;
  const clamped = Math.min(Math.max(qty, 1), product.inventory - existing);
  if (clamped < 1) return { ok: false, error: "UNAVAILABLE" };

  await writeCart(addLine(cart, sku, clamped));
  return { ok: true, qty: existing + clamped };
}

export async function setCartQty(
  sku: string,
  qty: number,
): Promise<CartActionResult> {
  const cart = await readCart();
  if (qty > 0) {
    const product = await buyableProduct(sku);
    if (!product) return { ok: false, error: "UNAVAILABLE" };
    qty = Math.min(qty, product.inventory);
  }
  await writeCart(setLineQty(cart, sku, qty));
  return { ok: true, qty };
}

export async function removeCartLine(sku: string): Promise<CartActionResult> {
  await writeCart(removeLine(await readCart(), sku));
  return { ok: true };
}

export async function applyPromoCode(code: string): Promise<CartActionResult> {
  const normalized = code.trim().toUpperCase();
  if (!normalized) return { ok: false, error: "PROMO_INVALID" };

  const promo = await fetchPromotionByCode(normalized);
  const now = Date.now();
  if (
    !promo ||
    promo.status !== "active" ||
    (promo.startsAt && now < Date.parse(promo.startsAt)) ||
    (promo.endsAt && now > Date.parse(promo.endsAt))
  ) {
    return { ok: false, error: "PROMO_INVALID" };
  }

  const cart = await readCart();
  await writeCart({ ...cart, promoCode: normalized });
  return { ok: true };
}

export async function removePromoCode(): Promise<CartActionResult> {
  const cart = await readCart();
  await writeCart({ lines: cart.lines });
  return { ok: true };
}

export type PlaceOrderInput = {
  customerName: string;
  shipLine1: string;
  shipLine2: string;
  cityState: string;
  shippingId: ShippingId;
  paymentBrand: string;
  paymentLastFour: string;
};

export type PlaceOrderResult = {
  ok: false;
  error: CheckoutError;
  detail?: string;
};

export async function placeOrder(
  input: PlaceOrderInput,
): Promise<PlaceOrderResult> {
  const cart = await readCart();
  if (cart.lines.length === 0) return { ok: false, error: "EMPTY_CART" };

  // Price the cart server-side to derive shipping/tax (the API re-prices items
  // and the discount as the authority; spec §7).
  const priced: PricedLine[] = [];
  for (const line of cart.lines) {
    const product = await buyableProduct(line.sku);
    if (!product) return { ok: false, error: "PRODUCT_NOT_FOUND" };
    priced.push({
      sku: line.sku,
      name: product.name,
      price: product.price,
      qty: line.qty,
    });
  }

  const promo = cart.promoCode
    ? await fetchPromotionByCode(cart.promoCode)
    : null;
  const shippingOption = SHIPPING_OPTIONS.find(
    (o) => o.id === input.shippingId,
  );
  if (!shippingOption) return { ok: false, error: "UNKNOWN" };
  const totals = computeTotals(priced, promo, input.shippingId);

  const payload: PlaceOrderPayload = {
    customerName: input.customerName,
    shipLine1: input.shipLine1,
    shipLine2: input.shipLine2,
    cityState: input.cityState,
    shippingMethod: shippingOption.label,
    shippingPaid: totals.shipping,
    tax: totals.tax,
    paymentBrand: input.paymentBrand,
    paymentLastFour: input.paymentLastFour,
    ...(cart.promoCode ? { promoCode: cart.promoCode } : {}),
    lines: cart.lines.map((l) => ({ sku: l.sku, qty: l.qty })),
  };

  const result = await placeStorefrontOrder(payload);
  if (!result.ok)
    return { ok: false, error: result.error, detail: result.detail };

  await clearCartCookie();
  // Mirror the API's eviction matrix on the web cache (lib/seller/*/data.ts tags),
  // plus "listings" because checkout decremented inventory.
  for (const tag of [
    "orders",
    "customers",
    "analytics",
    "dashboard",
    "payouts",
    "listings",
  ]) {
    revalidateTag(tag);
  }
  redirect(`/checkout/confirmation/${result.order.number}`);
  return { ok: false, error: "UNKNOWN" }; // unreachable; redirect throws
}
