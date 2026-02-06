"use client";

import Image from "next/image";
import Link from "next/link";
import { Minus, Plus, Trash2, Package } from "lucide-react";

import { Button } from "@/components/ui/button";
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
    <div className="flex items-center gap-4 rounded-lg border border-zinc-200 p-4 transition-colors hover:border-zinc-300">
      {/* Thumbnail */}
      <Link
        href={`/products/${item.productId}`}
        className="relative size-16 shrink-0 overflow-hidden rounded-md bg-gradient-to-br from-zinc-100 to-zinc-50"
      >
        {item.imageUrl ? (
          <Image
            src={item.imageUrl}
            alt={item.productName}
            fill
            sizes="64px"
            className="object-cover"
          />
        ) : (
          <div className="flex h-full items-center justify-center">
            <Package className="size-6 text-zinc-300" />
          </div>
        )}
      </Link>

      {/* Product info */}
      <div className="flex min-w-0 flex-1 flex-col gap-1 sm:flex-row sm:items-center sm:gap-4">
        <div className="min-w-0 flex-1">
          <Link
            href={`/products/${item.productId}`}
            className="text-sm font-medium text-zinc-900 transition-colors hover:text-zinc-600"
          >
            <span className="line-clamp-1">{item.productName}</span>
          </Link>
          <p className="text-xs text-zinc-500">
            {formatPrice(item.unitPrice)} each
          </p>
        </div>

        {/* Quantity stepper */}
        <div className="flex items-center gap-1.5">
          <Button
            variant="outline"
            size="icon"
            className="size-7"
            disabled={item.quantity <= 1 || isBusy}
            onClick={() => onUpdateQuantity(item.quantity - 1)}
            aria-label="Decrease quantity"
          >
            <Minus className="size-3" />
          </Button>
          <span className="w-8 text-center text-sm font-medium tabular-nums text-zinc-900">
            {item.quantity}
          </span>
          <Button
            variant="outline"
            size="icon"
            className="size-7"
            disabled={item.quantity >= 99 || isBusy}
            onClick={() => onUpdateQuantity(item.quantity + 1)}
            aria-label="Increase quantity"
          >
            <Plus className="size-3" />
          </Button>
        </div>

        {/* Line total */}
        <p className="text-sm font-semibold text-zinc-900 sm:w-20 sm:text-right">
          {formatPrice(item.lineTotal)}
        </p>
      </div>

      {/* Remove button with confirmation */}
      <AlertDialog>
        <AlertDialogTrigger asChild>
          <Button
            variant="ghost"
            size="icon"
            className="size-8 shrink-0 text-zinc-400 hover:text-red-500"
            disabled={isBusy}
            aria-label={`Remove ${item.productName}`}
          >
            <Trash2 className="size-4" />
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
