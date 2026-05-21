import Link from "next/link";
import type { FilterKey } from "@/components/seller/listings/filter-chips";
import { ListingsBrowser } from "@/components/seller/listings/listings-browser";
import { SellerTopbar } from "@/components/seller/shell/seller-topbar";
import { Button, buttonVariants } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { getListingCounts, getListings } from "@/lib/seller/listings/data";
import {
  LISTINGS_PAGE_SIZE,
  type ListingStatus,
} from "@/lib/seller/listings/types";
import { cn } from "@/lib/utils";

const FILTER_KEYS: readonly FilterKey[] = [
  "all",
  "active",
  "low",
  "out",
  "draft",
];

function parseFilter(raw: string | undefined): FilterKey {
  return FILTER_KEYS.find((k) => k === raw) ?? "all";
}

const ICON_PATH = {
  upload: "M10 14V3m-4 4l4-4 4 4M3 16h14",
  plus: "M10 4v12M4 10h12",
  search: "M9 3a6 6 0 1 1 0 12A6 6 0 0 1 9 3zm5 10l3.5 3.5",
  filter: "M3 5h14M5 10h10M8 15h4",
  sort: "M6 4v12m0 0l-3-3m3 3l3-3M14 16V4m0 0l-3 3m3-3l3 3",
} as const;

function Ico({
  name,
  className,
}: {
  name: keyof typeof ICON_PATH;
  className?: string;
}) {
  return (
    <svg
      className={className}
      width="14"
      height="14"
      viewBox="0 0 20 20"
      fill="none"
      stroke="currentColor"
      strokeWidth="1.6"
      strokeLinecap="round"
      strokeLinejoin="round"
      aria-hidden="true"
    >
      <title>{name}</title>
      <path d={ICON_PATH[name]} />
    </svg>
  );
}

export default async function ListingsPage({
  searchParams,
}: {
  searchParams: Promise<{ page?: string; status?: string }>;
}) {
  const { page: pageParam, status: statusParam } = await searchParams;
  const page = Math.max(1, Number(pageParam ?? "1") || 1);
  const filter = parseFilter(statusParam);
  const status: ListingStatus | undefined =
    filter === "all" ? undefined : filter;
  const [counts, listings] = await Promise.all([
    getListingCounts(),
    getListings({ page, limit: LISTINGS_PAGE_SIZE, status }),
  ]);
  return (
    <section>
      <SellerTopbar
        title="Listings"
        subtitle={`${counts.total} products · ${counts.active} active`}
        actions={
          <>
            <Button variant="outline" className="gap-1.5 rounded-full">
              <Ico name="upload" />
              Import CSV
            </Button>
            <Link
              href="/seller/listings/new"
              className={cn(buttonVariants(), "gap-1.5 rounded-full")}
            >
              <Ico name="plus" />
              New listing
            </Link>
          </>
        }
      />

      <ListingsBrowser
        counts={counts}
        initialStatus={filter}
        initialListings={listings.items}
        initialPage={listings.page}
        initialTotal={listings.total}
        pageSize={listings.pageSize}
        filterRowClassName="flex flex-wrap items-center justify-between gap-3 border-b border-black/[0.06] bg-white px-7 py-3.5"
        tableWrapperClassName="px-7 py-6"
        rightSlot={
          <div className="flex items-center gap-2">
            <div className="relative">
              <Ico
                name="search"
                className="pointer-events-none absolute left-2.5 top-1/2 -translate-y-1/2 text-foreground/50"
              />
              <Input
                type="search"
                placeholder="Search products…"
                aria-label="Search products"
                className="h-8 w-56 rounded-full bg-canvas-parchment pl-8 text-xs"
              />
            </div>
            <Button
              variant="outline"
              size="sm"
              className="h-8 gap-1.5 rounded-full text-xs"
            >
              <Ico name="filter" />
              Collection
            </Button>
            <Button
              variant="outline"
              size="sm"
              className="h-8 gap-1.5 rounded-full text-xs"
            >
              <Ico name="sort" />
              Best-selling
            </Button>
          </div>
        }
      />
    </section>
  );
}
