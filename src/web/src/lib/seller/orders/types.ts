// web/src/lib/seller/orders/types.ts
export type OrderStatus = "paid" | "fulfilled" | "refunded" | "pending";

export type Order = {
  id: string; // includes leading "#"
  customer: string;
  items: number;
  total: number;
  status: OrderStatus;
  placedAt: string; // ISO date
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
  discountCode: string | null;
  discountAmount: number;
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
