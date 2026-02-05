import Link from 'next/link';

export function HeroBanner() {
  return (
    <section className="relative overflow-hidden bg-white">
      {/* Subtle gradient background */}
      <div className="absolute inset-0 bg-gradient-to-b from-zinc-50 to-white" />

      <div className="relative mx-auto max-w-7xl px-4 py-24 sm:px-6 sm:py-32 lg:px-8 lg:py-40">
        <div className="mx-auto max-w-2xl text-center">
          <p className="text-sm font-medium uppercase tracking-widest text-zinc-400">
            New arrivals
          </p>
          <h1 className="mt-4 text-4xl font-semibold tracking-tight text-zinc-900 sm:text-5xl lg:text-6xl">
            The Latest in Tech
          </h1>
          <p className="mt-6 text-lg leading-relaxed text-zinc-500">
            Discover our curated collection of premium electronics.
            From laptops to accessories, find everything you need.
          </p>
          <div className="mt-10">
            <Link
              href="#products"
              className="inline-flex items-center rounded-full bg-zinc-900 px-8 py-3 text-sm font-medium text-white transition-colors hover:bg-zinc-700"
            >
              Browse Products
            </Link>
          </div>
        </div>
      </div>
    </section>
  );
}
