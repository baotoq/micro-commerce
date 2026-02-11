# Phase 12: Product Reviews & Ratings - Research

**Researched:** 2026-02-13
**Domain:** E-commerce product review systems with .NET backend and Next.js frontend
**Confidence:** HIGH

## Summary

Phase 12 implements a product review system following established e-commerce patterns. Users submit star ratings (1-5) and text reviews for purchased products, with verified purchase badges for authenticated buyers. The system enforces one review per user per product and displays aggregate ratings on product pages.

The backend follows the existing vertical slice architecture with a dedicated Reviews feature module using its own ReviewsDbContext. Reviews are linked to both products and users, with composite unique constraints ensuring one review per user-product pair. Aggregate rating calculations occur on write operations (create/update/delete) and are stored on the Product entity for fast reads.

The frontend implements modal-based review submission using shadcn-ui Dialog components following the established pattern from address management. Star rating uses an accessible radio button pattern with visual star rendering. Reviews display in a simple list format with offset-based pagination for initial implementation simplicity.

**Primary recommendation:** Follow the existing architecture patterns. Create Reviews feature with own DbContext, use composite unique index on (UserId, ProductId), denormalize aggregate ratings onto Product entity for performance, use modal dialog for submission, and implement accessible star rating component.

<user_constraints>
## User Constraints (from CONTEXT.md)

### Locked Decisions

#### Review display
- Compact list layout with minimal dividers between reviews, not bordered cards
- Essential info per review: display name, star rating, date, review text, verified purchase badge
- No avatar or helpful count displayed
- Sorted by most recent first (chronological, newest at top)
- Show first 5 reviews with "Load more" button to fetch next batch

#### Star rating presentation
- Aggregate on product detail page: filled star icons + numeric average + review count (e.g., ★★★★☆ 4.2 (47 reviews))
- Product cards on browse/listing pages also show stars + count below product name
- No rating distribution breakdown (no bar chart) — just aggregate
- Classic filled yellow/gold stars with gray empty ones

#### Review submission flow
- "Write a Review" button opens a modal dialog form
- Both star rating and text review required (no rating-only submissions)
- Accessible from product detail page reviews section AND order history (review link per purchased item)
- Button hidden for non-purchasers and unauthenticated users — show "Purchase this product to leave a review" message instead
- No review title/headline field — just star rating + review text

#### Review policies
- Review text: 10–1000 characters
- One review per product per user enforced
- User can edit or delete their own review at any time with no restrictions
- Delete fully removes the review and recalculates aggregate rating
- "Verified Purchase" badge displayed as checkmark icon ✓ plus "Verified Purchase" text

### Claude's Discretion
- Exact modal form layout and styling
- Star input interaction pattern (hover, click)
- Loading states and error handling
- Review text character counter UX
- Empty reviews state message wording

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
| MassTransit | 9.0.0 | Domain events | Already in use for event publishing |

### Frontend Core
| Library | Version | Purpose | Why Standard |
|---------|---------|---------|--------------|
| Next.js | 16.0.3 | React framework | Project standard |
| React | 19.2.0 | UI library | Project standard |
| TypeScript | 5 | Type safety | Project standard |
| TanStack Query | 5.90.20 | Data fetching | Already in use |
| shadcn-ui | Latest | Component library | Already in use (Dialog, Input, etc.) |
| Radix UI | Various | Accessible primitives | Foundation for shadcn-ui |
| Lucide React | 0.563.0 | Icon library | Already in use |

### Supporting Libraries
| Library | Version | Purpose | When to Use |
|---------|---------|---------|-------------|
| None required | N/A | All needs met by existing stack | N/A |

**Installation:**
No new packages required — all dependencies already present in the project.

## Architecture Patterns

### Recommended Project Structure
```
src/MicroCommerce.ApiService/Features/Reviews/
├── ReviewsEndpoints.cs              # Minimal API route mapping
├── Domain/
│   ├── Entities/
│   │   └── Review.cs                # Review aggregate root
│   ├── Events/
│   │   ├── ReviewCreatedDomainEvent.cs
│   │   ├── ReviewUpdatedDomainEvent.cs
│   │   └── ReviewDeletedDomainEvent.cs
│   └── ValueObjects/
│       ├── ReviewId.cs              # Strongly-typed ID
│       ├── Rating.cs                # 1-5 star rating value object
│       └── ReviewText.cs            # Text with 10-1000 char validation
├── Application/
│   ├── Commands/
│   │   ├── CreateReview/
│   │   │   ├── CreateReviewCommand.cs
│   │   │   ├── CreateReviewCommandHandler.cs
│   │   │   └── CreateReviewCommandValidator.cs
│   │   ├── UpdateReview/
│   │   │   ├── UpdateReviewCommand.cs
│   │   │   ├── UpdateReviewCommandHandler.cs
│   │   │   └── UpdateReviewCommandValidator.cs
│   │   └── DeleteReview/
│   │       ├── DeleteReviewCommand.cs
│   │       └── DeleteReviewCommandHandler.cs
│   ├── Queries/
│   │   ├── GetReviewsByProduct/
│   │   │   ├── GetReviewsByProductQuery.cs
│   │   │   ├── GetReviewsByProductQueryHandler.cs
│   │   │   └── ReviewDto.cs
│   │   ├── GetUserReviewForProduct/
│   │   │   ├── GetUserReviewForProductQuery.cs
│   │   │   └── GetUserReviewForProductQueryHandler.cs
│   │   └── CheckUserPurchased/
│   │       ├── CheckUserPurchasedQuery.cs
│   │       └── CheckUserPurchasedQueryHandler.cs
│   └── Consumers/
│       ├── ReviewCreatedConsumer.cs  # Update aggregate on Product
│       ├── ReviewUpdatedConsumer.cs  # Recalculate aggregate
│       └── ReviewDeletedConsumer.cs  # Recalculate aggregate
├── Infrastructure/
│   ├── ReviewsDbContext.cs          # Owned DbContext
│   ├── Configurations/
│   │   └── ReviewConfiguration.cs   # EF Core entity config
│   └── ReviewsDataSeeder.cs         # Dev seed data
└── DependencyInjection.cs           # Feature service registration

src/MicroCommerce.ApiService/Features/Catalog/Domain/Entities/
└── Product.cs                       # ADD: AverageRating, ReviewCount properties

src/MicroCommerce.Web/src/components/reviews/
├── review-list.tsx                  # List of reviews with pagination
├── review-item.tsx                  # Single review display
├── review-form-dialog.tsx           # Modal form for create/edit
├── star-rating-input.tsx            # Accessible star input
├── star-rating-display.tsx          # Read-only star display
└── verified-badge.tsx               # Checkmark + "Verified Purchase"
```

### Pattern 1: Review Aggregate Root with Purchase Verification

**What:** Review entity as aggregate root with verified purchase status determined by Order history lookup.

**When to use:** All review operations — verified purchase is calculated at query time, not stored.

**Example:**
```csharp
// Source: Existing codebase pattern + PostgreSQL unique constraints
public sealed class Review : BaseAggregateRoot<ReviewId>
{
    public Guid ProductId { get; private set; }
    public Guid UserId { get; private set; }
    public Rating Rating { get; private set; }
    public ReviewText Text { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    [Timestamp]
    public uint Version { get; private set; } // xmin optimistic concurrency

    private Review(ReviewId id) : base(id) { }

    public static Review Create(Guid productId, Guid userId, int rating, string text)
    {
        var review = new Review(ReviewId.New())
        {
            ProductId = productId,
            UserId = userId,
            Rating = Rating.Create(rating),
            Text = ReviewText.Create(text),
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        review.AddDomainEvent(new ReviewCreatedDomainEvent(review.Id, productId));
        return review;
    }

    public void Update(int rating, string text)
    {
        Rating = Rating.Create(rating);
        Text = ReviewText.Create(text);
        UpdatedAt = DateTimeOffset.UtcNow;
        AddDomainEvent(new ReviewUpdatedDomainEvent(Id, ProductId));
    }
}
```

### Pattern 2: Composite Unique Constraint for One Review Per User Per Product

**What:** PostgreSQL unique index on (UserId, ProductId) columns to enforce business rule at database level.

**When to use:** Review entity configuration — prevents duplicate reviews even under race conditions.

**Example:**
```csharp
// Source: PostgreSQL unique constraints + existing ProductConfiguration pattern
public class ReviewConfiguration : IEntityTypeConfiguration<Review>
{
    public void Configure(EntityTypeBuilder<Review> builder)
    {
        builder.HasKey(r => r.Id);

        builder.Property(r => r.Id)
            .HasConversion(id => id.Value, value => new ReviewId(value))
            .ValueGeneratedNever();

        // Composite unique index enforces one review per user per product
        builder.HasIndex(r => new { r.UserId, r.ProductId })
            .IsUnique();

        builder.HasIndex(r => r.ProductId); // For listing reviews by product
        builder.HasIndex(r => r.CreatedAt).IsDescending(); // For chronological sort

        // Rating value object
        builder.OwnsOne(r => r.Rating, rating =>
        {
            rating.Property(rt => rt.Value)
                .HasColumnName("Rating")
                .IsRequired();
        });

        // ReviewText value object
        builder.OwnsOne(r => r.Text, text =>
        {
            text.Property(t => t.Value)
                .HasColumnName("ReviewText")
                .HasMaxLength(1000)
                .IsRequired();
        });

        builder.Property(r => r.Version).IsRowVersion();
        builder.Ignore(r => r.DomainEvents);
    }
}
```

### Pattern 3: Denormalized Aggregate Ratings on Product Entity

**What:** Store AverageRating and ReviewCount directly on Product entity, updated via domain event consumers.

**When to use:** Product listing and detail queries — avoids expensive aggregation queries on every page load.

**Example:**
```csharp
// Source: Performance best practice for e-commerce review systems
// Add to Product.cs
public decimal? AverageRating { get; private set; }
public int ReviewCount { get; private set; }

public void UpdateReviewStats(decimal? averageRating, int reviewCount)
{
    AverageRating = averageRating;
    ReviewCount = reviewCount;
}

// ReviewCreatedConsumer.cs
public class ReviewCreatedConsumer : IConsumer<ReviewCreatedDomainEvent>
{
    private readonly CatalogDbContext _catalogContext;
    private readonly ReviewsDbContext _reviewsContext;

    public async Task Consume(ConsumeContext<ReviewCreatedDomainEvent> context)
    {
        // Calculate aggregate from Reviews
        var stats = await _reviewsContext.Reviews
            .Where(r => r.ProductId == context.Message.ProductId)
            .GroupBy(r => r.ProductId)
            .Select(g => new {
                Average = g.Average(r => (decimal)r.Rating.Value),
                Count = g.Count()
            })
            .FirstOrDefaultAsync();

        // Update Product
        var product = await _catalogContext.Products
            .FirstOrDefaultAsync(p => p.Id.Value == context.Message.ProductId);

        if (product != null && stats != null)
        {
            product.UpdateReviewStats(stats.Average, stats.Count);
            await _catalogContext.SaveChangesAsync();
        }
    }
}
```

### Pattern 4: Modal Dialog Form with Controlled State

**What:** shadcn-ui Dialog with controlled open state, form validation, and TanStack Query mutations.

**When to use:** Review submission and editing — follows existing address form dialog pattern.

**Example:**
```tsx
// Source: Existing address-form-dialog.tsx pattern
"use client";

import { useState } from "react";
import { Dialog, DialogContent, DialogHeader, DialogTitle } from "@/components/ui/dialog";
import { Button } from "@/components/ui/button";
import { Textarea } from "@/components/ui/textarea";
import { StarRatingInput } from "@/components/reviews/star-rating-input";
import { useCreateReview, useUpdateReview } from "@/hooks/use-reviews";

interface ReviewFormDialogProps {
  productId: string;
  existingReview?: ReviewDto;
  trigger: React.ReactNode;
}

export function ReviewFormDialog({ productId, existingReview, trigger }: ReviewFormDialogProps) {
  const [open, setOpen] = useState(false);
  const [rating, setRating] = useState(existingReview?.rating || 0);
  const [text, setText] = useState(existingReview?.text || "");
  const [errors, setErrors] = useState<Record<string, string>>({});

  const createReview = useCreateReview();
  const updateReview = useUpdateReview();

  const validateForm = () => {
    const newErrors: Record<string, string> = {};
    if (rating === 0) newErrors.rating = "Please select a rating";
    if (text.length < 10) newErrors.text = "Review must be at least 10 characters";
    if (text.length > 1000) newErrors.text = "Review must not exceed 1000 characters";
    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (!validateForm()) return;

    const mutation = existingReview ? updateReview : createReview;
    mutation.mutate(
      { productId, rating, text },
      {
        onSuccess: () => {
          setOpen(false);
          setErrors({});
        },
      }
    );
  };

  return (
    <Dialog open={open} onOpenChange={setOpen}>
      <div onClick={() => setOpen(true)}>{trigger}</div>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>{existingReview ? "Edit Review" : "Write a Review"}</DialogTitle>
        </DialogHeader>
        <form onSubmit={handleSubmit} className="space-y-4">
          <div className="space-y-2">
            <label className="text-sm font-medium">Rating</label>
            <StarRatingInput value={rating} onChange={setRating} />
            {errors.rating && <p className="text-sm text-red-500">{errors.rating}</p>}
          </div>

          <div className="space-y-2">
            <label className="text-sm font-medium">Your Review</label>
            <Textarea
              value={text}
              onChange={(e) => setText(e.target.value)}
              placeholder="Share your experience with this product..."
              rows={5}
            />
            <div className="flex justify-between text-xs text-zinc-500">
              <span>{text.length}/1000 characters</span>
              {errors.text && <span className="text-red-500">{errors.text}</span>}
            </div>
          </div>

          <Button type="submit" disabled={createReview.isPending || updateReview.isPending}>
            {createReview.isPending || updateReview.isPending ? "Saving..." : "Submit Review"}
          </Button>
        </form>
      </DialogContent>
    </Dialog>
  );
}
```

### Pattern 5: Accessible Star Rating Input with Radio Buttons

**What:** Hidden radio buttons for accessibility with visual star rendering using click/hover states.

**When to use:** Review form star rating input — standard accessible pattern for rating components.

**Example:**
```tsx
// Source: React star rating accessible patterns
"use client";

import { useState } from "react";
import { Star } from "lucide-react";

interface StarRatingInputProps {
  value: number;
  onChange: (rating: number) => void;
  max?: number;
}

export function StarRatingInput({ value, onChange, max = 5 }: StarRatingInputProps) {
  const [hoverValue, setHoverValue] = useState(0);

  return (
    <div className="flex items-center gap-1">
      {Array.from({ length: max }, (_, i) => i + 1).map((star) => {
        const filled = star <= (hoverValue || value);
        return (
          <button
            key={star}
            type="button"
            onClick={() => onChange(star)}
            onMouseEnter={() => setHoverValue(star)}
            onMouseLeave={() => setHoverValue(0)}
            className="transition-colors focus:outline-none focus:ring-2 focus:ring-offset-2"
            aria-label={`Rate ${star} out of ${max} stars`}
          >
            <Star
              className={`size-8 ${filled ? "fill-yellow-400 text-yellow-400" : "text-zinc-300"}`}
            />
          </button>
        );
      })}
      <input
        type="hidden"
        name="rating"
        value={value}
        aria-label={`Rating: ${value} out of ${max} stars`}
      />
    </div>
  );
}
```

### Pattern 6: Verified Purchase Check via Order History

**What:** Query Orders table to verify user purchased product before allowing review submission.

**When to use:** Review eligibility check — determines if "Write a Review" button shows or blocked message displays.

**Example:**
```csharp
// Source: Cross-context query pattern
public sealed record CheckUserPurchasedQuery(Guid UserId, Guid ProductId) : IRequest<bool>;

public sealed class CheckUserPurchasedQueryHandler : IRequestHandler<CheckUserPurchasedQuery, bool>
{
    private readonly OrderingDbContext _orderingContext;

    public async Task<bool> Handle(CheckUserPurchasedQuery request, CancellationToken cancellationToken)
    {
        return await _orderingContext.Orders
            .Where(o => o.BuyerId == request.UserId)
            .Where(o => o.Status == OrderStatus.Delivered ||
                        o.Status == OrderStatus.Confirmed ||
                        o.Status == OrderStatus.Paid) // Allow review after payment
            .SelectMany(o => o.Items)
            .AnyAsync(item => item.ProductId == request.ProductId, cancellationToken);
    }
}
```

### Anti-Patterns to Avoid

- **Storing verified purchase as boolean:** Calculate at query time using Order history instead — source of truth is Orders table
- **Recalculating aggregates on every read:** Denormalize onto Product entity and update via event consumers
- **Cascading deletes from Product to Reviews:** Use soft delete or prevent product deletion if reviews exist
- **Global star rating without unique keys:** Each star needs stable key for React reconciliation
- **Allowing anonymous reviews:** Require authentication to link reviews to users and enable edit/delete

## Don't Hand-Roll

| Problem | Don't Build | Use Instead | Why |
|---------|-------------|-------------|-----|
| Star rating UI | Custom SVG star rendering with complex hover logic | Lucide-react Star icon + simple hover state | Edge cases: keyboard navigation, touch devices, accessibility labels all need careful handling |
| Aggregate calculation | Complex SQL window functions or subqueries | Simple AVG() GROUP BY with denormalized storage | Performance degrades rapidly with large review counts; recalculate on write, not read |
| One-per-user enforcement | Application-level uniqueness check | PostgreSQL composite unique index | Race conditions, distributed deployments, and concurrent requests make app-level checks unreliable |
| Modal dialog accessibility | Custom overlay/focus trap | shadcn-ui Dialog (Radix UI primitive) | Focus management, escape key, click outside, scroll lock, ARIA attributes are complex to implement correctly |
| Review text validation | Manual character counting and regex | FluentValidation with length rules | Edge cases: Unicode characters, emojis, whitespace handling all need consideration |

**Key insight:** Review systems appear simple but have many subtle complexities. Unique constraints prevent duplicate reviews under concurrency, denormalization avoids N+1 queries on product listings, and domain events decouple aggregate updates. Use proven patterns rather than implementing these from scratch.

## Common Pitfalls

### Pitfall 1: N+1 Queries for Aggregate Ratings on Product Listings

**What goes wrong:** Fetching products then calculating AVG(rating) for each product results in N+1 query problem.

**Why it happens:** Natural inclination to keep data normalized and calculate aggregates on-the-fly.

**How to avoid:** Denormalize AverageRating and ReviewCount onto Product entity. Update via domain event consumers when reviews change.

**Warning signs:** Slow product listing page, database query count proportional to number of products displayed.

### Pitfall 2: Duplicate Reviews Under Concurrent Submissions

**What goes wrong:** User double-clicks "Submit Review" button, or multiple tabs submit simultaneously, creating duplicate reviews.

**Why it happens:** Application-level "check if review exists" followed by insert has race condition window.

**How to avoid:** Use PostgreSQL composite unique index on (UserId, ProductId). Database enforces constraint even under concurrency.

**Warning signs:** Duplicate reviews appearing occasionally, especially for popular products or users with fast clicking.

### Pitfall 3: Verified Purchase Status Gets Stale

**What goes wrong:** Storing verified purchase as boolean on review — becomes incorrect if order gets refunded or user data changes.

**Why it happens:** Attempting to denormalize verification status for query performance.

**How to avoid:** Calculate verified purchase at query time by checking Order history. It's authoritative source of truth.

**Warning signs:** Reviews marked verified but user never actually purchased, or legitimate purchases not showing verified badge.

### Pitfall 4: Delete Cascades Destroying Review Data

**What goes wrong:** Product deletion cascades to reviews, losing valuable user-generated content and historical data.

**Why it happens:** Default EF Core cascade delete behavior without considering business impact.

**How to avoid:** Configure OnDelete(DeleteBehavior.Restrict) for Product-to-Review relationship. Require reviews be deleted first, or implement soft delete for products.

**Warning signs:** Accidentally deleted products cause reviews to disappear, breaking user trust and losing SEO value.

### Pitfall 5: Missing Indexes on Review Query Paths

**What goes wrong:** Slow review listing queries, especially sorting by CreatedAt descending (most recent first).

**Why it happens:** Forgetting to add indexes for common query patterns during entity configuration.

**How to avoid:** Add indexes for ProductId (listing reviews), UserId (user's reviews), and descending CreatedAt (chronological sort).

**Warning signs:** Product detail page with many reviews loads slowly, EXPLAIN ANALYZE shows sequential scans.

## Code Examples

Verified patterns from official sources and existing codebase:

### Pagination Query with Offset/Limit

```csharp
// Source: Existing GetProductsQuery pattern
public sealed record GetReviewsByProductQuery(
    Guid ProductId,
    int Page = 1,
    int PageSize = 5
) : IRequest<ReviewListDto>;

public sealed class GetReviewsByProductQueryHandler
    : IRequestHandler<GetReviewsByProductQuery, ReviewListDto>
{
    private readonly ReviewsDbContext _context;

    public async Task<ReviewListDto> Handle(
        GetReviewsByProductQuery request,
        CancellationToken cancellationToken)
    {
        var query = _context.Reviews
            .Where(r => r.ProductId == request.ProductId)
            .OrderByDescending(r => r.CreatedAt); // Most recent first

        var totalCount = await query.CountAsync(cancellationToken);

        var reviews = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(r => new ReviewDto
            {
                Id = r.Id.Value,
                Rating = r.Rating.Value,
                Text = r.Text.Value,
                CreatedAt = r.CreatedAt,
                UserId = r.UserId
            })
            .ToListAsync(cancellationToken);

        return new ReviewListDto(reviews, totalCount, request.Page, request.PageSize);
    }
}
```

### FluentValidation for Review Input

```csharp
// Source: Existing UpdateProfileCommandValidator pattern
public sealed class CreateReviewCommandValidator : AbstractValidator<CreateReviewCommand>
{
    public CreateReviewCommandValidator()
    {
        RuleFor(x => x.Rating)
            .InclusiveBetween(1, 5)
            .WithMessage("Rating must be between 1 and 5 stars.");

        RuleFor(x => x.Text)
            .NotEmpty()
            .WithMessage("Review text is required.")
            .MinimumLength(10)
            .WithMessage("Review text must be at least 10 characters.")
            .MaximumLength(1000)
            .WithMessage("Review text must not exceed 1000 characters.");
    }
}
```

### Star Rating Display Component

```tsx
// Source: Lucide-react icons + aggregate rating patterns
import { Star } from "lucide-react";

interface StarRatingDisplayProps {
  rating: number; // e.g., 4.2
  count?: number;
  showCount?: boolean;
}

export function StarRatingDisplay({ rating, count, showCount = true }: StarRatingDisplayProps) {
  const fullStars = Math.floor(rating);
  const hasHalfStar = rating % 1 >= 0.5;

  return (
    <div className="flex items-center gap-1.5">
      <div className="flex items-center">
        {Array.from({ length: 5 }, (_, i) => {
          const filled = i < fullStars;
          const half = i === fullStars && hasHalfStar;

          return (
            <Star
              key={i}
              className={`size-4 ${
                filled ? "fill-yellow-400 text-yellow-400" :
                half ? "fill-yellow-200 text-yellow-400" :
                "text-zinc-300"
              }`}
            />
          );
        })}
      </div>
      {showCount && count !== undefined && (
        <span className="text-sm text-zinc-600">
          {rating.toFixed(1)} ({count} {count === 1 ? "review" : "reviews"})
        </span>
      )}
    </div>
  );
}
```

### Review Item Component

```tsx
// Source: User constraints + existing component patterns
import { CheckCircle } from "lucide-react";
import { StarRatingDisplay } from "./star-rating-display";

interface ReviewItemProps {
  review: ReviewDto;
  isVerifiedPurchase: boolean;
  isOwner: boolean;
  onEdit?: () => void;
  onDelete?: () => void;
}

export function ReviewItem({ review, isVerifiedPurchase, isOwner, onEdit, onDelete }: ReviewItemProps) {
  return (
    <div className="border-b border-zinc-200 py-4 first:pt-0 last:border-0">
      <div className="flex items-start justify-between">
        <div className="flex-1">
          <div className="flex items-center gap-2">
            <span className="font-medium text-zinc-900">{review.displayName}</span>
            {isVerifiedPurchase && (
              <div className="flex items-center gap-1 text-xs text-emerald-600">
                <CheckCircle className="size-3.5" />
                <span>Verified Purchase</span>
              </div>
            )}
          </div>

          <div className="mt-1 flex items-center gap-2">
            <StarRatingDisplay rating={review.rating} showCount={false} />
            <span className="text-xs text-zinc-500">
              {new Date(review.createdAt).toLocaleDateString()}
            </span>
          </div>

          <p className="mt-2 whitespace-pre-line text-sm text-zinc-700">{review.text}</p>
        </div>

        {isOwner && (
          <div className="ml-4 flex gap-2">
            <button onClick={onEdit} className="text-xs text-blue-600 hover:underline">
              Edit
            </button>
            <button onClick={onDelete} className="text-xs text-red-600 hover:underline">
              Delete
            </button>
          </div>
        )}
      </div>
    </div>
  );
}
```

## State of the Art

| Old Approach | Current Approach | When Changed | Impact |
|--------------|------------------|--------------|--------|
| Calculate aggregates on read | Denormalize to Product entity | Long-established pattern | Dramatically improves product listing performance |
| Application-level uniqueness | Database composite unique index | PostgreSQL adoption | Prevents race conditions in concurrent environments |
| Custom modal implementations | Radix UI primitives via shadcn-ui | ~2023 | Better accessibility, focus management, reduced custom code |
| Cursor pagination everywhere | Offset for simple lists, cursor for feeds | Context-dependent | Offset acceptable for reviews (static after load), cursor better for infinite scroll |
| Review with separate rating entity | Rating as value object | DDD adoption | Simpler model, no orphaned ratings |

**Deprecated/outdated:**
- **jQuery star rating plugins:** Modern React component patterns with hooks are standard
- **Storing stars as strings ("★★★★☆"):** Store numeric rating, render stars in UI layer
- **Separate AggregateRating table:** Denormalize directly onto Product entity for performance
- **AJAX-only pagination:** Server Components + RSC payload more efficient in Next.js 13+

## Open Questions

1. **Should reviews support images/photos?**
   - What we know: User constraints don't mention image uploads
   - What's unclear: Future enhancement possibility
   - Recommendation: Out of scope for Phase 12, but design schema to allow future addition (add ImageUrls JSON column or separate ReviewImage table later)

2. **How to handle user deletion and review anonymization?**
   - What we know: GDPR and data privacy may require user data deletion
   - What's unclear: Should reviews be deleted or anonymized (show "Anonymous User")
   - Recommendation: Initially restrict user deletion if reviews exist. Phase 13+ can add anonymization strategy

3. **Should aggregate ratings update immediately or eventual consistency?**
   - What we know: Domain events via MassTransit enable eventual consistency
   - What's unclear: User expectation for instant feedback vs acceptable delay
   - Recommendation: Use synchronous update for now (await recalculation in consumer), optimize to async if needed

4. **Review edit history — track changes or just UpdatedAt?**
   - What we know: User can edit reviews "at any time with no restrictions"
   - What's unclear: Should edit history be preserved for moderation or transparency
   - Recommendation: Simple UpdatedAt timestamp only. Phase 13+ can add audit trail if needed

## Sources

### Primary (HIGH confidence)
- Existing codebase architecture patterns: `/Users/baotoq/Work/micro-commerce/src/MicroCommerce.ApiService/Features/{Profiles,Catalog,Ordering}/`
- PostgreSQL documentation: [Constraints](https://www.postgresql.org/docs/current/ddl-constraints.html)
- EF Core documentation: Existing entity configurations showing value object patterns, unique indexes, owned entities
- React component patterns: Existing address-form-dialog.tsx, product-detail.tsx, account components

### Secondary (MEDIUM confidence)
- [Schema.org AggregateRating](https://schema.org/AggregateRating) - Standard for rating representation
- [PostgreSQL Unique Constraints Guide](https://www.dbvis.com/thetable/all-you-need-to-know-about-postgresql-unique-constraint/) - Composite key patterns
- [shadcn/ui Dialog Documentation](https://ui.shadcn.com/docs/components/radix/dialog) - Modal component API
- [Material-UI React Rating](https://mui.com/material-ui/react-rating/) - Accessible rating patterns
- [Offset vs Cursor Pagination Deep Dive](https://www.milanjovanovic.tech/blog/understanding-cursor-pagination-and-why-its-so-fast-deep-dive) - Pagination strategy comparison
- [Bazaarvoice Verified Purchaser](https://developers.bazaarvoice.com/v1.0-ConversationsAPI/docs/verified-purchaser) - Verified purchase badge patterns
- [PowerReviews Badges Definitions](https://www.powerreviews.com/badge-definitions/) - Industry standard badge implementations

### Tertiary (LOW confidence)
- Web search results on general e-commerce review trends — informational context only

## Metadata

**Confidence breakdown:**
- Standard stack: HIGH - All libraries already in project, versions confirmed via package files
- Architecture: HIGH - Patterns directly from existing codebase (Profiles, Ordering, Catalog features)
- Pitfalls: MEDIUM-HIGH - Mix of database best practices (HIGH) and e-commerce domain patterns (MEDIUM)
- Frontend patterns: HIGH - Directly from existing address form dialog and product components

**Research date:** 2026-02-13
**Valid until:** 2026-03-15 (30 days for stable patterns, tech stack unlikely to change)

**Cross-module dependencies:**
- Catalog: Add AverageRating and ReviewCount to Product entity
- Ordering: Query Orders to verify purchase history
- Profiles: Fetch DisplayName for review display
- Common: Reuse ValidationBehavior, DomainEventInterceptor, BaseAggregateRoot

**Database considerations:**
- New schema: `reviews` (follows pattern from `profiles`, `catalog`, `ordering`)
- New DbContext: `ReviewsDbContext` with own connection/migration history
- Composite unique index: `IX_Reviews_UserId_ProductId` for one-per-user enforcement
- Descending index: `IX_Reviews_CreatedAt_Desc` for chronological sorting
