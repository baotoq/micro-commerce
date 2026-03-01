"use client";

import { Heart, MapPin, Package, User } from "lucide-react";
import Link from "next/link";
import { usePathname } from "next/navigation";
import { useSession } from "next-auth/react";
import { Avatar, AvatarFallback, AvatarImage } from "@/components/ui/avatar";
import { useProfile } from "@/hooks/use-profile";

const sections = [
  {
    name: "Profile",
    href: "/account/profile",
    icon: User,
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
    name: "Addresses",
    href: "/account/addresses",
    icon: MapPin,
  },
];

export function AccountSidebar() {
  const pathname = usePathname();
  const { data: session } = useSession();
  const { data: profile } = useProfile();

  const initials = session?.user?.name
    ? session.user.name
        .split(" ")
        .map((n) => n[0])
        .join("")
        .toUpperCase()
        .slice(0, 2)
    : "??";

  return (
    <div className="rounded-lg border bg-card p-6">
      {/* User info header */}
      <div className="flex flex-col items-center gap-3 pb-6">
        <Avatar className="h-20 w-20">
          <AvatarImage
            src={profile?.avatarUrl || undefined}
            alt={session?.user?.name || "User"}
          />
          <AvatarFallback className="bg-primary text-primary-foreground text-2xl font-bold">
            {initials}
          </AvatarFallback>
        </Avatar>
        <div className="text-center">
          <p className="text-lg font-bold text-foreground">
            {session?.user?.name || "User"}
          </p>
          <p className="text-[13px] text-muted-foreground">
            {session?.user?.email}
          </p>
        </div>
      </div>

      {/* Navigation */}
      <nav aria-label="Account navigation" className="flex flex-col gap-1">
        {sections.map((section) => {
          const isActive =
            pathname === section.href ||
            (section.href !== "/account" && pathname.startsWith(section.href));
          const Icon = section.icon;

          return (
            <Link
              key={section.href}
              href={section.href}
              className={`flex items-center gap-2.5 rounded-md px-3 py-2.5 text-sm transition-colors ${
                isActive
                  ? "bg-accent text-primary font-semibold"
                  : "text-foreground hover:bg-secondary"
              }`}
            >
              <Icon
                className={`h-[18px] w-[18px] ${
                  isActive ? "text-primary" : "text-muted-foreground"
                }`}
              />
              {section.name}
            </Link>
          );
        })}
      </nav>
    </div>
  );
}
