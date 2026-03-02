"use client";

import { AlertCircle, CreditCard, Loader2 } from "lucide-react";
import { useState } from "react";

import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { useCart } from "@/hooks/use-cart";
import { useSimulatePayment, useSubmitOrder } from "@/hooks/use-checkout";
import type { ShippingAddressDto } from "@/lib/api";

function formatPrice(price: number): string {
  return new Intl.NumberFormat("en-US", {
    style: "currency",
    currency: "USD",
  }).format(price);
}

interface PaymentSectionProps {
  shippingData: ShippingAddressDto;
  onSuccess: (orderId: string) => void;
}

export function PaymentSection({
  shippingData,
  onSuccess,
}: PaymentSectionProps) {
  const { data: cart } = useCart();
  const submitOrder = useSubmitOrder();
  const simulatePayment = useSimulatePayment();

  const [shouldSucceed, setShouldSucceed] = useState(true);
  const [isProcessing, setIsProcessing] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const items = cart?.items ?? [];
  const subtotal = items.reduce((sum, item) => sum + item.lineTotal, 0);
  const shipping = 5.99;
  const tax = subtotal * 0.08;
  const total = subtotal + shipping + tax;

  async function handlePayment() {
    if (items.length === 0) return;

    setIsProcessing(true);
    setError(null);

    try {
      const orderId = await submitOrder.mutateAsync({
        email: shippingData.email,
        shippingAddress: shippingData,
        items: items.map((item) => ({
          productId: item.productId,
          productName: item.productName,
          unitPrice: item.unitPrice,
          imageUrl: item.imageUrl,
          quantity: item.quantity,
        })),
      });

      // Simulate realistic processing delay
      await new Promise((resolve) => setTimeout(resolve, 1500));

      const paymentResult = await simulatePayment.mutateAsync({
        orderId,
        data: { shouldSucceed },
      });

      if (paymentResult.success) {
        onSuccess(orderId);
      } else {
        setError(
          paymentResult.failureReason ||
            "Payment was declined. Please try again.",
        );
      }
    } catch {
      setError(
        "An error occurred while processing your payment. Please try again.",
      );
    } finally {
      setIsProcessing(false);
    }
  }

  return (
    <div className="space-y-6">
      {error && (
        <div className="flex items-start gap-3 rounded-lg border border-destructive/30 bg-error-bg p-4">
          <AlertCircle className="mt-0.5 size-5 shrink-0 text-destructive" />
          <div>
            <p className="text-sm font-medium text-error-foreground">
              Payment Failed
            </p>
            <p className="mt-1 text-sm text-destructive">{error}</p>
          </div>
        </div>
      )}

      {/* Card input fields */}
      <div className="space-y-4">
        <div className="space-y-1.5">
          <Label htmlFor="cardNumber" className="text-sm font-medium">
            Card Number
          </Label>
          <div className="relative">
            <Input
              id="cardNumber"
              placeholder="1234 5678 9012 3456"
              className="pr-10"
            />
            <CreditCard className="absolute top-1/2 right-3 size-4 -translate-y-1/2 text-muted-foreground" />
          </div>
        </div>

        <div className="grid grid-cols-2 gap-4">
          <div className="space-y-1.5">
            <Label htmlFor="expiry" className="text-sm font-medium">
              Expiry Date
            </Label>
            <Input id="expiry" placeholder="MM / YY" />
          </div>
          <div className="space-y-1.5">
            <Label htmlFor="cvc" className="text-sm font-medium">
              CVC
            </Label>
            <Input id="cvc" placeholder="123" />
          </div>
        </div>

        <div className="space-y-1.5">
          <Label htmlFor="cardName" className="text-sm font-medium">
            Name on Card
          </Label>
          <Input id="cardName" placeholder="John Doe" />
        </div>
      </div>

      {/* Order total */}
      <div className="rounded-lg border border-border bg-muted/50 p-4">
        <div className="space-y-2 text-sm">
          <div className="flex justify-between">
            <span className="text-muted-foreground">Subtotal</span>
            <span className="font-medium text-foreground">
              {formatPrice(subtotal)}
            </span>
          </div>
          <div className="flex justify-between">
            <span className="text-muted-foreground">Shipping</span>
            <span className="font-medium text-foreground">
              {formatPrice(shipping)}
            </span>
          </div>
          <div className="flex justify-between">
            <span className="text-muted-foreground">Estimated Tax</span>
            <span className="font-medium text-foreground">
              {formatPrice(tax)}
            </span>
          </div>
          <div className="flex justify-between border-t border-border pt-2 text-base font-bold">
            <span className="text-foreground">Total</span>
            <span className="text-foreground">{formatPrice(total)}</span>
          </div>
        </div>
      </div>

      {/* Payment simulation toggle */}
      <div className="rounded-lg border border-warning/30 bg-warning-bg p-4">
        <p className="mb-3 text-xs font-medium text-warning-foreground">
          Demo Mode: Simulate payment outcome
        </p>
        <div className="flex items-center gap-4">
          <Label className="flex cursor-pointer items-center gap-2">
            <input
              type="radio"
              name="paymentOutcome"
              checked={shouldSucceed}
              onChange={() => setShouldSucceed(true)}
              className="accent-primary"
            />
            <span className="text-sm text-foreground">Success</span>
          </Label>
          <Label className="flex cursor-pointer items-center gap-2">
            <input
              type="radio"
              name="paymentOutcome"
              checked={!shouldSucceed}
              onChange={() => setShouldSucceed(false)}
              className="accent-primary"
            />
            <span className="text-sm text-foreground">Failure</span>
          </Label>
        </div>
      </div>

      {/* Pay button */}
      <Button
        className="w-full"
        size="lg"
        onClick={handlePayment}
        disabled={isProcessing || items.length === 0}
        aria-label={`Pay ${formatPrice(total)}`}
      >
        {isProcessing ? (
          <>
            <Loader2 className="mr-2 size-4 animate-spin" />
            Processing payment...
          </>
        ) : (
          `Pay Now \u2014 ${formatPrice(total)}`
        )}
      </Button>
    </div>
  );
}
