"use client";

import {
  ChartColumn,
  LayoutDashboard,
  type LucideIcon,
  Package,
  ShoppingBag,
  Users,
} from "lucide-react";
import Link from "next/link";
import { usePathname } from "next/navigation";
import { cn } from "@/lib/utils";

type Item = { label: string; href: string };

const ICONS: Record<string, LucideIcon> = {
  "/seller": LayoutDashboard,
  "/seller/orders": ShoppingBag,
  "/seller/listings": Package,
  "/seller/analytics": ChartColumn,
  "/seller/customers": Users,
};

export function SidebarNav({ items }: { items: readonly Item[] }) {
  const pathname = usePathname();
  return (
    <nav className="flex flex-1 flex-col gap-0.5 px-3 py-4">
      {items.map((item) => {
        const active =
          item.href === "/seller"
            ? pathname === "/seller"
            : pathname === item.href || pathname.startsWith(`${item.href}/`);
        const Icon = ICONS[item.href];
        return (
          <Link
            key={item.href}
            href={item.href}
            className={cn(
              "relative flex items-center gap-3 rounded-md px-3 py-2 text-sm text-[#1d1d1f] transition-colors",
              active
                ? "bg-white font-semibold shadow-[inset_2px_0_0_#0066cc]"
                : "hover:bg-white/60",
            )}
          >
            {Icon && (
              <Icon
                aria-hidden
                className={cn(
                  "h-4 w-4 shrink-0",
                  active ? "text-[#0066cc]" : "text-[#1d1d1f]/60",
                )}
              />
            )}
            {item.label}
          </Link>
        );
      })}
    </nav>
  );
}
