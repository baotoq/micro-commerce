# Phase 13: Wishlists & Saved Items - Research

**Researched:** 2026-02-13
**Domain:** E-commerce wishlist/favorites system with .NET backend and Next.js frontend
**Confidence:** HIGH

## Summary

Phase 13 implements a persistent wishlist feature allowing authenticated users to save products for future purchase. Users toggle a heart icon on product cards and detail pages to add/remove items, view a dedicated wishlist page, and move items to cart with stock validation. The wishlist requires authentication and persists across sessions and devices.

The backend follows the existing vertical slice architecture with a dedicated Wishlists feature module using its own WishlistsDbContext. The domain model is simpler than Reviews - just a join table with composite unique constraint on (UserId, ProductId) to prevent duplicates. Unlike Cart (which supports guest users), Wishlist requires authentication from the start, simplifying the domain logic.

The frontend implements optimistic UI updates for heart icon toggles using TanStack Query's `onMutate` pattern (already proven in cart operations). The heart icon appears on both product cards and detail pages with instant visual feedback. The wishlist page reuses existing product card grid layout, adding an "Add to cart" button and handling out-of-stock states with disabled buttons and visual dimming.

**Primary recommendation:** Follow existing architecture patterns. Create Wishlists feature with own DbContext, use composite unique index on (UserId, ProductId), implement optimistic UI with TanStack Query mutations, add heart icon to header nav with count badge (mirror cart pattern), and reuse product card components for wishlist page grid.

<user_constraints>
## User Constraints (from CONTEXT.md)

### Locked Decisions

#### Heart icon interaction
- Heart icon appears on both product cards (grid) AND product detail page — consistent access everywhere
- Outlined heart → filled red heart on click (instant toggle, no animation)
- Clicking heart as guest redirects to login/register — wishlist requires authentication
- Heart icon position on product cards: Claude's discretion (top-right overlay on image is common pattern)

#### Wishlist page layout
- Reuse existing product card grid layout — consistent with storefront browsing experience
- Accessible from both My Account sidebar AND a heart icon in the header nav bar
- Header heart icon shows count badge (number of wishlist items) — similar to cart count
- Empty state: "Your wishlist is empty" message with a "Browse products" CTA button

#### Move-to-cart behavior
- "Add to cart" button per item — single item at a time, no batch operations
- Item stays in wishlist after adding to cart — user removes manually if they want
- Always adds quantity of 1 — user adjusts quantity in cart if needed
- Toast notification after adding: "Added to cart" — non-blocking, consistent with existing cart behavior

#### Out-of-stock handling
- Out-of-stock items stay visible but dimmed/grayed out with "Out of stock" badge
- "Add to cart" button disabled for out-of-stock items
- Always show current price — no comparison to when item was saved
- Out-of-stock items keep their original position in the grid (not pushed to bottom)
- Removing items: clicking the filled heart on a wishlist card removes it — consistent toggle pattern

### Claude's Discretion
- Heart icon exact position on product cards (top-right of image is suggested)
- Loading states and skeleton design for wishlist page
- Exact styling of the "Out of stock" badge
- Toast notification duration and style
- Sort order of wishlist items (e.g., most recently added first)

### Deferred Ideas (OUT OF SCOPE)
None — discussion stayed within phase scope

</user_constraints>

## Standard Stack

### Backend Core
| Library | Version | Purpose | Why Standard |
|---------|---------|---------|--------------|
| .NET | 10.0 | Runtime | Project standard, already in use |
| MediatR | 13.1.0 | CQRS pattern | Already in use for commands/queries |
| FluentValidation | 12.1.1 | Input validation | Already in use with pipeline behavior |
| EF Core PostgreSQL | 10.0.0 | Data access | Already in use for all features |
| MassTransit | 9.0.0 | Domain events | Already in use (optional for wishlists) |

### Frontend Core
| Library | Version | Purpose | Why Standard |
|---------|---------|---------|--------------|
| Next.js | 16.0.3 | React framework | Project standard |
| React | 19.2.0 | UI library | Project standard |
| TypeScript | 5 | Type safety | Project standard |
| TanStack Query | 5.90.20 | Data fetching + optimistic UI | Already in use |
| shadcn-ui | Latest | Component library | Already in use |
| Radix UI | Various | Accessible primitives | Foundation for shadcn-ui |
| Lucide React | 0.563.0 | Icon library (Heart icon) | Already in use |

### Supporting Libraries
| Library | Version | Purpose | When to Use |
|---------|---------|---------|-------------|
| None required | N/A | All needs met by existing stack | N/A |

**Installation:**
No new packages required — all dependencies already present in the project.

## Architecture Patterns

### Recommended Project Structure
```
src/MicroCommerce.ApiService/Features/Wishlists/
├── WishlistsEndpoints.cs            # Minimal API route mapping
├── Domain/
│   ├── Entities/
│   │   └── WishlistItem.cs          # Simple entity (not aggregate root)
│   └── ValueObjects/
│       └── WishlistItemId.cs        # Strongly-typed ID
├── Application/
│   ├── Commands/
│   │   ├── AddToWishlist/
│   │   │   ├── AddToWishlistCommand.cs
│   │   │   └── AddToWishlistCommandHandler.cs
│   │   └── RemoveFromWishlist/
│   │       ├── RemoveFromWishlistCommand.cs
│   │       └── RemoveFromWishlistCommandHandler.cs
│   └── Queries/
│       ├── GetUserWishlist/
│       │   ├── GetUserWishlistQuery.cs
│       │   ├── GetUserWishlistQueryHandler.cs
│       │   └── WishlistItemDto.cs
│       ├── CheckIfInWishlist/
│       │   ├── CheckIfInWishlistQuery.cs
│       │   └── CheckIfInWishlistQueryHandler.cs
│       └── GetWishlistCount/
│           ├── GetWishlistCountQuery.cs
│           └── GetWishlistCountQueryHandler.cs
├── Infrastructure/
│   ├── WishlistsDbContext.cs        # Owned DbContext
│   ├── Configurations/
│   │   └── WishlistItemConfiguration.cs  # EF Core entity config
│   └── WishlistsDataSeeder.cs       # Dev seed data (optional)
└── DependencyInjection.cs           # Feature service registration

src/MicroCommerce.Web/src/components/wishlist/
├── wishlist-toggle-button.tsx      # Heart icon toggle with optimistic UI
├── wishlist-grid.tsx                # Grid of wishlist items
├── wishlist-item-card.tsx           # Single wishlist item (extends product-card)
└── wishlist-empty-state.tsx         # Empty state message + CTA

src/MicroCommerce.Web/src/app/(storefront)/
└── wishlist/
    └── page.tsx                     # Wishlist page route
```

### Pattern 1: Simple Join Table Entity (Not Aggregate Root)

**What:** WishlistItem as a simple entity representing user-product relationship, not a full aggregate root.

**When to use:** Wishlist doesn't need domain events or complex business logic — just add/remove operations.

**Example:**
```csharp
// Source: Simplified pattern from existing entities
public sealed class WishlistItem
{
    public WishlistItemId Id { get; private set; } = null!;
    public Guid UserId { get; private set; }
    public Guid ProductId { get; private set; }
    public DateTimeOffset AddedAt { get; private set; }

    [Timestamp]
    public uint Version { get; private set; } // xmin optimistic concurrency

    // EF Core constructor
    private WishlistItem() { }

    public static WishlistItem Create(Guid userId, Guid productId)
    {
        return new WishlistItem
        {
            Id = WishlistItemId.New(),
            UserId = userId,
            ProductId = productId,
            AddedAt = DateTimeOffset.UtcNow
        };
    }
}
```

### Pattern 2: Composite Unique Constraint

**What:** PostgreSQL unique index on (UserId, ProductId) to enforce one entry per user per product.

**When to use:** Wishlist item configuration — prevents duplicate saves even under race conditions.

**Example:**
```csharp
// Source: Existing ReviewConfiguration pattern
public class WishlistItemConfiguration : IEntityTypeConfiguration<WishlistItem>
{
    public void Configure(EntityTypeBuilder<WishlistItem> builder)
    {
        builder.ToTable("WishlistItems");
        builder.HasKey(w => w.Id);

        builder.Property(w => w.Id)
            .HasConversion(id => id.Value, value => new WishlistItemId(value))
            .ValueGeneratedNever();

        // Composite unique index - one entry per user per product
        builder.HasIndex(w => new { w.UserId, w.ProductId })
            .IsUnique();

        // Index on UserId for listing user's wishlist
        builder.HasIndex(w => w.UserId);

        // Index on AddedAt descending for chronological sort
        builder.HasIndex(w => w.AddedAt)
            .IsDescending();

        builder.Property(w => w.UserId).IsRequired();
        builder.Property(w => w.ProductId).IsRequired();
        builder.Property(w => w.AddedAt).IsRequired();

        builder.Property(w => w.Version).IsRowVersion();
    }
}
```

### Pattern 3: Join Query with Multiple DbContexts

**What:** Query WishlistItems, then join with Catalog and Inventory for product details and stock info.

**When to use:** GetUserWishlist query — similar to GetReviewsByProduct pattern.

**Example:**
```csharp
// Source: Existing GetReviewsByProduct cross-context query pattern
public sealed class GetUserWishlistQueryHandler
    : IRequestHandler<GetUserWishlistQuery, List<WishlistItemDto>>
{
    private readonly WishlistsDbContext _wishlistsContext;
    private readonly CatalogDbContext _catalogContext;
    private readonly InventoryDbContext _inventoryContext;

    public async Task<List<WishlistItemDto>> Handle(
        GetUserWishlistQuery request,
        CancellationToken cancellationToken)
    {
        // Get user's wishlist items
        var wishlistItems = await _wishlistsContext.WishlistItems
            .Where(w => w.UserId == request.UserId)
            .OrderByDescending(w => w.AddedAt) // Most recent first
            .ToListAsync(cancellationToken);

        if (wishlistItems.Count == 0)
            return [];

        var productIds = wishlistItems.Select(w => w.ProductId).ToList();

        // Batch lookup: Get product details
        var products = await _catalogContext.Products
            .Where(p => productIds.Contains(p.Id.Value))
            .ToDictionaryAsync(p => p.Id.Value, cancellationToken);

        // Batch lookup: Get stock info
        var stockInfos = await _inventoryContext.Stocks
            .Where(s => productIds.Contains(s.ProductId))
            .ToDictionaryAsync(s => s.ProductId, cancellationToken);

        // Map to DTOs
        return wishlistItems.Select(w =>
        {
            var product = products.GetValueOrDefault(w.ProductId);
            var stock = stockInfos.GetValueOrDefault(w.ProductId);

            return new WishlistItemDto(
                w.Id.Value,
                w.ProductId,
                product?.Name.Value ?? "Unknown",
                product?.Price.Amount ?? 0,
                product?.Price.Currency ?? "USD",
                product?.ImageUrl,
                product?.AverageRating,
                product?.ReviewCount ?? 0,
                stock?.AvailableQuantity ?? 0,
                w.AddedAt
            );
        }).ToList();
    }
}
```

### Pattern 4: Optimistic UI with TanStack Query

**What:** Update UI instantly before server confirms, rollback on error using `onMutate` and `onError` callbacks.

**When to use:** Heart icon toggle for add/remove operations — proven pattern from existing cart hooks.

**Example:**
```tsx
// Source: Existing useRemoveCartItem optimistic pattern
"use client";

import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";
import { addToWishlist, removeFromWishlist, getUserWishlist } from "@/lib/api";

const WISHLIST_QUERY_KEY = ["wishlist"] as const;

export function useToggleWishlist(productId: string) {
  const queryClient = useQueryClient();

  const addMutation = useMutation({
    mutationFn: () => addToWishlist(productId),
    onMutate: async () => {
      await queryClient.cancelQueries({ queryKey: WISHLIST_QUERY_KEY });

      const previousWishlist = queryClient.getQueryData(WISHLIST_QUERY_KEY);

      // Optimistically add to UI
      queryClient.setQueryData(WISHLIST_QUERY_KEY, (old: any) => {
        // Add product ID to wishlist set
        return { ...old, items: [...(old?.items || []), { productId }] };
      });

      return { previousWishlist };
    },
    onError: (_err, _vars, context) => {
      if (context?.previousWishlist !== undefined) {
        queryClient.setQueryData(WISHLIST_QUERY_KEY, context.previousWishlist);
      }
      toast.error("Failed to add to wishlist");
    },
    onSettled: () => {
      queryClient.invalidateQueries({ queryKey: WISHLIST_QUERY_KEY });
    },
  });

  const removeMutation = useMutation({
    mutationFn: () => removeFromWishlist(productId),
    onMutate: async () => {
      await queryClient.cancelQueries({ queryKey: WISHLIST_QUERY_KEY });

      const previousWishlist = queryClient.getQueryData(WISHLIST_QUERY_KEY);

      // Optimistically remove from UI
      queryClient.setQueryData(WISHLIST_QUERY_KEY, (old: any) => {
        return {
          ...old,
          items: old?.items.filter((item: any) => item.productId !== productId) || []
        };
      });

      return { previousWishlist };
    },
    onError: (_err, _vars, context) => {
      if (context?.previousWishlist !== undefined) {
        queryClient.setQueryData(WISHLIST_QUERY_KEY, context.previousWishlist);
      }
      toast.error("Failed to remove from wishlist");
    },
    onSettled: () => {
      queryClient.invalidateQueries({ queryKey: WISHLIST_QUERY_KEY });
    },
  });

  return { addMutation, removeMutation };
}
```

### Pattern 5: Heart Icon Toggle Component

**What:** Client component with filled/outlined Heart icon from Lucide React, controlled by wishlist state.

**When to use:** Product cards and detail pages — requires "use client" for click handlers.

**Example:**
```tsx
// Source: Lucide-react Heart icon + TanStack Query pattern
"use client";

import { Heart } from "lucide-react";
import { useSession } from "next-auth/react";
import { useRouter } from "next/navigation";
import { useCheckIfInWishlist, useToggleWishlist } from "@/hooks/use-wishlist";

interface WishlistToggleButtonProps {
  productId: string;
  className?: string;
}

export function WishlistToggleButton({ productId, className }: WishlistToggleButtonProps) {
  const { data: session } = useSession();
  const router = useRouter();
  const { data: isInWishlist = false } = useCheckIfInWishlist(productId);
  const { addMutation, removeMutation } = useToggleWishlist(productId);

  const handleClick = (e: React.MouseEvent) => {
    e.preventDefault(); // Don't navigate to product detail
    e.stopPropagation();

    if (!session) {
      // Redirect to login
      router.push("/api/auth/signin");
      return;
    }

    if (isInWishlist) {
      removeMutation.mutate();
    } else {
      addMutation.mutate();
    }
  };

  const isPending = addMutation.isPending || removeMutation.isPending;

  return (
    <button
      onClick={handleClick}
      disabled={isPending}
      className={`transition-colors ${className}`}
      aria-label={isInWishlist ? "Remove from wishlist" : "Add to wishlist"}
    >
      <Heart
        className={`size-5 ${
          isInWishlist
            ? "fill-red-500 text-red-500"
            : "text-zinc-400 hover:text-red-500"
        }`}
      />
    </button>
  );
}
```

### Pattern 6: Header Count Badge

**What:** Mirror cart count badge pattern with wishlist icon and count in header nav.

**When to use:** Header component — shows user's total wishlist items.

**Example:**
```tsx
// Source: Existing Header cart count badge pattern
import { Heart } from "lucide-react";
import { useWishlistItemCount } from "@/hooks/use-wishlist";

// In Header component:
const { data: wishlistCount = 0 } = useWishlistItemCount();

<Link
  href="/wishlist"
  className="relative text-zinc-500 transition-colors hover:text-zinc-900"
  aria-label="Wishlist"
>
  <Heart className="h-4 w-4" />
  {wishlistCount > 0 && (
    <span className="absolute -right-1.5 -top-1.5 flex h-3.5 w-3.5 items-center justify-center rounded-full bg-zinc-900 text-[9px] font-medium text-white">
      {wishlistCount > 99 ? '99+' : wishlistCount}
    </span>
  )}
</Link>
```

### Anti-Patterns to Avoid

- **Storing product details on WishlistItem:** Only store ProductId — join with Catalog for current details (price, name, image)
- **Guest wishlist support:** Don't implement cookie-based wishlist — require authentication to avoid complexity
- **Cascade delete from User:** Use ON DELETE CASCADE from User to WishlistItems, but RESTRICT from Product (prevent product deletion if wishlisted)
- **Separate "isInWishlist" API per product:** Batch query all wishlist items on page load, check in memory for each product
- **Using aggregate root pattern:** WishlistItem is a simple entity, doesn't need domain events or complex invariants

## Don't Hand-Roll

| Problem | Don't Build | Use Instead | Why |
|---------|-------------|-------------|-----|
| Heart icon rendering | Custom SVG with fill states | Lucide-react Heart with fill prop | Accessibility, consistent sizing, optimized SVGs already handled |
| Optimistic UI rollback | Manual state management | TanStack Query onMutate/onError | Edge cases: concurrent mutations, network failures, race conditions all handled |
| Unique constraint enforcement | Application-level duplicate check | PostgreSQL composite unique index | Database guarantees atomicity, prevents race conditions from concurrent requests |
| Product data in wishlist | Denormalize product fields | Join query with CatalogDbContext | Product details change (price, name, image) — wishlist should always show current data |
| Authentication redirect | Custom login flow | NextAuth useSession + router.push | Session management, callback URLs, CSRF protection already implemented |

**Key insight:** Wishlist is fundamentally a simple join table with complex UI requirements. The complexity is in the frontend (optimistic updates, toast notifications, stock validation) not the backend domain model. Use proven query patterns (batch lookups) and existing UI patterns (cart count badge, product cards) rather than building custom solutions.

## Common Pitfalls

### Pitfall 1: N+1 Queries When Loading Wishlist Page

**What goes wrong:** Fetching wishlist items then querying Product and Stock details one-by-one results in N+1 query problem.

**Why it happens:** Natural inclination to use navigation properties or separate queries per item.

**How to avoid:** Batch lookup pattern — collect all ProductIds, then query Catalog and Inventory in two queries with `WHERE IN` clauses.

**Warning signs:** Slow wishlist page load, database query count proportional to number of wishlist items.

### Pitfall 2: Stale Product Data in Wishlist

**What goes wrong:** Showing old product prices, images, or names from when item was added to wishlist.

**Why it happens:** Denormalizing product details onto WishlistItem entity.

**How to avoid:** Store only ProductId, join with Catalog on every query to get current product data.

**Warning signs:** Users see outdated prices that don't match product detail page, confusion about price changes.

### Pitfall 3: Guest Wishlist Complexity

**What goes wrong:** Attempting to support guest wishlists with cookie storage and merge logic like Cart.

**Why it happens:** Trying to maximize conversions by allowing non-authenticated saves.

**How to avoid:** Require authentication from the start — redirect to login when guest clicks heart icon. Simpler implementation, better user experience (persistent across devices).

**Warning signs:** Complex merge logic on login, cookie size limits, cross-device inconsistency.

### Pitfall 4: Heart Icon State Flicker on Page Load

**What goes wrong:** Heart shows outlined briefly then fills in after query completes, causing visual flicker.

**Why it happens:** Waiting for individual `checkIfInWishlist` API calls per product on page.

**How to avoid:** Load all wishlist items once, store ProductIds in Set, check membership in memory. Or use optimistic rendering with Suspense boundaries.

**Warning signs:** Visible flash from outlined to filled heart icons when browsing products.

### Pitfall 5: Missing Authentication Check on Backend

**What goes wrong:** Allowing unauthenticated requests to wishlist endpoints, causing authorization errors.

**Why it happens:** Forgetting to add `.RequireAuthorization()` on all wishlist endpoints.

**How to avoid:** All wishlist endpoints require auth — extract UserId from JWT claims (same pattern as Reviews).

**Warning signs:** 401 errors in browser console, inability to save wishlist items even when logged in.

### Pitfall 6: Product Deletion Breaks Wishlist Display

**What goes wrong:** Deleted products cause null references or missing data in wishlist grid.

**Why it happens:** No foreign key constraint or graceful handling of missing products.

**How to avoid:** Either restrict product deletion if wishlisted (ON DELETE RESTRICT), or filter out null products in query handler with warning log.

**Warning signs:** Blank cards in wishlist grid, null reference exceptions in query handler.

## Code Examples

Verified patterns from official sources and existing codebase:

### Add to Wishlist Command

```csharp
// Source: Existing CreateReviewCommand pattern simplified
public sealed record AddToWishlistCommand(Guid UserId, Guid ProductId) : IRequest<Guid>;

public sealed class AddToWishlistCommandHandler
    : IRequestHandler<AddToWishlistCommand, Guid>
{
    private readonly WishlistsDbContext _context;

    public async Task<Guid> Handle(
        AddToWishlistCommand request,
        CancellationToken cancellationToken)
    {
        // Check if already in wishlist (upsert pattern)
        var existing = await _context.WishlistItems
            .FirstOrDefaultAsync(
                w => w.UserId == request.UserId && w.ProductId == request.ProductId,
                cancellationToken);

        if (existing is not null)
            return existing.Id.Value;

        var item = WishlistItem.Create(request.UserId, request.ProductId);
        _context.WishlistItems.Add(item);

        await _context.SaveChangesAsync(cancellationToken);

        return item.Id.Value;
    }
}
```

### Remove from Wishlist Command

```csharp
// Source: Existing DeleteReviewCommand pattern
public sealed record RemoveFromWishlistCommand(Guid UserId, Guid ProductId) : IRequest;

public sealed class RemoveFromWishlistCommandHandler
    : IRequestHandler<RemoveFromWishlistCommand>
{
    private readonly WishlistsDbContext _context;

    public async Task Handle(
        RemoveFromWishlistCommand request,
        CancellationToken cancellationToken)
    {
        var item = await _context.WishlistItems
            .FirstOrDefaultAsync(
                w => w.UserId == request.UserId && w.ProductId == request.ProductId,
                cancellationToken);

        if (item is null)
            return; // Idempotent - already removed

        _context.WishlistItems.Remove(item);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
```

### Check if Product is in Wishlist

```csharp
// Source: Simple boolean query pattern
public sealed record CheckIfInWishlistQuery(Guid UserId, Guid ProductId) : IRequest<bool>;

public sealed class CheckIfInWishlistQueryHandler
    : IRequestHandler<CheckIfInWishlistQuery, bool>
{
    private readonly WishlistsDbContext _context;

    public async Task<bool> Handle(
        CheckIfInWishlistQuery request,
        CancellationToken cancellationToken)
    {
        return await _context.WishlistItems
            .AnyAsync(
                w => w.UserId == request.UserId && w.ProductId == request.ProductId,
                cancellationToken);
    }
}
```

### Wishlist API Endpoints

```csharp
// Source: Existing ReviewsEndpoints pattern
public static class WishlistsEndpoints
{
    public static IEndpointRouteBuilder MapWishlistsEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/wishlist")
            .WithTags("Wishlist")
            .RequireAuthorization(); // ALL endpoints require auth

        group.MapGet("/", GetUserWishlist)
            .WithName("GetUserWishlist")
            .WithSummary("Get current user's wishlist")
            .Produces<List<WishlistItemDto>>();

        group.MapGet("/count", GetWishlistCount)
            .WithName("GetWishlistCount")
            .WithSummary("Get wishlist item count")
            .Produces<int>();

        group.MapGet("/check/{productId:guid}", CheckIfInWishlist)
            .WithName("CheckIfInWishlist")
            .WithSummary("Check if product is in wishlist")
            .Produces<bool>();

        group.MapPost("/{productId:guid}", AddToWishlist)
            .WithName("AddToWishlist")
            .WithSummary("Add product to wishlist")
            .Produces(StatusCodes.Status201Created);

        group.MapDelete("/{productId:guid}", RemoveFromWishlist)
            .WithName("RemoveFromWishlist")
            .WithSummary("Remove product from wishlist")
            .Produces(StatusCodes.Status204NoContent);

        return endpoints;
    }

    private static Guid GetUserId(HttpContext context)
    {
        var sub = context.User.FindFirstValue(ClaimTypes.NameIdentifier)
                  ?? context.User.FindFirstValue("sub");

        if (string.IsNullOrEmpty(sub) || !Guid.TryParse(sub, out var userId))
        {
            throw new UnauthorizedAccessException("User ID not found in claims");
        }

        return userId;
    }
}
```

### Wishlist Item Card Component

```tsx
// Source: Extending existing ProductCard pattern
import { ProductCard } from "@/components/storefront/product-card";
import { Button } from "@/components/ui/button";
import { WishlistToggleButton } from "./wishlist-toggle-button";
import { useAddToCart } from "@/hooks/use-cart";
import type { WishlistItemDto } from "@/lib/api";

interface WishlistItemCardProps {
  item: WishlistItemDto;
}

export function WishlistItemCard({ item }: WishlistItemCardProps) {
  const addToCart = useAddToCart();
  const isOutOfStock = item.availableQuantity === 0;

  const handleAddToCart = (e: React.MouseEvent) => {
    e.preventDefault();
    e.stopPropagation();

    addToCart.mutate({
      productId: item.productId,
      quantity: 1,
    });
  };

  return (
    <div className="relative group">
      {/* Reuse existing product card */}
      <div className={`relative ${isOutOfStock ? "opacity-60" : ""}`}>
        {/* Heart icon - top right */}
        <div className="absolute right-2 top-2 z-10">
          <WishlistToggleButton productId={item.productId} />
        </div>

        {/* Product details - reuse ProductCard or inline */}
        <Link href={`/products/${item.productId}`} className="block">
          <div className="aspect-square bg-zinc-100 rounded-lg overflow-hidden">
            {item.imageUrl && (
              <Image
                src={item.imageUrl}
                alt={item.productName}
                fill
                className={`object-cover ${isOutOfStock ? "grayscale" : ""}`}
              />
            )}
          </div>
          <div className="mt-3">
            <h3 className="text-sm font-medium">{item.productName}</h3>
            <p className="text-sm font-semibold mt-1">
              {formatPrice(item.price, item.currency)}
            </p>
          </div>
        </Link>

        {/* Add to Cart button */}
        <Button
          onClick={handleAddToCart}
          disabled={isOutOfStock || addToCart.isPending}
          className="w-full mt-3"
          size="sm"
        >
          {isOutOfStock ? "Out of Stock" : "Add to Cart"}
        </Button>
      </div>
    </div>
  );
}
```

## State of the Art

| Old Approach | Current Approach | When Changed | Impact |
|--------------|------------------|--------------|--------|
| localStorage for guest wishlist | Authentication required | User decision for Phase 13 | Simpler implementation, cross-device sync, no merge logic |
| Synchronous toggle updates | Optimistic UI with TanStack Query | React Query v3+ (2021) | Instant feedback, better UX, automatic error recovery |
| Denormalized product data | Join query with Catalog | DDD adoption | Always current prices/names, no data staleness |
| Individual product check APIs | Batch query all wishlist items | Performance optimization pattern | Reduces API calls from N to 1 on page load |
| Lucide icons outline only | Fill prop for filled state | Lucide v0.200+ (2023) | Native filled icon support, no custom SVGs |

**Deprecated/outdated:**
- **Cookie-based wishlist:** Modern auth-required approach is simpler and provides better UX
- **localStorage wishlist:** Doesn't sync across devices, lost on browser clear
- **Custom heart SVG icons:** Lucide-react provides optimized, accessible Heart component
- **Individual REST endpoints per wishlist item:** Batch operations and queries are standard now

## Open Questions

1. **Should wishlist support pagination or infinite scroll?**
   - What we know: User constraints don't mention pagination, "Load more" pattern used in Reviews
   - What's unclear: Expected maximum wishlist size per user
   - Recommendation: Start with no pagination (load all items), add offset pagination if needed (Phase 13+ enhancement)

2. **Should wishlists have a maximum item limit?**
   - What we know: No limit mentioned in requirements
   - What's unclear: Database and performance implications of very large wishlists
   - Recommendation: No limit for Phase 13, monitor usage and add soft limit (e.g., 100 items) if needed

3. **How to handle product price changes since adding to wishlist?**
   - What we know: User constraints say "always show current price"
   - What's unclear: Should users be notified of price drops or increases?
   - Recommendation: Phase 13 shows current price only. Phase 14+ could add "price when saved" field and show delta

4. **Should wishlist items have notes or tags?**
   - What we know: Requirements mention simple save/unsave functionality
   - What's unclear: User desire for organizing wishlist (gift lists, categories)
   - Recommendation: Out of scope for Phase 13. Simple list is sufficient for MVP

## Sources

### Primary (HIGH confidence)
- Existing codebase architecture patterns: `/Users/baotoq/Work/micro-commerce/src/MicroCommerce.ApiService/Features/{Reviews,Cart,Profiles}/`
- PostgreSQL documentation: [Constraints](https://www.postgresql.org/docs/current/ddl-constraints.html), [Unique Indexes](https://www.postgresql.org/docs/current/indexes-unique.html)
- EF Core documentation: Existing entity configurations showing unique indexes, owned entities
- React component patterns: Existing product-card.tsx, header.tsx, use-cart.ts hooks
- TanStack Query documentation: Optimistic updates pattern from existing cart hooks

### Secondary (MEDIUM confidence)
- [Vertabelo E-commerce Database Design](https://vertabelo.com/blog/er-diagram-for-online-shop/) - Wishlist entity relationships
- [PostgreSQL Unique Constraint Best Practices](https://www.dbvis.com/thetable/all-you-need-to-know-about-postgresql-unique-constraint/) - Composite key patterns
- [CQRS Pattern in .NET](https://learn.microsoft.com/en-us/dotnet/architecture/microservices/microservice-ddd-cqrs-patterns/cqrs-microservice-reads) - Microsoft official guidance
- [React useOptimistic Hook](https://react.dev/reference/react/useOptimistic) - Official React 19 documentation
- [Lucide React Documentation](https://lucide.dev/guide/packages/lucide-react) - Heart icon fill prop usage
- [Lucide Filled Icons Guide](https://lucide.dev/guide/advanced/filled-icons) - Using fill prop for toggle states

### Tertiary (LOW confidence)
- E-commerce wishlist UX patterns from web search — informational context only

## Metadata

**Confidence breakdown:**
- Standard stack: HIGH - All libraries already in project, versions confirmed via package files
- Architecture: HIGH - Patterns directly from existing codebase (Reviews, Cart, Profiles features)
- Pitfalls: MEDIUM-HIGH - Mix of database best practices (HIGH) and e-commerce domain patterns (MEDIUM)
- Frontend patterns: HIGH - Directly from existing cart hooks, header components, and product cards

**Research date:** 2026-02-13
**Valid until:** 2026-03-15 (30 days for stable patterns, tech stack unlikely to change)

**Cross-module dependencies:**
- Catalog: Query Products for current name, price, image, rating, review count
- Inventory: Query Stocks for availableQuantity to show out-of-stock state
- Profiles: Extract UserId from JWT claims (same pattern as Reviews)
- Cart: Reuse AddToCart mutation when moving wishlist items to cart
- Common: Reuse ValidationBehavior, strongly-typed IDs, base patterns

**Database considerations:**
- New schema: `wishlists` (follows pattern from `profiles`, `reviews`, `catalog`, `ordering`)
- New DbContext: `WishlistsDbContext` with own connection/migration history
- Composite unique index: `IX_WishlistItems_UserId_ProductId` for one-per-user enforcement
- Descending index: `IX_WishlistItems_AddedAt_Desc` for chronological sorting (most recent first)
- Foreign keys: UserId (no FK, just Guid), ProductId (FK to Products with ON DELETE RESTRICT or manual handling)

**UI/UX considerations:**
- Heart icon position: Top-right of product image (z-index above image, below stock badge)
- Optimistic updates: Heart fills/unfills instantly, reverses on error
- Toast notifications: "Added to wishlist" / "Removed from wishlist" / "Added to cart"
- Loading states: Disable heart button while mutation pending, show loading state on wishlist page
- Empty state: Center message with "Browse products" button linking to home/catalog
- Out-of-stock styling: Grayscale image, opacity 60%, disabled "Add to cart" button, "Out of stock" badge
