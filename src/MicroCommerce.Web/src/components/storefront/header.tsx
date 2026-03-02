"use client";

import { useQueryClient } from "@tanstack/react-query";
import {
  ClipboardList,
  Heart,
  Menu,
  ShoppingBag,
  ShoppingCart,
  User,
} from "lucide-react";
import Link from "next/link";
import { signIn, useSession } from "next-auth/react";
import { Suspense, useEffect, useRef, useState } from "react";
import { Avatar, AvatarFallback } from "@/components/ui/avatar";
import {
  Sheet,
  SheetContent,
  SheetHeader,
  SheetTitle,
  SheetTrigger,
} from "@/components/ui/sheet";
import { useCartItemCount } from "@/hooks/use-cart";
import { useWishlistCount } from "@/hooks/use-wishlist";
import { mergeCart } from "@/lib/api";
import { SearchBar } from "./search-bar";

export function Header() {
  const [mobileMenuOpen, setMobileMenuOpen] = useState(false);
  const { data: cartCount = 0 } = useCartItemCount();
  const { data: wishlistCount = 0 } = useWishlistCount();
  const [bounce, setBounce] = useState(false);
  const prevCount = useRef(cartCount);
  const { data: session } = useSession();
  const queryClient = useQueryClient();
  const hasMerged = useRef(false);

  useEffect(() => {
    if (cartCount !== prevCount.current && cartCount > 0) {
      setBounce(true);
      const timer = setTimeout(() => setBounce(false), 300);
      prevCount.current = cartCount;
      return () => clearTimeout(timer);
    }
    prevCount.current = cartCount;
  }, [cartCount]);

  useEffect(() => {
    if (session?.isNewLogin && session?.accessToken && !hasMerged.current) {
      hasMerged.current = true;
      mergeCart(session.accessToken).then(() => {
        queryClient.invalidateQueries({ queryKey: ["cart"] });
        queryClient.invalidateQueries({ queryKey: ["cartItemCount"] });
      });
    }
  }, [session, queryClient]);

  const userInitials = session?.user?.name
    ? session.user.name
        .split(" ")
        .map((n) => n[0])
        .join("")
        .toUpperCase()
        .slice(0, 2)
    : null;

  return (
    <header className="sticky top-0 z-50 border-b border-border bg-background/80 backdrop-blur-xl">
      <nav className="mx-auto flex h-16 max-w-7xl items-center justify-between gap-4 px-4 sm:px-6 lg:px-10">
        {/* Left: Logo */}
        <div className="flex shrink-0 items-center gap-2">
          <ShoppingBag className="size-6 text-primary" />
          <Link
            href="/"
            className="text-lg font-bold tracking-tight text-foreground"
          >
            MicroCommerce
          </Link>
        </div>

        {/* Center: Search bar (desktop) */}
        <div className="hidden flex-1 justify-center sm:flex">
          <div className="w-full max-w-[400px]">
            <Suspense fallback={null}>
              <SearchBar />
            </Suspense>
          </div>
        </div>

        {/* Right: Icons */}
        <div className="flex shrink-0 items-center gap-5">
          <Link
            href="/wishlist"
            className="relative hidden text-muted-foreground transition-colors hover:text-foreground sm:block"
            aria-label="Wishlist"
          >
            <Heart className="size-[22px]" />
            {wishlistCount > 0 && (
              <span className="absolute -right-1.5 -top-1.5 flex size-4 items-center justify-center rounded-full bg-primary text-[10px] font-semibold text-primary-foreground">
                {wishlistCount > 99 ? "99+" : wishlistCount}
              </span>
            )}
          </Link>

          <Link
            href="/cart"
            className="relative flex items-center gap-1.5 text-muted-foreground transition-colors hover:text-foreground"
            aria-label="Shopping cart"
          >
            <ShoppingCart
              className={`size-[22px] transition-transform duration-300 ${bounce ? "scale-125" : "scale-100"}`}
            />
            {cartCount > 0 && (
              <span className="flex size-5 items-center justify-center rounded-full bg-primary text-[11px] font-semibold text-primary-foreground">
                {cartCount > 99 ? "99+" : cartCount}
              </span>
            )}
          </Link>

          {session ? (
            <Link
              href="/account"
              className="hidden sm:block"
              aria-label="My account"
            >
              <Avatar className="size-10">
                <AvatarFallback className="bg-muted text-sm font-semibold text-foreground">
                  {userInitials ?? <User className="size-4" />}
                </AvatarFallback>
              </Avatar>
            </Link>
          ) : (
            <button
              type="button"
              onClick={() => signIn("keycloak")}
              className="hidden sm:block"
              aria-label="Sign in"
            >
              <Avatar className="size-10">
                <AvatarFallback className="bg-muted text-foreground">
                  <User className="size-4" />
                </AvatarFallback>
              </Avatar>
            </button>
          )}

          {/* Mobile menu toggle */}
          <Sheet open={mobileMenuOpen} onOpenChange={setMobileMenuOpen}>
            <SheetTrigger asChild>
              <button
                type="button"
                className="text-muted-foreground transition-colors hover:text-foreground sm:hidden"
                aria-label="Open menu"
              >
                <Menu className="size-5" />
              </button>
            </SheetTrigger>
            <SheetContent side="right" className="w-72">
              <SheetHeader>
                <SheetTitle>Menu</SheetTitle>
              </SheetHeader>
              <div className="mt-4 flex flex-col gap-1 px-4">
                <Suspense fallback={null}>
                  <SearchBar />
                </Suspense>
                <div className="mt-4 flex flex-col gap-3">
                  <Link
                    href="/"
                    className="flex items-center gap-3 text-sm font-medium text-foreground transition-colors hover:text-primary"
                    onClick={() => setMobileMenuOpen(false)}
                  >
                    <ShoppingBag className="size-4" />
                    Products
                  </Link>
                  {session ? (
                    <Link
                      href="/account"
                      className="flex items-center gap-3 text-sm font-medium text-foreground transition-colors hover:text-primary"
                      onClick={() => setMobileMenuOpen(false)}
                    >
                      <User className="size-4" />
                      Account
                    </Link>
                  ) : (
                    <button
                      type="button"
                      onClick={() => {
                        setMobileMenuOpen(false);
                        signIn("keycloak");
                      }}
                      className="flex items-center gap-3 text-left text-sm font-medium text-foreground transition-colors hover:text-primary"
                    >
                      <User className="size-4" />
                      Sign In
                    </button>
                  )}
                  <Link
                    href="/wishlist"
                    className="flex items-center gap-3 text-sm font-medium text-foreground transition-colors hover:text-primary"
                    onClick={() => setMobileMenuOpen(false)}
                  >
                    <Heart className="size-4" />
                    Wishlist
                  </Link>
                  <Link
                    href="/orders"
                    className="flex items-center gap-3 text-sm font-medium text-foreground transition-colors hover:text-primary"
                    onClick={() => setMobileMenuOpen(false)}
                  >
                    <ClipboardList className="size-4" />
                    Orders
                  </Link>
                </div>
              </div>
            </SheetContent>
          </Sheet>
        </div>
      </nav>
    </header>
  );
}
