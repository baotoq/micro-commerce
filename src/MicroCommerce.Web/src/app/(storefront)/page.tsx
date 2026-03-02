import { Suspense } from "react";

import { CategoryPillsRow } from "@/components/storefront/category-pills-row";
import { HeroBanner } from "@/components/storefront/hero-banner";
import { ProductFilters } from "@/components/storefront/product-filters";
import { ProductGrid } from "@/components/storefront/product-grid";

function parseSortParam(sort: string | undefined): {
  sortBy?: string;
  sortDirection?: string;
} {
  switch (sort) {
    case "price-asc":
      return { sortBy: "price", sortDirection: "asc" };
    case "price-desc":
      return { sortBy: "price", sortDirection: "desc" };
    case "name-asc":
      return { sortBy: "name", sortDirection: "asc" };
    default:
      return {};
  }
}

interface HomePageProps {
  searchParams: Promise<{
    search?: string;
    category?: string;
    sort?: string;
  }>;
}

export default async function HomePage({ searchParams }: HomePageProps) {
  const params = await searchParams;
  const { sortBy, sortDirection } = parseSortParam(params.sort);

  return (
    <div>
      <HeroBanner />

      {/* Category Pills Section */}
      <section className="mx-auto max-w-7xl px-4 py-12 sm:px-6 lg:px-20">
        <h2 className="mb-6 text-2xl font-bold tracking-tight text-foreground">
          Shop by Category
        </h2>
        <Suspense fallback={null}>
          <CategoryPillsRow activeCategory={params.category} />
        </Suspense>
      </section>

      {/* Featured Products Section */}
      <section
        id="products"
        className="mx-auto max-w-7xl px-4 pb-12 sm:px-6 lg:px-20"
      >
        <div className="mb-6 flex items-center justify-between">
          <div>
            <h2 className="text-2xl font-bold tracking-tight text-foreground">
              Featured Products
            </h2>
            <p className="mt-1 text-sm text-muted-foreground">
              Explore our collection of premium electronics
            </p>
          </div>
        </div>

        {/* Two-column layout: sidebar filters + product grid */}
        <div className="flex gap-8">
          <Suspense fallback={null}>
            <ProductFilters />
          </Suspense>

          <div className="min-w-0 flex-1">
            <Suspense fallback={null}>
              <ProductGrid
                categoryId={params.category}
                search={params.search}
                sortBy={sortBy}
                sortDirection={sortDirection}
              />
            </Suspense>
          </div>
        </div>
      </section>
    </div>
  );
}
