import { Star } from "lucide-react";
import Link from "next/link";
import { money } from "@/lib/money";
import type { OrderInboxRow, StatusTone } from "@/lib/seller/types";

function ChevronRight() {
  return (
    <svg
      width="13"
      height="13"
      viewBox="0 0 20 20"
      fill="none"
      stroke="currentColor"
      strokeWidth="1.6"
      strokeLinecap="round"
      strokeLinejoin="round"
      aria-hidden="true"
    >
      <path d="M8 5l5 5-5 5" />
    </svg>
  );
}

function Checkbox({ checked }: { checked: boolean }) {
  return (
    <span
      className={`inline-flex size-3.5 items-center justify-center rounded-[3px] ${
        checked ? "bg-[#1d1d1f]" : "border-[1.5px] border-black/25 bg-white"
      }`}
      aria-hidden="true"
    >
      {checked && (
        <svg
          width="9"
          height="9"
          viewBox="0 0 12 12"
          fill="none"
          stroke="white"
          strokeWidth="2.6"
          strokeLinecap="round"
          strokeLinejoin="round"
          aria-hidden="true"
        >
          <path d="M2 6l3 3 5-5" />
        </svg>
      )}
    </span>
  );
}

function Avatar({ name }: { name: string }) {
  return (
    <span className="flex size-6 shrink-0 items-center justify-center rounded-full bg-[#e8e3da] text-[10px] font-semibold text-[#1d1d1f]">
      {name.charAt(0)}
    </span>
  );
}

const CHIP_STYLES: Record<StatusTone, string> = {
  warn: "bg-amber-50 text-amber-700",
  mute: "bg-black/[0.04] text-[#1d1d1f]/70",
  good: "bg-emerald-50 text-emerald-700",
  bad: "bg-red-50 text-red-700",
};

const DOT_STYLES: Record<StatusTone, string> = {
  warn: "bg-amber-500",
  mute: "bg-[#1d1d1f]/30",
  good: "bg-emerald-500",
  bad: "bg-red-500",
};

export function OrdersInboxTable({
  rows,
  selectedCount,
}: {
  rows: OrderInboxRow[];
  selectedCount: number;
}) {
  return (
    <div className="overflow-auto">
      <table className="w-full min-w-[900px]">
        <thead>
          <tr className="border-b border-black/[0.06]">
            <th className="w-9 py-2.5 pl-7 pr-2" aria-label="Select">
              <span
                className="inline-flex size-3.5 items-center justify-center rounded-[3px] border-[1.5px] border-black/25"
                aria-hidden="true"
              />
            </th>
            <th className="py-2.5 pr-4 text-left text-[11px] font-medium text-[#1d1d1f]/40">
              Order
            </th>
            <th className="py-2.5 pr-4 text-left text-[11px] font-medium text-[#1d1d1f]/40">
              Customer
            </th>
            <th className="py-2.5 pr-4 text-left text-[11px] font-medium text-[#1d1d1f]/40">
              Items
            </th>
            <th className="py-2.5 pr-4 text-left text-[11px] font-medium text-[#1d1d1f]/40">
              Ship
            </th>
            <th className="py-2.5 pr-4 text-left text-[11px] font-medium text-[#1d1d1f]/40">
              Total
            </th>
            <th className="py-2.5 pr-4 text-left text-[11px] font-medium text-[#1d1d1f]/40">
              Status
            </th>
            <th className="py-2.5 pr-4 text-left text-[11px] font-medium text-[#1d1d1f]/40">
              Age
            </th>
            <th className="w-8 py-2.5 pr-7" aria-label="Open" />
          </tr>
        </thead>
        <tbody>
          {rows.map((row, i) => {
            const selected = i < selectedCount;
            const href = `/seller/orders/${row.id.replace("#", "")}`;
            return (
              <tr
                key={row.id}
                data-selected={selected ? "true" : undefined}
                style={
                  selected ? { background: "rgba(0,102,204,0.04)" } : undefined
                }
                className="border-b border-black/[0.04] last:border-0"
              >
                <td className="py-3 pl-7 pr-2">
                  <Checkbox checked={selected} />
                </td>
                <td className="py-3 pr-4">
                  <div className="flex items-center gap-2">
                    {row.starred && (
                      <Star
                        size={11}
                        className="shrink-0 text-[#1d1d1f]"
                        fill="currentColor"
                        aria-hidden="true"
                      />
                    )}
                    <div>
                      <div className="font-mono text-[13px] font-semibold text-[#1d1d1f]">
                        {row.id}
                      </div>
                      <div className="text-[11px] text-[#1d1d1f]/50">
                        {row.placedLabel}
                      </div>
                    </div>
                  </div>
                </td>
                <td className="py-3 pr-4">
                  <div className="flex items-center gap-2">
                    <Avatar name={row.customer} />
                    <div>
                      <div className="text-[12.5px] font-semibold text-[#1d1d1f]">
                        {row.customer}
                      </div>
                      <div className="text-[11px] text-[#1d1d1f]/50">
                        {row.city}
                      </div>
                    </div>
                  </div>
                </td>
                <td className="max-w-[220px] py-3 pr-4">
                  <div className="overflow-hidden text-ellipsis whitespace-nowrap text-[12px] text-[#1d1d1f]/70">
                    {row.items}
                  </div>
                  <div className="text-[11px] text-[#1d1d1f]/50">
                    {row.qty} item{row.qty > 1 ? "s" : ""}
                  </div>
                </td>
                <td className="py-3 pr-4 text-[11px] text-[#1d1d1f]/50">
                  {row.ship}
                </td>
                <td className="py-3 pr-4 tabular-nums text-[13px] font-semibold text-[#1d1d1f]">
                  {money(row.total)}
                </td>
                <td className="py-3 pr-4">
                  <span
                    className={`inline-flex items-center gap-1.5 rounded-full px-2.5 py-1 text-[11px] font-semibold ${CHIP_STYLES[row.tone]}`}
                  >
                    <span
                      className={`size-1.5 rounded-full ${DOT_STYLES[row.tone]}`}
                      aria-hidden="true"
                    />
                    {row.status}
                  </span>
                </td>
                <td className="py-3 pr-4 tabular-nums text-[11px] text-[#1d1d1f]/50">
                  {row.age}
                </td>
                <td className="py-3 pr-7 text-[#1d1d1f]/30">
                  <Link href={href} aria-label={`Open order ${row.id}`}>
                    <ChevronRight />
                  </Link>
                </td>
              </tr>
            );
          })}
        </tbody>
      </table>
    </div>
  );
}
