"use client";

import Link from "next/link";
import { useState } from "react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { errorMessage } from "@/lib/api";
import { addToCart, maxQuantity } from "@/lib/cart";
import type { StorefrontProduct } from "@/lib/storefront";

// Picks a Variant by its option values and adds it to the signed-in Account's Cart.
export function AddToCart({ product }: { product: StorefrontProduct }) {
  const [selected, setSelected] = useState<string[]>(() => product.variants[0]?.optionValues ?? []);
  const [quantity, setQuantity] = useState(1);
  const [isPending, setIsPending] = useState(false);
  const [added, setAdded] = useState(false);
  const [error, setError] = useState<string>();

  const variant = product.variants.find((v) => v.optionValues.every((value, i) => value === selected[i]));

  async function handleSubmit(event: React.FormEvent) {
    event.preventDefault();
    if (!variant) return;
    setIsPending(true);
    setAdded(false);
    setError(undefined);
    try {
      await addToCart(variant.id, quantity);
      setAdded(true);
    } catch (e) {
      setError(errorMessage(e));
    } finally {
      setIsPending(false);
    }
  }

  return (
    <form className="mt-6 flex flex-wrap items-end gap-4" onSubmit={(e) => void handleSubmit(e)}>
      {product.options.map((option, i) => (
        <div key={option.name} className="grid gap-1.5">
          <Label htmlFor={`option-${i}`}>{option.name}</Label>
          <select
            id={`option-${i}`}
            className="h-8 rounded-lg border border-input bg-transparent px-2.5 text-sm"
            value={selected[i]}
            onChange={(e) => {
              setAdded(false);
              setSelected((current) => current.map((value, j) => (j === i ? e.target.value : value)));
            }}
          >
            {option.values.map((value) => (
              <option key={value}>{value}</option>
            ))}
          </select>
        </div>
      ))}
      <div className="grid w-20 gap-1.5">
        <Label htmlFor="quantity">Quantity</Label>
        <Input
          id="quantity"
          type="number"
          min={1}
          max={maxQuantity}
          value={quantity}
          onChange={(e) => setQuantity(Number(e.target.value))}
        />
      </div>
      <Button type="submit" disabled={!variant || isPending}>
        Add to cart
      </Button>
      {added && (
        <p role="status" className="text-sm">
          Added to your Cart.{" "}
          <Link href="/cart" className="underline">
            View Cart
          </Link>
        </p>
      )}
      {error && (
        <p role="alert" className="text-sm text-destructive">
          {error}
        </p>
      )}
    </form>
  );
}
