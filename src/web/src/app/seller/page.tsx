// web/src/app/seller/page.tsx

import Link from "next/link";
import { connection } from "next/server";
import { KpiCard } from "@/components/seller/analytics/kpi-card";
import { RevenueChart } from "@/components/seller/analytics/revenue-chart";
import { TodayPanel } from "@/components/seller/dashboard/today-panel";
import { RecentOrders } from "@/components/seller/orders/recent-orders";
import { Button, buttonVariants } from "@/components/ui/button";
import { getOverviewKpis, getRevenueSeries } from "@/lib/seller/analytics/data";
import { BRAND } from "@/lib/seller/brand";
import {
  getDashboardRecentOrders,
  getDateLabel,
  getTodayItems,
} from "@/lib/seller/dashboard/data";
import { cn } from "@/lib/utils";

export default async function SellerOverview() {
  await connection();
  const [kpis, revenueSeries, todayItems, recentOrders, dateLabel] =
    await Promise.all([
      getOverviewKpis(),
      getRevenueSeries(),
      getTodayItems(),
      getDashboardRecentOrders(),
      getDateLabel(),
    ]);
  return (
    <section className="px-10 py-8">
      <header className="flex items-center justify-between">
        <div>
          <p className="text-xs uppercase tracking-wider text-foreground/60">
            {dateLabel}
          </p>
          <h1 className="mt-1 text-3xl font-semibold tracking-tight">
            Good morning, {BRAND.owner}
          </h1>
        </div>
        <div className="flex items-center gap-2">
          <Button variant="outline" className="rounded-full">
            Export
          </Button>
          <Link
            href="/seller/listings/new"
            className={cn(buttonVariants(), "rounded-full")}
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
          <RevenueChart points={revenueSeries} />
        </div>
        <TodayPanel items={todayItems} />
      </div>

      <div className="mt-6">
        <RecentOrders orders={recentOrders} />
      </div>
    </section>
  );
}
