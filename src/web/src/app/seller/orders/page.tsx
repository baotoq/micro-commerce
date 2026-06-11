import { Plus, Upload } from "lucide-react";
import { connection } from "next/server";
import { OrdersInboxBulkBar } from "@/components/seller/orders/orders-inbox-bulk-bar";
import { OrdersInboxFilterRow } from "@/components/seller/orders/orders-inbox-filter-row";
import { OrdersInboxPagination } from "@/components/seller/orders/orders-inbox-pagination";
import { OrdersInboxTable } from "@/components/seller/orders/orders-inbox-table";
import { OrdersInboxTabs } from "@/components/seller/orders/orders-inbox-tabs";
import { SellerTopbar } from "@/components/seller/shell/seller-topbar";
import { Button } from "@/components/ui/button";
import {
  getOrderInbox,
  getOrderInboxSummary,
  getOrderInboxTabs,
} from "@/lib/seller/orders/data";

const SELECTED_COUNT = 3;

export default async function OrdersPage() {
  await connection();
  const [rows, tabs, summary] = await Promise.all([
    getOrderInbox(),
    getOrderInboxTabs(),
    getOrderInboxSummary(),
  ]);

  const selectedTotal = rows
    .slice(0, SELECTED_COUNT)
    .reduce((sum, r) => sum + r.total, 0);

  const subtitle = `${summary.totalLifetime} lifetime · ${summary.needAction} need action`;

  return (
    <div className="flex h-screen min-w-0 flex-col overflow-hidden">
      <SellerTopbar
        title="Orders"
        subtitle={subtitle}
        actions={
          <>
            <Button variant="outline" size="sm">
              <Upload size={11} aria-hidden />
              Export CSV
            </Button>
            <Button size="sm">
              <Plus size={12} aria-hidden />
              Manual order
            </Button>
          </>
        }
      />
      <OrdersInboxTabs tabs={tabs} />
      <OrdersInboxFilterRow />
      <OrdersInboxBulkBar
        selectedCount={SELECTED_COUNT}
        total={`$${selectedTotal}`}
      />
      <div className="flex-1 overflow-auto">
        <OrdersInboxTable rows={rows} selectedCount={SELECTED_COUNT} />
      </div>
      <OrdersInboxPagination showing="1 – 10" total={summary.totalLifetime} />
    </div>
  );
}
