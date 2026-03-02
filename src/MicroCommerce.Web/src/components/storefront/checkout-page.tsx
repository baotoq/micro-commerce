"use client";

import { ArrowLeft, ArrowRight, ShoppingCart } from "lucide-react";
import Link from "next/link";
import { useRouter } from "next/navigation";
import { useCallback, useState } from "react";
import { CheckoutLoginGate } from "@/components/storefront/checkout-login-gate";
import { OrderSidebar } from "@/components/storefront/order-sidebar";
import { PaymentSection } from "@/components/storefront/payment-section";
import { ShippingSection } from "@/components/storefront/shipping-section";
import { StepIndicator } from "@/components/storefront/step-indicator";
import { Button } from "@/components/ui/button";
import { Skeleton } from "@/components/ui/skeleton";
import { useCart } from "@/hooks/use-cart";
import type { ShippingAddressDto } from "@/lib/api";

const CHECKOUT_STEPS = [
  { label: "Shipping" },
  { label: "Payment" },
  { label: "Review" },
];

export function CheckoutPage() {
  const router = useRouter();
  const { data: cart, isLoading } = useCart();

  const [checkoutStarted, setCheckoutStarted] = useState(false);
  const [currentStep, setCurrentStep] = useState(1);
  const [shippingData, setShippingData] = useState<ShippingAddressDto | null>(
    null,
  );

  const handleContinueAsGuest = useCallback(() => {
    setCheckoutStarted(true);
  }, []);

  function handleShippingComplete(data: ShippingAddressDto) {
    setShippingData(data);
    setCurrentStep(2);
  }

  function handlePaymentSuccess(orderId: string) {
    router.push(`/order-confirmation/${orderId}`);
  }

  function handleBack() {
    if (currentStep > 1) {
      setCurrentStep(currentStep - 1);
    }
  }

  if (isLoading) {
    return <CheckoutSkeleton />;
  }

  const items = cart?.items ?? [];

  if (items.length === 0) {
    return (
      <div className="mx-auto max-w-7xl px-4 py-16">
        <div className="flex flex-col items-center justify-center py-24 text-center">
          <ShoppingCart className="mb-4 size-12 text-muted-foreground" />
          <h2 className="text-xl font-semibold text-foreground">
            Your cart is empty
          </h2>
          <p className="mt-2 text-sm text-muted-foreground">
            Add some items to your cart before checking out.
          </p>
          <Button asChild className="mt-6" size="lg">
            <Link href="/">Continue shopping</Link>
          </Button>
        </div>
      </div>
    );
  }

  if (!checkoutStarted) {
    return (
      <div className="mx-auto max-w-7xl px-4 py-8">
        <h1 className="mb-8 text-2xl font-bold tracking-tight text-foreground">
          Checkout
        </h1>
        <CheckoutLoginGate onContinueAsGuest={handleContinueAsGuest} />
      </div>
    );
  }

  return (
    <div className="mx-auto max-w-7xl px-4 py-8">
      <h1 className="mb-6 text-2xl font-bold tracking-tight text-foreground">
        Checkout
      </h1>

      {/* Step Indicator */}
      <div className="mb-8">
        <StepIndicator steps={CHECKOUT_STEPS} currentStep={currentStep} />
      </div>

      {/* Two-column layout: form + sidebar */}
      <div className="grid grid-cols-1 gap-8 lg:grid-cols-3">
        {/* Left column: step content */}
        <div className="lg:col-span-2">
          {/* Step 1: Shipping */}
          {currentStep === 1 && (
            <div className="rounded-lg border bg-card p-6">
              <h2 className="mb-5 text-lg font-bold text-foreground">
                Shipping Address
              </h2>
              <ShippingSection onComplete={handleShippingComplete} />
            </div>
          )}

          {/* Step 2: Payment */}
          {currentStep === 2 && shippingData && (
            <div className="rounded-lg border bg-card p-6">
              <h2 className="mb-5 text-lg font-bold text-foreground">
                Payment
              </h2>
              <PaymentSection
                shippingData={shippingData}
                onSuccess={handlePaymentSuccess}
              />
            </div>
          )}

          {/* Step 3: Review */}
          {currentStep === 3 && shippingData && (
            <div className="rounded-lg border bg-card p-6">
              <h2 className="mb-5 text-lg font-bold text-foreground">
                Review Order
              </h2>
              <div className="space-y-4">
                <div className="rounded-md border bg-background p-4">
                  <p className="mb-1 text-sm font-medium text-foreground">
                    Shipping to
                  </p>
                  <p className="text-sm text-muted-foreground">
                    {shippingData.name}
                  </p>
                  <p className="text-sm text-muted-foreground">
                    {shippingData.street}
                  </p>
                  <p className="text-sm text-muted-foreground">
                    {shippingData.city}, {shippingData.state}{" "}
                    {shippingData.zipCode}
                  </p>
                </div>
                <PaymentSection
                  shippingData={shippingData}
                  onSuccess={handlePaymentSuccess}
                />
              </div>
            </div>
          )}

          {/* Navigation buttons */}
          <div className="mt-6 flex justify-end gap-3">
            {currentStep > 1 && (
              <Button variant="outline" onClick={handleBack}>
                <ArrowLeft className="mr-2 size-4" />
                Back
              </Button>
            )}
            {currentStep === 1 && (
              <Button variant="outline" asChild>
                <Link href="/cart">
                  <ArrowLeft className="mr-2 size-4" />
                  Back to Cart
                </Link>
              </Button>
            )}
            {currentStep === 1 && (
              <Button
                type="submit"
                form="shipping-form"
                aria-label="Continue to Payment"
              >
                Continue to Payment
                <ArrowRight className="ml-2 size-4" />
              </Button>
            )}
          </div>
        </div>

        {/* Right column: order summary sidebar */}
        <div>
          <OrderSidebar />
        </div>
      </div>
    </div>
  );
}

function CheckoutSkeleton() {
  return (
    <div className="mx-auto max-w-7xl px-4 py-8">
      <Skeleton className="mb-6 h-8 w-32" />
      <Skeleton className="mb-8 h-10 w-full max-w-lg" />
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
