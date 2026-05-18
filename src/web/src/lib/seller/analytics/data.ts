// web/src/lib/seller/analytics/data.ts
import type {
  FunnelStage,
  KpiPoint,
  RevenuePoint,
  SourceBreakdown,
  TopProduct,
} from "@/lib/seller/analytics/types";

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
