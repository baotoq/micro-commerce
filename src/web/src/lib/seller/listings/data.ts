// Listings are persisted by the Catalog API. Dev/e2e data is seeded by the
// SeedData/products.json fixture in src/Services/Catalog.API/src/Api.
import { cacheTag } from "next/cache";
import {
  fetchProductBySku,
  fetchProductCounts,
  fetchProducts,
} from "@/lib/catalog/api";
import {
  LISTINGS_PAGE_SIZE,
  type Listing,
  type ListingCounts,
  type ListingStatus,
} from "@/lib/seller/listings/types";

// Cache Components: each loader is tagged "listings" so any server action
// that mutates a product can invalidate the whole table with one
// `revalidateTag("listings")` instead of enumerating paths.

export async function getListings(
  query: { page?: number; limit?: number; status?: ListingStatus } = {},
) {
  "use cache";
  cacheTag("listings");
  return fetchProducts({
    page: query.page ?? 1,
    limit: query.limit ?? LISTINGS_PAGE_SIZE,
    status: query.status,
  });
}

export async function getListingBySku(sku: string): Promise<Listing | null> {
  "use cache";
  cacheTag("listings");
  return fetchProductBySku(sku);
}

export async function getListingCounts(): Promise<ListingCounts> {
  "use cache";
  cacheTag("listings");
  return fetchProductCounts();
}
