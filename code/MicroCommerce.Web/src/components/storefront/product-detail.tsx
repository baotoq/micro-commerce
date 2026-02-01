"use client";

import { useEffect, useState } from "react";
import Image from "next/image";
import Link from "next/link";
import { ShoppingCart, Package } from "lucide-react";
import { toast } from "sonner";

import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Separator } from "@/components/ui/separator";
import { Skeleton } from "@/components/ui/skeleton";
import { RelatedProducts } from "@/components/storefront/related-products";
import { getProductById, type ProductDto } from "@/lib/api";

interface ProductDetailProps {
  productId: string;
}

function formatPrice(price: number, currency: string): string {
  return new Intl.NumberFormat("en-US", {
    style: "currency",
    currency,
  }).format(price);
}

export function ProductDetail({ productId }: ProductDetailProps) {
  const [product, setProduct] = useState<ProductDto | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(false);

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

    fetchProduct();
    return () => {
      cancelled = true;
    };
  }, [productId]);

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

          <h1 className="text-3xl font-bold tracking-tight text-zinc-900">
            {product.name}
          </h1>

          <p className="mt-4 text-2xl font-semibold text-zinc-900">
            {formatPrice(product.price, product.priceCurrency)}
          </p>

          {product.description && (
            <p className="mt-6 whitespace-pre-line leading-relaxed text-zinc-500">
              {product.description}
            </p>
          )}

          {product.sku && (
            <p className="mt-4 text-xs text-zinc-400">SKU: {product.sku}</p>
          )}

          <div className="mt-8">
            <Button
              size="lg"
              className="w-full rounded-full sm:w-auto"
              onClick={() => toast("Cart coming soon!")}
            >
              <ShoppingCart className="mr-2 size-4" />
              Add to Cart
            </Button>
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
