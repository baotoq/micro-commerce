"use client";

import { Accordion } from "radix-ui";
import { Check, ChevronDown } from "lucide-react";

import { ShippingSection } from "@/components/storefront/shipping-section";
import { PaymentSection } from "@/components/storefront/payment-section";
import type { ShippingAddressDto } from "@/lib/api";

interface CheckoutAccordionProps {
  activeSection: string;
  onSectionChange: (value: string) => void;
  shippingData: ShippingAddressDto | null;
  onShippingComplete: (data: ShippingAddressDto) => void;
  onPaymentSuccess: (orderId: string) => void;
}

export function CheckoutAccordion({
  activeSection,
  onSectionChange,
  shippingData,
  onShippingComplete,
  onPaymentSuccess,
}: CheckoutAccordionProps) {
  const isShippingComplete = shippingData !== null;

  return (
    <Accordion.Root
      type="single"
      value={activeSection}
      onValueChange={(value) => {
        if (value) onSectionChange(value);
      }}
      className="space-y-4"
    >
      {/* Shipping Section */}
      <Accordion.Item
        value="shipping"
        className="rounded-lg border border-zinc-200 bg-white"
      >
        <Accordion.Header>
          <Accordion.Trigger className="flex w-full items-center justify-between px-6 py-4 text-left">
            <div className="flex items-center gap-3">
              <span
                className={`flex size-7 items-center justify-center rounded-full text-sm font-medium ${
                  isShippingComplete
                    ? "bg-green-100 text-green-700"
                    : "bg-zinc-100 text-zinc-600"
                }`}
              >
                {isShippingComplete ? (
                  <Check className="size-4" />
                ) : (
                  "1"
                )}
              </span>
              <span className="text-base font-medium text-zinc-900">
                Shipping Information
              </span>
            </div>
            <ChevronDown className="size-4 text-zinc-400 transition-transform data-[state=open]:rotate-180" />
          </Accordion.Trigger>
        </Accordion.Header>
        <Accordion.Content className="overflow-hidden data-[state=closed]:animate-accordion-up data-[state=open]:animate-accordion-down">
          <div className="px-6 pb-6">
            <ShippingSection onComplete={onShippingComplete} />
          </div>
        </Accordion.Content>
      </Accordion.Item>

      {/* Payment Section */}
      <Accordion.Item
        value="payment"
        disabled={!isShippingComplete}
        className={`rounded-lg border border-zinc-200 bg-white ${
          !isShippingComplete ? "opacity-50" : ""
        }`}
      >
        <Accordion.Header>
          <Accordion.Trigger
            className="flex w-full items-center justify-between px-6 py-4 text-left disabled:cursor-not-allowed"
            disabled={!isShippingComplete}
          >
            <div className="flex items-center gap-3">
              <span className="flex size-7 items-center justify-center rounded-full bg-zinc-100 text-sm font-medium text-zinc-600">
                2
              </span>
              <span className="text-base font-medium text-zinc-900">Payment</span>
            </div>
            <ChevronDown className="size-4 text-zinc-400 transition-transform data-[state=open]:rotate-180" />
          </Accordion.Trigger>
        </Accordion.Header>
        <Accordion.Content className="overflow-hidden data-[state=closed]:animate-accordion-up data-[state=open]:animate-accordion-down">
          <div className="px-6 pb-6">
            {shippingData && (
              <PaymentSection
                shippingData={shippingData}
                onSuccess={onPaymentSuccess}
              />
            )}
          </div>
        </Accordion.Content>
      </Accordion.Item>
    </Accordion.Root>
  );
}
