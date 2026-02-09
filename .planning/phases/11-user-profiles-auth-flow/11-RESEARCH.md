# Phase 11: User Profiles & Authentication Flow - Research

**Researched:** 2026-02-13
**Domain:** User profile management (display name, avatar, addresses) + guest-to-authenticated migration
**Confidence:** HIGH

## Summary

Phase 11 introduces user profile management with a new ProfilesDbContext following the established database-per-feature pattern. The backend will manage display names, avatar images (Azure Blob Storage with server-side ImageSharp processing), and address book CRUD. The frontend adds a sidebar-navigated "My Account" page with four sections (Profile, Addresses, Orders, Security). The critical cart merge on login already has established patterns from BuyerIdentity.GetOrCreateBuyerId() used in Cart and Ordering modules.

The codebase already has: (1) Keycloak authentication with JWT validation, (2) NextAuth.js v5 client-side integration, (3) Azure Blob Storage client for image uploads (ImageUploadService pattern), (4) BuyerIdentity cookie-to-auth resolution, (5) @tanstack/react-query for data fetching, (6) Dialog/AlertDialog components for modals. The main new additions are: (1) Profiles feature module with UserProfile aggregate, (2) Address value object collection pattern, (3) ImageSharp NuGet package for server-side image crop/resize, (4) cart merge command triggered on login, (5) My Account sidebar navigation UI.

**Primary recommendation:** Follow the Catalog/Inventory module structure exactly. Add ImageSharp v3+ for avatar processing (crop to square, resize to 400x400). Store display name and avatar URL in the database, NOT in Keycloak (avoids complex Keycloak Admin API integration). Cart merge on login uses existing BuyerIdentity pattern with a new MergeCartsCommand. Guest orders stay orphaned (user decision - no email matching).

<user_constraints>
## User Constraints (from CONTEXT.md)

### Locked Decisions

**Profile page layout:**
- Sidebar navigation with four sections: Profile, Addresses, Orders, Security
- Profile info displayed in view mode with an "Edit" button to switch to editable form
- Security section links to Keycloak account management (not a custom password form)

**Avatar handling:**
- Click directly on the avatar circle to trigger file upload (no separate button)
- No crop UI — server-side auto-crop to square from center and resize
- Default placeholder: generic silhouette icon (no initials)
- Remove button available after uploading — reverts to silhouette placeholder
- Images stored in Azure Blob Storage (per success criteria)

**Address book behavior:**
- Modal/dialog form for adding and editing addresses
- No limit on number of saved addresses
- Star icon / "Set as default" toggle on each address card to mark default
- Delete with confirmation prompt

**Guest-to-auth migration:**
- Silent cart merge on login — guest cart items add to user's existing cart, quantities combine for same products
- Guest orders are NOT linked to authenticated accounts (no email matching)
- Login/register available via header account icon AND at checkout
- After login, user stays on the current page (no redirect)

### Claude's Discretion

- Order history display format (compact list vs cards)
- Address form fields (standard fixed vs country-adaptive)
- Exact loading states and error handling
- Typography, spacing, and visual polish
- Security section implementation details

### Deferred Ideas (OUT OF SCOPE)

None — discussion stayed within phase scope

</user_constraints>

## Standard Stack

### Core (Already in Codebase)

| Library | Version | Purpose | Why Standard |
|---------|---------|---------|--------------|
| EF Core (Npgsql) | 10.0 | Profile/Address PostgreSQL persistence | Already used for Catalog, Cart, Inventory, Ordering schemas |
| MediatR | 13.1.0 | CQRS command/query handlers | Already wired with validation pipeline |
| FluentValidation | (current) | Input validation (addresses, display name) | Already auto-discovered from assembly |
| Azure.Storage.Blobs | (current) | Avatar image storage | Already used in ImageUploadService |
| @tanstack/react-query | ^5.90.20 | Server state management | Already installed in Phase 6 (Cart) |
| @radix-ui/react-dialog | ^1.1.15 | Address add/edit modal | Already installed (shadcn/ui) |
| @radix-ui/react-alert-dialog | ^1.1.15 | Delete confirmation | Already installed (shadcn/ui) |
| @radix-ui/react-avatar | ^1.1.11 | Avatar component | Already installed |
| lucide-react | ^0.563.0 | Icons (User, MapPin, Package, Shield, Star, Trash2, Upload) | Already installed |
| sonner | ^2.0.7 | Toast notifications | Already installed |

### New Dependencies

| Library | Version | Purpose | Why Standard |
|---------|---------|---------|--------------|
| SixLabors.ImageSharp | ^3.1.6 | Server-side image crop/resize | Cross-platform, high-performance, official .NET image library; no System.Drawing dependency |

### Alternatives Considered

| Instead of | Could Use | Tradeoff |
|------------|-----------|----------|
| ImageSharp | System.Drawing.Common | System.Drawing is Windows-only and deprecated for ASP.NET Core; ImageSharp is cross-platform standard |
| ImageSharp | SkiaSharp | SkiaSharp is heavier; ImageSharp is simpler for basic crop/resize |
| DB-stored profiles | Keycloak custom attributes | Keycloak Admin API adds complexity; DB storage follows established module pattern and allows domain events |
| Separate avatar service | Extend ImageUploadService | Reusing existing pattern is simpler; avatar needs different processing (square crop, fixed size) |

**Installation:**

```bash
# Backend
cd src/MicroCommerce.ApiService
dotnet add package SixLabors.ImageSharp --version 3.1.6

# Frontend - no new packages needed
```

## Architecture Patterns

### Recommended Project Structure

```
Features/Profiles/
├── Domain/
│   ├── Entities/
│   │   └── UserProfile.cs              # Aggregate root
│   ├── Events/
│   │   ├── ProfileCreatedDomainEvent.cs
│   │   ├── ProfileUpdatedDomainEvent.cs
│   │   └── AddressSetAsDefaultDomainEvent.cs
│   └── ValueObjects/
│       ├── UserProfileId.cs            # StronglyTypedId<Guid>
│       ├── DisplayName.cs              # ValueObject with 2-50 char validation
│       ├── Address.cs                  # ValueObject (owned entity collection)
│       └── AddressId.cs                # StronglyTypedId<Guid>
├── Application/
│   ├── Commands/
│   │   ├── CreateProfile/              # CreateProfileCommand + Handler
│   │   ├── UpdateProfile/              # UpdateProfileCommand + Handler + Validator
│   │   ├── UploadAvatar/               # UploadAvatarCommand + Handler
│   │   ├── RemoveAvatar/               # RemoveAvatarCommand + Handler
│   │   ├── AddAddress/                 # AddAddressCommand + Handler + Validator
│   │   ├── UpdateAddress/              # UpdateAddressCommand + Handler + Validator
│   │   ├── DeleteAddress/              # DeleteAddressCommand + Handler
│   │   └── SetDefaultAddress/          # SetDefaultAddressCommand + Handler
│   └── Queries/
│       ├── GetProfile/                 # GetProfileQuery + Handler + ProfileDto
│       └── GetProfileByUserId/         # GetProfileByUserIdQuery (internal)
├── Infrastructure/
│   ├── ProfilesDbContext.cs            # Schema: 'profiles'
│   ├── Configurations/
│   │   ├── UserProfileConfiguration.cs
│   │   └── AddressConfiguration.cs     # OwnsMany configuration
│   ├── AvatarImageService.cs           # Extends/wraps ImageUploadService with crop logic
│   └── Migrations/
└── ProfilesEndpoints.cs                # Minimal API endpoints

Cart/Application/Commands/MergeCarts/   # NEW - cart merge on login
└── MergeCartsCommand.cs                # Triggered by login event/middleware
```

Frontend additions:

```
src/
├── app/
│   └── (storefront)/
│       └── account/
│           ├── layout.tsx              # Sidebar layout (NEW)
│           ├── page.tsx                # Redirects to /account/profile
│           ├── profile/
│           │   └── page.tsx            # Profile section (NEW)
│           ├── addresses/
│           │   └── page.tsx            # Address book section (NEW)
│           ├── orders/
│           │   └── page.tsx            # Order history (Phase 8 already implemented)
│           └── security/
│               └── page.tsx            # Link to Keycloak (NEW)
├── components/
│   └── account/
│       ├── account-sidebar.tsx         # Nav sidebar (NEW)
│       ├── profile-form.tsx            # Edit profile form (NEW)
│       ├── avatar-upload.tsx           # Avatar click upload (NEW)
│       ├── address-form-dialog.tsx     # Add/edit modal (NEW)
│       ├── address-card.tsx            # Address display card (NEW)
│       └── delete-address-dialog.tsx   # Confirmation dialog (NEW)
├── hooks/
│   ├── use-profile.ts                  # Profile query + mutations (NEW)
│   └── use-addresses.ts                # Address CRUD mutations (NEW)
└── lib/
    └── api.ts                          # Add profile + address API functions
```

### Pattern 1: UserProfile Aggregate with Address Collection

**What:** UserProfile is the aggregate root; Address is a value object collection (OwnsMany in EF Core). All mutations go through UserProfile methods.

**When to use:** Always - this is the established DDD pattern for owned collections.

**Example:**

```csharp
// Source: DDD aggregate pattern established in Cart, Inventory modules
public sealed class UserProfile : BaseAggregateRoot<UserProfileId>
{
    private readonly List<Address> _addresses = [];

    public Guid UserId { get; private set; }  // Keycloak sub claim (unique index)
    public DisplayName DisplayName { get; private set; } = null!;
    public string? AvatarUrl { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    [Timestamp]
    public uint Version { get; private set; }  // xmin concurrency token

    public IReadOnlyCollection<Address> Addresses => _addresses.AsReadOnly();

    // Factory method
    public static UserProfile Create(Guid userId, string displayName)
    {
        var profile = new UserProfile(UserProfileId.New())
        {
            UserId = userId,
            DisplayName = DisplayName.Create(displayName),
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        profile.AddDomainEvent(new ProfileCreatedDomainEvent(profile.Id, userId));
        return profile;
    }

    public void UpdateDisplayName(string displayName)
    {
        DisplayName = DisplayName.Create(displayName);
        Touch();
    }

    public void SetAvatar(string avatarUrl)
    {
        AvatarUrl = avatarUrl;
        Touch();
    }

    public void RemoveAvatar()
    {
        AvatarUrl = null;
        Touch();
    }

    public AddressId AddAddress(string name, string street, string city,
        string state, string zipCode, string country, bool setAsDefault = false)
    {
        var address = Address.Create(name, street, city, state, zipCode, country);

        if (setAsDefault || _addresses.Count == 0)
        {
            ClearDefaultAddress();
            address.SetAsDefault();
        }

        _addresses.Add(address);
        Touch();
        return address.Id;
    }

    public void UpdateAddress(AddressId addressId, string name, string street,
        string city, string state, string zipCode, string country)
    {
        var address = FindAddress(addressId);
        var updated = Address.Create(name, street, city, state, zipCode, country);

        // Preserve default flag
        if (address.IsDefault)
        {
            updated.SetAsDefault();
        }

        _addresses.Remove(address);
        _addresses.Add(updated);
        Touch();
    }

    public void DeleteAddress(AddressId addressId)
    {
        var address = FindAddress(addressId);
        bool wasDefault = address.IsDefault;

        _addresses.Remove(address);

        // If deleted default, set first remaining as default
        if (wasDefault && _addresses.Count > 0)
        {
            _addresses[0].SetAsDefault();
        }

        Touch();
    }

    public void SetDefaultAddress(AddressId addressId)
    {
        var address = FindAddress(addressId);

        ClearDefaultAddress();
        address.SetAsDefault();

        Touch();
        AddDomainEvent(new AddressSetAsDefaultDomainEvent(Id, addressId));
    }

    private Address FindAddress(AddressId addressId)
    {
        return _addresses.FirstOrDefault(a => a.Id == addressId)
            ?? throw new InvalidOperationException($"Address {addressId} not found");
    }

    private void ClearDefaultAddress()
    {
        foreach (var addr in _addresses.Where(a => a.IsDefault))
        {
            addr.ClearDefault();
        }
    }

    private void Touch()
    {
        UpdatedAt = DateTimeOffset.UtcNow;
        AddDomainEvent(new ProfileUpdatedDomainEvent(Id, UserId));
    }
}
```

### Pattern 2: Address as Value Object with Identity (Owned Entity)

**What:** Address is a value object with an ID, owned by UserProfile via EF Core OwnsMany. Immutable creation pattern.

**When to use:** Collections of domain objects that have identity but no independent lifecycle.

**Example:**

```csharp
// Source: Established ValueObject pattern from CategoryName, Money
public sealed class Address : ValueObject
{
    public AddressId Id { get; private set; } = null!;
    public string Name { get; private set; } = null!;  // "Home", "Work", etc.
    public string Street { get; private set; } = null!;
    public string City { get; private set; } = null!;
    public string State { get; private set; } = null!;
    public string ZipCode { get; private set; } = null!;
    public string Country { get; private set; } = null!;
    public bool IsDefault { get; private set; }

    private Address() { }  // EF constructor

    public static Address Create(string name, string street, string city,
        string state, string zipCode, string country)
    {
        Guard.Against.NullOrWhiteSpace(name, nameof(name));
        Guard.Against.NullOrWhiteSpace(street, nameof(street));
        Guard.Against.NullOrWhiteSpace(city, nameof(city));
        Guard.Against.NullOrWhiteSpace(state, nameof(state));
        Guard.Against.NullOrWhiteSpace(zipCode, nameof(zipCode));
        Guard.Against.NullOrWhiteSpace(country, nameof(country));

        return new Address
        {
            Id = AddressId.New(),
            Name = name.Trim(),
            Street = street.Trim(),
            City = city.Trim(),
            State = state.Trim(),
            ZipCode = zipCode.Trim(),
            Country = country.Trim(),
            IsDefault = false
        };
    }

    internal void SetAsDefault() => IsDefault = true;
    internal void ClearDefault() => IsDefault = false;

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Street;
        yield return City;
        yield return State;
        yield return ZipCode;
        yield return Country;
    }
}
```

### Pattern 3: Server-Side Avatar Processing with ImageSharp

**What:** Upload handler receives image, uses ImageSharp to crop to square from center and resize to 400x400, then uploads to Azure Blob Storage.

**When to use:** Always for avatar uploads - consistent, square, performant images.

**Example:**

```csharp
// Source: ImageSharp docs + existing ImageUploadService pattern
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

public class AvatarImageService
{
    private readonly BlobServiceClient _blobServiceClient;
    private const string ContainerName = "avatars";
    private const int AvatarSize = 400;

    public AvatarImageService(BlobServiceClient blobServiceClient)
    {
        _blobServiceClient = blobServiceClient;
    }

    public async Task<string> ProcessAndUploadAvatarAsync(
        Stream imageStream,
        string originalFileName,
        CancellationToken cancellationToken = default)
    {
        using var image = await Image.LoadAsync(imageStream, cancellationToken);

        // Crop to square from center
        int size = Math.Min(image.Width, image.Height);
        int x = (image.Width - size) / 2;
        int y = (image.Height - size) / 2;

        image.Mutate(ctx => ctx
            .Crop(new Rectangle(x, y, size, size))
            .Resize(AvatarSize, AvatarSize));

        // Upload to blob storage
        var containerClient = _blobServiceClient.GetBlobContainerClient(ContainerName);
        await containerClient.CreateIfNotExistsAsync(
            PublicAccessType.Blob,
            cancellationToken: cancellationToken);

        var blobName = $"{Guid.NewGuid()}.jpg";
        var blobClient = containerClient.GetBlobClient(blobName);

        using var outputStream = new MemoryStream();
        await image.SaveAsJpegAsync(outputStream, cancellationToken);
        outputStream.Position = 0;

        await blobClient.UploadAsync(
            outputStream,
            new BlobHttpHeaders { ContentType = "image/jpeg" },
            cancellationToken: cancellationToken);

        return blobClient.Uri.ToString();
    }

    public async Task DeleteAvatarAsync(string avatarUrl, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(avatarUrl)) return;

        var containerClient = _blobServiceClient.GetBlobContainerClient(ContainerName);
        var blobName = Path.GetFileName(new Uri(avatarUrl).LocalPath);

        await containerClient.DeleteBlobIfExistsAsync(blobName, cancellationToken: cancellationToken);
    }
}
```

### Pattern 4: Cart Merge on Login Using Existing BuyerIdentity Pattern

**What:** On login, check if guest BuyerId cookie exists. If yes, merge guest cart into authenticated user's cart, then clear cookie.

**When to use:** Triggered after successful authentication (middleware or callback).

**Example:**

```csharp
// Source: Established BuyerIdentity pattern from CartEndpoints, OrderingEndpoints
public sealed record MergeCartsCommand(Guid GuestBuyerId, Guid AuthenticatedBuyerId) : IRequest;

public sealed class MergeCartsCommandHandler : IRequestHandler<MergeCartsCommand>
{
    private readonly CartDbContext _context;

    public MergeCartsCommandHandler(CartDbContext context)
    {
        _context = context;
    }

    public async Task Handle(MergeCartsCommand request, CancellationToken cancellationToken)
    {
        // Load both carts
        var guestCart = await _context.Carts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.BuyerId == request.GuestBuyerId, cancellationToken);

        if (guestCart is null || !guestCart.Items.Any())
            return;  // No guest cart to merge

        var authCart = await _context.Carts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.BuyerId == request.AuthenticatedBuyerId, cancellationToken);

        if (authCart is null)
        {
            // No existing auth cart - just transfer ownership
            guestCart.TransferOwnership(request.AuthenticatedBuyerId);
        }
        else
        {
            // Merge items: sum quantities for matching products
            foreach (var guestItem in guestCart.Items)
            {
                authCart.AddItem(
                    guestItem.ProductId,
                    guestItem.ProductName,
                    guestItem.UnitPrice,
                    guestItem.ImageUrl,
                    guestItem.Quantity);
            }

            // Delete guest cart
            _context.Carts.Remove(guestCart);
        }

        await _context.SaveChangesAsync(cancellationToken);
    }
}
```

### Pattern 5: React Sidebar Navigation with Active State

**What:** Sidebar nav with Link components, highlight active route, persistent across sections.

**When to use:** Multi-section account/settings pages.

**Example:**

```typescript
// Source: Next.js Link + usePathname pattern
"use client";

import { usePathname } from "next/navigation";
import Link from "next/link";
import { User, MapPin, Package, Shield } from "lucide-react";
import { cn } from "@/lib/utils";

const sections = [
  { href: "/account/profile", label: "Profile", icon: User },
  { href: "/account/addresses", label: "Addresses", icon: MapPin },
  { href: "/account/orders", label: "Orders", icon: Package },
  { href: "/account/security", label: "Security", icon: Shield },
];

export function AccountSidebar() {
  const pathname = usePathname();

  return (
    <nav className="flex flex-col gap-1">
      {sections.map((section) => {
        const Icon = section.icon;
        const isActive = pathname.startsWith(section.href);

        return (
          <Link
            key={section.href}
            href={section.href}
            className={cn(
              "flex items-center gap-3 rounded-lg px-3 py-2 transition-colors",
              isActive
                ? "bg-primary text-primary-foreground"
                : "text-muted-foreground hover:bg-accent hover:text-accent-foreground"
            )}
          >
            <Icon className="h-5 w-5" />
            {section.label}
          </Link>
        );
      })}
    </nav>
  );
}
```

### Pattern 6: Avatar Upload via Click with Hidden File Input

**What:** Click avatar to trigger hidden file input, show loading state, optimistic update.

**When to use:** Avatar upload UI without separate button.

**Example:**

```typescript
// Source: Standard file input + label pattern
"use client";

import { Avatar, AvatarFallback, AvatarImage } from "@/components/ui/avatar";
import { User, Upload } from "lucide-react";
import { useRef } from "react";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { uploadAvatar } from "@/lib/api";
import { toast } from "sonner";

export function AvatarUpload({ currentAvatarUrl }: { currentAvatarUrl?: string }) {
  const fileInputRef = useRef<HTMLInputElement>(null);
  const queryClient = useQueryClient();

  const uploadMutation = useMutation({
    mutationFn: uploadAvatar,
    onSuccess: (newAvatarUrl) => {
      queryClient.invalidateQueries({ queryKey: ["profile"] });
      toast.success("Avatar updated");
    },
    onError: () => {
      toast.error("Failed to upload avatar");
    },
  });

  const handleFileChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (!file) return;

    // Validate file type
    if (!file.type.startsWith("image/")) {
      toast.error("Please select an image file");
      return;
    }

    // Validate file size (5MB max)
    if (file.size > 5 * 1024 * 1024) {
      toast.error("Image must be less than 5MB");
      return;
    }

    uploadMutation.mutate(file);
  };

  return (
    <div className="relative">
      <button
        type="button"
        onClick={() => fileInputRef.current?.click()}
        className="group relative"
        disabled={uploadMutation.isPending}
      >
        <Avatar className="h-24 w-24">
          <AvatarImage src={currentAvatarUrl} alt="Avatar" />
          <AvatarFallback>
            <User className="h-12 w-12" />
          </AvatarFallback>
        </Avatar>

        {/* Upload overlay on hover */}
        <div className="absolute inset-0 flex items-center justify-center rounded-full bg-black/50 opacity-0 transition-opacity group-hover:opacity-100">
          <Upload className="h-8 w-8 text-white" />
        </div>
      </button>

      <input
        ref={fileInputRef}
        type="file"
        accept="image/*"
        onChange={handleFileChange}
        className="hidden"
      />
    </div>
  );
}
```

### Anti-Patterns to Avoid

- **Storing profiles in Keycloak custom attributes:** Adds complexity, requires Admin API, doesn't fit domain event pattern. Use database for profile domain.
- **Client-side image crop UI:** User decision specifies server-side only. Don't add client crop libraries.
- **Separate Address entity/table:** Addresses are owned by UserProfile via OwnsMany. No independent lifecycle.
- **Linking guest orders by email:** Explicitly out of scope per user decision. Guest orders stay orphaned.
- **Custom password change form:** Security section links to Keycloak account management. No custom implementation.
- **Initialed avatar placeholders:** User decision specifies generic silhouette icon only.

## Don't Hand-Roll

| Problem | Don't Build | Use Instead | Why |
|---------|-------------|-------------|-----|
| Image processing (crop, resize) | Custom canvas manipulation or external API | SixLabors.ImageSharp | Cross-platform, high-performance, built for .NET, handles edge cases |
| Address validation (postal codes) | Custom regex per country | FluentValidation with regex patterns | FluentValidation already in use; regex patterns well-documented |
| Avatar file upload | Custom multipart form handling | IFormFile (built-in) | ASP.NET Core built-in multipart handling is robust |
| Modal dialogs | Custom modal component | @radix-ui/react-dialog (already installed) | Already installed via shadcn/ui, accessible, composable |
| Confirmation dialogs | Custom alert component | @radix-ui/react-alert-dialog (already installed) | Already installed, accessible, follows platform patterns |
| Default address selection | Multiple boolean flags | Single IsDefault flag with invariant enforcement | Aggregate ensures exactly one default via ClearDefaultAddress() |

**Key insight:** ImageSharp is the only new library needed. Everything else uses existing patterns or built-in capabilities.

## Common Pitfalls

### Pitfall 1: Forgetting to Merge Cart on Login

**What goes wrong:** User adds items as guest, logs in, finds empty cart.

**Why it happens:** No login callback triggers cart merge.

**How to avoid:** Add middleware or NextAuth callback that checks for guest buyer_id cookie after authentication. If present, trigger MergeCartsCommand. Clear cookie after merge.

**Warning signs:** Users complaining about lost cart items after login.

### Pitfall 2: Multiple Default Addresses

**What goes wrong:** User has multiple addresses marked as default, checkout shows wrong address.

**Why it happens:** SetAsDefault doesn't clear other defaults.

**How to avoid:** Use ClearDefaultAddress() before setting new default (aggregate invariant enforcement). Add database constraint if needed.

**Warning signs:** Multiple default addresses in database.

### Pitfall 3: Avatar Upload Memory Leak

**What goes wrong:** Large image uploads cause memory issues or timeouts.

**Why it happens:** Loading entire image into memory without streaming.

**How to avoid:** Use streaming upload with ImageSharp. Process stream, write to Blob Storage stream. Add file size validation (5MB max recommended).

**Warning signs:** Memory spikes on avatar upload, 500 errors on large files.

### Pitfall 4: Address Validation Too Strict

**What goes wrong:** International users can't save addresses due to US-centric validation.

**Why it happens:** Hard-coded zip code format, state field required.

**How to avoid:** Use flexible validation - alphanumeric for postal codes, optional state field, country dropdown. Don't enforce country-specific rules unless user decision requires it.

**Warning signs:** Support requests from international users about address validation errors.

### Pitfall 5: Exposing UserId Instead of Profile Aggregate ID

**What goes wrong:** Frontend queries by Keycloak userId (Guid from sub claim) but endpoints expect UserProfileId.

**Why it happens:** Confusion between userId (external identity) and UserProfileId (aggregate ID).

**How to avoid:** Endpoints use userId for lookup (GetProfileByUserIdQuery), return ProfileDto with both IDs. Internal queries use UserProfileId. Clear naming: `userId` vs `profileId`.

**Warning signs:** 404 errors when fetching profile after login.

### Pitfall 6: Race Condition on Profile Creation

**What goes wrong:** First login creates profile, rapid API calls create duplicate profiles for same userId.

**Why it happens:** No unique constraint on UserId column.

**How to avoid:** Add unique index on UserId in ProfileConfiguration. Use InsertOrUpdate pattern (check existence before creating).

**Warning signs:** Duplicate UserProfile rows with same UserId.

### Pitfall 7: Not Invalidating Profile Query After Avatar Upload

**What goes wrong:** Avatar uploads but UI doesn't update until page refresh.

**Why it happens:** React Query cache not invalidated after mutation.

**How to avoid:** Use onSuccess callback in uploadAvatar mutation to invalidate `["profile"]` query. Avatar is part of profile data.

**Warning signs:** Stale avatar shown after upload.

## Code Examples

### Profile Endpoint Pattern (Following Cart/Inventory Modules)

```csharp
// ProfilesEndpoints.cs - follows CartEndpoints, InventoryEndpoints pattern
public static class ProfilesEndpoints
{
    public static IEndpointRouteBuilder MapProfilesEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/profiles")
            .WithTags("Profiles")
            .RequireAuthorization();  // All profile endpoints require auth

        group.MapGet("/me", GetMyProfile)
            .WithName("GetMyProfile")
            .Produces<ProfileDto>()
            .Produces(StatusCodes.Status404NotFound);

        group.MapPut("/me", UpdateProfile)
            .WithName("UpdateProfile")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem();

        group.MapPost("/me/avatar", UploadAvatar)
            .WithName("UploadAvatar")
            .Accepts<IFormFile>("multipart/form-data")
            .Produces<UploadAvatarResult>()
            .ProducesProblem(StatusCodes.Status400BadRequest);

        group.MapDelete("/me/avatar", RemoveAvatar)
            .WithName("RemoveAvatar")
            .Produces(StatusCodes.Status204NoContent);

        group.MapPost("/me/addresses", AddAddress)
            .WithName("AddAddress")
            .Produces<AddAddressResult>(StatusCodes.Status201Created)
            .ProducesValidationProblem();

        group.MapPut("/me/addresses/{addressId:guid}", UpdateAddress)
            .WithName("UpdateAddress")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapDelete("/me/addresses/{addressId:guid}", DeleteAddress)
            .WithName("DeleteAddress")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPatch("/me/addresses/{addressId:guid}/default", SetDefaultAddress)
            .WithName("SetDefaultAddress")
            .Produces(StatusCodes.Status204NoContent);

        return endpoints;
    }

    private static async Task<IResult> GetMyProfile(
        HttpContext httpContext,
        ISender sender,
        CancellationToken ct)
    {
        var userId = GetUserId(httpContext);
        var result = await sender.Send(new GetProfileQuery(userId), ct);
        return result is null ? Results.NotFound() : Results.Ok(result);
    }

    private static Guid GetUserId(HttpContext context)
    {
        var sub = context.User.FindFirst("sub")?.Value
            ?? throw new UnauthorizedAccessException("User not authenticated");

        return Guid.Parse(sub);
    }

    // ... other endpoint implementations
}
```

### Address Validation with FluentValidation

```csharp
// Source: FluentValidation docs + established pattern from product validators
public sealed class AddAddressCommandValidator : AbstractValidator<AddAddressCommand>
{
    public AddAddressCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(50)
            .WithMessage("Address name must be 50 characters or less");

        RuleFor(x => x.Street)
            .NotEmpty()
            .MaximumLength(200)
            .WithMessage("Street address must be 200 characters or less");

        RuleFor(x => x.City)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.State)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(x => x.ZipCode)
            .NotEmpty()
            .Matches(@"^[0-9a-zA-Z\s\-]+$")
            .WithMessage("Postal code must contain only letters, numbers, spaces, and hyphens")
            .MaximumLength(20);

        RuleFor(x => x.Country)
            .NotEmpty()
            .MaximumLength(100);
    }
}
```

### React Query Profile Hooks

```typescript
// src/hooks/use-profile.ts
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";
import {
  getMyProfile,
  updateProfile,
  uploadAvatar,
  removeAvatar
} from "@/lib/api";

export function useProfile() {
  return useQuery({
    queryKey: ["profile"],
    queryFn: getMyProfile,
  });
}

export function useUpdateProfile() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: updateProfile,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["profile"] });
      toast.success("Profile updated");
    },
    onError: (error: Error) => {
      toast.error(error.message || "Failed to update profile");
    },
  });
}

export function useUploadAvatar() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: uploadAvatar,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["profile"] });
      toast.success("Avatar updated");
    },
    onError: (error: Error) => {
      toast.error(error.message || "Failed to upload avatar");
    },
  });
}

export function useRemoveAvatar() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: removeAvatar,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["profile"] });
      toast.success("Avatar removed");
    },
    onError: () => {
      toast.error("Failed to remove avatar");
    },
  });
}
```

### Address Form Dialog with Radix UI

```typescript
// src/components/account/address-form-dialog.tsx
"use client";

import { useState } from "react";
import { useForm } from "react-hook-form";
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogTrigger,
} from "@/components/ui/dialog";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { useAddAddress, useUpdateAddress } from "@/hooks/use-addresses";

interface AddressFormData {
  name: string;
  street: string;
  city: string;
  state: string;
  zipCode: string;
  country: string;
}

interface Props {
  address?: AddressDto;
  trigger: React.ReactNode;
}

export function AddressFormDialog({ address, trigger }: Props) {
  const [open, setOpen] = useState(false);
  const isEdit = !!address;

  const { register, handleSubmit, formState: { errors } } = useForm<AddressFormData>({
    defaultValues: address || {},
  });

  const addMutation = useAddAddress();
  const updateMutation = useUpdateAddress();

  const onSubmit = async (data: AddressFormData) => {
    if (isEdit) {
      await updateMutation.mutateAsync({ id: address.id, ...data });
    } else {
      await addMutation.mutateAsync(data);
    }
    setOpen(false);
  };

  return (
    <Dialog open={open} onOpenChange={setOpen}>
      <DialogTrigger asChild>{trigger}</DialogTrigger>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>{isEdit ? "Edit Address" : "Add Address"}</DialogTitle>
        </DialogHeader>

        <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
          <div>
            <Label htmlFor="name">Address Name</Label>
            <Input
              id="name"
              placeholder="Home, Work, etc."
              {...register("name", { required: "Name is required" })}
            />
            {errors.name && (
              <p className="text-sm text-destructive">{errors.name.message}</p>
            )}
          </div>

          {/* ... other fields (street, city, state, zipCode, country) ... */}

          <Button type="submit" disabled={addMutation.isPending || updateMutation.isPending}>
            {isEdit ? "Update" : "Add"} Address
          </Button>
        </form>
      </DialogContent>
    </Dialog>
  );
}
```

## State of the Art

| Old Approach | Current Approach | When Changed | Impact |
|--------------|------------------|--------------|--------|
| System.Drawing for image processing | SixLabors.ImageSharp | 2020+ | Cross-platform, no Windows dependency, better performance |
| Keycloak user attributes for profile | Database-backed profiles | Established DDD practice | Domain events, easier querying, follows module pattern |
| Custom avatar crop UI | Server-side processing | 2020s standard | Simpler, consistent results, no client library weight |
| Plain fetch + useState | @tanstack/react-query | Phase 6 (Cart) | Cache invalidation, optimistic updates, loading states |
| react-hook-form + manual validation | react-hook-form + zod/yup | 2024+ | Schema-driven validation, type safety |

**Deprecated/outdated:**
- System.Drawing.Common: Deprecated for ASP.NET Core, Windows-only
- Keycloak Admin API for simple profile fields: Overengineering for basic display name + avatar
- Client-side image crop libraries (Cropper.js, react-easy-crop): User decision specifies server-side

## Open Questions

1. **Address book pagination**
   - What we know: "No limit on number of saved addresses" (user decision)
   - What's unclear: Should we paginate if user has 100+ addresses?
   - Recommendation: No pagination initially. If > 50 addresses, show all but optimize query. Very rare edge case.

2. **Display name change frequency limit**
   - What we know: Profile can be updated with "Edit" button
   - What's unclear: Should we rate-limit display name changes to prevent abuse?
   - Recommendation: No limit initially. Add if abuse occurs. Most platforms don't restrict.

3. **Avatar file format restrictions**
   - What we know: Upload via file input, server processes to JPEG
   - What's unclear: Accept only JPEG/PNG uploads or allow any image format?
   - Recommendation: Accept all image/* formats, convert to JPEG on server. ImageSharp handles conversion.

4. **Profile creation timing**
   - What we know: User must have profile to use "My Account" page
   - What's unclear: Create on first login automatically or require explicit setup?
   - Recommendation: Create automatically on first authenticated request with default display name from Keycloak (name or email). User can edit later.

5. **Order history integration**
   - What we know: "Orders" section in sidebar, Phase 8 already implemented order history
   - What's unclear: Link to existing `/orders` page or duplicate functionality?
   - Recommendation: Redirect `/account/orders` to existing `/orders` page. No duplication.

## Sources

### Primary (HIGH confidence)

- Codebase analysis: `Features/Cart/BuyerIdentity.cs` - existing guest-to-auth pattern
- Codebase analysis: `Features/Catalog/Infrastructure/ImageUploadService.cs` - Azure Blob Storage pattern
- Codebase analysis: `Features/Cart/Domain/Entities/Cart.cs` - aggregate with owned collection pattern
- Codebase analysis: `Features/Catalog/Domain/ValueObjects/CategoryName.cs` - value object validation pattern
- Codebase analysis: `Program.cs` - database-per-feature registration pattern
- Codebase analysis: `package.json` - React Query, Radix UI components already installed
- [Microsoft Learn: Identity model customization in ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/customize-identity-model?view=aspnetcore-10.0)
- [SixLabors ImageSharp Resizing Docs](https://docs.sixlabors.com/articles/imagesharp/resize.html)
- [ImageSharp GitHub - Crop and Resize Discussion](https://github.com/SixLabors/ImageSharp.Web/discussions/168)

### Secondary (MEDIUM confidence)

- [Custom User Management in ASP.NET Core MVC with Identity](https://codewithmukesh.com/blog/user-management-in-aspnet-core-mvc/)
- [Using ImageSharp to resize images in ASP.NET Core - Andrew Lock](https://andrewlock.net/using-imagesharp-to-resize-images-in-asp-net-core-part-2/)
- [Upload and resize an image with ASP.NET Core and ImageSharp](https://blog.elmah.io/upload-and-resize-an-image-with-asp-net-core-and-imagesharp/)
- [Adobe Commerce mergeCarts mutation](https://developer.adobe.com/commerce/webapi/graphql/schema/cart/mutations/merge/)
- [Magento 2 mergeCarts mutation](https://r-martins.github.io/m1docs/guides/v2.4/graphql/mutations/merge-carts.html)
- [E-Commerce Authentication: 2026 Benchmark + Best Practice](https://www.corbado.com/blog/ecommerce-authentication)
- [FluentValidation built-in validators](https://docs.fluentvalidation.net/en/latest/built-in-validators.html)
- [International zip code validation: The Ultimate Postal Code Regex](https://www.geopostcodes.com/blog/international-zip-code-validation/)
- [React Form Validation Using Hooks: Complete Patterns & Best Practices](https://react.wiki/hooks/form-validation/)
- [React 19 - A Brief Look At Form Handling](https://syntackle.com/blog/form-handling-in-react-19/)
- [Keycloak Custom User Attributes](https://www.baeldung.com/keycloak-custom-user-attributes)
- [Managing Keycloak user metadata and custom attributes](https://www.mastertheboss.com/keycloak/managing-keycloak-user-metadata-and-custom-attributes/)
- [Next.js UserProfile Component - Clerk](https://clerk.com/docs/nextjs/reference/components/user/user-profile)

### Tertiary (LOW confidence)

- [CRUD Beyond Grids: Modern UI Patterns, Best Practices & 2026 Trends](https://copyprogramming.com/howto/what-is-the-best-ux-to-let-user-perform-crud-operations)
- [9 Best Address Book Software for Businesses in 2026](https://www.bigcontacts.com/blog/best-address-book-software/)

## Metadata

**Confidence breakdown:**
- Standard stack: HIGH - ImageSharp is the only new dependency; all other libraries already in codebase
- Architecture: HIGH - Follows established Cart/Inventory/Catalog module patterns exactly
- Pitfalls: MEDIUM-HIGH - Cart merge and avatar upload patterns verified; address validation best practices from industry sources

**Research date:** 2026-02-13
**Valid until:** 2026-03-13 (30 days - stable technologies, established patterns)
