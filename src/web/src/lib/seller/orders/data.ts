// web/src/lib/seller/orders/data.ts
import type {
  Order,
  OrderDetail,
  OrderDetailFull,
  OrderInboxRow,
  OrderInboxSummary,
  OrderInboxTab,
  ShippingOption,
} from "@/lib/seller/orders/types";

const RECENT_ORDERS: Order[] = [
  {
    id: "#1042",
    customer: "Mia Chen",
    items: 2,
    total: 172,
    status: "paid",
    placedAt: "2026-04-08",
  },
  {
    id: "#1041",
    customer: "Theo Park",
    items: 1,
    total: 86,
    status: "fulfilled",
    placedAt: "2026-04-07",
  },
  {
    id: "#1040",
    customer: "Sara Novak",
    items: 3,
    total: 248,
    status: "fulfilled",
    placedAt: "2026-04-07",
  },
  {
    id: "#1039",
    customer: "Lucas Ferreira",
    items: 1,
    total: 110,
    status: "paid",
    placedAt: "2026-04-06",
  },
  {
    id: "#1038",
    customer: "Aiko Tanaka",
    items: 2,
    total: 124,
    status: "refunded",
    placedAt: "2026-04-06",
  },
];
export function getRecentOrders(): Order[] {
  return RECENT_ORDERS;
}

const ORDER_DETAIL_1001: OrderDetail = {
  id: "#1001",
  customer: "Sasha Leblanc",
  shortCustomer: "Sasha L.",
  productName: "Persimmon vase",
  productSubtitle: "Glazed terra · qty 1",
  qty: 1,
  subtotal: 86,
  shippingLabel: "USPS Ground",
  shippingCost: 0,
  customerPaid: 86,
  feePct: 4,
  fee: 3.44,
  net: 82.56,
  shipTo: {
    name: "Sasha Leblanc",
    line1: "820 Sutter St #4B",
    cityState: "SF CA 94109",
  },
  customerNote: "So excited — please pack carefully, this is for my mom.",
};
export function getOrderDetail(id: string): OrderDetail | null {
  if (id === "1001") return ORDER_DETAIL_1001;
  return null;
}

const SHIPPING_OPTIONS: ShippingOption[] = [
  {
    label: "USPS Priority · 1–3 days",
    sub: "Tracked · $50 insured",
    price: 9.84,
    selected: true,
  },
  {
    label: "USPS Ground Advantage",
    sub: "2–5 days · tracked",
    price: 6.52,
    selected: false,
  },
  {
    label: "UPS Ground",
    sub: "3–4 days · pickup avail.",
    price: 11.2,
    selected: false,
  },
];
export function getShippingOptions(): ShippingOption[] {
  return SHIPPING_OPTIONS;
}

// ── Seller management: orders inbox ──

const ORDER_INBOX: OrderInboxRow[] = [
  {
    id: "#1042",
    placedLabel: "Today · 2:14 PM",
    customer: "Sasha L.",
    city: "San Francisco, CA",
    items: "Persimmon vase, Ash budstem",
    qty: 2,
    total: 152,
    ship: "USPS Priority",
    status: "New",
    tone: "warn",
    age: "2h",
    starred: true,
  },
  {
    id: "#1041",
    placedLabel: "Today · 11:08 AM",
    customer: "Devon T.",
    city: "Brooklyn, NY",
    items: "Forest bowl, lg.",
    qty: 1,
    total: 64,
    ship: "USPS Ground",
    status: "New",
    tone: "warn",
    age: "5h",
  },
  {
    id: "#1040",
    placedLabel: "Today · 9:41 AM",
    customer: "Ari K.",
    city: "Portland, OR",
    items: "Cream tumbler set",
    qty: 1,
    total: 48,
    ship: "USPS Ground",
    status: "New",
    tone: "warn",
    age: "7h",
  },
  {
    id: "#1039",
    placedLabel: "Yesterday",
    customer: "June P.",
    city: "Seattle, WA",
    items: "Indigo carafe",
    qty: 1,
    total: 110,
    ship: "UPS Ground",
    status: "Packed",
    tone: "mute",
    age: "1d",
  },
  {
    id: "#1038",
    placedLabel: "2 days ago",
    customer: "Theo R.",
    city: "Austin, TX",
    items: "Soft hand vessel +2",
    qty: 3,
    total: 218,
    ship: "USPS Priority",
    status: "Shipped",
    tone: "mute",
    age: "2d",
  },
  {
    id: "#1037",
    placedLabel: "3 days ago",
    customer: "Liu W.",
    city: "Vancouver, BC",
    items: "Ceremony bowl",
    qty: 1,
    total: 142,
    ship: "USPS Intl",
    status: "Shipped",
    tone: "mute",
    age: "3d",
  },
  {
    id: "#1036",
    placedLabel: "4 days ago",
    customer: "Marisol G.",
    city: "Mexico City, MX",
    items: "Field cup × 4",
    qty: 4,
    total: 88,
    ship: "USPS Intl",
    status: "Refund req.",
    tone: "bad",
    age: "4d",
  },
  {
    id: "#1035",
    placedLabel: "5 days ago",
    customer: "Sam D.",
    city: "Chicago, IL",
    items: "Storm bowl",
    qty: 1,
    total: 58,
    ship: "USPS Ground",
    status: "Delivered",
    tone: "good",
    age: "5d",
  },
  {
    id: "#1034",
    placedLabel: "6 days ago",
    customer: "Hana R.",
    city: "Oakland, CA",
    items: "Earth tumbler ×2",
    qty: 2,
    total: 56,
    ship: "Local pickup",
    status: "Delivered",
    tone: "good",
    age: "6d",
  },
  {
    id: "#1033",
    placedLabel: "1 week ago",
    customer: "Paul N.",
    city: "Los Angeles, CA",
    items: "Linen vase wrap",
    qty: 1,
    total: 18,
    ship: "USPS First",
    status: "Cancelled",
    tone: "mute",
    age: "7d",
  },
];
export function getOrderInbox(): OrderInboxRow[] {
  return ORDER_INBOX;
}

const ORDER_INBOX_TABS: OrderInboxTab[] = [
  { label: "All", count: 47 },
  { label: "Needs action", count: 4, on: true },
  { label: "Packed", count: 2 },
  { label: "Shipped", count: 18 },
  { label: "Delivered", count: 21 },
  { label: "Refund / cancel", count: 2 },
];
export function getOrderInboxTabs(): OrderInboxTab[] {
  return ORDER_INBOX_TABS;
}

const ORDER_INBOX_SUMMARY: OrderInboxSummary = {
  totalLifetime: 47,
  needAction: 4,
};
export function getOrderInboxSummary(): OrderInboxSummary {
  return ORDER_INBOX_SUMMARY;
}

// ── Seller management: order detail #1042 ──

const ORDER_DETAIL_1042: OrderDetailFull = {
  id: "#1042",
  status: "Partially fulfilled",
  statusTone: "warn",
  customerShort: "Sasha L.",
  age: "2 hours ago",
  outstandingProductName: "Ash budstem",
  fulfillments: [
    {
      idx: 1,
      of: 2,
      status: "shipped",
      productName: "Persimmon vase",
      productSubtitle: "SKU PV-08 · qty 1 · $86.00",
      productTone: "clay",
      qty: 1,
      price: 86,
      tracking: "USPS · 9405 5036 9930 0124 2317",
    },
    {
      idx: 2,
      of: 2,
      status: "awaiting",
      productName: "Ash budstem",
      productSubtitle: "SKU AB-02 · qty 1 · $66.00",
      productTone: "rust",
      qty: 1,
      price: 66,
      restockNote: "back in stock Tue",
    },
  ],
  refund: {
    refundable: 152,
    itemsCount: 2,
    items: [
      {
        name: "Persimmon vase",
        qty: 1,
        price: 86,
        selected: false,
      },
      {
        name: "Ash budstem",
        qty: 1,
        price: 66,
        selected: true,
        partial: 30,
      },
    ],
    reason: "Item arrived chipped",
    restockOptions: ["Yes", "No", "Damage"],
    restockSelected: "Damage",
    total: 30,
    lastFour: "4421",
  },
  customer: {
    name: "Sasha Leblanc",
    shortName: "Sasha L",
    lifetimeOrdersLabel: "3rd order · $284 lifetime",
    ship: {
      line1: "820 Sutter St · #4B",
      line2: "San Francisco, CA 94109",
    },
    billSameAsShip: true,
    email: "sasha.l@gmail.com",
  },
  summary: {
    subtotal: 152,
    itemsCount: 2,
    shipping: 0,
    tax: 0,
    paid: 152,
    feePct: 4,
    fee: 6.08,
    labelCarrier: "USPS",
    labelCost: 9.84,
    net: 136.08,
  },
  internalNote: "Held until budstem restocks Tue. Sasha OK with split.",
  customerTags: ["VIP", "Repeat buyer", "Gift"],
  customerTagsActive: ["VIP", "Repeat buyer"],
  timeline: [
    {
      icon: "check",
      title: "Order placed",
      sub: "2 items · $152.00 paid via Visa · 4421",
      when: "2h ago",
      on: true,
    },
    {
      icon: "box",
      title: "Persimmon vase packed",
      sub: "Box S · 1lb 4oz",
      when: "1h ago",
    },
    {
      icon: "truck",
      title: "Persimmon vase shipped",
      sub: "USPS Priority · 1–3 days",
      when: "52m ago",
    },
    {
      icon: "chat",
      title: "Note from Sasha",
      sub: "“No rush on the budstem — ship together if it’s faster!”",
      when: "14m ago",
    },
    {
      icon: "info",
      title: "Ash budstem oversold",
      sub: "Restock arrives Tue · auto-fulfill on",
      when: "8m ago",
      tone: "warn",
    },
  ],
};

export function getOrderDetailFull(id: string): OrderDetailFull | null {
  if (id === "1042") return ORDER_DETAIL_1042;
  return null;
}
