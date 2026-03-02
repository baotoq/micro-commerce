import { ShoppingBag } from "lucide-react";
import Link from "next/link";

const shopLinks = [
  { label: "All Products", href: "/" },
  { label: "Categories", href: "/#products" },
  { label: "New Arrivals", href: "/" },
  { label: "Best Sellers", href: "/" },
];

const accountLinks = [
  { label: "My Orders", href: "/orders" },
  { label: "Wishlist", href: "/wishlist" },
  { label: "Profile", href: "/account" },
];

const supportLinks = [
  { label: "Help Center", href: "#" },
  { label: "Returns", href: "#" },
  { label: "Contact Us", href: "#" },
];

export function Footer() {
  return (
    <footer className="border-t border-border bg-card">
      <div className="mx-auto max-w-7xl px-4 py-10 sm:px-6 lg:px-10">
        <div className="grid grid-cols-1 gap-8 sm:grid-cols-2 lg:grid-cols-4">
          {/* Column 1: Brand */}
          <div className="flex flex-col gap-3">
            <div className="flex items-center gap-2">
              <ShoppingBag className="size-5 text-primary" />
              <span className="text-base font-bold text-foreground">
                MicroCommerce
              </span>
            </div>
            <p className="max-w-[280px] text-[13px] leading-relaxed text-muted-foreground">
              Your one-stop shop for premium products. Quality you can trust,
              prices you will love.
            </p>
          </div>

          {/* Column 2: Shop */}
          <div className="flex flex-col gap-2.5">
            <h3 className="text-sm font-semibold text-foreground">Shop</h3>
            {shopLinks.map((link) => (
              <Link
                key={link.label}
                href={link.href}
                className="text-[13px] text-muted-foreground transition-colors hover:text-foreground"
              >
                {link.label}
              </Link>
            ))}
          </div>

          {/* Column 3: Account */}
          <div className="flex flex-col gap-2.5">
            <h3 className="text-sm font-semibold text-foreground">Account</h3>
            {accountLinks.map((link) => (
              <Link
                key={link.label}
                href={link.href}
                className="text-[13px] text-muted-foreground transition-colors hover:text-foreground"
              >
                {link.label}
              </Link>
            ))}
          </div>

          {/* Column 4: Support */}
          <div className="flex flex-col gap-2.5">
            <h3 className="text-sm font-semibold text-foreground">Support</h3>
            {supportLinks.map((link) => (
              <Link
                key={link.label}
                href={link.href}
                className="text-[13px] text-muted-foreground transition-colors hover:text-foreground"
              >
                {link.label}
              </Link>
            ))}
          </div>
        </div>

        {/* Bottom bar */}
        <div className="mt-10 border-t border-border pt-6 text-center sm:text-left">
          <div className="flex flex-col items-center gap-2 sm:flex-row sm:justify-between">
            <p className="text-xs text-muted-foreground">
              &copy; {new Date().getFullYear()} MicroCommerce. All rights
              reserved.
            </p>
            <Link
              href="/admin"
              className="text-xs text-muted-foreground transition-colors hover:text-foreground"
            >
              Admin
            </Link>
          </div>
        </div>
      </div>
    </footer>
  );
}
