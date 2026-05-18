import type { PromoTab } from "@/lib/seller/promos/types";

export function PromosTabs({ tabs }: { tabs: PromoTab[] }) {
  return (
    <div className="flex items-end gap-[22px] px-7 pt-5">
      {tabs.map((tab) => (
        <button
          key={tab.label}
          type="button"
          className={[
            "flex items-center gap-1.5 border-b-2 pb-3 text-sm font-medium transition-colors",
            tab.on
              ? "border-[#1d1d1f] text-[#1d1d1f]"
              : "border-transparent text-[#1d1d1f]/50 hover:text-[#1d1d1f]/70",
          ].join(" ")}
        >
          {tab.label}
          <span
            className="rounded-full px-[6px] py-px text-[11px] font-semibold tabular-nums"
            style={{
              background: tab.on ? "#1d1d1f" : "#f5f5f7",
              color: tab.on ? "white" : "#1d1d1f99",
            }}
          >
            {tab.count}
          </span>
        </button>
      ))}
    </div>
  );
}
