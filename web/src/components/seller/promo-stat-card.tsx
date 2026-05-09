import { Sparkline } from "@/components/seller/sparkline";
import type { PromoStat } from "@/lib/seller/types";

export function PromoStatCard({ stat }: { stat: PromoStat }) {
  return (
    <div className="rounded-xl border border-black/[0.06] bg-white p-4">
      <div className="text-xs text-[#1d1d1f]/50">{stat.label}</div>
      <div
        className="mt-1.5 font-semibold tabular-nums leading-none"
        style={{ fontSize: 26 }}
      >
        {stat.value}
      </div>
      <div className="mt-1 text-xs text-[#1d1d1f]/50">{stat.sub}</div>
      <div className="mt-2 h-6 text-[#0066cc]">
        <Sparkline points={stat.spark} className="h-6 w-full text-[#0066cc]" />
      </div>
    </div>
  );
}
