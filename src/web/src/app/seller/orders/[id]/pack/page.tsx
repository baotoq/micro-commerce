import { X } from "lucide-react";
import Link from "next/link";
import { notFound } from "next/navigation";
import { getOrderDetail, getShippingOptions } from "@/lib/seller/orders/data";

export default async function PackShipPage(props: {
  params: Promise<{ id: string }>;
}) {
  const { id } = await props.params;
  const order = getOrderDetail(id);
  if (!order) notFound();

  const shippingOptions = getShippingOptions();

  return (
    <div className="flex h-screen min-w-0 flex-col overflow-hidden">
      {/* Topbar breadcrumb */}
      <div className="flex items-center justify-between border-b border-black/[0.06] px-6 py-3.5">
        <div className="flex items-center gap-3">
          <Link href="/seller/orders" className="text-sm text-muted-foreground">
            Orders /
          </Link>
          <span className="font-mono text-sm text-muted-foreground">
            {order.id}
          </span>
          <span className="flex items-center gap-1.5 rounded-full bg-warn/15 px-2.5 py-1 text-[11px] font-semibold text-warn">
            <span className="inline-block h-1.5 w-1.5 rounded-full bg-warn" />
            Needs shipping
          </span>
        </div>
        <div className="flex gap-2">
          <button
            type="button"
            className="rounded-lg border border-black/[0.12] px-3 py-1.5 text-xs font-medium text-foreground"
          >
            Message Sasha
          </button>
          <button
            type="button"
            className="rounded-lg border border-black/[0.12] px-3 py-1.5 text-xs font-medium text-foreground"
          >
            Print packing slip
          </button>
        </div>
      </div>

      {/* Main content */}
      <div className="flex-1 overflow-auto px-7 py-6">
        <h2 className="mb-3.5 text-[26px] font-semibold tracking-tight">
          {order.customer} · {order.productName}
        </h2>

        <div
          className="grid gap-3"
          style={{ gridTemplateColumns: "1.6fr 1fr" }}
        >
          {/* Left: product + financials */}
          <div className="overflow-hidden rounded-xl border border-black/[0.06] bg-white">
            <div className="flex items-center gap-3 px-[18px] py-3.5">
              {/* Product colour swatch — demo placeholder, no product colour in data */}
              <div className="h-[60px] w-[60px] shrink-0 rounded-lg bg-terra opacity-[0.18]" />
              <div className="flex-1">
                <div className="text-sm font-semibold">{order.productName}</div>
                <div className="text-xs text-muted-foreground">
                  {order.productSubtitle}
                </div>
              </div>
              <span className="tabular-nums font-semibold">
                ${order.subtotal.toFixed(2)}
              </span>
            </div>

            <div className="border-t border-black/[0.06]" />

            <div className="flex flex-col gap-2 px-[18px] py-3.5">
              <div className="flex justify-between text-sm">
                <span className="text-muted-foreground">Subtotal</span>
                <span className="tabular-nums">
                  ${order.subtotal.toFixed(2)}
                </span>
              </div>
              <div className="flex justify-between text-sm">
                <span className="text-muted-foreground">
                  Shipping · {order.shippingLabel}
                </span>
                <span className="tabular-nums">
                  ${order.shippingCost.toFixed(2)}
                </span>
              </div>
              <div className="flex justify-between">
                <span className="text-sm font-semibold">Customer paid</span>
                <span className="text-[18px] font-semibold tabular-nums">
                  ${order.customerPaid.toFixed(2)}
                </span>
              </div>
              <div
                className="flex justify-between border-t pt-2 text-sm"
                style={{
                  borderTopStyle: "dashed",
                  borderColor: "rgba(0,0,0,0.1)",
                  marginTop: 6,
                }}
              >
                <span className="text-muted-foreground">
                  Micro fee · {order.feePct}%
                </span>
                <span className="tabular-nums">−${order.fee.toFixed(2)}</span>
              </div>
              <div className="flex justify-between text-sm">
                <span className="text-muted-foreground">You'll receive</span>
                <span className="font-semibold tabular-nums text-good">
                  ${order.net.toFixed(2)}
                </span>
              </div>
            </div>
          </div>

          {/* Right: ship-to + note */}
          <div className="flex flex-col gap-3">
            <div className="rounded-xl border border-black/[0.06] bg-white p-4">
              <div className="mb-1.5 text-xs text-muted-foreground">
                Ship to
              </div>
              <div className="text-sm font-semibold">{order.shipTo.name}</div>
              <div className="mt-0.5 text-sm text-muted-foreground">
                {order.shipTo.line1}
                <br />
                {order.shipTo.cityState}
              </div>
            </div>

            <div className="rounded-xl border-none bg-canvas-parchment p-4">
              <div className="mb-1 text-xs text-muted-foreground">
                Customer note
              </div>
              <div className="text-sm">"{order.customerNote}"</div>
            </div>
          </div>
        </div>
      </div>

      {/* Modal overlay — always open, static visual */}
      <div
        className="absolute inset-0 flex items-center justify-center"
        style={{ background: "rgba(21,18,14,0.42)", zIndex: 50 }}
      >
        <div className="w-[540px] rounded-2xl border border-black/[0.06] bg-white p-7">
          <div className="mb-[18px] flex items-start justify-between">
            <div>
              <div className="mb-1 text-xs font-semibold uppercase tracking-wider text-muted-foreground">
                Step 2 of 2
              </div>
              <h2 className="text-[22px] font-semibold">
                Buy your shipping label
              </h2>
            </div>
            <button
              type="button"
              className="flex h-8 w-8 items-center justify-center rounded-full text-muted-foreground hover:bg-black/[0.06]"
              aria-label="Close"
            >
              <X size={14} />
            </button>
          </div>

          <div className="flex flex-col gap-2">
            {shippingOptions.map((opt) => (
              <div
                key={opt.label}
                className="flex items-center gap-3 rounded-[10px] p-3.5"
                style={{
                  border: opt.selected
                    ? "1.5px solid var(--foreground)"
                    : "1px solid rgba(0,0,0,0.1)",
                  background: opt.selected
                    ? "var(--canvas-parchment)"
                    : "white",
                }}
              >
                <span
                  className="flex h-[18px] w-[18px] shrink-0 items-center justify-center rounded-full"
                  style={{ border: "1.5px solid var(--foreground)" }}
                >
                  {opt.selected && (
                    <span
                      className="h-[9px] w-[9px] rounded-full"
                      style={{ background: "var(--foreground)" }}
                    />
                  )}
                </span>
                <div className="flex-1">
                  <div className="text-[13.5px] font-semibold">{opt.label}</div>
                  <div className="text-xs text-muted-foreground">{opt.sub}</div>
                </div>
                <span className="font-semibold tabular-nums">
                  ${opt.price.toFixed(2)}
                </span>
              </div>
            ))}
          </div>

          <div className="mt-[18px] flex items-center justify-between rounded-lg bg-canvas-parchment px-3.5 py-3">
            <span className="text-sm">Buy label · charge to payouts</span>
            <span className="font-semibold tabular-nums">
              $
              {shippingOptions.find((o) => o.selected)?.price.toFixed(2) ??
                "0.00"}
            </span>
          </div>

          <button
            type="button"
            className="mt-3.5 h-11 w-full rounded-xl bg-foreground text-sm font-semibold text-white"
          >
            Buy &amp; print label →
          </button>

          <p className="mt-2.5 text-center text-xs text-muted-foreground">
            Marks order shipped automatically when scanned
          </p>
        </div>
      </div>
    </div>
  );
}
