'use client';

import Link from 'next/link';
import { ShoppingCart, Menu, X } from 'lucide-react';
import { Suspense, useState } from 'react';

import { SearchBar } from './search-bar';

export function Header() {
  const [mobileMenuOpen, setMobileMenuOpen] = useState(false);

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
          <button
            type="button"
            className="relative text-zinc-500 transition-colors hover:text-zinc-900"
            aria-label="Shopping cart"
          >
            <ShoppingCart className="h-4 w-4" />
            <span className="absolute -right-1.5 -top-1.5 flex h-3.5 w-3.5 items-center justify-center rounded-full bg-zinc-900 text-[9px] font-medium text-white">
              0
            </span>
          </button>

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
          </div>
        </div>
      )}
    </header>
  );
}
