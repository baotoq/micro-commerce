import { Check, ChevronDown } from "lucide-react";
import { money } from "@/lib/money";
import type { OrderRefund } from "@/lib/seller/types";

export function OrderDetailRefund({ refund }: { refund: OrderRefund }) {
  return (
    <div className="rounded-xl border border-black/[0.06] bg-white p-[18px]">
      <div className="mb-3 flex items-baseline justify-between">
        <span className="text-[15px] font-semibold text-foreground">
          Issue refund
        </span>
        <span className="text-xs text-muted-foreground">
          refundable: {money(refund.refundable)} · across {refund.itemsCount}{" "}
          items
        </span>
      </div>

      <div className="flex flex-col gap-2">
        {refund.items.map((r) => (
          <div
            key={r.name}
            className="flex items-center gap-3 rounded-[10px] p-3"
            style={{
              border: r.selected
                ? "1.5px solid var(--foreground)"
                : "1px solid rgba(0,0,0,0.1)",
              background: r.selected ? "var(--canvas-parchment)" : "white",
            }}
          >
            <span
              className="flex shrink-0 items-center justify-center rounded"
              style={{
                width: 16,
                height: 16,
                borderRadius: 4,
                background: r.selected ? "var(--foreground)" : "white",
                border: r.selected ? "none" : "1.5px solid rgba(0,0,0,0.25)",
                color: "white",
              }}
            >
              {r.selected && <Check size={10} strokeWidth={2.6} />}
            </span>
            <div className="flex-1">
              <div className="text-[13px] font-semibold text-foreground">
                {r.name}
              </div>
              <div className="text-xs text-muted-foreground">
                qty {r.qty} · paid {money(r.price)}
              </div>
            </div>
            {r.selected && r.partial !== undefined ? (
              <div className="flex items-center gap-2">
                <span className="text-xs text-muted-foreground">refund</span>
                <div
                  className="flex items-center gap-1"
                  style={{
                    height: 32,
                    padding: "0 10px",
                    border: "1.5px solid var(--foreground)",
                    borderRadius: 8,
                  }}
                >
                  <span className="font-mono text-muted-foreground">$</span>
                  <span className="text-[13px] font-semibold tabular-nums text-foreground">
                    {r.partial.toFixed(2)}
                  </span>
                </div>
                <span className="text-xs text-muted-foreground">
                  of {money(r.price)}
                </span>
              </div>
            ) : (
              <span className="text-xs text-muted-foreground">—</span>
            )}
          </div>
        ))}
      </div>

      <div
        className="mt-3.5 grid gap-3"
        style={{ gridTemplateColumns: "1fr 1fr" }}
      >
        <div>
          <div className="mb-1 text-xs text-muted-foreground">Reason</div>
          <div
            className="flex items-center justify-between rounded-lg px-3"
            style={{
              height: 36,
              border: "1px solid rgba(0,0,0,0.1)",
            }}
          >
            <span className="text-sm text-foreground">{refund.reason}</span>
            <ChevronDown size={11} className="text-muted-foreground" />
          </div>
        </div>
        <div>
          <div className="mb-1 text-xs text-muted-foreground">Restock?</div>
          <div className="flex gap-1">
            {refund.restockOptions.map((o) => (
              <span
                key={o}
                className={`inline-flex items-center rounded-full px-2.5 py-1 text-[11.5px] font-medium ${
                  o === refund.restockSelected
                    ? "bg-foreground text-white"
                    : "border border-black/[0.1] text-muted-foreground"
                }`}
              >
                {o}
              </span>
            ))}
          </div>
        </div>
      </div>

      <div className="mt-3.5 flex items-center justify-between rounded-lg bg-canvas-parchment px-3.5 py-3">
        <div>
          <div className="text-xs text-muted-foreground">
            Refund total · to Visa · {refund.lastFour}
          </div>
          <div className="mt-0.5 text-[15px] font-semibold tabular-nums text-foreground">
            {money(refund.total)}
          </div>
        </div>
        <button
          type="button"
          className="rounded-xl bg-foreground px-4 py-2 text-sm font-semibold text-white"
        >
          Issue refund
        </button>
      </div>
    </div>
  );
}
