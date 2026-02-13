'use client';

import Link from 'next/link';
import { ShoppingCart, Menu, X, ClipboardList, User, Heart } from 'lucide-react';
import { Suspense, useEffect, useRef, useState } from 'react';
import { useSession, signIn } from 'next-auth/react';
import { useQueryClient } from '@tanstack/react-query';

import { useCartItemCount } from '@/hooks/use-cart';
import { useWishlistCount } from '@/hooks/use-wishlist';
import { SearchBar } from './search-bar';
import { mergeCart } from '@/lib/api';

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

  return (
    <header className="sticky top-0 z-50 border-b border-zinc-200/80 bg-white/80 backdrop-blur-xl">
      <nav className="mx-auto flex h-12 max-w-7xl items-center justify-between gap-4 px-4 sm:px-6 lg:px-8">
        {/* Left: Logo */}
        <div className="flex shrink-0 items-center">
          <Link href="/" className="text-sm font-semibold tracking-tight text-zinc-900">
            MicroCommerce
          </Link>
        </div>

        {/* Center: Search bar (desktop) */}
        <div className="hidden flex-1 justify-center sm:flex">
          <Suspense fallback={null}>
            <SearchBar />
          </Suspense>
        </div>

        {/* Right: Icons */}
        <div className="flex shrink-0 items-center gap-4">
          {session ? (
            <Link
              href="/account"
              className="hidden text-zinc-500 transition-colors hover:text-zinc-900 sm:block"
              aria-label="My account"
            >
              <User className="h-4 w-4" />
            </Link>
          ) : (
            <button
              onClick={() => signIn("keycloak")}
              className="hidden text-zinc-500 transition-colors hover:text-zinc-900 sm:block"
              aria-label="Sign in"
            >
              <User className="h-4 w-4" />
            </button>
          )}
          <Link
            href="/orders"
            className="hidden text-zinc-500 transition-colors hover:text-zinc-900 sm:block"
            aria-label="My orders"
          >
            <ClipboardList className="h-4 w-4" />
          </Link>
          <Link
            href="/wishlist"
            className="hidden text-zinc-500 transition-colors hover:text-zinc-900 sm:block relative"
            aria-label="Wishlist"
          >
            <Heart className="h-4 w-4" />
            {wishlistCount > 0 && (
              <span className="absolute -right-1.5 -top-1.5 flex h-3.5 w-3.5 items-center justify-center rounded-full bg-zinc-900 text-[9px] font-medium text-white">
                {wishlistCount > 99 ? '99+' : wishlistCount}
              </span>
            )}
          </Link>
          <Link
            href="/cart"
            className="relative text-zinc-500 transition-colors hover:text-zinc-900"
            aria-label="Shopping cart"
          >
            <ShoppingCart className={`h-4 w-4 transition-transform duration-300 ${bounce ? 'scale-125' : 'scale-100'}`} />
            {cartCount > 0 && (
              <span className="absolute -right-1.5 -top-1.5 flex h-3.5 w-3.5 items-center justify-center rounded-full bg-zinc-900 text-[9px] font-medium text-white">
                {cartCount > 99 ? '99+' : cartCount}
              </span>
            )}
          </Link>

          {/* Mobile menu toggle */}
          <button
            type="button"
            className="sm:hidden text-zinc-500 transition-colors hover:text-zinc-900"
            aria-label="Toggle menu"
            onClick={() => setMobileMenuOpen(!mobileMenuOpen)}
          >
            {mobileMenuOpen ? <X className="h-4 w-4" /> : <Menu className="h-4 w-4" />}
          </button>
        </div>
      </nav>

      {/* Mobile menu */}
      {mobileMenuOpen && (
        <div className="border-t border-zinc-200/80 bg-white sm:hidden">
          <div className="space-y-3 px-4 py-3">
            <Suspense fallback={null}>
              <SearchBar />
            </Suspense>
            <Link
              href="/"
              className="block text-sm font-medium text-zinc-500 transition-colors hover:text-zinc-900"
              onClick={() => setMobileMenuOpen(false)}
            >
              Products
            </Link>
            {session ? (
              <Link
                href="/account"
                className="block text-sm font-medium text-zinc-500 transition-colors hover:text-zinc-900"
                onClick={() => setMobileMenuOpen(false)}
              >
                Account
              </Link>
            ) : (
              <button
                onClick={() => {
                  setMobileMenuOpen(false);
                  signIn("keycloak");
                }}
                className="block w-full text-left text-sm font-medium text-zinc-500 transition-colors hover:text-zinc-900"
              >
                Sign In
              </button>
            )}
            <Link
              href="/wishlist"
              className="block text-sm font-medium text-zinc-500 transition-colors hover:text-zinc-900"
              onClick={() => setMobileMenuOpen(false)}
            >
              Wishlist
            </Link>
            <Link
              href="/orders"
              className="block text-sm font-medium text-zinc-500 transition-colors hover:text-zinc-900"
              onClick={() => setMobileMenuOpen(false)}
            >
              Orders
            </Link>
          </div>
        </div>
      )}
    </header>
  );
}
