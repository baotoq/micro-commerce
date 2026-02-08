"use client";

import { useEffect, useState } from "react";

import { ProductCard, ProductCardSkeleton } from "@/components/storefront/product-card";
import { getProducts, type ProductDto } from "@/lib/api";

interface RelatedProductsProps {
  categoryId: string;
  categoryName: string;
  currentProductId: string;
}

export function RelatedProducts({
  categoryId,
  categoryName,
  currentProductId,
}: RelatedProductsProps) {
  const [products, setProducts] = useState<ProductDto[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    let cancelled = false;

    async function fetchRelated() {
      try {
        setLoading(true);
        const data = await getProducts({
          categoryId,
          status: "Published",
          pageSize: 5,
        });
        if (!cancelled) {
          const filtered = data.items
            .filter((p) => p.id !== currentProductId)
            .slice(0, 4);
          setProducts(filtered);
        }
      } catch {
        // Silently fail - related products are non-critical
        if (!cancelled) {
          setProducts([]);
        }
      } finally {
        if (!cancelled) {
          setLoading(false);
        }
      }
    }

    fetchRelated();
    return () => {
      cancelled = true;
    };
  }, [categoryId, currentProductId]);

  if (loading) {
    return (
      <section>
        <h2 className="mb-6 text-xl font-semibold tracking-tight text-zinc-900">
          More from {categoryName}
        </h2>
        <div className="grid grid-cols-2 gap-4 lg:grid-cols-4 lg:gap-6">
          {Array.from({ length: 4 }).map((_, i) => (
            <ProductCardSkeleton key={i} />
          ))}
        </div>
      </section>
    );
  }

  if (products.length === 0) {
    return null;
  }

  return (
    <section>
      <h2 className="mb-6 text-xl font-semibold tracking-tight text-zinc-900">
        More from {categoryName}
      </h2>
      <div className="grid grid-cols-2 gap-4 lg:grid-cols-4 lg:gap-6">
        {products.map((product) => (
          <ProductCard key={product.id} product={product} />
        ))}
      </div>
    </section>
  );
}
