import Link from "next/link";
import { Sparkline } from "@/components/seller/shared/sparkline";
import { SellerTopbar } from "@/components/seller/shell/seller-topbar";
import { getFirstOrderStats } from "@/lib/seller/onboarding/data";

export default function FirstOrderPage() {
  const stats = getFirstOrderStats();

  return (
    <div className="flex min-h-screen flex-col overflow-hidden">
      <SellerTopbar
        title="Good afternoon, Alex"
        subtitle="Day 4 · Friday, March 15"
      />

      <div className="flex-1 overflow-auto px-7 py-6">
        {/* Celebration banner */}
        <div className="relative mb-5 overflow-hidden rounded-xl border-none bg-primary p-[22px] text-primary-foreground">
          <div
            className="absolute rounded-full"
            style={{
              right: -20,
              top: -20,
              width: 180,
              height: 180,
              background: "rgba(255,255,255,0.08)",
            }}
          />
          <div
            className="absolute rounded-full"
            style={{
              right: 80,
              bottom: -40,
              width: 110,
              height: 110,
              background: "rgba(255,255,255,0.06)",
            }}
          />
          <div className="relative flex items-end justify-between">
            <div>
              <div
                className="text-xs font-medium uppercase tracking-wide"
                style={{ opacity: 0.85, letterSpacing: "0.08em" }}
              >
                ★ Your first order
              </div>
              <h2
                className="mt-1.5 text-[36px] font-semibold leading-none"
                style={{ color: "white" }}
              >
                Sasha bought a Persimmon vase.
              </h2>
              <p className="mt-1.5 text-sm" style={{ opacity: 0.9 }}>
                $86.00 · placed 12 minutes ago · we held it for you to confirm.
              </p>
            </div>
            <div className="flex gap-2 shrink-0 ml-6">
              <button
                type="button"
                className="rounded-lg px-4 py-2 text-sm font-medium"
                style={{
                  background: "rgba(255,255,255,0.18)",
                  color: "white",
                  border: "1px solid rgba(255,255,255,0.3)",
                }}
              >
                Send a thank-you
              </button>
              <Link
                href="/seller/orders/1001/pack"
                className="rounded-lg bg-white px-4 py-2 text-sm font-medium text-terra"
              >
                Open order →
              </Link>
            </div>
          </div>
        </div>

        {/* KPI stat row */}
        <div className="mb-5 grid grid-cols-4 gap-3">
          {stats.map((s) => (
            <div
              key={s.label}
              className="rounded-xl border border-black/[0.06] bg-white p-[18px]"
            >
              <div className="mb-1.5 text-xs text-muted-foreground">
                {s.label}
              </div>
              <div className="flex items-end justify-between">
                <div className="text-[28px] font-semibold leading-none tabular-nums">
                  {s.value}
                </div>
                <span className="text-xs font-semibold text-good">
                  ↑ {s.delta}
                </span>
              </div>
              <div className="mt-2.5 h-8">
                <Sparkline points={s.spark} label={`${s.label} trend`} />
              </div>
            </div>
          ))}
        </div>

        {/* Orders table */}
        <div className="overflow-hidden rounded-xl border border-black/[0.06] bg-white">
          <div className="flex items-center justify-between border-b border-black/[0.06] px-5 py-3.5">
            <h2 className="text-base font-semibold">Orders</h2>
            <a
              href="/seller/orders"
              className="text-sm font-medium text-muted-foreground"
            >
              All orders →
            </a>
          </div>
          <table className="w-full text-sm">
            <thead>
              <tr className="border-b border-black/[0.06] text-left text-xs font-medium text-muted-foreground">
                <th className="px-5 py-3">Order</th>
                <th className="px-5 py-3">Customer</th>
                <th className="px-5 py-3">Items</th>
                <th className="px-5 py-3">Total</th>
                <th className="px-5 py-3">Status</th>
                <th className="px-5 py-3" />
              </tr>
            </thead>
            <tbody>
              <tr className="bg-terra/[0.05]">
                <td className="px-5 py-3.5">
                  <span className="flex items-center gap-2">
                    <span className="inline-block size-2 rounded-full bg-terra" />
                    <span className="font-mono font-semibold text-foreground">
                      #1001
                    </span>
                  </span>
                </td>
                <td className="px-5 py-3.5">
                  <span className="flex items-center gap-2">
                    <span className="flex size-6 items-center justify-center rounded-full bg-primary/10 text-xs font-semibold text-primary">
                      S
                    </span>
                    <span>Sasha L.</span>
                  </span>
                </td>
                <td className="px-5 py-3.5 text-muted-foreground">
                  Persimmon vase
                </td>
                <td className="px-5 py-3.5 font-semibold tabular-nums text-foreground">
                  $86.00
                </td>
                <td className="px-5 py-3.5">
                  <span className="rounded-full bg-warn/15 px-2.5 py-1 text-[11px] font-semibold text-warn">
                    New · pack today
                  </span>
                </td>
                <td className="px-5 py-3.5 text-muted-foreground">
                  <Link
                    href="/seller/orders/1001/pack"
                    aria-label="View order #1001"
                  >
                    ›
                  </Link>
                </td>
              </tr>
            </tbody>
          </table>
          <div className="flex items-center gap-2 border-t border-black/[0.06] px-5 py-3.5 text-xs text-muted-foreground">
            <span>ℹ</span>
            <span>
              Funds are released to your bank 2 days after the order ships.
            </span>
          </div>
        </div>
      </div>
    </div>
  );
}
