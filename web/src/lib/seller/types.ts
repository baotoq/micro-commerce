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

export type SellerApplication = {
  shopName: string;
  domain: string;
  available: boolean;
  category: string;
  categories: string[];
  stepsLeft: number;
};

export type SetupStep = {
  label: string;
  status: "done" | "active" | "pending";
};

export type LaunchTask = {
  label: string;
  subject: string;
  done: boolean;
  hint?: string;
};

export type ShippingOption = {
  label: string;
  sub: string;
  price: number;
  selected: boolean;
};

export type OrderDetail = {
  id: string;
  customer: string;
  shortCustomer: string;
  productName: string;
  productSubtitle: string;
  qty: number;
  subtotal: number;
  shippingLabel: string;
  shippingCost: number;
  customerPaid: number;
  feePct: number;
  fee: number;
  net: number;
  shipTo: { name: string; line1: string; cityState: string };
  customerNote: string;
};

export type LedgerEntry = {
  date: string;
  label: string;
  subject: string;
  amount: number;
  type: "payout" | "sale" | "fee";
};

export type PayoutSummary = {
  lastPayout: number;
  lastPayoutSentLabel: string;
  available: number;
  availableSendsOn: string;
  availableFromOrders: number;
  lifetime: number;
  lifetimeOrders: number;
  lifetimeRange: string;
};

export type FirstOrderKpi = {
  label: string;
  value: string;
  delta: string;
  spark: number[];
};
export type FirstMonthKpi = {
  label: string;
  value: string;
  delta: string;
  spark: number[];
};
export type TopSeller = {
  sku: string;
  name: string;
  soldLabel: string;
  revenue: number;
  tone: string;
};
