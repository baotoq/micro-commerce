"use client";

import { useState, useTransition } from "react";
import { money } from "@/lib/money";
import { addToCart } from "@/lib/storefront/actions";

export function AddToBag({
  sku,
  price,
  maxQty,
}: {
  sku: string;
  price: number;
  maxQty: number;
}) {
  const [qty, setQty] = useState(1);
  const [status, setStatus] = useState<"idle" | "added" | "error">("idle");
  const [pending, startTransition] = useTransition();

  const submit = () =>
    startTransition(async () => {
      const result = await addToCart(sku, qty);
      setStatus(result.ok ? "added" : "error");
    });

  return (
    <div>
      <div className="flex gap-2">
        <div className="flex h-11 items-center rounded-full border px-2">
          <button
            type="button"
            aria-label="Decrease quantity"
            className="px-2 text-lg leading-none disabled:opacity-30"
            disabled={qty <= 1}
            onClick={() => setQty((q) => Math.max(1, q - 1))}
          >
            −
          </button>
          <span className="w-6 text-center text-sm font-semibold tabular-nums">
            {qty}
          </span>
          <button
            type="button"
            aria-label="Increase quantity"
            className="px-2 text-lg leading-none disabled:opacity-30"
            disabled={qty >= maxQty}
            onClick={() => setQty((q) => Math.min(maxQty, q + 1))}
          >
            +
          </button>
        </div>
        <button
          type="button"
          onClick={submit}
          disabled={pending || maxQty < 1}
          className="h-11 flex-1 rounded-full bg-foreground text-sm font-semibold text-background disabled:opacity-50"
        >
          {pending ? "Adding…" : `Add to bag · ${money(price * qty)}`}
        </button>
      </div>
      <p aria-live="polite" className="mt-2 min-h-5 text-xs">
        {status === "added" && (
          <span className="text-green-700">Added to bag ✓</span>
        )}
        {status === "error" && (
          <span className="text-red-600">This piece just sold out.</span>
        )}
      </p>
    </div>
  );
}
