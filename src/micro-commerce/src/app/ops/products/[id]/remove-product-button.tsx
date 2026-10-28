"use client";

import { useRouter } from "next/navigation";
import { useState } from "react";
import { Button } from "@/components/ui/button";
import { errorMessage } from "@/lib/api";
import { removeProduct } from "@/lib/catalog";

// Removes the Product from the Marketplace after the operator confirms, then re-reads the page.
// Removal is final: its Merchant can't edit or publish it again.
export function RemoveProductButton({ product }: { product: { id: string; name: string } }) {
  const router = useRouter();
  const [confirming, setConfirming] = useState(false);
  const [isPending, setIsPending] = useState(false);
  const [error, setError] = useState<string>();

  async function handleRemove() {
    setIsPending(true);
    setError(undefined);
    try {
      await removeProduct(product.id);
      router.refresh();
    } catch (e) {
      setError(errorMessage(e));
    } finally {
      setIsPending(false);
    }
  }

  if (!confirming) {
    return (
      <Button variant="destructive" size="sm" onClick={() => setConfirming(true)}>
        Remove
      </Button>
    );
  }

  return (
    <div className="flex flex-col items-end gap-2">
      <p className="text-sm">Remove for good? The Merchant can&apos;t undo this.</p>
      <div className="flex gap-2">
        <Button variant="outline" size="sm" disabled={isPending} onClick={() => setConfirming(false)}>
          Cancel
        </Button>
        <Button
          variant="destructive"
          size="sm"
          aria-label={`Remove ${product.name} for good`}
          disabled={isPending}
          onClick={() => void handleRemove()}
        >
          Remove for good
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
