"use client";

import { useState, useTransition } from "react";
import { applyPromoCode, removePromoCode } from "@/lib/storefront/actions";

export function PromoForm({ appliedCode }: { appliedCode?: string }) {
  const [error, setError] = useState<string | null>(null);
  const [pending, startTransition] = useTransition();

  if (appliedCode) {
    return (
      <div className="flex items-center justify-between rounded-lg border border-dashed px-3 py-2.5 text-xs">
        <span className="font-semibold text-green-700">
          {appliedCode} applied ✓
        </span>
        <button
          type="button"
          className="text-muted-foreground hover:text-foreground"
          onClick={() =>
            startTransition(async () => void (await removePromoCode()))
          }
        >
          Remove
        </button>
      </div>
    );
  }

  return (
    <form
      className="flex items-center gap-2 rounded-lg border border-dashed px-3 py-2"
      onSubmit={(e) => {
        e.preventDefault();
        const code = String(new FormData(e.currentTarget).get("code") ?? "");
        startTransition(async () => {
          const result = await applyPromoCode(code);
          setError(result.ok ? null : "That code isn’t valid right now.");
        });
      }}
    >
      <input
        name="code"
        placeholder="Promo code"
        aria-label="Promo code"
        className="h-7 grow bg-transparent text-xs outline-none"
      />
      <button
        type="submit"
        disabled={pending}
        className="text-xs font-semibold disabled:opacity-50"
      >
        Apply
      </button>
      {error && (
        <span role="alert" className="text-xs text-red-600">
          {error}
        </span>
      )}
    </form>
  );
}
