import { Icon } from "@/components/icon";
import { AreaChart, Avatar, Spark } from "@/components/primitives";
import { SellerSidebar, SellerTopbar } from "@/components/seller-chrome";
import { money } from "@/lib/money";

const stats = [
  {
    l: "Revenue · 7 days",
    v: money(4280),
    d: "12%",
    t: "up" as const,
    spark: [0.4, 0.5, 0.45, 0.6, 0.55, 0.7, 0.85],
  },
  {
    l: "Orders · 7 days",
    v: "38",
    d: "7%",
    t: "up" as const,
    spark: [0.5, 0.6, 0.55, 0.5, 0.7, 0.65, 0.8],
  },
  {
    l: "Avg. order",
    v: money(112),
    d: "2%",
    t: "down" as const,
    spark: [0.6, 0.55, 0.7, 0.5, 0.4, 0.55, 0.5],
  },
  {
    l: "Storefront views",
    v: "2.1k",
    d: "24%",
    t: "up" as const,
    spark: [0.3, 0.45, 0.4, 0.55, 0.6, 0.75, 0.9],
  },
];

const tasks = [
  { l: "Pack 2 orders ready to ship", s: "#1042 · #1041", t: "warn" },
  { l: "Reply to 3 messages", s: "Sasha asked about glaze", t: "good" },
  {
    l: "Restock: Persimmon vase",
    s: "Out of stock — 4 in pre-order",
    t: "bad",
  },
  {
    l: "Review draft: Forest bowl Sm.",
    s: "Last edited 2 days ago",
    t: "mute",
  },
];

const orders = [
  {
    id: "#1042",
    name: "Sasha L.",
    items: "Persimmon vase + 1",
    total: 152,
    status: "New",
    tone: "warn",
  },
  {
    id: "#1041",
    name: "Devon T.",
    items: "Forest bowl, lg.",
    total: 64,
    status: "Paid",
    tone: "good",
  },
  {
    id: "#1040",
    name: "Ari K.",
    items: "Cream tumbler set",
    total: 48,
    status: "Paid",
    tone: "good",
  },
  {
    id: "#1039",
    name: "June P.",
    items: "Indigo carafe",
    total: 110,
    status: "Shipped",
    tone: "mute",
  },
  {
    id: "#1038",
    name: "Theo R.",
    items: "Soft hand vessel + 2",
    total: 218,
    status: "Shipped",
    tone: "mute",
  },
];

function statusChipClass(tone: string) {
  if (tone === "good") return "hf-chip hf-chip-good";
  if (tone === "warn") return "hf-chip hf-chip-warn";
  return "hf-chip hf-chip-soft";
}

export default function SellerOverviewPage() {
  return (
    <div
      className="hf hf-desktop"
      style={{ flexDirection: "row", minHeight: "100vh" }}
    >
      <SellerSidebar active="Overview" />
      <div className="hf-grow hf-col" style={{ overflow: "hidden" }}>
        <SellerTopbar
          title="Good morning, Mira"
          subtitle="Tuesday · April 8"
          actions={
            <>
              <button className="hf-btn hf-btn-outline" type="button">
                <Icon n="upload" s={12} /> Export
              </button>
              <button className="hf-btn hf-btn-primary" type="button">
                <Icon n="plus" s={12} /> New listing
              </button>
            </>
          }
        />
        <div
          className="hf-grow"
          style={{ overflow: "auto", padding: "24px 28px" }}
        >
          {/* Stat row */}
          <div
            className="hf-grid hf-gap-3"
            style={{
              gridTemplateColumns: "repeat(4, 1fr)",
              marginBottom: 20,
            }}
          >
            {stats.map((s) => (
              <div key={s.l} className="hf-card" style={{ padding: 18 }}>
                <div className="hf-tiny hf-muted" style={{ marginBottom: 6 }}>
                  {s.l}
                </div>
                <div className="hf-flex hf-between hf-items-end">
                  <div
                    className="hf-display-2 hf-num"
                    style={{ fontSize: 30, lineHeight: 1 }}
                  >
                    {s.v}
                  </div>
                  <span
                    className="hf-tiny"
                    style={{
                      color: s.t === "up" ? "var(--good)" : "var(--bad)",
                      fontWeight: 600,
                      letterSpacing: 0,
                    }}
                  >
                    {s.t === "up" ? "↑" : "↓"} {s.d}
                  </span>
                </div>
                <div
                  style={{ height: 36, marginTop: 10, color: "var(--ink-3)" }}
                >
                  <Spark
                    data={s.spark}
                    color="var(--ink-3)"
                    fill="rgba(21,18,14,0.05)"
                  />
                </div>
              </div>
            ))}
          </div>

          {/* 2-col grid */}
          <div
            className="hf-grid hf-gap-3"
            style={{ gridTemplateColumns: "1.6fr 1fr" }}
          >
            {/* Revenue chart */}
            <div className="hf-card" style={{ padding: 20 }}>
              <div
                className="hf-flex hf-between hf-items-start"
                style={{ marginBottom: 14 }}
              >
                <div>
                  <div className="hf-tiny hf-muted">Revenue</div>
                  <div
                    className="hf-display-2 hf-num"
                    style={{ fontSize: 26, marginTop: 2 }}
                  >
                    {money(4280)}
                  </div>
                </div>
                <div className="hf-flex hf-gap-1">
                  {["7d", "30d", "90d", "Year"].map((p, i) => (
                    <span
                      key={p}
                      className={`hf-chip${i === 0 ? " hf-chip-on" : ""}`}
                      style={{ fontSize: 11 }}
                    >
                      {p}
                    </span>
                  ))}
                </div>
              </div>
              <div style={{ height: 200 }}>
                <AreaChart
                  data={[
                    120, 280, 220, 340, 410, 380, 520, 480, 540, 620, 590, 720,
                    680, 780,
                  ]}
                  color="var(--ink)"
                  fill="rgba(21,18,14,0.06)"
                />
              </div>
              <div
                className="hf-flex hf-between hf-tiny hf-muted"
                style={{ marginTop: 8 }}
              >
                <span>Apr 1</span>
                <span>Apr 4</span>
                <span>Apr 7</span>
                <span>Apr 10</span>
                <span>Apr 14</span>
              </div>
            </div>

            {/* Today tasks */}
            <div className="hf-card" style={{ padding: 20 }}>
              <div className="hf-h3" style={{ marginBottom: 12 }}>
                Today
              </div>
              <div className="hf-col hf-gap-3">
                {tasks.map((task) => (
                  <div key={task.l} className="hf-flex hf-items-start hf-gap-3">
                    <span
                      className={`hf-dot hf-dot-${task.t}`}
                      style={{ marginTop: 7, flexShrink: 0 }}
                    />
                    <div className="hf-grow">
                      <div className="hf-h4">{task.l}</div>
                      <div className="hf-tiny hf-muted">{task.s}</div>
                    </div>
                    <Icon n="chevR" s={13} />
                  </div>
                ))}
              </div>
            </div>
          </div>

          {/* Recent orders */}
          <div className="hf-card" style={{ marginTop: 20, padding: 0 }}>
            <div
              className="hf-flex hf-between"
              style={{
                padding: "14px 20px",
                borderBottom: "1px solid var(--line)",
              }}
            >
              <div className="hf-h3">Recent orders</div>
              <a
                href="/seller/orders"
                className="hf-small"
                style={{ color: "var(--ink-2)", fontWeight: 500 }}
              >
                All orders →
              </a>
            </div>
            <table className="hf-table">
              <thead>
                <tr>
                  <th>Order</th>
                  <th>Customer</th>
                  <th>Items</th>
                  <th>Total</th>
                  <th>Status</th>
                  <th />
                </tr>
              </thead>
              <tbody>
                {orders.map((o) => (
                  <tr key={o.id}>
                    <td
                      className="hf-mono"
                      style={{ color: "var(--ink)", fontWeight: 500 }}
                    >
                      {o.id}
                    </td>
                    <td>
                      <div className="hf-flex hf-items-center hf-gap-2">
                        <Avatar name={o.name} size="sm" />
                        <span>{o.name}</span>
                      </div>
                    </td>
                    <td className="hf-muted">{o.items}</td>
                    <td
                      className="hf-num"
                      style={{ color: "var(--ink)", fontWeight: 500 }}
                    >
                      {money(o.total)}
                    </td>
                    <td>
                      <span
                        className={statusChipClass(o.tone)}
                        style={{ fontSize: 11 }}
                      >
                        {o.status}
                      </span>
                    </td>
                    <td>
                      <Icon n="chevR" s={13} />
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      </div>
    </div>
  );
}
