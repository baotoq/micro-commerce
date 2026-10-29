export default function Loading() {
  return (
    <main aria-busy="true" aria-label="Loading Marketplace" className="mx-auto w-full max-w-7xl px-4 py-8 sm:px-6">
      <div className="mb-6 h-8 w-48 animate-pulse rounded bg-muted motion-reduce:animate-none" />
      <div className="mb-6 h-10 w-full max-w-xl animate-pulse rounded bg-muted motion-reduce:animate-none" />
      <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-4">
        {Array.from({ length: 8 }, (_, index) => (
          <div
            key={index}
            className="grid gap-3 rounded-lg border p-4"
            aria-hidden="true"
          >
            <div className="aspect-square animate-pulse rounded bg-muted motion-reduce:animate-none" />
            <div className="h-5 w-3/4 animate-pulse rounded bg-muted motion-reduce:animate-none" />
            <div className="h-4 w-1/2 animate-pulse rounded bg-muted motion-reduce:animate-none" />
          </div>
        ))}
      </div>
      <span className="sr-only">Loading Marketplace</span>
    </main>
  );
}
