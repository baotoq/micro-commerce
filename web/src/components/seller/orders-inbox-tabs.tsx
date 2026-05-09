import type { OrderInboxTab } from "@/lib/seller/types";

export function OrdersInboxTabs({ tabs }: { tabs: OrderInboxTab[] }) {
  return (
    <div
      role="tablist"
      className="flex items-end gap-[22px] border-b border-black/[0.06] px-7"
    >
      {tabs.map((t) => (
        <button
          key={t.label}
          role="tab"
          type="button"
          aria-selected={t.on ?? false}
          className={`flex items-center gap-1.5 border-b-2 pb-3 pt-3 text-[13px] font-medium transition-colors ${
            t.on
              ? "border-[#1d1d1f] text-[#1d1d1f]"
              : "border-transparent text-[#1d1d1f]/50 hover:text-[#1d1d1f]"
          }`}
        >
          {t.label}
          <span
            className={`rounded-full px-[6px] py-px text-[11px] font-semibold ${
              t.on
                ? "bg-[#1d1d1f] text-white"
                : "bg-black/[0.06] text-[#1d1d1f]/60"
            }`}
          >
            {t.count}
          </span>
        </button>
      ))}
    </div>
  );
}
