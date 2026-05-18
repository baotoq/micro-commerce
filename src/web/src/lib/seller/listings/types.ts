// web/src/lib/seller/listings/types.ts
export type ListingStatus = "active" | "low" | "out" | "draft";

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
