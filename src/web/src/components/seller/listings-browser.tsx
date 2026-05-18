// web/src/components/seller/listings-browser.tsx
"use client";

import { useCallback, useEffect, useState } from "react";
import {
  chipHref,
  FilterChips,
  type FilterKey,
} from "@/components/seller/filter-chips";
import { ListingsTable } from "@/components/seller/listings-table";
import type { Listing, ListingCounts, ListingStatus } from "@/lib/seller/types";

const FILTER_KEYS: readonly FilterKey[] = [
  "all",
  "active",
  "low",
  "out",
  "draft",
];

function parseFilter(raw: string | null): FilterKey {
  return FILTER_KEYS.find((k) => k === raw) ?? "all";
}

function toApiStatus(key: FilterKey): ListingStatus | undefined {
  return key === "all" ? undefined : key;
}

export function ListingsBrowser({
  counts,
  initialStatus,
  initialListings,
  initialPage,
  pageSize,
  initialTotal,
  chipsSlot: _chipsSlot,
  rightSlot,
  tableWrapperClassName,
  filterRowClassName,
}: {
  counts: ListingCounts;
  initialStatus: FilterKey;
  initialListings: Listing[];
  initialPage: number;
  pageSize: number;
  initialTotal: number;
  chipsSlot?: never;
  rightSlot: React.ReactNode;
  tableWrapperClassName: string;
  filterRowClassName: string;
}) {
  const [status, setStatus] = useState<FilterKey>(initialStatus);

  useEffect(() => {
    function onPopState() {
      const params = new URLSearchParams(window.location.search);
      setStatus(parseFilter(params.get("status")));
    }
    window.addEventListener("popstate", onPopState);
    return () => window.removeEventListener("popstate", onPopState);
  }, []);

  const handleSelect = useCallback((next: FilterKey) => {
    setStatus(next);
    window.history.replaceState(null, "", chipHref(next));
  }, []);

  return (
    <>
      <div className={filterRowClassName}>
        <FilterChips counts={counts} active={status} onSelect={handleSelect} />
        {rightSlot}
      </div>
      <div className={tableWrapperClassName}>
        <ListingsTable
          listings={initialListings}
          currentPage={initialPage}
          pageSize={pageSize}
          total={initialTotal}
          status={toApiStatus(status)}
        />
      </div>
    </>
  );
}
