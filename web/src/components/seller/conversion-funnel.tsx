// web/src/components/seller/conversion-funnel.tsx
import type { FunnelStage } from "@/lib/seller/types";

export function ConversionFunnel({ stages }: { stages: FunnelStage[] }) {
  const top = stages[0]?.count || 1;
  return (
    <div className="rounded-lg border border-black/[0.06] bg-white p-5">
      <h2 className="text-base font-semibold tracking-tight">Conversion funnel</h2>
      <ul className="mt-4 space-y-3">
        {stages.map((s) => {
          const pct = (s.count / top) * 100;
          return (
            <li key={s.label}>
              <div className="flex items-baseline justify-between text-sm">
                <span>{s.label}</span>
                <span className="tabular-nums text-[#1d1d1f]/70">
                  {s.count.toLocaleString("en-US")}
                </span>
              </div>
              <div className="mt-1 h-2 w-full rounded-full bg-black/[0.05]">
                <div
                  className="h-2 rounded-full bg-[#1d1d1f]"
                  style={{ width: `${pct}%` }}
                  aria-hidden
                />
              </div>
            </li>
          );
        })}
      </ul>
    </div>
  );
}
