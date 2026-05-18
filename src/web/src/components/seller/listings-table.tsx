// web/src/components/seller/listings-table.tsx
"use client";

import { keepPreviousData, useQuery } from "@tanstack/react-query";
import { ChevronRightIcon, Loader2Icon } from "lucide-react";
import Link from "next/link";
import { type MouseEvent, useEffect, useRef, useState } from "react";
import {
  Pagination,
  PaginationContent,
  PaginationItem,
  PaginationLink,
  PaginationNext,
  PaginationPrevious,
} from "@/components/ui/pagination";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import { money } from "@/lib/money";
import { paginate } from "@/lib/pagination";
import type { Listing, ListingStatus } from "@/lib/seller/types";
import { cn } from "@/lib/utils";

const STATUS_STYLES: Record<Listing["status"], string> = {
  active: "bg-good/15 text-good",
  low: "bg-amber-50 text-amber-800",
  out: "bg-rose-50 text-rose-700",
  draft: "bg-zinc-100 text-zinc-700",
};

const TONE_BY_CATEGORY: Record<string, string> = {
  Vessels: "bg-[#E5C0A6]",
  Tableware: "bg-[#C9D4BC]",
  Drinkware: "bg-[#F0E8D7]",
};

type ProductPage = {
  items: Listing[];
  total: number;
  page: number;
  pageSize: number;
};

async function fetchListingsPage(
  page: number,
  pageSize: number,
  status: ListingStatus | undefined,
): Promise<ProductPage> {
  const params = new URLSearchParams({
    page: String(page),
    limit: String(pageSize),
  });
  if (status) params.set("status", status);
  const res = await fetch(`/api/listings?${params.toString()}`);
  if (!res.ok) throw new Error(`GET /api/listings failed: ${res.status}`);
  return (await res.json()) as ProductPage;
}

function buildPageHref(
  page: number,
  status: ListingStatus | undefined,
): string {
  const params = new URLSearchParams();
  if (status) params.set("status", status);
  if (page > 1) params.set("page", String(page));
  const qs = params.toString();
  return qs ? `?${qs}` : "?";
}

export function ListingsTable({
  listings,
  currentPage = 1,
  pageSize = 9,
  total,
  status,
}: {
  listings: Listing[];
  currentPage?: number;
  pageSize?: number;
  total?: number;
  status?: ListingStatus;
}) {
  const [page, setPage] = useState(currentPage);
  const [prevStatus, setPrevStatus] = useState(status);
  if (prevStatus !== status) {
    setPrevStatus(status);
    setPage(1);
  }

  const initialPage: ProductPage = {
    items: listings,
    total: total ?? listings.length,
    page: currentPage,
    pageSize,
  };

  // initialData applies only to the queryKey representing the server-rendered
  // bundle. When status or page diverges from the server values, drop it so
  // TanStack fetches fresh data for the new key.
  const seedStatusRef = useRef(status);
  const isSeedKey = page === currentPage && status === seedStatusRef.current;
  const initialDataUpdatedAt = useRef(Date.now()).current;

  const { data, isFetching } = useQuery<ProductPage>({
    queryKey: ["listings", page, pageSize, status ?? "all"],
    queryFn: () => fetchListingsPage(page, pageSize, status),
    initialData: isSeedKey ? initialPage : undefined,
    initialDataUpdatedAt: isSeedKey ? initialDataUpdatedAt : undefined,
    placeholderData: keepPreviousData,
    staleTime: 30_000,
  });

  const view = paginate(page, data?.total ?? 0, pageSize);
  const rows = data?.items ?? [];
  const totalRows = data?.total ?? 0;
  const disabledNav = "pointer-events-none opacity-40";

  const isFirstRender = useRef(true);
  useEffect(() => {
    if (isFirstRender.current) {
      isFirstRender.current = false;
      return;
    }
    const path = window.location.pathname;
    const href = buildPageHref(page, status);
    const target = href === "?" ? path : `${path}${href}`;
    window.history.replaceState(null, "", target);
  }, [page, status]);

  // Capture-phase handler runs before any bubble-phase listener (Base UI
  // composes its own onClick during bubble) so we can guarantee that the
  // browser's <a href> navigation is suppressed before anything else runs.
  function handleNav(target: number | null) {
    return (e: MouseEvent) => {
      e.preventDefault();
      e.stopPropagation();
      if (target == null || target === page) return;
      setPage(target);
    };
  }

  return (
    <div className="relative overflow-hidden rounded-lg border border-black/[0.06] bg-white">
      {isFetching && (
        <output className="pointer-events-none absolute inset-0 z-10 flex items-center justify-center bg-white/40 backdrop-blur-[1px]">
          <span className="inline-flex items-center gap-1.5 rounded-full bg-white px-3 py-1.5 text-xs font-medium text-foreground/70 shadow-sm ring-1 ring-black/[0.06]">
            <Loader2Icon
              className="size-3.5 animate-spin"
              data-testid="listings-loading-spinner"
            />
            Loading…
          </span>
        </output>
      )}
      <Table>
        <TableHeader>
          <TableRow>
            <TableHead className="w-8 pr-0" aria-label="Select" />
            <TableHead className="min-w-[240px]">Product</TableHead>
            <TableHead>SKU</TableHead>
            <TableHead>Status</TableHead>
            <TableHead className="text-right">Stock</TableHead>
            <TableHead className="text-right">Price</TableHead>
            <TableHead className="text-right">Views · 30d</TableHead>
            <TableHead className="text-right">Sales · 30d</TableHead>
            <TableHead className="w-8" aria-label="Open" />
          </TableRow>
        </TableHeader>
        <TableBody aria-busy={isFetching}>
          {rows.map((l) => {
            const sales = Math.max(0, Math.round(l.views7d / 28));
            const stockTone =
              l.inventory === 0
                ? "font-medium text-rose-700"
                : l.inventory < 5
                  ? "font-medium text-amber-700"
                  : "text-foreground";
            return (
              <TableRow key={l.sku}>
                <TableCell className="pr-0">
                  <span
                    className="block size-3.5 rounded-[3px] border-[1.5px] border-black/25"
                    aria-hidden
                  />
                </TableCell>
                <TableCell>
                  <div className="flex items-center gap-3">
                    <span
                      className={cn(
                        "block size-9 shrink-0 rounded-md",
                        TONE_BY_CATEGORY[l.category] ?? "bg-stone-200",
                      )}
                      aria-hidden
                    />
                    <span className="font-medium text-foreground">
                      {l.name}
                    </span>
                  </div>
                </TableCell>
                <TableCell className="font-mono text-foreground/70">
                  {l.sku}
                </TableCell>
                <TableCell>
                  <span
                    className={cn(
                      "inline-flex items-center gap-1.5 rounded-full px-2 py-0.5 text-xs font-medium",
                      STATUS_STYLES[l.status],
                    )}
                  >
                    <span
                      className="size-1.5 rounded-full bg-current opacity-60"
                      aria-hidden
                    />
                    {l.status.charAt(0).toUpperCase() + l.status.slice(1)}
                  </span>
                </TableCell>
                <TableCell className={cn("text-right tabular-nums", stockTone)}>
                  {l.inventory}
                </TableCell>
                <TableCell className="text-right font-medium tabular-nums text-foreground">
                  {money(l.price)}
                </TableCell>
                <TableCell className="text-right tabular-nums text-foreground/70">
                  {l.views7d.toLocaleString()}
                </TableCell>
                <TableCell className="text-right tabular-nums text-foreground/70">
                  {sales}
                </TableCell>
                <TableCell className="text-right">
                  <Link
                    href={`/seller/listings/${l.sku}/edit`}
                    aria-label={`Edit ${l.name}`}
                    className="inline-flex size-7 items-center justify-center rounded-md text-foreground/40 hover:bg-black/[0.04] hover:text-foreground"
                  >
                    <ChevronRightIcon className="size-3.5" />
                  </Link>
                </TableCell>
              </TableRow>
            );
          })}
        </TableBody>
      </Table>
      <div className="flex items-center justify-between border-t border-black/[0.06] bg-white px-4 py-3 text-xs">
        <span className="inline-flex items-center gap-1.5 text-foreground/60">
          {rows.length} of {totalRows} shown
          {isFetching && (
            <Loader2Icon
              className="size-3 animate-spin text-foreground/50"
              data-testid="listings-pagination-spinner"
              aria-label="Loading next page"
            />
          )}
        </span>
        <Pagination className="mx-0 w-auto justify-end">
          <PaginationContent>
            <PaginationItem>
              {view.prevPage ? (
                <PaginationPrevious
                  href={buildPageHref(view.prevPage, status)}
                  onClickCapture={handleNav(view.prevPage)}
                />
              ) : (
                <PaginationPrevious
                  aria-disabled
                  tabIndex={-1}
                  className={disabledNav}
                />
              )}
            </PaginationItem>
            {view.window.map((p) => (
              <PaginationItem key={p}>
                <PaginationLink
                  href={buildPageHref(p, status)}
                  isActive={p === view.page}
                  onClickCapture={handleNav(p)}
                >
                  {p}
                </PaginationLink>
              </PaginationItem>
            ))}
            <PaginationItem>
              {view.nextPage ? (
                <PaginationNext
                  href={buildPageHref(view.nextPage, status)}
                  onClickCapture={handleNav(view.nextPage)}
                />
              ) : (
                <PaginationNext
                  aria-disabled
                  tabIndex={-1}
                  className={disabledNav}
                />
              )}
            </PaginationItem>
          </PaginationContent>
        </Pagination>
      </div>
    </div>
  );
}
