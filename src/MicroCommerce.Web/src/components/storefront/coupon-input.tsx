"use client";

import { CheckCircle, Loader2, XCircle } from "lucide-react";
import { useState } from "react";

import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { useValidateCoupon } from "@/hooks/use-coupons";

function formatPrice(price: number): string {
  return new Intl.NumberFormat("en-US", {
    style: "currency",
    currency: "USD",
  }).format(price);
}

interface CouponInputProps {
  subtotal: number;
  onApply: (code: string, discountAmount: number) => void;
  onRemove: () => void;
  appliedCode: string | null;
  discountAmount: number;
}

export function CouponInput({
  subtotal,
  onApply,
  onRemove,
  appliedCode,
  discountAmount,
}: CouponInputProps) {
  const [code, setCode] = useState("");
  const [errorMessage, setErrorMessage] = useState<string | null>(null);
  const validateCoupon = useValidateCoupon();

  async function handleApply() {
    const trimmed = code.trim().toUpperCase();
    if (!trimmed) return;

    setErrorMessage(null);

    validateCoupon.mutate(
      { code: trimmed, subtotal },
      {
        onSuccess: (result) => {
          if (result.isValid) {
            onApply(trimmed, result.discountAmount);
            setCode("");
          } else {
            setErrorMessage(result.errorMessage ?? "Invalid coupon code");
          }
        },
        onError: () => {
          setErrorMessage("Failed to validate coupon. Please try again.");
        },
      },
    );
  }

  function handleRemove() {
    onRemove();
    setCode("");
    setErrorMessage(null);
  }

  if (appliedCode) {
    return (
      <div className="flex items-center justify-between rounded-md border border-success/30 bg-success/5 px-3 py-2">
        <div className="flex items-center gap-2">
          <CheckCircle className="h-4 w-4 text-success-foreground shrink-0" />
          <div>
            <span className="text-sm font-medium text-foreground font-mono">
              {appliedCode}
            </span>
            <span className="ml-2 text-sm text-success-foreground">
              −{formatPrice(discountAmount)}
            </span>
          </div>
        </div>
        <button
          type="button"
          onClick={handleRemove}
          className="text-muted-foreground hover:text-foreground transition-colors"
          aria-label="Remove coupon"
        >
          <XCircle className="h-4 w-4" />
        </button>
      </div>
    );
  }

  return (
    <div className="space-y-2">
      <div className="flex gap-2">
        <Input
          placeholder="Enter coupon code"
          value={code}
          onChange={(e) => {
            setCode(e.target.value.toUpperCase());
            if (errorMessage) setErrorMessage(null);
          }}
          onKeyDown={(e) => e.key === "Enter" && handleApply()}
          disabled={validateCoupon.isPending}
          aria-label="Coupon code"
          className={errorMessage ? "border-destructive" : ""}
        />
        <Button
          type="button"
          variant="outline"
          onClick={handleApply}
          disabled={!code.trim() || validateCoupon.isPending}
        >
          {validateCoupon.isPending ? (
            <Loader2 className="h-4 w-4 animate-spin" />
          ) : (
            "Apply"
          )}
        </Button>
      </div>
      {errorMessage && (
        <p className="text-sm text-destructive flex items-center gap-1.5">
          <XCircle className="h-3.5 w-3.5 shrink-0" />
          {errorMessage}
        </p>
      )}
    </div>
  );
}
