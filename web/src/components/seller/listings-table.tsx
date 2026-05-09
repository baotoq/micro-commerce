// web/src/components/seller/listings-table.tsx
import type { Listing } from "@/lib/seller/types";
import { money } from "@/lib/money";
import { cn } from "@/lib/utils";
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table";

const STATUS_STYLES: Record<Listing["status"], string> = {
  active: "bg-emerald-50 text-emerald-700",
  low:    "bg-amber-50 text-amber-800",
  out:    "bg-rose-50 text-rose-700",
  draft:  "bg-zinc-100 text-zinc-700",
};

export function ListingsTable({
  listings,
  pageSize = 9,
}: {
  listings: Listing[];
  pageSize?: number;
}) {
  const visible = Math.min(pageSize, listings.length);
  const rows = listings.slice(0, visible);
  return (
    <div className="rounded-lg border border-black/[0.06] bg-white">
      <Table>
        <TableHeader>
          <TableRow>
            <TableHead>SKU</TableHead>
            <TableHead>Product</TableHead>
            <TableHead>Category</TableHead>
            <TableHead className="text-right">Price</TableHead>
            <TableHead className="text-right">Inv.</TableHead>
            <TableHead>Status</TableHead>
            <TableHead className="text-right">Views (7d)</TableHead>
          </TableRow>
        </TableHeader>
        <TableBody>
          {rows.map((l) => (
            <TableRow key={l.sku}>
              <TableCell className="font-mono">{l.sku}</TableCell>
              <TableCell>{l.name}</TableCell>
              <TableCell>{l.category}</TableCell>
              <TableCell className="text-right">{money(l.price)}</TableCell>
              <TableCell className="text-right">{l.inventory}</TableCell>
              <TableCell>
                <span className={cn("rounded-full px-2 py-0.5 text-xs capitalize", STATUS_STYLES[l.status])}>
                  {l.status}
                </span>
              </TableCell>
              <TableCell className="text-right">{l.views7d}</TableCell>
            </TableRow>
          ))}
        </TableBody>
      </Table>
      <div className="border-t border-black/[0.06] px-4 py-3 text-xs text-[#1d1d1f]/60">
        {visible} of {listings.length} shown
      </div>
    </div>
  );
}
