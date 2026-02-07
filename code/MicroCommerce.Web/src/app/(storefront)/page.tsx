import { HeroBanner } from '@/components/storefront/hero-banner';
import { Skeleton } from '@/components/ui/skeleton';

export default function HomePage() {
  return (
    <div>
      <HeroBanner />

      {/* Featured Products Section */}
      <section id="products" className="mx-auto max-w-7xl px-4 py-16 sm:px-6 lg:px-8">
        <div className="mb-8">
          <h2 className="text-2xl font-semibold tracking-tight text-zinc-900">
            Featured Products
          </h2>
          <p className="mt-2 text-sm text-zinc-500">
            Explore our collection of premium electronics
          </p>
        </div>

        {/* Product grid placeholder -- will be replaced in Plan 03 */}
        <div className="grid grid-cols-1 gap-6 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4">
          {Array.from({ length: 8 }).map((_, i) => (
            <div key={i} className="space-y-3">
              <Skeleton className="aspect-square rounded-xl" />
              <Skeleton className="h-4 w-16" />
              <Skeleton className="h-4 w-3/4" />
              <Skeleton className="h-4 w-1/4" />
            </div>
          ))}
        </div>
      </section>
    </div>
  );
}
