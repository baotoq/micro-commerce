// web/src/components/seller/sidebar.tsx

import { SidebarNav } from "@/components/seller/shell/sidebar-nav";
import { BRAND } from "@/lib/seller/brand";

const NAV = [
  { label: "Overview", href: "/seller" },
  { label: "Orders", href: "/seller/orders", badge: 4 },
  { label: "Listings", href: "/seller/listings" },
  { label: "Discounts", href: "/seller/promos" },
  { label: "Analytics", href: "/seller/analytics" },
  { label: "Customers", href: "/seller/customers" },
] as const;

export function SellerSidebar() {
  const initial = BRAND.name.charAt(0);
  return (
    <aside className="flex h-screen w-60 flex-col border-r border-black/[0.06] bg-canvas-parchment">
      <div className="flex items-center gap-3 px-5 py-5">
        <div className="flex size-8 items-center justify-center rounded-lg bg-foreground text-base font-semibold leading-none text-white">
          {initial}
        </div>
        <div className="min-w-0">
          <div className="truncate text-[13px] font-semibold tracking-tight">
            {BRAND.name}
          </div>
          <div className="text-[11px] text-foreground/60">Plan · Maker</div>
        </div>
      </div>
      <SidebarNav items={NAV} />
      <div className="m-3 rounded-lg bg-white/80 p-3 shadow-[0_1px_0_rgba(0,0,0,0.04)]">
        <div className="text-[13px] font-semibold tracking-tight">
          Setup · 4 of 6
        </div>
        <div className="my-2 h-1 overflow-hidden rounded-full bg-black/[0.08]">
          <div className="h-full w-2/3 rounded-full bg-foreground" />
        </div>
        <div className="text-[11px] text-foreground/60">
          Add payouts &amp; ship rates
        </div>
      </div>
    </aside>
  );
}
