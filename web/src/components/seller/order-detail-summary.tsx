import { money } from "@/lib/money";
import type { OrderSummary } from "@/lib/seller/types";

export function OrderDetailSummary({ summary }: { summary: OrderSummary }) {
  return (
    <div className="rounded-xl border border-black/[0.06] bg-white p-4">
      <div className="mb-3 text-[15px] font-semibold text-[#1d1d1f]">
        Summary
      </div>
      <div className="flex flex-col gap-2 text-sm">
        <div className="flex justify-between">
          <span className="text-[#1d1d1f]/50">
            Subtotal · {summary.itemsCount} items
          </span>
          <span className="tabular-nums">{money(summary.subtotal)}</span>
        </div>
        <div className="flex justify-between">
          <span className="text-[#1d1d1f]/50">Shipping</span>
          <span className="tabular-nums">{money(summary.shipping)}</span>
        </div>
        <div className="flex justify-between">
          <span className="text-[#1d1d1f]/50">Tax</span>
          <span className="tabular-nums">{money(summary.tax)}</span>
        </div>
        <div className="my-1 h-px bg-black/[0.06]" />
        <div className="flex items-baseline justify-between">
          <span className="text-[13.5px] font-semibold text-[#1d1d1f]">
            Customer paid
          </span>
          <span className="text-[18px] font-semibold tabular-nums text-[#1d1d1f]">
            {money(summary.paid)}
          </span>
        </div>
        <div className="mt-2 flex justify-between text-xs text-[#1d1d1f]/50">
          <span>Micro fee · {summary.feePct}%</span>
          <span className="tabular-nums">−{money(summary.fee)}</span>
        </div>
        <div className="flex justify-between text-xs text-[#1d1d1f]/50">
          <span>Shipping label · {summary.labelCarrier}</span>
          <span className="tabular-nums">−{money(summary.labelCost)}</span>
        </div>
        <div className="flex justify-between">
          <span className="text-sm text-[#1d1d1f]">You'll receive</span>
          <span
            className="text-[13.5px] font-semibold tabular-nums"
            style={{ color: "#16a34a" }}
          >
            {money(summary.net)}
          </span>
        </div>
      </div>
    </div>
  );
}
