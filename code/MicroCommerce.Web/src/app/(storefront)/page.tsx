import { HeroBanner } from '@/components/storefront/hero-banner';
import { ProductGrid } from '@/components/storefront/product-grid';

export default function HomePage() {
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

        <ProductGrid />
      </section>
    </div>
  );
}
