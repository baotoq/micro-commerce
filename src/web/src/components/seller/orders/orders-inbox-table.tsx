import { Star } from "lucide-react";
import Link from "next/link";
import { money } from "@/lib/money";
import type { OrderInboxRow, StatusTone } from "@/lib/seller/orders/types";

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
        checked ? "bg-foreground" : "border-[1.5px] border-black/25 bg-white"
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
    <span className="flex size-6 shrink-0 items-center justify-center rounded-full bg-[#e8e3da] text-[10px] font-semibold text-foreground">
      {name.charAt(0)}
    </span>
  );
}

const CHIP_STYLES: Record<StatusTone, string> = {
  warn: "bg-warn/15 text-warn",
  mute: "bg-black/[0.04] text-muted-foreground",
  good: "bg-good/15 text-good",
  bad: "bg-bad/15 text-bad",
};

const DOT_STYLES: Record<StatusTone, string> = {
  warn: "bg-warn",
  mute: "bg-foreground/30",
  good: "bg-good",
  bad: "bg-bad",
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
            <th className="py-2.5 pr-4 text-left text-[11px] font-medium text-muted-foreground">
              Order
            </th>
            <th className="py-2.5 pr-4 text-left text-[11px] font-medium text-muted-foreground">
              Customer
            </th>
            <th className="py-2.5 pr-4 text-left text-[11px] font-medium text-muted-foreground">
              Items
            </th>
            <th className="py-2.5 pr-4 text-left text-[11px] font-medium text-muted-foreground">
              Ship
            </th>
            <th className="py-2.5 pr-4 text-left text-[11px] font-medium text-muted-foreground">
              Total
            </th>
            <th className="py-2.5 pr-4 text-left text-[11px] font-medium text-muted-foreground">
              Status
            </th>
            <th className="py-2.5 pr-4 text-left text-[11px] font-medium text-muted-foreground">
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
                className={`border-b border-black/[0.04] last:border-0${selected ? " bg-primary/[0.04]" : ""}`}
              >
                <td className="py-3 pl-7 pr-2">
                  <Checkbox checked={selected} />
                </td>
                <td className="py-3 pr-4">
                  <div className="flex items-center gap-2">
                    {row.starred && (
                      <Star
                        size={11}
                        className="shrink-0 text-foreground"
                        fill="currentColor"
                        aria-hidden="true"
                      />
                    )}
                    <div>
                      <div className="font-mono text-[13px] font-semibold text-foreground">
                        {row.id}
                      </div>
                      <div className="text-[11px] text-muted-foreground">
                        {row.placedLabel}
                      </div>
                    </div>
                  </div>
                </td>
                <td className="py-3 pr-4">
                  <div className="flex items-center gap-2">
                    <Avatar name={row.customer} />
                    <div>
                      <div className="text-[12.5px] font-semibold text-foreground">
                        {row.customer}
                      </div>
                      <div className="text-[11px] text-muted-foreground">
                        {row.city}
                      </div>
                    </div>
                  </div>
                </td>
                <td className="max-w-[220px] py-3 pr-4">
                  <div className="overflow-hidden text-ellipsis whitespace-nowrap text-[12px] text-muted-foreground">
                    {row.items}
                  </div>
                  <div className="text-[11px] text-muted-foreground">
                    {row.qty} item{row.qty > 1 ? "s" : ""}
                  </div>
                </td>
                <td className="py-3 pr-4 text-[11px] text-muted-foreground">
                  {row.ship}
                </td>
                <td className="py-3 pr-4 tabular-nums text-[13px] font-semibold text-foreground">
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
                <td className="py-3 pr-4 tabular-nums text-[11px] text-muted-foreground">
                  {row.age}
                </td>
                <td className="py-3 pr-7 text-muted-foreground">
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
