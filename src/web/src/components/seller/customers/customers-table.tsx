import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import type { CustomerRow } from "@/lib/seller/customers/types";

function Avatar({ name }: { name: string }) {
  return (
    <span className="flex size-7 shrink-0 items-center justify-center rounded-full bg-surface-avatar-warm text-[11px] font-semibold text-foreground">
      {name.charAt(0)}
    </span>
  );
}

function Tag({ label }: { label: string }) {
  return (
    <span className="inline-flex items-center rounded-full bg-black/[0.04] px-2.5 py-1 text-[11px] font-semibold text-muted-foreground">
      {label}
    </span>
  );
}

export function CustomersTable({ rows }: { rows: CustomerRow[] }) {
  return (
    <Table className="min-w-[820px]">
      <TableHeader>
        <TableRow className="text-[11px] font-medium text-muted-foreground">
          <TableHead className="pl-7 pr-4">Customer</TableHead>
          <TableHead className="pr-4">City</TableHead>
          <TableHead className="pr-4">Tags</TableHead>
          <TableHead className="pr-4 text-right">Lifetime spend</TableHead>
          <TableHead className="pr-4 text-right">Orders</TableHead>
          <TableHead className="pr-7">Last order</TableHead>
        </TableRow>
      </TableHeader>
      <TableBody>
        {rows.map((row) => (
          <TableRow key={row.email}>
            <TableCell className="py-3 pl-7 pr-4">
              <div className="flex items-center gap-2.5">
                <Avatar name={row.name} />
                <div>
                  <div className="text-[12.5px] font-semibold text-foreground">
                    {row.name}
                  </div>
                  <div className="text-[11px] text-muted-foreground">
                    {row.email}
                  </div>
                </div>
              </div>
            </TableCell>
            <TableCell className="py-3 pr-4 text-[12px] text-muted-foreground">
              {row.city}
            </TableCell>
            <TableCell className="py-3 pr-4">
              {row.tags.length > 0 ? (
                <div className="flex flex-wrap items-center gap-1.5">
                  {row.tags.map((tag) => (
                    <Tag key={tag} label={tag} />
                  ))}
                </div>
              ) : (
                <span className="text-[12px] text-muted-foreground">—</span>
              )}
            </TableCell>
            <TableCell className="py-3 pr-4 text-right tabular-nums text-[13px] font-semibold text-foreground">
              {row.lifetimeSpend}
            </TableCell>
            <TableCell className="py-3 pr-4 text-right tabular-nums text-[12.5px] text-foreground">
              {row.lifetimeOrderCount}
            </TableCell>
            <TableCell className="py-3 pr-7 text-[11px] text-muted-foreground">
              {row.lastOrder}
            </TableCell>
          </TableRow>
        ))}
      </TableBody>
    </Table>
  );
}
