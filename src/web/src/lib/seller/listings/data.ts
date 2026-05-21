// Listings are persisted by the Catalog API. Dev/e2e data is seeded by the
// SeedData/products.json fixture in src/Services/Catalog.API/src/Api.
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

export async function getListings(
  query: { page?: number; limit?: number; status?: ListingStatus } = {},
) {
  return fetchProducts({
    page: query.page ?? 1,
    limit: query.limit ?? LISTINGS_PAGE_SIZE,
    status: query.status,
  });
}

export async function getListingBySku(sku: string): Promise<Listing | null> {
  return fetchProductBySku(sku);
}

export async function getListingCounts(): Promise<ListingCounts> {
  return fetchProductCounts();
}
