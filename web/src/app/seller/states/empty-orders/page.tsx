// web/src/app/seller/states/empty-orders/page.tsx
import { Inbox } from "lucide-react";
import { CopyShopLink } from "@/components/seller/copy-shop-link";

const DOMAIN = "alex-studio.micro.shop";

const FILTER_CHIPS = ["All · 0", "New", "Pack", "Ship", "Done"] as const;

export default function EmptyOrdersPage() {
  return (
    <div className="flex min-h-screen flex-col overflow-hidden">
      {/* Topbar */}
      <div className="flex items-center justify-between border-b border-black/[0.06] px-6 py-3.5">
        <div className="flex items-center gap-3">
          <h2 className="text-lg font-semibold tracking-tight">Orders</h2>
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

      {/* Two-pane content */}
      <div className="flex flex-1 overflow-hidden">
        {/* Left pane — inbox list (empty) */}
        <div className="flex w-[340px] shrink-0 flex-col items-center border-r border-black/[0.06] px-6 py-10 text-center text-[#1d1d1f]/50">
          <Inbox className="h-8 w-8" aria-hidden />
          <h4 className="mt-3.5 text-sm font-semibold text-[#1d1d1f]">
            No orders yet
          </h4>
          <p className="mt-1 text-xs">
            They'll show up here as soon as someone buys.
          </p>
        </div>

        {/* Right pane — centered empty-state card */}
        <div className="flex flex-1 items-center justify-center overflow-auto p-10">
          <div className="max-w-[460px] text-center">
            {/* Icon circle */}
            <div className="mx-auto mb-[18px] flex h-[84px] w-[84px] items-center justify-center rounded-full bg-[#f5f5f7] text-[#1d1d1f]/50">
              <Inbox className="h-9 w-9" aria-hidden />
            </div>

            <h2 className="mb-2 text-[32px] font-semibold leading-none tracking-tight">
              Quiet, isn't it.
            </h2>

            <p className="mx-auto mb-[22px] max-w-[380px] text-sm text-[#1d1d1f]/60">
              Most shops get their first order within a week of sharing the
              link. While you wait, two things tend to help.
            </p>

            {/* Helper cards */}
            <div className="grid grid-cols-2 gap-2 text-left">
              <div className="rounded-xl border border-black/[0.08] bg-[#f5f5f7] p-3.5">
                <p className="mb-1 text-[13px] font-semibold text-[#1d1d1f]">
                  Add 2 more listings
                </p>
                <p className="text-xs text-[#1d1d1f]/60">
                  Shops with 5+ items get found 3× more.
                </p>
              </div>
              <div className="rounded-xl border border-black/[0.08] bg-[#f5f5f7] p-3.5">
                <p className="mb-1 text-[13px] font-semibold text-[#1d1d1f]">
                  Share your link
                </p>
                <p className="text-xs text-[#1d1d1f]/60">
                  A short note to friends does most of the lifting.
                </p>
              </div>
            </div>

            <CopyShopLink domain={DOMAIN} />
          </div>
        </div>
      </div>
    </div>
  );
}
