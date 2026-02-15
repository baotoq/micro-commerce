# Architecture Integration: User Profiles, Reviews & Wishlists

**Domain:** E-commerce modular monolith with bounded contexts
**Researched:** 2026-02-13
**Confidence:** HIGH

## Executive Summary

User profiles, product reviews, and wishlists should be **three separate bounded contexts** that integrate with the existing modular monolith via domain events, cross-context queries through MediatR, and data migration patterns at authentication boundaries.

**Key findings:**
1. **Profiles** is a new bounded context (not extension of existing) — owns user identity enrichment, addresses, avatar storage
2. **Reviews** is a new bounded context with cross-context references via ProductId and UserId (NOT foreign keys) — verified purchase validation via domain events
3. **Wishlists** is separate from Cart (different lifecycle, behavior, persistence needs) — similar structure but distinct aggregate
4. **Guest-to-user migration** follows WooCommerce 2026 pattern — filter-based merge strategy on authentication events
5. **Avatar storage** uses Azure Blob Storage (already integrated for product images) with database URL references

This preserves database-per-feature isolation, leverages existing event-driven infrastructure (MassTransit + DomainEventInterceptor), and maintains clear bounded context boundaries for future service extraction.

## Recommended Architecture

### Bounded Context Boundaries

```
Features/
  ├── Catalog/          [EXISTING] Products, categories, images
  ├── Cart/             [EXISTING] Shopping cart (guest + auth)
  ├── Ordering/         [EXISTING] Checkout, orders, payment
  ├── Inventory/        [EXISTING] Stock management
  ├── Messaging/        [EXISTING] Dead letter queue UI
  ├── Profiles/         [NEW] User identity enrichment, addresses, avatar
  ├── Reviews/          [NEW] Product reviews with verified purchase
  └── Wishlists/        [NEW] User wishlists (single list per user)
```

**Rationale for three new contexts:**
- **Profiles**: User identity is a distinct subdomain from ordering/cart. Different lifecycle (created once, updated rarely vs transactional cart/order operations). Potential future extraction to identity service.
- **Reviews**: Product feedback is a separate concern from catalog (products exist without reviews). Cross-cuts Users and Products but owns review-specific rules (verified purchase, moderation state, helpful votes).
- **Wishlists**: Similar to cart structurally but fundamentally different behavior: no expiration, no checkout flow, purely aspirational vs transactional. Separate lifecycle warrants separate aggregate.

### Component Boundaries

| Component | Responsibility | Communicates With |
|-----------|---------------|-------------------|
| **Profiles** | User profile CRUD, avatar upload, address management, link BuyerId to auth user | Ordering (via events: user created → link existing orders), Cart (via events: merge guest cart) |
| **Reviews** | Review CRUD, rating aggregation, verified purchase validation, moderation | Catalog (read ProductId for validation), Ordering (consume OrderConfirmed event for verified flag), Profiles (read UserId for display name) |
| **Wishlists** | Wishlist item CRUD, move to cart | Catalog (read product data), Cart (publish AddToCart command when moving items) |
| **Cart** [MODIFIED] | Add guest-to-user merge on authentication event | Profiles (consume UserAuthenticated event) |
| **Ordering** [MODIFIED] | Add UserId denormalization for profile linking | Profiles (consume UserCreated event to backfill BuyerId→UserId mapping) |

### Data Flow

#### 1. User Profile Creation
```
[NextAuth.js login] → [Keycloak JWT with sub claim]
  → [POST /api/profiles] (CreateProfile command)
  → [UserCreatedDomainEvent published]
  → [OrderingContext: UserCreatedConsumer updates Orders.UserId from BuyerId mapping]
  → [CartContext: UserCreatedConsumer merges guest cart if BuyerId matches]
```

#### 2. Product Review Submission
```
[User clicks "Write Review" on product page]
  → [POST /api/reviews] (SubmitReview command with ProductId, UserId, Rating, Text)
  → [ReviewContext validates: ProductId exists? User purchased product?]
  → [Query OrderingContext via MediatR: GetOrdersByBuyerQuery filtered by ProductId]
  → [If order found + status Delivered → IsVerifiedPurchase = true]
  → [ReviewSubmittedDomainEvent published]
  → [CatalogContext: ReviewSubmittedConsumer increments product review count, updates average rating]
```

**Cross-context query pattern (NOT foreign keys):**
```csharp
// In ReviewsContext SubmitReviewCommandHandler
public async Task<SubmitReviewResult> Handle(SubmitReviewCommand request, CancellationToken ct)
{
    // Validate product exists (cross-context query)
    var productExists = await _mediator.Send(new ProductExistsQuery(request.ProductId), ct);
    if (!productExists) throw new DomainException("Product not found");

    // Check verified purchase (cross-context query)
    var hasPurchased = await _mediator.Send(
        new HasUserPurchasedProductQuery(request.UserId, request.ProductId), ct);

    var review = Review.Create(
        request.ProductId,
        request.UserId,
        request.Rating,
        request.ReviewText,
        isVerifiedPurchase: hasPurchased);

    _context.Reviews.Add(review);
    await _context.SaveChangesAsync(ct);

    return new SubmitReviewResult(review.Id.Value);
}
```

#### 3. Wishlist to Cart Migration
```
[User clicks "Move to Cart" on wishlist item]
  → [POST /api/wishlists/{id}/move-to-cart] (MoveWishlistItemToCart command)
  → [WishlistContext: Load wishlist item, get product data]
  → [CartContext: AddToCart command published as domain event]
  → [WishlistContext: Remove item after cart confirmation]
```

#### 4. Guest Cart to Authenticated User Migration
```
[User logs in with existing guest cart]
  → [NextAuth.js callback → POST /api/profiles/authenticate]
  → [ProfilesContext: UserAuthenticatedDomainEvent published with BuyerId from cookie + UserId from JWT]
  → [CartContext: UserAuthenticatedConsumer]
  → [Load guest cart by BuyerId, load user cart by UserId]
  → [Merge strategy: Combine items, deduplicate by ProductId, sum quantities (capped at MaxQuantity)]
  → [Delete guest cart after merge]
  → [Update cookie to use UserId instead of BuyerId]
```

**Migration filter (inspired by WooCommerce 2026 pattern):**
```csharp
public class UserAuthenticatedConsumer : IConsumer<UserAuthenticatedDomainEvent>
{
    public async Task Consume(ConsumeContext<UserAuthenticatedDomainEvent> context)
    {
        var guestCart = await _context.Carts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.BuyerId == context.Message.GuestBuyerId);

        var userCart = await _context.Carts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.BuyerId == context.Message.UserId);

        if (guestCart is null) return;

        if (userCart is null)
        {
            // No existing user cart: transfer ownership of guest cart
            guestCart.TransferOwnership(context.Message.UserId);
        }
        else
        {
            // Merge: add guest items to user cart, then delete guest cart
            foreach (var guestItem in guestCart.Items)
            {
                userCart.AddItem(
                    guestItem.ProductId,
                    guestItem.ProductName,
                    guestItem.UnitPrice,
                    guestItem.ImageUrl,
                    guestItem.Quantity);
            }
            _context.Carts.Remove(guestCart);
        }

        await _context.SaveChangesAsync();
    }
}
```

### Database Schema Isolation

Each new bounded context gets its own schema in PostgreSQL:

```sql
-- ProfilesDbContext
CREATE SCHEMA profiles;
CREATE TABLE profiles.user_profiles (...);
CREATE TABLE profiles.user_addresses (...);

-- ReviewsDbContext
CREATE SCHEMA reviews;
CREATE TABLE reviews.product_reviews (...);

-- WishlistsDbContext
CREATE SCHEMA wishlists;
CREATE TABLE wishlists.wishlists (...);
CREATE TABLE wishlists.wishlist_items (...);
```

**Migration history isolation:**
```csharp
builder.AddNpgsqlDbContext<ProfilesDbContext>("appdb", configureDbContextOptions: options =>
{
    options.UseNpgsql(npgsql =>
        npgsql.MigrationsHistoryTable("__EFMigrationsHistory", "profiles"));
});
```

### Cross-Context Reference Pattern

**DO NOT use foreign keys across contexts.** Use Guid references with validation via MediatR queries.

```csharp
// In ReviewsContext
public sealed class Review : BaseAggregateRoot<ReviewId>
{
    public Guid ProductId { get; private set; }  // NOT ProductId foreign key
    public Guid UserId { get; private set; }     // NOT UserId foreign key
    public Rating Rating { get; private set; }
    public string ReviewText { get; private set; }
    public bool IsVerifiedPurchase { get; private set; }
    public ReviewStatus Status { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
}
```

**Validation happens in application layer:**
```csharp
// In Application/Commands/SubmitReview/SubmitReviewCommandHandler.cs
var productExists = await _mediator.Send(new ProductExistsQuery(request.ProductId), ct);
var userExists = await _mediator.Send(new UserExistsQuery(request.UserId), ct);
```

**Denormalization for display (eventual consistency acceptable):**
```csharp
// In ReviewsContext for display purposes
public sealed class Review
{
    // Denormalized for read performance
    public string ProductName { get; private set; }  // Updated via ProductUpdated event
    public string UserDisplayName { get; private set; }  // Updated via ProfileUpdated event
}
```

### Avatar Storage Pattern

**Use Azure Blob Storage (already integrated for product images).**

```csharp
// In ProfilesContext Infrastructure
public interface IAvatarStorageService
{
    Task<string> UploadAvatarAsync(Guid userId, Stream fileStream, string contentType, CancellationToken ct);
    Task DeleteAvatarAsync(string avatarUrl, CancellationToken ct);
}

public class AvatarStorageService : IAvatarStorageService
{
    private readonly BlobServiceClient _blobServiceClient;
    private const string ContainerName = "avatars";

    public async Task<string> UploadAvatarAsync(Guid userId, Stream fileStream, string contentType, CancellationToken ct)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(ContainerName);
        await containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob, cancellationToken: ct);

        // Use random filename (security best practice)
        var fileName = $"{userId}/{Path.GetRandomFileName()}.jpg";
        var blobClient = containerClient.GetBlobClient(fileName);

        await blobClient.UploadAsync(fileStream, new BlobHttpHeaders { ContentType = contentType }, cancellationToken: ct);

        return blobClient.Uri.ToString();
    }
}
```

**Security considerations (from Microsoft Learn):**
- Never use client-supplied filename (use `Path.GetRandomFileName()`)
- Disable execute permission on upload folder
- Scan with anti-virus API immediately after upload (production)
- Validate file type and size before upload
- Store URL in database, not file contents

**UserProfile entity:**
```csharp
public sealed class UserProfile : BaseAggregateRoot<UserProfileId>
{
    public Guid UserId { get; private set; }  // From Keycloak sub claim
    public string DisplayName { get; private set; }
    public string? AvatarUrl { get; private set; }  // Blob Storage URL
    public string Email { get; private set; }

    public void UpdateAvatar(string newAvatarUrl, string? previousAvatarUrl)
    {
        AvatarUrl = newAvatarUrl;
        AddDomainEvent(new AvatarUpdatedDomainEvent(Id, previousAvatarUrl)); // For cleanup
    }
}
```

## Integration Points Summary

### New Bounded Contexts

| Context | DbContext | Schema | Aggregate Roots | Domain Events Published | Domain Events Consumed |
|---------|-----------|--------|-----------------|------------------------|----------------------|
| **Profiles** | `ProfilesDbContext` | `profiles` | `UserProfile`, `UserAddress` | `UserCreatedDomainEvent`, `UserAuthenticatedDomainEvent`, `ProfileUpdatedDomainEvent`, `AvatarUpdatedDomainEvent` | None |
| **Reviews** | `ReviewsDbContext` | `reviews` | `Review` | `ReviewSubmittedDomainEvent`, `ReviewApprovedDomainEvent` | `OrderConfirmedDomainEvent` (from Ordering), `ProductArchivedDomainEvent` (from Catalog) |
| **Wishlists** | `WishlistsDbContext` | `wishlists` | `Wishlist`, `WishlistItem` | `WishlistItemAddedDomainEvent`, `WishlistItemRemovedDomainEvent` | None |

### Modified Bounded Contexts

| Context | Changes | New Consumers | New Queries |
|---------|---------|---------------|-------------|
| **Cart** | Add `TransferOwnership` method on Cart aggregate | `UserAuthenticatedConsumer` (merge guest cart) | None |
| **Ordering** | Add optional `UserId` field to Order entity (nullable, backfilled) | `UserCreatedConsumer` (backfill BuyerId→UserId) | `HasUserPurchasedProductQuery` (for verified purchase) |
| **Catalog** | Add review count and average rating (denormalized) | `ReviewSubmittedConsumer`, `ReviewApprovedConsumer` | `ProductExistsQuery` |

### Cross-Context Queries (MediatR)

| Query | Source Context | Target Context | Purpose |
|-------|----------------|----------------|---------|
| `ProductExistsQuery` | Reviews, Wishlists | Catalog | Validate product reference |
| `UserExistsQuery` | Reviews, Wishlists | Profiles | Validate user reference |
| `HasUserPurchasedProductQuery` | Reviews | Ordering | Verified purchase check |
| `GetProductQuery` | Wishlists | Catalog | Fetch product details for wishlist display |
| `GetUserProfileQuery` | Reviews (display) | Profiles | Fetch display name for review UI |

**Implementation pattern:**
```csharp
// In Catalog/Application/Queries/ProductExistsQuery.cs
public sealed record ProductExistsQuery(Guid ProductId) : IRequest<bool>;

public sealed class ProductExistsQueryHandler : IRequestHandler<ProductExistsQuery, bool>
{
    private readonly CatalogDbContext _context;

    public async Task<bool> Handle(ProductExistsQuery request, CancellationToken ct)
    {
        return await _context.Products
            .AnyAsync(p => p.Id == ProductId.From(request.ProductId) && p.Status != ProductStatus.Archived, ct);
    }
}
```

### Event Flow Diagram

```
┌─────────────┐
│  NextAuth   │ Login → JWT with sub claim
└──────┬──────┘
       │
       ↓
┌──────────────────────────────────────────────────────────┐
│ ProfilesContext: CreateProfile (if first login)          │
│ ├─ UserProfile.Create(userId from sub, email, name)      │
│ └─ Publish: UserCreatedDomainEvent                       │
└──────┬───────────────────────────────────────────────────┘
       │
       ├───→ OrderingContext: UserCreatedConsumer
       │     └─ Backfill Order.UserId from BuyerId
       │
       └───→ CartContext: UserAuthenticatedConsumer
             └─ Merge guest cart (BuyerId) → user cart (UserId)

┌─────────────┐
│   Ordering  │ Order Confirmed
└──────┬──────┘
       │
       ↓
┌──────────────────────────────────────────────────────────┐
│ ReviewsContext: OrderConfirmedConsumer                   │
│ └─ Cache eligible products for verified purchase reviews │
└──────────────────────────────────────────────────────────┘

┌─────────────┐
│   Reviews   │ Review Submitted
└──────┬──────┘
       │
       ↓
┌──────────────────────────────────────────────────────────┐
│ CatalogContext: ReviewSubmittedConsumer                  │
│ ├─ Increment Product.ReviewCount                         │
│ └─ Recalculate Product.AverageRating                     │
└──────────────────────────────────────────────────────────┘
```

## Patterns to Follow

### Pattern 1: Cross-Context Validation via MediatR

**What:** Validate references to entities in other bounded contexts using MediatR queries, not foreign keys.

**When:** Any command that references an entity owned by another context (ProductId in Review, UserId in Wishlist).

**Example:**
```csharp
// In Reviews/Application/Commands/SubmitReview/SubmitReviewCommandHandler.cs
public sealed class SubmitReviewCommandHandler : IRequestHandler<SubmitReviewCommand, SubmitReviewResult>
{
    private readonly ReviewsDbContext _context;
    private readonly ISender _mediator;

    public async Task<SubmitReviewResult> Handle(SubmitReviewCommand request, CancellationToken ct)
    {
        // Cross-context validation via MediatR
        bool productExists = await _mediator.Send(new ProductExistsQuery(request.ProductId), ct);
        if (!productExists)
            throw new DomainException("Product not found");

        bool userExists = await _mediator.Send(new UserExistsQuery(request.UserId), ct);
        if (!userExists)
            throw new DomainException("User not found");

        // Verified purchase check (cross-context business rule)
        bool hasPurchased = await _mediator.Send(
            new HasUserPurchasedProductQuery(request.UserId, request.ProductId), ct);

        Review review = Review.Create(
            ProductId.From(request.ProductId),
            UserId.From(request.UserId),
            Rating.Create(request.Rating),
            request.ReviewText,
            isVerifiedPurchase: hasPurchased);

        _context.Reviews.Add(review);
        await _context.SaveChangesAsync(ct);

        return new SubmitReviewResult(review.Id.Value);
    }
}
```

### Pattern 2: Denormalization for Display via Event Consumers

**What:** Cache display-only data from other contexts by consuming domain events and updating denormalized fields.

**When:** Read-heavy operations that would otherwise require cross-context joins (e.g., showing reviewer name on product page).

**Example:**
```csharp
// In Reviews/Domain/Entities/Review.cs
public sealed class Review : BaseAggregateRoot<ReviewId>
{
    // Owned data
    public Guid ProductId { get; private set; }
    public Guid UserId { get; private set; }
    public Rating Rating { get; private set; }

    // Denormalized for display (eventual consistency acceptable)
    public string ProductName { get; private set; }  // From Catalog
    public string UserDisplayName { get; private set; }  // From Profiles

    public void UpdateDenormalizedProductName(string productName)
    {
        ProductName = productName;  // No domain event needed (display-only)
    }
}

// In Reviews/Application/Consumers/ProductUpdatedConsumer.cs
public sealed class ProductUpdatedConsumer : IConsumer<ProductUpdatedDomainEvent>
{
    public async Task Consume(ConsumeContext<ProductUpdatedDomainEvent> context)
    {
        var productId = context.Message.ProductId;

        // Fetch updated product name (cross-context query)
        var product = await _mediator.Send(new GetProductQuery(productId));
        if (product is null) return;

        // Update all reviews for this product
        var reviews = await _context.Reviews
            .Where(r => r.ProductId == productId)
            .ToListAsync();

        foreach (var review in reviews)
        {
            review.UpdateDenormalizedProductName(product.Name);
        }

        await _context.SaveChangesAsync();
    }
}
```

### Pattern 3: Guest-to-User Data Migration on Authentication

**What:** Merge or transfer ownership of guest data (cart, orders) when user authenticates.

**When:** User logs in with existing guest session (BuyerId cookie + UserId from JWT sub claim don't match).

**Example:**
```csharp
// In Cart/Application/Consumers/UserAuthenticatedConsumer.cs
public sealed class UserAuthenticatedConsumer : IConsumer<UserAuthenticatedDomainEvent>
{
    private readonly CartDbContext _context;

    public async Task Consume(ConsumeContext<UserAuthenticatedDomainEvent> context)
    {
        Guid guestBuyerId = context.Message.GuestBuyerId;
        Guid userId = context.Message.UserId;

        if (guestBuyerId == userId) return;  // Already same user

        Cart? guestCart = await _context.Carts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.BuyerId == guestBuyerId);

        Cart? userCart = await _context.Carts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.BuyerId == userId);

        if (guestCart is null) return;

        if (userCart is null)
        {
            // Strategy 1: Transfer ownership (guest cart becomes user cart)
            guestCart.TransferOwnership(userId);
        }
        else
        {
            // Strategy 2: Merge items (combine guest + user carts)
            foreach (CartItem guestItem in guestCart.Items)
            {
                userCart.AddItem(
                    guestItem.ProductId,
                    guestItem.ProductName,
                    guestItem.UnitPrice,
                    guestItem.ImageUrl,
                    guestItem.Quantity);
            }
            _context.Carts.Remove(guestCart);
        }

        await _context.SaveChangesAsync();
    }
}

// In Cart/Domain/Entities/Cart.cs
public void TransferOwnership(Guid newBuyerId)
{
    BuyerId = newBuyerId;
    Touch();  // Reset TTL
}
```

### Pattern 4: Strongly Typed IDs Across Contexts

**What:** Use strongly typed IDs (record wrappers around Guid) for type safety, but store raw Guids for cross-context references.

**When:** Always for entities within a context, but cross-context references use raw Guid for simplicity.

**Example:**
```csharp
// In Reviews/Domain/ValueObjects/ReviewId.cs
public sealed record ReviewId(Guid Value) : StronglyTypedId<Guid>(Value)
{
    public static ReviewId New() => new(Guid.NewGuid());
    public static ReviewId From(Guid value) => new(value);
}

// In Reviews/Domain/Entities/Review.cs
public sealed class Review : BaseAggregateRoot<ReviewId>
{
    // Own ID: strongly typed
    // Cross-context references: raw Guid for simplicity
    public Guid ProductId { get; private set; }
    public Guid UserId { get; private set; }
}
```

**Rationale:** Strongly typed IDs prevent mixing up entity types within a context (e.g., passing ProductId where CategoryId expected). Cross-context references use raw Guid because they're validated via MediatR queries anyway, and Guid is universally understood.

## Anti-Patterns to Avoid

### Anti-Pattern 1: Foreign Keys Across Bounded Contexts

**What goes wrong:** Adding foreign key constraints from ReviewsContext to CatalogContext or ProfilesContext.

**Why bad:** Violates database-per-feature isolation. Prevents independent deployment, schema evolution, and future service extraction. Creates coupling at database level.

**Instead:** Use raw Guid references with validation via MediatR queries in application layer. Accept eventual consistency for denormalized display data.

**Detection:** Look for migrations that create foreign keys referencing tables in different schemas.

### Anti-Pattern 2: Shared DbContext Across Features

**What goes wrong:** Creating a single `AppDbContext` with `DbSet<Product>`, `DbSet<Review>`, `DbSet<UserProfile>`.

**Why bad:** Breaks bounded context boundaries. All contexts share same database transaction scope, migration history, and schema. Prevents gradual service extraction.

**Instead:** One `DbContext` per bounded context with separate schema and migration history. Cross-context queries via MediatR.

**Detection:** Single DbContext with multiple feature entities, shared migration history.

### Anti-Pattern 3: Synchronous Cross-Context Calls in Domain Events

**What goes wrong:** In `ReviewSubmittedDomainEvent` handler, directly call `CatalogDbContext.SaveChangesAsync()`.

**Why bad:** Couples contexts at transaction level. Domain events should be handled asynchronously via message bus (MassTransit).

**Instead:** Publish domain events to MassTransit after `SaveChangesAsync` (via `DomainEventInterceptor`). Consumers in other contexts handle events asynchronously.

**Detection:** Domain event handlers that inject multiple DbContexts or call `SaveChangesAsync` on contexts outside their boundary.

### Anti-Pattern 4: User Profile as Extension of Ordering Context

**What goes wrong:** Adding `UserProfile` entity to `OrderingDbContext` because orders reference users.

**Why bad:** Mixes two subdomains with different lifecycles and responsibilities. User profiles evolve independently from orders. Future identity service extraction becomes harder.

**Instead:** Create separate `ProfilesDbContext`. Link orders to users via optional `UserId` field (nullable, backfilled on first profile creation).

**Detection:** User profile tables in `ordering` schema, or `OrderingDbContext` with `DbSet<UserProfile>`.

### Anti-Pattern 5: Wishlist as Extension of Cart

**What goes wrong:** Adding `IsWishlist` boolean flag to Cart aggregate and reusing cart logic.

**Why bad:** Wishlist and cart have fundamentally different behaviors: wishlist never expires, can't checkout, supports "move to cart" operation. Mixing concerns creates confusing aggregate with dual responsibility.

**Instead:** Separate `Wishlist` aggregate in `WishlistsDbContext` with its own lifecycle and rules. Similar structure but distinct entity.

**Detection:** Cart entity with `IsWishlist` flag, or cart endpoints handling both cart and wishlist operations.

### Anti-Pattern 6: Avatar Blob Content in Database

**What goes wrong:** Storing avatar image bytes in `UserProfile.AvatarBlob` BYTEA column.

**Why bad:** Database bloat, poor scalability, no CDN support, backup overhead.

**Instead:** Azure Blob Storage with URL reference in database (`UserProfile.AvatarUrl`). Leverage existing blob storage integration for product images.

**Detection:** BYTEA or VARBINARY columns for avatar storage, large database size from profile table.

## Scalability Considerations

| Concern | At 100 users | At 10K users | At 1M users |
|---------|--------------|--------------|-------------|
| **User profiles** | PostgreSQL profiles schema | Same (profiles are small, infrequent writes) | Consider read replicas for profile queries |
| **Reviews** | PostgreSQL reviews schema | Add database index on `ProductId` for aggregation queries | Consider materialized view for average ratings, separate read model (CQRS) |
| **Wishlists** | PostgreSQL wishlists schema | Same (wishlist operations are lightweight) | Partition by UserId if tables grow very large |
| **Avatar storage** | Azure Blob Storage with Aspire integration | Add CDN (Azure Front Door) for avatar delivery | Same (blob storage scales automatically) |
| **Guest cart merge** | Synchronous on login | Queue migration via background job if merge takes >500ms | Async migration with callback notification |
| **Cross-context queries** | Direct MediatR queries | Cache ProductExistsQuery results (Redis) | Extract Reviews to separate service with API gateway |

**Critical at 10K users:**
- Add composite index on `reviews.product_id, reviews.status, reviews.created_at` for product detail page queries
- Add index on `wishlists.user_id` for my-wishlist page
- Enable blob storage CDN for avatar delivery

**Critical at 1M users:**
- Extract Reviews to separate service (most cross-context queries, independent scaling need)
- Introduce read model for product review aggregations (average rating, count) — publish to Redis on ReviewSubmitted
- Consider separate authentication service for Profiles (aligns with Keycloak as identity provider)

## Build Order Recommendations

### Phase Sequence

**Phase 1: Profiles Context (Foundation)**
- Rationale: Other features depend on UserId from profiles. Establishes guest-to-user migration pattern that Reviews and Wishlists will use.
- Deliverables: ProfilesDbContext, UserProfile aggregate, avatar upload, address management, UserCreatedDomainEvent, UserAuthenticatedDomainEvent

**Phase 2: Reviews Context**
- Rationale: Requires Profiles for UserId. Most complex cross-context integration (validates ProductId, checks verified purchase, denormalizes user/product names). Tests event-driven architecture thoroughly.
- Deliverables: ReviewsDbContext, Review aggregate, verified purchase validation, rating aggregation, ReviewSubmittedDomainEvent consumers in Catalog

**Phase 3: Wishlists Context**
- Rationale: Simplest of three features. Similar to Cart structure (proven pattern). Depends on Profiles for UserId.
- Deliverables: WishlistsDbContext, Wishlist aggregate, move-to-cart integration with Cart context

**Phase 4: Integration & Polish**
- Rationale: Connect all features via frontend, add navigation between profile/reviews/wishlists, end-to-end testing
- Deliverables: Frontend profile page, review submission UI, wishlist management UI, E2E tests

### Dependency Graph

```
Phase 1: Profiles
  ├─ No dependencies (foundation)
  └─ Establishes: UserId, avatar storage pattern, guest migration pattern

Phase 2: Reviews
  ├─ Depends on: Profiles (UserId)
  ├─ Depends on: Ordering (verified purchase check)
  └─ Depends on: Catalog (ProductId validation)

Phase 3: Wishlists
  ├─ Depends on: Profiles (UserId)
  ├─ Depends on: Catalog (ProductId validation)
  └─ Depends on: Cart (move-to-cart integration)

Phase 4: Integration
  └─ Depends on: Phases 1-3 complete
```

### Critical Path Items

1. **UserAuthenticatedDomainEvent** (Phase 1) — Enables cart merge, order linking
2. **Cross-context MediatR query pattern** (Phase 1) — Used by Reviews and Wishlists for validation
3. **Avatar storage service** (Phase 1) — Reusable pattern for future file uploads
4. **Verified purchase validation** (Phase 2) — Core business rule for review credibility
5. **Denormalized rating aggregation** (Phase 2) — Product detail page performance

## Sources

### Modular Monolith Architecture
- [Refactoring Overgrown Bounded Contexts in Modular Monoliths](https://www.milanjovanovic.tech/blog/refactoring-overgrown-bounded-contexts-in-modular-monoliths) — Milan Jovanović on bounded context boundaries
- [Evolving modular monoliths: Passing data between bounded contexts](https://www.thereformedprogrammer.net/evolving-modular-monoliths-3-passing-data-between-bounded-contexts/) — Cross-context query patterns
- [GitHub: modular-monolith-with-ddd](https://github.com/kgrzybek/modular-monolith-with-ddd) — Reference implementation
- [GitHub: booking-modular-monolith](https://github.com/meysamhadeli/booking-modular-monolith) — .NET 9 with Vertical Slice, CQRS, MassTransit

### Cross-Context Queries & CQRS
- [Modular Monolith: 42% Ditch Microservices in 2026](https://byteiota.com/modular-monolith-42-ditch-microservices-in-2026/) — CNCF survey on architecture consolidation
- [Modern Application Architecture Trends](https://www.cerbos.dev/blog/modern-application-architecture-trends) — Hybrid modular monolith + targeted microservices

### Domain-Driven Design Bounded Contexts
- [Implementing Cart Service with DDD & Hexagonal Architecture](https://medium.com/walmartglobaltech/implementing-cart-service-with-ddd-hexagonal-port-adapter-architecture-part-1-4dab93b3fa9f) — Cart as aggregate root
- [Service boundaries identification example in e-commerce](https://hackernoon.com/service-boundaries-identification-example-in-e-commerce-a2c01a1b8ee9) — Separating Wishlist, Cart, Inventory contexts
- [Bounded Context - DDD in Ruby on Rails](https://www.visuality.pl/posts/bounded-context-ddd-in-ruby-on-rails) — Why wishlist and cart should be separate

### File Upload & Avatar Storage
- [Upload files in ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/mvc/models/file-uploads?view=aspnetcore-10.0) — Microsoft official docs
- [Upload a blob with .NET - Azure Storage](https://learn.microsoft.com/en-us/azure/storage/blobs/storage-blob-upload) — Azure Blob Storage integration
- [A Developer's Guide to Uploading Images: Best Practices](https://medium.com/@digveshparab123/a-developers-guide-to-uploading-images-best-practices-and-methods-2ecb20133928) — Security considerations

### Guest Data Migration
- [WooCommerce Cart Merge & Sessions: Major June 2025 Changes](https://www.businessbloomer.com/woocommerce-cart-merge-sessions-changes/) — `woocommerce_migrate_guest_session_to_user_session` filter pattern
- [How to merge cart items of guest user to logged in user](https://github.com/woocommerce/woocommerce/discussions/49448) — Community discussion on merge strategies
- [Update Cart Customer - Craft Commerce](https://craftcms.com/docs/commerce/3.x/update-cart-customer) — Cart ownership transfer on login

### Keycloak & JWT Claims
- [Integrate Keycloak with ASP.NET Core Using OAuth 2.0](https://www.milanjovanovic.tech/blog/integrate-keycloak-with-aspnetcore-using-oauth-2) — JWT bearer authentication setup
- [How to Access Roles in JWT Token in .NET Core After Keycloak Update](https://www.codegenes.net/blog/can-t-access-roles-in-jwt-token-net-core/) — realm_access and resource_access claims
- [Configure Authentication - Keycloak.AuthServices](https://nikiforovall.blog/keycloak-authorization-services-dotnet/configuration/configuration-authentication.html) — Claims transformation for .NET

### Product Reviews & Verified Purchase
- [Hands-on DDD and Event Sourcing - Domain events](https://falberthen.github.io/posts/ecommerceddd-pt3/) — Event sourcing patterns for e-commerce
- [Review Snippet (AggregateRating) - Google Search](https://developers.google.com/search/docs/appearance/structured-data/review-snippet) — Schema.org aggregateRating structure

---
*Architecture research for v1.1 User Features*
*Researched: 2026-02-13*
