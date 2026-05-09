import Link from "next/link";
import { SellerTopbar } from "@/components/seller/seller-topbar";
import { money } from "@/lib/money";

const FILTER_CHIPS = ["All", "New", "Pack", "Ship", "Done"] as const;

const ORDERS = [
  {
    id: "#1001",
    href: "/seller/orders/1001/pack",
    customer: "Sasha L.",
    item: "Persimmon vase",
    total: 86,
    status: "New · pack today",
    statusColor: "warn" as const,
    highlight: true,
  },
] as const;

export default function OrdersPage() {
  return (
    <div className="flex flex-col min-h-screen">
      <SellerTopbar title="Orders" />

      {/* Filter bar */}
      <div className="flex items-center justify-between border-b border-black/[0.06] px-6 py-3.5">
        <div className="flex items-center gap-3">
          <div className="flex gap-1">
            {FILTER_CHIPS.map((chip, i) => (
              <span
                key={chip}
                className={
                  i === 0
                    ? "rounded-full bg-[#1d1d1f] px-3 py-1 text-xs font-medium text-white"
                    : "rounded-full border border-black/[0.08] px-3 py-1 text-xs text-[#1d1d1f]/60"
                }
              >
                {chip}
              </span>
            ))}
          </div>
        </div>
        <button
          type="button"
          className="rounded-lg border border-black/[0.12] px-3 py-1.5 text-xs font-medium text-[#1d1d1f] hover:bg-black/[0.03]"
        >
          Filter
        </button>
      </div>

      {/* Orders table */}
      <div className="flex-1 overflow-auto p-7">
        <div className="overflow-hidden rounded-xl border border-black/[0.06] bg-white">
          <div className="flex items-center justify-between border-b border-black/[0.06] px-5 py-3.5">
            <h3 className="text-[15px] font-semibold text-[#1d1d1f]">Orders</h3>
            <Link
              href="/seller/orders"
              className="text-sm font-medium text-[#1d1d1f]/50"
            >
              All orders →
            </Link>
          </div>
          <table className="w-full">
            <thead>
              <tr className="border-b border-black/[0.04]">
                {["Order", "Customer", "Items", "Total", "Status", ""].map(
                  (h) => (
                    <th
                      key={h}
                      className="px-5 py-2.5 text-left text-[11px] font-medium text-[#1d1d1f]/40"
                    >
                      {h}
                    </th>
                  ),
                )}
              </tr>
            </thead>
            <tbody>
              {ORDERS.map((order) => (
                <tr
                  key={order.id}
                  style={
                    order.highlight
                      ? { background: "rgba(194,65,12,0.05)" }
                      : undefined
                  }
                  className="border-b border-black/[0.04] last:border-0"
                >
                  <td className="px-5 py-3">
                    <span className="flex items-center gap-2 font-mono text-sm font-semibold text-[#1d1d1f]">
                      <span className="inline-block h-2 w-2 rounded-full bg-orange-500" />
                      {order.id}
                    </span>
                  </td>
                  <td className="px-5 py-3">
                    <div className="flex items-center gap-2">
                      <span className="flex h-6 w-6 items-center justify-center rounded-full bg-[#e8e3da] text-[10px] font-semibold text-[#1d1d1f]">
                        {order.customer.charAt(0)}
                      </span>
                      <span className="text-sm text-[#1d1d1f]">
                        {order.customer}
                      </span>
                    </div>
                  </td>
                  <td className="px-5 py-3 text-sm text-[#1d1d1f]/50">
                    {order.item}
                  </td>
                  <td className="px-5 py-3 text-sm font-semibold tabular-nums text-[#1d1d1f]">
                    {money(order.total)}
                  </td>
                  <td className="px-5 py-3">
                    <span className="inline-flex items-center gap-1.5 rounded-full bg-orange-100 px-2.5 py-1 text-[11px] font-semibold text-orange-700">
                      {order.status}
                    </span>
                  </td>
                  <td className="px-5 py-3">
                    <Link
                      href={order.href}
                      aria-label={`Open order ${order.id}`}
                    >
                      <svg
                        width="13"
                        height="13"
                        viewBox="0 0 24 24"
                        fill="none"
                        stroke="currentColor"
                        strokeWidth="2"
                        strokeLinecap="round"
                        strokeLinejoin="round"
                        className="text-[#1d1d1f]/40"
                        aria-hidden="true"
                      >
                        <polyline points="9 18 15 12 9 6" />
                      </svg>
                    </Link>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
          <div className="flex items-center gap-2 border-t border-black/[0.06] px-5 py-3.5 text-[11px] text-[#1d1d1f]/50">
            <svg
              width="11"
              height="11"
              viewBox="0 0 24 24"
              fill="none"
              stroke="currentColor"
              strokeWidth="2"
              strokeLinecap="round"
              strokeLinejoin="round"
              aria-hidden="true"
            >
              <circle cx="12" cy="12" r="10" />
              <line x1="12" y1="8" x2="12" y2="12" />
              <line x1="12" y1="16" x2="12.01" y2="16" />
            </svg>
            Funds are released to your bank 2 days after the order ships.
          </div>
        </div>
      </div>
    </div>
  );
}
