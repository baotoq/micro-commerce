"use client";

import Image from "next/image";
import { ChevronDown } from "lucide-react";
import { useState } from "react";

import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Separator } from "@/components/ui/separator";
import { useCart } from "@/hooks/use-cart";

function formatPrice(price: number): string {
  return new Intl.NumberFormat("en-US", {
    style: "currency",
    currency: "USD",
  }).format(price);
}

export function OrderSidebar() {
  const { data: cart } = useCart();
  const [mobileExpanded, setMobileExpanded] = useState(false);

  const items = cart?.items ?? [];
  const subtotal = items.reduce((sum, item) => sum + item.lineTotal, 0);
  const shipping = 5.99;
  const tax = subtotal * 0.08;
  const total = subtotal + shipping + tax;

  return (
    <Card className="sticky top-20">
      <CardHeader className="pb-3">
        <button
          type="button"
          className="flex w-full items-center justify-between lg:cursor-default"
          onClick={() => setMobileExpanded(!mobileExpanded)}
          aria-label="Toggle order summary"
        >
          <CardTitle className="text-base">
            Order Summary ({items.length} {items.length === 1 ? "item" : "items"})
          </CardTitle>
          <ChevronDown
            className={`size-4 text-zinc-400 transition-transform lg:hidden ${
              mobileExpanded ? "rotate-180" : ""
            }`}
          />
        </button>
      </CardHeader>

      <CardContent
        className={`space-y-4 ${mobileExpanded ? "block" : "hidden"} lg:block`}
      >
        {/* Item list */}
        <div className="space-y-3">
          {items.map((item) => (
            <div key={item.id} className="flex gap-3">
              <div className="relative size-12 shrink-0 overflow-hidden rounded-md border border-zinc-200 bg-zinc-100">
                {item.imageUrl ? (
                  <Image
                    src={item.imageUrl}
                    alt={item.productName}
                    fill
                    className="object-cover"
                    sizes="48px"
                  />
                ) : (
                  <div className="flex size-full items-center justify-center text-xs text-zinc-400">
                    N/A
                  </div>
                )}
                <span className="absolute -right-1 -top-1 flex size-5 items-center justify-center rounded-full bg-zinc-700 text-[10px] font-medium text-white">
                  {item.quantity}
                </span>
              </div>
              <div className="flex flex-1 items-center justify-between">
                <p className="text-sm text-zinc-700 line-clamp-1">
                  {item.productName}
                </p>
                <p className="shrink-0 text-sm font-medium text-zinc-900">
                  {formatPrice(item.lineTotal)}
                </p>
              </div>
            </div>
          ))}
        </div>

        <Separator />

        {/* Totals */}
        <div className="space-y-2 text-sm">
          <div className="flex justify-between">
            <span className="text-zinc-500">Subtotal</span>
            <span>{formatPrice(subtotal)}</span>
          </div>
          <div className="flex justify-between">
            <span className="text-zinc-500">Shipping</span>
            <span>{formatPrice(shipping)}</span>
          </div>
          <div className="flex justify-between">
            <span className="text-zinc-500">Estimated Tax (8%)</span>
            <span>{formatPrice(tax)}</span>
          </div>
        </div>

        <Separator />

        <div className="flex justify-between text-base font-semibold">
          <span>Total</span>
          <span>{formatPrice(total)}</span>
        </div>
      </CardContent>
    </Card>
  );
}
