"use client";

import Link from "next/link";
import { useRouter } from "next/navigation";
import { ShoppingCart } from "lucide-react";
import { useCallback, useState } from "react";

import { Button } from "@/components/ui/button";
import { Skeleton } from "@/components/ui/skeleton";
import { CheckoutLoginGate } from "@/components/storefront/checkout-login-gate";
import { CheckoutAccordion } from "@/components/storefront/checkout-accordion";
import { OrderSidebar } from "@/components/storefront/order-sidebar";
import { useCart } from "@/hooks/use-cart";
import type { ShippingAddressDto } from "@/lib/api";

export function CheckoutPage() {
  const router = useRouter();
  const { data: cart, isLoading } = useCart();

  const [checkoutStarted, setCheckoutStarted] = useState(false);
  const [activeSection, setActiveSection] = useState("shipping");
  const [shippingData, setShippingData] = useState<ShippingAddressDto | null>(null);

  const handleContinueAsGuest = useCallback(() => {
    setCheckoutStarted(true);
  }, []);

  function handleShippingComplete(data: ShippingAddressDto) {
    setShippingData(data);
    setActiveSection("payment");
  }

  function handlePaymentSuccess(orderId: string) {
    router.push(`/order-confirmation/${orderId}`);
  }

  if (isLoading) {
    return <CheckoutSkeleton />;
  }

  const items = cart?.items ?? [];

  if (items.length === 0) {
    return (
      <div className="mx-auto max-w-7xl px-4 py-16 sm:px-6 lg:px-8">
        <div className="flex flex-col items-center justify-center py-24 text-center">
          <ShoppingCart className="mb-4 size-12 text-zinc-300" />
          <h2 className="text-xl font-semibold text-zinc-900">Your cart is empty</h2>
          <p className="mt-2 text-sm text-zinc-500">
            Add some items to your cart before checking out.
          </p>
          <Button asChild className="mt-6 rounded-full" size="lg">
            <Link href="/">Continue shopping</Link>
          </Button>
        </div>
      </div>
    );
  }

  if (!checkoutStarted) {
    return (
      <div className="mx-auto max-w-7xl px-4 py-8 sm:px-6 lg:px-8">
        <h1 className="mb-8 text-2xl font-bold tracking-tight text-zinc-900">
          Checkout
        </h1>
        <CheckoutLoginGate onContinueAsGuest={handleContinueAsGuest} />
      </div>
    );
  }

  return (
    <div className="mx-auto max-w-7xl px-4 py-8 sm:px-6 lg:px-8">
      <h1 className="mb-8 text-2xl font-bold tracking-tight text-zinc-900">
        Checkout
      </h1>

      <div className="grid grid-cols-1 gap-8 lg:grid-cols-3">
        <div className="lg:col-span-2">
          <CheckoutAccordion
            activeSection={activeSection}
            onSectionChange={setActiveSection}
            shippingData={shippingData}
            onShippingComplete={handleShippingComplete}
            onPaymentSuccess={handlePaymentSuccess}
          />
        </div>

        <div>
          <OrderSidebar />
        </div>
      </div>
    </div>
  );
}

function CheckoutSkeleton() {
  return (
    <div className="mx-auto max-w-7xl px-4 py-8 sm:px-6 lg:px-8">
      <Skeleton className="mb-8 h-8 w-32" />
      <div className="grid grid-cols-1 gap-8 lg:grid-cols-3">
        <div className="space-y-4 lg:col-span-2">
          <Skeleton className="h-64 rounded-lg" />
          <Skeleton className="h-16 rounded-lg" />
        </div>
        <div>
          <Skeleton className="h-80 rounded-lg" />
        </div>
      </div>
    </div>
  );
}
