"use client";

import {
  AlertTriangle,
  FolderTree,
  LayoutDashboard,
  Package,
  ShoppingBag,
  Tag,
} from "lucide-react";
import Link from "next/link";
import { usePathname } from "next/navigation";
import { Toaster } from "sonner";
import { QueryProvider } from "@/components/providers/query-provider";
import { Badge } from "@/components/ui/badge";
import { Separator } from "@/components/ui/separator";
import {
  Sidebar,
  SidebarContent,
  SidebarGroup,
  SidebarGroupLabel,
  SidebarHeader,
  SidebarInset,
  SidebarMenu,
  SidebarMenuButton,
  SidebarMenuItem,
  SidebarProvider,
  SidebarTrigger,
} from "@/components/ui/sidebar";

const NAV_ITEMS = [
  { href: "/admin", label: "Dashboard", icon: LayoutDashboard, exact: true },
  { href: "/admin/products", label: "Products", icon: Package },
  { href: "/admin/categories", label: "Categories", icon: FolderTree },
  { href: "/admin/orders", label: "Orders", icon: ShoppingBag },
  { href: "/admin/coupons", label: "Coupons", icon: Tag },
  { href: "/admin/dead-letters", label: "Dead Letters", icon: AlertTriangle },
];

function AdminSidebarContent() {
  const pathname = usePathname();

  return (
    <Sidebar>
      <SidebarHeader className="px-4 py-5">
        <Link href="/admin" className="flex items-center gap-2 px-2">
          <ShoppingBag className="h-[22px] w-[22px] text-primary" />
          <span className="text-base font-bold text-sidebar-foreground">
            MicroCommerce
          </span>
          <Badge
            variant="secondary"
            className="bg-sidebar-accent text-[#9CA3AF] text-xs"
          >
            Admin
          </Badge>
        </Link>
      </SidebarHeader>
      <SidebarContent>
        <SidebarGroup>
          <SidebarGroupLabel>Navigation</SidebarGroupLabel>
          <SidebarMenu>
            {NAV_ITEMS.map((item) => {
              const isActive = item.exact
                ? pathname === item.href
                : pathname.startsWith(item.href);
              return (
                <SidebarMenuItem key={item.href}>
                  <SidebarMenuButton
                    asChild
                    isActive={isActive}
                    tooltip={item.label}
                  >
                    <Link href={item.href}>
                      <item.icon />
                      <span>{item.label}</span>
                    </Link>
                  </SidebarMenuButton>
                </SidebarMenuItem>
              );
            })}
          </SidebarMenu>
        </SidebarGroup>
      </SidebarContent>
    </Sidebar>
  );
}

export default function AdminLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  return (
    <QueryProvider>
      <div
        style={
          {
            "--sidebar": "oklch(0.178 0.02 256.788)",
            "--sidebar-foreground": "oklch(0.985 0.002 247.839)",
            "--sidebar-accent": "oklch(0.237 0.02 256.788)",
            "--sidebar-accent-foreground": "oklch(0.985 0.002 247.839)",
            "--sidebar-primary": "oklch(0.546 0.245 262.881)",
            "--sidebar-primary-foreground": "oklch(1 0 0)",
            "--sidebar-border": "oklch(0.295 0.02 256.788)",
          } as React.CSSProperties
        }
      >
        <SidebarProvider>
          <AdminSidebarContent />
          <SidebarInset>
            <header className="flex h-14 items-center gap-2 border-b px-6">
              <SidebarTrigger className="-ml-2" />
              <Separator orientation="vertical" className="mr-2 h-4" />
              <span className="text-sm font-medium text-muted-foreground">
                Admin Panel
              </span>
            </header>
            <main className="flex-1 overflow-auto p-6">{children}</main>
          </SidebarInset>
        </SidebarProvider>
      </div>
      <Toaster position="top-right" />
    </QueryProvider>
  );
}
