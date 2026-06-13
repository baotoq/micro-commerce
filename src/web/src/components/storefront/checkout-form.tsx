"use client";
"use no memo"; // RHF error state is swallowed by the React Compiler otherwise (project memory)

import { useState, useTransition } from "react";
import { useForm } from "react-hook-form";
import type { CheckoutError } from "@/lib/catalog/checkout";
import { money } from "@/lib/money";
import { placeOrder } from "@/lib/storefront/actions";
import {
  SHIPPING_OPTIONS,
  type ShippingId,
  type Totals,
} from "@/lib/storefront/totals";

type FormValues = {
  customerName: string;
  shipLine1: string;
  cityState: string;
  shippingId: ShippingId;
  cardNumber: string;
  cardName: string;
};

const ERROR_COPY: Record<CheckoutError, string> = {
  EMPTY_CART: "Your bag is empty — head back to the shop.",
  PRODUCT_NOT_FOUND: "An item in your bag is no longer available.",
  INSUFFICIENT_STOCK:
    "An item in your bag is no longer in stock at that quantity.",
  PROMO_INVALID: "Your promo code is no longer valid — remove it in the bag.",
  PROMO_MIN_ORDER: "Your bag no longer meets the promo's minimum.",
  UNKNOWN: "Something went wrong placing the order. Please try again.",
};

export function detectBrand(cardNumber: string): string {
  const digits = cardNumber.replace(/\D/g, "");
  if (digits.startsWith("4")) return "Visa";
  if (/^5[1-5]/.test(digits) || /^2[2-7]/.test(digits)) return "Mastercard";
  if (/^3[47]/.test(digits)) return "Amex";
  return "Card";
}

type Step = "ship" | "pay" | "review";
const STEPS: Step[] = ["ship", "pay", "review"];

export function CheckoutForm({
  email,
  totals,
}: {
  email: string;
  totals: Totals;
}) {
  const [step, setStep] = useState<Step>("ship");
  const [serverError, setServerError] = useState<string | null>(null);
  const [pending, startTransition] = useTransition();

  const {
    register,
    trigger,
    getValues,
    watch,
    formState: { errors },
  } = useForm<FormValues>({
    mode: "onTouched",
    defaultValues: {
      shippingId: "standard",
      customerName: "",
      shipLine1: "",
      cityState: "",
      cardNumber: "",
      cardName: "",
    },
  });

  const shippingId = watch("shippingId");

  const next = async (target: Step, fields: (keyof FormValues)[]) => {
    if (await trigger(fields)) setStep(target);
  };

  const submit = () =>
    startTransition(async () => {
      const v = getValues();
      const digits = v.cardNumber.replace(/\D/g, "");
      const result = await placeOrder({
        customerName: v.customerName,
        shipLine1: v.shipLine1,
        shipLine2: v.cityState,
        cityState: v.cityState,
        shippingId: v.shippingId,
        paymentBrand: detectBrand(v.cardNumber),
        paymentLastFour: digits.slice(-4),
      });
      // placeOrder redirects on success; reaching here means failure.
      if (!result.ok) setServerError(ERROR_COPY[result.error]);
    });

  const showShip = step === "ship";
  const showPay = step === "pay";
  const showReview = step === "review";

  return (
    <div>
      {/* Step rail (mobile-first; desktop shows it too per Checkout_Desktop) */}
      <ol className="mb-5 flex items-center gap-2 text-xs">
        {STEPS.map((s, i) => (
          <li key={s} className="flex items-center gap-2">
            <span
              className={`flex h-5 w-5 items-center justify-center rounded-full text-[10px] font-bold ${
                STEPS.indexOf(step) >= i
                  ? "bg-foreground text-background"
                  : "border text-muted-foreground"
              }`}
            >
              {i + 1}
            </span>
            <span
              className={
                STEPS.indexOf(step) >= i
                  ? "font-medium"
                  : "text-muted-foreground"
              }
            >
              {s === "ship" ? "Ship" : s === "pay" ? "Pay" : "Review"}
            </span>
            {i < STEPS.length - 1 && <span className="w-6 border-t" />}
          </li>
        ))}
      </ol>

      <form onSubmit={(e) => e.preventDefault()} noValidate>
        {showShip && (
          <section className="space-y-3">
            <h2 className="font-display text-2xl">
              Shipping <i>address</i>
            </h2>
            <p className="text-xs text-muted-foreground">
              Signed in as {email}
            </p>
            <div>
              <label htmlFor="customerName" className="text-xs font-medium">
                Full name
              </label>
              <input
                id="customerName"
                className="mt-1 h-10 w-full rounded-lg border px-3 text-sm"
                {...register("customerName", {
                  required: "Your name is required.",
                })}
              />
              {errors.customerName && (
                <p role="alert" className="mt-1 text-xs text-red-600">
                  {errors.customerName.message}
                </p>
              )}
            </div>
            <div>
              <label htmlFor="shipLine1" className="text-xs font-medium">
                Street address
              </label>
              <input
                id="shipLine1"
                className="mt-1 h-10 w-full rounded-lg border px-3 text-sm"
                {...register("shipLine1", {
                  required: "Street address is required.",
                })}
              />
              {errors.shipLine1 && (
                <p role="alert" className="mt-1 text-xs text-red-600">
                  {errors.shipLine1.message}
                </p>
              )}
            </div>
            <div>
              <label htmlFor="cityState" className="text-xs font-medium">
                City, State ZIP
              </label>
              <input
                id="cityState"
                className="mt-1 h-10 w-full rounded-lg border px-3 text-sm"
                placeholder="Oakland, CA 94612"
                {...register("cityState", {
                  required: "City/state is required.",
                })}
              />
              {errors.cityState && (
                <p role="alert" className="mt-1 text-xs text-red-600">
                  {errors.cityState.message}
                </p>
              )}
            </div>

            <fieldset className="space-y-2 pt-1">
              <legend className="text-xs font-medium">Delivery method</legend>
              {SHIPPING_OPTIONS.map((o) => (
                <label
                  key={o.id}
                  className={`flex cursor-pointer items-center gap-3 rounded-lg border px-3.5 py-2.5 ${shippingId === o.id ? "border-foreground bg-muted" : ""}`}
                >
                  <input
                    type="radio"
                    value={o.id}
                    {...register("shippingId")}
                    className="accent-foreground"
                  />
                  <span className="grow">
                    <span className="block text-sm font-semibold">
                      {o.label}
                    </span>
                    <span className="block text-xs text-muted-foreground">
                      {o.detail}
                    </span>
                  </span>
                  <span className="text-sm font-semibold tabular-nums">
                    {o.price === 0 ? "Free" : money(o.price)}
                  </span>
                </label>
              ))}
            </fieldset>

            <button
              type="button"
              onClick={() =>
                next("pay", ["customerName", "shipLine1", "cityState"])
              }
              className="mt-2 w-full rounded-full bg-foreground py-3 text-sm font-semibold text-background"
            >
              Continue to payment →
            </button>
          </section>
        )}

        {showPay && (
          <section className="space-y-3">
            <h2 className="font-display text-2xl">
              Payment <i>details</i>
            </h2>
            <p className="rounded-lg bg-muted px-3 py-2 text-xs text-muted-foreground">
              Demo checkout — nothing is charged. Card details are validated for
              shape only; just the brand and last four digits go on the order.
            </p>
            <div>
              <label htmlFor="cardNumber" className="text-xs font-medium">
                Card number
              </label>
              <input
                id="cardNumber"
                inputMode="numeric"
                placeholder="4242 4242 4242 4242"
                className="mt-1 h-10 w-full rounded-lg border px-3 text-sm tabular-nums"
                {...register("cardNumber", {
                  required: "Card number is required.",
                  validate: (v) =>
                    /^\d{13,19}$/.test(v.replace(/[\s-]/g, "")) ||
                    "That doesn't look like a card number.",
                })}
              />
              {errors.cardNumber && (
                <p role="alert" className="mt-1 text-xs text-red-600">
                  {errors.cardNumber.message}
                </p>
              )}
            </div>
            <div>
              <label htmlFor="cardName" className="text-xs font-medium">
                Name on card
              </label>
              <input
                id="cardName"
                className="mt-1 h-10 w-full rounded-lg border px-3 text-sm"
                {...register("cardName", {
                  required: "Name on card is required.",
                })}
              />
              {errors.cardName && (
                <p role="alert" className="mt-1 text-xs text-red-600">
                  {errors.cardName.message}
                </p>
              )}
            </div>
            <div className="flex gap-2 pt-1">
              <button
                type="button"
                onClick={() => setStep("ship")}
                className="rounded-full border px-5 py-3 text-sm"
              >
                ← Back
              </button>
              <button
                type="button"
                onClick={() => next("review", ["cardNumber", "cardName"])}
                className="grow rounded-full bg-foreground py-3 text-sm font-semibold text-background"
              >
                Review order →
              </button>
            </div>
          </section>
        )}

        {showReview && (
          <section className="space-y-3">
            <h2 className="font-display text-2xl">
              Review &amp; <i>place order</i>
            </h2>
            <dl className="space-y-1.5 rounded-xl border p-4 text-sm">
              <div className="flex justify-between">
                <dt className="text-muted-foreground">Ship to</dt>
                <dd>
                  {getValues("shipLine1")}, {getValues("cityState")}
                </dd>
              </div>
              <div className="flex justify-between">
                <dt className="text-muted-foreground">Contact</dt>
                <dd>{email}</dd>
              </div>
              <div className="flex justify-between">
                <dt className="text-muted-foreground">Delivery</dt>
                <dd>
                  {SHIPPING_OPTIONS.find((o) => o.id === shippingId)?.label}
                </dd>
              </div>
              <div className="flex justify-between">
                <dt className="text-muted-foreground">Pay with</dt>
                <dd>
                  {detectBrand(getValues("cardNumber"))} ····{" "}
                  {getValues("cardNumber").replace(/\D/g, "").slice(-4)}
                </dd>
              </div>
              <div className="mt-2 flex justify-between border-t pt-2 font-semibold">
                <dt>Total</dt>
                <dd className="tabular-nums">{money(totals.total)}</dd>
              </div>
            </dl>
            {serverError && (
              <p
                role="alert"
                className="rounded-lg bg-red-50 px-3 py-2 text-xs text-red-700"
              >
                {serverError}
              </p>
            )}
            <div className="flex gap-2">
              <button
                type="button"
                onClick={() => setStep("pay")}
                className="rounded-full border px-5 py-3 text-sm"
              >
                ← Back
              </button>
              <button
                type="button"
                onClick={submit}
                disabled={pending}
                className="grow rounded-full bg-foreground py-3 text-sm font-semibold text-background disabled:opacity-50"
              >
                {pending
                  ? "Placing order…"
                  : `Place order · ${money(totals.total)}`}
              </button>
            </div>
          </section>
        )}
      </form>
    </div>
  );
}
