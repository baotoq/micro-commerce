"use client";

import { ShoppingBag } from "lucide-react";
import Link from "next/link";
import { CartItemRow } from "@/components/storefront/cart-item-row";
import { CartSummary } from "@/components/storefront/cart-summary";
import { Button } from "@/components/ui/button";
import { Skeleton } from "@/components/ui/skeleton";
import {
  useCart,
  useRemoveCartItem,
  useUpdateCartItem,
} from "@/hooks/use-cart";

export function CartPage() {
  const { data: cart, isLoading } = useCart();
  const updateItem = useUpdateCartItem();
  const removeItem = useRemoveCartItem();

  if (isLoading) {
    return <CartPageSkeleton />;
  }

  const items = cart?.items ?? [];
  const itemCount = items.reduce((sum, item) => sum + item.quantity, 0);

  if (items.length === 0) {
    return (
      <div className="mx-auto max-w-7xl px-4 py-8 sm:px-10 lg:px-20">
        <div className="flex flex-col items-center justify-center py-24 text-center">
          <div className="mb-6 flex size-20 items-center justify-center rounded-full bg-muted">
            <ShoppingBag className="size-10 text-muted-foreground" />
          </div>
          <h2 className="text-xl font-bold text-foreground">
            Your cart is empty
          </h2>
          <p className="mt-2 text-sm text-muted-foreground">
            Looks like you haven&apos;t added anything yet.
          </p>
          <Button asChild className="mt-6" size="lg">
            <Link href="/">Continue Shopping</Link>
          </Button>
        </div>
      </div>
    );
  }

  return (
    <div className="mx-auto max-w-7xl px-4 py-8 sm:px-10 lg:px-20">
      {/* Page body: items + summary */}
      <div className="flex flex-col gap-10 lg:flex-row">
        {/* Cart items - main column */}
        <div className="flex-1 space-y-6">
          {/* Cart title */}
          <div className="flex items-center gap-2">
            <h1 className="text-2xl font-bold text-foreground">
              Shopping Cart
            </h1>
            <span className="text-base text-muted-foreground">
              ({itemCount} {itemCount === 1 ? "item" : "items"})
            </span>
          </div>

          {/* Items list in bordered container */}
          <div className="overflow-hidden rounded-lg border border-border">
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
        </div>

        {/* Order summary sidebar */}
        <div className="w-full shrink-0 lg:w-[380px]">
          <CartSummary items={items} />
        </div>
      </div>
    </div>
  );
}

function CartPageSkeleton() {
  return (
    <div className="mx-auto max-w-7xl px-4 py-8 sm:px-10 lg:px-20">
      <div className="flex flex-col gap-10 lg:flex-row">
        <div className="flex-1 space-y-6">
          <div className="flex items-center gap-2">
            <Skeleton className="h-8 w-48" />
            <Skeleton className="h-5 w-20" />
          </div>
          <div className="overflow-hidden rounded-lg border border-border">
            {["sk-1", "sk-2", "sk-3"].map((key) => (
              <div
                key={key}
                className="flex items-center gap-4 border-b border-border p-5 last:border-b-0"
              >
                <Skeleton className="size-[100px] shrink-0 rounded-md" />
                <div className="flex flex-1 flex-col gap-2">
                  <Skeleton className="h-4 w-2/3" />
                  <Skeleton className="h-3 w-24" />
                  <Skeleton className="h-4 w-16" />
                </div>
                <Skeleton className="h-9 w-[120px] rounded-md" />
                <Skeleton className="h-5 w-20" />
                <Skeleton className="size-9 rounded-md" />
              </div>
            ))}
          </div>
        </div>
        <div className="w-full shrink-0 lg:w-[380px]">
          <Skeleton className="h-72 rounded-lg" />
        </div>
      </div>
    </div>
  );
}
