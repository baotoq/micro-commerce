// web/src/lib/seller/data.ts
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
  Order,
  OrderDetail,
  PayoutSummary,
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

// First 9 SKUs must match the e2e expectation exactly (display order = array order)
// Status distribution: active=34, low=3, out=1, draft=4 → total=42
const LISTINGS: Listing[] = [
  {
    sku: "MC-VS-001",
    name: "Persimmon vase",
    category: "Vessels",
    price: 86,
    inventory: 24,
    status: "active",
    views7d: 412,
  },
  {
    sku: "MC-VS-002",
    name: "Shadow vase, tall",
    category: "Vessels",
    price: 124,
    inventory: 6,
    status: "active",
    views7d: 281,
  },
  {
    sku: "MC-BW-014",
    name: "Forest bowl",
    category: "Tableware",
    price: 68,
    inventory: 11,
    status: "active",
    views7d: 198,
  },
  {
    sku: "MC-TB-007",
    name: "Cream tumbler · 2 pk",
    category: "Drinkware",
    price: 54,
    inventory: 32,
    status: "active",
    views7d: 174,
  },
  {
    sku: "MC-CR-003",
    name: "Indigo carafe",
    category: "Drinkware",
    price: 110,
    inventory: 9,
    status: "active",
    views7d: 153,
  },
  {
    sku: "MC-PL-022",
    name: "Sage stoneware plate",
    category: "Tableware",
    price: 42,
    inventory: 48,
    status: "active",
    views7d: 128,
  },
  {
    sku: "MC-MG-041",
    name: "Rust mug Nº 04",
    category: "Drinkware",
    price: 28,
    inventory: 0,
    status: "out",
    views7d: 117,
  },
  {
    sku: "MC-SC-008",
    name: "Ember tea bowl",
    category: "Drinkware",
    price: 36,
    inventory: 2,
    status: "low",
    views7d: 98,
  },
  {
    sku: "MC-VS-031",
    name: "Persimmon vase, micro",
    category: "Vessels",
    price: 48,
    inventory: 14,
    status: "active",
    views7d: 84,
  },
  // 33 more listings → running totals after row 9: active=7, low=1, out=1, draft=0
  {
    sku: "MC-BW-002",
    name: "Ash bowl",
    category: "Tableware",
    price: 72,
    inventory: 18,
    status: "active",
    views7d: 76,
  },
  {
    sku: "MC-VS-009",
    name: "Ochre bud vase",
    category: "Vessels",
    price: 38,
    inventory: 22,
    status: "active",
    views7d: 71,
  },
  {
    sku: "MC-PL-005",
    name: "Stone dinner plate",
    category: "Tableware",
    price: 58,
    inventory: 30,
    status: "active",
    views7d: 68,
  },
  {
    sku: "MC-CR-011",
    name: "Clay water carafe",
    category: "Drinkware",
    price: 98,
    inventory: 7,
    status: "active",
    views7d: 64,
  },
  {
    sku: "MC-TB-019",
    name: "Smoke tumbler",
    category: "Drinkware",
    price: 44,
    inventory: 16,
    status: "active",
    views7d: 59,
  },
  {
    sku: "MC-VS-015",
    name: "Linen vase, wide",
    category: "Vessels",
    price: 92,
    inventory: 5,
    status: "active",
    views7d: 55,
  },
  {
    sku: "MC-BW-027",
    name: "River-stone bowl",
    category: "Tableware",
    price: 64,
    inventory: 12,
    status: "active",
    views7d: 51,
  },
  {
    sku: "MC-MG-013",
    name: "Charcoal mug",
    category: "Drinkware",
    price: 32,
    inventory: 40,
    status: "active",
    views7d: 48,
  },
  {
    sku: "MC-PL-033",
    name: "Oat glaze side plate",
    category: "Tableware",
    price: 34,
    inventory: 25,
    status: "active",
    views7d: 44,
  },
  {
    sku: "MC-VS-044",
    name: "Terracotta bud vase",
    category: "Vessels",
    price: 46,
    inventory: 19,
    status: "active",
    views7d: 41,
  },
  {
    sku: "MC-CR-018",
    name: "Dusk pitcher",
    category: "Drinkware",
    price: 88,
    inventory: 8,
    status: "active",
    views7d: 38,
  },
  {
    sku: "MC-TB-025",
    name: "Pebble tumbler set",
    category: "Drinkware",
    price: 78,
    inventory: 10,
    status: "active",
    views7d: 35,
  },
  {
    sku: "MC-BW-039",
    name: "Celadon salad bowl",
    category: "Tableware",
    price: 82,
    inventory: 6,
    status: "active",
    views7d: 32,
  },
  {
    sku: "MC-VS-028",
    name: "Haze vase",
    category: "Vessels",
    price: 74,
    inventory: 15,
    status: "active",
    views7d: 29,
  },
  {
    sku: "MC-MG-007",
    name: "Taupe espresso cup",
    category: "Drinkware",
    price: 24,
    inventory: 50,
    status: "active",
    views7d: 27,
  },
  {
    sku: "MC-PL-047",
    name: "Cream breakfast plate",
    category: "Tableware",
    price: 38,
    inventory: 28,
    status: "active",
    views7d: 24,
  },
  {
    sku: "MC-CR-029",
    name: "Storm carafe, small",
    category: "Drinkware",
    price: 76,
    inventory: 11,
    status: "active",
    views7d: 22,
  },
  {
    sku: "MC-VS-052",
    name: "Bone cylinder vase",
    category: "Vessels",
    price: 56,
    inventory: 20,
    status: "active",
    views7d: 19,
  },
  {
    sku: "MC-BW-051",
    name: "Peat serving bowl",
    category: "Tableware",
    price: 90,
    inventory: 4,
    status: "low",
    views7d: 17,
  },
  {
    sku: "MC-TB-038",
    name: "Mist tumbler",
    category: "Drinkware",
    price: 40,
    inventory: 3,
    status: "low",
    views7d: 15,
  },
  {
    sku: "MC-MG-022",
    name: "Warm white mug",
    category: "Drinkware",
    price: 30,
    inventory: 35,
    status: "active",
    views7d: 13,
  },
  {
    sku: "MC-PL-061",
    name: "Slate cheese board",
    category: "Tableware",
    price: 66,
    inventory: 9,
    status: "active",
    views7d: 11,
  },
  {
    sku: "MC-VS-063",
    name: "Reed bud vase",
    category: "Vessels",
    price: 42,
    inventory: 17,
    status: "active",
    views7d: 9,
  },
  {
    sku: "MC-CR-042",
    name: "Fog decanter",
    category: "Drinkware",
    price: 104,
    inventory: 6,
    status: "active",
    views7d: 8,
  },
  // draft listings (4 total)
  {
    sku: "MC-VS-071",
    name: "Blush column vase",
    category: "Vessels",
    price: 118,
    inventory: 0,
    status: "draft",
    views7d: 0,
  },
  {
    sku: "MC-BW-068",
    name: "Smoke serving platter",
    category: "Tableware",
    price: 96,
    inventory: 0,
    status: "draft",
    views7d: 0,
  },
  {
    sku: "MC-TB-055",
    name: "Graphite long-drink glass",
    category: "Drinkware",
    price: 52,
    inventory: 0,
    status: "draft",
    views7d: 0,
  },
  {
    sku: "MC-MG-059",
    name: "Pearl double-wall mug",
    category: "Drinkware",
    price: 48,
    inventory: 0,
    status: "draft",
    views7d: 0,
  },
  // active listings to reach active=34
  {
    sku: "MC-PL-072",
    name: "Birch salad plate",
    category: "Tableware",
    price: 36,
    inventory: 22,
    status: "active",
    views7d: 7,
  },
  {
    sku: "MC-VS-078",
    name: "Dune floor vase",
    category: "Vessels",
    price: 148,
    inventory: 3,
    status: "active",
    views7d: 6,
  },
  {
    sku: "MC-CR-065",
    name: "Lavender milk jug",
    category: "Drinkware",
    price: 62,
    inventory: 14,
    status: "active",
    views7d: 5,
  },
  {
    sku: "MC-MG-074",
    name: "Ink dip mug",
    category: "Drinkware",
    price: 26,
    inventory: 38,
    status: "active",
    views7d: 4,
  },
  {
    sku: "MC-BW-079",
    name: "Chalk fruit bowl",
    category: "Tableware",
    price: 84,
    inventory: 7,
    status: "active",
    views7d: 3,
  },
];

export function getListings(): Listing[] {
  return LISTINGS;
}

export function getListingCounts(): ListingCounts {
  const c = { total: LISTINGS.length, active: 0, low: 0, out: 0, draft: 0 };
  for (const l of LISTINGS) c[l.status] += 1;
  return c;
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
  { label: "Claim shop name", subject: "mira-studio · 2 days ago", done: true },
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
