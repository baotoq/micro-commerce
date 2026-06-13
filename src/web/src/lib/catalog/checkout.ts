import "server-only";
import { getAccessToken } from "@/lib/auth/token";
import type { OrderDetailDto } from "@/lib/catalog/orders";

// Buyer checkout + confirmation. Both attach the session bearer token and are
// never cached (AUTH RULE: authenticated fetchers are uncached). The endpoints
// return the raw OrderDetailDto wire shape (camelCase ASP.NET JSON), which is
// the only detail type carrying both `.number` (consumed by the placeOrder
// action / confirmation page) and `.summary` (totals + discount).

function apiBase(): string {
  const url = process.env.API_URL;
  if (!url) {
    throw new Error(
      "API_URL env var is not set. Run the app via the Aspire AppHost (`dotnet run --project src/AppHost`).",
    );
  }
  return url.replace(/\/$/, "");
}

export type PlaceOrderPayload = {
  customerName: string;
  shipLine1: string;
  shipLine2: string;
  cityState: string;
  shippingMethod: string;
  shippingPaid: number;
  tax: number;
  paymentBrand: string;
  paymentLastFour: string;
  promoCode?: string;
  lines: { sku: string; qty: number }[];
};

export type CheckoutError =
  | "EMPTY_CART"
  | "PRODUCT_NOT_FOUND"
  | "INSUFFICIENT_STOCK"
  | "PROMO_INVALID"
  | "PROMO_MIN_ORDER"
  | "UNKNOWN";

export type CheckoutResult =
  | { ok: true; order: OrderDetailDto }
  | { ok: false; error: CheckoutError; detail?: string };

export async function placeStorefrontOrder(
  payload: PlaceOrderPayload,
): Promise<CheckoutResult> {
  const res = await fetch(`${apiBase()}/api/orders/checkout`, {
    cache: "no-store",
    method: "POST",
    headers: {
      "Content-Type": "application/json",
      Authorization: `Bearer ${await getAccessToken()}`,
    },
    body: JSON.stringify(payload),
  });
  if (res.status === 201) {
    return { ok: true, order: (await res.json()) as OrderDetailDto };
  }
  if (res.status === 409) {
    const problem = (await res.json()) as { title?: string; detail?: string };
    const known: CheckoutError[] = [
      "EMPTY_CART",
      "PRODUCT_NOT_FOUND",
      "INSUFFICIENT_STOCK",
      "PROMO_INVALID",
      "PROMO_MIN_ORDER",
    ];
    const error = known.find((k) => k === problem.title) ?? "UNKNOWN";
    return { ok: false, error, detail: problem.detail };
  }
  throw new Error(`POST /api/orders/checkout failed: ${res.status}`);
}

export async function fetchOrderConfirmation(
  number: number,
): Promise<OrderDetailDto | null> {
  const res = await fetch(`${apiBase()}/api/orders/${number}/confirmation`, {
    cache: "no-store",
    headers: { Authorization: `Bearer ${await getAccessToken()}` },
  });
  if (res.status === 404) return null;
  if (!res.ok)
    throw new Error(
      `GET /api/orders/${number}/confirmation failed: ${res.status}`,
    );
  return (await res.json()) as OrderDetailDto;
}
