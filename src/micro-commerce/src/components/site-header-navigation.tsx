"use client";

import Link from "next/link";
import { ChevronDown, ShoppingCart } from "lucide-react";
import { DropdownMenu } from "radix-ui";
import { usePathname } from "next/navigation";
import { Button } from "@/components/ui/button";
import { signInAction, signOutAction } from "@/app/auth-actions";
import type { Me } from "@/lib/accounts";

const menuItemClass =
  "flex w-full cursor-default items-center rounded-sm px-2 py-1.5 text-sm outline-none focus:bg-accent focus:text-accent-foreground";

function isCurrentPath(pathname: string, href: string) {
  return pathname === href || (href !== "/" && pathname.startsWith(`${href}/`));
}

export function SiteHeaderNavigation({
  me,
  cartLineCount,
}: {
  me: Me | null;
  cartLineCount: number;
}) {
  const pathname = usePathname();
  const cartLabel = `${cartLineCount} ${cartLineCount === 1 ? "line" : "lines"}`;

  return (
    <nav aria-label="Main" className="flex w-full items-center justify-between gap-2">
      <Link
        href="/"
        aria-current={pathname === "/" ? "page" : undefined}
        className="shrink-0 rounded-sm font-semibold focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-ring"
      >
        MicroCommerce
      </Link>
      <div className="flex items-center gap-2">
        <Link
          href="/cart"
          aria-current={isCurrentPath(pathname, "/cart") ? "page" : undefined}
          className="flex h-9 items-center gap-2 rounded-lg px-2 text-sm font-medium hover:bg-muted focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-ring"
        >
          <ShoppingCart aria-hidden="true" className="size-4" />
          <span>Cart</span>
          <span aria-hidden="true" className="min-w-5 rounded-full bg-muted px-1.5 text-center text-xs tabular-nums">
            {cartLineCount}
          </span>
          <span className="sr-only">{cartLabel}</span>
        </Link>
        {me ? (
          <DropdownMenu.Root>
            <DropdownMenu.Trigger asChild>
              <Button
                variant="outline"
                aria-label={`Account menu for ${me.displayName}`}
                className="max-w-36 gap-1.5 px-2 sm:max-w-none sm:px-2.5"
              >
                <span className="max-w-24 truncate sm:max-w-none">{me.displayName}</span>
                <ChevronDown aria-hidden="true" className="size-4 shrink-0" />
              </Button>
            </DropdownMenu.Trigger>
            <DropdownMenu.Portal>
              <DropdownMenu.Content
                align="end"
                sideOffset={4}
                className="z-50 min-w-52 rounded-lg border bg-popover p-1 text-popover-foreground shadow-md"
              >
                <DropdownMenu.Label className="px-2 py-1.5 text-xs text-muted-foreground">
                  Signed in as {me.displayName}
                </DropdownMenu.Label>
                <DropdownMenu.Separator className="my-1 h-px bg-border" />
                <DropdownMenu.Item asChild className={menuItemClass}>
                  <Link
                    href={me.merchant ? "/merchant" : "/open-shop"}
                    aria-current={
                      isCurrentPath(pathname, me.merchant ? "/merchant" : "/open-shop") ? "page" : undefined
                    }
                  >
                    {me.merchant ? "Merchant Centre" : "Open your Shop"}
                  </Link>
                </DropdownMenu.Item>
                {me.isPlatformOperator && (
                  <DropdownMenu.Item asChild className={menuItemClass}>
                    <Link href="/ops" aria-current={isCurrentPath(pathname, "/ops") ? "page" : undefined}>
                      Operations
                    </Link>
                  </DropdownMenu.Item>
                )}
                <DropdownMenu.Item asChild className={menuItemClass}>
                  <Link href="/orders" aria-current={isCurrentPath(pathname, "/orders") ? "page" : undefined}>
                    Orders
                  </Link>
                </DropdownMenu.Item>
                <DropdownMenu.Separator className="my-1 h-px bg-border" />
                <form action={signOutAction}>
                  <DropdownMenu.Item asChild className={menuItemClass} onSelect={(event) => event.preventDefault()}>
                    <button type="submit">Sign out</button>
                  </DropdownMenu.Item>
                </form>
              </DropdownMenu.Content>
            </DropdownMenu.Portal>
          </DropdownMenu.Root>
        ) : (
          <form action={signInAction}>
            <Button type="submit" size="sm">
              Sign in
            </Button>
          </form>
        )}
      </div>
    </nav>
  );
}
