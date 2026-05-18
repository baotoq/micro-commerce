import { Plus, Upload } from "lucide-react";
import { OrdersInboxBulkBar } from "@/components/seller/orders-inbox-bulk-bar";
import { OrdersInboxFilterRow } from "@/components/seller/orders-inbox-filter-row";
import { OrdersInboxPagination } from "@/components/seller/orders-inbox-pagination";
import { OrdersInboxTable } from "@/components/seller/orders-inbox-table";
import { OrdersInboxTabs } from "@/components/seller/orders-inbox-tabs";
import { SellerTopbar } from "@/components/seller/seller-topbar";
import {
  getOrderInbox,
  getOrderInboxSummary,
  getOrderInboxTabs,
} from "@/lib/seller/orders/data";

const SELECTED_COUNT = 3;

export default function OrdersPage() {
  const rows = getOrderInbox();
  const tabs = getOrderInboxTabs();
  const summary = getOrderInboxSummary();

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
            <button
              type="button"
              className="inline-flex items-center gap-1.5 rounded-lg border border-black/[0.12] px-3 py-1.5 text-[12px] font-medium text-[#1d1d1f] hover:bg-black/[0.03]"
            >
              <Upload size={11} aria-hidden />
              Export CSV
            </button>
            <button
              type="button"
              className="inline-flex items-center gap-1.5 rounded-lg bg-[#1d1d1f] px-3 py-1.5 text-[12px] font-medium text-white hover:bg-[#1d1d1f]/90"
            >
              <Plus size={12} aria-hidden />
              Manual order
            </button>
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
