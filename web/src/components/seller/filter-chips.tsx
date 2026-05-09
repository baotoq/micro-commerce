// web/src/components/seller/filter-chips.tsx
import type { ListingCounts } from "@/lib/seller/types";
import { cn } from "@/lib/utils";

export type FilterKey = "all" | "active" | "low" | "out" | "draft";

export function FilterChips({
  counts,
  active,
}: {
  counts: ListingCounts;
  active: FilterKey;
}) {
  const chips: { key: FilterKey; label: string; count: number }[] = [
    { key: "all",    label: "All",    count: counts.total },
    { key: "active", label: "Active", count: counts.active },
    { key: "low",    label: "Low",    count: counts.low },
    { key: "out",    label: "Out",    count: counts.out },
    { key: "draft",  label: "Drafts", count: counts.draft },
  ];
  return (
    <div className="flex flex-wrap gap-2">
      {chips.map((c) => {
        const isActive = c.key === active;
        return (
          <button
            key={c.key}
            type="button"
            aria-pressed={isActive}
            className={cn(
              "rounded-full border px-3 py-1 text-xs",
              isActive
                ? "border-[#0066cc] text-[#0066cc] ring-1 ring-[#0066cc]"
                : "border-black/[0.08] text-[#1d1d1f]",
            )}
          >
            {c.label} · {c.count}
          </button>
        );
      })}
    </div>
  );
}
