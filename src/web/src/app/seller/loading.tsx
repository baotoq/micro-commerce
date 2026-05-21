// Route-level loading fallback for the seller area. Renders while async Server
// Components in `/seller/*` suspend (initial load + Cache Components revalidation).
// We don't have a shadcn Skeleton primitive in the project, so this uses Tailwind's
// `animate-pulse` directly — keeps the layout shift minimal while data resolves.

export default function SellerLoading() {
  return (
    <section
      aria-busy="true"
      aria-live="polite"
      className="flex flex-col gap-4 px-7 py-6"
    >
      <span className="sr-only">Loading…</span>
      <div className="h-7 w-48 animate-pulse rounded-md bg-muted" />
      <div className="h-4 w-72 animate-pulse rounded-md bg-muted" />
      <div className="mt-4 grid gap-3">
        {Array.from({ length: 6 }).map((_, i) => (
          <div
            // biome-ignore lint/suspicious/noArrayIndexKey: static skeleton, no reorder
            key={i}
            className="h-12 animate-pulse rounded-md bg-muted"
          />
        ))}
      </div>
    </section>
  );
}
