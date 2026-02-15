"use client";

import { AlertCircle, Loader2 } from "lucide-react";
import { useState } from "react";

import { Button } from "@/components/ui/button";
import { Label } from "@/components/ui/label";
import { useSubmitOrder, useSimulatePayment } from "@/hooks/use-checkout";
import { useCart } from "@/hooks/use-cart";
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

export function PaymentSection({ shippingData, onSuccess }: PaymentSectionProps) {
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
          paymentResult.failureReason || "Payment was declined. Please try again."
        );
      }
    } catch {
      setError("An error occurred while processing your payment. Please try again.");
    } finally {
      setIsProcessing(false);
    }
  }

  return (
    <div className="space-y-6">
      {error && (
        <div className="flex items-start gap-3 rounded-lg border border-red-200 bg-red-50 p-4">
          <AlertCircle className="mt-0.5 size-5 shrink-0 text-red-500" />
          <div>
            <p className="text-sm font-medium text-red-800">Payment Failed</p>
            <p className="mt-1 text-sm text-red-600">{error}</p>
          </div>
        </div>
      )}

      {/* Order total */}
      <div className="rounded-lg border border-zinc-200 bg-zinc-50 p-4">
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
            <span className="text-zinc-500">Estimated Tax</span>
            <span>{formatPrice(tax)}</span>
          </div>
          <div className="flex justify-between border-t border-zinc-200 pt-2 text-base font-semibold">
            <span>Total</span>
            <span>{formatPrice(total)}</span>
          </div>
        </div>
      </div>

      {/* Payment simulation toggle */}
      <div className="rounded-lg border border-amber-200 bg-amber-50 p-4">
        <p className="mb-3 text-xs font-medium text-amber-700">
          Demo Mode: Simulate payment outcome
        </p>
        <div className="flex items-center gap-4">
          <Label className="flex cursor-pointer items-center gap-2">
            <input
              type="radio"
              name="paymentOutcome"
              checked={shouldSucceed}
              onChange={() => setShouldSucceed(true)}
              className="accent-zinc-900"
            />
            <span className="text-sm">Success</span>
          </Label>
          <Label className="flex cursor-pointer items-center gap-2">
            <input
              type="radio"
              name="paymentOutcome"
              checked={!shouldSucceed}
              onChange={() => setShouldSucceed(false)}
              className="accent-zinc-900"
            />
            <span className="text-sm">Failure</span>
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
