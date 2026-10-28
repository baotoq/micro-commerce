import { apiFetch } from "@/lib/api";
import type { ProductOption } from "@/lib/catalog";

// The storefront is anonymous, so these work from the server and the browser alike.

// Mirrors PagedResponse in the API.
export type Page<T> = {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
};

// Mirrors StorefrontProductSummaryResponse in the API: a Published Product as the storefront lists it.
export type StorefrontProductSummary = {
  id: string;
  name: string;
  categoryId: string;
  minPrice: number;
  maxPrice: number;
  inStock: boolean;
  shopId: string;
  shopName: string;
};

// Mirrors StorefrontVariantResponse in the API: one value of each option, in option order.
export type StorefrontVariant = {
  id: string;
  optionValues: string[];
  price: number;
  stock: number;
};

// Mirrors StorefrontProductResponse in the API.
export type StorefrontProduct = {
  id: string;
  name: string;
  description: string | null;
  categoryId: string;
  options: ProductOption[];
  variants: StorefrontVariant[];
  shopId: string;
  shopName: string;
};

// Mirrors ShopResponse in the API: a Shop page's header.
export type Shop = {
  id: string;
  shopName: string;
  description: string | null;
};

export const storefrontPageSize = 24;

// Mirrors ListStorefrontProductsRequest.QueryMaxLength in the API.
export const searchQueryMaxLength = 200;

// `query` matches words in Product names and descriptions; `categoryId` includes the Category's descendants.
export function listStorefrontProducts({
  page = 1,
  shopId,
  query,
  categoryId,
}: { page?: number; shopId?: string; query?: string; categoryId?: string } = {}) {
  const params = new URLSearchParams({ page: String(page), pageSize: String(storefrontPageSize) });
  if (shopId) params.set("shopId", shopId);
  if (query) params.set("query", query);
  if (categoryId) params.set("categoryId", categoryId);
  return apiFetch<Page<StorefrontProductSummary>>(`/storefront/products?${params}`);
}

export function getStorefrontProduct(id: string) {
  return apiFetch<StorefrontProduct>(`/storefront/products/${encodeURIComponent(id)}`);
}

export function getShop(id: string) {
  return apiFetch<Shop>(`/storefront/shops/${encodeURIComponent(id)}`);
}

// A search parameter's single value, or undefined when it's missing or blank.
export function searchParam(value: string | string[] | undefined) {
  const text = (Array.isArray(value) ? value[0] : value)?.trim();
  return text || undefined;
}

const uuid = /^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i;

// A search parameter holding an ID, or undefined unless it is one, so a mangled link lists everything.
export function idParam(value: string | string[] | undefined) {
  const id = searchParam(value);
  return id && uuid.test(id) ? id : undefined;
}

// The `page` search parameter as a page number; anything else is the first page.
export function pageNumber(value: string | string[] | undefined) {
  const page = Number(value);
  return Number.isInteger(page) && page > 0 ? page : 1;
}
