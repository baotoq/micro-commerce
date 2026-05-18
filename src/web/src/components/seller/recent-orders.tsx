// web/src/components/seller/recent-orders.tsx

import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import { money } from "@/lib/money";
import type { Order } from "@/lib/seller/orders/types";

export function RecentOrders({ orders }: { orders: Order[] }) {
  return (
    <div className="rounded-lg border border-black/[0.06] bg-white p-5">
      <h2 className="text-base font-semibold tracking-tight">Recent orders</h2>
      <Table className="mt-4">
        <TableHeader>
          <TableRow>
            <TableHead>Order</TableHead>
            <TableHead>Customer</TableHead>
            <TableHead className="text-right">Items</TableHead>
            <TableHead className="text-right">Total</TableHead>
            <TableHead>Status</TableHead>
          </TableRow>
        </TableHeader>
        <TableBody>
          {orders.map((o) => (
            <TableRow key={o.id}>
              <TableCell className="font-mono">{o.id}</TableCell>
              <TableCell>{o.customer}</TableCell>
              <TableCell className="text-right">{o.items}</TableCell>
              <TableCell className="text-right">{money(o.total)}</TableCell>
              <TableCell className="capitalize">{o.status}</TableCell>
            </TableRow>
          ))}
        </TableBody>
      </Table>
    </div>
  );
}
