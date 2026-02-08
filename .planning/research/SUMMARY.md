# Project Research Summary

**Project:** MicroCommerce v1.1 - User Features (Profiles, Reviews, Wishlists)
**Domain:** E-commerce modular monolith enhancement
**Researched:** 2026-02-13
**Confidence:** HIGH

## Executive Summary

Research confirms that the existing MicroCommerce stack (.NET 10, Aspire 13.1.0, Next.js 16, Keycloak, PostgreSQL, MassTransit) handles 95% of requirements for user accounts, product reviews, and wishlists. Only ONE new backend package is required (SixLabors.ImageSharp 3.1.12 for avatar processing). The frontend needs ZERO new packages, leveraging existing shadcn/ui and React 19 capabilities.

The recommended approach follows the existing architectural patterns: three new bounded contexts (Profiles, Reviews, Wishlists) with database-per-feature isolation, cross-context communication via domain events (MassTransit), and validation via MediatR queries. The critical architectural decision is separating these as distinct bounded contexts rather than extending existing features, which preserves future microservice extraction paths and maintains clean domain boundaries.

Key risks center on cross-context data integration: guest-to-authenticated migration requires atomic operations with advisory locks to prevent cart/order loss; verified purchase validation needs event-driven denormalization to avoid N+1 query hell; and avatar storage must use external blob storage (not container filesystem) to survive deployments. The 8 critical pitfalls identified all have well-documented prevention strategies from .NET modular monolith references and e-commerce domain research.

## Key Findings

### Recommended Stack

The existing stack requires minimal additions. **Only SixLabors.ImageSharp 3.1.12 is required** for avatar upload validation, resizing, and WebP conversion. Frontend rating components come from shadcn/ui community (copy/paste, no npm package). PostgreSQL full-text search for reviews uses existing Npgsql provider (no additional package). Azure Blob Storage for avatars leverages the existing Aspire.Azure.Storage.Blobs integration used for product images.

**Core technologies (unchanged):**
- Keycloak + NextAuth.js: User authentication, custom attributes for display name/avatar URL
- PostgreSQL + EF Core 10: Three new schemas (profiles, reviews, wishlists) with separate DbContexts
- MassTransit 9.0.0: Domain events for cross-context communication (ReviewCreated, UserAuthenticated)
- Next.js 16 Server Actions: File upload via FormData (built-in, no libraries needed)
- shadcn/ui: Rating component from community, forms/dialogs for profile management

**New package:**
- SixLabors.ImageSharp 3.1.12: Avatar validation (magic bytes, decompression bomb detection), resize to 200x200, convert to WebP for storage efficiency

### Expected Features

Research identified clear table stakes vs competitive differentiators based on Amazon, Shopify, and e-commerce UX patterns.

**Must have (table stakes):**
- User profiles: Display name, avatar upload, address book CRUD
- Guest-to-authenticated migration: Preserve cart and order history on login
- Product reviews: Star rating (1-5) + written text, one review per product per user
- Verified purchase badge: Cross-reference order history before allowing review
- Average rating calculation: Denormalized on product entity, updated via events
- Wishlists: Add/remove products, persistent across sessions, move to cart

**Should have (competitive advantages for v1.2+):**
- Review helpfulness voting: Upvote/downvote to surface quality reviews
- Review sorting/filtering: By date, rating, verified status (add when >20 reviews/product)
- Review images: Visual proof increases trust 3x (deferred until moderation workflow validated)
- Multiple wishlists: Power users organize by occasion (validate user demand first)
- Wishlist sharing: Read-only link for gift registry use case

**Defer (v2+):**
- Wishlist price drop alerts: Requires background job infrastructure, notification system
- AI review summary: Requires LLM integration, cost analysis
- Review Q&A section: Large scope, separate feature entirely

### Architecture Approach

Three new bounded contexts integrate with the existing modular monolith via domain events and cross-context queries, preserving database-per-feature isolation.

**Major components:**
1. **Profiles Context** (schema: profiles) — Owns UserProfile aggregate with display name, avatar URL, address book. Publishes UserCreatedDomainEvent and UserAuthenticatedDomainEvent for cross-context coordination. Uses Keycloak Admin API to sync custom attributes (display name, avatar URL) into JWT claims.

2. **Reviews Context** (schema: reviews) — Owns Review aggregate with rating, text, verified purchase flag. Consumes OrderConfirmedDomainEvent to enable post-purchase reviews. Publishes ReviewSubmittedDomainEvent to trigger product rating recalculation in Catalog. Validates ProductId and UserId via MediatR cross-context queries (no foreign keys).

3. **Wishlists Context** (schema: wishlists) — Owns Wishlist aggregate with single list per user. Integrates with Cart via MoveWishlistToCartSaga to handle partial failures. Validates ProductId via MediatR cross-context query.

**Integration patterns:**
- Cross-context validation: MediatR queries (ProductExistsQuery, HasUserPurchasedProductQuery) instead of foreign keys
- Denormalization for display: Review read model caches product name, user display name to avoid joins
- Guest data migration: UserAuthenticatedConsumer merges guest cart/orders using PostgreSQL advisory locks
- Avatar storage: Azure Blob Storage with URL references in database (existing pattern from product images)

### Critical Pitfalls

Research identified 8 critical pitfalls with prevention strategies validated against Microsoft Learn, DDD references, and e-commerce implementations.

1. **Guest-to-Authenticated Migration Race Condition** — Without synchronization, concurrent login requests create duplicate carts or lose data. **Prevention:** PostgreSQL advisory locks (`pg_advisory_xact_lock`) during migration, idempotent merge strategy, validate ownership transfer before deleting guest session.

2. **Cross-Context Query Hell for Verified Purchase** — Displaying reviews joins 4 DbContexts (Reviews, Catalog, Ordering, Profiles), causing N+1 queries. **Prevention:** Create read model projection (ProductReviewSummary) denormalizing product name, user display name, verified purchase flag. Update via event consumers (eventual consistency acceptable).

3. **Keycloak Profile Data Duplication** — Storing user data in both Keycloak custom attributes and application database creates sync nightmares. **Prevention:** Keycloak stores only authentication data (sub, email). Application database owns profile data (display name, avatar, addresses). Sync display name to Keycloak attribute only for JWT claims.

4. **Review Spam Without Verified Purchase Enforcement** — Accepting reviews without purchase validation enables fake reviews. **Prevention:** Database schema includes OrderId from day 1 (non-nullable). Validation query checks `order.BuyerId == userId && order.Status == Delivered && order.Items.Contains(productId)`.

5. **Wishlist-to-Cart Bounded Context Violation** — "Move all to cart" fails partially, leaving wishlist deleted but cart incomplete. **Prevention:** Saga pattern with compensating transactions. UI shows partial success ("3 of 5 added, 2 out of stock").

6. **Avatar Storage in Ephemeral Container Filesystem** — Saving avatars to `/app/uploads` loses files on container restart/scale. **Prevention:** Azure Blob Storage from day 1 (leverage existing integration). Store full CDN URL in database.

7. **Review Helpfulness Recency Bias** — Sorting by raw helpful votes buries new reviews. **Prevention:** Composite score with time decay, verified purchase bonus, Wilson score for small samples.

8. **Domain Event Eventual Consistency UX Breakdown** — User submits review, redirected to product page, review not visible yet. **Prevention:** Optimistic UI updates, polling fallback, inline read for write user, health checks for event lag.

## Implications for Roadmap

Based on research, suggested phase structure follows dependency order and risk mitigation.

### Phase 1: User Profiles & Authentication Flow
**Rationale:** Foundation for all other features. Establishes critical patterns (guest-to-auth migration, avatar storage, cross-context events) that Reviews and Wishlists depend on. Addresses the highest-risk pitfall (guest data loss on login) before adding new features that compound it.

**Delivers:**
- ProfilesDbContext with UserProfile and UserAddress entities
- Avatar upload with ImageSharp validation and Azure Blob Storage
- Address book CRUD (add, edit, delete, set default)
- Guest-to-authenticated migration (cart + order ownership transfer)
- UserCreatedDomainEvent and UserAuthenticatedDomainEvent infrastructure
- Keycloak Admin API integration for custom attribute sync

**Addresses features:**
- User profile management (display name, avatar) — table stakes
- Address book — reduces checkout friction
- Guest migration — preserves UX continuity

**Avoids pitfalls:**
- Guest-to-auth race condition (advisory locks implemented)
- Avatar container storage (blob storage from start)
- Keycloak data duplication (clear separation established)

**Research needs:** Standard patterns, no additional research required. Well-documented in ASP.NET Core and Azure Storage docs.

### Phase 2: Product Reviews & Ratings
**Rationale:** Most complex cross-context integration. Validates architecture patterns (event-driven denormalization, CQRS read models, MediatR cross-context queries) before simpler Wishlist feature. Depends on Profiles for UserId and Ordering for verified purchase validation.

**Delivers:**
- ReviewsDbContext with Review aggregate
- Verified purchase validation (query OrderingDbContext via MediatR)
- Review submission form with star rating (shadcn/ui component)
- Review display on product pages (read model with denormalized data)
- Product average rating calculation (domain event to Catalog)
- Profanity filter (automated moderation)

**Addresses features:**
- Star rating + review text — table stakes, influences 93% purchases
- Verified purchase badge — trust signal (Amazon weights 10x heavier)
- Average rating display — quick trust signal

**Avoids pitfalls:**
- Cross-context query hell (read model projection)
- Review spam (verified purchase enforcement from day 1)
- Eventual consistency UX (optimistic updates, polling fallback)
- Recency bias (composite score algorithm)

**Uses stack:**
- SixLabors.ImageSharp (defer review image uploads to v1.2)
- PostgreSQL tsvector (optional, only if review search needed)
- shadcn/ui rating component (copy from community)

**Research needs:** Standard CQRS patterns, no additional research. Reference: Milan Jovanovic modular monolith guides, Microsoft CQRS documentation.

### Phase 3: Wishlists & Saved Items
**Rationale:** Simplest of three features. Similar to Cart structure (proven pattern). Depends on Profiles for UserId and Cart for move-to-cart integration. Can leverage patterns established in Phases 1-2.

**Delivers:**
- WishlistsDbContext with Wishlist and WishlistItem entities
- Add/remove products (CRUD operations)
- Wishlist page with product grid
- Move to cart action (saga with partial failure handling)
- Visual indicator on product cards (heart icon)

**Addresses features:**
- Persistent wishlist — table stakes
- Move to cart — primary conversion path
- Visual feedback — prevents duplicate adds

**Avoids pitfalls:**
- Wishlist-to-cart bounded context violation (saga pattern)

**Research needs:** None. Reuses cart patterns and event-driven integration established in Phase 2.

### Phase 4: Integration & Polish
**Rationale:** Connects all features via frontend, adds navigation, end-to-end testing. Ensures cohesive UX across profile/review/wishlist features.

**Delivers:**
- Profile page with avatar upload, address management
- Review submission UI integrated into product pages
- Wishlist management UI with bulk actions
- Cross-feature navigation (profile → order history → review)
- E2E tests with Playwright (guest flow, authenticated flow, migration)

**Research needs:** None. Standard Next.js UI integration.

### Phase Ordering Rationale

- **Profiles first:** Other features depend on UserId, avatar storage pattern, guest migration infrastructure. Highest-risk pitfall (data loss) must be addressed before compounding with new features.
- **Reviews second:** Most complex integration validates architecture. Establishes read model pattern that Wishlists can reference. Front-loads hardest work to validate feasibility early.
- **Wishlists third:** Leverages proven patterns. Lower complexity means faster delivery. Allows phased rollout (Profiles + Reviews can launch first if needed).
- **Integration last:** UI polish happens after all backend features validated. Prevents rework if backend patterns need adjustment.

**Critical path:** UserAuthenticatedDomainEvent (Phase 1) enables cart merge and order linking, unblocking Reviews (needs order history) and Wishlists (needs user identity).

### Research Flags

**Phases with standard patterns (skip research-phase):**
- **Phase 1:** Well-documented in ASP.NET Core file upload, Azure Blob Storage, Keycloak Admin API docs
- **Phase 2:** Reference implementations exist (modular-monolith-with-ddd, booking-modular-monolith repos)
- **Phase 3:** Cart patterns already proven in v1.0
- **Phase 4:** Standard Next.js + Playwright patterns

**No phases require additional research.** All patterns validated against official documentation and reference implementations.

## Confidence Assessment

| Area | Confidence | Notes |
|------|------------|-------|
| Stack | HIGH | Existing stack covers 95%. SixLabors.ImageSharp verified compatible with .NET 10. PostgreSQL tsvector built into Npgsql provider. |
| Features | MEDIUM | Table stakes validated against Amazon/Shopify benchmarks. Competitive features prioritized based on UX research, but actual user validation needed post-launch. |
| Architecture | HIGH | Bounded context separation validated in multiple .NET DDD references (Milan Jovanovic, modular-monolith-with-ddd). Cross-context patterns match existing Cart/Ordering integration. |
| Pitfalls | MEDIUM | All 8 pitfalls sourced from Microsoft Learn, e-commerce research, DDD literature. Prevention strategies verified but not production-tested in this codebase. |

**Overall confidence:** HIGH

Research findings align with existing architectural decisions. No paradigm shifts required. Incremental addition of bounded contexts following established patterns.

### Gaps to Address

**Gap 1: Review image upload moderation workflow**
- Research identified review images increase trust 3x, but moderation is complex
- Deferred to v1.2+ pending user demand validation
- **Action:** Monitor review volume post-launch. If >20 reviews/product and users request images, research moderation services (AWS Rekognition, Azure Content Moderator) before implementing

**Gap 2: Wishlist price drop notification infrastructure**
- Background jobs and notification system not currently implemented
- Required for price alerts, stock alerts, review response notifications
- **Action:** Track user requests for alerts. If demand exists, research job scheduler options (Hangfire, Quartz.NET) and notification providers (SendGrid, Twilio) in dedicated phase

**Gap 3: Keycloak webhook event handling**
- User email changes in Keycloak don't automatically sync to UserProfileDbContext
- Keycloak Event Listener SPI requires custom development
- **Action:** Phase 1 implements manual sync via Admin API. If Keycloak becomes primary profile editor, investigate Event Listener SPI during Phase 1 execution

**Gap 4: Review ranking algorithm tuning**
- Composite score formula (Wilson score + time decay + verified bonus) based on academic research
- Real-world tuning requires production data
- **Action:** Implement configurable weighting in Phase 2. A/B test different formulas post-launch to optimize for conversion

## Sources

### Primary (HIGH confidence)

**Stack:**
- [SixLabors.ImageSharp NuGet](https://www.nuget.org/packages/sixlabors.imagesharp/) — Version 3.1.12, .NET 10 compatibility verified
- [ImageSharp Official Docs](https://docs.sixlabors.com/articles/imagesharp/) — Processing guide, API reference
- [ASP.NET Core File Uploads (Microsoft Learn)](https://learn.microsoft.com/en-us/aspnet/core/mvc/models/file-uploads?view=aspnetcore-10.0) — Official guidance
- [PostgreSQL Full-Text Search](https://www.postgresql.org/docs/current/textsearch.html) — tsvector/tsquery docs
- [Npgsql EF Core Full-Text Search](https://www.npgsql.org/efcore/mapping/full-text-search.html) — Provider documentation
- [Next.js Server Actions](https://nextjs.org/docs/app/building-your-application/data-fetching/server-actions-and-mutations) — FormData handling

**Architecture:**
- [Refactoring Overgrown Bounded Contexts (Milan Jovanovic)](https://www.milanjovanovic.tech/blog/refactoring-overgrown-bounded-contexts-in-modular-monoliths) — Bounded context boundaries
- [Evolving Modular Monoliths: Passing Data Between Contexts](https://www.thereformedprogrammer.net/evolving-modular-monoliths-3-passing-data-between-bounded-contexts/) — Cross-context query patterns
- [GitHub: modular-monolith-with-ddd](https://github.com/kgrzybek/modular-monolith-with-ddd) — Reference implementation
- [Upload files in ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/mvc/models/file-uploads?view=aspnetcore-10.0) — Microsoft official docs
- [Upload a blob with .NET - Azure Storage](https://learn.microsoft.com/en-us/azure/storage/blobs/storage-blob-upload) — Azure Blob Storage integration

**Pitfalls:**
- [Implementing reads/queries in a CQRS microservice - Microsoft Learn](https://learn.microsoft.com/en-us/dotnet/architecture/microservices/microservice-ddd-cqrs-patterns/cqrs-microservice-reads) — Cross-context query patterns
- [CQRS - Martin Fowler](https://www.martinfowler.com/bliki/CQRS.html) — When to use CQRS
- [Domain events: Design and implementation - Microsoft Learn](https://learn.microsoft.com/en-us/dotnet/architecture/microservices/microservice-ddd-cqrs-patterns/domain-events-design-implementation) — Eventual consistency patterns
- [Using PostgreSQL advisory locks](https://firehydrant.com/blog/using-advisory-locks-to-avoid-race-conditions-in-rails/) — Race condition prevention

### Secondary (MEDIUM confidence)

**Features:**
- [15 Must-Have E-commerce Features for 2026](https://www.sctinfo.com/blog/build-a-e-commerce-website/) — Feature prioritization
- [Ecommerce User Experience: Complete Guide (2025)](https://www.parallelhq.com/blog/ecommerce-user-experience) — UX patterns
- [Verified Purchase Reviews: The Power in E-Commerce](https://fastercapital.com/content/Product-reviews-and-ratings--Verified-Purchase-Reviews--The-Power-of-Verified-Purchase-Reviews-in-E-Commerce.html) — Industry patterns
- [Wishlist or shopping cart? (Nielsen Norman Group)](https://www.nngroup.com/articles/wishlist-or-cart/) — Wishlist UX research

**Architecture:**
- [GitHub: booking-modular-monolith](https://github.com/meysamhadeli/booking-modular-monolith) — .NET 9 with Vertical Slice, CQRS, MassTransit
- [Service boundaries identification example in e-commerce](https://hackernoon.com/service-boundaries-identification-example-in-e-commerce-a2c01a1b8ee9) — Context separation

**Pitfalls:**
- [Ranking Online Consumer Reviews (arXiv)](https://arxiv.org/pdf/1901.06274) — Recency bias research
- [Review helpfulness prediction survey (ScienceDirect)](https://www.sciencedirect.com/science/article/abs/pii/S0952197623012599) — Ranking algorithms
- [WooCommerce Cart Merge & Sessions (June 2025)](https://www.businessbloomer.com/woocommerce-cart-merge-sessions-changes/) — Migration patterns
- [Custom User Attributes with Keycloak (Baeldung)](https://www.baeldung.com/keycloak-custom-user-attributes) — Integration guide

### Tertiary (LOW confidence)

- [Spam Review Detection Techniques (MDPI)](https://www.mdpi.com/2076-3417/9/5/987) — 30-35% spam rate statistic
- [Generated Columns vs Triggers (OnGres)](https://ongres.com/blog/generate_columns_vs_triggers/) — Performance comparison
- [Next.js 15 File Upload (Strapi)](https://strapi.io/blog/epic-next-js-15-tutorial-part-5-file-upload-using-server-actions) — Implementation patterns

---
*Research completed: 2026-02-13*
*Ready for roadmap: yes*
