import { Icon } from "@/components/icon";
import { AreaChart, Donut, ProductImage, Spark } from "@/components/primitives";
import { SellerSidebar, SellerTopbar } from "@/components/seller-chrome";
import { money } from "@/lib/money";

type Trend = "up" | "down";

function Stat({
  label,
  value,
  delta,
  trend = "up",
  spark,
  sparkColor,
}: {
  label: string;
  value: string;
  delta: string;
  trend?: Trend;
  spark: number[];
  sparkColor?: string;
}) {
  return (
    <div className="hf-card" style={{ padding: 18 }}>
      <div className="hf-tiny hf-muted" style={{ marginBottom: 6 }}>
        {label}
      </div>
      <div
        className="hf-display-2 hf-num"
        style={{ fontSize: 28, lineHeight: 1 }}
      >
        {value}
      </div>
      <div
        className="hf-flex hf-items-center hf-gap-2"
        style={{ marginTop: 6 }}
      >
        <span
          className="hf-tiny"
          style={{
            color: trend === "up" ? "var(--good)" : "var(--bad)",
            fontWeight: 600,
          }}
        >
          {trend === "up" ? "↑" : "↓"} {delta}
        </span>
        <span className="hf-tiny hf-muted">vs prev. period</span>
      </div>
      <div
        style={{
          height: 28,
          marginTop: 10,
          color: sparkColor ?? "var(--ink-3)",
        }}
      >
        <Spark
          data={spark}
          color="currentColor"
          fill="rgba(21,18,14,0.05)"
          sw={1.5}
        />
      </div>
    </div>
  );
}

const sources = [
  { label: "Organic search", pct: "48%", count: "2,304", color: "var(--ink)" },
  { label: "Social", pct: "28%", count: "1,344", color: "var(--terra)" },
  { label: "Direct", pct: "14%", count: "672", color: "var(--forest)" },
  { label: "Email", pct: "10%", count: "480", color: "var(--ink-4)" },
];

const topProducts = [
  {
    title: "Persimmon vase",
    tone: "clay" as const,
    units: 14,
    revenue: 1204,
    share: 0.85,
  },
  {
    title: "Cream tumbler set",
    tone: "bone" as const,
    units: 12,
    revenue: 576,
    share: 0.62,
  },
  {
    title: "Forest bowl, lg.",
    tone: "sage" as const,
    units: 9,
    revenue: 576,
    share: 0.58,
  },
  {
    title: "Bone dinner plate",
    tone: "cream" as const,
    units: 8,
    revenue: 304,
    share: 0.46,
  },
];

const funnel = [
  { label: "Storefront views", value: 4830, pct: 1.0 },
  { label: "Product views", value: 2104, pct: 0.44 },
  { label: "Added to cart", value: 412, pct: 0.085 },
  { label: "Checkout started", value: 218, pct: 0.045 },
  { label: "Purchased", value: 124, pct: 0.026 },
];

const ranges = ["7d", "30d", "90d", "Year"] as const;
const ACTIVE_RANGE = "30d";

export default function SellerAnalyticsPage() {
  return (
    <div
      className="hf hf-desktop"
      style={{ flexDirection: "row", minHeight: "100vh" }}
    >
      <SellerSidebar active="Analytics" />
      <div className="hf-grow hf-col" style={{ overflow: "hidden" }}>
        <SellerTopbar
          title="Analytics"
          subtitle="Apr 1 – Apr 30 · vs Mar 1 – Mar 30"
          actions={
            <>
              <div
                className="hf-flex hf-gap-1"
                style={{
                  background: "var(--paper-2)",
                  padding: 3,
                  borderRadius: 8,
                }}
              >
                {ranges.map((r) => (
                  <button
                    key={r}
                    type="button"
                    className="hf-btn hf-btn-sm"
                    style={{
                      background:
                        r === ACTIVE_RANGE ? "var(--card)" : "transparent",
                      boxShadow:
                        r === ACTIVE_RANGE ? "var(--shadow-1)" : "none",
                    }}
                  >
                    {r}
                  </button>
                ))}
              </div>
              <button className="hf-btn hf-btn-outline" type="button">
                <Icon n="dl" s={12} /> Export
              </button>
            </>
          }
        />

        <div
          className="hf-grow"
          style={{ overflow: "auto", padding: "24px 28px" }}
        >
          {/* KPI row */}
          <div
            className="hf-grid hf-gap-3"
            style={{
              gridTemplateColumns: "repeat(4, 1fr)",
              marginBottom: 20,
            }}
          >
            <Stat
              label="Revenue"
              value={money(12480)}
              delta="14%"
              spark={[0.3, 0.4, 0.45, 0.55, 0.5, 0.7, 0.65, 0.85]}
            />
            <Stat
              label="Orders"
              value="124"
              delta="9%"
              spark={[0.4, 0.5, 0.45, 0.6, 0.55, 0.7, 0.75, 0.8]}
            />
            <Stat
              label="Conversion"
              value="3.4%"
              delta="0.6 pp"
              spark={[0.5, 0.55, 0.5, 0.6, 0.7, 0.65, 0.75, 0.8]}
            />
            <Stat
              label="Avg. order"
              value={money(112)}
              delta="3%"
              trend="down"
              sparkColor="var(--bad)"
              spark={[0.7, 0.65, 0.6, 0.55, 0.5, 0.55, 0.6, 0.5]}
            />
          </div>

          {/* Revenue chart + sources donut */}
          <div
            className="hf-grid hf-gap-3"
            style={{ gridTemplateColumns: "2fr 1fr" }}
          >
            <div className="hf-card" style={{ padding: 20 }}>
              <div className="hf-flex hf-between" style={{ marginBottom: 14 }}>
                <div>
                  <div className="hf-h3">Revenue over time</div>
                  <div className="hf-tiny hf-muted" style={{ marginTop: 2 }}>
                    Daily, by source
                  </div>
                </div>
                <div className="hf-flex hf-gap-3 hf-tiny">
                  <span className="hf-flex hf-items-center hf-gap-1">
                    <span
                      className="hf-dot"
                      style={{ background: "var(--ink)" }}
                    />{" "}
                    Organic
                  </span>
                  <span className="hf-flex hf-items-center hf-gap-1">
                    <span
                      className="hf-dot"
                      style={{ background: "var(--terra)" }}
                    />{" "}
                    Social
                  </span>
                  <span className="hf-flex hf-items-center hf-gap-1">
                    <span
                      className="hf-dot"
                      style={{ background: "var(--ink-4)" }}
                    />{" "}
                    Direct
                  </span>
                </div>
              </div>
              <div style={{ height: 220, position: "relative" }}>
                <AreaChart
                  data={[
                    120, 180, 160, 220, 260, 200, 290, 260, 310, 360, 330, 420,
                    380, 460, 500, 540, 520, 580, 620, 640, 680, 720, 700, 780,
                    820, 860, 840, 920, 950, 1020,
                  ]}
                  color="var(--ink)"
                  fill="rgba(21,18,14,0.06)"
                />
                <div style={{ position: "absolute", inset: 0, opacity: 0.5 }}>
                  <AreaChart
                    data={[
                      40, 60, 55, 70, 90, 80, 110, 100, 130, 150, 140, 170, 160,
                      200, 220, 240, 230, 260, 280, 290, 300, 330, 320, 350,
                      380, 400, 390, 420, 440, 470,
                    ]}
                    color="var(--terra)"
                    fill="rgba(194,65,12,0.05)"
                  />
                </div>
              </div>
              <div
                className="hf-flex hf-between hf-tiny hf-muted"
                style={{ marginTop: 8 }}
              >
                <span>Apr 1</span>
                <span>Apr 8</span>
                <span>Apr 15</span>
                <span>Apr 22</span>
                <span>Apr 30</span>
              </div>
            </div>

            <div className="hf-card" style={{ padding: 20 }}>
              <div className="hf-h3" style={{ marginBottom: 14 }}>
                Sources
              </div>
              <div className="hf-center" style={{ marginBottom: 14 }}>
                <div style={{ position: "relative" }}>
                  <Donut
                    size={140}
                    thickness={20}
                    segments={[
                      { value: 48, color: "var(--ink)" },
                      { value: 28, color: "var(--terra)" },
                      { value: 14, color: "var(--forest)" },
                      { value: 10, color: "var(--ink-4)" },
                    ]}
                  />
                  <div
                    style={{
                      position: "absolute",
                      inset: 0,
                      display: "flex",
                      flexDirection: "column",
                      alignItems: "center",
                      justifyContent: "center",
                    }}
                  >
                    <div className="hf-tiny hf-muted">Sessions</div>
                    <div
                      className="hf-display-2 hf-num"
                      style={{ fontSize: 22 }}
                    >
                      4.8k
                    </div>
                  </div>
                </div>
              </div>
              <div className="hf-col hf-gap-2">
                {sources.map((s) => (
                  <div
                    key={s.label}
                    className="hf-flex hf-items-center hf-gap-2 hf-small"
                  >
                    <span className="hf-dot" style={{ background: s.color }} />
                    <span className="hf-grow">{s.label}</span>
                    <span className="hf-num hf-muted">{s.count}</span>
                    <span
                      className="hf-num"
                      style={{
                        width: 40,
                        textAlign: "right",
                        fontWeight: 500,
                      }}
                    >
                      {s.pct}
                    </span>
                  </div>
                ))}
              </div>
            </div>
          </div>

          {/* Top products + funnel */}
          <div
            className="hf-grid hf-gap-3"
            style={{ gridTemplateColumns: "1fr 1fr", marginTop: 20 }}
          >
            <div className="hf-card" style={{ padding: 0 }}>
              <div
                className="hf-flex hf-between"
                style={{
                  padding: "14px 18px",
                  borderBottom: "1px solid var(--line)",
                }}
              >
                <span className="hf-h3">Top products</span>
                <a
                  className="hf-small"
                  style={{ fontWeight: 500 }}
                  href="/seller/listings"
                >
                  All →
                </a>
              </div>
              {topProducts.map((p, i) => (
                <div
                  key={p.title}
                  className="hf-flex hf-items-center hf-gap-3"
                  style={{
                    padding: "12px 18px",
                    borderBottom:
                      i < topProducts.length - 1
                        ? "1px solid var(--line)"
                        : "none",
                  }}
                >
                  <div style={{ width: 36, flexShrink: 0 }}>
                    <ProductImage tone={p.tone} h={36} r="6px" />
                  </div>
                  <div className="hf-grow">
                    <div className="hf-h4">{p.title}</div>
                    <div className="hf-tiny hf-muted">{p.units} sold</div>
                  </div>
                  <div style={{ width: 140 }}>
                    <div className="hf-progress">
                      <i style={{ width: `${p.share * 100}%` }} />
                    </div>
                  </div>
                  <span
                    className="hf-num hf-h4"
                    style={{ width: 66, textAlign: "right" }}
                  >
                    {money(p.revenue)}
                  </span>
                </div>
              ))}
            </div>

            <div className="hf-card" style={{ padding: 20 }}>
              <div className="hf-h3" style={{ marginBottom: 14 }}>
                Conversion funnel
              </div>
              <div className="hf-col hf-gap-3">
                {funnel.map((s, i) => (
                  <div key={s.label}>
                    <div
                      className="hf-flex hf-between"
                      style={{ marginBottom: 4 }}
                    >
                      <span className="hf-small" style={{ fontWeight: 500 }}>
                        {s.label}
                      </span>
                      <span className="hf-flex hf-gap-2 hf-tiny">
                        <span className="hf-num hf-muted">
                          {(s.pct * 100).toFixed(1)}%
                        </span>
                        <span
                          className="hf-num"
                          style={{ color: "var(--ink)", fontWeight: 600 }}
                        >
                          {s.value.toLocaleString()}
                        </span>
                      </span>
                    </div>
                    <div
                      style={{
                        height: 22,
                        borderRadius: 4,
                        background: "var(--paper-2)",
                        position: "relative",
                        overflow: "hidden",
                      }}
                    >
                      <div
                        style={{
                          width: `${s.pct * 100}%`,
                          height: "100%",
                          background:
                            i === funnel.length - 1
                              ? "var(--terra)"
                              : "var(--ink)",
                          transition: "width .3s",
                        }}
                      />
                    </div>
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
