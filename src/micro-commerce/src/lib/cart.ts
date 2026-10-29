import { apiFetch } from "@/lib/api";

// Why a Cart line can't be checked out as it is. Mirrors CartLineFlag in the API.
export type CartLineFlag = "Unavailable" | "InsufficientStock" | "OwnMerchant" | "TotalTooLarge";

// Mirrors CartLineResponse in the API: the Product, Shop and price are null once the Variant is gone.
export type CartLine = {
  variantId: string;
  quantity: number;
  productId: string | null;
  productName: string | null;
  optionValues: string[];
  shopId: string | null;
  shopName: string | null;
  unitPrice: number | null;
  lineTotal: number | null;
  available: number | null;
  flags: CartLineFlag[];
};

// Mirrors CartResponse in the API. The items total leaves out Unavailable and TotalTooLarge lines.
export type Cart = {
  lines: CartLine[];
  itemsTotal: number;
};

// Mirrors Cart.MaxQuantity in the API.
export const maxQuantity = 99;

// Server-side: the caller passes the session's access token.
export function getCart(accessToken: string) {
  return apiFetch<Cart>("/cart", { headers: { Authorization: `Bearer ${accessToken}` } });
}

export function addToCart(variantId: string, quantity: number) {
  return apiFetch<Cart>("/cart/lines", { method: "POST", body: JSON.stringify({ variantId, quantity }) });
}

export function updateCartLine(variantId: string, quantity: number) {
  return apiFetch<Cart>(`/cart/lines/${encodeURIComponent(variantId)}`, {
    method: "PUT",
    body: JSON.stringify({ quantity }),
  });
}

export function removeCartLine(variantId: string) {
  return apiFetch<Cart>(`/cart/lines/${encodeURIComponent(variantId)}`, { method: "DELETE" });
}

// What a flag means, to show next to the line.
export function flagReason(flag: CartLineFlag, line: CartLine) {
  switch (flag) {
    case "Unavailable":
      return "No longer available.";
    case "InsufficientStock":
      return `Only ${line.available ?? 0} available.`;
    case "OwnMerchant":
      return "Sold by your own Shop, so you can't buy it.";
    case "TotalTooLarge":
      return "Takes the Cart total beyond what can be paid.";
  }
}
