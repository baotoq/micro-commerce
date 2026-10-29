"use client";

import { useRouter } from "next/navigation";
import { useState } from "react";
import { toast } from "sonner";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { errorMessage } from "@/lib/api";
import { type CartLine, maxQuantity, removeCartLine, updateCartLine } from "@/lib/cart";

// Changes a Cart line's quantity or removes it, then re-reads the page.
export function CartLineControls({ line }: { line: CartLine }) {
  const router = useRouter();
  const [quantity, setQuantity] = useState(line.quantity);
  const [isPending, setIsPending] = useState(false);
  const [error, setError] = useState<string>();
  const name = [line.productName ?? "item", ...line.optionValues].join(" ");

  async function run(change: () => Promise<unknown>, message: string) {
    setIsPending(true);
    setError(undefined);
    try {
      await change();
      toast.success(message);
      router.refresh();
    } catch (e) {
      setError(errorMessage(e));
    } finally {
      setIsPending(false);
    }
  }

  return (
    <div className="grid justify-items-end gap-1">
      <div className="flex items-center gap-2">
        <Input
          type="number"
          className="w-16"
          aria-label={`Quantity of ${name}`}
          min={1}
          max={maxQuantity}
          value={quantity}
          disabled={isPending}
          onChange={(e) => setQuantity(Number(e.target.value))}
          onBlur={() => {
            if (quantity !== line.quantity) void run(() => updateCartLine(line.variantId, quantity), "Cart updated.");
          }}
        />
        <Button
          variant="outline"
          size="sm"
          aria-label={`Remove ${name}`}
          disabled={isPending}
          onClick={() => void run(() => removeCartLine(line.variantId), "Removed from your Cart.")}
        >
          Remove
        </Button>
      </div>
      {error && (
        <p role="alert" className="text-sm text-destructive">
          {error}
        </p>
      )}
    </div>
  );
}
