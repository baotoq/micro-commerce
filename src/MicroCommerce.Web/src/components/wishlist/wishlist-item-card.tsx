"use client";

import Image from "next/image";
import Link from "next/link";
import { ShoppingCart } from "lucide-react";
import { toast } from "sonner";

import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { StarRatingDisplay } from "@/components/reviews/star-rating-display";
import { WishlistToggleButton } from "./wishlist-toggle-button";
import { useAddToCart } from "@/hooks/use-cart";
import type { WishlistItemDto } from "@/lib/api";

interface WishlistItemCardProps {
  item: WishlistItemDto;
}

function formatPrice(price: number, currency: string): string {
  return new Intl.NumberFormat("en-US", {
    style: "currency",
    currency,
  }).format(price);
}

export function WishlistItemCard({ item }: WishlistItemCardProps) {
  const addToCart = useAddToCart();
  const isOutOfStock = item.availableQuantity === 0;

  const handleAddToCart = () => {
    addToCart.mutate(
      {
        productId: item.productId,
        productName: item.productName,
        unitPrice: item.price,
        imageUrl: item.imageUrl,
        quantity: 1,
      },
      {
        onSuccess: () => {
          toast.success("Added to cart");
        },
      }
    );
  };

  return (
    <div
      className={`relative overflow-hidden rounded-xl bg-white shadow-sm transition-shadow duration-300 hover:shadow-md ${
        isOutOfStock ? "opacity-60" : ""
      }`}
    >
      {/* Image area */}
      <div className="relative aspect-square overflow-hidden bg-gradient-to-br from-zinc-100 to-zinc-50">
        <Link href={`/products/${item.productId}`}>
          {item.imageUrl ? (
            <Image
              src={item.imageUrl}
              alt={item.productName}
              fill
              sizes="(max-width: 640px) 100vw, (max-width: 1024px) 50vw, (max-width: 1280px) 33vw, 25vw"
              className={`object-cover transition-transform duration-500 hover:scale-105 ${
                isOutOfStock ? "grayscale" : ""
              }`}
            />
          ) : (
            <div className="flex h-full items-center justify-center">
              <ShoppingCart className="size-12 text-zinc-300" />
            </div>
          )}
        </Link>

        {/* Wishlist heart - top right */}
        <div className="absolute right-2 top-2 z-10">
          <WishlistToggleButton productId={item.productId} />
        </div>

        {/* Out of stock badge */}
        {isOutOfStock && (
          <div className="absolute left-2 top-2">
            <Badge variant="destructive" className="text-[10px] font-semibold">
              Out of Stock
            </Badge>
          </div>
        )}
      </div>

      {/* Product info */}
      <div className="p-4">
        <Link href={`/products/${item.productId}`}>
          <h3 className="line-clamp-2 text-sm font-medium text-zinc-900 hover:text-zinc-700">
            {item.productName}
          </h3>
        </Link>

        {item.reviewCount > 0 && item.averageRating !== null && (
          <div className="mt-1.5">
            <StarRatingDisplay
              rating={item.averageRating}
              count={item.reviewCount}
              size="sm"
            />
          </div>
        )}

        <p className="mt-1 text-sm font-semibold text-zinc-900">
          {formatPrice(item.price, item.currency)}
        </p>

        {/* Add to Cart button */}
        <Button
          size="sm"
          className="mt-3 w-full rounded-full"
          disabled={isOutOfStock || addToCart.isPending}
          onClick={handleAddToCart}
        >
          {isOutOfStock ? (
            "Out of Stock"
          ) : (
            <>
              <ShoppingCart className="mr-1 size-4" />
              Add to Cart
            </>
          )}
        </Button>
      </div>
    </div>
  );
}
