"use client";

import {
  AlertTriangle,
  CheckCircle,
  Loader2,
  Package,
  ShoppingCart,
  XCircle,
} from "lucide-react";
import Image from "next/image";
import Link from "next/link";
import { useEffect, useState } from "react";
import { ReviewList } from "@/components/reviews/review-list";
import { StarRatingDisplay } from "@/components/reviews/star-rating-display";
import { QuantityStepper } from "@/components/storefront/quantity-stepper";
import { RelatedProducts } from "@/components/storefront/related-products";
import { Badge } from "@/components/ui/badge";
import {
  Breadcrumb,
  BreadcrumbItem,
  BreadcrumbLink,
  BreadcrumbList,
  BreadcrumbPage,
  BreadcrumbSeparator,
} from "@/components/ui/breadcrumb";
import { Button } from "@/components/ui/button";
import { Separator } from "@/components/ui/separator";
import { Skeleton } from "@/components/ui/skeleton";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { WishlistToggleButton } from "@/components/wishlist/wishlist-toggle-button";
import { useAddToCart } from "@/hooks/use-cart";
import {
  getProductById,
  getStockByProductId,
  type ProductDto,
  type StockInfoDto,
} from "@/lib/api";

interface ProductDetailProps {
  productId: string;
}

function formatPrice(price: number, currency: string): string {
  return new Intl.NumberFormat("en-US", {
    style: "currency",
    currency,
  }).format(price);
}

function StockStatus({
  stockInfo,
  loading: stockLoading,
}: {
  stockInfo: StockInfoDto | null;
  loading: boolean;
}) {
  if (stockLoading) {
    return <Skeleton className="h-6 w-28 rounded-full" />;
  }

  if (!stockInfo || stockInfo.availableQuantity === 0) {
    return (
      <div className="flex items-center gap-1.5 text-sm font-medium text-error">
        <XCircle className="size-4" />
        <span>Out of Stock</span>
      </div>
    );
  }

  if (stockInfo.availableQuantity <= 10) {
    return (
      <div className="flex items-center gap-1.5 text-sm font-medium text-warning">
        <AlertTriangle className="size-4" />
        <span>Only {stockInfo.availableQuantity} left!</span>
      </div>
    );
  }

  return (
    <div className="flex items-center gap-1.5 text-sm font-medium text-success">
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
  const [quantity, setQuantity] = useState(1);
  const [selectedImageIndex, setSelectedImageIndex] = useState(0);
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
  const maxQuantity = stockInfo
    ? Math.min(stockInfo.availableQuantity, 99)
    : 99;

  // Build image list (main image + any additional images)
  const images = product?.imageUrl ? [product.imageUrl] : [];

  if (loading) {
    return <ProductDetailSkeleton />;
  }

  if (error || !product) {
    return (
      <div className="flex flex-col items-center justify-center py-24 text-center">
        <Package className="mb-4 size-12 text-muted-foreground" />
        <h2 className="text-xl font-semibold text-foreground">
          Product not found
        </h2>
        <p className="mt-2 text-sm text-muted-foreground">
          The product you are looking for does not exist or has been removed.
        </p>
        <Link href="/">
          <Button className="mt-6">Back to Home</Button>
        </Link>
      </div>
    );
  }

  return (
    <div>
      {/* Breadcrumb */}
      <Breadcrumb className="mb-8">
        <BreadcrumbList>
          <BreadcrumbItem>
            <BreadcrumbLink asChild>
              <Link href="/">Home</Link>
            </BreadcrumbLink>
          </BreadcrumbItem>
          <BreadcrumbSeparator />
          <BreadcrumbItem>
            <BreadcrumbLink asChild>
              <Link href="/#products">Products</Link>
            </BreadcrumbLink>
          </BreadcrumbItem>
          <BreadcrumbSeparator />
          <BreadcrumbItem>
            <BreadcrumbPage>{product.name}</BreadcrumbPage>
          </BreadcrumbItem>
        </BreadcrumbList>
      </Breadcrumb>

      {/* Two-column product layout */}
      <div className="grid grid-cols-1 gap-10 lg:grid-cols-2 lg:gap-16">
        {/* Left: Image gallery */}
        <div className="space-y-4">
          {/* Main image */}
          <div className="relative aspect-square overflow-hidden rounded-xl border border-border bg-muted">
            {images.length > 0 ? (
              <Image
                src={images[selectedImageIndex] || images[0]}
                alt={product.name}
                fill
                sizes="(max-width: 1024px) 100vw, 50vw"
                className="object-cover"
                priority
              />
            ) : (
              <div className="flex h-full items-center justify-center">
                <Package className="size-24 text-muted-foreground" />
              </div>
            )}
          </div>

          {/* Thumbnail row */}
          {images.length > 1 && (
            <div className="flex gap-3">
              {images.map((img, index) => (
                <button
                  key={img}
                  type="button"
                  onClick={() => setSelectedImageIndex(index)}
                  className={`relative size-20 overflow-hidden rounded-lg border-2 transition-all ${
                    index === selectedImageIndex
                      ? "border-primary ring-2 ring-primary/20"
                      : "border-border hover:border-muted-foreground"
                  }`}
                  aria-label={`View image ${index + 1}`}
                >
                  <Image
                    src={img}
                    alt={`${product.name} thumbnail ${index + 1}`}
                    fill
                    sizes="80px"
                    className="object-cover"
                  />
                </button>
              ))}
            </div>
          )}
        </div>

        {/* Right: Product info */}
        <div className="flex flex-col">
          <Link href="/#products">
            <Badge
              variant="secondary"
              className="mb-3 text-[11px] font-medium uppercase tracking-wider"
            >
              {product.categoryName}
            </Badge>
          </Link>

          <div className="flex items-start justify-between gap-4">
            <h1 className="text-2xl font-semibold tracking-tight text-foreground lg:text-3xl">
              {product.name}
            </h1>
            <div className="flex-shrink-0">
              <WishlistToggleButton productId={product.id} />
            </div>
          </div>

          {/* Rating summary */}
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
              <span className="text-sm text-muted-foreground">
                No reviews yet
              </span>
            </div>
          )}

          {/* Price */}
          <p className="mt-4 text-2xl font-semibold text-foreground">
            {formatPrice(product.price, product.priceCurrency)}
          </p>

          {/* Stock status */}
          <div className="mt-3">
            <StockStatus stockInfo={stockInfo} loading={stockLoading} />
          </div>

          {/* Description snippet */}
          {product.description && (
            <p className="mt-6 line-clamp-3 leading-relaxed text-muted-foreground">
              {product.description}
            </p>
          )}

          {/* SKU */}
          {product.sku && (
            <p className="mt-3 text-xs text-muted-foreground">
              SKU: {product.sku}
            </p>
          )}

          {/* Quantity + Add to Cart */}
          <div className="mt-8 space-y-4">
            {isInStock ? (
              <>
                <div className="flex items-center gap-4">
                  <span className="text-sm font-medium text-foreground">
                    Quantity
                  </span>
                  <QuantityStepper
                    value={quantity}
                    onIncrement={() =>
                      setQuantity((q) => Math.min(q + 1, maxQuantity))
                    }
                    onDecrement={() => setQuantity((q) => Math.max(q - 1, 1))}
                    min={1}
                    max={maxQuantity}
                  />
                </div>
                <Button
                  size="lg"
                  className="w-full sm:w-auto"
                  disabled={addToCart.isPending}
                  onClick={() =>
                    addToCart.mutate({
                      productId: product.id,
                      productName: product.name,
                      unitPrice: product.price,
                      imageUrl: product.imageUrl,
                      quantity,
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
              </>
            ) : (
              <div className="rounded-lg border border-error/20 bg-error-bg px-6 py-3 text-center">
                <p className="text-sm font-medium text-error-foreground">
                  This product is currently out of stock
                </p>
              </div>
            )}
          </div>
        </div>
      </div>

      {/* Tabbed content: Description / Reviews & Ratings */}
      <div className="mt-16">
        <Tabs defaultValue="description">
          <TabsList
            variant="line"
            className="w-full justify-start border-b border-border"
          >
            <TabsTrigger value="description">Description</TabsTrigger>
            <TabsTrigger value="reviews">
              Reviews & Ratings
              {product.reviewCount > 0 && (
                <span className="ml-1.5 text-xs text-muted-foreground">
                  ({product.reviewCount})
                </span>
              )}
            </TabsTrigger>
          </TabsList>

          <TabsContent value="description" className="pt-6">
            {product.description ? (
              <p className="max-w-prose whitespace-pre-line leading-relaxed text-muted-foreground">
                {product.description}
              </p>
            ) : (
              <p className="text-sm text-muted-foreground">
                No description available.
              </p>
            )}
          </TabsContent>

          <TabsContent value="reviews" className="pt-6">
            <ReviewList
              productId={product.id}
              averageRating={product.averageRating}
              reviewCount={product.reviewCount}
            />
          </TabsContent>
        </Tabs>
      </div>

      {/* Related Products */}
      <Separator className="my-16" />

      <RelatedProducts
        categoryId={product.categoryId}
        categoryName={product.categoryName}
        currentProductId={product.id}
      />
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

      {/* Two-column layout skeleton */}
      <div className="grid grid-cols-1 gap-10 lg:grid-cols-2 lg:gap-16">
        <div className="space-y-4">
          <Skeleton className="aspect-square rounded-xl" />
          <div className="flex gap-3">
            <Skeleton className="size-20 rounded-lg" />
            <Skeleton className="size-20 rounded-lg" />
            <Skeleton className="size-20 rounded-lg" />
          </div>
        </div>
        <div className="flex flex-col">
          <Skeleton className="mb-3 h-5 w-20 rounded-full" />
          <Skeleton className="h-9 w-3/4" />
          <Skeleton className="mt-3 h-5 w-32" />
          <Skeleton className="mt-4 h-7 w-24" />
          <Skeleton className="mt-3 h-5 w-20" />
          <Skeleton className="mt-6 h-4 w-full" />
          <Skeleton className="mt-2 h-4 w-full" />
          <Skeleton className="mt-2 h-4 w-2/3" />
          <Skeleton className="mt-8 h-9 w-40" />
          <Skeleton className="mt-4 h-10 w-full sm:w-40" />
        </div>
      </div>
    </div>
  );
}
