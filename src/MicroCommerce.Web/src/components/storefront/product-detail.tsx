"use client";

import { useEffect, useState } from "react";
import Image from "next/image";
import Link from "next/link";
import { ShoppingCart, Package, CheckCircle, AlertTriangle, XCircle, Loader2 } from "lucide-react";

import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Separator } from "@/components/ui/separator";
import { Skeleton } from "@/components/ui/skeleton";
import { RelatedProducts } from "@/components/storefront/related-products";
import { ReviewList } from "@/components/reviews/review-list";
import { StarRatingDisplay } from "@/components/reviews/star-rating-display";
import { WishlistToggleButton } from "@/components/wishlist/wishlist-toggle-button";
import { useAddToCart } from "@/hooks/use-cart";
import { getProductById, getStockByProductId, type ProductDto, type StockInfoDto } from "@/lib/api";

interface ProductDetailProps {
  productId: string;
}

function formatPrice(price: number, currency: string): string {
  return new Intl.NumberFormat("en-US", {
    style: "currency",
    currency,
  }).format(price);
}

function StockStatus({ stockInfo, loading: stockLoading }: { stockInfo: StockInfoDto | null; loading: boolean }) {
  if (stockLoading) {
    return <Skeleton className="h-6 w-28 rounded-full" />;
  }

  if (!stockInfo || stockInfo.availableQuantity === 0) {
    return (
      <div className="flex items-center gap-1.5 text-sm font-medium text-red-600">
        <XCircle className="size-4" />
        <span>Out of Stock</span>
      </div>
    );
  }

  if (stockInfo.availableQuantity <= 10) {
    return (
      <div className="flex items-center gap-1.5 text-sm font-medium text-amber-600">
        <AlertTriangle className="size-4" />
        <span>Only {stockInfo.availableQuantity} left!</span>
      </div>
    );
  }

  return (
    <div className="flex items-center gap-1.5 text-sm font-medium text-emerald-600">
      <CheckCircle className="size-4" />
      <span>In Stock</span>
    </div>
  );
}

export function ProductDetail({ productId }: ProductDetailProps) {
  const [product, setProduct] = useState<ProductDto | null>(null);
  const [stockInfo, setStockInfo] = useState<StockInfoDto | null>(null);
  const [loading, setLoading] = useState(true);
  const [stockLoading, setStockLoading] = useState(true);
  const [error, setError] = useState(false);
  const addToCart = useAddToCart();

  useEffect(() => {
    let cancelled = false;

    async function fetchProduct() {
      try {
        setLoading(true);
        setError(false);
        const data = await getProductById(productId);
        if (!cancelled) {
          setProduct(data);
        }
      } catch {
        if (!cancelled) {
          setError(true);
        }
      } finally {
        if (!cancelled) {
          setLoading(false);
        }
      }
    }

    async function fetchStock() {
      try {
        setStockLoading(true);
        const data = await getStockByProductId(productId);
        if (!cancelled) {
          setStockInfo(data);
        }
      } catch {
        // Stock fetch failure is non-critical
      } finally {
        if (!cancelled) {
          setStockLoading(false);
        }
      }
    }

    fetchProduct();
    fetchStock();
    return () => {
      cancelled = true;
    };
  }, [productId]);

  const isInStock = stockInfo ? stockInfo.availableQuantity > 0 : !stockLoading;

  if (loading) {
    return <ProductDetailSkeleton />;
  }

  if (error || !product) {
    return (
      <div className="flex flex-col items-center justify-center py-24 text-center">
        <Package className="mb-4 size-12 text-zinc-300" />
        <h2 className="text-xl font-semibold text-zinc-900">
          Product not found
        </h2>
        <p className="mt-2 text-sm text-zinc-500">
          The product you are looking for does not exist or has been removed.
        </p>
        <Link
          href="/"
          className="mt-6 inline-flex items-center rounded-full bg-zinc-900 px-6 py-2.5 text-sm font-medium text-white transition-colors hover:bg-zinc-700"
        >
          Back to Home
        </Link>
      </div>
    );
  }

  return (
    <div>
      {/* Breadcrumb */}
      <nav className="mb-8 flex items-center gap-2 text-sm text-zinc-500">
        <Link href="/" className="transition-colors hover:text-zinc-900">
          Home
        </Link>
        <span>/</span>
        <Link
          href="/#products"
          className="transition-colors hover:text-zinc-900"
        >
          Products
        </Link>
        <span>/</span>
        <span className="text-zinc-900">{product.name}</span>
      </nav>

      {/* Product detail layout */}
      <div className="grid grid-cols-1 gap-10 lg:grid-cols-5 lg:gap-16">
        {/* Image - 60% on desktop */}
        <div className="lg:col-span-3">
          <div className="relative aspect-square overflow-hidden rounded-2xl bg-gradient-to-br from-zinc-100 to-zinc-50">
            {product.imageUrl ? (
              <Image
                src={product.imageUrl}
                alt={product.name}
                fill
                sizes="(max-width: 1024px) 100vw, 60vw"
                className="object-cover"
                priority
              />
            ) : (
              <div className="flex h-full items-center justify-center">
                <Package className="size-24 text-zinc-200" />
              </div>
            )}
          </div>
        </div>

        {/* Info - 40% on desktop */}
        <div className="flex flex-col justify-center lg:col-span-2">
          <Link href="/#products">
            <Badge
              variant="secondary"
              className="mb-4 text-[11px] font-medium uppercase tracking-wider"
            >
              {product.categoryName}
            </Badge>
          </Link>

          <div className="flex items-center justify-between gap-4">
            <h1 className="text-3xl font-bold tracking-tight text-zinc-900">
              {product.name}
            </h1>
            <div className="flex-shrink-0 rounded-full p-2 transition-colors hover:bg-zinc-100">
              <WishlistToggleButton productId={product.id} />
            </div>
          </div>

          {product.reviewCount > 0 && product.averageRating !== null ? (
            <div className="mt-3">
              <StarRatingDisplay
                rating={product.averageRating}
                count={product.reviewCount}
                showCount
              />
            </div>
          ) : (
            <div className="mt-3">
              <span className="text-sm text-zinc-500">No reviews yet</span>
            </div>
          )}

          <p className="mt-4 text-2xl font-semibold text-zinc-900">
            {formatPrice(product.price, product.priceCurrency)}
          </p>

          <div className="mt-3">
            <StockStatus stockInfo={stockInfo} loading={stockLoading} />
          </div>

          {product.description && (
            <p className="mt-6 whitespace-pre-line leading-relaxed text-zinc-500">
              {product.description}
            </p>
          )}

          {product.sku && (
            <p className="mt-4 text-xs text-zinc-400">SKU: {product.sku}</p>
          )}

          <div className="mt-8">
            {isInStock ? (
              <Button
                size="lg"
                className="w-full rounded-full sm:w-auto"
                disabled={addToCart.isPending}
                onClick={() =>
                  addToCart.mutate({
                    productId: product.id,
                    productName: product.name,
                    unitPrice: product.price,
                    imageUrl: product.imageUrl,
                    quantity: 1,
                  })
                }
              >
                {addToCart.isPending ? (
                  <>
                    <Loader2 className="mr-2 size-4 animate-spin" />
                    Adding...
                  </>
                ) : (
                  <>
                    <ShoppingCart className="mr-2 size-4" />
                    Add to Cart
                  </>
                )}
              </Button>
            ) : (
              <div className="rounded-lg border border-red-200 bg-red-50 px-6 py-3 text-center">
                <p className="text-sm font-medium text-red-700">
                  This product is currently out of stock
                </p>
              </div>
            )}
          </div>
        </div>
      </div>

      {/* Related Products */}
      <Separator className="my-16" />

      <RelatedProducts
        categoryId={product.categoryId}
        categoryName={product.categoryName}
        currentProductId={product.id}
      />

      {/* Customer Reviews */}
      <Separator className="my-16" />

      <div id="reviews">
        <h2 className="text-xl font-semibold text-zinc-900 mb-6">Customer Reviews</h2>
        <ReviewList
          productId={product.id}
          averageRating={product.averageRating}
          reviewCount={product.reviewCount}
        />
      </div>
    </div>
  );
}

function ProductDetailSkeleton() {
  return (
    <div>
      {/* Breadcrumb skeleton */}
      <div className="mb-8 flex items-center gap-2">
        <Skeleton className="h-4 w-12" />
        <Skeleton className="h-4 w-4" />
        <Skeleton className="h-4 w-16" />
        <Skeleton className="h-4 w-4" />
        <Skeleton className="h-4 w-32" />
      </div>

      {/* Detail layout skeleton */}
      <div className="grid grid-cols-1 gap-10 lg:grid-cols-5 lg:gap-16">
        <div className="lg:col-span-3">
          <Skeleton className="aspect-square rounded-2xl" />
        </div>
        <div className="flex flex-col justify-center lg:col-span-2">
          <Skeleton className="mb-4 h-5 w-20 rounded-full" />
          <Skeleton className="h-9 w-3/4" />
          <Skeleton className="mt-4 h-7 w-24" />
          <Skeleton className="mt-6 h-4 w-full" />
          <Skeleton className="mt-2 h-4 w-full" />
          <Skeleton className="mt-2 h-4 w-2/3" />
          <Skeleton className="mt-4 h-3 w-20" />
          <Skeleton className="mt-8 h-10 w-40 rounded-full" />
        </div>
      </div>
    </div>
  );
}
