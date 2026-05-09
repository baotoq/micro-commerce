import { Check } from "lucide-react";
import { money } from "@/lib/money";
import type { OrderFulfillment } from "@/lib/seller/types";

const TONE_BG: Record<string, string> = {
  clay: "#d6c2a8",
  rust: "#bf6b3c",
  sage: "#8aad8a",
  cream: "#e8dfd0",
};

function ProdImg({ tone, size = 56 }: { tone: string; size?: number }) {
  return (
    <div
      className="shrink-0 rounded-lg"
      style={{
        width: size,
        height: size,
        background: TONE_BG[tone] ?? "#d6c2a8",
      }}
    />
  );
}

function FulfillmentBox({ f }: { f: OrderFulfillment }) {
  const shipped = f.status === "shipped";
  return (
    <div>
      {/* Header */}
      <div
        className="flex items-center justify-between px-[18px] py-3.5"
        style={{
          background: shipped ? "#f5f5f7" : "rgba(194,65,12,0.05)",
          borderBottom: "1px solid rgba(0,0,0,0.06)",
        }}
      >
        <div className="flex items-center gap-2">
          {shipped ? (
            <span
              className="flex items-center justify-center rounded-full"
              style={{
                width: 22,
                height: 22,
                background: "#16a34a",
                color: "white",
              }}
            >
              <Check size={11} strokeWidth={2.4} />
            </span>
          ) : (
            <span
              className="rounded-full"
              style={{
                display: "inline-block",
                width: 22,
                height: 22,
                border: "1.5px solid rgba(0,0,0,0.25)",
              }}
            />
          )}
          <span className="text-[13.5px] font-semibold text-[#1d1d1f]">
            Fulfillment {f.idx} of {f.of} ·{" "}
            {f.status === "shipped" ? "shipped" : "awaiting restock"}
          </span>
        </div>
        {shipped && f.tracking ? (
          <span className="font-mono text-xs text-[#1d1d1f]/50">
            {f.tracking}
          </span>
        ) : (
          <button
            type="button"
            className="rounded-lg border border-black/[0.12] px-3 py-1.5 text-xs font-medium text-[#1d1d1f]"
          >
            Buy label
          </button>
        )}
      </div>

      {/* Body */}
      <div className="flex items-center gap-3 px-[18px] py-3.5">
        <ProdImg tone={f.productTone} />
        <div className="flex-1">
          <div className="text-[13.5px] font-semibold text-[#1d1d1f]">
            {f.productName}
          </div>
          <div className="flex items-center gap-2">
            <span className="text-xs text-[#1d1d1f]/50">
              {f.productSubtitle}
            </span>
            {f.restockNote && (
              <span className="inline-flex items-center rounded-full bg-orange-100 px-2 py-0.5 text-[10px] font-semibold text-orange-700">
                {f.restockNote}
              </span>
            )}
          </div>
        </div>
        <span className="font-mono text-[13.5px] font-semibold tabular-nums text-[#1d1d1f]">
          {money(f.price)}
        </span>
      </div>
    </div>
  );
}

export function OrderDetailFulfillments({
  fulfillments,
}: {
  fulfillments: OrderFulfillment[];
}) {
  return (
    <div
      className="overflow-hidden rounded-xl border border-black/[0.06] bg-white"
      style={{ padding: 0 }}
    >
      {fulfillments.map((f, i) => (
        <div
          key={f.idx}
          style={
            i > 0 ? { borderTop: "1px solid rgba(0,0,0,0.06)" } : undefined
          }
        >
          <FulfillmentBox f={f} />
        </div>
      ))}
    </div>
  );
}
