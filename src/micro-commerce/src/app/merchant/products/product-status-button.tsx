"use client";

import { useRouter } from "next/navigation";
import { useState } from "react";
import { Button } from "@/components/ui/button";
import { ApiError } from "@/lib/api";
import { type ProductStatus, publishProduct, unpublishProduct } from "@/lib/catalog";

// Publishes a Draft Product or moves a Published one back to Draft, then re-reads the page.
export function ProductStatusButton({ product }: { product: { id: string; name: string; status: ProductStatus } }) {
  const router = useRouter();
  const [isPending, setIsPending] = useState(false);
  const [error, setError] = useState<string>();
  const action = product.status === "Published" ? "Unpublish" : "Publish";

  async function handleClick() {
    setIsPending(true);
    setError(undefined);
    try {
      await (product.status === "Published" ? unpublishProduct : publishProduct)(product.id);
      router.refresh();
    } catch (e) {
      setError(e instanceof ApiError ? e.message : "Something went wrong. Please try again.");
    } finally {
      setIsPending(false);
    }
  }

  return (
    <>
      <Button
        variant="outline"
        size="sm"
        aria-label={`${action} ${product.name}`}
        disabled={isPending}
        onClick={() => void handleClick()}
      >
        {action}
      </Button>
      {error && (
        <p role="alert" className="text-sm text-destructive">
          {error}
        </p>
      )}
    </>
  );
}
