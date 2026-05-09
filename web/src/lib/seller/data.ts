// web/src/lib/seller/data.ts
import type {
  Brand,
  FunnelStage,
  KpiPoint,
  Listing,
  ListingCounts,
  Order,
  RevenuePoint,
  SourceBreakdown,
  TodayItem,
  TopProduct,
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
