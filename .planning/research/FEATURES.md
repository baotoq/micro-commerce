# Feature Research

**Domain:** E-commerce User Accounts, Product Reviews, and Wishlists
**Researched:** 2026-02-13
**Confidence:** MEDIUM

## Feature Landscape

### Table Stakes (Users Expect These)

Features users assume exist. Missing these = product feels incomplete.

#### User Profiles & Accounts

| Feature | Why Expected | Complexity | Notes |
|---------|--------------|------------|-------|
| Display name & avatar | Users expect personalized identity in modern apps | LOW | Simple profile metadata, image upload/storage |
| Saved shipping addresses (address book) | Checkout friction killer—users expect to save addresses | MEDIUM | Need CRUD UI, default address selection, validation |
| Order history linkage | "My orders" requires account—users expect permanent record | LOW | Link existing orders to authenticated user ID |
| Account settings page | Self-service profile management is standard UX | LOW | Form with validation, update endpoints already exist pattern |
| Guest-to-authenticated migration | Users expect cart/orders to persist after login/signup | MEDIUM | Merge guest BuyerId with authenticated user, data migration |

#### Product Reviews & Ratings

| Feature | Why Expected | Complexity | Notes |
|---------|--------------|------------|-------|
| Star rating (1-5) | Universal e-commerce standard | LOW | Aggregate rating calculation, display on product cards |
| Written review text | Users expect detailed feedback beyond stars | LOW | Text field with length limits (e.g., 500-5000 chars) |
| Verified purchase badge | Trust signal—60% of users filter to verified only | MEDIUM | Cross-reference order history, badge display logic |
| Review display on product page | Reviews influence 93% of purchase decisions | LOW | Query reviews by ProductId, pagination, sorting |
| Average rating summary | Users scan rating before reading reviews | LOW | Aggregate calculation, cache for performance |
| Review submission form | Standard expectation after purchase | MEDIUM | Validation, one-review-per-product-per-user enforcement |

#### Wishlists

| Feature | Why Expected | Complexity | Notes |
|---------|--------------|------------|-------|
| Add/remove products | Core wishlist functionality | LOW | Simple CRUD on wishlist items |
| Persistent across sessions | Users expect saved lists, not ephemeral | LOW | Database storage, user association |
| Move to cart | Conversion path—users expect quick checkout from wishlist | LOW | Copy wishlist item to cart, remove from wishlist |
| Visual indicator on product cards | "Already wishlisted" feedback prevents duplicates | LOW | Check existence, show filled/unfilled heart icon |
| Wishlist page/view | Dedicated UI to manage saved items | MEDIUM | List view with product details, bulk actions |

### Differentiators (Competitive Advantage)

Features that set the product apart. Not required, but valuable.

| Feature | Value Proposition | Complexity | Notes |
|---------|-------------------|------------|-------|
| Review helpfulness voting | Surfaces quality reviews, reduces noise | MEDIUM | Upvote/downvote counts, sort by helpfulness, prevent self-voting |
| Review images/media upload | Visual proof increases trust 3x vs text-only | HIGH | Image storage, moderation, thumbnail generation |
| Review response from admin | Shows brand engagement, builds trust | MEDIUM | Admin reply to reviews, notification to reviewer |
| Wishlist price drop alerts | Re-engagement tool—30% conversion on price drops | HIGH | Price tracking, background job, email/notification |
| Wishlist stock alerts | Notify when out-of-stock items return | MEDIUM | Inventory event subscriber, notification system |
| Review summary with AI | Quick insights without reading all reviews | HIGH | LLM integration, cost per summary, caching |
| Multiple wishlists | Power users organize by category (birthday, holiday, etc.) | MEDIUM | List CRUD, name/description, default list |
| Wishlist sharing (read-only link) | Gift registry use case, viral growth | MEDIUM | Public/private toggle, share token generation |
| Review sorting/filtering | By date, rating, verified, helpfulness | LOW | Query parameterization, index optimization |
| Review photos in lightbox | Enhanced UX for image reviews | LOW | Frontend component, already common pattern |

### Anti-Features (Commonly Requested, Often Problematic)

Features that seem good but create problems.

| Feature | Why Requested | Why Problematic | Alternative |
|---------|---------------|-----------------|-------------|
| Anonymous reviews | "Increases participation" claim | Fake reviews explode, trust collapses, Amazon/Walmart ban them | Require verified purchase, simple signup |
| Incentivized reviews (discount for review) | Boosts review volume fast | FTC compliance nightmare, biases ratings upward, damages trust | Post-purchase email campaigns, patience |
| Real-time review moderation UI | "Catch bad reviews instantly" | Human moderation doesn't scale, creates bottleneck, legal risks if selective | Automated profanity filter + flag/report system |
| Public wishlist by default | "Social proof drives sales" | Privacy violation perception—users think it's "greedy" (NN/g research) | Private by default, opt-in sharing |
| Unlimited review length | "Let customers express fully" | Review spam, SEO manipulation, poor UX (no one reads novels) | 500 min, 5000 max characters with validation |
| Review editing after submission | "Users make typos" | Review manipulation after purchase disputes, trust issues | Show "edited" badge if allowed, or disallow entirely |
| Wishlist notification spam | "Keep users engaged" | Unsubscribes spike, brand damage, every price fluctuation triggers email | Smart throttling: max 1 alert/week per item, batch notifications |

## Feature Dependencies

```
[User Profile]
    └──requires──> [Authentication] (✓ exists: Keycloak)
    └──enables──> [Product Reviews]
                      └──requires──> [Order History] (✓ exists)
                      └──enables──> [Verified Purchase Badge]
    └──enables──> [Wishlists]
                      └──enhances──> [Shopping Cart] (✓ exists)

[Address Book]
    └──requires──> [User Profile]
    └──enhances──> [Checkout] (✓ exists)
    └──reduces──> [Cart Abandonment]

[Guest-to-Auth Migration]
    └──requires──> [User Profile]
    └──requires──> [Guest Cart] (✓ exists: BuyerIdentity)
    └──requires──> [Order History] (✓ exists)

[Verified Purchase Badge]
    └──requires──> [Order History] (✓ exists)
    └──requires──> [Product Reviews]
    └──conflicts──> [Anonymous Reviews] (anti-feature)

[Review Helpfulness Voting]
    └──requires──> [Product Reviews]
    └──optional──> [User Profile] (can allow guest voting, but prevents manipulation if required)

[Wishlist Stock Alerts]
    └──requires──> [Wishlists]
    └──requires──> [Inventory Events] (✓ exists: stock reservation system)
    └──requires──> [Notification System] (new)

[Wishlist Price Drop Alerts]
    └──requires──> [Wishlists]
    └──requires──> [Price History Tracking] (new)
    └──requires──> [Background Jobs] (new)
    └──requires──> [Notification System] (new)
```

### Dependency Notes

- **User Profile requires Authentication:** Already satisfied—Keycloak integrated, JWT validation working.
- **Product Reviews require Order History:** Verified purchase badge needs order-product association check.
- **Guest-to-Auth Migration is critical path:** Users signing up after guest checkout expect data persistence—UX failure if cart/orders vanish.
- **Wishlists enhance Shopping Cart:** "Move to cart" is primary conversion path from wishlist.
- **Review voting optional auth:** Guest voting increases participation but enables manipulation; require auth for quality.
- **Notification system is cross-cutting:** Needed for review responses, wishlist alerts, future features—build once, reuse.
- **Price alerts are complex:** Require background job infrastructure (Hangfire/Quartz), price history table, throttling logic.

## MVP Definition

### Launch With (v1.1)

Minimum viable product for user accounts, reviews, and wishlists.

- **User Profile Management**
  - [ ] Display name & avatar upload — Identity foundation
  - [ ] Address book CRUD (add, edit, delete, set default) — Reduces checkout friction
  - [ ] Link existing orders to authenticated users — "My account" completeness
  - [ ] Guest-to-auth migration on signup/login — Preserve cart & order history

- **Product Reviews**
  - [ ] Star rating (1-5) + written review submission — Core review functionality
  - [ ] One review per product per user enforcement — Prevents spam
  - [ ] Verified purchase badge — Trust signal (Amazon weights 10x heavier)
  - [ ] Review display on product pages — Influences 93% of purchases
  - [ ] Average rating calculation & display — Quick trust signal
  - [ ] Basic profanity filter — Automated moderation

- **Wishlists**
  - [ ] Single wishlist per user — Simplicity over power-user features
  - [ ] Add/remove products — Core CRUD
  - [ ] Wishlist page with product grid — Dedicated view
  - [ ] Move to cart action — Primary conversion path
  - [ ] Visual indicator on product cards — Prevent duplicate adds

### Add After Validation (v1.2+)

Features to add once core is working and usage patterns emerge.

- [ ] Review helpfulness voting — Add when review volume > 10/product (signal-to-noise issue)
- [ ] Review sorting/filtering (date, rating, verified) — Add when reviews > 20/product
- [ ] Review image uploads — Add after moderation workflow validated
- [ ] Wishlist sharing (read-only link) — Add if gift registry requests appear
- [ ] Multiple wishlists — Add if user research shows organization need
- [ ] Admin review response — Add when support team requests engagement tool

### Future Consideration (v2+)

Features to defer until product-market fit and resource availability.

- [ ] Wishlist price drop alerts — Requires background job infrastructure, notification system, price history
- [ ] Wishlist stock alerts — Requires notification infrastructure
- [ ] AI review summary — Requires LLM integration, cost analysis, caching strategy
- [ ] Review media lightbox — Nice-to-have UX polish
- [ ] Review question/answer section — Separate feature, large scope

## Feature Prioritization Matrix

| Feature | User Value | Implementation Cost | Priority |
|---------|------------|---------------------|----------|
| User profile (name, avatar) | HIGH | LOW | P1 |
| Address book | HIGH | MEDIUM | P1 |
| Guest-to-auth migration | HIGH | MEDIUM | P1 |
| Star rating + review text | HIGH | LOW | P1 |
| Verified purchase badge | HIGH | MEDIUM | P1 |
| Review display on product page | HIGH | LOW | P1 |
| Average rating summary | HIGH | LOW | P1 |
| Wishlist add/remove | HIGH | LOW | P1 |
| Wishlist page | HIGH | MEDIUM | P1 |
| Move to cart | HIGH | LOW | P1 |
| Review helpfulness voting | MEDIUM | MEDIUM | P2 |
| Review image upload | MEDIUM | HIGH | P2 |
| Review sorting/filtering | MEDIUM | LOW | P2 |
| Admin review response | MEDIUM | MEDIUM | P2 |
| Multiple wishlists | LOW | MEDIUM | P2 |
| Wishlist sharing | MEDIUM | MEDIUM | P2 |
| Wishlist price alerts | MEDIUM | HIGH | P3 |
| Wishlist stock alerts | MEDIUM | MEDIUM | P3 |
| AI review summary | LOW | HIGH | P3 |

**Priority key:**
- P1: Must have for v1.1 launch — core functionality, table stakes
- P2: Should have, add in v1.2+ — enhances core, user requests validate
- P3: Nice to have, future consideration — complex, requires infrastructure

## Competitor Feature Analysis

| Feature | Amazon | Shopify (default) | Our Approach (v1.1) |
|---------|--------|-------------------|---------------------|
| User profiles | Name, avatar, payment methods | Basic account info | Name, avatar, address book (focused scope) |
| Address book | Full CRUD, default, nicknames | Saved addresses | Full CRUD, default address, no nicknames (v1) |
| Verified purchase | Verified badge, weighted 10x | Varies by app (Yotpo, Judge.me) | Verified badge, order history cross-check |
| Review submission | Star + text + images | Star + text (apps add images) | Star + text (images deferred to v1.2) |
| Review voting | Helpful/Not helpful | Plugin-dependent | Defer to v1.2 (needs review volume first) |
| Review moderation | Automated + manual | Plugin-dependent | Automated profanity filter only (v1) |
| Wishlists | Single list, shareable | App-based (multiple lists common) | Single list, private (sharing v1.2+) |
| Wishlist alerts | Stock alerts, Lightning Deals | App-based (price/stock) | Defer to v2 (needs notification infra) |
| Guest migration | Seamless on login | Account merge prompts | Automatic merge on first auth after guest session |

## Implementation Notes

### Existing Assets to Leverage

**Already Built (v1.0):**
- Keycloak authentication (backend JWT + NextAuth.js frontend)
- Guest cart with BuyerIdentity cookie system
- Order history with status tracking
- Product catalog with images
- Inventory system with stock tracking
- MassTransit messaging infrastructure
- PostgreSQL per-feature databases

**Integration Points:**
- **User Profile DB:** New `Users` feature with `UserProfile`, `Address` entities
- **Reviews DB:** New `Reviews` feature with `ProductReview`, `ReviewVote` entities (vote deferred)
- **Wishlist DB:** New `Wishlists` feature with `Wishlist`, `WishlistItem` entities
- **Link to existing:** Orders (via UserId), Products (via ProductId), Inventory (for stock checks)

### Technical Considerations

**User Profile:**
- **Avatar storage:** Azure Blob Storage or local file system (dev), size limits (2MB), format validation (jpg, png, webp)
- **Address validation:** Consider address verification API (SmartyStreets, Google) or basic format validation
- **Guest migration:** On signup/login, check for guest BuyerId cookie → migrate cart + orders → clear cookie

**Product Reviews:**
- **Verified purchase logic:** `SELECT COUNT(*) FROM Orders WHERE UserId = @UserId AND OrderItems.ProductId = @ProductId AND Status = 'Completed'`
- **One review per user-product:** Unique index on `(UserId, ProductId)`
- **Rating aggregation:** Cached computed column or materialized view, recalculate on review add/update/delete
- **Profanity filter:** Library like `ProfanityFilter.NET` or simple regex blacklist

**Wishlists:**
- **Single list simplification:** `Wishlist` table with `UserId` (1:1), `WishlistItems` join table (many-to-many with Products)
- **Stock check on display:** Join with Inventory to show "Out of Stock" on wishlist page
- **Move to cart:** Copy `WishlistItem.ProductId` to `CartItem`, delete from wishlist

### Data Model Sketch

```csharp
// Users feature
public class UserProfile : Entity<UserId>
{
    public string DisplayName { get; private set; }
    public string? AvatarUrl { get; private set; }
    public string Email { get; private set; } // from Keycloak
    public DateTime CreatedAt { get; private set; }
    private readonly List<Address> _addresses = [];
    public IReadOnlyCollection<Address> Addresses => _addresses.AsReadOnly();
}

public class Address : Entity<AddressId>
{
    public UserId UserId { get; private set; }
    public string FullName { get; private set; }
    public string Line1 { get; private set; }
    public string? Line2 { get; private set; }
    public string City { get; private set; }
    public string State { get; private set; }
    public string PostalCode { get; private set; }
    public string Country { get; private set; }
    public bool IsDefault { get; private set; }
}

// Reviews feature
public class ProductReview : Entity<ReviewId>
{
    public ProductId ProductId { get; private set; }
    public UserId UserId { get; private set; }
    public int Rating { get; private set; } // 1-5
    public string ReviewText { get; private set; }
    public bool IsVerifiedPurchase { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    // Future: HelpfulVotes, UnhelpfulVotes, AdminResponse
}

// Wishlists feature
public class Wishlist : Entity<WishlistId>
{
    public UserId UserId { get; private set; }
    public DateTime CreatedAt { get; private set; }
    private readonly List<WishlistItem> _items = [];
    public IReadOnlyCollection<WishlistItem> Items => _items.AsReadOnly();
}

public class WishlistItem : Entity<WishlistItemId>
{
    public WishlistId WishlistId { get; private set; }
    public ProductId ProductId { get; private set; }
    public DateTime AddedAt { get; private set; }
}
```

## Sources

**E-commerce User Profile Best Practices:**
- [60+ Best Profile page Top 2026 Design Patterns | Muzli](https://muz.li/inspiration/profile-page/)
- [The Ultimate Ecommerce Website Checklist For 2026 | Limely](https://www.limely.co.uk/blog/the-ultimate-ecommerce-website-checklist-for-2026)
- [15 Must-Have E-commerce Features for 2026 (and How to Build Them)](https://www.sctinfo.com/blog/build-a-e-commerce-website/)
- [Ecommerce User Experience: Complete Guide (2025)](https://www.parallelhq.com/blog/ecommerce-user-experience)

**Product Review Systems:**
- [11 Best Product Review Software for Ecommerce (2026)](https://wiserreview.com/blog/product-review-software/)
- [Amazon Customer Reviews & Ratings](https://www.amazon.com/gp/help/customer/display.html?nodeId=G8UYX7LALQC8V9KA)
- [Verified Purchaser | Bazaarvoice](https://developers.bazaarvoice.com/v1.0-ConversationsAPI/docs/verified-purchaser)
- [Do Verified Badges on Reviews Boost Shopper Confidence?](https://en.verified-reviews.com/blog/verified-badges-reviews/)

**Review Moderation & Voting:**
- [Moderating content: Why it matters and how to do it | Bazaarvoice](https://www.bazaarvoice.com/blog/moderating-content-tips-and-best-practices/)
- [What Is Product Review Moderation? | Yotpo](https://www.yotpo.com/glossary/product-review-moderation/)
- [How product review voting is influenced | ScienceDirect](https://www.sciencedirect.com/science/article/abs/pii/S0167923623000568)

**Wishlist Features:**
- [15 Ways a Wishlist Can Boost Your E-commerce Strategy](https://www.getswym.com/blog/15-ways-a-wishlist-can-boost-your-e-commerce-strategy)
- [Use These E-Commerce Wishlist Examples to Increase Revenue](https://www.drip.com/blog/e-commerce-wishlist-examples)
- [Wishlist or shopping cart? Saving products for later | NN/G](https://www.nngroup.com/articles/wishlist-or-cart/)
- [What is a WishList and Why It is an Important in Ecommerce?](https://devrims.com/blog/ecommerce-wishlist/)
- [Understand wish list features - Oracle Commerce](https://docs.oracle.com/en/cloud/saas/cx-commerce/uoccs/understand-wish-list-features.html)

**Address Book UX:**
- [713 'Address Book' Design Examples | Baymard](https://baymard.com/ecommerce-design-examples/59-address-book)
- [Commerce Addressbook | Drupal.org](https://www.drupal.org/project/commerce_addressbook)

**Review & Rating System Pitfalls:**
- [Ratings and Reviews in E-Commerce: A Guide for Brands](https://metricscart.com/insights/ratings-and-reviews-in-e-commerce/)
- [A Complete Guide to eCommerce Reviews Management | Sendlane](https://www.sendlane.com/blog/manage-ecommerce-reviews)
- [What's Really Causing Bad Reviews for Ecommerce Businesses? | Veeqo](https://www.veeqo.com/blog/bad-reviews-ecommerce-businesses)

---
*Feature research for: E-commerce User Accounts, Product Reviews, and Wishlists*
*Researched: 2026-02-13*
