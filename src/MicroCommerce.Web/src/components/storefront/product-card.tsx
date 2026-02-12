"use client";

import Image from "next/image";
import Link from "next/link";
import { CheckCircle, ShoppingCart } from "lucide-react";
import { toast } from "sonner";

import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Skeleton } from "@/components/ui/skeleton";
import { StarRatingDisplay } from "@/components/reviews/star-rating-display";
import type { ProductDto, StockInfoDto } from "@/lib/api";

interface ProductCardProps {
  product: ProductDto;
  stockInfo?: StockInfoDto;
}

function formatPrice(price: number, currency: string): string {
  return new Intl.NumberFormat("en-US", {
    style: "currency",
    currency,
  }).format(price);
}

function StockBadge({ stockInfo }: { stockInfo?: StockInfoDto }) {
  if (!stockInfo) return null;

  const { availableQuantity } = stockInfo;

  if (availableQuantity === 0) {
    return (
      <Badge variant="destructive" className="text-[10px] font-semibold">
        Out of Stock
      </Badge>
    );
  }

  if (availableQuantity <= 10) {
    return (
      <Badge className="bg-amber-100 text-[10px] font-semibold text-amber-800 hover:bg-amber-100">
        Only {availableQuantity} left!
      </Badge>
    );
  }

  return (
    <Badge className="bg-emerald-50 text-[10px] font-semibold text-emerald-700 hover:bg-emerald-50">
      <CheckCircle className="mr-0.5 size-3" />
      In Stock
    </Badge>
  );
}

export function ProductCard({ product, stockInfo }: ProductCardProps) {
  const isOutOfStock = stockInfo ? stockInfo.availableQuantity === 0 : false;

  return (
    <Link href={`/products/${product.id}`} className="group block">
      <div
        className={`relative overflow-hidden rounded-xl bg-white shadow-sm transition-shadow duration-300 group-hover:shadow-md ${
          isOutOfStock ? "opacity-60" : ""
        }`}
      >
        {/* Image area */}
        <div className="relative aspect-square overflow-hidden bg-gradient-to-br from-zinc-100 to-zinc-50">
          {product.imageUrl ? (
            <Image
              src={product.imageUrl}
              alt={product.name}
              fill
              sizes="(max-width: 640px) 100vw, (max-width: 1024px) 50vw, (max-width: 1280px) 33vw, 25vw"
              className={`object-cover transition-transform duration-500 group-hover:scale-105 ${
                isOutOfStock ? "grayscale" : ""
              }`}
            />
          ) : (
            <div className="flex h-full items-center justify-center">
              <span className="text-4xl text-zinc-300">
                <ShoppingCart className="size-12" />
              </span>
            </div>
          )}

          {/* Stock badge - top right of image */}
          {stockInfo && (
            <div className="absolute right-2 top-2">
              <StockBadge stockInfo={stockInfo} />
            </div>
          )}

          {/* Out of stock overlay */}
          {isOutOfStock && (
            <div className="absolute inset-0 bg-white/30" />
          )}

          {/* Hover overlay with Add to Cart */}
          {!isOutOfStock && (
            <div className="absolute inset-0 flex items-end justify-center bg-black/0 pb-4 opacity-0 transition-all duration-300 group-hover:bg-black/20 group-hover:opacity-100">
              <Button
                size="sm"
                className="translate-y-2 rounded-full transition-transform duration-300 group-hover:translate-y-0"
                onClick={(e) => {
                  e.preventDefault();
                  e.stopPropagation();
                  toast("Cart coming soon!");
                }}
              >
                <ShoppingCart className="mr-1 size-4" />
                Add to Cart
              </Button>
            </div>
          )}
        </div>

        {/* Product info */}
        <div className="p-4">
          <Badge
            variant="secondary"
            className="mb-2 text-[10px] font-medium uppercase tracking-wider"
          >
            {product.categoryName}
          </Badge>
          <h3 className="line-clamp-2 text-sm font-medium text-zinc-900">
            {product.name}
          </h3>
          {product.reviewCount > 0 && product.averageRating !== null && (
            <div className="mt-1.5">
              <StarRatingDisplay
                rating={product.averageRating}
                count={product.reviewCount}
                size="sm"
              />
            </div>
          )}
          <p className="mt-1 text-sm font-semibold text-zinc-900">
            {formatPrice(product.price, product.priceCurrency)}
          </p>
        </div>
      </div>
    </Link>
  );
}

export function ProductCardSkeleton() {
  return (
    <div className="overflow-hidden rounded-xl bg-white shadow-sm">
      <Skeleton className="aspect-square rounded-none" />
      <div className="p-4">
        <Skeleton className="mb-2 h-4 w-16 rounded-full" />
        <Skeleton className="h-4 w-3/4" />
        <Skeleton className="mt-2 h-4 w-1/4" />
      </div>
    </div>
  );
}
