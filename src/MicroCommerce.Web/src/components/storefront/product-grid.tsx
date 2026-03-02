"use client";

import { PackageSearch } from "lucide-react";
import { useRouter, useSearchParams } from "next/navigation";
import { useCallback, useEffect, useRef, useState } from "react";

import {
  Pagination,
  PaginationContent,
  PaginationEllipsis,
  PaginationItem,
  PaginationLink,
  PaginationNext,
  PaginationPrevious,
} from "@/components/ui/pagination";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import type { ProductDto, StockInfoDto } from "@/lib/api";
import { getStockLevels, getStorefrontProducts } from "@/lib/api";

import { ProductCard, ProductCardSkeleton } from "./product-card";

interface ProductGridProps {
  categoryId?: string;
  search?: string;
  sortBy?: string;
  sortDirection?: string;
}

const PAGE_SIZE = 12;

const SORT_OPTIONS = [
  { value: "featured", label: "Featured" },
  { value: "price-asc", label: "Price: Low to High" },
  { value: "price-desc", label: "Price: High to Low" },
  { value: "name-asc", label: "Name: A to Z" },
  { value: "newest", label: "Newest" },
] as const;

interface PageItem {
  type: "page" | "ellipsis";
  value: number;
  key: string;
}

function generatePageNumbers(
  currentPage: number,
  totalPages: number,
): PageItem[] {
  if (totalPages <= 5) {
    return Array.from({ length: totalPages }, (_, i) => ({
      type: "page" as const,
      value: i + 1,
      key: `page-${i + 1}`,
    }));
  }

  const pages: PageItem[] = [{ type: "page", value: 1, key: "page-1" }];

  if (currentPage > 3) {
    pages.push({ type: "ellipsis", value: 0, key: "ellipsis-start" });
  }

  const start = Math.max(2, currentPage - 1);
  const end = Math.min(totalPages - 1, currentPage + 1);

  for (let i = start; i <= end; i++) {
    pages.push({ type: "page", value: i, key: `page-${i}` });
  }

  if (currentPage < totalPages - 2) {
    pages.push({ type: "ellipsis", value: 0, key: "ellipsis-end" });
  }

  pages.push({
    type: "page",
    value: totalPages,
    key: `page-${totalPages}`,
  });

  return pages;
}

export function ProductGrid({
  categoryId,
  search,
  sortBy,
  sortDirection,
}: ProductGridProps) {
  const router = useRouter();
  const searchParams = useSearchParams();
  const [products, setProducts] = useState<ProductDto[]>([]);
  const [stockMap, setStockMap] = useState<Map<string, StockInfoDto>>(
    new Map(),
  );
  const [totalCount, setTotalCount] = useState(0);
  const [loading, setLoading] = useState(true);

  const currentPage = Number(searchParams.get("page") ?? "1");
  const currentSort = searchParams.get("sort") ?? "featured";
  const totalPages = Math.max(1, Math.ceil(totalCount / PAGE_SIZE));

  // Track filter changes to reset page
  const filtersRef = useRef({ categoryId, search, sortBy, sortDirection });

  // Reset page when filters change
  useEffect(() => {
    const prev = filtersRef.current;
    const filtersChanged =
      prev.categoryId !== categoryId ||
      prev.search !== search ||
      prev.sortBy !== sortBy ||
      prev.sortDirection !== sortDirection;

    if (filtersChanged) {
      filtersRef.current = { categoryId, search, sortBy, sortDirection };
    }
  }, [categoryId, search, sortBy, sortDirection]);

  // Fetch stock levels for a batch of products and merge into stockMap
  const fetchStockForProducts = useCallback(
    async (productItems: ProductDto[]) => {
      if (productItems.length === 0) return;
      try {
        const productIds = productItems.map((p) => p.id);
        const stockLevels = await getStockLevels(productIds);
        setStockMap((prev) => {
          const next = new Map(prev);
          for (const stock of stockLevels) {
            next.set(stock.productId, stock);
          }
          return next;
        });
      } catch {
        // Stock fetch failure is non-critical - cards render without stock badges
      }
    },
    [],
  );

  // Fetch products for current page
  useEffect(() => {
    let cancelled = false;

    async function load() {
      setLoading(true);
      try {
        const data = await getStorefrontProducts({
          page: currentPage,
          pageSize: PAGE_SIZE,
          categoryId,
          search,
          sortBy,
          sortDirection,
        });

        if (!cancelled) {
          setProducts(data.items);
          setTotalCount(data.totalCount);
          fetchStockForProducts(data.items);
        }
      } catch {
        // Silently handle error - products stay as-is
      } finally {
        if (!cancelled) {
          setLoading(false);
        }
      }
    }

    load();
    return () => {
      cancelled = true;
    };
  }, [
    currentPage,
    categoryId,
    search,
    sortBy,
    sortDirection,
    fetchStockForProducts,
  ]);

  const updateSearchParam = (key: string, value: string | null) => {
    const params = new URLSearchParams(searchParams.toString());
    if (value) {
      params.set(key, value);
    } else {
      params.delete(key);
    }
    router.replace(`?${params.toString()}`, { scroll: false });
  };

  const handleSortChange = (value: string) => {
    const params = new URLSearchParams(searchParams.toString());
    if (value === "featured") {
      params.delete("sort");
    } else {
      params.set("sort", value);
    }
    params.delete("page");
    router.replace(`?${params.toString()}`, { scroll: false });
  };

  const handlePageChange = (page: number) => {
    if (page === 1) {
      updateSearchParam("page", null);
    } else {
      updateSearchParam("page", String(page));
    }
    window.scrollTo({ top: 0, behavior: "smooth" });
  };

  const startItem = (currentPage - 1) * PAGE_SIZE + 1;
  const endItem = Math.min(currentPage * PAGE_SIZE, totalCount);

  // Initial loading state
  if (loading && products.length === 0) {
    return (
      <div className="flex flex-col gap-6">
        <div className="flex items-center justify-between">
          <div className="h-5 w-48 animate-pulse rounded bg-muted" />
          <div className="h-9 w-40 animate-pulse rounded-md bg-muted" />
        </div>
        <div className="grid grid-cols-1 gap-5 sm:grid-cols-2 lg:grid-cols-3">
          {Array.from({ length: 6 }, (_, i) => `skeleton-${i}`).map((key) => (
            <ProductCardSkeleton key={key} />
          ))}
        </div>
      </div>
    );
  }

  // Empty state
  if (!loading && products.length === 0) {
    return (
      <div className="flex flex-col items-center justify-center py-20 text-center">
        <PackageSearch className="mb-4 size-12 text-muted-foreground" />
        <p className="text-lg font-medium text-foreground">No products found</p>
        <p className="mt-2 text-sm text-muted-foreground">
          Try adjusting your search or filters.
        </p>
      </div>
    );
  }

  return (
    <div className="flex flex-col gap-6">
      {/* Results bar with sort */}
      <div className="flex items-center justify-between">
        <p className="text-sm text-muted-foreground">
          Showing {startItem}&ndash;{endItem} of {totalCount} products
        </p>
        <Select value={currentSort} onValueChange={handleSortChange}>
          <SelectTrigger className="w-auto gap-2">
            <span className="text-sm text-foreground">Sort by:</span>
            <SelectValue />
          </SelectTrigger>
          <SelectContent position="popper" align="end">
            {SORT_OPTIONS.map((option) => (
              <SelectItem key={option.value} value={option.value}>
                {option.label}
              </SelectItem>
            ))}
          </SelectContent>
        </Select>
      </div>

      {/* Product grid */}
      <div className="grid grid-cols-1 gap-5 sm:grid-cols-2 lg:grid-cols-3">
        {products.map((product) => (
          <ProductCard
            key={product.id}
            product={product}
            stockInfo={stockMap.get(product.id)}
          />
        ))}
      </div>

      {/* Pagination */}
      {totalPages > 1 && (
        <Pagination className="mt-4">
          <PaginationContent>
            <PaginationItem>
              <PaginationPrevious
                href="#"
                onClick={(e) => {
                  e.preventDefault();
                  if (currentPage > 1) handlePageChange(currentPage - 1);
                }}
                aria-disabled={currentPage <= 1}
                className={
                  currentPage <= 1
                    ? "pointer-events-none opacity-50"
                    : "cursor-pointer"
                }
              />
            </PaginationItem>

            {generatePageNumbers(currentPage, totalPages).map((item) =>
              item.type === "ellipsis" ? (
                <PaginationItem key={item.key}>
                  <PaginationEllipsis />
                </PaginationItem>
              ) : (
                <PaginationItem key={item.key}>
                  <PaginationLink
                    href="#"
                    isActive={item.value === currentPage}
                    onClick={(e) => {
                      e.preventDefault();
                      handlePageChange(item.value);
                    }}
                    className="cursor-pointer"
                  >
                    {item.value}
                  </PaginationLink>
                </PaginationItem>
              ),
            )}

            <PaginationItem>
              <PaginationNext
                href="#"
                onClick={(e) => {
                  e.preventDefault();
                  if (currentPage < totalPages)
                    handlePageChange(currentPage + 1);
                }}
                aria-disabled={currentPage >= totalPages}
                className={
                  currentPage >= totalPages
                    ? "pointer-events-none opacity-50"
                    : "cursor-pointer"
                }
              />
            </PaginationItem>
          </PaginationContent>
        </Pagination>
      )}
    </div>
  );
}
