// web/src/components/seller/today-panel.tsx
import type { TodayItem } from "@/lib/seller/dashboard/types";

export function TodayPanel({ items }: { items: TodayItem[] }) {
  return (
    <div className="rounded-lg border border-black/[0.06] bg-white p-5">
      <h2 className="text-base font-semibold tracking-tight">Today</h2>
      <ul className="mt-4 space-y-2 text-sm">
        {items.map((it) => (
          <li key={it.label} className="flex items-start gap-2">
            <span className="mt-1.5 inline-block h-1.5 w-1.5 rounded-full bg-[#0066cc]" />
            <span>{it.label}</span>
          </li>
        ))}
      </ul>
    </div>
  );
}
