# Pitfalls Research

**Domain:** E-commerce platform adding user profiles, product reviews, and wishlists to existing modular monolith
**Researched:** 2026-02-13
**Confidence:** MEDIUM

## Critical Pitfalls

### Pitfall 1: Guest-to-Authenticated Cart/Order Migration Race Condition

**What goes wrong:**
When a user logs in after browsing as a guest, their cookie-based cart (BuyerId = guest GUID) needs to merge with their authenticated account (BuyerId = Keycloak sub claim). Without proper synchronization, concurrent requests during login can result in:
- Duplicate carts (guest cart retained, new empty authenticated cart created)
- Lost cart items (guest cart deleted before merge completes)
- Partial migrations (some items copied, then interrupted)
- Order history orphaned (guest orders not linked to authenticated account)

The existing `BuyerIdentity.GetOrCreateBuyerId()` immediately switches from cookie to claim on authentication, but there's no migration logic. Orders table stores `BuyerId` as raw GUID with no link to user profile.

**Why it happens:**
The code switches identity sources (cookie → claim) without coordinating data ownership. The cart uses `Guid BuyerId` which is overloaded to mean both "guest session" and "authenticated user ID". When Keycloak sub claim appears, the system treats it as a new buyer, abandoning the guest session's data.

**How to avoid:**
1. Create a `MigrateGuestDataCommand` triggered on first authenticated request per session
2. Use PostgreSQL advisory locks during migration (`pg_advisory_xact_lock(hashtext(guest_guid))`)
3. Query both CartDbContext and OrderingDbContext for guest BuyerId records
4. Update `BuyerId` atomically within a distributed transaction or saga
5. Only delete guest cookie after successful migration verification
6. Handle idempotency: if migration already happened (cart exists for authenticated ID), merge items instead of failing

**Warning signs:**
- Authenticated users report empty carts after login
- "Lost my cart" support tickets correlate with login events
- Database contains multiple carts with same items but different BuyerIds
- Order history page shows guest orders disappearing after account creation

**Phase to address:**
Phase 1 (User Profiles & Authentication Flow) — must implement migration before user profiles launch, as this affects existing cart/order data.

---

### Pitfall 2: Cross-Context Query Hell for Verified Purchase Reviews

**What goes wrong:**
Displaying reviews requires joining data from 4 separate DbContexts:
1. **ReviewDbContext**: Review text, rating, helpful votes
2. **CatalogDbContext**: Product name, image (for review cards)
3. **OrderingDbContext**: Verify purchase (is reviewer a real buyer?)
4. **UserProfileDbContext**: Reviewer name, avatar, badge (new context)

Naive approaches cause N+1 queries, cartesian explosion, or inconsistent data:
- Load reviews, then loop fetching product/user/order data = 100+ queries for 20 reviews
- Perform JOIN across contexts via in-memory LINQ = loads entire tables into memory
- Use separate queries with eventual consistency = reviews show "Verified Buyer" before order confirmation event propagates

CQRS pattern says "define a view database for queries," but [Microsoft's documentation warns](https://learn.microsoft.com/en-us/dotnet/architecture/microservices/microservice-ddd-cqrs-patterns/cqrs-microservice-reads) you can't enlist message brokers and databases into a single distributed transaction, creating synchronization challenges.

**Why it happens:**
Database-per-feature isolation (correct DDD) collides with read-heavy UI requirements (product pages need aggregated review data). Developers reach for EF Core's familiar LINQ and accidentally query across boundaries. [Martin Fowler warns](https://www.martinfowler.com/bliki/CQRS.html) "CQRS should only be used on specific portions of a system (a BoundedContext in DDD lingo) and not the system as a whole," but reviews inherently need data from multiple contexts.

**How to avoid:**
1. Create a **read model projection** (`ProductReviewSummary` table in ReviewDbContext):
   - Subscribe to `OrderConfirmedEvent`, `ProductCreatedEvent`, `UserProfileCreatedEvent`
   - Denormalize: store product name, user display name, avatar URL, verified purchase flag
   - Update via event handlers, not in review write path
2. Query read model for display, use write model for mutations
3. Accept eventual consistency: "Verified Buyer" badge appears ~100ms after order confirmation
4. Implement health checks: alert if event lag exceeds 5 seconds

**Warning signs:**
- Product page load time scales linearly with review count (N+1 queries)
- Database CPU spikes when users view popular products
- EF Core query logs show `Include()` chains across multiple DbContexts
- "Verified Buyer" badge disappears/reappears inconsistently

**Phase to address:**
Phase 2 (Product Reviews & Ratings) — design read model upfront, before implementing review display.

---

### Pitfall 3: Keycloak Profile Data Duplication (Auth vs Application Domain)

**What goes wrong:**
Developers duplicate user profile data across Keycloak custom attributes and application UserProfileDbContext:
- **Keycloak**: `custom_attribute.display_name`, `custom_attribute.avatar_url`
- **Application DB**: `user_profiles.display_name`, `user_profiles.avatar_url`

Changes to one don't propagate to the other. Users update their name in the app, but JWTs still contain the old value (Keycloak hasn't synced). Worse, [Keycloak v24+ enforces declarative user profile schemas](https://medium.com/@saissv2398/keycloak-user-attributes-what-no-one-tells-you-about-version-differences-v21-1-1-vs-v24-876118a11a43), reducing flexibility for raw key-value attributes. [Security concerns](https://www.baeldung.com/keycloak-custom-user-attributes) mean only explicitly configured attributes are "manageable" by default.

Avatar URLs stored in Keycloak custom attributes create another problem: Keycloak doesn't manage file storage, so the URL points to your application's blob storage, creating a circular dependency.

**Why it happens:**
Confusion between "identity claims" (authentication, in JWT) vs "application profile data" (business logic, in database). Developers see Keycloak has a user model and assume it should be the single source of truth for all user data, but [Keycloak's official guidance](https://www.keycloak.org/docs/latest/server_admin/index.html) shows custom attributes work best for identity-related data, not complex business objects.

**How to avoid:**
1. **Keycloak stores**: Authentication-critical data only
   - `sub` (user ID, immutable)
   - `email`, `email_verified`
   - `preferred_username` (login identifier)
2. **UserProfileDbContext stores**: Application-specific data
   - Display name, bio, avatar URL
   - Preference flags (newsletter opt-in, theme)
   - Business logic attributes (loyalty tier, seller status)
3. **Profile sync strategy**:
   - On first login, create UserProfile record with `sub` as foreign key
   - Application queries UserProfileDbContext for display data
   - If Keycloak user updates email via account console, sync to UserProfile via webhook
4. **JWT claims**: Include only `sub` + `email` + `roles`. App fetches rest from database.

**Warning signs:**
- User profile updates require restarting Keycloak or clearing sessions
- Avatar images return 404 because URL in JWT is stale
- Profile data differs between pages (some read Keycloak claims, some read database)
- Unit tests mock 20+ claim values to simulate authenticated users

**Phase to address:**
Phase 1 (User Profiles & Authentication Flow) — establish the pattern before adding reviews/wishlists that reference user data.

---

### Pitfall 4: Review Spam Without Verified Purchase Enforcement

**What goes wrong:**
Product reviews accept any authenticated user, without verifying they actually purchased the product. Result:
- Competitors post fake 1-star reviews
- Sellers create sockpuppet accounts for fake 5-star reviews
- Users review products they've never used based on unboxing videos

Research shows [30-35% of online reviews are spam](https://www.mdpi.com/2076-3417/9/5/987), and [fake reviews on dark web forums start at £4 per review](https://almcorp.com/blog/google-reviews-deleted-ai-legal-takedowns-2025/). Without moderation, automated detection is the only defense, but it's complex (behavioral patterns, ML models, real-time transaction verification).

**Why it happens:**
Developers implement the "write a review" feature first, deferring verification as "phase 2 hardening." By then, fake reviews pollute the dataset, making cleanup impossible (which are real?). The database schema lacks a foreign key between Reviews and Orders, so the verification query isn't enforced.

**How to avoid:**
1. **Database constraint**: Add `OrderId` foreign key to Reviews table (nullable for backwards compatibility, but require for new reviews)
2. **Write operation**: `CreateReviewCommand` requires `OrderId`, validate via query:
   ```csharp
   var order = await orderingDbContext.Orders
       .Where(o => o.BuyerId == userId && o.Items.Any(i => i.ProductId == productId) && o.Status == OrderStatus.Delivered)
       .FirstOrDefaultAsync();
   if (order == null) throw new ValidationException("Must purchase product before reviewing");
   ```
3. **Once-per-order**: `HasReviewedOrderItem(userId, productId, orderId)` check prevents duplicate reviews for same purchase
4. **UI indicator**: "Verified Purchase" badge pulled from `Reviews.OrderId IS NOT NULL`
5. **Delayed review window**: Allow reviews 24 hours after order delivery (prevents immediate competitor spam)

**Warning signs:**
- Product rating distribution is "I-shaped" (all 5-star or all 1-star) instead of natural "J-shaped"
- New products get 50+ reviews within hours of launch
- Accounts with single purchase review 100+ other products
- Review text is generic template ("Great product! Highly recommend!!!1!")

**Phase to address:**
Phase 2 (Product Reviews & Ratings) — enforce on day 1, not "later." Schema must include OrderId from the start.

---

### Pitfall 5: Wishlist-to-Cart Bounded Context Violation

**What goes wrong:**
"Add Wishlist to Cart" button requires moving items from WishlistDbContext to CartDbContext. Naive implementation:
1. Load wishlist items (ProductId, quantity)
2. Loop: call `AddToCartCommand` for each item
3. Delete wishlist

Problems:
- **Partial failure**: Item 3 fails (out of stock), but items 1-2 already added to cart, wishlist fully deleted
- **Inconsistent pricing**: Wishlist stores snapshot price, cart fetches current price from Catalog — user sees different total
- **Inventory race**: Between wishlist load and cart add, last item is purchased by another user
- **No rollback**: Cart commands succeed, then wishlist delete fails — items duplicated

[DDD integration patterns](https://medium.com/ssense-tech/ddd-beyond-the-basics-mastering-multi-bounded-context-integration-ca0c7cec6561) recommend anti-corruption layers, but [Reformed Programmer warns](https://www.thereformedprogrammer.net/evolving-modular-monoliths-3-passing-data-between-bounded-contexts/) "too many tight interactions may be a sign of high coupling."

**Why it happens:**
Wishlist and Cart feel like related features, so developers reuse commands/queries across boundaries. The shared `ProductId` tempts direct coupling instead of event-driven coordination.

**How to avoid:**
1. **Saga pattern**: `MoveWishlistToCartSaga` coordinates the workflow
   ```
   Start → ValidateInventory → AddItemsToCart → ClearWishlist → Complete
   ```
2. **Compensating transactions**: If cart add fails, saga doesn't reach delete step
3. **UI feedback**: Partial success states
   - "3 of 5 items added (2 out of stock)"
   - "Added to cart, keeping in wishlist until checkout"
4. **Alternative design**: Wishlist items stay in wishlist, cart has "from wishlist" flag — delete wishlist items on order confirmation, not on cart add

**Warning signs:**
- Users report "wishlist disappeared but cart is empty"
- Cart total doesn't match wishlist subtotal shown before migration
- Database logs show orphaned wishlist records with no corresponding items
- Error logs show `AddToCartCommand` failures but no rollback of wishlist deletions

**Phase to address:**
Phase 3 (Wishlists & Saved Items) — design the integration pattern before implementing the "add all to cart" feature.

---

### Pitfall 6: Avatar Storage in Ephemeral Container Filesystem

**What goes wrong:**
User uploads avatar image, code saves to `/app/uploads/avatars/{userId}.jpg` inside the container. File persists until:
- Container restarts (lost on deploy)
- Horizontal scaling adds second instance (avatar only exists on instance A, requests to instance B return 404)
- Aspire `docker compose down` (development data lost)

Database stores `avatar_url = "/uploads/avatars/123.jpg"`, but file is gone. All profile images break.

**Why it happens:**
Developers familiar with traditional web hosting (persistent `/var/www/html`) assume containerized apps work the same way. [Container data typically disappears once the instance shuts down](https://www.aquasec.com/cloud-native-academy/docker-container/microservices-and-containerization/), requiring external storage mechanisms.

**How to avoid:**
1. **External blob storage**: Azure Blob Storage, AWS S3, MinIO (self-hosted S3-compatible)
2. **Development**: Add MinIO service to Aspire AppHost
   ```csharp
   builder.AddContainer("minio", "minio/minio")
       .WithEnvironment("MINIO_ROOT_USER", "minioadmin")
       .WithEnvironment("MINIO_ROOT_PASSWORD", "minioadmin")
       .WithBindMount("./data/minio", "/data")
       .WithArgs("server", "/data");
   ```
3. **Production**: Use managed blob storage with CDN
4. **Database**: Store full URL including bucket (`avatar_url = "https://cdn.example.com/avatars/123.jpg"`)
5. **Cleanup policy**: Delete orphaned images when user account is deleted

**Warning signs:**
- Avatar images break after deployment
- "File not found" errors in production logs
- `/app/uploads` directory grows unbounded (no cleanup)
- Different users see different avatars (load balancer round-robin to different instances)

**Phase to address:**
Phase 1 (User Profiles & Authentication Flow) — set up blob storage before implementing avatar uploads.

---

### Pitfall 7: Review Helpfulness Ranking Creates Recency Bias

**What goes wrong:**
Reviews are sorted by "helpful votes" (descending). Older reviews accumulate votes over months, newer reviews stay buried at the bottom even if more relevant. Example:
- Review from 2023: "Great product" (150 helpful votes) — product version changed since
- Review from 2026: "Improved version fixes main issue" (2 helpful votes) — actually more useful, but invisible

[Research shows](https://arxiv.org/pdf/1901.06274) "older reviews have an advantage because they accumulated helpfulness votes over time, while recent reviews may not be considered good for not having enough votes." Additionally, [non-voted reviews are often overlooked](https://www.sciencedirect.com/science/article/abs/pii/S0952197623012599), even though they might be highly relevant.

**Why it happens:**
Sorting by raw vote count is simple but time-biased. Algorithms that account for review age, vote velocity, and verified purchase status are complex, so MVPs skip them.

**How to avoid:**
1. **Composite score** instead of raw votes:
   ```
   Score = (helpful_votes / (helpful_votes + unhelpful_votes + 1))
         × log(1 + total_votes)
         × recency_multiplier
         × verified_purchase_bonus
   ```
   - **Wilson score** for vote ratio (handles small sample sizes)
   - **Log scale** prevents old reviews from dominating
   - **Recency multiplier**: Reviews <30 days get 1.5x, 30-90 days 1.2x, >90 days 1.0x
   - **Verified purchase bonus**: +0.2 to score
2. **Default sort**: "Most Relevant" (composite score), with manual sort options (newest, highest rated, most helpful)
3. **Highlighted**: Auto-promote "helpful recent reviews" to top (score >X, created <60 days)

**Warning signs:**
- All top reviews are >6 months old
- Product update released, but top reviews reference old version
- Conversion rate drops (users reading outdated negative reviews)
- New reviewers complain "no one votes on my review"

**Phase to address:**
Phase 2 (Product Reviews & Ratings) — implement in initial ranking query, not post-launch fix.

---

### Pitfall 8: Domain Event Eventual Consistency UX Breakdown

**What goes wrong:**
User submits review. Review write succeeds, but the read model projection lags:
1. ReviewWriteDbContext commits review (tx committed)
2. Browser redirects to product page
3. ProductReviewSummaryQuery executes against read model (still processing)
4. User sees "No reviews yet" or old review count
5. User refreshes → review appears

Same problem with verified purchase verification:
1. Order status changes to Delivered
2. `OrderDeliveredEvent` published
3. Review service subscribes, updates `can_review` flag
4. User immediately tries to review → "Must purchase product first" error
5. 2 seconds later, works

[Microsoft's CQRS docs](https://learn.microsoft.com/en-us/dotnet/architecture/microservices/microservice-ddd-cqrs-patterns/domain-events-design-implementation) note "if you commit changes to the original aggregate and afterwards, when the events are being dispatched, if there's an issue, you'll have inconsistencies between aggregates."

**Why it happens:**
Eventual consistency is inherent to CQRS + event-driven architecture, but UX doesn't account for lag. Developers test locally (events process in <10ms) and ship, production has 200ms+ lag under load.

**How to avoid:**
1. **Optimistic UI updates**: After review submit, inject new review into client-side state before API response
2. **Polling fallback**: If review not visible after 3s, poll read model every 500ms (max 5 attempts)
3. **Event lag monitoring**: Alert if `DomainEventPublisher` lag >1 second
4. **Inline read for write user**: `CreateReviewCommand` returns full `ReviewDto`, UI renders that instead of querying read model
5. **Health check**: Canary event published every 10s, alarm if not consumed within 2s

**Warning signs:**
- "My review disappeared" support tickets spike
- Users refresh page multiple times after posting review
- Metrics show high "view product immediately after review" rate with no review visible
- MassTransit dead letter queue accumulates events

**Phase to address:**
Phase 2 (Product Reviews & Ratings) — test with artificial event delay (Thread.Sleep(2000)) during development.

---

## Technical Debt Patterns

Shortcuts that seem reasonable but create long-term problems.

| Shortcut | Immediate Benefit | Long-term Cost | When Acceptable |
|----------|-------------------|----------------|-----------------|
| Store avatar as base64 in database | No blob storage setup | Database bloat, slow queries, no CDN caching | Never — avatar images 50KB-5MB each |
| Allow unverified reviews "for now" | Faster MVP launch | Fake review pollution, impossible cleanup, reputation damage | Never — schema must include OrderId from day 1 |
| Use Keycloak custom attributes for profile data | Single source of truth | Sync nightmares, schema inflexibility, JWT bloat | Only for `email`, `preferred_username` — everything else in app DB |
| Skip guest-to-auth migration | Simpler login flow | Lost carts, orphaned orders, angry users | Never — migration required before user profiles launch |
| Sort reviews by newest first | No algorithm needed | Buries helpful reviews, reduces conversion | Acceptable for <50 reviews per product, fix before scaling |
| Synchronous cross-context queries (JOIN) | Familiar LINQ patterns | N+1 queries, memory explosion, coupling | Never in production — use read model projections |
| Store wishlist item prices | Faster subtotal display | Stale prices confuse users | Acceptable if UI shows "Price may have changed" warning |
| Manual review moderation | No ML infrastructure | Doesn't scale, spam overwhelms moderators | Acceptable for <100 reviews/day, automate before growth |

---

## Integration Gotchas

Common mistakes when connecting to external services.

| Integration | Common Mistake | Correct Approach |
|-------------|----------------|------------------|
| **Keycloak JWT** | Trusting claims without validation | Verify signature, issuer, audience, expiration — use `Microsoft.AspNetCore.Authentication.JwtBearer` built-in validation |
| **Keycloak user sync** | Polling `/admin/users` API every 5 min | Subscribe to Keycloak event webhooks (`USER_UPDATED`, `USER_DELETED`) via custom Event Listener SPI |
| **Blob storage URLs** | Storing relative paths (`/avatars/123.jpg`) | Store absolute URLs with CDN (`https://cdn.example.com/avatars/123.jpg`), support URL migration when CDN changes |
| **MassTransit events** | Fire-and-forget publish without correlation ID | Always include `CorrelationId`, enable OpenTelemetry tracing, use Inbox/Outbox pattern for transactional guarantees |
| **PostgreSQL locks** | Use table-level locks for cart migration | Use advisory locks `pg_advisory_xact_lock()` scoped to specific BuyerId GUID hash |
| **EF Core migrations** | Share DbContext across features | Each feature owns its DbContext, migrations run independently, use separate connection strings or schemas |

---

## Performance Traps

Patterns that work at small scale but fail as usage grows.

| Trap | Symptoms | Prevention | When It Breaks |
|------|----------|------------|----------------|
| **Loading full wishlist in single query** | Query time scales linearly with items | Paginate: load 10 items, lazy-load rest on scroll | >50 items per wishlist |
| **Counting review helpful votes on every page load** | `COUNT(*)` query on reviews_helpful_votes table | Denormalize: store `helpful_count` column, update via event | >500 votes per review |
| **Fetching user avatars in foreach loop** | N+1 queries for 20 reviews = 21 DB queries | Batch query: `userIds.ToList()` → `IN (...)` query, or use read model with denormalized avatars | >10 reviews per page |
| **Synchronous blob upload during avatar change** | HTTP request blocks for 2-5 seconds | Background job: save to temp storage, enqueue upload, return immediately | Files >500KB |
| **Broadcasting domain events to all subscribers** | MassTransit overhead scales with subscriber count | Topic-based routing: reviews only subscribe to `OrderDeliveredEvent`, not all order events | >5 subscribers per event type |
| **Recalculating "Top Reviewed Products" on every dashboard load** | Full table scan of reviews, GROUP BY product | Materialized view or scheduled cache refresh (every 5 min) | >10,000 reviews |

---

## Security Mistakes

Domain-specific security issues beyond general web security.

| Mistake | Risk | Prevention |
|---------|------|------------|
| **Exposing internal BuyerId GUIDs in review APIs** | Attacker correlates anonymous reviews to users, de-anonymizes buyers | Return `reviewer_display_name` and `avatar_url`, never BuyerId |
| **No rate limiting on review submission** | Single user spams 1000 fake reviews | Rate limit: 1 review per product per user, 5 reviews per day per user, CAPTCHA after 3 |
| **Trusting client-side OrderId in `CreateReviewCommand`** | Attacker claims fake purchase, forges "Verified Buyer" badge | Server validates `OrderId` ownership: `order.BuyerId == currentUserId && order.Status == Delivered` |
| **Avatar upload accepts all file types** | User uploads `avatar.php`, executes code | Validate MIME type (image/jpeg, image/png), strip EXIF metadata, resize to fixed dimensions, rename with GUID |
| **Review helpful votes allow unlimited votes** | Bots upvote fake reviews to front page | 1 vote per user per review (unique constraint on `user_id + review_id`), rate limit 10 votes/hour per user |
| **Guest cart cookie not HttpOnly** | XSS attack steals buyer_id cookie, hijacks cart | Use `HttpOnly = true, Secure = true, SameSite = Lax` (already implemented in BuyerIdentity.cs) |

---

## UX Pitfalls

Common user experience mistakes in this domain.

| Pitfall | User Impact | Better Approach |
|---------|-------------|-----------------|
| **Silent guest cart abandonment on login** | "Where did my cart go?" confusion | Show migration progress toast: "Moving 3 items from guest cart..." |
| **Review submitted → redirected to product page → review not visible** | "Did it work? Should I submit again?" | Show success banner: "Review submitted, may take a few seconds to appear" OR optimistically inject review into page |
| **Wishlist "Add All to Cart" fails on item 5 of 20** | User doesn't know which items failed, manually re-adds all 20 | Show partial success: "Added 4 items. 1 item out of stock: [Product Name]" with retry button |
| **Avatar upload fails silently** | User thinks avatar changed, but still sees old one | Show upload progress bar, validation errors ("File too large"), preview before save |
| **"Verified Purchase" badge with no explanation** | Users don't trust it (what does it mean?) | Tooltip: "This reviewer purchased this product" |
| **Review sorting defaults to "Newest"** | Least helpful reviews shown first | Default to "Most Relevant" (composite score), allow manual sort |
| **Wishlist items show stale prices** | User adds to cart, shocked by real price | Show "Price may have changed" if wishlist item >7 days old, refresh prices on "Add to Cart" |

---

## "Looks Done But Isn't" Checklist

Things that appear complete but are missing critical pieces.

- [ ] **Guest-to-Auth Migration:** Often missing race condition handling — verify advisory locks implemented, test with concurrent login requests
- [ ] **Review Verified Purchase:** Often missing OrderId foreign key — verify database schema includes non-nullable OrderId, query validates order ownership
- [ ] **Avatar Upload:** Often missing blob storage — verify files NOT saved to container filesystem, test avatar persistence after container restart
- [ ] **Review Read Model:** Often missing event subscription setup — verify MassTransit consumers registered, test projection updates when order delivered
- [ ] **Wishlist to Cart:** Often missing partial failure handling — verify saga compensating transactions, test with intentionally failed inventory check
- [ ] **Review Helpfulness:** Often missing recency weighting — verify score algorithm includes time decay, test that new reviews can outrank old ones
- [ ] **User Profile Sync:** Often missing Keycloak webhook handlers — verify email changes in Keycloak propagate to UserProfileDbContext
- [ ] **Cross-Context Queries:** Often missing read model — verify no direct JOINs between CatalogDbContext and ReviewDbContext in production queries
- [ ] **Avatar Security:** Often missing file type validation — verify MIME type checks, test upload of .exe renamed to .jpg
- [ ] **Event Lag Monitoring:** Often missing health checks — verify alerts configured for domain event processing delays >2 seconds

---

## Recovery Strategies

When pitfalls occur despite prevention, how to recover.

| Pitfall | Recovery Cost | Recovery Steps |
|---------|---------------|----------------|
| **Lost guest carts on login** | LOW | Query orphaned carts (BuyerId = cookie not in user_profiles), match by session timestamps, manual migration script |
| **Fake unverified reviews** | MEDIUM | Add nullable OrderId column, backfill from order history where possible, mark rest as "Legacy Review (Unverified)", implement verification going forward |
| **Avatars stored in container** | MEDIUM | Batch migration: enumerate /app/uploads, upload to blob storage, update avatar_url in database, add blob cleanup cron job |
| **Review read model out of sync** | LOW | Rebuild projection: query ReviewWriteDbContext, denormalize product/user/order data, bulk insert to read model, resume event subscription |
| **Wishlist items duplicated in cart** | LOW | Deduplication query: `DELETE FROM cart_items WHERE id NOT IN (SELECT MIN(id) FROM cart_items GROUP BY cart_id, product_id)` |
| **Keycloak profile data out of sync with app DB** | MEDIUM | One-time sync job: query Keycloak /admin/users, upsert to UserProfileDbContext, establish webhook for future changes |
| **Review rankings stuck with old algorithm** | LOW | Recalculate scores: update `helpful_score = NewAlgorithm(votes, created_at)`, rebuild product review summary, redeploy frontend |
| **Event processing lag accumulation** | HIGH | Scale out MassTransit consumers (add instances), increase partition count if using Azure Service Bus, investigate slow event handlers, add database indexes |

---

## Pitfall-to-Phase Mapping

How roadmap phases should address these pitfalls.

| Pitfall | Prevention Phase | Verification |
|---------|------------------|--------------|
| Guest-to-Auth Migration Race | Phase 1: User Profiles | Integration test: simulate concurrent login + cart add, verify no duplicate/lost carts |
| Cross-Context Query Hell | Phase 2: Reviews | Query profiling: max 3 DB queries per product page load, no N+1 patterns |
| Keycloak Data Duplication | Phase 1: User Profiles | Schema review: UserProfile table exists, Keycloak custom attributes limited to identity data |
| Review Spam (Unverified) | Phase 2: Reviews | Schema constraint: reviews.order_id NOT NULL, validation test rejects fake OrderId |
| Wishlist-to-Cart Violation | Phase 3: Wishlists | Saga test: simulate AddToCart failure, verify wishlist not deleted |
| Avatar Container Storage | Phase 1: User Profiles | Deployment test: restart AppHost, verify avatars still load |
| Review Recency Bias | Phase 2: Reviews | Algorithm test: new review with 5 votes outranks old review with 50 votes |
| Event Consistency UX | Phase 2: Reviews | UX test: submit review, measure time until visible, verify <3 seconds or loading state shown |

---

## Sources

**Guest-to-Authenticated Migration:**
- [E-Commerce Authentication: 2026 Benchmark + Best Practice](https://www.corbado.com/blog/ecommerce-authentication)
- [Passwordless Authentication: The Most Important Ecommerce Upgrade for Secure, High-Converting Stores in 2026](https://www.nopaccelerate.com/passwordless-authentication-ecommerce-2026/)
- [Preventing Postgres SQL Race Conditions with SELECT FOR UPDATE](https://on-systems.tech/blog/128-preventing-read-committed-sql-concurrency-errors/)
- [Using PostgreSQL advisory locks to avoid race conditions](https://firehydrant.com/blog/using-advisory-locks-to-avoid-race-conditions-in-rails/)

**Cross-Context Queries & CQRS:**
- [Implementing reads/queries in a CQRS microservice - .NET | Microsoft Learn](https://learn.microsoft.com/en-us/dotnet/architecture/microservices/microservice-ddd-cqrs-patterns/cqrs-microservice-reads)
- [CQRS - Martin Fowler](https://www.martinfowler.com/bliki/CQRS.html)
- [CQRS Design Pattern in Microservices - GeeksforGeeks](https://www.geeksforgeeks.org/system-design/cqrs-design-pattern-in-microservices/)

**Keycloak Profile Management:**
- [Keycloak User Attributes – What No One Tells You About Version Differences (v21.1.1 vs v24+)](https://medium.com/@saissv2398/keycloak-user-attributes-what-no-one-tells-you-about-version-differences-v21-1-1-vs-v24-876118a11a43)
- [Custom User Attributes with Keycloak | Baeldung](https://www.baeldung.com/keycloak-custom-user-attributes)
- [Managing Keycloak user metadata and custom attributes - Mastertheboss](https://www.mastertheboss.com/keycloak/managing-keycloak-user-metadata-and-custom-attributes/)

**Review Spam Prevention:**
- [The Global Wave of Google Reviews Being Deleted: AI Detection, Legal Takedowns, and What Businesses Must Know Going Into 2026](https://almcorp.com/blog/google-reviews-deleted-ai-legal-takedowns-2025/)
- [Spam Review Detection Techniques: A Systematic Literature Review](https://www.mdpi.com/2076-3417/9/5/987)
- [Optional purchase verification in e-commerce platforms](https://onlinelibrary.wiley.com/doi/abs/10.1111/poms.13731)

**Review Ranking & Helpfulness:**
- [A Survey on E-Commerce Learning to Rank](https://arxiv.org/html/2412.03581v1)
- [Ranking Online Consumer Reviews](https://arxiv.org/pdf/1901.06274)
- [Review helpfulness prediction on e-commerce websites: A comprehensive survey](https://www.sciencedirect.com/science/article/abs/pii/S0952197623012599)

**Bounded Context Integration:**
- [Integrating Bounded Context for DDD Beginners](https://medium.com/@dangeabunea/integrating-bounded-context-for-ddd-beginners-63c21af875fb)
- [DDD Beyond the Basics: Mastering Multi-Bounded Context Integration](https://medium.com/ssense-tech/ddd-beyond-the-basics-mastering-multi-bounded-context-integration-ca0c7cec6561)
- [Evolving modular monoliths: 3. Passing data between bounded contexts](https://www.thereformedprogrammer.net/evolving-modular-monoliths-3-passing-data-between-bounded-contexts/)

**Container Storage:**
- [Microservices and Containerization: Challenges and Best Practices](https://www.aquasec.com/cloud-native-academy/docker-container/microservices-and-containerization/)
- [Mastering Microservices: Top Best Practices for 2026](https://www.imaginarycloud.com/blog/microservices-best-practices)

**Domain Events & Eventual Consistency:**
- [Domain events: Design and implementation - .NET | Microsoft Learn](https://learn.microsoft.com/en-us/dotnet/architecture/microservices/microservice-ddd-cqrs-patterns/domain-events-design-implementation)
- [CQRS and Event Sourcing in Event Driven Architecture of Ordering Microservices](https://medium.com/aspnetrun/cqrs-and-event-sourcing-in-event-driven-architecture-of-ordering-microservices-fb67dc44da7a)

---

*Pitfalls research for: MicroCommerce v1.1 — User Profiles, Reviews, Wishlists*
*Researched: 2026-02-13*
*Confidence: MEDIUM — Web search findings verified against official Microsoft/.NET documentation where possible, some e-commerce-specific patterns based on industry research without direct technical verification*
