import { Icon } from "@/components/icon";
import { ProductImage } from "@/components/primitives";
import { SellerSidebar, SellerTopbar } from "@/components/seller-chrome";
import { money } from "@/lib/money";

type Status = "Active" | "Low" | "Out" | "Draft";

const rows: {
  t: string;
  sku: string;
  tone:
    | "clay"
    | "sage"
    | "bone"
    | "rose"
    | "cobalt"
    | "cream"
    | "rust"
    | "moss"
    | "shadow";
  price: number;
  stock: number;
  status: Status;
  views: number;
  sales: number;
}[] = [
  {
    t: "Persimmon vase",
    sku: "MS-VS-001",
    tone: "clay",
    price: 86,
    stock: 0,
    status: "Out",
    views: 412,
    sales: 14,
  },
  {
    t: "Forest bowl, lg.",
    sku: "MS-BW-014",
    tone: "sage",
    price: 64,
    stock: 8,
    status: "Active",
    views: 308,
    sales: 9,
  },
  {
    t: "Cream tumbler — set of 2",
    sku: "MS-TB-007",
    tone: "bone",
    price: 48,
    stock: 22,
    status: "Active",
    views: 256,
    sales: 12,
  },
  {
    t: "Soft hand vessel",
    sku: "MS-VS-019",
    tone: "rose",
    price: 92,
    stock: 2,
    status: "Low",
    views: 180,
    sales: 6,
  },
  {
    t: "Indigo carafe",
    sku: "MS-CR-003",
    tone: "cobalt",
    price: 110,
    stock: 5,
    status: "Active",
    views: 148,
    sales: 4,
  },
  {
    t: "Bone dinner plate",
    sku: "MS-PL-022",
    tone: "cream",
    price: 38,
    stock: 36,
    status: "Active",
    views: 142,
    sales: 8,
  },
  {
    t: "Rust mug, Nº 04",
    sku: "MS-MG-041",
    tone: "rust",
    price: 32,
    stock: 18,
    status: "Active",
    views: 132,
    sales: 7,
  },
  {
    t: "Moss saucer, set of 4",
    sku: "MS-SC-008",
    tone: "moss",
    price: 44,
    stock: 0,
    status: "Draft",
    views: 0,
    sales: 0,
  },
  {
    t: "Shadow vase, tall",
    sku: "MS-VS-031",
    tone: "shadow",
    price: 124,
    stock: 3,
    status: "Low",
    views: 92,
    sales: 3,
  },
];

const filterChips = [
  "All · 42",
  "Active · 38",
  "Low · 3",
  "Out · 1",
  "Drafts · 4",
];

function statusTone(s: Status): "good" | "warn" | "bad" | "soft" {
  if (s === "Active") return "good";
  if (s === "Low") return "warn";
  if (s === "Out") return "bad";
  return "soft";
}

function stockColor(stock: number) {
  if (stock === 0) return "var(--bad)";
  if (stock < 5) return "var(--warn)";
  return "var(--ink)";
}

export default function SellerListingsPage() {
  return (
    <div
      className="hf hf-desktop"
      style={{ flexDirection: "row", minHeight: "100vh" }}
    >
      <SellerSidebar active="Listings" />
      <div className="hf-grow hf-col" style={{ overflow: "hidden" }}>
        <SellerTopbar
          title="Listings"
          subtitle="42 products · 38 active"
          actions={
            <>
              <button className="hf-btn hf-btn-outline" type="button">
                <Icon n="upload" s={12} /> Import CSV
              </button>
              <button className="hf-btn hf-btn-primary" type="button">
                <Icon n="plus" s={12} /> New listing
              </button>
            </>
          }
        />

        {/* Filter row */}
        <div
          className="hf-flex hf-between"
          style={{
            padding: "14px 28px",
            borderBottom: "1px solid var(--line)",
            flexWrap: "wrap",
            gap: 12,
          }}
        >
          <div className="hf-flex hf-items-center hf-gap-2">
            {filterChips.map((c, i) => (
              <span
                key={c}
                className={`hf-chip${i === 0 ? " hf-chip-on" : ""}`}
              >
                {c}
              </span>
            ))}
          </div>
          <div className="hf-flex hf-items-center hf-gap-2">
            <div
              className="hf-flex hf-items-center hf-gap-2 hf-px-3"
              style={{
                height: 30,
                background: "var(--paper-2)",
                borderRadius: 999,
                color: "var(--ink-3)",
                width: 220,
              }}
            >
              <Icon n="search" s={12} />
              <span className="hf-small">Search products…</span>
            </div>
            <button className="hf-btn hf-btn-outline hf-btn-sm" type="button">
              <Icon n="filter" s={11} /> Collection
            </button>
            <button className="hf-btn hf-btn-outline hf-btn-sm" type="button">
              <Icon n="sort" s={11} /> Best-selling
            </button>
          </div>
        </div>

        {/* Table */}
        <div className="hf-grow" style={{ overflow: "auto" }}>
          <table className="hf-table" style={{ minWidth: "100%" }}>
            <thead>
              <tr>
                <th style={{ width: 32, paddingRight: 0 }}>
                  <div
                    style={{
                      width: 14,
                      height: 14,
                      border: "1.5px solid var(--ink-4)",
                      borderRadius: 3,
                    }}
                  />
                </th>
                <th style={{ width: 320 }}>Product</th>
                <th>SKU</th>
                <th>Status</th>
                <th>Stock</th>
                <th>Price</th>
                <th>Views · 30d</th>
                <th>Sales · 30d</th>
                <th />
              </tr>
            </thead>
            <tbody>
              {rows.map((r) => (
                <tr key={r.sku}>
                  <td style={{ paddingRight: 0 }}>
                    <div
                      style={{
                        width: 14,
                        height: 14,
                        border: "1.5px solid var(--ink-4)",
                        borderRadius: 3,
                      }}
                    />
                  </td>
                  <td>
                    <div className="hf-flex hf-items-center hf-gap-3">
                      <div style={{ width: 36, flexShrink: 0 }}>
                        <ProductImage tone={r.tone} h={36} r="6px" />
                      </div>
                      <div>
                        <div className="hf-h4" style={{ color: "var(--ink)" }}>
                          {r.t}
                        </div>
                        <div className="hf-tiny hf-muted">
                          Vessels · 4 photos
                        </div>
                      </div>
                    </div>
                  </td>
                  <td className="hf-mono" style={{ color: "var(--ink-3)" }}>
                    {r.sku}
                  </td>
                  <td>
                    <span
                      className={`hf-chip hf-chip-${statusTone(r.status)}`}
                      style={{ fontSize: 11 }}
                    >
                      {r.status}
                    </span>
                  </td>
                  <td
                    className="hf-num"
                    style={{ color: stockColor(r.stock), fontWeight: 500 }}
                  >
                    {r.stock}
                  </td>
                  <td
                    className="hf-num"
                    style={{ color: "var(--ink)", fontWeight: 500 }}
                  >
                    {money(r.price)}
                  </td>
                  <td className="hf-num">{r.views.toLocaleString()}</td>
                  <td className="hf-num">{r.sales}</td>
                  <td>
                    <Icon n="chevR" s={13} />
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>

        {/* Footer / pagination */}
        <div
          className="hf-flex hf-between"
          style={{
            padding: "12px 28px",
            borderTop: "1px solid var(--line)",
            background: "var(--paper)",
          }}
        >
          <span className="hf-small hf-muted">9 of 42 shown</span>
          <div className="hf-flex hf-gap-1">
            <button className="hf-btn hf-btn-outline hf-btn-sm" type="button">
              <Icon n="chevL" s={11} />
            </button>
            <button
              className="hf-btn hf-btn-sm"
              type="button"
              style={{ background: "var(--ink)", color: "var(--paper)" }}
            >
              1
            </button>
            <button className="hf-btn hf-btn-ghost hf-btn-sm" type="button">
              2
            </button>
            <button className="hf-btn hf-btn-ghost hf-btn-sm" type="button">
              3
            </button>
            <button className="hf-btn hf-btn-outline hf-btn-sm" type="button">
              <Icon n="chevR" s={11} />
            </button>
          </div>
        </div>
      </div>
    </div>
  );
}
