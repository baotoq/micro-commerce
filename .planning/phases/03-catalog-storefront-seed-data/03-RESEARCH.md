# Phase 3: Catalog Storefront & Seed Data - Research

**Researched:** 2026-02-07
**Domain:** Next.js 16 storefront UI, infinite scroll, search/filter, EF Core seed data
**Confidence:** HIGH

## Summary

This phase builds the customer-facing product browsing experience on top of the existing Phase 2 catalog API. The backend already has product listing with pagination, search, and category filtering via `GetProductsQuery`. The frontend needs a storefront layout (hero, grid, detail page), infinite scroll, debounced search, category filtering, and sort options.

The project already uses Next.js 16.0.3, React 19.2.0, shadcn/ui (new-york style), Tailwind CSS 4, and raw `fetch` calls in `src/lib/api.ts`. The admin pages use a `use client` pattern with `useState`/`useEffect` for data fetching. For the storefront, we should keep the same pattern (no TanStack Query) to maintain consistency with the existing codebase, and implement infinite scroll with the native Intersection Observer API.

For seed data, the backend uses .NET 10 with EF Core 10 (Npgsql 10.0.0). The modern approach is `UseSeeding`/`UseAsyncSeeding` which allows conditional seeding logic (only seed if tables are empty) and works with complex domain objects via factory methods.

**Primary recommendation:** Build the storefront using the existing `fetch`-based API layer with client components, Intersection Observer for infinite scroll, URL search params for shareable filter/search state, and `UseAsyncSeeding` for database seed data.

## Standard Stack

### Core (Already Installed)
| Library | Version | Purpose | Why Standard |
|---------|---------|---------|--------------|
| Next.js | 16.0.3 | Framework | Already in project |
| React | 19.2.0 | UI library | Already in project |
| shadcn/ui | new-york | Component library | Already in project, neutral base color |
| Tailwind CSS | 4.1.17 | Styling | Already in project |
| lucide-react | 0.563.0 | Icons | Already in project |
| sonner | 2.0.7 | Toast notifications | Already in project |
| next/image | built-in | Image optimization | Built into Next.js, already configured for Azurite and Azure Blob |

### New shadcn/ui Components to Add
| Component | Purpose | When to Use |
|-----------|---------|-------------|
| `card` | Product cards in grid | Product listing page |
| `separator` | Visual dividers | Section separation |
| `scroll-area` | Scrollable regions | Filter sidebars if needed |
| `aspect-ratio` | Consistent image sizing | Product card images |
| `tooltip` | Hover hints | Quick actions |

### Alternatives Considered
| Instead of | Could Use | Tradeoff |
|------------|-----------|----------|
| Raw fetch + useState | TanStack Query | TQ adds caching/dedup but project already uses raw fetch everywhere; consistency wins |
| nuqs (URL state) | Manual URLSearchParams | nuqs is cleaner but adds a dependency for a simple use case; manual is fine for 3-4 params |
| react-intersection-observer | Native IntersectionObserver | Library adds convenience but only ~15 lines to hand-write; keep dependency count low |

**Installation (shadcn/ui components only):**
```bash
cd code/MicroCommerce.Web
npx shadcn@latest add card separator aspect-ratio
```

## Architecture Patterns

### Recommended Project Structure
```
src/
├── app/
│   ├── (storefront)/              # Route group for storefront layout
│   │   ├── layout.tsx             # Storefront layout (header, nav, footer)
│   │   ├── page.tsx               # Homepage (hero + featured + grid)
│   │   └── products/
│   │       └── [id]/
│   │           └── page.tsx       # Product detail page
│   ├── admin/                     # Existing admin pages (unchanged)
│   ├── layout.tsx                 # Root layout (unchanged)
│   └── globals.css                # Existing styles
├── components/
│   ├── ui/                        # shadcn/ui components (existing)
│   ├── admin/                     # Existing admin components
│   └── storefront/                # NEW: storefront-specific components
│       ├── header.tsx             # Site header with search, nav, cart icon
│       ├── footer.tsx             # Site footer
│       ├── hero-banner.tsx        # Hero section
│       ├── product-card.tsx       # Individual product card
│       ├── product-grid.tsx       # Grid container with infinite scroll
│       ├── product-filters.tsx    # Category filter + sort controls
│       ├── search-bar.tsx         # Debounced search input
│       └── product-detail.tsx     # Product detail content
├── hooks/
│   └── use-intersection-observer.ts  # Reusable intersection observer hook
└── lib/
    ├── api.ts                     # Existing API functions (extend for storefront)
    ├── config.ts                  # Existing config
    └── utils.ts                   # Existing utilities
```

### Pattern 1: Route Group for Storefront Layout
**What:** Use `(storefront)` route group to give the customer-facing pages their own layout (header/footer) without affecting admin pages.
**When to use:** Always for storefront pages.
**Example:**
```typescript
// src/app/(storefront)/layout.tsx
import { Header } from '@/components/storefront/header';
import { Footer } from '@/components/storefront/footer';

export default function StorefrontLayout({ children }: { children: React.ReactNode }) {
  return (
    <div className="flex min-h-screen flex-col">
      <Header />
      <main className="flex-1">{children}</main>
      <Footer />
    </div>
  );
}
```

### Pattern 2: Infinite Scroll with Intersection Observer
**What:** Client component that loads successive pages when a sentinel element becomes visible.
**When to use:** Product grid on homepage and browse-all view.
**Example:**
```typescript
// src/hooks/use-intersection-observer.ts
'use client';
import { useEffect, useRef, useState } from 'react';

export function useIntersectionObserver(options?: IntersectionObserverInit) {
  const ref = useRef<HTMLDivElement>(null);
  const [isIntersecting, setIsIntersecting] = useState(false);

  useEffect(() => {
    const element = ref.current;
    if (!element) return;

    const observer = new IntersectionObserver(
      ([entry]) => setIsIntersecting(entry.isIntersecting),
      { threshold: 0, rootMargin: '100px', ...options }
    );

    observer.observe(element);
    return () => observer.disconnect();
  }, [options]);

  return { ref, isIntersecting };
}
```

### Pattern 3: URL-Synced Filter State
**What:** Store search, category, and sort in URL search params so links are shareable and browser back works.
**When to use:** All filter/search/sort controls.
**Example:**
```typescript
// Use Next.js useSearchParams + useRouter for URL state
'use client';
import { useSearchParams, useRouter, usePathname } from 'next/navigation';

function updateSearchParams(params: Record<string, string | undefined>) {
  const searchParams = new URLSearchParams(window.location.search);
  for (const [key, value] of Object.entries(params)) {
    if (value) searchParams.set(key, value);
    else searchParams.delete(key);
  }
  return searchParams.toString();
}
```

### Pattern 4: Debounced Search
**What:** Delay API calls while user is typing, fire after 300ms pause.
**When to use:** Search input.
**Example:**
```typescript
// Debounce pattern (same as admin pages already use)
useEffect(() => {
  const debounce = setTimeout(() => {
    // Update URL params, which triggers data fetch
    router.replace(`${pathname}?${updateSearchParams({ search: query || undefined })}`);
  }, 300);
  return () => clearTimeout(debounce);
}, [query]);
```

### Pattern 5: Backend Sort Parameter
**What:** Add `SortBy` and `SortDirection` to `GetProductsQuery` for sort support.
**When to use:** Product listing endpoint.
**Example:**
```csharp
// Extend GetProductsQuery
public sealed record GetProductsQuery(
    int Page = 1,
    int PageSize = 20,
    Guid? CategoryId = null,
    string? Status = null,
    string? Search = null,
    string? SortBy = null,        // NEW: "price", "name", "newest"
    string? SortDirection = null   // NEW: "asc", "desc"
) : IRequest<ProductListDto>;
```

### Anti-Patterns to Avoid
- **Fetching in Server Components then passing to client for infinite scroll:** The initial page can be a server component, but infinite scroll needs client-side state. Use server component for SSR of first page, then client component takes over for subsequent pages.
- **Storing all filter state in React state only:** URL params must be the source of truth for shareability and browser back support.
- **Using `useEffect` chains for data fetching with multiple dependencies:** Consolidate fetch logic into a single effect that reads from URL params.
- **Loading all 50 products at once:** Even though 50 is small, implement proper pagination (12-16 per page) to establish the pattern for when data grows.

## Don't Hand-Roll

| Problem | Don't Build | Use Instead | Why |
|---------|-------------|-------------|-----|
| Image optimization | Custom image resizing/lazy loading | `next/image` component | Handles responsive sizes, lazy loading, format conversion, blur placeholder |
| Loading skeletons | Custom shimmer animations | shadcn/ui `Skeleton` component | Already installed, consistent with admin pages |
| Toast notifications | Custom notification system | `sonner` (already installed) | Already wired in the project |
| Product card hover effects | Complex JS-based hover | Tailwind CSS `group-hover` utilities | Pure CSS, zero JS, better performance |
| Responsive grid | Manual media query breakpoints | Tailwind CSS grid classes (`grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4`) | Built-in responsive, zero custom CSS |
| Currency formatting | Manual string formatting | `Intl.NumberFormat` (built-in JS) | Handles locale, currency symbols, decimal places |
| Debounce | Custom debounce utility | `setTimeout`/`clearTimeout` in useEffect | Already the pattern used in admin products page; 3 lines of code |

**Key insight:** The project already has established patterns from the admin UI (skeleton loading, fetch-based API calls, debounced search). The storefront should follow these same patterns, not introduce new paradigms.

## Common Pitfalls

### Pitfall 1: Infinite Scroll Duplicate/Missing Items on Page Boundaries
**What goes wrong:** When items are added/removed while user scrolls, offset-based pagination can skip or duplicate items.
**Why it happens:** `OFFSET` shifts when new items are inserted before the current position.
**How to avoid:** For ~50 products with infrequent changes, this is negligible. Use cursor-based pagination only if needed later. For now, offset pagination is fine and matches existing `GetProductsQuery`.
**Warning signs:** Users report seeing duplicate product cards.

### Pitfall 2: Search Flicker on URL Update
**What goes wrong:** Updating URL params triggers a re-render that clears the input momentarily.
**Why it happens:** `useSearchParams()` causes component re-render when URL changes.
**How to avoid:** Keep a local `useState` for the input value, debounce before pushing to URL. The input reads from local state, the fetch reads from URL params.
**Warning signs:** Text input flickers or cursor jumps while typing.

### Pitfall 3: next/image with Placeholder Images
**What goes wrong:** `next/image` requires known dimensions for remote images. Placeholder/generated images may not have consistent sizes.
**Why it happens:** Next.js requires `width` and `height` for remote images, or use `fill` with a sized container.
**How to avoid:** Always use `fill` mode with a fixed-aspect-ratio container (`aspect-square` or `aspect-[4/3]`). Use `object-cover` for consistent rendering.
**Warning signs:** Images appear stretched or layout shift occurs.

### Pitfall 4: Storefront Showing Draft/Archived Products
**What goes wrong:** Customer-facing pages show admin-only products.
**Why it happens:** Existing `GetProductsQuery` returns all statuses unless filtered.
**How to avoid:** Create a dedicated storefront query (or always pass `status=Published`) that filters to Published products only. Never rely on frontend filtering for visibility control.
**Warning signs:** Customers see "Draft" badges or incomplete products.

### Pitfall 5: Seed Data with Hardcoded GUIDs Conflicting on Re-run
**What goes wrong:** Seed data fails on second run due to duplicate key violations.
**Why it happens:** `HasData` uses fixed IDs that conflict if data already exists.
**How to avoid:** Use `UseAsyncSeeding` which supports `if (!context.Products.Any())` guard logic.
**Warning signs:** Application crashes on startup after initial seed.

### Pitfall 6: CORS Issues with Storefront Fetch
**What goes wrong:** Storefront client-side fetches fail due to CORS.
**Why it happens:** The existing CORS config allows `localhost:3000` and `localhost:3001` only.
**How to avoid:** Verify CORS config includes the port the storefront runs on. If using server-side data fetching (RSC), CORS is not needed since it goes server-to-server.
**Warning signs:** Network errors in browser console with CORS messages.

## Code Examples

### Product Card Component
```typescript
// src/components/storefront/product-card.tsx
'use client';
import Image from 'next/image';
import Link from 'next/link';
import { Badge } from '@/components/ui/badge';
import { Button } from '@/components/ui/button';
import { ShoppingCart } from 'lucide-react';
import type { ProductDto } from '@/lib/api';

export function ProductCard({ product }: { product: ProductDto }) {
  const formattedPrice = new Intl.NumberFormat('en-US', {
    style: 'currency',
    currency: product.priceCurrency,
  }).format(product.price);

  return (
    <Link href={`/products/${product.id}`} className="group">
      <div className="relative aspect-square overflow-hidden rounded-xl bg-muted">
        {product.imageUrl ? (
          <Image
            src={product.imageUrl}
            alt={product.name}
            fill
            className="object-cover transition-transform duration-300 group-hover:scale-105"
            sizes="(max-width: 640px) 100vw, (max-width: 1024px) 50vw, 25vw"
          />
        ) : (
          <div className="flex h-full items-center justify-center text-muted-foreground">
            No image
          </div>
        )}
        {/* Hover overlay with Add to Cart */}
        <div className="absolute inset-0 flex items-end justify-center bg-black/0 p-4 opacity-0 transition-all group-hover:bg-black/10 group-hover:opacity-100">
          <Button size="sm" className="w-full" onClick={(e) => { e.preventDefault(); /* Cart Phase 6 */ }}>
            <ShoppingCart className="mr-2 h-4 w-4" />
            Add to Cart
          </Button>
        </div>
      </div>
      <div className="mt-3 space-y-1">
        <Badge variant="secondary" className="text-xs">{product.categoryName}</Badge>
        <h3 className="font-medium text-sm line-clamp-1">{product.name}</h3>
        <p className="text-sm font-semibold">{formattedPrice}</p>
      </div>
    </Link>
  );
}
```

### Infinite Scroll Product Grid
```typescript
// src/components/storefront/product-grid.tsx
'use client';
import { useState, useEffect, useCallback } from 'react';
import { useIntersectionObserver } from '@/hooks/use-intersection-observer';
import { ProductCard } from './product-card';
import { Skeleton } from '@/components/ui/skeleton';
import { getProducts, type ProductDto } from '@/lib/api';

interface ProductGridProps {
  categoryId?: string;
  search?: string;
  sortBy?: string;
  sortDirection?: string;
}

export function ProductGrid({ categoryId, search, sortBy, sortDirection }: ProductGridProps) {
  const [products, setProducts] = useState<ProductDto[]>([]);
  const [page, setPage] = useState(1);
  const [hasMore, setHasMore] = useState(true);
  const [loading, setLoading] = useState(true);
  const { ref: sentinelRef, isIntersecting } = useIntersectionObserver();

  const pageSize = 12;

  const fetchPage = useCallback(async (pageNum: number, reset = false) => {
    setLoading(true);
    try {
      const data = await getProducts({
        page: pageNum,
        pageSize,
        categoryId: categoryId || undefined,
        status: 'Published',
        search: search || undefined,
      });
      setProducts(prev => reset ? data.items : [...prev, ...data.items]);
      setHasMore(pageNum * pageSize < data.totalCount);
    } finally {
      setLoading(false);
    }
  }, [categoryId, search, sortBy, sortDirection]);

  // Reset on filter change
  useEffect(() => {
    setPage(1);
    setProducts([]);
    fetchPage(1, true);
  }, [fetchPage]);

  // Load next page on intersection
  useEffect(() => {
    if (isIntersecting && hasMore && !loading) {
      const nextPage = page + 1;
      setPage(nextPage);
      fetchPage(nextPage);
    }
  }, [isIntersecting]);

  return (
    <>
      <div className="grid grid-cols-1 gap-6 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4">
        {products.map(product => (
          <ProductCard key={product.id} product={product} />
        ))}
        {loading && Array.from({ length: pageSize }).map((_, i) => (
          <ProductCardSkeleton key={`skeleton-${i}`} />
        ))}
      </div>
      {hasMore && <div ref={sentinelRef} className="h-1" />}
    </>
  );
}

function ProductCardSkeleton() {
  return (
    <div className="space-y-3">
      <Skeleton className="aspect-square rounded-xl" />
      <Skeleton className="h-4 w-16" />
      <Skeleton className="h-4 w-3/4" />
      <Skeleton className="h-4 w-1/4" />
    </div>
  );
}
```

### Backend Sort Extension
```csharp
// In GetProductsQueryHandler, replace fixed OrderByDescending:
query = request.SortBy?.ToLower() switch
{
    "price" => request.SortDirection == "desc"
        ? query.OrderByDescending(p => p.Price.Amount)
        : query.OrderBy(p => p.Price.Amount),
    "name" => request.SortDirection == "desc"
        ? query.OrderByDescending(p => p.Name.Value)
        : query.OrderBy(p => p.Name.Value),
    _ => query.OrderByDescending(p => p.CreatedAt) // "newest" or default
};
```

### Seed Data with UseAsyncSeeding
```csharp
// In Program.cs or CatalogDbContext configuration
builder.Services.AddDbContext<CatalogDbContext>((sp, options) =>
{
    // ... existing config ...

    options.UseAsyncSeeding(async (context, _, ct) =>
    {
        var dbContext = (CatalogDbContext)context;
        if (await dbContext.Categories.AnyAsync(ct)) return; // Already seeded

        // Create categories
        var laptops = Category.Create(CategoryName.Create("Laptops"), "Portable computing devices");
        // ... more categories ...

        dbContext.Categories.AddRange(laptops, /* ... */);
        await dbContext.SaveChangesAsync(ct);

        // Create products using domain factory methods
        var macbook = Product.Create(
            ProductName.Create("MacBook Pro 16\""),
            "Apple M4 Pro chip, 18GB RAM, 512GB SSD",
            Money.Create(2499.00m, "USD"),
            laptops.Id,
            imageUrl: "/images/placeholder-laptop.svg");
        macbook.Publish(); // Make visible to customers

        dbContext.Products.AddRange(macbook, /* ... */);
        await dbContext.SaveChangesAsync(ct);
    });
});
```

### Responsive Grid Breakpoints (Recommendation)
```
- Mobile (< 640px):  1 column, full width cards
- Tablet (640-1023px): 2 columns
- Desktop (1024-1279px): 3 columns
- Wide (>= 1280px): 4 columns
```
Achieved with: `grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4`

## Discretion Recommendations

Based on research, here are recommendations for the Claude's Discretion items:

| Decision | Recommendation | Rationale |
|----------|---------------|-----------|
| Grid columns | 1/2/3/4 responsive (above) | Standard e-commerce pattern, Apple Store uses 3-4 |
| Loading approach | Skeleton cards (not spinner) | Already established pattern in admin pages, better perceived performance |
| Search bar placement | In the storefront header | Persistent access from any page, Apple Store pattern |
| Filter UX | Top bar with category chips + sort dropdown | Cleaner than sidebar for <10 categories, mobile-friendly |
| Product detail layout | Image left (60%), info right (40%) on desktop; stacked on mobile | Standard e-commerce, Apple Store uses this |
| Related products | Yes, simple "More from [category]" section, 4 items | Low effort, high value for browsing |
| Quantity selector | Simple button only ("Add to Cart") | Quantity selector adds complexity; defer to Cart phase |
| Product detail depth | Description + category + SKU. No specs table yet | Keep simple for Phase 3, can add specs later |

## State of the Art

| Old Approach | Current Approach | When Changed | Impact |
|--------------|------------------|--------------|--------|
| `getServerSideProps` / `getStaticProps` | App Router Server Components + client `use client` | Next.js 13+ (stable in 14+) | Already using App Router |
| EF Core `HasData` for seeding | `UseSeeding` / `UseAsyncSeeding` | EF Core 9 (2024) | Conditional seeding, complex objects, no migration coupling |
| `placeholder="blur"` requires static import | `fill` + `aspect-ratio` container for dynamic images | Next.js 13+ | Remote images use fill mode, not blur placeholder |
| CSS Grid with custom breakpoints | Tailwind responsive grid utilities | Tailwind v3+ | Zero custom CSS needed |
| `next/image` `priority` prop | `next/image` `preload` prop | Next.js 16 | `priority` deprecated, use `preload` instead |

**Deprecated/outdated:**
- `next/image` `priority` prop: Use `preload` instead in Next.js 16
- `pages/` directory router: Project already on App Router
- `getServerSideProps`: Use Server Components or client-side fetch

## Open Questions

1. **UseAsyncSeeding availability in .NET 10 + Aspire integration**
   - What we know: UseAsyncSeeding was introduced in EF Core 9. The project uses Npgsql.EntityFrameworkCore.PostgreSQL 10.0.0 which should include it. However, the DbContext is configured via `builder.AddNpgsqlDbContext<>()` (Aspire helper), not direct `AddDbContext`.
   - What's unclear: Whether `UseAsyncSeeding` works when DbContext is registered via Aspire's `AddNpgsqlDbContext`. May need to configure seeding separately.
   - Recommendation: If `UseAsyncSeeding` doesn't integrate cleanly with Aspire registration, fall back to an `IHostedService` that runs on startup and seeds if empty. This is equally valid and more explicit.

2. **Placeholder images for seed data**
   - What we know: Products need images. Real product photos aren't available. Azure Blob Storage (Azurite) is configured for image storage.
   - What's unclear: Whether to use inline SVG placeholders, generate solid-color placeholder URLs, or use a placeholder service.
   - Recommendation: Use simple colored SVG data URIs or local `/public/images/` placeholder files. Avoid external placeholder services (they break offline). Store the image URL as a simple path; don't upload to Blob Storage during seeding.

3. **Backend query for storefront vs admin**
   - What we know: The existing `GetProductsQuery` serves admin needs (all statuses). The storefront needs only Published products.
   - What's unclear: Whether to create a separate query (e.g., `BrowseProductsQuery`) or reuse `GetProductsQuery` with `status=Published` filter from the frontend.
   - Recommendation: Reuse `GetProductsQuery` with `status=Published` passed from the storefront. Add sort parameters to the existing query. This avoids duplication while the app is a monolith. Create separate queries only when extracting to microservices.

## Sources

### Primary (HIGH confidence)
- Project codebase: `src/lib/api.ts`, `src/app/admin/products/page.tsx` - established data-fetching patterns
- Project codebase: `GetProductsQuery.cs`, `GetProductsQueryHandler.cs` - existing API contract
- Project codebase: `package.json` - Next.js 16.0.3, React 19.2.0 confirmed
- Project codebase: `MicroCommerce.ApiService.csproj` - .NET 10, EF Core 10 confirmed
- [Next.js Image Component docs](https://nextjs.org/docs/app/api-reference/components/image) - `preload` replaces `priority` in v16
- [EF Core Data Seeding docs](https://learn.microsoft.com/en-us/ef/core/modeling/data-seeding) - UseSeeding/UseAsyncSeeding

### Secondary (MEDIUM confidence)
- [Infinite Scroll with Server Actions - LogRocket](https://blog.logrocket.com/implementing-infinite-scroll-next-js-server-actions/) - Intersection Observer pattern verified across multiple sources
- [TanStack Query Infinite Queries docs](https://tanstack.com/query/latest/docs/framework/react/guides/infinite-queries) - Confirmed TanStack Query NOT needed for our simpler use case
- [Npgsql Full Text Search docs](https://www.npgsql.org/efcore/mapping/full-text-search.html) - ILIKE sufficient for ~50 products
- [nuqs - URL state management](https://nuqs.dev/) - Evaluated but not recommended (overkill for this use case)

### Tertiary (LOW confidence)
- [shadcn/ui Product Cards - ShadcnUIKit](https://shadcnuikit.com/components/cards/product) - Third-party patterns, not official

## Metadata

**Confidence breakdown:**
- Standard stack: HIGH - All libraries already in project, versions confirmed from package.json/csproj
- Architecture: HIGH - Route groups, client components, Intersection Observer are well-documented Next.js patterns
- Pitfalls: HIGH - Derived from actual codebase analysis (CORS config, status filtering, image handling)
- Seed data: MEDIUM - UseAsyncSeeding is documented but integration with Aspire's AddNpgsqlDbContext is unverified
- Discretion recommendations: MEDIUM - Based on Apple Store aesthetic requirement and e-commerce conventions

**Research date:** 2026-02-07
**Valid until:** 2026-03-07 (stable stack, 30 days)
