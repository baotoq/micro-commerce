import { Suspense } from 'react';

import { HeroBanner } from '@/components/storefront/hero-banner';
import { ProductFilters } from '@/components/storefront/product-filters';
import { ProductGrid } from '@/components/storefront/product-grid';

function parseSortParam(sort: string | undefined): {
  sortBy?: string;
  sortDirection?: string;
} {
  switch (sort) {
    case 'price-asc':
      return { sortBy: 'price', sortDirection: 'asc' };
    case 'price-desc':
      return { sortBy: 'price', sortDirection: 'desc' };
    case 'name-asc':
      return { sortBy: 'name', sortDirection: 'asc' };
    case 'newest':
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

      {/* Products Section */}
      <section id="products" className="mx-auto max-w-7xl px-4 py-16 sm:px-6 lg:px-8">
        <div className="mb-8">
          <h2 className="text-2xl font-semibold tracking-tight text-zinc-900">
            All Products
          </h2>
          <p className="mt-2 text-sm text-zinc-500">
            Explore our collection of premium electronics
          </p>
        </div>

        <div className="mb-8">
          <Suspense fallback={null}>
            <ProductFilters />
          </Suspense>
        </div>

        <Suspense fallback={null}>
          <ProductGrid
            categoryId={params.category}
            search={params.search}
            sortBy={sortBy}
            sortDirection={sortDirection}
          />
        </Suspense>
      </section>
    </div>
  );
}
