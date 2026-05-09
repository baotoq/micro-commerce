// web/src/lib/seller/types.ts
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

export type OrderStatus = "paid" | "fulfilled" | "refunded" | "pending";

export type Order = {
  id: string; // includes leading "#"
  customer: string;
  items: number;
  total: number;
  status: OrderStatus;
  placedAt: string; // ISO date
};

export type KpiFormat = "currency" | "number" | "percent";

export type KpiPoint = {
  label: string;
  value: number;
  format: KpiFormat;
  delta?: number; // signed percent vs prior period
};

export type RevenuePoint = { day: string; amount: number };

export type SourceBreakdown = { name: string; visits: number; share: number };

export type FunnelStage = { label: string; count: number };

export type TopProduct = {
  sku: string;
  name: string;
  units: number;
  revenue: number;
};

export type TodayItem = { label: string; count?: number };

export type ListingCounts = {
  total: number;
  active: number;
  low: number;
  out: number;
  draft: number;
};

export type Brand = { name: string; owner: string };
