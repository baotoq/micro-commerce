import { beforeEach, describe, expect, it, vi } from "vitest";

const readCart = vi.fn();
const writeCart = vi.fn();
const clearCartCookie = vi.fn();
vi.mock("@/lib/storefront/cart-cookie", () => ({
  readCart: (...a: unknown[]) => readCart(...a),
  writeCart: (...a: unknown[]) => writeCart(...a),
  clearCartCookie: (...a: unknown[]) => clearCartCookie(...a),
}));

const fetchProductBySku = vi.fn();
vi.mock("@/lib/catalog/api", () => ({
  fetchProductBySku: (...a: unknown[]) => fetchProductBySku(...a),
}));

const fetchPromotionByCode = vi.fn();
vi.mock("@/lib/catalog/promotions", () => ({
  fetchPromotionByCode: (...a: unknown[]) => fetchPromotionByCode(...a),
}));

const placeStorefrontOrder = vi.fn();
vi.mock("@/lib/catalog/checkout", () => ({
  placeStorefrontOrder: (...a: unknown[]) => placeStorefrontOrder(...a),
}));

const updateTag = vi.fn();
vi.mock("next/cache", () => ({
  updateTag: (...a: unknown[]) => updateTag(...a),
}));

const redirect = vi.fn((url: string) => {
  throw new Error(`NEXT_REDIRECT:${url}`);
});
vi.mock("next/navigation", () => ({
  redirect: (url: string) => redirect(url),
}));

import {
  addToCart,
  applyPromoCode,
  placeOrder,
  removeCartLine,
  setCartQty,
} from "@/lib/storefront/actions";

const activeProduct = {
  sku: "MC-VS-001",
  name: "Persimmon vase",
  price: 86,
  inventory: 5,
  status: "active",
};

beforeEach(() => {
  vi.clearAllMocks();
  readCart.mockResolvedValue({ lines: [] });
  fetchProductBySku.mockResolvedValue(activeProduct);
});

describe("addToCart", () => {
  it("adds a buyable product and clamps qty to inventory", async () => {
    const result = await addToCart("MC-VS-001", 99);
    expect(result).toEqual({ ok: true, qty: 5 });
    expect(writeCart).toHaveBeenCalledWith({
      lines: [{ sku: "MC-VS-001", qty: 5 }],
    });
  });

  it("rejects unknown or non-buyable products", async () => {
    fetchProductBySku.mockResolvedValue(null);
    expect(await addToCart("MC-NOPE", 1)).toEqual({
      ok: false,
      error: "UNAVAILABLE",
    });
    fetchProductBySku.mockResolvedValue({ ...activeProduct, status: "out" });
    expect(await addToCart("MC-VS-001", 1)).toEqual({
      ok: false,
      error: "UNAVAILABLE",
    });
    expect(writeCart).not.toHaveBeenCalled();
  });
});

describe("setCartQty / removeCartLine", () => {
  it("updates quantity within stock", async () => {
    readCart.mockResolvedValue({ lines: [{ sku: "MC-VS-001", qty: 1 }] });
    await setCartQty("MC-VS-001", 3);
    expect(writeCart).toHaveBeenCalledWith({
      lines: [{ sku: "MC-VS-001", qty: 3 }],
    });
  });

  it("removes a line", async () => {
    readCart.mockResolvedValue({ lines: [{ sku: "MC-VS-001", qty: 1 }] });
    await removeCartLine("MC-VS-001");
    expect(writeCart).toHaveBeenCalledWith({ lines: [] });
  });
});

describe("applyPromoCode", () => {
  it("stores a valid active promo", async () => {
    readCart.mockResolvedValue({ lines: [{ sku: "MC-VS-001", qty: 1 }] });
    fetchPromotionByCode.mockResolvedValue({
      code: "WELCOME10",
      status: "active",
      kind: "fixed",
      fixedAmount: 10,
      percentValue: null,
      minOrderAmount: 40,
      startsAt: null,
      endsAt: null,
      description: "",
    });
    const result = await applyPromoCode("welcome10");
    expect(result).toEqual({ ok: true });
    expect(writeCart).toHaveBeenCalledWith({
      lines: [{ sku: "MC-VS-001", qty: 1 }],
      promoCode: "WELCOME10",
    });
  });

  it("rejects unknown and inactive codes", async () => {
    fetchPromotionByCode.mockResolvedValue(null);
    expect(await applyPromoCode("NOPE")).toEqual({
      ok: false,
      error: "PROMO_INVALID",
    });
    fetchPromotionByCode.mockResolvedValue({
      code: "FRIENDS",
      status: "draft",
      kind: "percentage",
      percentValue: 15,
      fixedAmount: null,
      minOrderAmount: null,
      startsAt: null,
      endsAt: null,
      description: "",
    });
    expect(await applyPromoCode("FRIENDS")).toEqual({
      ok: false,
      error: "PROMO_INVALID",
    });
  });
});

describe("placeOrder", () => {
  const shippingInput = {
    customerName: "Bao Buyer",
    shipLine1: "241 Telegraph Ave",
    shipLine2: "Oakland, CA 94612",
    cityState: "Oakland, CA",
    shippingId: "standard" as const,
    paymentBrand: "Visa",
    paymentLastFour: "4242",
  };

  it("posts server-computed shipping/tax, clears the cart, redirects", async () => {
    readCart.mockResolvedValue({ lines: [{ sku: "MC-VS-001", qty: 1 }] });
    placeStorefrontOrder.mockResolvedValue({
      ok: true,
      order: { number: 1043 },
    });

    await expect(placeOrder(shippingInput)).rejects.toThrow(
      "NEXT_REDIRECT:/checkout/confirmation/1043",
    );
    expect(placeStorefrontOrder).toHaveBeenCalledWith(
      expect.objectContaining({
        shippingMethod: "Standard",
        shippingPaid: 8,
        tax: 7.31, // 86 * 0.085
        lines: [{ sku: "MC-VS-001", qty: 1 }],
      }),
    );
    expect(clearCartCookie).toHaveBeenCalled();
    expect(updateTag).toHaveBeenCalledWith("orders");
    expect(updateTag).toHaveBeenCalledWith("listings");
  });

  it("returns the API error without clearing the cart", async () => {
    readCart.mockResolvedValue({ lines: [{ sku: "MC-VS-001", qty: 1 }] });
    placeStorefrontOrder.mockResolvedValue({
      ok: false,
      error: "INSUFFICIENT_STOCK",
    });

    const result = await placeOrder(shippingInput);
    expect(result).toEqual({ ok: false, error: "INSUFFICIENT_STOCK" });
    expect(clearCartCookie).not.toHaveBeenCalled();
  });

  it("rejects an empty cart", async () => {
    readCart.mockResolvedValue({ lines: [] });
    expect(await placeOrder(shippingInput)).toEqual({
      ok: false,
      error: "EMPTY_CART",
    });
  });
});
