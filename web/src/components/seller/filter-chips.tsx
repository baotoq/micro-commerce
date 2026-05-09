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
    { key: "all", label: "All", count: counts.total },
    { key: "active", label: "Active", count: counts.active },
    { key: "low", label: "Low", count: counts.low },
    { key: "out", label: "Out", count: counts.out },
    { key: "draft", label: "Drafts", count: counts.draft },
  ];
  return (
    <div className="flex flex-wrap items-center gap-1.5">
      {chips.map((c) => {
        const isActive = c.key === active;
        return (
          <button
            key={c.key}
            type="button"
            aria-pressed={isActive}
            className={cn(
              "rounded-full border px-3 py-1 text-xs font-medium transition-colors",
              isActive
                ? "border-[#1d1d1f] bg-[#1d1d1f] text-white"
                : "border-black/[0.08] bg-white text-[#1d1d1f]/80 hover:border-black/20 hover:text-[#1d1d1f]",
            )}
          >
            {c.label} · {c.count}
          </button>
        );
      })}
    </div>
  );
}
