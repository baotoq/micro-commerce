"use client";

import Link from "next/link";
import { usePathname } from "next/navigation";
import { User, MapPin, Package, Shield, Heart } from "lucide-react";

const sections = [
  {
    name: "Profile",
    href: "/account/profile",
    icon: User,
  },
  {
    name: "Addresses",
    href: "/account/addresses",
    icon: MapPin,
  },
  {
    name: "Orders",
    href: "/account/orders",
    icon: Package,
  },
  {
    name: "Wishlist",
    href: "/wishlist",
    icon: Heart,
  },
  {
    name: "Security",
    href: "/account/security",
    icon: Shield,
  },
];

export function AccountSidebar() {
  const pathname = usePathname();

  return (
    <nav aria-label="Account navigation" className="space-y-1">
      {sections.map((section) => {
        const isActive = pathname === section.href;
        const Icon = section.icon;

        return (
          <Link
            key={section.href}
            href={section.href}
            className={`flex items-center gap-3 rounded-md px-3 py-2 text-sm transition-colors ${
              isActive
                ? "bg-zinc-100 text-zinc-900"
                : "text-zinc-500 hover:text-zinc-900"
            }`}
          >
            <Icon className="h-4 w-4" />
            {section.name}
          </Link>
        );
      })}
    </nav>
  );
}
