"use client";

import Image from "next/image";
import Link from "next/link";
import { ShoppingCart } from "lucide-react";
import { toast } from "sonner";

import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Skeleton } from "@/components/ui/skeleton";
import type { ProductDto } from "@/lib/api";

interface ProductCardProps {
  product: ProductDto;
}

function formatPrice(price: number, currency: string): string {
  return new Intl.NumberFormat("en-US", {
    style: "currency",
    currency,
  }).format(price);
}

export function ProductCard({ product }: ProductCardProps) {
  const isOutOfStock = product.status !== "Published";

  return (
    <Link href={`/products/${product.id}`} className="group block">
      <div
        className={`relative overflow-hidden rounded-xl bg-white shadow-sm transition-shadow duration-300 group-hover:shadow-md ${
          isOutOfStock ? "opacity-60 grayscale" : ""
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
              className="object-cover transition-transform duration-500 group-hover:scale-105"
            />
          ) : (
            <div className="flex h-full items-center justify-center">
              <span className="text-4xl text-zinc-300">
                <ShoppingCart className="size-12" />
              </span>
            </div>
          )}

          {/* Out of stock overlay */}
          {isOutOfStock && (
            <div className="absolute inset-0 flex items-center justify-center bg-white/50">
              <Badge variant="secondary" className="text-xs font-semibold">
                Out of Stock
              </Badge>
            </div>
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
