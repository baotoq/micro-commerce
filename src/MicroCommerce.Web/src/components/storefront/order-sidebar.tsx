"use client";

import { ChevronDown } from "lucide-react";
import Image from "next/image";
import { useState } from "react";

import { Separator } from "@/components/ui/separator";
import { useCart } from "@/hooks/use-cart";

function formatPrice(price: number): string {
  return new Intl.NumberFormat("en-US", {
    style: "currency",
    currency: "USD",
  }).format(price);
}

interface OrderSidebarProps {
  couponCode?: string | null;
  discountAmount?: number;
}

export function OrderSidebar({
  couponCode,
  discountAmount = 0,
}: OrderSidebarProps = {}) {
  const { data: cart } = useCart();
  const [mobileExpanded, setMobileExpanded] = useState(false);

  const items = cart?.items ?? [];
  const subtotal = items.reduce((sum, item) => sum + item.lineTotal, 0);
  const shipping = 5.99;
  const tax = subtotal * 0.08;
  const total = Math.max(0, subtotal + shipping + tax - discountAmount);

  return (
    <div className="sticky top-20 rounded-lg border border-border bg-card p-6">
      {/* Header with mobile toggle */}
      <button
        type="button"
        className="flex w-full items-center justify-between lg:cursor-default"
        onClick={() => setMobileExpanded(!mobileExpanded)}
        aria-label="Toggle order summary"
      >
        <h3 className="text-base font-bold text-foreground">Order Summary</h3>
        <ChevronDown
          className={`size-4 text-muted-foreground transition-transform lg:hidden ${
            mobileExpanded ? "rotate-180" : ""
          }`}
        />
      </button>

      {/* Content - collapsible on mobile */}
      <div
        className={`mt-4 space-y-4 ${mobileExpanded ? "block" : "hidden"} lg:block`}
      >
        {/* Item list */}
        <div className="space-y-3">
          {items.map((item) => (
            <div key={item.id} className="flex items-center gap-3">
              <div className="relative size-12 shrink-0 overflow-hidden rounded bg-muted">
                {item.imageUrl ? (
                  <Image
                    src={item.imageUrl}
                    alt={item.productName}
                    fill
                    className="object-cover"
                    sizes="48px"
                  />
                ) : (
                  <div className="flex size-full items-center justify-center text-xs text-muted-foreground">
                    N/A
                  </div>
                )}
              </div>
              <div className="flex-1 space-y-0.5">
                <p className="text-[13px] text-foreground line-clamp-1">
                  {item.productName} x{item.quantity}
                </p>
                <p className="text-[13px] font-semibold text-foreground">
                  {formatPrice(item.lineTotal)}
                </p>
              </div>
            </div>
          ))}
        </div>

        <Separator />

        {/* Subtotals */}
        <div className="space-y-2">
          <div className="flex justify-between">
            <span className="text-[13px] text-muted-foreground">Subtotal</span>
            <span className="text-[13px] font-medium text-foreground">
              {formatPrice(subtotal)}
            </span>
          </div>
          <div className="flex justify-between">
            <span className="text-[13px] text-muted-foreground">Shipping</span>
            <span className="text-[13px] font-medium text-success-foreground">
              {formatPrice(shipping)}
            </span>
          </div>
          <div className="flex justify-between">
            <span className="text-[13px] text-muted-foreground">
              Estimated Tax
            </span>
            <span className="text-[13px] font-medium text-foreground">
              {formatPrice(tax)}
            </span>
          </div>
          {discountAmount > 0 && couponCode && (
            <div className="flex justify-between">
              <span className="text-[13px] text-muted-foreground">
                Coupon ({couponCode})
              </span>
              <span className="text-[13px] font-medium text-success-foreground">
                −{formatPrice(discountAmount)}
              </span>
            </div>
          )}
        </div>

        <Separator />

        {/* Total */}
        <div className="flex items-center justify-between">
          <span className="text-[15px] font-bold text-foreground">Total</span>
          <span className="text-lg font-bold text-foreground">
            {formatPrice(total)}
          </span>
        </div>
      </div>
    </div>
  );
}
