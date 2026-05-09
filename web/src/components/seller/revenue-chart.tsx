// web/src/components/seller/revenue-chart.tsx
import type { RevenuePoint } from "@/lib/seller/types";

export function RevenueChart({ points }: { points: RevenuePoint[] }) {
  const max = Math.max(...points.map((p) => p.amount), 1);
  const w = 600;
  const h = 180;
  const barW = w / points.length / 2;
  return (
    <div className="rounded-lg border border-black/[0.06] bg-white p-5">
      <h2 className="text-base font-semibold tracking-tight">Revenue</h2>
      <svg
        viewBox={`0 0 ${w} ${h + 30}`}
        role="img"
        aria-label="Revenue last 7 days"
        className="mt-4 w-full"
      >
        {points.map((p, i) => {
          const barH = Math.round((p.amount / max) * h);
          const x = (i + 0.25) * (w / points.length);
          return (
            <g key={p.day}>
              <rect
                x={x}
                y={h - barH}
                width={barW}
                height={barH}
                rx={3}
                fill="#1d1d1f"
              />
              <text
                x={x + barW / 2}
                y={h + 18}
                textAnchor="middle"
                fontSize="11"
                fill="#1d1d1f99"
              >
                {p.day}
              </text>
            </g>
          );
        })}
      </svg>
    </div>
  );
}
