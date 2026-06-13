import "server-only";
import { cookies } from "next/headers";
import {
  type CartState,
  EMPTY_CART,
  parseCart,
  serializeCart,
} from "@/lib/storefront/cart";

// httpOnly cookie so only server actions can mutate the cart. The cookie holds
// skus+qtys+promo code only — prices are always re-fetched from the API, so a
// stale or tampered cookie cannot corrupt totals (spec §7).
const CART_COOKIE = "mc_cart";
const ONE_WEEK_SECONDS = 60 * 60 * 24 * 7;

export async function readCart(): Promise<CartState> {
  const store = await cookies();
  return parseCart(store.get(CART_COOKIE)?.value);
}

export async function writeCart(cart: CartState): Promise<void> {
  const store = await cookies();
  if (cart.lines.length === 0 && !cart.promoCode) {
    store.delete(CART_COOKIE);
    return;
  }
  store.set(CART_COOKIE, serializeCart(cart), {
    httpOnly: true,
    sameSite: "lax",
    path: "/",
    maxAge: ONE_WEEK_SECONDS,
  });
}

export async function clearCartCookie(): Promise<void> {
  const store = await cookies();
  store.delete(CART_COOKIE);
}

export { EMPTY_CART };
