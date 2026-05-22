import { AnalyticsKpiRow } from "@/components/seller/analytics/analytics-kpi-row";
import { ConversionFunnel } from "@/components/seller/analytics/conversion-funnel";
import { SourcesDonut } from "@/components/seller/analytics/sources-donut";
import { TopProducts } from "@/components/seller/analytics/top-products";
import { SellerTopbar } from "@/components/seller/shell/seller-topbar";
import { Button } from "@/components/ui/button";
import {
  getAnalyticsKpis,
  getFunnel,
  getRangeOptions,
  getSources,
  getTopProducts,
} from "@/lib/seller/analytics/data";

export default function AnalyticsPage() {
  const rangeOptions = getRangeOptions();
  return (
    <div className="flex flex-col min-h-screen">
      <SellerTopbar
        title="Analytics"
        subtitle="Apr 1 – Apr 30 · vs Mar 1 – Mar 30"
        actions={
          <>
            <div className="flex gap-1 rounded-lg bg-muted p-0.5">
              {rangeOptions.map((opt, i) => (
                <button
                  key={opt}
                  type="button"
                  className={`rounded-md px-2.5 py-1 text-xs font-medium ${i === 1 ? "bg-white shadow-sm" : "bg-transparent"}`}
                >
                  {opt}
                </button>
              ))}
            </div>
            <Button variant="outline" size="sm">
              <svg
                aria-hidden="true"
                width="12"
                height="12"
                viewBox="0 0 24 24"
                fill="none"
                stroke="currentColor"
                strokeWidth="2"
                strokeLinecap="round"
                strokeLinejoin="round"
              >
                <path d="M21 15v4a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2v-4" />
                <polyline points="7 10 12 15 17 10" />
                <line x1="12" y1="15" x2="12" y2="3" />
              </svg>
              Export
            </Button>
          </>
        }
      />

      <div className="flex-1 overflow-auto p-7">
        {/* KPI row */}
        <AnalyticsKpiRow kpis={getAnalyticsKpis()} />

        {/* Revenue chart + Sources (2fr 1fr) */}
        <div
          className="mt-5 grid gap-4"
          style={{ gridTemplateColumns: "2fr 1fr" }}
        >
          {/* Revenue over time */}
          <div className="rounded-lg border border-black/[0.06] bg-white p-5">
            <div className="mb-3.5 flex items-start justify-between">
              <div>
                <h3 className="text-[15px] font-semibold text-foreground">
                  Revenue over time
                </h3>
                <p className="mt-0.5 text-[11px] text-muted-foreground">
                  Daily, by source
                </p>
              </div>
              <div className="flex gap-3 text-[11px] text-muted-foreground">
                <span className="flex items-center gap-1">
                  <span className="inline-block size-2 rounded-full bg-foreground" />
                  Organic
                </span>
                <span className="flex items-center gap-1">
                  <span className="inline-block size-2 rounded-full bg-terra" />
                  Social
                </span>
                <span className="flex items-center gap-1">
                  <span className="inline-block size-2 rounded-full bg-foreground/30" />
                  Direct
                </span>
              </div>
            </div>
            {/* Area chart placeholder */}
            <div
              className="w-full rounded-lg bg-canvas-parchment"
              style={{ height: 220 }}
              aria-label="Revenue over time chart"
              role="img"
            />
            <div className="mt-2 flex justify-between text-[11px] text-muted-foreground">
              <span>Apr 1</span>
              <span>Apr 8</span>
              <span>Apr 15</span>
              <span>Apr 22</span>
              <span>Apr 30</span>
            </div>
          </div>

          <SourcesDonut sources={getSources()} />
        </div>

        {/* Top products + Conversion funnel (1fr 1fr) */}
        <div className="mt-5 grid grid-cols-2 gap-4">
          <TopProducts products={getTopProducts()} />
          <ConversionFunnel stages={getFunnel()} />
        </div>
      </div>
    </div>
  );
}
