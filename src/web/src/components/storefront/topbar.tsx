import Link from "next/link";
import { Suspense } from "react";
import { cartCount } from "@/lib/storefront/cart";
import { readCart } from "@/lib/storefront/cart-cookie";

async function CartBadge() {
  const count = cartCount(await readCart());
  if (count === 0) return null;
  return (
    <span className="absolute -right-1 -top-1 flex h-4 min-w-4 items-center justify-center rounded-full bg-foreground px-1 text-[10px] font-bold text-background">
      {count}
    </span>
  );
}

export function ShopTopbar() {
  return (
    <header className="border-b">
      <div className="mx-auto flex max-w-6xl items-center gap-7 px-6 py-3.5">
        <Link
          href="/"
          className="font-display text-lg font-semibold tracking-tight"
        >
          Micro Commerce
        </Link>
        <nav className="flex gap-5 text-[13px] font-medium">
          <Link href="/" className="text-foreground">
            Shop
          </Link>
          <Link
            href="/cart"
            className="text-muted-foreground hover:text-foreground"
          >
            Bag
          </Link>
        </nav>
        <form action="/" className="ml-auto hidden sm:block">
          <input
            type="search"
            name="q"
            placeholder="Search products…"
            className="h-8 w-52 rounded-full bg-muted px-4 text-xs outline-none placeholder:text-muted-foreground"
          />
        </form>
        <Link href="/cart" aria-label="Bag" className="relative p-1.5">
          <svg
            width="16"
            height="16"
            viewBox="0 0 24 24"
            fill="none"
            stroke="currentColor"
            strokeWidth="1.8"
            aria-hidden="true"
          >
            <title>Bag</title>
            <path d="M6 7h12l-1 13H7L6 7Z" />
            <path d="M9 7a3 3 0 0 1 6 0" />
          </svg>
          <Suspense fallback={null}>
            <CartBadge />
          </Suspense>
        </Link>
      </div>
    </header>
  );
}
