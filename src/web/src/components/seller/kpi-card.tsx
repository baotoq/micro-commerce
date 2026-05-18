// web/src/components/seller/kpi-card.tsx

import { money } from "@/lib/money";
import type { KpiPoint } from "@/lib/seller/analytics/types";

function format(value: number, fmt: KpiPoint["format"]) {
  if (fmt === "currency") return money(value);
  if (fmt === "percent") return `${value.toFixed(1)}%`;
  return value.toLocaleString("en-US");
}

export function KpiCard({ kpi }: { kpi: KpiPoint }) {
  const positive = (kpi.delta ?? 0) >= 0;
  return (
    <div className="rounded-lg border border-black/[0.06] bg-white p-5">
      <div className="text-[13px] text-foreground/70">{kpi.label}</div>
      <div className="mt-1 text-2xl font-semibold tracking-tight">
        {format(kpi.value, kpi.format)}
      </div>
      {kpi.delta !== undefined && (
        <div
          className={
            positive
              ? "mt-1 text-xs text-good"
              : "mt-1 text-xs text-foreground/60"
          }
        >
          {positive ? "▲" : "▼"} {Math.abs(kpi.delta)}%
        </div>
      )}
    </div>
  );
}
