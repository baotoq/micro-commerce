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

// — Seller management: orders inbox —

export type OrderInboxStatus =
  | "New"
  | "Packed"
  | "Shipped"
  | "Delivered"
  | "Refund req."
  | "Cancelled";

export type StatusTone = "warn" | "mute" | "good" | "bad";

export type OrderInboxRow = {
  id: string; // includes leading "#"
  placedLabel: string; // e.g. "Today · 2:14 PM"
  customer: string;
  city: string;
  items: string;
  qty: number;
  total: number;
  ship: string;
  status: OrderInboxStatus;
  tone: StatusTone;
  age: string;
  starred?: boolean;
};

export type OrderInboxTab = {
  label: string;
  count: number;
  on?: boolean;
};

export type OrderInboxSummary = {
  totalLifetime: number;
  needAction: number;
};

// — Seller management: order detail —

export type OrderFulfillment = {
  idx: number;
  of: number;
  status: "shipped" | "awaiting";
  productName: string;
  productSubtitle: string;
  productTone: string;
  qty: number;
  price: number;
  tracking?: string;
  restockNote?: string;
};

export type OrderRefundItem = {
  name: string;
  qty: number;
  price: number;
  selected: boolean;
  partial?: number;
};

export type OrderRefund = {
  refundable: number;
  itemsCount: number;
  items: OrderRefundItem[];
  reason: string;
  restockOptions: string[];
  restockSelected: string;
  total: number;
  lastFour: string;
};

export type OrderCustomer = {
  name: string;
  shortName: string;
  lifetimeOrdersLabel: string;
  ship: { line1: string; line2: string };
  billSameAsShip: boolean;
  email: string;
};

export type OrderSummary = {
  subtotal: number;
  itemsCount: number;
  shipping: number;
  tax: number;
  paid: number;
  feePct: number;
  fee: number;
  labelCarrier: string;
  labelCost: number;
  net: number;
};

export type OrderTimelineEvent = {
  icon: string;
  title: string;
  sub: string;
  when: string;
  on?: boolean;
  tone?: "warn";
};

export type OrderDetailFull = {
  id: string;
  status: string;
  statusTone: StatusTone;
  customerShort: string;
  age: string;
  fulfillments: OrderFulfillment[];
  refund: OrderRefund;
  customer: OrderCustomer;
  summary: OrderSummary;
  internalNote: string;
  customerTags: string[];
  customerTagsActive: string[];
  timeline: OrderTimelineEvent[];
  outstandingProductName: string;
};

// — Seller management: promos —

export type PromoStatus = "Active" | "Ended" | "Draft";

export type PromoCode = {
  code: string;
  what: string;
  window: string;
  redemptions: number;
  drivenRevenue: number;
  status: PromoStatus;
  tone: "good" | "mute";
  highlight?: boolean;
};

export type PromoStat = {
  label: string;
  value: string;
  sub: string;
  spark: number[];
};

export type PromoTab = {
  label: string;
  count: number;
  on?: boolean;
};

// — Seller management: marketing —

export type MarketingAudience = {
  label: string;
  sub: string;
  count: number;
  on: boolean;
};

export type MarketingTemplate = {
  label: string;
  on: boolean;
};

export type MarketingScheduleOption = {
  label: string;
  sub: string;
  on: boolean;
};

export type MarketingFollowup = {
  label: string;
  on: boolean;
};

export type MarketingDraft = {
  senderName: string;
  senderInitial: string;
  recipientCount: number;
  deliverable: string;
  audiences: MarketingAudience[];
  templates: MarketingTemplate[];
  subject: string;
  subjectCharCount: number;
  subjectMaxChars: number;
  openRateForecast: string;
  previewText: string;
  productName: string;
  productInventoryLabel: string;
  productPrice: number;
  productTone: string;
  schedule: MarketingScheduleOption[];
  followups: MarketingFollowup[];
  greeting: string;
  body: string[];
  ctaLabel: string;
  signoff: string;
  unsubscribeFooter: string;
};
