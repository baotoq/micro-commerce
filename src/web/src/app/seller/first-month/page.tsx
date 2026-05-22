import { Sparkline } from "@/components/seller/shared/sparkline";
import { SellerTopbar } from "@/components/seller/shell/seller-topbar";
import {
  getFirstMonthKpis,
  getFirstMonthSeries,
  getFirstMonthTopSellers,
} from "@/lib/seller/onboarding/data";

const TONE_COLORS: Record<string, string> = {
  clay: "#c2410c",
  sage: "#4d7c3f",
  cream: "#d4b896",
};

function formatMoney(n: number) {
  return new Intl.NumberFormat("en-US", {
    style: "currency",
    currency: "USD",
    minimumFractionDigits: 2,
  }).format(n);
}

function AreaChart({ data }: { data: number[] }) {
  const w = 100;
  const h = 100;
  const max = Math.max(...data, 0.0001);
  const step = data.length > 1 ? w / (data.length - 1) : w;
  const path = data
    .map(
      (v, i) =>
        `${i === 0 ? "M" : "L"}${(i * step).toFixed(2)},${(h - (v / max) * h).toFixed(2)}`,
    )
    .join(" ");
  const area = `${path} L${w},${h} L0,${h} Z`;
  return (
    <svg
      viewBox={`0 0 ${w} ${h}`}
      preserveAspectRatio="none"
      className="h-full w-full"
      role="img"
      aria-label="Daily revenue chart"
    >
      <path
        d={area}
        fill="color-mix(in oklch, var(--terra) 10%, transparent)"
      />
      <path d={path} fill="none" stroke="var(--terra)" strokeWidth="1.5" />
    </svg>
  );
}

export default function FirstMonthPage() {
  const kpis = getFirstMonthKpis();
  const series = getFirstMonthSeries();
  const topSellers = getFirstMonthTopSellers();

  return (
    <div className="flex flex-col">
      <SellerTopbar
        title="Your first month"
        subtitle="Analytics · March 12 → April 11"
        actions={
          <>
            <button
              type="button"
              className="rounded-full border border-black/20 px-3.5 py-1.5 text-[13px] font-medium text-foreground hover:bg-black/5"
              style={{ background: "transparent", cursor: "pointer" }}
            >
              Compare
            </button>
            <button
              type="button"
              className="rounded-full border border-black/20 px-3.5 py-1.5 text-[13px] font-medium text-foreground hover:bg-black/5"
              style={{ background: "transparent", cursor: "pointer" }}
            >
              Export
            </button>
          </>
        }
      />

      <div className="overflow-auto p-7">
        {/* 4-up KPI row */}
        <div
          className="mb-5 grid gap-3"
          style={{ gridTemplateColumns: "repeat(4, 1fr)" }}
        >
          {kpis.map((kpi) => (
            <div
              key={kpi.label}
              className="rounded-xl border border-black/[0.06] bg-white p-4"
            >
              <p className="mb-1.5 text-[11px] text-muted-foreground">
                {kpi.label}
              </p>
              <div
                className="font-semibold tabular-nums"
                style={{ fontSize: 28, lineHeight: 1 }}
              >
                {kpi.value}
              </div>
              <p className="mt-1 text-[11px] font-semibold text-good">
                ↑ {kpi.delta}
              </p>
              <div className="mt-2 h-8">
                <Sparkline
                  points={kpi.spark}
                  label={`${kpi.label} trend`}
                  className="h-8 w-full text-muted-foreground"
                />
              </div>
            </div>
          ))}
        </div>

        {/* 1.6/1 grid */}
        <div
          className="grid gap-3"
          style={{ gridTemplateColumns: "1.6fr 1fr" }}
        >
          {/* Daily revenue card */}
          <div className="rounded-xl border border-black/[0.06] bg-white p-5">
            <div className="mb-3.5 flex items-start justify-between">
              <div>
                <p className="text-[11px] text-muted-foreground">
                  Daily revenue
                </p>
                <div
                  className="mt-0.5 font-semibold tabular-nums"
                  style={{ fontSize: 24 }}
                >
                  {formatMoney(2148)}
                </div>
              </div>
              <div className="flex gap-1">
                {["Day", "Week", "Month"].map((p) => (
                  <span
                    key={p}
                    className={`rounded-full px-2.5 py-1 text-[11px] font-medium ${
                      p === "Month"
                        ? "bg-foreground text-background"
                        : "border border-black/10 bg-muted text-foreground/60"
                    }`}
                  >
                    {p}
                  </span>
                ))}
              </div>
            </div>

            {/* Chart with annotation */}
            <div className="relative" style={{ height: 220 }}>
              <AreaChart data={series} />
              {/* Annotation pin */}
              <div
                className="absolute rounded"
                style={{
                  left: "11%",
                  top: 16,
                  background: "var(--foreground)",
                  color: "white",
                  fontSize: 11,
                  padding: "6px 10px",
                  borderRadius: 6,
                  fontFamily: "var(--font-geist-sans, sans-serif)",
                }}
              >
                Day 4 · first sale
                <div
                  style={{
                    position: "absolute",
                    left: 14,
                    bottom: -4,
                    width: 8,
                    height: 8,
                    background: "var(--foreground)",
                    transform: "rotate(45deg)",
                  }}
                />
              </div>
            </div>

            {/* Date axis */}
            <div className="mt-2 flex justify-between text-[11px] text-muted-foreground">
              <span>Mar 12</span>
              <span>Mar 19</span>
              <span>Mar 26</span>
              <span>Apr 2</span>
              <span>Apr 11</span>
            </div>
          </div>

          {/* Right column */}
          <div className="flex flex-col gap-3">
            {/* Insight card */}
            <div className="rounded-xl border border-black/[0.06] bg-white p-5">
              <p
                className="mb-2 text-[10px] font-semibold uppercase tracking-wider"
                style={{ color: "#0066cc" }}
              >
                ★ Insight
              </p>
              <h3
                className="mb-1.5 font-semibold leading-snug text-foreground"
                style={{ fontSize: 15 }}
              >
                Friday afternoons sell 2.3× more than the rest of the week.
              </h3>
              <p className="text-[13px] text-muted-foreground">
                Try posting new listings Thursday evening — they tend to land in
                feeds before the Friday rush.
              </p>
            </div>

            {/* Top sellers card */}
            <div className="rounded-xl border border-black/[0.06] bg-white p-5">
              <h4 className="mb-3 font-semibold text-foreground">
                Top sellers
              </h4>
              <div className="flex flex-col gap-3">
                {topSellers.map((seller) => (
                  <div key={seller.sku} className="flex items-center gap-2">
                    <div
                      className="size-9 shrink-0 rounded-md"
                      style={{
                        background: TONE_COLORS[seller.tone] ?? "#e0e0e0",
                        opacity: 0.7,
                      }}
                    />
                    <div className="flex-1 min-w-0">
                      <p className="text-[13px] font-semibold text-foreground truncate">
                        {seller.name}
                      </p>
                      <p className="text-[11px] text-muted-foreground">
                        {seller.soldLabel}
                      </p>
                    </div>
                    <span className="text-[13px] font-medium tabular-nums text-foreground">
                      {formatMoney(seller.revenue)}
                    </span>
                  </div>
                ))}
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}
