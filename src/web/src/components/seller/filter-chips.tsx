// web/src/components/seller/filter-chips.tsx
import Link from "next/link";
import { Badge } from "@/components/ui/badge";
import type { ListingCounts } from "@/lib/seller/types";

export type FilterKey = "all" | "active" | "low" | "out" | "draft";

const LISTINGS_PATH = "/seller/listings";

function chipHref(key: FilterKey): string {
  return key === "all" ? LISTINGS_PATH : `${LISTINGS_PATH}?status=${key}`;
}

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
          <Badge
            key={c.key}
            variant={isActive ? "default" : "outline"}
            className="h-7 cursor-pointer px-3 py-1 text-xs"
            render={
              <Link
                href={chipHref(c.key)}
                aria-current={isActive ? "page" : undefined}
              />
            }
          >
            {c.label} · {c.count}
          </Badge>
        );
      })}
    </div>
  );
}
