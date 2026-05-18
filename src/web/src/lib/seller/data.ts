// web/src/lib/seller/data.ts
import {
  fetchProductBySku,
  fetchProductCounts,
  fetchProducts,
} from "@/lib/catalog/api";
import type {
  Brand,
  FirstMonthKpi,
  FirstOrderKpi,
  FunnelStage,
  KpiPoint,
  LaunchTask,
  LedgerEntry,
  Listing,
  ListingCounts,
  ListingStatus,
  MarketingDraft,
  Order,
  OrderDetail,
  OrderDetailFull,
  OrderInboxRow,
  OrderInboxSummary,
  OrderInboxTab,
  PayoutSummary,
  PromoCode,
  PromoStat,
  PromoTab,
  RevenuePoint,
  SellerApplication,
  SetupStep,
  ShippingOption,
  SourceBreakdown,
  TodayItem,
  TopProduct,
  TopSeller,
} from "./types";

export const BRAND: Brand = { name: "Micro Commerce", owner: "Alex" };
export const TODAY = new Date("2026-04-08T09:00:00.000Z");
export const DATE_LABEL = "Tuesday · April 8";

const RANGE_OPTIONS = ["7d", "30d", "90d", "Year"] as const;
export function getRangeOptions(): string[] {
  return [...RANGE_OPTIONS];
}

const OVERVIEW_KPIS: KpiPoint[] = [
  { label: "Revenue · 7 days", value: 4280, format: "currency", delta: 12 },
  { label: "Orders · 7 days", value: 38, format: "number", delta: 8 },
  { label: "Storefront views", value: 2304, format: "number", delta: -3 },
];
export function getOverviewKpis(): KpiPoint[] {
  return OVERVIEW_KPIS;
}

const ANALYTICS_KPIS: KpiPoint[] = [
  { label: "Revenue", value: 12480, format: "currency", delta: 9 },
  { label: "Orders", value: 132, format: "number", delta: 6 },
  { label: "Conversion", value: 3.4, format: "percent", delta: -1 },
  { label: "Avg. order", value: 94.55, format: "currency", delta: 2 },
];
export function getAnalyticsKpis(): KpiPoint[] {
  return ANALYTICS_KPIS;
}

const REVENUE_SERIES: RevenuePoint[] = [
  { day: "Mon", amount: 520 },
  { day: "Tue", amount: 680 },
  { day: "Wed", amount: 590 },
  { day: "Thu", amount: 740 },
  { day: "Fri", amount: 820 },
  { day: "Sat", amount: 490 },
  { day: "Sun", amount: 440 },
];
export function getRevenueSeries(): RevenuePoint[] {
  return REVENUE_SERIES;
}

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

const TODAY_ITEMS: TodayItem[] = [
  { label: "Pack 2 orders ready to ship" },
  { label: "Low inventory alert: Ember tea bowl (2 left)" },
  { label: "3 new reviews to moderate" },
  { label: "Restock reminder: Rust mug Nº 04 is out of stock" },
];
export function getTodayItems(): TodayItem[] {
  return TODAY_ITEMS;
}

// Listings are persisted by the Catalog API. Dev/e2e data is seeded by the
// SeedData/products.json fixture in src/Services/Catalog.API/src/Api.
export async function getListings(
  query: { page?: number; limit?: number; status?: ListingStatus } = {},
) {
  return fetchProducts({
    page: query.page ?? 1,
    limit: query.limit ?? 9,
    status: query.status,
  });
}

export async function getListingBySku(sku: string): Promise<Listing | null> {
  return fetchProductBySku(sku);
}

export async function getListingCounts(): Promise<ListingCounts> {
  return fetchProductCounts();
}

const SOURCES: SourceBreakdown[] = [
  { name: "Organic search", visits: 2304, share: 0.52 },
  { name: "Direct", visits: 884, share: 0.2 },
  { name: "Social", visits: 619, share: 0.14 },
  { name: "Referral", visits: 442, share: 0.1 },
  { name: "Email", visits: 177, share: 0.04 },
];
export function getSources(): SourceBreakdown[] {
  return SOURCES;
}

const TOP_PRODUCTS: TopProduct[] = [
  { sku: "MC-VS-001", name: "Persimmon vase", units: 48, revenue: 4128 },
  { sku: "MC-CR-003", name: "Indigo carafe", units: 32, revenue: 3520 },
  { sku: "MC-VS-002", name: "Shadow vase, tall", units: 22, revenue: 2728 },
  { sku: "MC-BW-014", name: "Forest bowl", units: 35, revenue: 2380 },
  { sku: "MC-TB-007", name: "Cream tumbler · 2 pk", units: 41, revenue: 2214 },
  { sku: "MC-PL-022", name: "Sage stoneware plate", units: 44, revenue: 1848 },
];
export function getTopProducts(): TopProduct[] {
  return TOP_PRODUCTS;
}

const FUNNEL: FunnelStage[] = [
  { label: "Storefront views", count: 12000 },
  { label: "Product views", count: 8400 },
  { label: "Added to cart", count: 2300 },
  { label: "Checkout started", count: 1100 },
  { label: "Purchased", count: 410 },
];
export function getFunnel(): FunnelStage[] {
  return FUNNEL;
}

const APPLICATION: SellerApplication = {
  shopName: "Mira Studio",
  domain: "mira-studio.micro.shop",
  available: true,
  category: "Ceramics",
  categories: ["Ceramics", "Bakery", "Textiles", "Jewelry", "Vintage", "Other"],
  stepsLeft: 6,
};
export function getApplication(): SellerApplication {
  return APPLICATION;
}

const SETUP_STEPS: SetupStep[] = [
  { label: "Shop name", status: "done" },
  { label: "Location & payouts", status: "active" },
  { label: "Brand", status: "pending" },
  { label: "First listing", status: "pending" },
  { label: "Shipping", status: "pending" },
  { label: "Review", status: "pending" },
];
export function getSetupSteps(): SetupStep[] {
  return SETUP_STEPS;
}

const LAUNCH_CHECKLIST: LaunchTask[] = [
  { label: "Claim shop name", subject: "alex-studio · 2 days ago", done: true },
  { label: "Add payout method", subject: "Bank · ending 4421", done: true },
  {
    label: "Publish first listing",
    subject: "Persimmon vase · just now",
    done: true,
  },
  {
    label: "Add 2 more listings",
    subject: "Most shops launch with 5+",
    done: false,
    hint: "Recommended",
  },
  {
    label: "Set shipping rates",
    subject: "US · Intl · Local pickup",
    done: false,
  },
  {
    label: "Share with 3 friends",
    subject: "Average shop gets first sale in 4 days",
    done: false,
  },
];
export function getLaunchChecklist(): LaunchTask[] {
  return LAUNCH_CHECKLIST;
}

const DAY_ONE_STATS: { label: string; value: string; subject: string }[] = [
  { label: "Sales · today", value: "$0.00", subject: "no activity yet" },
  { label: "Orders · today", value: "0", subject: "no activity yet" },
  { label: "Visits · today", value: "14", subject: "mostly you :)" },
];
export function getDayOneStats(): {
  label: string;
  value: string;
  subject: string;
}[] {
  return DAY_ONE_STATS;
}

const FIRST_ORDER_STATS: FirstOrderKpi[] = [
  {
    label: "Sales · today",
    value: "$86",
    delta: "first sale!",
    spark: [0, 0, 0, 0, 0, 0, 1],
  },
  {
    label: "Orders · today",
    value: "1",
    delta: "new",
    spark: [0, 0, 0, 0, 0, 0, 1],
  },
  {
    label: "Visits · today",
    value: "142",
    delta: "8× yesterday",
    spark: [0.1, 0.15, 0.1, 0.2, 0.3, 0.6, 0.95],
  },
  {
    label: "Followers",
    value: "14",
    delta: "+5 today",
    spark: [0.1, 0.2, 0.2, 0.4, 0.45, 0.7, 0.95],
  },
];
export function getFirstOrderStats(): FirstOrderKpi[] {
  return FIRST_ORDER_STATS;
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

const FIRST_MONTH_KPIS: FirstMonthKpi[] = [
  {
    label: "Revenue",
    value: "$2,148",
    delta: "+ first month",
    spark: [0.05, 0.1, 0.2, 0.25, 0.4, 0.55, 0.7, 0.85],
  },
  {
    label: "Orders",
    value: "23",
    delta: "avg $93.39",
    spark: [0.05, 0.1, 0.2, 0.3, 0.45, 0.6, 0.7, 0.9],
  },
  {
    label: "Conversion",
    value: "3.1%",
    delta: "vs 2.4% benchmark",
    spark: [0.3, 0.4, 0.5, 0.55, 0.6, 0.7, 0.75, 0.85],
  },
  {
    label: "Repeat buyers",
    value: "4",
    delta: "17% of orders",
    spark: [0, 0, 0.1, 0.2, 0.3, 0.4, 0.5, 0.7],
  },
];
export function getFirstMonthKpis(): FirstMonthKpi[] {
  return FIRST_MONTH_KPIS;
}

const FIRST_MONTH_SERIES: number[] = [
  0, 0, 0, 86, 0, 0, 0, 152, 78, 110, 0, 64, 230, 86, 110, 0, 178, 220, 64, 152,
  86, 320, 110, 152, 64, 86, 230, 110, 320, 86,
];
export function getFirstMonthSeries(): number[] {
  return FIRST_MONTH_SERIES;
}

const FIRST_MONTH_TOP_SELLERS: TopSeller[] = [
  {
    sku: "MC-VS-001",
    name: "Persimmon vase",
    soldLabel: "14 sold",
    revenue: 1204,
    tone: "clay",
  },
  {
    sku: "MC-BW-014",
    name: "Forest bowl, lg.",
    soldLabel: "6 sold",
    revenue: 384,
    tone: "sage",
  },
  {
    sku: "MC-TB-007",
    name: "Cream tumbler set",
    soldLabel: "4 sold",
    revenue: 192,
    tone: "cream",
  },
];
export function getFirstMonthTopSellers(): TopSeller[] {
  return FIRST_MONTH_TOP_SELLERS;
}

const LEDGER_ENTRIES: LedgerEntry[] = [
  {
    date: "Apr 8",
    label: "Payout · weekly",
    subject: "Sent · ending 4421",
    amount: 1284.62,
    type: "payout",
  },
  {
    date: "Apr 7",
    label: "Order #1042 · Sasha L.",
    subject: "Net of $3.44 fee",
    amount: 82.56,
    type: "sale",
  },
  {
    date: "Apr 7",
    label: "Order #1041 · Devon T.",
    subject: "Net of $2.56 fee",
    amount: 61.44,
    type: "sale",
  },
  {
    date: "Apr 6",
    label: "Shipping label · USPS",
    subject: "#1041 · 1lb 4oz",
    amount: -6.52,
    type: "fee",
  },
  {
    date: "Apr 5",
    label: "Order #1040 · Ari K.",
    subject: "Net of $1.92 fee",
    amount: 46.08,
    type: "sale",
  },
  {
    date: "Apr 1",
    label: "Payout · weekly",
    subject: "Sent · ending 4421",
    amount: 624.18,
    type: "payout",
  },
];
export function getLedgerEntries(): LedgerEntry[] {
  return LEDGER_ENTRIES;
}

const PAYOUT_SUMMARY: PayoutSummary = {
  lastPayout: 1284.62,
  lastPayoutSentLabel: "Sent · arriving Wed",
  available: 184.08,
  availableSendsOn: "Tue, Apr 15",
  availableFromOrders: 3,
  lifetime: 2148.36,
  lifetimeOrders: 23,
  lifetimeRange: "Mar 12 → Apr 11",
};
export function getPayoutSummary(): PayoutSummary {
  return PAYOUT_SUMMARY;
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

// ── Seller management: promos ──

const PROMOS: PromoCode[] = [
  {
    code: "SPRING20",
    what: "20% off · sitewide",
    window: "Apr 1 → May 15",
    redemptions: 142,
    drivenRevenue: 1842,
    status: "Active",
    tone: "good",
  },
  {
    code: "WELCOME10",
    what: "$10 off · first order $40+",
    window: "Always · 1 per buyer",
    redemptions: 38,
    drivenRevenue: 612,
    status: "Active",
    tone: "good",
  },
  {
    code: "STUDIO15",
    what: "15% off · followers only",
    window: "Apr 22 → May 06",
    redemptions: 17,
    drivenRevenue: 286,
    status: "Active",
    tone: "good",
    highlight: true,
  },
  {
    code: "BLOOM",
    what: "Free ship · $80+",
    window: "Mar 1 → Apr 12",
    redemptions: 84,
    drivenRevenue: 0,
    status: "Ended",
    tone: "mute",
  },
  {
    code: "FRIENDS",
    what: "15% off · sitewide",
    window: "Drafted",
    redemptions: 0,
    drivenRevenue: 0,
    status: "Draft",
    tone: "mute",
  },
];
export function getPromos(): PromoCode[] {
  return PROMOS;
}

const PROMO_STATS: PromoStat[] = [
  {
    label: "Driven revenue",
    value: "$2,740",
    sub: "Last 30 days",
    spark: [0.1, 0.2, 0.3, 0.45, 0.6, 0.55, 0.7, 0.85],
  },
  {
    label: "Redemptions",
    value: "281",
    sub: "14% of orders",
    spark: [0.2, 0.25, 0.4, 0.45, 0.55, 0.6, 0.7, 0.8],
  },
  {
    label: "Avg. discount",
    value: "$9.74",
    sub: "per redemption",
    spark: [0.3, 0.32, 0.36, 0.4, 0.42, 0.45, 0.5, 0.52],
  },
  {
    label: "New buyers",
    value: "38",
    sub: "from WELCOME10",
    spark: [0, 0.1, 0.15, 0.2, 0.3, 0.4, 0.5, 0.7],
  },
];
export function getPromoStats(): PromoStat[] {
  return PROMO_STATS;
}

const PROMO_TABS: PromoTab[] = [
  { label: "Promotions", count: 5, on: true },
  { label: "Automatic", count: 1 },
  { label: "Gift cards", count: 0 },
];
export function getPromoTabs(): PromoTab[] {
  return PROMO_TABS;
}

// ── Seller management: marketing draft ──
// Brand-swapped from the design's "Mira Studio" / "Mira" to BRAND.name / BRAND.owner
// per CLAUDE.md (Mira may only appear on /seller/apply).

const MARKETING_DRAFT: MarketingDraft = {
  senderName: BRAND.name,
  senderInitial: BRAND.name.charAt(0),
  recipientCount: 184,
  deliverable: "184 buyers · 96% deliverable",
  audiences: [
    {
      label: "Buyers · last 30 days",
      sub: "47 buyers · avg $74 spend",
      count: 47,
      on: true,
    },
    {
      label: "Repeat buyers",
      sub: "23 buyers · 2+ orders",
      count: 23,
      on: true,
    },
    {
      label: "Followers without an order",
      sub: "114 followers",
      count: 114,
      on: true,
    },
    {
      label: "All-time buyers",
      sub: "142 buyers · since Mar",
      count: 142,
      on: false,
    },
  ],
  templates: [
    { label: "Restock", on: true },
    { label: "New drop", on: false },
    { label: "Behind the scenes", on: false },
    { label: "Discount code", on: false },
    { label: "Plain text", on: false },
  ],
  subject: "The persimmon vase is back · just 8 this batch",
  subjectCharCount: 52,
  subjectMaxChars: 80,
  openRateForecast: "Open-rate forecast: 32% (above your avg)",
  previewText: "A small restock — three glaze variations this round.",
  productName: "Persimmon vase",
  productInventoryLabel: "8 in stock · $86",
  productPrice: 86,
  productTone: "clay",
  schedule: [
    { label: "Now", sub: "Sends within 5 min", on: false },
    {
      label: "Best time · Thu 6 PM",
      sub: "Highest opens for your list",
      on: true,
    },
    { label: "Pick a time", sub: "— select date & time —", on: false },
  ],
  followups: [
    { label: "Re-send to non-openers · 3 days later", on: true },
    { label: "Auto-pause if > 0.5% spam complaint", on: false },
  ],
  greeting: "Hi Sasha,",
  body: [
    `Pulled eight persimmon vases out of the kiln Sunday — the warm batch, with the soft asymmetry on the rim you asked about last time.`,
    `They tend to go in a day. If you'd like one, the link is below — followers get $5 off through Friday.`,
  ],
  ctaLabel: "Shop the restock →",
  signoff: `— ${BRAND.owner}`,
  unsubscribeFooter: `You're getting this because you bought from ${BRAND.name}`,
};
export function getMarketingDraft(): MarketingDraft {
  return MARKETING_DRAFT;
}
