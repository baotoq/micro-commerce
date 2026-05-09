// web/src/components/seller/sidebar.tsx
import { BRAND } from "@/lib/seller/data";
import { SidebarNav } from "@/components/seller/sidebar-nav";

const NAV = [
  { label: "Overview",  href: "/seller" },
  { label: "Orders",    href: "/seller/orders" },
  { label: "Listings",  href: "/seller/listings" },
  { label: "Analytics", href: "/seller/analytics" },
  { label: "Customers", href: "/seller/customers" },
] as const;

export function SellerSidebar() {
  return (
    <aside className="flex h-screen w-60 flex-col border-r border-black/[0.06] bg-[#f5f5f7]">
      <div className="flex h-16 items-center px-6 text-[15px] font-semibold tracking-tight">
        {BRAND.name}
      </div>
      <SidebarNav items={NAV} />
    </aside>
  );
}
