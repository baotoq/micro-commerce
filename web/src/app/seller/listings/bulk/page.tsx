// web/src/app/seller/listings/bulk/page.tsx
import Link from "next/link";
import { SellerTopbar } from "@/components/seller/seller-topbar";
import { Button, buttonVariants } from "@/components/ui/button";
import { cn } from "@/lib/utils";

const CHIPS = [
  { label: "All · 42", active: false },
  { label: "Active · 38", active: false },
  { label: "Low · 3", active: true },
  { label: "Out · 1", active: true },
  { label: "Drafts · 4", active: false },
];

type Row = {
  name: string;
  sku: string;
  status: "Out" | "Low";
  stock: number;
  price: number;
  newPrice: number | null;
  selected: boolean;
};

const ROWS: Row[] = [
  {
    name: "Rust mug Nº 04",
    sku: "MC-MG-041",
    status: "Out",
    stock: 0,
    price: 28,
    newPrice: 30.8,
    selected: true,
  },
  {
    name: "Ember tea bowl",
    sku: "MC-SC-008",
    status: "Low",
    stock: 2,
    price: 36,
    newPrice: 39.6,
    selected: true,
  },
  {
    name: "Peat serving bowl",
    sku: "MC-BW-051",
    status: "Low",
    stock: 4,
    price: 90,
    newPrice: 99,
    selected: true,
  },
  {
    name: "Mist tumbler",
    sku: "MC-TB-038",
    status: "Low",
    stock: 3,
    price: 40,
    newPrice: null,
    selected: false,
  },
];

const money = (n: number) =>
  `$${n.toLocaleString("en-US", { minimumFractionDigits: n % 1 === 0 ? 0 : 2, maximumFractionDigits: 2 })}`;

const SELECTED = ROWS.filter((r) => r.selected);

export default function ListingsBulkPage() {
  return (
    <div className="flex min-h-screen flex-col">
      <SellerTopbar
        title="Listings"
        subtitle="Filtered · low & out of stock"
        actions={
          <Link
            href="/seller/listings/new"
            className={cn(buttonVariants(), "rounded-full")}
          >
            + New listing
          </Link>
        }
      />

      {/* Filter chips */}
      <div className="border-b border-black/[0.06] px-7 py-3.5">
        <div className="flex items-center gap-2">
          {CHIPS.map((c) => (
            <span
              key={c.label}
              className={
                c.active
                  ? "rounded-full bg-[#1d1d1f] px-3 py-1 text-[12px] font-medium text-white"
                  : "rounded-full bg-[#f5f5f7] px-3 py-1 text-[12px] font-medium text-[#1d1d1f]/70"
              }
            >
              {c.label}
            </span>
          ))}
        </div>
      </div>

      {/* Dark bulk-action bar */}
      <div className="flex items-center justify-between bg-[#1d1d1f] px-7 py-3 text-white">
        <div className="flex items-center gap-3">
          <span
            aria-hidden="true"
            className="flex h-[18px] w-[18px] items-center justify-center rounded-[3px] bg-white text-[11px] text-[#1d1d1f]"
          >
            ✓
          </span>
          <span className="text-[13.5px] font-semibold">3 of 4 selected</span>
        </div>
        <div className="flex gap-2">
          {["Edit price", "Adjust stock", "Move to draft"].map((label) => (
            <Button
              key={label}
              variant="ghost"
              size="sm"
              className="border border-white/20 bg-white/[0.12] text-white hover:bg-white/20 hover:text-white"
            >
              {label}
            </Button>
          ))}
          <Link
            href="/seller/listings/published"
            className={cn(
              buttonVariants({ size: "sm" }),
              "bg-white text-[#1d1d1f] hover:bg-white/90",
            )}
          >
            Apply →
          </Link>
        </div>
      </div>

      {/* Body: table + drawer */}
      <div className="flex flex-1 overflow-hidden">
        <div className="flex-1 overflow-auto">
          <table className="w-full text-sm">
            <thead>
              <tr className="border-b border-black/[0.06] text-left text-[11px] uppercase tracking-wider text-[#1d1d1f]/50">
                <th className="w-[32px] px-7 py-2.5"></th>
                <th className="px-2 py-2.5 font-medium">Product</th>
                <th className="px-2 py-2.5 font-medium">SKU</th>
                <th className="px-2 py-2.5 font-medium">Status</th>
                <th className="px-2 py-2.5 font-medium">Stock</th>
                <th className="px-2 py-2.5 font-medium">Price</th>
                <th className="px-7 py-2.5 font-medium">New price</th>
              </tr>
            </thead>
            <tbody>
              {ROWS.map((r) => (
                <tr
                  key={r.sku}
                  className="border-b border-black/[0.04]"
                  style={
                    r.selected
                      ? { background: "rgba(194,65,12,0.04)" }
                      : undefined
                  }
                >
                  <td className="px-7 py-3">
                    <span
                      aria-hidden="true"
                      className={
                        r.selected
                          ? "flex h-[14px] w-[14px] items-center justify-center rounded-[3px] bg-[#1d1d1f] text-[9px] text-white"
                          : "flex h-[14px] w-[14px] items-center justify-center rounded-[3px] border-[1.5px] border-[#1d1d1f]/40"
                      }
                    >
                      {r.selected ? "✓" : ""}
                    </span>
                  </td>
                  <td className="px-2 py-3 font-medium text-[#1d1d1f]">
                    {r.name}
                  </td>
                  <td className="px-2 py-3 font-mono text-[12px] text-[#1d1d1f]/60">
                    {r.sku}
                  </td>
                  <td className="px-2 py-3">
                    <span
                      className={
                        r.status === "Out"
                          ? "rounded-full bg-red-50 px-2 py-0.5 text-[11px] font-medium text-red-700"
                          : "rounded-full bg-amber-50 px-2 py-0.5 text-[11px] font-medium text-amber-700"
                      }
                    >
                      {r.status}
                    </span>
                  </td>
                  <td
                    className="px-2 py-3 tabular-nums"
                    style={{
                      color: r.stock === 0 ? "#dc2626" : "#b45309",
                      fontWeight: 500,
                    }}
                  >
                    {r.stock}
                  </td>
                  <td
                    className="px-2 py-3 tabular-nums text-[#1d1d1f]/60"
                    style={{
                      textDecoration: r.selected ? "line-through" : "none",
                    }}
                  >
                    {money(r.price)}
                  </td>
                  <td
                    className="px-7 py-3 tabular-nums"
                    style={{
                      color: r.selected ? "#16a34a" : "#1d1d1f40",
                      fontWeight: 600,
                    }}
                  >
                    {r.newPrice !== null ? money(r.newPrice) : "—"}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>

        {/* Right drawer */}
        <aside className="w-[340px] shrink-0 border-l border-black/[0.06] bg-white p-[22px]">
          <div className="mb-1.5 text-[11px] uppercase tracking-wider text-[#1d1d1f]/50">
            Bulk edit · 3 items
          </div>
          <h2 className="mb-[18px] text-[20px] font-semibold tracking-tight text-[#1d1d1f]">
            Adjust price
          </h2>

          <div className="mb-4 flex gap-1 rounded-lg bg-[#f5f5f7] p-[3px]">
            {["Set to", "Increase", "Decrease"].map((s, i) => (
              <Button
                key={s}
                variant="ghost"
                size="sm"
                className={
                  i === 1
                    ? "flex-1 bg-white text-[#1d1d1f] shadow-sm hover:bg-white"
                    : "flex-1 bg-transparent text-[#1d1d1f] hover:bg-transparent"
                }
              >
                {s}
              </Button>
            ))}
          </div>

          <div className="mb-4 flex gap-2">
            <div className="flex-1">
              <div className="mb-1 text-[11px] text-[#1d1d1f]/50">Amount</div>
              <div
                className="flex items-center rounded-lg px-3.5"
                style={{ height: 44, border: "1.5px solid #1d1d1f" }}
              >
                <span className="text-[16px] font-semibold tabular-nums text-[#1d1d1f]">
                  10
                </span>
              </div>
            </div>
            <div style={{ width: 90 }}>
              <div className="mb-1 text-[11px] text-[#1d1d1f]/50">Unit</div>
              <div className="flex h-[44px] items-center justify-between rounded-lg border border-black/[0.1] px-3">
                <span className="text-[16px] font-semibold text-[#1d1d1f]">
                  %
                </span>
                <span className="text-[10px] text-[#1d1d1f]/50">▾</span>
              </div>
            </div>
          </div>

          <div className="mb-[18px] rounded-lg bg-[#f5f5f7] p-3.5">
            <div className="mb-1.5 text-[11px] text-[#1d1d1f]/50">
              Preview · 3 items
            </div>
            <div className="flex flex-col gap-1.5 text-[13px]">
              {SELECTED.map((r) => (
                <div key={r.sku} className="flex justify-between">
                  <span className="truncate text-[#1d1d1f]/60">{r.name}</span>
                  <span className="tabular-nums">
                    <span className="text-[#1d1d1f]/50 line-through">
                      {money(r.price)}
                    </span>{" "}
                    <span className="font-semibold text-emerald-600">
                      {r.newPrice !== null ? money(r.newPrice) : ""}
                    </span>
                  </span>
                </div>
              ))}
            </div>
          </div>

          <Link
            href="/seller/listings/published"
            className={cn(buttonVariants(), "h-[42px] w-full rounded-lg")}
          >
            Apply to 3 items
          </Link>
          <Link
            href="/seller/listings"
            className={cn(
              buttonVariants({ variant: "ghost" }),
              "mt-1.5 w-full",
            )}
          >
            Cancel
          </Link>
        </aside>
      </div>
    </div>
  );
}
