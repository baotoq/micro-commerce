// Storefront read loaders. AUTH RULE (Keycloak): cached loaders are anonymous —
// calling auth()/getAccessToken() inside "use cache" is illegal. Tagged
// "listings" so every existing product mutation (and storefront checkout's
// inventory decrement) invalidates the shop with the already-wired
// revalidateTag("listings").
import { cacheTag } from "next/cache";
import {
  fetchProductBySku,
  fetchProductCategories,
  fetchProducts,
  type ProductQuery,
} from "@/lib/catalog/api";
import type { Listing } from "@/lib/seller/listings/types";

export const SHOP_PAGE_SIZE = 12;

export type ShopQuery = Pick<
  ProductQuery,
  "page" | "category" | "sort" | "search"
>;

export async function getShopProducts(query: ShopQuery = {}) {
  "use cache";
  cacheTag("listings");
  return fetchProducts({
    page: query.page ?? 1,
    limit: SHOP_PAGE_SIZE,
    buyable: true,
    category: query.category,
    sort: query.sort,
    search: query.search,
  });
}

export async function getShopProduct(sku: string): Promise<Listing | null> {
  "use cache";
  cacheTag("listings");
  const product = await fetchProductBySku(sku);
  // Draft/out products exist for the seller but are not buyable.
  if (!product || product.status === "draft" || product.status === "out")
    return null;
  return product;
}

export async function getShopCategories(): Promise<string[]> {
  "use cache";
  cacheTag("listings");
  return fetchProductCategories();
}
