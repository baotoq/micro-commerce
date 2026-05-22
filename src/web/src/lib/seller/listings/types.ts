// web/src/lib/seller/listings/types.ts
export type ListingStatus = "active" | "low" | "out" | "draft";

/**
 * Default page size for the seller listings table. Shared between the server
 * component (`page.tsx`), the API route (`/api/listings`), the data loader
 * (`getListings`), and the client `ListingsTable` so changing it in one place
 * doesn't silently break pagination math elsewhere.
 */
export const LISTINGS_PAGE_SIZE = 9;

export type Listing = {
  sku: string;
  name: string;
  category: string;
  price: number;
  inventory: number;
  status: ListingStatus;
  views7d: number;
};

export type ListingCounts = {
  total: number;
  active: number;
  low: number;
  out: number;
  draft: number;
};
