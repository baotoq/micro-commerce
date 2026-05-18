// web/src/components/seller/filter-chips.tsx
"use client";

import type { MouseEvent } from "react";
import { Badge } from "@/components/ui/badge";
import type { ListingCounts } from "@/lib/seller/listings/types";

export type FilterKey = "all" | "active" | "low" | "out" | "draft";

const LISTINGS_PATH = "/seller/listings";

export function chipHref(key: FilterKey): string {
  return key === "all" ? LISTINGS_PATH : `${LISTINGS_PATH}?status=${key}`;
}

export function FilterChips({
  counts,
  active,
  onSelect,
}: {
  counts: ListingCounts;
  active: FilterKey;
  onSelect: (key: FilterKey) => void;
}) {
  const chips: { key: FilterKey; label: string; count: number }[] = [
    { key: "all", label: "All", count: counts.total },
    { key: "active", label: "Active", count: counts.active },
    { key: "low", label: "Low", count: counts.low },
    { key: "out", label: "Out", count: counts.out },
    { key: "draft", label: "Drafts", count: counts.draft },
  ];

  function handleClick(key: FilterKey) {
    return (e: MouseEvent<HTMLAnchorElement>) => {
      if (e.button !== 0 || e.metaKey || e.ctrlKey || e.shiftKey || e.altKey) {
        return;
      }
      e.preventDefault();
      onSelect(key);
    };
  }

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
              // biome-ignore lint/a11y/useAnchorContent: Badge composes label/count children via useRender. <a> intentionally empty at source.
              <a
                href={chipHref(c.key)}
                aria-current={isActive ? "page" : undefined}
                onClick={handleClick(c.key)}
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
