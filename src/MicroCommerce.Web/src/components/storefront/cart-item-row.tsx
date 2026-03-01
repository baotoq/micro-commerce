"use client";

import { Minus, Package, Plus, Trash2 } from "lucide-react";
import Image from "next/image";
import Link from "next/link";
import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
  AlertDialogTrigger,
} from "@/components/ui/alert-dialog";
import { Button } from "@/components/ui/button";
import type { CartItemDto } from "@/lib/api";

interface CartItemRowProps {
  item: CartItemDto;
  onUpdateQuantity: (quantity: number) => void;
  onRemove: () => void;
  isUpdating: boolean;
  isRemoving: boolean;
}

function formatPrice(price: number): string {
  return new Intl.NumberFormat("en-US", {
    style: "currency",
    currency: "USD",
  }).format(price);
}

export function CartItemRow({
  item,
  onUpdateQuantity,
  onRemove,
  isUpdating,
  isRemoving,
}: CartItemRowProps) {
  const isBusy = isUpdating || isRemoving;

  return (
    <div className="flex items-center gap-4 border-b border-border p-5 last:border-b-0 max-md:flex-wrap">
      {/* Product image - 100x100 thumbnail */}
      <Link
        href={`/products/${item.productId}`}
        className="relative size-[100px] shrink-0 overflow-hidden rounded-md bg-muted"
      >
        {item.imageUrl ? (
          <Image
            src={item.imageUrl}
            alt={item.productName}
            fill
            sizes="100px"
            className="object-cover"
          />
        ) : (
          <div className="flex h-full items-center justify-center">
            <Package className="size-8 text-muted-foreground" />
          </div>
        )}
      </Link>

      {/* Product info */}
      <div className="min-w-0 flex-1 space-y-1">
        <Link
          href={`/products/${item.productId}`}
          className="text-[15px] font-semibold text-foreground transition-colors hover:text-primary"
        >
          <span className="line-clamp-1">{item.productName}</span>
        </Link>
        <p className="text-[13px] text-muted-foreground">
          {formatPrice(item.unitPrice)} each
        </p>
        <p className="text-[15px] font-semibold text-foreground md:hidden">
          {formatPrice(item.unitPrice)}
        </p>
      </div>

      {/* Quantity stepper - inline implementation matching design-system.pen QuantityStepper */}
      <div className="inline-flex h-9 items-center rounded-md border border-border">
        <button
          type="button"
          onClick={() => onUpdateQuantity(item.quantity - 1)}
          disabled={item.quantity <= 1 || isBusy}
          className="flex h-full w-9 items-center justify-center text-foreground transition-colors hover:bg-muted disabled:opacity-50"
          aria-label="Decrease quantity"
        >
          <Minus className="size-3.5" />
        </button>
        <span className="flex h-full w-12 items-center justify-center border-x border-border text-sm font-medium tabular-nums">
          {item.quantity}
        </span>
        <button
          type="button"
          onClick={() => onUpdateQuantity(item.quantity + 1)}
          disabled={item.quantity >= 99 || isBusy}
          className="flex h-full w-9 items-center justify-center text-foreground transition-colors hover:bg-muted disabled:opacity-50"
          aria-label="Increase quantity"
        >
          <Plus className="size-3.5" />
        </button>
      </div>

      {/* Line total */}
      <p className="w-24 text-right text-base font-bold text-foreground max-md:hidden">
        {formatPrice(item.lineTotal)}
      </p>

      {/* Remove button with confirmation */}
      <AlertDialog>
        <AlertDialogTrigger asChild>
          <Button
            variant="ghost"
            size="icon"
            className="size-9 shrink-0 text-muted-foreground hover:text-destructive"
            disabled={isBusy}
            aria-label={`Remove ${item.productName}`}
          >
            <Trash2 className="size-[18px]" />
          </Button>
        </AlertDialogTrigger>
        <AlertDialogContent>
          <AlertDialogHeader>
            <AlertDialogTitle>Remove item?</AlertDialogTitle>
            <AlertDialogDescription>
              This will remove {item.productName} from your cart.
            </AlertDialogDescription>
          </AlertDialogHeader>
          <AlertDialogFooter>
            <AlertDialogCancel>Cancel</AlertDialogCancel>
            <AlertDialogAction variant="destructive" onClick={onRemove}>
              Remove
            </AlertDialogAction>
          </AlertDialogFooter>
        </AlertDialogContent>
      </AlertDialog>
    </div>
  );
}
