// web/src/lib/seller/analytics/data.ts
//
// AUTH RULE (Keycloak): these loaders are cached (`"use cache"`) and therefore
// anonymous — calling auth()/getAccessToken() inside `"use cache"` is illegal.
// A loader is either cached+anonymous or uncached+authenticated, never both.
// The underlying read endpoints are public; authenticated writes live in the
// uncached fetchers under lib/catalog/*.ts.
import { cacheTag } from "next/cache";
import {
  type AnalyticsOverviewDto,
  fetchAnalyticsOverview,
  type KpiPointDto,
} from "@/lib/catalog/analytics";
import type {
  FunnelStage,
  KpiFormat,
  KpiPoint,
  RevenuePoint,
  SourceBreakdown,
  TopProduct,
} from "@/lib/seller/analytics/types";

const RANGE_OPTIONS = ["7d", "30d", "90d", "Year"] as const;
export function getRangeOptions(): string[] {
  return [...RANGE_OPTIONS];
}

/** Signed KPI-change label, e.g. "+12.4%" / "-3.1%". */
export function formatKpiChange(change: number, isUp: boolean): string {
  return `${isUp ? "+" : "-"}${Math.abs(change)}%`;
}

/** Infer the display format from the KPI label keywords. */
function inferKpiFormat(label: string): KpiFormat {
  const lower = label.toLowerCase();
  if (lower.includes("conversion") || lower.includes("%")) return "percent";
  if (
    lower.includes("orders") ||
    lower.includes("views") ||
    lower.includes("visits") ||
    lower.includes("customers")
  ) {
    return "number";
  }
  if (
    lower.includes("revenue") ||
    lower.includes("order") || // "avg. order", "avg order"
    lower.includes("aov")
  ) {
    return "currency";
  }
  return "number";
}

function dtoToKpiPoint(dto: KpiPointDto): KpiPoint {
  return {
    label: dto.label,
    value: dto.value,
    format: inferKpiFormat(dto.label),
    delta: dto.isUp ? dto.change : -dto.change,
  };
}

async function overview(): Promise<AnalyticsOverviewDto> {
  return fetchAnalyticsOverview();
}

export async function getAnalyticsKpis(): Promise<KpiPoint[]> {
  "use cache";
  cacheTag("analytics");
  const data = await overview();
  return data.kpiCards.map(dtoToKpiPoint);
}

export async function getOverviewKpis(): Promise<KpiPoint[]> {
  "use cache";
  cacheTag("analytics");
  const data = await overview();
  return data.kpiCards.slice(0, 3).map(dtoToKpiPoint);
}

export async function getRevenueSeries(): Promise<RevenuePoint[]> {
  "use cache";
  cacheTag("analytics");
  const data = await overview();
  return data.revenueSeries.map((p) => ({ day: p.date, amount: p.revenue }));
}

export async function getSources(): Promise<SourceBreakdown[]> {
  "use cache";
  cacheTag("analytics");
  const data = await overview();
  return data.sources.map((s) => ({
    name: s.source,
    visits: s.revenue,
    share: s.pct / 100,
  }));
}

export async function getTopProducts(): Promise<TopProduct[]> {
  "use cache";
  cacheTag("analytics");
  const data = await overview();
  return data.topProducts.map((p) => ({
    sku: p.sku,
    name: p.name,
    units: p.units,
    revenue: p.revenue,
  }));
}

export async function getFunnel(): Promise<FunnelStage[]> {
  "use cache";
  cacheTag("analytics");
  const data = await overview();
  return data.funnel.map((s) => ({ label: s.label, count: s.count }));
}
