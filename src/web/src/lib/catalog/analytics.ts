import "server-only";

// ── API DTO shapes (camelCase — ASP.NET web defaults) ──
// Mirror the backend records in src/Services/Catalog.API/src/Application/Analytics.

export interface KpiPointDto {
  label: string;
  value: number;
  change: number;
  isUp: boolean;
}

export interface RevenuePointDto {
  date: string;
  revenue: number;
  orders: number;
}

export interface SourceBreakdownDto {
  source: string;
  pct: number;
  revenue: number;
}

export interface FunnelStageDto {
  label: string;
  count: number;
  rate: number;
}

export interface TopProductDto {
  sku: string;
  name: string;
  units: number;
  revenue: number;
}

export interface AnalyticsOverviewDto {
  kpiCards: KpiPointDto[];
  revenueSeries: RevenuePointDto[];
  sources: SourceBreakdownDto[];
  topProducts: TopProductDto[];
  funnel: FunnelStageDto[];
}

export interface OrderInboxRowDto {
  number: number;
  placedAt: string;
  customerName: string;
  cityState: string;
  itemsSummary: string;
  qty: number;
  total: number;
  shippingMethod: string;
  status: string;
  starred: boolean;
}

export interface DashboardTodayItemDto {
  label: string;
  count: number | null;
}

export interface DashboardSummaryDto {
  dateLabel: string;
  newOrders: number;
  ordersToShip: number;
  revenue: number;
  recentReviews: number;
  todayItems: DashboardTodayItemDto[];
  recentOrders: OrderInboxRowDto[];
}

function apiBase(): string {
  const url = process.env.API_URL;
  if (!url) {
    throw new Error(
      "API_URL env var is not set. Run the app via the Aspire AppHost (`dotnet run --project src/AppHost`).",
    );
  }
  return url.replace(/\/$/, "");
}

export async function fetchAnalyticsOverview(): Promise<AnalyticsOverviewDto> {
  const res = await fetch(`${apiBase()}/api/analytics/overview`);
  if (!res.ok)
    throw new Error(`GET /api/analytics/overview failed: ${res.status}`);
  return (await res.json()) as AnalyticsOverviewDto;
}

export async function fetchDashboardSummary(): Promise<DashboardSummaryDto> {
  const res = await fetch(`${apiBase()}/api/analytics/dashboard`);
  if (!res.ok)
    throw new Error(`GET /api/analytics/dashboard failed: ${res.status}`);
  return (await res.json()) as DashboardSummaryDto;
}
