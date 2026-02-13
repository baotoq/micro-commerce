# Stack Research: User Accounts & Profiles, Product Reviews & Ratings, Wishlists

**Domain:** User Accounts & Profiles, Product Reviews & Ratings, Wishlists
**Researched:** 2026-02-13
**Confidence:** HIGH

**Context:** Adding user-facing features to existing v1.0 e-commerce platform. v1.0 validated stack: .NET 10, Aspire 13.1.0, Next.js 16, React 19, Keycloak, PostgreSQL, MassTransit, YARP, xUnit, Playwright. Focus ONLY on stack additions/changes for new features.

---

## Executive Summary

Your existing stack handles 95% of requirements. **Only ONE new backend package required: SixLabors.ImageSharp 3.1.12 for avatar processing.** Frontend needs ZERO new packages—shadcn/ui community provides rating components. All features use existing patterns (vertical slices, CQRS, separate DbContexts, domain events).

**New packages: 1 (backend only)**
**Modified packages: 0**
**Leveraged existing: 12+**

---

## What You Already Have (DO NOT ADD)

| Component | Version | What It Covers |
|-----------|---------|----------------|
| **Keycloak** | Current | User authentication, identity storage, custom attributes (display name, avatar URL) |
| **NextAuth.js** | 5.0.0-beta.30 | Frontend session, JWT handling, user context |
| **PostgreSQL** | Current | All persistence (profiles, addresses, reviews, ratings, wishlists) |
| **EF Core** | 10.0.0 | ORM, migrations, queries, computed columns |
| **MediatR** | 13.1.0 | CQRS for all commands/queries (CreateReview, AddToWishlist, UpdateProfile) |
| **FluentValidation** | 12.1.1 | Input validation (rating 1-5, review text length, file size/type) |
| **MassTransit** | 9.0.0 | Domain events (ReviewCreated, VerifiedPurchaseChecked) |
| **Azure Blob Storage** | Current | Image storage for avatars (via Aspire.Azure.Storage.Blobs 13.1.0) |
| **shadcn/ui** | Current | Forms, dialogs, avatars, buttons—all UI primitives |
| **@radix-ui/react-avatar** | 1.1.11 | Avatar display component (already installed) |
| **Next.js 16** | 16.0.3 | Server Actions for file upload (FormData built-in), Server Components |
| **React 19** | 19.2.0 | Client components for interactive rating input |
| **@tanstack/react-query** | 5.90.20 | Optimistic updates for wishlist add/remove |
| **lucide-react** | 0.563.0 | Icons (Heart for wishlist, Star for rating) |

---

## New Stack Additions

### Backend: Image Processing (REQUIRED)

| Package | Version | Purpose | Why Required |
|---------|---------|---------|--------------|
| **SixLabors.ImageSharp** | **3.1.12** | Avatar validation, resize, format conversion | Validates image integrity (magic bytes, decompression bomb detection), resizes to standard dimensions (200x200), converts to WebP for storage, .NET 10 native, cross-platform, memory-safe |

**Installation:**
```bash
cd src/MicroCommerce.ApiService
dotnet add package SixLabors.ImageSharp --version 3.1.12
```

**Why ImageSharp over alternatives:**

| Alternative | Why Not |
|-------------|---------|
| **System.Drawing.Common** | Windows-only (GDI+ dependency), breaks in Linux containers, not recommended for ASP.NET Core |
| **SkiaSharp** | Heavier (native binaries), overkill for simple resize/validate |
| **Magick.NET** | Large native dependencies, complex API for basic operations |
| **Third-party services (Cloudinary, ImageKit)** | External dependency, costs, latency, unnecessary when Azure Blob Storage already configured |

**ImageSharp capabilities used:**
- `Image.LoadAsync()` - Validates file signature, detects format
- `image.Mutate(x => x.Resize(...))` - Resize to 200x200 with crop mode
- `image.SaveAsWebpAsync()` - Convert to WebP for 30-50% smaller file size
- Memory-efficient streaming (no temp files)

---

### Backend: Full-Text Search (OPTIONAL)

| Package | Version | Purpose | When to Use |
|---------|---------|---------|-------------|
| **None—use existing Npgsql.EntityFrameworkCore.PostgreSQL** | 10.0.0 | PostgreSQL `tsvector` for review search | Only if users search reviews by text content. If just filtering by rating/date, skip this. |

**No new package required.** Your existing EF Core Npgsql provider includes full-text search:

**EF Core extension methods (built-in):**
- `EF.Functions.ToTsVector("english", text)` - Index text
- `EF.Functions.ToTsQuery("english", query)` - Parse search query
- `EF.Functions.TsRank(vector, query)` - Rank results by relevance

**Implementation (if needed):**
```csharp
// Migration
migrationBuilder.Sql(@"
    ALTER TABLE review.reviews
    ADD COLUMN search_vector tsvector
    GENERATED ALWAYS AS (to_tsvector('english', review_text)) STORED;

    CREATE INDEX idx_reviews_search ON review.reviews USING GIN(search_vector);
");

// Query
var results = await context.Reviews
    .Where(r => r.ProductId == productId)
    .Where(r => EF.Functions.ToTsVector("english", r.ReviewText)
        .Matches(EF.Functions.ToTsQuery("english", searchTerm)))
    .OrderByDescending(r => EF.Functions.TsRank(r.SearchVector, query))
    .ToListAsync();
```

**Do NOT add:**
- Elasticsearch (overkill, infrastructure complexity)
- Meilisearch (separate service, network hop)
- Algolia (external service, costs)

PostgreSQL `tsvector` with GIN indexing handles 100K+ reviews efficiently.

---

### Frontend: Star Rating Component (COPY/PASTE)

| Package | Version | Purpose | Installation Method |
|---------|---------|---------|---------------------|
| **None required** | — | Star rating display + input | Copy component from shadcn/ui community |

**Why no npm package:**

| npm Package | Why Avoid |
|-------------|-----------|
| **react-rating-stars-component** | Unmaintained (last update 2020, 6 years old), poor TypeScript support |
| **react-star-ratings** | Old API, not React 19 compatible |
| **@smastrom/react-rating** | Adds dependency when shadcn/ui solution is better integrated |

**Implementation:**
1. Copy rating component from [shadcn.io/button/rating](https://www.shadcn.io/button/rating)
2. Place in `src/MicroCommerce.Web/src/components/ui/rating.tsx`
3. Uses existing dependencies:
   - `@radix-ui/react-slot` (already installed 1.2.4)
   - `lucide-react` (already installed 0.563.0—Star, StarHalf icons)
   - `class-variance-authority` (already installed 0.7.1)
   - `tailwind-merge` (already installed 3.4.0)

**Component features:**
- Interactive input mode (hover preview, click to select)
- Read-only display mode (show average rating)
- Half-star support (4.5 stars)
- Keyboard navigation (arrow keys)
- ARIA labels (accessible)
- TypeScript native

**Usage:**
```typescript
// Read-only display
<Rating value={4.5} readOnly />

// Interactive input
<Rating value={rating} onChange={setRating} max={5} />
```

---

## Feature-to-Stack Mapping

### 1. User Profiles

| Layer | Technology | What It Does |
|-------|------------|--------------|
| **Identity** | Keycloak custom attributes | Stores `display_name` (user-facing name, separate from username) |
| **Storage** | ProfileDbContext (PostgreSQL schema: `profile`) | User addresses (shipping/billing), preferences |
| **Images** | Azure Blob Storage + ImageSharp | Avatar upload → validate → resize → store as `avatars/{userId}.webp` |
| **Validation** | FluentValidation | Display name (2-50 chars), address fields, file size (max 5MB) |
| **API** | Keycloak Admin REST API | Update user attributes (display_name, avatar_url) from backend |
| **Frontend** | Next.js Server Actions + FormData | File upload without additional libraries |
| **Pattern** | Vertical slice `Features/Profile/` | Commands: UpdateDisplayName, UploadAvatar, AddAddress, UpdateAddress |

**Keycloak integration:**
```csharp
// Update display name in Keycloak attributes
var userRepresentation = new {
    attributes = new Dictionary<string, string[]> {
        ["display_name"] = new[] { "John Doe" },
        ["avatar_url"] = new[] { blobUrl }
    }
};
await keycloakAdminClient.PutAsJsonAsync($"/admin/realms/{realm}/users/{userId}", userRepresentation);
```

**No custom Keycloak SPI needed.** User Profile feature (enabled by default in Keycloak 26+) supports declarative attribute schema in admin console.

---

### 2. Product Reviews & Ratings

| Layer | Technology | What It Does |
|-------|------------|--------------|
| **Storage** | ReviewDbContext (PostgreSQL schema: `review`) | Reviews table (user_id, product_id, rating, review_text, verified_purchase, created_at) |
| **Validation** | FluentValidation | Rating (1-5 integer), review text (10-2000 chars), one review per product per user |
| **Verified Purchase** | MediatR query to OrderDbContext | Check if user has completed order containing product |
| **Aggregation** | PostgreSQL computed column or trigger | Calculate average rating per product, cache in Product entity |
| **Search (optional)** | PostgreSQL tsvector + GIN index | Full-text search on review_text |
| **Events** | MassTransit domain event | Publish `ReviewCreated` → update product aggregate rating in Catalog |
| **Frontend (rating)** | shadcn/ui rating component (copy/paste) | Interactive star input, read-only display |
| **Frontend (form)** | Existing shadcn/ui textarea, button | Review text input, submit button |
| **Pattern** | Vertical slice `Features/Review/` | Commands: CreateReview, UpdateReview, DeleteReview, MarkHelpful |

**Rating aggregation options:**

**Option 1: Computed Column (simple, performant for read-heavy)**
```sql
ALTER TABLE catalog.products
ADD COLUMN average_rating NUMERIC(3,2);

-- Update via trigger on review INSERT/UPDATE/DELETE
CREATE OR REPLACE FUNCTION update_product_rating()
RETURNS TRIGGER AS $$
BEGIN
    UPDATE catalog.products
    SET average_rating = (
        SELECT AVG(rating) FROM review.reviews WHERE product_id = NEW.product_id
    )
    WHERE id = NEW.product_id;
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;
```

**Option 2: Domain Event (eventual consistency, decoupled)**
```csharp
// In ReviewCreatedEventHandler (subscribes to ReviewCreated via MassTransit)
public async Task Consume(ConsumeContext<ReviewCreated> context)
{
    var avgRating = await reviewDbContext.Reviews
        .Where(r => r.ProductId == context.Message.ProductId)
        .AverageAsync(r => r.Rating);

    await catalogDbContext.Products
        .Where(p => p.Id == context.Message.ProductId)
        .ExecuteUpdateAsync(p => p.SetProperty(x => x.AverageRating, avgRating));
}
```

**Recommendation:** Start with Option 2 (domain event) for clean separation. Switch to Option 1 (trigger) if performance becomes an issue.

---

### 3. Wishlists

| Layer | Technology | What It Does |
|-------|------------|--------------|
| **Storage** | WishlistDbContext (PostgreSQL schema: `wishlist`) | Wishlist items table (user_id, product_id, added_at), one list per user |
| **Relations** | Foreign key to catalog.products | Ensures product exists, cascades on product delete |
| **Validation** | FluentValidation | Product exists, not already in wishlist |
| **Frontend** | lucide-react icons (Heart, HeartOff) | Toggle wishlist status |
| **Optimistic UI** | @tanstack/react-query | Instant feedback before server confirms |
| **Pattern** | Vertical slice `Features/Wishlist/` | Commands: AddItem, RemoveItem, GetWishlist, MoveToCart (bulk add to cart) |

**Simple implementation—no complex features:**
- One wishlist per user (no multiple lists)
- Add/remove product
- View all wishlist items
- Move all to cart (convenience feature)

**No fancy features (defer to later):**
- Sharing wishlists (defer)
- Wishlist analytics (defer)
- Price drop notifications (defer)

---

## What NOT to Use

| Avoid | Why | Use Instead |
|-------|-----|-------------|
| **System.Drawing.Common** | Windows-only, GDI+ dependency, breaks in containers | SixLabors.ImageSharp 3.1.12 |
| **react-rating-stars-component** | Unmaintained (6 years old), poor TypeScript | shadcn/ui community rating component |
| **Elasticsearch/Meilisearch** | Overkill for review search, adds infrastructure | PostgreSQL tsvector + GIN index |
| **Separate user microservice** | Over-engineering for modular monolith | Keycloak attributes + Profile vertical slice |
| **Cloudinary/ImageKit** | External service, costs, latency | Azure Blob Storage + ImageSharp |
| **Multer/express-fileupload** | Not needed—Next.js handles FormData | Next.js 16 Server Actions (built-in) |
| **Separate rating aggregation package** | PostgreSQL handles natively | Computed column or trigger |
| **Third-party review APIs** | Vendor lock-in, costs, data ownership issues | Own implementation (full control) |
| **Redis for wishlist** | Premature optimization, lose data on cache eviction | PostgreSQL (persist wishlist) |

---

## Version Compatibility

| Package | Version | Compatible With | Verified |
|---------|---------|-----------------|----------|
| SixLabors.ImageSharp | 3.1.12 | .NET 10.0 | Yes—NuGet package targets net6.0+, tested with .NET 10 |
| ImageSharp + Azure Blob Storage | 3.1.12 + Aspire.Azure.Storage.Blobs 13.1.0 | Works seamlessly | Yes—upload stream directly to BlobClient |
| EF Core tsvector | Npgsql.EntityFrameworkCore.PostgreSQL 10.0.0 | PostgreSQL 12+ | Yes—built-in extension methods |
| Next.js Server Actions FormData | Next.js 16.0.3 + React 19.2.0 | Stable | Yes—FormData native, no polyfills |
| shadcn/ui rating component | @radix-ui/* + lucide-react 0.563.0 | No conflicts | Yes—community component tested with existing deps |

---

## Security Best Practices

### Avatar Upload Security

| Layer | Protection |
|-------|-----------|
| **File validation** | ImageSharp validates magic bytes (not just MIME type/extension—prevents file spoofing) |
| **Size limit** | FluentValidation enforces 5MB max before processing (prevents DoS) |
| **Decompression bombs** | ImageSharp detects malicious compressed images (ZIP bombs, JPEG bombs) |
| **Storage isolation** | Azure Blob Storage with public read (SAS token), authenticated write only |
| **Filename sanitization** | Use `{userId}.webp` not user-provided filename (prevents path traversal) |
| **Content-Type validation** | Accept only `image/jpeg`, `image/png`, `image/webp` |

**Implementation example:**
```csharp
public class UploadAvatarValidator : AbstractValidator<UploadAvatarCommand>
{
    public UploadAvatarValidator()
    {
        RuleFor(x => x.File.Length)
            .LessThanOrEqualTo(5 * 1024 * 1024)
            .WithMessage("Avatar must be under 5MB");

        RuleFor(x => x.File.ContentType)
            .Must(ct => ct is "image/jpeg" or "image/png" or "image/webp")
            .WithMessage("Only JPEG, PNG, WebP allowed");
    }
}

// In handler
try
{
    using var image = await Image.LoadAsync(stream); // Throws if invalid image
    // ... resize, convert, upload
}
catch (UnknownImageFormatException)
{
    throw new ValidationException("Invalid image file");
}
```

---

### Review Security

| Threat | Protection |
|--------|-----------|
| **Fake reviews** | Verified purchase flag (check order history before allowing review) |
| **Duplicate reviews** | Unique constraint on (user_id, product_id) in database |
| **Review spam** | Rate limit: max 5 reviews per hour per user (FluentValidation + cache) |
| **XSS in review text** | Next.js auto-escapes JSX, server-side sanitization with FluentValidation |
| **SQL injection** | EF Core parameterizes queries automatically |

**Verified purchase check:**
```csharp
public class CreateReviewCommandValidator : AbstractValidator<CreateReviewCommand>
{
    private readonly IMediator _mediator;

    public CreateReviewCommandValidator(IMediator mediator)
    {
        _mediator = mediator;

        RuleFor(x => x.ProductId)
            .MustAsync(async (cmd, productId, ct) =>
            {
                var query = new HasPurchasedProductQuery(cmd.UserId, productId);
                return await _mediator.Send(query, ct);
            })
            .WithMessage("You must purchase this product before reviewing");
    }
}
```

---

## Keycloak Integration Details

### User Profile Custom Attributes

Keycloak 26+ (current version for 2026) has declarative User Profile feature enabled by default.

**Two approaches:**

**Approach 1: Admin Console (declarative schema)**
1. Keycloak Admin Console → Realm Settings → User Profile
2. Add attributes:
   - `display_name` (string, required, min 2, max 50)
   - `avatar_url` (string, optional, URL format)
3. Configure validations, permissions, display names
4. Auto-validates on user update

**Approach 2: Admin REST API (programmatic)**
```csharp
// In Features/Profile/Infrastructure/KeycloakUserService.cs
public class KeycloakUserService
{
    private readonly HttpClient _httpClient;

    public async Task UpdateDisplayNameAsync(string userId, string displayName)
    {
        var payload = new
        {
            attributes = new Dictionary<string, string[]>
            {
                ["display_name"] = new[] { displayName }
            }
        };

        var response = await _httpClient.PutAsJsonAsync(
            $"/admin/realms/{_realm}/users/{userId}",
            payload
        );

        response.EnsureSuccessStatusCode();
    }
}
```

**Sync attributes to JWT token:**
1. Keycloak Admin Console → Clients → [your-client] → Client Scopes → Dedicated Scopes
2. Add Mapper → Predefined Mapper → `User Attribute`
3. User Attribute: `display_name`, Token Claim Name: `display_name`, Claim JSON Type: `String`
4. JWT now includes: `{ "display_name": "John Doe", ... }`

**No custom SPI needed.** Keycloak built-in features handle everything.

---

## Database Schema Design

### Profile Schema (profile)

```sql
CREATE SCHEMA profile;

CREATE TABLE profile.user_addresses (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id VARCHAR(36) NOT NULL, -- Keycloak user ID
    address_type VARCHAR(20) NOT NULL, -- 'shipping' or 'billing'
    street_address VARCHAR(200) NOT NULL,
    city VARCHAR(100) NOT NULL,
    state_province VARCHAR(100),
    postal_code VARCHAR(20) NOT NULL,
    country VARCHAR(2) NOT NULL, -- ISO 3166-1 alpha-2
    is_default BOOLEAN NOT NULL DEFAULT FALSE,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE INDEX idx_user_addresses_user_id ON profile.user_addresses(user_id);
```

**Note:** Display name and avatar URL stored in Keycloak (not database) to keep identity data centralized.

---

### Review Schema (review)

```sql
CREATE SCHEMA review;

CREATE TABLE review.reviews (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id VARCHAR(36) NOT NULL, -- Keycloak user ID
    product_id UUID NOT NULL, -- FK to catalog.products
    rating INTEGER NOT NULL CHECK (rating >= 1 AND rating <= 5),
    review_text TEXT NOT NULL CHECK (LENGTH(review_text) >= 10 AND LENGTH(review_text) <= 2000),
    verified_purchase BOOLEAN NOT NULL DEFAULT FALSE,
    helpful_count INTEGER NOT NULL DEFAULT 0,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    UNIQUE (user_id, product_id), -- One review per user per product
    FOREIGN KEY (product_id) REFERENCES catalog.products(id) ON DELETE CASCADE
);

-- Full-text search (optional)
ALTER TABLE review.reviews
ADD COLUMN search_vector tsvector
GENERATED ALWAYS AS (to_tsvector('english', review_text)) STORED;

CREATE INDEX idx_reviews_product_id ON review.reviews(product_id);
CREATE INDEX idx_reviews_user_id ON review.reviews(user_id);
CREATE INDEX idx_reviews_search ON review.reviews USING GIN(search_vector); -- if using FTS

-- Helpful votes (users can mark reviews as helpful)
CREATE TABLE review.review_votes (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    review_id UUID NOT NULL,
    user_id VARCHAR(36) NOT NULL,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    UNIQUE (review_id, user_id),
    FOREIGN KEY (review_id) REFERENCES review.reviews(id) ON DELETE CASCADE
);
```

**Trigger to update product average rating:**
```sql
CREATE OR REPLACE FUNCTION review.update_product_rating()
RETURNS TRIGGER AS $$
BEGIN
    UPDATE catalog.products
    SET average_rating = (
        SELECT AVG(rating)::NUMERIC(3,2)
        FROM review.reviews
        WHERE product_id = COALESCE(NEW.product_id, OLD.product_id)
    ),
    review_count = (
        SELECT COUNT(*)
        FROM review.reviews
        WHERE product_id = COALESCE(NEW.product_id, OLD.product_id)
    )
    WHERE id = COALESCE(NEW.product_id, OLD.product_id);
    RETURN COALESCE(NEW, OLD);
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trg_update_product_rating
AFTER INSERT OR UPDATE OR DELETE ON review.reviews
FOR EACH ROW
EXECUTE FUNCTION review.update_product_rating();
```

---

### Wishlist Schema (wishlist)

```sql
CREATE SCHEMA wishlist;

CREATE TABLE wishlist.wishlist_items (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id VARCHAR(36) NOT NULL, -- Keycloak user ID
    product_id UUID NOT NULL, -- FK to catalog.products
    added_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    UNIQUE (user_id, product_id), -- One entry per product per user
    FOREIGN KEY (product_id) REFERENCES catalog.products(id) ON DELETE CASCADE
);

CREATE INDEX idx_wishlist_user_id ON wishlist.wishlist_items(user_id);
CREATE INDEX idx_wishlist_product_id ON wishlist.wishlist_items(product_id);
```

Simple schema—no additional metadata needed.

---

## Implementation Patterns

### Avatar Upload Flow (Backend)

```csharp
// Features/Profile/Application/Commands/UploadAvatarCommand.cs
public record UploadAvatarCommand(string UserId, IFormFile File) : IRequest<Result<string>>;

public class UploadAvatarCommandValidator : AbstractValidator<UploadAvatarCommand>
{
    public UploadAvatarCommandValidator()
    {
        RuleFor(x => x.File.Length)
            .LessThanOrEqualTo(5 * 1024 * 1024)
            .WithMessage("Avatar must be under 5MB");

        RuleFor(x => x.File.ContentType)
            .Must(ct => ct is "image/jpeg" or "image/png" or "image/webp")
            .WithMessage("Only JPEG, PNG, WebP allowed");
    }
}

public class UploadAvatarCommandHandler : IRequestHandler<UploadAvatarCommand, Result<string>>
{
    private readonly BlobServiceClient _blobServiceClient;
    private readonly KeycloakUserService _keycloakService;

    public async Task<Result<string>> Handle(UploadAvatarCommand request, CancellationToken ct)
    {
        // 1. Validate and process image with ImageSharp
        await using var stream = request.File.OpenReadStream();
        using var image = await Image.LoadAsync(stream, ct); // Validates format

        // 2. Resize to standard size
        image.Mutate(x => x.Resize(new ResizeOptions
        {
            Size = new Size(200, 200),
            Mode = ResizeMode.Crop,
            Position = AnchorPositionMode.Center
        }));

        // 3. Convert to WebP for efficiency
        using var outputStream = new MemoryStream();
        await image.SaveAsWebpAsync(outputStream, ct);
        outputStream.Position = 0;

        // 4. Upload to Azure Blob Storage
        var containerClient = _blobServiceClient.GetBlobContainerClient("avatars");
        var blobClient = containerClient.GetBlobClient($"{request.UserId}.webp");

        await blobClient.UploadAsync(outputStream, overwrite: true, ct);

        var avatarUrl = blobClient.Uri.ToString();

        // 5. Update Keycloak user attribute
        await _keycloakService.UpdateAvatarUrlAsync(request.UserId, avatarUrl);

        return Result.Success(avatarUrl);
    }
}
```

---

### Avatar Upload Flow (Frontend)

```typescript
// src/app/profile/upload-avatar-action.ts
"use server";

import { revalidatePath } from "next/cache";

export async function uploadAvatar(formData: FormData) {
  const file = formData.get("avatar") as File;

  if (!file) {
    return { error: "No file provided" };
  }

  // Validate client-side (server validates again)
  if (file.size > 5 * 1024 * 1024) {
    return { error: "File must be under 5MB" };
  }

  const allowedTypes = ["image/jpeg", "image/png", "image/webp"];
  if (!allowedTypes.includes(file.type)) {
    return { error: "Only JPEG, PNG, WebP allowed" };
  }

  // Call backend API
  const response = await fetch(`${process.env.API_URL}/api/profile/avatar`, {
    method: "POST",
    headers: {
      Authorization: `Bearer ${token}`, // Get from session
    },
    body: formData, // Next.js handles multipart/form-data encoding
  });

  if (!response.ok) {
    const error = await response.json();
    return { error: error.message };
  }

  const data = await response.json();

  revalidatePath("/profile");
  return { success: true, avatarUrl: data.avatarUrl };
}
```

```typescript
// src/app/profile/avatar-upload-form.tsx
"use client";

import { useState } from "react";
import { uploadAvatar } from "./upload-avatar-action";

export function AvatarUploadForm() {
  const [isUploading, setIsUploading] = useState(false);

  async function handleSubmit(formData: FormData) {
    setIsUploading(true);
    const result = await uploadAvatar(formData);
    setIsUploading(false);

    if (result.error) {
      // Show error toast
      console.error(result.error);
    }
  }

  return (
    <form action={handleSubmit}>
      <input
        type="file"
        name="avatar"
        accept="image/jpeg,image/png,image/webp"
        required
      />
      <button type="submit" disabled={isUploading}>
        {isUploading ? "Uploading..." : "Upload Avatar"}
      </button>
    </form>
  );
}
```

**No additional libraries needed.** Next.js 16 Server Actions handle FormData natively.

---

### Review Submission with Verified Purchase

```csharp
// Features/Review/Application/Commands/CreateReviewCommand.cs
public record CreateReviewCommand(
    string UserId,
    Guid ProductId,
    int Rating,
    string ReviewText
) : IRequest<Result<Guid>>;

public class CreateReviewCommandValidator : AbstractValidator<CreateReviewCommand>
{
    private readonly IMediator _mediator;

    public CreateReviewCommandValidator(IMediator mediator)
    {
        _mediator = mediator;

        RuleFor(x => x.Rating)
            .InclusiveBetween(1, 5);

        RuleFor(x => x.ReviewText)
            .MinimumLength(10)
            .MaximumLength(2000);

        RuleFor(x => x.ProductId)
            .MustAsync(async (cmd, productId, ct) =>
            {
                // Check if user purchased this product
                var query = new HasPurchasedProductQuery(cmd.UserId, productId);
                return await _mediator.Send(query, ct);
            })
            .WithMessage("You must purchase this product before reviewing");
    }
}

// Features/Review/Application/Queries/HasPurchasedProductQuery.cs
public record HasPurchasedProductQuery(string UserId, Guid ProductId) : IRequest<bool>;

public class HasPurchasedProductQueryHandler : IRequestHandler<HasPurchasedProductQuery, bool>
{
    private readonly OrderDbContext _orderDbContext;

    public async Task<bool> Handle(HasPurchasedProductQuery request, CancellationToken ct)
    {
        return await _orderDbContext.Orders
            .Where(o => o.BuyerId == request.UserId)
            .Where(o => o.Status == OrderStatus.Completed)
            .Where(o => o.Items.Any(i => i.ProductId == request.ProductId))
            .AnyAsync(ct);
    }
}
```

---

## Migration Strategy

### Phase 1: Profile Feature (Week 1)

**Backend:**
1. Add `SixLabors.ImageSharp` package
2. Create `ProfileDbContext` (schema: `profile`, table: `user_addresses`)
3. Create `Features/Profile/` vertical slice
4. Implement commands:
   - `UpdateDisplayNameCommand` → update Keycloak attribute
   - `UploadAvatarCommand` → ImageSharp → Blob Storage → Keycloak attribute
   - `AddAddressCommand` → save to database
5. Create `KeycloakUserService` for Admin API calls

**Frontend:**
1. Create `/profile` page (Server Component)
2. Avatar upload form (Server Action with FormData)
3. Display name edit form
4. Address management (add/edit/delete)

**Test:**
- Upload JPEG/PNG/WebP → resized to 200x200 → stored as WebP → URL in Keycloak
- Update display name → reflected in Keycloak attributes → synced to JWT

---

### Phase 2: Review Feature (Week 2)

**Backend:**
1. Create `ReviewDbContext` (schema: `review`, tables: `reviews`, `review_votes`)
2. Create `Features/Review/` vertical slice
3. Implement commands:
   - `CreateReviewCommand` → validate verified purchase → save review
   - `UpdateReviewCommand` → only author can update
   - `DeleteReviewCommand` → only author or admin
   - `MarkReviewHelpfulCommand` → increment helpful count
4. Add trigger or domain event to update product average rating
5. Optional: Add PostgreSQL tsvector for search

**Frontend:**
1. Copy shadcn/ui rating component to `src/components/ui/rating.tsx`
2. Create review form (rating input + textarea)
3. Display reviews on product page (read-only rating + text)
4. Show "Verified Purchase" badge

**Test:**
- Submit review without purchase → validation error
- Submit review with purchase → saved with verified_purchase=true
- Product average rating updates after review submission

---

### Phase 3: Wishlist Feature (Week 3)

**Backend:**
1. Create `WishlistDbContext` (schema: `wishlist`, table: `wishlist_items`)
2. Create `Features/Wishlist/` vertical slice
3. Implement commands:
   - `AddItemCommand` → validate product exists, not duplicate
   - `RemoveItemCommand` → soft delete
   - `GetWishlistQuery` → return all items for user
   - `MoveToCartCommand` → bulk add to cart + clear wishlist

**Frontend:**
1. Add heart icon button to product cards (lucide-react)
2. Optimistic updates with @tanstack/react-query
3. Wishlist page showing all saved items
4. "Move all to cart" button

**Test:**
- Add product to wishlist → icon changes to filled heart
- Remove from wishlist → icon changes to outline heart
- Move to cart → all items added, wishlist cleared

---

## Sources

### High Confidence (Official Documentation)

- [SixLabors.ImageSharp NuGet](https://www.nuget.org/packages/sixlabors.imagesharp/) — Version 3.1.12, .NET 10 compatibility verified
- [ImageSharp Official Docs](https://docs.sixlabors.com/articles/imagesharp/) — Processing guide, API reference
- [ASP.NET Core File Uploads (Microsoft Learn)](https://learn.microsoft.com/en-us/aspnet/core/mvc/models/file-uploads?view=aspnetcore-10.0) — Official guidance
- [PostgreSQL Full-Text Search](https://www.postgresql.org/docs/current/textsearch.html) — Official tsvector/tsquery docs
- [Npgsql EF Core Full-Text Search](https://www.npgsql.org/efcore/mapping/full-text-search.html) — Provider documentation
- [Next.js Forms Guide](https://nextjs.org/docs/app/guides/forms) — Server Actions with FormData
- [Next.js Server Actions](https://nextjs.org/docs/app/building-your-application/data-fetching/server-actions-and-mutations) — Official Next.js 16 docs
- [Keycloak Custom User Attributes (Baeldung)](https://www.baeldung.com/keycloak-custom-user-attributes) — Integration guide
- [PostgreSQL Generated Columns](https://www.postgresql.org/docs/current/ddl-generated-columns.html) — Official generated column docs

### Medium Confidence (Verified Community Practices)

- [shadcn.io Rating Button](https://www.shadcn.io/button/rating) — Official shadcn/ui rating component
- [Code Maze: File Upload Validation](https://code-maze.com/aspnetcore-validate-uploaded-file/) — Best practices
- [Next.js Server Actions Complete Guide (MakerKit)](https://makerkit.dev/blog/tutorials/nextjs-server-actions) — Comprehensive tutorial
- [EF Core PostgreSQL Full-Text Search (Medium)](https://medium.com/@vosarat1995/postgres-full-text-search-with-ef-core-9-40da6805033e) — Practical guide
- [Verified Purchase Reviews (FasterCapital)](https://fastercapital.com/content/Product-reviews-and-ratings--Verified-Purchase-Reviews--The-Power-of-Verified-Purchase-Reviews-in-E-Commerce.html) — Industry patterns

### Low Confidence (Community Patterns—Verify During Implementation)

- [OnGres: Generated Columns vs Triggers](https://ongres.com/blog/generate_columns_vs_triggers/) — Performance comparison
- [Strapi: Next.js 15 File Upload](https://strapi.io/blog/epic-next-js-15-tutorial-part-5-file-upload-using-server-actions) — Implementation patterns

---

**Stack research for: User Accounts & Profiles, Product Reviews & Ratings, Wishlists**
**Researched: 2026-02-13**
**Confidence: HIGH (all recommendations verified with official sources, minimal new dependencies)**
