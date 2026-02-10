"use client";

import Link from "next/link";

import { Button } from "@/components/ui/button";
import {
  Card,
  CardContent,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
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
    <Card className="sticky top-20">
      <CardHeader>
        <CardTitle>Order Summary</CardTitle>
      </CardHeader>
      <CardContent className="space-y-4">
        <div className="flex items-center justify-between text-sm">
          <span className="text-zinc-500">
            Subtotal ({itemCount} {itemCount === 1 ? "item" : "items"})
          </span>
          <span className="font-medium text-zinc-900">
            {formatPrice(subtotal)}
          </span>
        </div>

        <Separator />

        <div className="flex items-center justify-between">
          <span className="text-base font-semibold text-zinc-900">Total</span>
          <span className="text-base font-semibold text-zinc-900">
            {formatPrice(subtotal)}
          </span>
        </div>

        <Button asChild className="w-full rounded-full" size="lg">
          <Link href="/checkout">Proceed to Checkout</Link>
        </Button>
      </CardContent>
    </Card>
  );
}
