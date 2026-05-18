// web/src/components/seller/sources-donut.tsx
import type { SourceBreakdown } from "@/lib/seller/analytics/types";

const PALETTE = [
  "var(--foreground)",
  "var(--terra)",
  "#7a7a7a",
  "#cccccc",
  "#aacbe7",
];

export function SourcesDonut({ sources }: { sources: SourceBreakdown[] }) {
  const total = sources.reduce((s, x) => s + x.visits, 0);
  let acc = 0;
  const r = 60;
  const c = 2 * Math.PI * r;

  return (
    <div className="rounded-lg border border-black/[0.06] bg-white p-5">
      <h2 className="text-base font-semibold tracking-tight">Sources</h2>
      <div className="mt-4 flex items-center gap-6">
        <svg
          viewBox="-80 -80 160 160"
          className="h-40 w-40"
          role="img"
          aria-label="Traffic sources breakdown"
        >
          {sources.map((s, i) => {
            const len = s.share * c;
            const dasharray = `${len} ${c - len}`;
            const dashoffset = -acc;
            acc += len;
            return (
              <circle
                key={s.name}
                r={r}
                cx={0}
                cy={0}
                fill="transparent"
                stroke={PALETTE[i % PALETTE.length]}
                strokeWidth={18}
                strokeDasharray={dasharray}
                strokeDashoffset={dashoffset}
                transform="rotate(-90)"
              />
            );
          })}
        </svg>
        <ul className="space-y-2 text-sm">
          {sources.map((s, i) => (
            <li key={s.name} className="flex items-center gap-3">
              <span
                aria-hidden
                className="inline-block h-2.5 w-2.5 rounded-full"
                style={{ background: PALETTE[i % PALETTE.length] }}
              />
              <span className="min-w-[8rem]">{s.name}</span>
              <span className="tabular-nums text-muted-foreground">
                {s.visits.toLocaleString("en-US")}
              </span>
            </li>
          ))}
        </ul>
      </div>
      <p className="mt-3 text-xs text-muted-foreground">
        {total.toLocaleString("en-US")} visits
      </p>
    </div>
  );
}
