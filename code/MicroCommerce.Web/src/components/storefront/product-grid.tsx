"use client";

import { useCallback, useEffect, useRef, useState } from "react";

import { useIntersectionObserver } from "@/hooks/use-intersection-observer";
import type { ProductDto } from "@/lib/api";
import { getStorefrontProducts } from "@/lib/api";

import { ProductCard, ProductCardSkeleton } from "./product-card";

interface ProductGridProps {
  categoryId?: string;
  search?: string;
  sortBy?: string;
  sortDirection?: string;
}

const PAGE_SIZE = 12;

export function ProductGrid({
  categoryId,
  search,
  sortBy,
  sortDirection,
}: ProductGridProps) {
  const [products, setProducts] = useState<ProductDto[]>([]);
  const [page, setPage] = useState(1);
  const [loading, setLoading] = useState(true);
  const [loadingMore, setLoadingMore] = useState(false);
  const [hasMore, setHasMore] = useState(true);
  const [initialLoad, setInitialLoad] = useState(true);

  const { ref: sentinelRef, isIntersecting } = useIntersectionObserver({
    threshold: 0.1,
    rootMargin: "200px",
  });

  // Track filter changes to reset
  const filtersRef = useRef({ categoryId, search, sortBy, sortDirection });

  // Reset when filters change
  useEffect(() => {
    const prev = filtersRef.current;
    const filtersChanged =
      prev.categoryId !== categoryId ||
      prev.search !== search ||
      prev.sortBy !== sortBy ||
      prev.sortDirection !== sortDirection;

    if (filtersChanged) {
      filtersRef.current = { categoryId, search, sortBy, sortDirection };
      setProducts([]);
      setPage(1);
      setHasMore(true);
      setInitialLoad(true);
      setLoading(true);
    }
  }, [categoryId, search, sortBy, sortDirection]);

  // Fetch products
  const fetchProducts = useCallback(async (
    pageNum: number,
    isInitial: boolean,
    filters: { categoryId?: string; search?: string; sortBy?: string; sortDirection?: string }
  ) => {
    try {
      if (isInitial) {
        setLoading(true);
      } else {
        setLoadingMore(true);
      }

      const data = await getStorefrontProducts({
        page: pageNum,
        pageSize: PAGE_SIZE,
        categoryId: filters.categoryId,
        search: filters.search,
        sortBy: filters.sortBy,
        sortDirection: filters.sortDirection,
      });

      setProducts((prev) =>
        isInitial ? data.items : [...prev, ...data.items]
      );
      setHasMore(
        isInitial
          ? data.totalCount > data.items.length
          : data.totalCount > products.length + data.items.length
      );
    } catch {
      // Silently handle error - products stay as-is
    } finally {
      setLoading(false);
      setLoadingMore(false);
      setInitialLoad(false);
    }
  }, [products.length]);

  // Initial load and filter reset
  useEffect(() => {
    if (initialLoad) {
      fetchProducts(1, true, { categoryId, search, sortBy, sortDirection });
    }
  }, [initialLoad, categoryId, search, sortBy, sortDirection, fetchProducts]);

  // Infinite scroll - load next page
  useEffect(() => {
    if (isIntersecting && hasMore && !loading && !loadingMore && !initialLoad) {
      const nextPage = page + 1;
      setPage(nextPage);
      fetchProducts(nextPage, false, { categoryId, search, sortBy, sortDirection });
    }
  }, [isIntersecting, hasMore, loading, loadingMore, initialLoad, page, categoryId, search, sortBy, sortDirection, fetchProducts]);

  // Initial loading state
  if (loading && products.length === 0) {
    return (
      <div className="grid grid-cols-1 gap-6 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4">
        {Array.from({ length: 8 }).map((_, i) => (
          <ProductCardSkeleton key={i} />
        ))}
      </div>
    );
  }

  // Empty state
  if (!loading && products.length === 0) {
    return (
      <div className="flex flex-col items-center justify-center py-20 text-center">
        <p className="text-lg font-medium text-zinc-900">No products found</p>
        <p className="mt-2 text-sm text-zinc-500">
          Try adjusting your search or filters.
        </p>
      </div>
    );
  }

  return (
    <div>
      <div className="grid grid-cols-1 gap-6 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4">
        {products.map((product) => (
          <ProductCard key={product.id} product={product} />
        ))}

        {/* Loading more skeletons */}
        {loadingMore &&
          Array.from({ length: 4 }).map((_, i) => (
            <ProductCardSkeleton key={`skeleton-${i}`} />
          ))}
      </div>

      {/* Sentinel for infinite scroll */}
      {hasMore && (
        <div ref={sentinelRef} className="h-1" aria-hidden="true" />
      )}
    </div>
  );
}
