"use client";

import { CheckCircle, Heart, ShoppingCart } from "lucide-react";
import Image from "next/image";
import Link from "next/link";
import { StarRatingDisplay } from "@/components/reviews/star-rating-display";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Card, CardContent } from "@/components/ui/card";
import { Skeleton } from "@/components/ui/skeleton";
import { WishlistToggleButton } from "@/components/wishlist/wishlist-toggle-button";
import { useAddToCart } from "@/hooks/use-cart";
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
      <Badge className="bg-warning-bg text-[10px] font-semibold text-warning-foreground hover:bg-warning-bg">
        Only {availableQuantity} left!
      </Badge>
    );
  }

  return (
    <Badge className="bg-success-bg text-[10px] font-semibold text-success-foreground hover:bg-success-bg">
      <CheckCircle className="mr-0.5 size-3" />
      In Stock
    </Badge>
  );
}

export function ProductCard({ product, stockInfo }: ProductCardProps) {
  const isOutOfStock = stockInfo ? stockInfo.availableQuantity === 0 : false;
  const addToCart = useAddToCart();

  return (
    <Link href={`/products/${product.id}`} className="group block">
      <Card
        className={`overflow-hidden border-border py-0 transition-shadow duration-300 group-hover:shadow-md ${
          isOutOfStock ? "opacity-60" : ""
        }`}
      >
        {/* Image area */}
        <div className="relative aspect-[4/3] overflow-hidden bg-muted">
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
              <ShoppingCart className="size-12 text-border" />
            </div>
          )}

          {/* Wishlist heart - top left of image */}
          <div className="absolute left-2 top-2 z-10">
            <WishlistToggleButton productId={product.id} />
          </div>

          {/* Stock badge - top right of image */}
          {stockInfo && (
            <div className="absolute right-2 top-2">
              <StockBadge stockInfo={stockInfo} />
            </div>
          )}

          {/* Out of stock overlay */}
          {isOutOfStock && (
            <div className="absolute inset-0 bg-background/30" />
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
                  addToCart.mutate({
                    productId: product.id,
                    productName: product.name,
                    unitPrice: product.price,
                    imageUrl: product.imageUrl,
                    quantity: 1,
                  });
                }}
                disabled={addToCart.isPending}
              >
                <ShoppingCart className="mr-1 size-4" />
                Add to Cart
              </Button>
            </div>
          )}
        </div>

        {/* Product info */}
        <CardContent className="flex flex-col gap-2 p-4">
          <h3 className="line-clamp-2 text-sm font-semibold text-foreground">
            {product.name}
          </h3>

          <div className="flex items-center justify-between">
            <p className="text-base font-bold text-foreground">
              {formatPrice(product.price, product.priceCurrency)}
            </p>
            <Heart className="size-[18px] text-muted-foreground" />
          </div>

          {product.reviewCount > 0 && product.averageRating !== null && (
            <StarRatingDisplay
              rating={product.averageRating}
              count={product.reviewCount}
              size="sm"
            />
          )}

          <Button
            className="mt-1 w-full"
            onClick={(e) => {
              e.preventDefault();
              e.stopPropagation();
              addToCart.mutate({
                productId: product.id,
                productName: product.name,
                unitPrice: product.price,
                imageUrl: product.imageUrl,
                quantity: 1,
              });
            }}
            disabled={isOutOfStock || addToCart.isPending}
          >
            {addToCart.isPending ? (
              <>
                <ShoppingCart className="mr-1.5 size-4 animate-pulse" />
                Adding...
              </>
            ) : (
              <>
                <ShoppingCart className="mr-1.5 size-4" />
                Add to Cart
              </>
            )}
          </Button>
        </CardContent>
      </Card>
    </Link>
  );
}

export function ProductCardSkeleton() {
  return (
    <Card className="overflow-hidden py-0">
      <Skeleton className="aspect-[4/3] rounded-none" />
      <div className="p-4">
        <Skeleton className="mb-2 h-4 w-3/4" />
        <Skeleton className="mb-2 h-5 w-1/3" />
        <Skeleton className="h-9 w-full" />
      </div>
    </Card>
  );
}
