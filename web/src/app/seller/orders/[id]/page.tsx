import { ChevronLeft, MessageSquare } from "lucide-react";
import Link from "next/link";
import { notFound } from "next/navigation";
import { OrderDetailCustomer } from "@/components/seller/order-detail-customer";
import { OrderDetailFulfillments } from "@/components/seller/order-detail-fulfillments";
import { OrderDetailInternalNote } from "@/components/seller/order-detail-internal-note";
import { OrderDetailRefund } from "@/components/seller/order-detail-refund";
import { OrderDetailSummary } from "@/components/seller/order-detail-summary";
import { OrderDetailTimeline } from "@/components/seller/order-detail-timeline";
import { getOrderDetailFull } from "@/lib/seller/data";

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
            className="flex items-center justify-center rounded-lg p-1 text-[#1d1d1f]/40 hover:bg-black/[0.04]"
            aria-label="Back to orders"
          >
            <ChevronLeft size={14} />
          </Link>
          <Link href="/seller/orders" className="text-xs text-[#1d1d1f]/50">
            Orders /
          </Link>
          <span className="font-mono text-sm font-semibold text-[#1d1d1f]">
            {order.id}
          </span>
          <span className="inline-flex items-center gap-1.5 rounded-full bg-orange-100 px-2.5 py-1 text-[11px] font-semibold text-orange-700">
            <span className="inline-block h-1.5 w-1.5 rounded-full bg-orange-500" />
            {order.status}
          </span>
          <span className="text-[11px] text-[#1d1d1f]/50">
            · {order.customerShort} · {order.age}
          </span>
        </div>
        <div className="flex items-center gap-2">
          <button
            type="button"
            className="flex items-center gap-1.5 rounded-lg border border-black/[0.12] px-3 py-1.5 text-xs font-medium text-[#1d1d1f]"
          >
            <MessageSquare size={11} />
            Message
          </button>
          <button
            type="button"
            className="rounded-lg border border-black/[0.12] px-3 py-1.5 text-xs font-medium text-[#1d1d1f]"
          >
            Print slip
          </button>
          <button
            type="button"
            className="rounded-lg px-3 py-1.5 text-xs font-medium text-red-600 hover:bg-red-50"
          >
            Cancel order
          </button>
          <button
            type="button"
            className="rounded-xl bg-[#1d1d1f] px-3 py-1.5 text-xs font-semibold text-white"
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
