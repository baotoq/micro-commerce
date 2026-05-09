"use client";

import Link from "next/link";
import { usePathname } from "next/navigation";
import { cn } from "@/lib/utils";

type Item = { label: string; href: string };

export function SidebarNav({ items }: { items: readonly Item[] }) {
  const pathname = usePathname();
  return (
    <nav className="flex flex-1 flex-col gap-0.5 px-3 py-4">
      {items.map((item) => {
        const active =
          item.href === "/seller"
            ? pathname === "/seller"
            : pathname === item.href || pathname.startsWith(`${item.href}/`);
        return (
          <Link
            key={item.href}
            href={item.href}
            className={cn(
              "relative rounded-md px-3 py-2 text-sm text-[#1d1d1f] transition-colors",
              active
                ? "bg-white font-semibold shadow-[inset_2px_0_0_#0066cc]"
                : "hover:bg-white/60",
            )}
          >
            {item.label}
          </Link>
        );
      })}
    </nav>
  );
}
