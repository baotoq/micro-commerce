"use client";

import Link from "next/link";

import { Button } from "@/components/ui/button";
import { Separator } from "@/components/ui/separator";
import type { CartItemDto } from "@/lib/api";

interface CartSummaryProps {
  items: CartItemDto[];
}

function formatPrice(price: number): string {
  return new Intl.NumberFormat("en-US", {
    style: "currency",
    currency: "USD",
  }).format(price);
}

export function CartSummary({ items }: CartSummaryProps) {
  const subtotal = items.reduce((sum, item) => sum + item.lineTotal, 0);
  const itemCount = items.reduce((sum, item) => sum + item.quantity, 0);

  return (
    <div className="sticky top-20 rounded-lg border border-border bg-card p-6 space-y-5">
      <h2 className="text-lg font-bold text-foreground">Order Summary</h2>

      {/* Subtotal */}
      <div className="flex items-center justify-between">
        <span className="text-sm text-muted-foreground">
          Subtotal ({itemCount} {itemCount === 1 ? "item" : "items"})
        </span>
        <span className="text-sm font-medium text-foreground">
          {formatPrice(subtotal)}
        </span>
      </div>

      {/* Shipping */}
      <div className="flex items-center justify-between">
        <span className="text-sm text-muted-foreground">Shipping</span>
        <span className="text-sm font-medium text-success-foreground">
          Free
        </span>
      </div>

      <Separator />

      {/* Total */}
      <div className="flex items-center justify-between">
        <span className="text-base font-bold text-foreground">Total</span>
        <span className="text-xl font-bold text-foreground">
          {formatPrice(subtotal)}
        </span>
      </div>

      {/* Checkout CTA */}
      <Button asChild className="w-full h-12 text-base font-semibold" size="lg">
        <Link href="/checkout">Proceed to Checkout</Link>
      </Button>

      {/* Continue Shopping link */}
      <div className="text-center">
        <Link
          href="/"
          className="text-[13px] font-medium text-primary hover:underline"
        >
          or Continue Shopping
        </Link>
      </div>
    </div>
  );
}
