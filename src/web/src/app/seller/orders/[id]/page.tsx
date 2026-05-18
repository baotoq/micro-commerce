import { ChevronLeft, MessageSquare, X } from "lucide-react";
import Link from "next/link";
import { notFound } from "next/navigation";
import { OrderDetailCustomer } from "@/components/seller/orders/order-detail-customer";
import { OrderDetailFulfillments } from "@/components/seller/orders/order-detail-fulfillments";
import { OrderDetailInternalNote } from "@/components/seller/orders/order-detail-internal-note";
import { OrderDetailRefund } from "@/components/seller/orders/order-detail-refund";
import { OrderDetailSummary } from "@/components/seller/orders/order-detail-summary";
import { OrderDetailTimeline } from "@/components/seller/orders/order-detail-timeline";
import { getOrderDetailFull } from "@/lib/seller/orders/data";

export default async function OrderDetailPage({
  params,
}: {
  params: Promise<{ id: string }>;
}) {
  const { id } = await params;
  const order = getOrderDetailFull(id);
  if (!order) notFound();

  return (
    <div className="flex h-screen min-w-0 flex-col overflow-hidden">
      {/* Breadcrumb header */}
      <div className="flex items-center justify-between border-b border-black/[0.06] px-7 py-3.5">
        <div className="flex items-center gap-2">
          <Link
            href="/seller/orders"
            className="flex items-center justify-center rounded-lg p-1 text-muted-foreground hover:bg-black/[0.04]"
            aria-label="Back to orders"
          >
            <ChevronLeft size={14} />
          </Link>
          <Link href="/seller/orders" className="text-xs text-muted-foreground">
            Orders /
          </Link>
          <span className="font-mono text-sm font-semibold text-foreground">
            {order.id}
          </span>
          <span className="inline-flex items-center gap-1.5 rounded-full bg-warn/15 px-2.5 py-1 text-[11px] font-semibold text-warn">
            <span className="inline-block h-1.5 w-1.5 rounded-full bg-warn" />
            {order.status}
          </span>
          <span className="text-[11px] text-muted-foreground">
            · {order.customerShort} · {order.age}
          </span>
        </div>
        <div className="flex items-center gap-2">
          <button
            type="button"
            className="flex items-center gap-1.5 rounded-lg border border-black/[0.12] px-3 py-1.5 text-xs font-medium text-foreground"
          >
            <MessageSquare size={11} />
            Message
          </button>
          <button
            type="button"
            className="rounded-lg border border-black/[0.12] px-3 py-1.5 text-xs font-medium text-foreground"
          >
            Print slip
          </button>
          <button
            type="button"
            className="flex items-center gap-1.5 rounded-lg px-3 py-1.5 text-xs font-medium text-red-600 hover:bg-red-50"
          >
            <X size={11} />
            Cancel order
          </button>
          <button
            type="button"
            className="rounded-lg bg-foreground px-3 py-1.5 text-xs font-semibold text-white"
          >
            Buy label · {order.outstandingProductName}
          </button>
        </div>
      </div>

      {/* Body */}
      <div className="flex-1 overflow-auto px-7 py-5">
        <div
          className="grid gap-3"
          style={{ gridTemplateColumns: "1.6fr 1fr", alignItems: "start" }}
        >
          {/* Left column */}
          <div className="flex flex-col gap-3">
            <OrderDetailFulfillments fulfillments={order.fulfillments} />
            <OrderDetailRefund refund={order.refund} />
            <OrderDetailTimeline timeline={order.timeline} />
          </div>

          {/* Right column */}
          <div className="flex flex-col gap-3">
            <OrderDetailCustomer customer={order.customer} />
            <OrderDetailSummary summary={order.summary} />
            <OrderDetailInternalNote
              note={order.internalNote}
              tags={order.customerTags}
              activeTags={order.customerTagsActive}
            />
          </div>
        </div>
      </div>
    </div>
  );
}
