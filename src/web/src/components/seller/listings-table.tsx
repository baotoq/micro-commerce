// web/src/components/seller/listings-table.tsx

import { ChevronRightIcon } from "lucide-react";
import Link from "next/link";
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
import { pageHref, paginate } from "@/lib/pagination";
import type { Listing } from "@/lib/seller/types";
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

export function ListingsTable({
  listings,
  currentPage = 1,
  pageSize = 9,
  total,
}: {
  listings: Listing[];
  currentPage?: number;
  pageSize?: number;
  total?: number;
}) {
  const totalRows = total ?? listings.length;
  const view = paginate(currentPage, totalRows, pageSize);
  const rows = listings;
  const disabledNav = "pointer-events-none opacity-40";

  return (
    <div className="overflow-hidden rounded-lg border border-black/[0.06] bg-white">
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
        <TableBody>
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
        <span className="text-foreground/60">
          {rows.length} of {totalRows} shown
        </span>
        <Pagination className="mx-0 w-auto justify-end">
          <PaginationContent>
            <PaginationItem>
              {view.prevPage ? (
                <PaginationPrevious href={pageHref(view.prevPage)} />
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
                <PaginationLink href={pageHref(p)} isActive={p === view.page}>
                  {p}
                </PaginationLink>
              </PaginationItem>
            ))}
            <PaginationItem>
              {view.nextPage ? (
                <PaginationNext href={pageHref(view.nextPage)} />
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
