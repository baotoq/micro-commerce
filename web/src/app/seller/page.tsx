// web/src/app/seller/page.tsx

import { KpiCard } from "@/components/seller/kpi-card";
import { RecentOrders } from "@/components/seller/recent-orders";
import { RevenueChart } from "@/components/seller/revenue-chart";
import { TodayPanel } from "@/components/seller/today-panel";
import {
  BRAND,
  DATE_LABEL,
  getOverviewKpis,
  getRecentOrders,
  getRevenueSeries,
  getTodayItems,
} from "@/lib/seller/data";

export default function SellerOverview() {
  const kpis = getOverviewKpis();
  return (
    <section className="px-10 py-8">
      <header className="flex items-center justify-between">
        <div>
          <p className="text-xs uppercase tracking-wider text-[#1d1d1f]/60">
            {DATE_LABEL}
          </p>
          <h1 className="mt-1 text-3xl font-semibold tracking-tight">
            Good morning, {BRAND.owner}
          </h1>
        </div>
      </header>

      <div className="mt-8 grid grid-cols-3 gap-4">
        {kpis.map((k) => (
          <KpiCard key={k.label} kpi={k} />
        ))}
      </div>

      <div className="mt-6 grid grid-cols-3 gap-4">
        <div className="col-span-2">
          <RevenueChart points={getRevenueSeries()} />
        </div>
        <TodayPanel items={getTodayItems()} />
      </div>

      <div className="mt-6">
        <RecentOrders orders={getRecentOrders()} />
      </div>
    </section>
  );
}
