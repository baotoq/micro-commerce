# Phase 6: Cart Domain - Research

**Researched:** 2026-02-09
**Domain:** Shopping cart (backend DDD aggregate + frontend optimistic UI)
**Confidence:** HIGH

## Summary

The Cart Domain requires a Cart aggregate root with CartItem entities, a guest buyer identity system via cookies, database-backed persistence using the existing schema-per-module pattern, a background job for cart expiration, and a React Query-powered frontend with optimistic mutations.

The existing codebase already has established patterns for all backend concerns: module DbContexts with schema isolation (Catalog, Inventory), BackgroundService for periodic cleanup (ReservationCleanupService), CQRS via MediatR, minimal API endpoints, and a Sonner toast system in the storefront layout. The main new additions are: (1) introducing `@tanstack/react-query` for the first time to the frontend, (2) implementing cookie-based buyer identity middleware, and (3) the cart merge logic on authentication.

**Primary recommendation:** Follow the existing module patterns exactly (Inventory is the closest analog). Add `@tanstack/react-query` v5 for optimistic mutations on the cart page. Use a cookie-based `BuyerId` (GUID) with 7-day TTL, stored server-side as a raw Guid on the Cart aggregate (same cross-module pattern as `ProductId` in Inventory).

## Standard Stack

### Core (Already in Codebase)
| Library | Version | Purpose | Why Standard |
|---------|---------|---------|--------------|
| EF Core (Npgsql) | 10.0 | Cart PostgreSQL persistence | Already used for Catalog, Inventory, Ordering schemas |
| MediatR | 13.1.0 | CQRS command/query handlers | Already wired with validation pipeline |
| FluentValidation | (current) | Request validation | Already auto-discovered from assembly |
| Sonner | 2.0.7 | Toast notifications | Already installed and configured in storefront layout |
| shadcn/ui | (current) | UI components (AlertDialog, Button, Skeleton, Card) | Already installed |
| lucide-react | 0.563.0 | Icons (ShoppingCart, Trash2, Minus, Plus) | Already installed |

### New Dependencies
| Library | Version | Purpose | Why Standard |
|---------|---------|---------|--------------|
| @tanstack/react-query | ^5.90 | Server state management, optimistic updates | De facto standard for React server state; roadmap specifies React Query for data fetching |
| @tanstack/react-query-devtools | ^5.91 | Dev tools for debugging queries | Optional but highly recommended for development |

### Alternatives Considered
| Instead of | Could Use | Tradeoff |
|------------|-----------|----------|
| @tanstack/react-query | SWR | React Query has better mutation/optimistic update support and more features; project roadmap explicitly specifies React Query |
| @tanstack/react-query | Plain fetch + useState (current pattern) | Current codebase uses this but it lacks cache invalidation, optimistic updates, and deduplication needed for cart |
| Cookie middleware | ASP.NET Session | Cookies are lighter, work across browser restarts, and don't require session state server-side |

**Installation:**
```bash
cd src/MicroCommerce.Web && npm install @tanstack/react-query @tanstack/react-query-devtools
```

No new NuGet packages needed - all backend dependencies already in project.

## Architecture Patterns

### Recommended Project Structure
```
Features/Cart/
├── Domain/
│   ├── Entities/
│   │   ├── Cart.cs              # Aggregate root
│   │   └── CartItem.cs          # Entity (owned by Cart)
│   ├── Events/
│   │   ├── ItemAddedToCartDomainEvent.cs
│   │   └── CartExpiredDomainEvent.cs
│   └── ValueObjects/
│       ├── CartId.cs            # StronglyTypedId<Guid>
│       ├── CartItemId.cs        # StronglyTypedId<Guid>
│       └── BuyerId.cs           # StronglyTypedId<Guid> or raw Guid
├── Application/
│   ├── Commands/
│   │   ├── AddToCart/           # AddToCartCommand + Handler + Validator
│   │   ├── UpdateCartItem/      # UpdateCartItemCommand + Handler
│   │   ├── RemoveCartItem/      # RemoveCartItemCommand + Handler
│   │   └── MergeCarts/          # MergeCartsCommand + Handler (login merge)
│   └── Queries/
│       ├── GetCart/             # GetCartQuery + Handler + CartDto
│       └── GetCartItemCount/   # GetCartItemCountQuery + Handler
├── Infrastructure/
│   ├── CartDbContext.cs         # Already exists (schema: 'cart')
│   ├── Configurations/
│   │   ├── CartConfiguration.cs
│   │   └── CartItemConfiguration.cs
│   ├── CartExpirationService.cs # BackgroundService (30-day TTL)
│   └── Migrations/
└── CartEndpoints.cs             # Minimal API endpoints
```

Frontend additions:
```
src/
├── lib/
│   └── api.ts                   # Add cart API types + functions
├── components/
│   ├── providers/
│   │   └── query-provider.tsx   # QueryClientProvider wrapper (NEW)
│   └── storefront/
│       ├── header.tsx           # Update: cart badge with live count
│       ├── cart-page.tsx        # Cart page component (NEW)
│       ├── cart-item-row.tsx    # Individual cart item row (NEW)
│       └── cart-summary.tsx     # Order summary sidebar (NEW)
├── hooks/
│   └── use-cart.ts              # Cart React Query hooks (NEW)
└── app/
    └── (storefront)/
        └── cart/
            └── page.tsx         # /cart route (NEW)
```

### Pattern 1: Cart Aggregate with CartItem Entities

**What:** Cart is the aggregate root; CartItem is an entity owned by Cart. All mutations go through Cart methods.
**When to use:** Always - this is the DDD aggregate pattern already established in this codebase.
**Example:**
```csharp
// Follows same pattern as StockItem aggregate in Inventory
public sealed class Cart : BaseAggregateRoot<CartId>
{
    private readonly List<CartItem> _items = [];

    public Guid BuyerId { get; private set; }  // Raw Guid like ProductId in Inventory
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset LastModifiedAt { get; private set; }
    public DateTimeOffset ExpiresAt { get; private set; }

    [Timestamp]
    public uint Version { get; private set; }  // xmin concurrency token

    public IReadOnlyCollection<CartItem> Items => _items.AsReadOnly();

    // Factory method (established pattern)
    public static Cart Create(Guid buyerId)
    {
        return new Cart(CartId.New())
        {
            BuyerId = buyerId,
            CreatedAt = DateTimeOffset.UtcNow,
            LastModifiedAt = DateTimeOffset.UtcNow,
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(30)
        };
    }

    public void AddItem(Guid productId, string productName, decimal unitPrice, string? imageUrl, int quantity = 1)
    {
        var existing = _items.FirstOrDefault(i => i.ProductId == productId);
        if (existing is not null)
        {
            existing.IncrementQuantity(quantity);
        }
        else
        {
            _items.Add(CartItem.Create(Id, productId, productName, unitPrice, imageUrl, quantity));
        }
        Touch();
    }

    public void UpdateItemQuantity(CartItemId itemId, int newQuantity)
    {
        var item = _items.FirstOrDefault(i => i.Id == itemId)
            ?? throw new InvalidOperationException("Cart item not found");
        item.SetQuantity(newQuantity);
        Touch();
    }

    public void RemoveItem(CartItemId itemId)
    {
        var item = _items.FirstOrDefault(i => i.Id == itemId);
        if (item is not null)
        {
            _items.Remove(item);
            Touch();
        }
    }

    private void Touch()
    {
        LastModifiedAt = DateTimeOffset.UtcNow;
        ExpiresAt = DateTimeOffset.UtcNow.AddDays(30); // Reset TTL on activity
    }
}
```

### Pattern 2: Cookie-Based Buyer Identity

**What:** ASP.NET Core middleware that reads/sets a `buyer_id` cookie containing a GUID. No authentication required.
**When to use:** Every cart endpoint needs a buyer identity. Guests get a cookie; authenticated users use their `sub` claim.
**Example:**
```csharp
// Middleware or endpoint helper
public static Guid GetOrCreateBuyerId(HttpContext context)
{
    // Authenticated user: use their subject claim
    if (context.User.Identity?.IsAuthenticated == true)
    {
        var sub = context.User.FindFirst("sub")?.Value;
        if (sub is not null && Guid.TryParse(sub, out var userId))
            return userId;
    }

    // Guest: read from cookie or create new
    const string CookieName = "buyer_id";
    if (context.Request.Cookies.TryGetValue(CookieName, out var cookieValue)
        && Guid.TryParse(cookieValue, out var buyerId))
    {
        return buyerId;
    }

    // Generate new buyer ID
    var newBuyerId = Guid.NewGuid();
    context.Response.Cookies.Append(CookieName, newBuyerId.ToString(), new CookieOptions
    {
        HttpOnly = true,
        Secure = true,
        SameSite = SameSiteMode.Lax,
        MaxAge = TimeSpan.FromDays(7),
        Path = "/"
    });
    return newBuyerId;
}
```

### Pattern 3: React Query Optimistic Mutation for Cart

**What:** useMutation with onMutate for instant UI feedback, rollback on failure.
**When to use:** All cart update operations (add, update quantity, remove).
**Example:**
```typescript
// Source: TanStack Query v5 official docs pattern
function useUpdateCartItem() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (params: { itemId: string; quantity: number }) =>
      updateCartItemQuantity(params.itemId, params.quantity),
    onMutate: async (params) => {
      // Cancel outgoing refetches
      await queryClient.cancelQueries({ queryKey: ['cart'] });

      // Snapshot previous value
      const previousCart = queryClient.getQueryData<CartDto>(['cart']);

      // Optimistically update
      queryClient.setQueryData<CartDto>(['cart'], (old) => {
        if (!old) return old;
        return {
          ...old,
          items: old.items.map((item) =>
            item.id === params.itemId
              ? { ...item, quantity: params.quantity }
              : item
          ),
        };
      });

      return { previousCart };
    },
    onError: (_err, _params, context) => {
      // Rollback on error
      if (context?.previousCart) {
        queryClient.setQueryData(['cart'], context.previousCart);
      }
      toast.error('Failed to update quantity');
    },
    onSettled: () => {
      // Refetch to ensure server state
      queryClient.invalidateQueries({ queryKey: ['cart'] });
    },
  });
}
```

### Pattern 4: QueryClientProvider Setup in Next.js

**What:** Wrap the app with QueryClientProvider as a client component.
**When to use:** One-time setup in providers.
**Example:**
```tsx
// src/components/providers/query-provider.tsx
"use client";

import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { ReactQueryDevtools } from "@tanstack/react-query-devtools";
import { useState, type ReactNode } from "react";

export function QueryProvider({ children }: { children: ReactNode }) {
  const [queryClient] = useState(
    () =>
      new QueryClient({
        defaultOptions: {
          queries: {
            staleTime: 60 * 1000, // 1 minute
            retry: 1,
          },
        },
      })
  );

  return (
    <QueryClientProvider client={queryClient}>
      {children}
      <ReactQueryDevtools initialIsOpen={false} />
    </QueryClientProvider>
  );
}
```

### Anti-Patterns to Avoid
- **Cart as value object:** Cart has identity and lifecycle (creation, modification, expiration). It must be an entity/aggregate, not a value object.
- **Storing full cart in cookie:** Only store the buyer_id in the cookie, not cart contents. Cart data belongs in the database (CART-02 requirement).
- **Bypassing aggregate for item updates:** All CartItem mutations must go through Cart methods to maintain invariants (e.g., max quantity cap).
- **Separate query keys per cart item:** Use a single `['cart']` query key for the entire cart. Invalidating individual items creates cache inconsistency.
- **Creating QueryClient outside useState:** In Next.js with React 18+, creating QueryClient at module level causes shared state across requests in SSR.

## Don't Hand-Roll

| Problem | Don't Build | Use Instead | Why |
|---------|-------------|-------------|-----|
| Server state caching & sync | Custom fetch + useState cache | @tanstack/react-query | Handles stale-while-revalidate, deduplication, cache invalidation, background refetch |
| Optimistic UI with rollback | Manual state management + error handling | useMutation with onMutate/onError/onSettled | Handles concurrent mutations, automatic rollback, query invalidation |
| Toast notifications | Custom toast component | sonner (already installed) | Already configured in storefront layout with proper positioning |
| Confirmation dialog | Custom modal | AlertDialog from shadcn/ui (already installed) | Already installed, accessible, consistent with admin UI patterns |
| Cookie management | Custom cookie parsing library | Native HttpContext.Request.Cookies / Response.Cookies | ASP.NET Core built-in cookie handling is sufficient; no library needed |
| Periodic cleanup | Hangfire / Quartz.NET | BackgroundService (built-in) | Existing pattern in ReservationCleanupService; simple timer is sufficient for cart expiration |

**Key insight:** The codebase already has 90% of the infrastructure needed. The only truly new addition is @tanstack/react-query. Everything else follows established patterns.

## Common Pitfalls

### Pitfall 1: Race Condition on Add-to-Cart with Duplicate Detection
**What goes wrong:** Two rapid "add to cart" clicks create two separate cart items for the same product instead of incrementing quantity.
**Why it happens:** The aggregate loads, checks for existing item, doesn't find one (because first request hasn't saved yet), and creates a new item.
**How to avoid:** Use optimistic concurrency (xmin token, already the established pattern). The second request will get a concurrency exception and retry. Also, disable the "Add to Cart" button during mutation (isPending from useMutation).
**Warning signs:** Duplicate cart items for same product.

### Pitfall 2: QueryClient Created at Module Level in Next.js
**What goes wrong:** Shared QueryClient state between server-side requests, causing data leakage between users.
**Why it happens:** Module-level singletons are shared in Node.js.
**How to avoid:** Always create QueryClient inside useState(() => new QueryClient()) in a client component.
**Warning signs:** Users seeing other users' cart data.

### Pitfall 3: Cart Merge Losing Items on Login
**What goes wrong:** When a guest logs in with an existing authenticated cart, items from one cart are lost.
**Why it happens:** Naive merge replaces one cart with another instead of combining.
**How to avoid:** Load both carts (guest by cookie buyer_id, authenticated by user sub), merge items summing quantities for matching products, delete the guest cart, update the cookie to the authenticated buyer_id.
**Warning signs:** Users reporting items disappearing after login.

### Pitfall 4: Forgetting to Pass Credentials (Cookies) in Fetch
**What goes wrong:** Cart API returns empty cart or creates new one on every request.
**Why it happens:** fetch() doesn't send cookies by default in cross-origin requests.
**How to avoid:** Use `credentials: 'include'` in fetch options. CORS must also have `AllowCredentials()` (already configured in Program.cs).
**Warning signs:** Every cart request returns a different/empty cart.

### Pitfall 5: Not Resetting Cart Expiration on Activity
**What goes wrong:** Active users' carts expire after 30 days from creation, even though they were using the cart yesterday.
**Why it happens:** ExpiresAt set once at cart creation and never updated.
**How to avoid:** Update ExpiresAt to `now + 30 days` on every cart modification (the `Touch()` method in the aggregate).
**Warning signs:** Active users getting "cart expired" messages.

### Pitfall 6: Cart Badge Count Not Updating Across Pages
**What goes wrong:** Adding item on product detail page, then navigating to another page shows stale badge count.
**Why it happens:** Badge count is fetched independently and not connected to cart query cache.
**How to avoid:** Use the same `['cart']` query key for both the cart page and the badge count. The badge can derive count from the cached cart data using `select` option in useQuery.
**Warning signs:** Badge shows 0 after adding item until page refresh.

## Code Examples

### Cart Endpoint Pattern (Following Inventory Module)
```csharp
// CartEndpoints.cs - follows exact pattern of InventoryEndpoints.cs
public static class CartEndpoints
{
    public static IEndpointRouteBuilder MapCartEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/cart")
            .WithTags("Cart");

        group.MapGet("/", GetCart)
            .WithName("GetCart");

        group.MapPost("/items", AddToCart)
            .WithName("AddToCart");

        group.MapPut("/items/{itemId:guid}", UpdateCartItem)
            .WithName("UpdateCartItem");

        group.MapDelete("/items/{itemId:guid}", RemoveCartItem)
            .WithName("RemoveCartItem");

        return endpoints;
    }

    private static async Task<IResult> GetCart(
        HttpContext httpContext,
        ISender sender,
        CancellationToken ct)
    {
        var buyerId = BuyerIdentity.GetOrCreateBuyerId(httpContext);
        var result = await sender.Send(new GetCartQuery(buyerId), ct);
        return Results.Ok(result);
    }

    // ... other endpoints follow same pattern
}
```

### Cart Expiration BackgroundService (Following ReservationCleanupService)
```csharp
// Follows exact pattern of ReservationCleanupService
public sealed class CartExpirationService : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromHours(1); // Hourly check

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<CartExpirationService> _logger;

    public CartExpirationService(
        IServiceScopeFactory scopeFactory,
        ILogger<CartExpirationService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(Interval, stoppingToken);

            try
            {
                using var scope = _scopeFactory.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<CartDbContext>();

                var expiredCount = await context.Carts
                    .Where(c => c.ExpiresAt <= DateTimeOffset.UtcNow)
                    .ExecuteDeleteAsync(stoppingToken);

                if (expiredCount > 0)
                {
                    _logger.LogInformation(
                        "Removed {Count} expired carts", expiredCount);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cleaning up expired carts");
            }
        }
    }
}
```

### React Query Cart Hook Pattern
```typescript
// src/hooks/use-cart.ts
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";
import { getCart, addToCart, updateCartItemQuantity, removeCartItem } from "@/lib/api";

export function useCart() {
  return useQuery({
    queryKey: ["cart"],
    queryFn: getCart,
  });
}

export function useCartItemCount() {
  return useQuery({
    queryKey: ["cart"],
    queryFn: getCart,
    select: (data) => data?.items.reduce((sum, item) => sum + item.quantity, 0) ?? 0,
  });
}

export function useAddToCart() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: addToCart,
    onSuccess: (_data, variables) => {
      queryClient.invalidateQueries({ queryKey: ["cart"] });
      toast.success(variables.isUpdate ? "Updated quantity" : "Added to cart");
    },
    onError: () => {
      toast.error("Failed to add item to cart");
    },
  });
}
```

### Fetch with Credentials (Critical for Cookie-Based Identity)
```typescript
// All cart API functions must include credentials
export async function getCart(): Promise<CartDto> {
  const response = await fetch(`${API_BASE}/api/cart`, {
    cache: "no-store",
    credentials: "include",  // CRITICAL: sends buyer_id cookie
  });

  if (!response.ok) {
    throw new Error("Failed to fetch cart");
  }

  return response.json();
}
```

## State of the Art

| Old Approach | Current Approach | When Changed | Impact |
|--------------|------------------|--------------|--------|
| Session-based cart storage | Database-backed with cookie identity | Standard practice | Survives server restarts, scalable |
| Redux for server state | TanStack Query (React Query) | 2021-2022 | Purpose-built for server state; less boilerplate |
| Manual optimistic UI | useMutation onMutate/onError/onSettled | React Query v4+ | Automatic rollback, cache management |
| Custom toast system | Sonner | 2023+ | Lightweight, accessible, promise-based |

**Deprecated/outdated:**
- Session-based cart: Does not persist across browser restarts
- Redux Toolkit Query for cart: Viable but React Query is the project standard
- SWR: Less mutation support than React Query

## Open Questions

1. **Max quantity per item cap**
   - What we know: Context says "Claude's discretion"
   - Recommendation: Cap at 99 (reasonable for e-commerce). Validate server-side in aggregate. Show validation error toast if exceeded.

2. **Cart merge edge cases**
   - What we know: "Sum quantities for same products" is the decision
   - What's unclear: Should summed quantities respect the max cap? What if sum exceeds stock?
   - Recommendation: Cap at max quantity (99). Don't check stock during merge (stock is checked at checkout in Phase 7).

3. **Cart expiration toast timing**
   - What we know: "Toast notification 'Your cart has expired' on next visit after expiry"
   - What's unclear: How to detect "expired cart" vs "never had a cart"
   - Recommendation: When GetCart returns null/empty but the `buyer_id` cookie exists, check if a cart was recently expired. Alternatively, the simpler approach: if cookie exists but no cart found, show the expiration toast. First-time visitors won't have the cookie.

4. **Existing data fetching pattern migration**
   - What we know: Current frontend uses plain fetch + useState (no React Query)
   - What's unclear: Should we migrate existing pages (product grid, product detail) to React Query too?
   - Recommendation: Only add React Query for cart operations in Phase 6. Migrating existing pages would be scope creep. The QueryProvider will be set up globally so future phases can adopt React Query incrementally.

## Sources

### Primary (HIGH confidence)
- Codebase analysis: `Features/Inventory/` module (aggregate, DbContext, endpoints, BackgroundService patterns)
- Codebase analysis: `Features/Catalog/` module (CQRS, domain events, factory method patterns)
- Codebase analysis: `Program.cs` (CartDbContext already registered, CORS with AllowCredentials already configured)
- Codebase analysis: `Components/storefront/header.tsx` (existing cart badge placeholder with hardcoded "0")
- Codebase analysis: `Components/storefront/product-detail.tsx` (existing "Add to Cart" button with placeholder toast)
- Codebase analysis: `Components/ui/sonner.tsx` (Sonner already configured)
- Codebase analysis: `package.json` (current dependency list, no React Query yet)

### Secondary (MEDIUM confidence)
- [TanStack Query v5 Optimistic Updates Docs](https://tanstack.com/query/latest/docs/framework/react/guides/optimistic-updates) - onMutate/onError/onSettled pattern
- [TanStack Query v5 Mutations Docs](https://tanstack.com/query/v5/docs/react/guides/mutations) - useMutation API
- [TanStack Query v5 Installation](https://tanstack.com/query/v5/docs/react/installation) - v5.90+ confirmed current
- [TanStack Query Next.js Integration Guide](https://www.storieasy.com/blog/integrate-tanstack-query-with-next-js-app-router-2025-ultimate-guide) - QueryClientProvider setup with useState
- [TkDodo: Concurrent Optimistic Updates](https://tkdodo.eu/blog/concurrent-optimistic-updates-in-react-query) - cancelQueries + snapshot pattern

### Tertiary (LOW confidence)
- [Walmart Cart Microservice DDD Pattern](https://medium.com/walmartglobaltech/implementing-cart-service-with-ddd-hexagonal-port-adapter-architecture-part-2-d9c00e290ab) - Cart aggregate design reference
- [Martin Fowler: DDD Aggregate](https://martinfowler.com/bliki/DDD_Aggregate.html) - Aggregate boundaries theory

## Metadata

**Confidence breakdown:**
- Standard stack: HIGH - All backend libraries already in codebase; React Query is well-documented and the project decision
- Architecture: HIGH - Directly follows established Inventory/Catalog module patterns; structure is predictable
- Pitfalls: HIGH - Identified from codebase analysis (cookie/CORS setup, concurrency patterns) and documented React Query patterns

**Research date:** 2026-02-09
**Valid until:** 2026-03-09 (30 days - stable technologies, established patterns)
