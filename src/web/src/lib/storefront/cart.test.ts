import { describe, expect, it } from "vitest";
import {
  addLine,
  cartCount,
  EMPTY_CART,
  parseCart,
  removeLine,
  serializeCart,
  setLineQty,
} from "@/lib/storefront/cart";

describe("cart codec", () => {
  it("round-trips through serialize/parse", () => {
    const cart = {
      lines: [{ sku: "MC-VS-001", qty: 2 }],
      promoCode: "WELCOME10",
    };
    expect(parseCart(serializeCart(cart))).toEqual(cart);
  });

  it("parses garbage to the empty cart", () => {
    expect(parseCart(undefined)).toEqual(EMPTY_CART);
    expect(parseCart("not json")).toEqual(EMPTY_CART);
    expect(parseCart('{"lines":"nope"}')).toEqual(EMPTY_CART);
    expect(parseCart('{"lines":[{"sku":1,"qty":"x"}]}')).toEqual(EMPTY_CART);
  });

  it("addLine merges quantities for the same sku", () => {
    const cart = addLine(addLine(EMPTY_CART, "MC-VS-001", 1), "MC-VS-001", 2);
    expect(cart.lines).toEqual([{ sku: "MC-VS-001", qty: 3 }]);
  });

  it("setLineQty updates and removes at zero", () => {
    const cart = addLine(EMPTY_CART, "MC-VS-001", 2);
    expect(setLineQty(cart, "MC-VS-001", 5).lines[0]?.qty).toBe(5);
    expect(setLineQty(cart, "MC-VS-001", 0).lines).toEqual([]);
  });

  it("removeLine drops the sku and keeps the promo", () => {
    const cart = {
      lines: [
        { sku: "MC-VS-001", qty: 1 },
        { sku: "MC-BW-014", qty: 2 },
      ],
      promoCode: "WELCOME10",
    };
    expect(removeLine(cart, "MC-VS-001")).toEqual({
      lines: [{ sku: "MC-BW-014", qty: 2 }],
      promoCode: "WELCOME10",
    });
  });

  it("counts total units", () => {
    expect(cartCount(EMPTY_CART)).toBe(0);
    expect(
      cartCount({
        lines: [
          { sku: "a", qty: 2 },
          { sku: "b", qty: 1 },
        ],
      }),
    ).toBe(3);
  });
});
