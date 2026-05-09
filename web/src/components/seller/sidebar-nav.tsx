"use client";

import {
  BadgePercent,
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

type Item = { label: string; href: string; badge?: number };

const ICONS: Record<string, LucideIcon> = {
  "/seller": LayoutDashboard,
  "/seller/orders": ShoppingBag,
  "/seller/listings": Package,
  "/seller/promos": BadgePercent,
  "/seller/analytics": ChartColumn,
  "/seller/customers": Users,
};

export function SidebarNav({ items }: { items: readonly Item[] }) {
  const pathname = usePathname();
  return (
    <nav className="flex flex-1 flex-col gap-0.5 px-3 py-2">
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
              "relative flex items-center gap-3 rounded-md px-3 py-2 text-sm transition-colors",
              active
                ? "bg-[#1d1d1f] font-medium text-white"
                : "text-[#1d1d1f] hover:bg-white/60",
            )}
          >
            {Icon && (
              <Icon
                aria-hidden
                className={cn(
                  "h-4 w-4 shrink-0",
                  active ? "text-white" : "text-[#1d1d1f]/60",
                )}
              />
            )}
            <span className="flex-1">{item.label}</span>
            {item.badge != null && (
              <span
                aria-hidden="true"
                className={cn(
                  "inline-flex h-5 min-w-5 items-center justify-center rounded-full px-1.5 text-[11px] font-semibold tabular-nums",
                  active ? "bg-white/20 text-white" : "bg-[#cf5a2c] text-white",
                )}
              >
                {item.badge}
              </span>
            )}
          </Link>
        );
      })}
    </nav>
  );
}
