// web/src/app/seller/analytics/page.tsx

import { AnalyticsKpiRow } from "@/components/seller/analytics-kpi-row";
import { ConversionFunnel } from "@/components/seller/conversion-funnel";
import { RangeTabs } from "@/components/seller/range-tabs";
import { SourcesDonut } from "@/components/seller/sources-donut";
import { TopProducts } from "@/components/seller/top-products";
import { Button } from "@/components/ui/button";
import {
  getAnalyticsKpis,
  getFunnel,
  getRangeOptions,
  getSources,
  getTopProducts,
} from "@/lib/seller/data";

export default function AnalyticsPage() {
  return (
    <section className="px-10 py-8">
      <header className="flex flex-wrap items-end justify-between gap-4">
        <div>
          <h1 className="text-3xl font-semibold tracking-tight">Analytics</h1>
          <p className="mt-1 text-sm text-[#1d1d1f]/70">
            Apr 1 – Apr 30 · vs Mar 1 – Mar 30
          </p>
        </div>
        <div className="flex items-center gap-3">
          <RangeTabs options={getRangeOptions()} defaultValue="30d" />
          <Button variant="outline" className="rounded-full">
            Export
          </Button>
        </div>
      </header>

      <div className="mt-8">
        <AnalyticsKpiRow kpis={getAnalyticsKpis()} />
      </div>

      <div className="mt-6 grid gap-4 md:grid-cols-2">
        <SourcesDonut sources={getSources()} />
        <TopProducts products={getTopProducts()} />
      </div>

      <div className="mt-6">
        <ConversionFunnel stages={getFunnel()} />
      </div>
    </section>
  );
}
