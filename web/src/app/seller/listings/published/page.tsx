// web/src/app/seller/listings/published/page.tsx
import { SellerTopbar } from "@/components/seller/seller-topbar";
import { Button } from "@/components/ui/button";

const KPIS = [
  { label: "Active listings", value: "39", delta: "+1" },
  { label: "Variants in stock", value: "128", delta: "+2" },
  { label: "Out-of-stock items", value: "1", delta: "−2" },
];

const ACTIVITY = [
  {
    when: "just now",
    item: "Persimmon vase · Medium",
    change: "Price · $86 → $95",
    by: "You",
  },
  {
    when: "just now",
    item: "Persimmon vase · Small",
    change: "Price · $64 → $70",
    by: "You",
  },
  {
    when: "just now",
    item: "Persimmon vase · Large",
    change: "Status · Out → still out (no stock)",
    by: "You",
  },
  {
    when: "12 min ago",
    item: "Ember tea bowl",
    change: "Price · $36 → $39.60",
    by: "You · bulk",
  },
  {
    when: "12 min ago",
    item: "Peat serving bowl",
    change: "Price · $90 → $99",
    by: "You · bulk",
  },
];

export default function ListingsPublishedPage() {
  return (
    <div className="flex min-h-screen flex-col">
      <SellerTopbar
        title="Listings"
        subtitle="42 products · 39 active"
        actions={<Button className="rounded-full">+ New listing</Button>}
      />

      <div className="border-b border-black/[0.06] bg-[#DDEDE1] px-7 py-3.5">
        <div className="flex items-center gap-3">
          <span
            aria-hidden="true"
            className="flex h-[26px] w-[26px] shrink-0 items-center justify-center rounded-full bg-emerald-600 text-sm text-white"
          >
            ✓
          </span>
          <span className="flex-1 text-[13.5px] font-semibold text-[#1d1d1f]">
            Persimmon vase published · 2 variants updated, 1 went live.
          </span>
          <Button variant="ghost" size="sm">
            View shop →
          </Button>
          <Button variant="ghost" size="sm">
            Undo
          </Button>
        </div>
      </div>

      <div className="flex-1 overflow-auto px-7 py-6">
        <div className="mb-5 grid grid-cols-3 gap-3">
          {KPIS.map((k) => (
            <div
              key={k.label}
              className="rounded-xl border border-black/[0.06] bg-white p-[18px]"
            >
              <div className="text-[11px] text-[#1d1d1f]/50">{k.label}</div>
              <div className="mt-1.5 flex items-end gap-2">
                <div className="text-[28px] font-semibold leading-none tabular-nums text-[#1d1d1f]">
                  {k.value}
                </div>
                <span className="mb-1 text-[11px] font-semibold text-emerald-600">
                  ↑ {k.delta}
                </span>
              </div>
            </div>
          ))}
        </div>

        <div className="rounded-xl border border-black/[0.06] bg-white">
          <div className="flex items-center justify-between border-b border-black/[0.06] px-5 py-3.5">
            <h2 className="text-[15px] font-semibold text-[#1d1d1f]">
              What just changed
            </h2>
            <span className="text-[13px] font-medium text-[#1d1d1f]/70">
              Activity log →
            </span>
          </div>
          <table className="w-full text-sm">
            <thead>
              <tr className="text-left text-[11px] uppercase tracking-wider text-[#1d1d1f]/50">
                <th className="px-5 py-2.5 font-medium">When</th>
                <th className="px-5 py-2.5 font-medium">Item</th>
                <th className="px-5 py-2.5 font-medium">Change</th>
                <th className="px-5 py-2.5 font-medium">By</th>
              </tr>
            </thead>
            <tbody>
              {ACTIVITY.map((r) => (
                <tr key={r.item} className="border-t border-black/[0.04]">
                  <td className="px-5 py-3 text-[11px] text-[#1d1d1f]/60">
                    {r.when}
                  </td>
                  <td className="px-5 py-3 font-medium text-[#1d1d1f]">
                    {r.item}
                  </td>
                  <td className="px-5 py-3 text-[#1d1d1f]/70">{r.change}</td>
                  <td className="px-5 py-3 text-[13px]">{r.by}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </div>
    </div>
  );
}
