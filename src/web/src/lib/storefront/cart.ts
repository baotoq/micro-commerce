export type CartLine = { sku: string; qty: number };
export type CartState = { lines: CartLine[]; promoCode?: string };

export const EMPTY_CART: CartState = { lines: [] };

export function parseCart(raw: string | undefined): CartState {
  if (!raw) return EMPTY_CART;
  try {
    const parsed: unknown = JSON.parse(raw);
    if (typeof parsed !== "object" || parsed === null) return EMPTY_CART;
    const { lines, promoCode } = parsed as {
      lines?: unknown;
      promoCode?: unknown;
    };
    if (!Array.isArray(lines)) return EMPTY_CART;
    const valid: CartLine[] = [];
    for (const line of lines) {
      const { sku, qty } = (line ?? {}) as { sku?: unknown; qty?: unknown };
      if (
        typeof sku !== "string" ||
        typeof qty !== "number" ||
        !Number.isInteger(qty) ||
        qty < 1
      ) {
        return EMPTY_CART;
      }
      valid.push({ sku, qty });
    }
    return {
      lines: valid,
      ...(typeof promoCode === "string" && promoCode ? { promoCode } : {}),
    };
  } catch {
    return EMPTY_CART;
  }
}

export function serializeCart(cart: CartState): string {
  return JSON.stringify(cart);
}

export function addLine(cart: CartState, sku: string, qty: number): CartState {
  const existing = cart.lines.find((l) => l.sku === sku);
  const lines = existing
    ? cart.lines.map((l) => (l.sku === sku ? { ...l, qty: l.qty + qty } : l))
    : [...cart.lines, { sku, qty }];
  return { ...cart, lines };
}

export function setLineQty(
  cart: CartState,
  sku: string,
  qty: number,
): CartState {
  if (qty < 1) return removeLine(cart, sku);
  return {
    ...cart,
    lines: cart.lines.map((l) => (l.sku === sku ? { ...l, qty } : l)),
  };
}

export function removeLine(cart: CartState, sku: string): CartState {
  return { ...cart, lines: cart.lines.filter((l) => l.sku !== sku) };
}

export function cartCount(cart: CartState): number {
  return cart.lines.reduce((sum, l) => sum + l.qty, 0);
}
