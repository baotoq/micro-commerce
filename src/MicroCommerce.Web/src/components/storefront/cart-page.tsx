"use client";

import Link from "next/link";
import { ShoppingCart } from "lucide-react";

import { Button } from "@/components/ui/button";
import { Skeleton } from "@/components/ui/skeleton";
import { CartItemRow } from "@/components/storefront/cart-item-row";
import { CartSummary } from "@/components/storefront/cart-summary";
import { useCart, useUpdateCartItem, useRemoveCartItem } from "@/hooks/use-cart";

export function CartPage() {
  const { data: cart, isLoading } = useCart();
  const updateItem = useUpdateCartItem();
  const removeItem = useRemoveCartItem();

  if (isLoading) {
    return <CartPageSkeleton />;
  }

  const items = cart?.items ?? [];

  if (items.length === 0) {
    return (
      <div className="flex flex-col items-center justify-center py-24 text-center">
        <ShoppingCart className="mb-4 size-12 text-zinc-300" />
        <h2 className="text-xl font-semibold text-zinc-900">
          Your cart is empty
        </h2>
        <p className="mt-2 text-sm text-zinc-500">
          Looks like you haven&apos;t added anything yet.
        </p>
        <Button asChild className="mt-6 rounded-full" size="lg">
          <Link href="/">Continue shopping</Link>
        </Button>
      </div>
    );
  }

  return (
    <div>
      <h1 className="mb-8 text-2xl font-bold tracking-tight text-zinc-900">
        Cart
      </h1>

      <div className="grid grid-cols-1 gap-8 lg:grid-cols-3">
        {/* Items list */}
        <div className="space-y-4 lg:col-span-2">
          {items.map((item) => (
            <CartItemRow
              key={item.id}
              item={item}
              onUpdateQuantity={(quantity) =>
                updateItem.mutate({ itemId: item.id, quantity })
              }
              onRemove={() => removeItem.mutate(item.id)}
              isUpdating={updateItem.isPending}
              isRemoving={removeItem.isPending}
            />
          ))}
        </div>

        {/* Order summary */}
        <div>
          <CartSummary items={items} />
        </div>
      </div>
    </div>
  );
}

function CartPageSkeleton() {
  return (
    <div>
      <Skeleton className="mb-8 h-8 w-20" />
      <div className="grid grid-cols-1 gap-8 lg:grid-cols-3">
        <div className="space-y-4 lg:col-span-2">
          {Array.from({ length: 3 }).map((_, i) => (
            <div
              key={i}
              className="flex gap-4 rounded-lg border border-zinc-200 p-4"
            >
              <Skeleton className="size-16 shrink-0 rounded-md" />
              <div className="flex flex-1 flex-col gap-2">
                <Skeleton className="h-4 w-2/3" />
                <Skeleton className="h-4 w-20" />
              </div>
              <Skeleton className="h-8 w-24" />
            </div>
          ))}
        </div>
        <div>
          <Skeleton className="h-48 rounded-xl" />
        </div>
      </div>
    </div>
  );
}
