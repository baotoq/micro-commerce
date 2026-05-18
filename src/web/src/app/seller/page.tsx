// web/src/app/seller/page.tsx

import Link from "next/link";
import { KpiCard } from "@/components/seller/analytics/kpi-card";
import { RevenueChart } from "@/components/seller/analytics/revenue-chart";
import { TodayPanel } from "@/components/seller/dashboard/today-panel";
import { RecentOrders } from "@/components/seller/orders/recent-orders";
import { getOverviewKpis, getRevenueSeries } from "@/lib/seller/analytics/data";
import { BRAND, DATE_LABEL } from "@/lib/seller/brand";
import { getTodayItems } from "@/lib/seller/dashboard/data";
import { getRecentOrders } from "@/lib/seller/orders/data";

export default function SellerOverview() {
  const kpis = getOverviewKpis();
  return (
    <section className="px-10 py-8">
      <header className="flex items-center justify-between">
        <div>
          <p className="text-xs uppercase tracking-wider text-foreground/60">
            {DATE_LABEL}
          </p>
          <h1 className="mt-1 text-3xl font-semibold tracking-tight">
            Good morning, {BRAND.owner}
          </h1>
        </div>
        <div className="flex items-center gap-2">
          <button
            type="button"
            className="rounded-full border border-black/[0.12] px-4 py-2 text-sm font-medium text-foreground hover:bg-black/[0.04]"
          >
            Export
          </button>
          <Link
            href="/seller/listings/new"
            className="rounded-full bg-primary px-4 py-2 text-sm font-medium text-white hover:bg-primary/90"
          >
            + New listing
          </Link>
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
