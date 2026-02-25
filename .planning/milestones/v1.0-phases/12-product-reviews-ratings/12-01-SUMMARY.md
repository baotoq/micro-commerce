---
phase: 12-product-reviews-ratings
plan: 01
subsystem: backend
tags: [domain-model, infrastructure, database, reviews]
dependencies:
  requires:
    - phase-11-profiles (user identity foundation)
  provides:
    - reviews-domain-model
    - reviews-database-schema
    - product-review-aggregates
  affects:
    - catalog (Product entity extended with review stats)
tech_stack:
  added:
    - ReviewsDbContext (reviews schema)
  patterns:
    - DDD aggregate root (Review)
    - Value objects (Rating 1-5, ReviewText 10-1000 chars)
    - Domain events (ReviewCreated, ReviewUpdated, ReviewDeleted)
    - Composite unique constraint (UserId, ProductId)
    - Denormalized aggregates (Product.AverageRating, ReviewCount)
key_files:
  created:
    - src/MicroCommerce.ApiService/Features/Reviews/Domain/Entities/Review.cs
    - src/MicroCommerce.ApiService/Features/Reviews/Domain/ValueObjects/ReviewId.cs
    - src/MicroCommerce.ApiService/Features/Reviews/Domain/ValueObjects/Rating.cs
    - src/MicroCommerce.ApiService/Features/Reviews/Domain/ValueObjects/ReviewText.cs
    - src/MicroCommerce.ApiService/Features/Reviews/Domain/Events/ReviewCreatedDomainEvent.cs
    - src/MicroCommerce.ApiService/Features/Reviews/Domain/Events/ReviewUpdatedDomainEvent.cs
    - src/MicroCommerce.ApiService/Features/Reviews/Domain/Events/ReviewDeletedDomainEvent.cs
    - src/MicroCommerce.ApiService/Features/Reviews/Infrastructure/ReviewsDbContext.cs
    - src/MicroCommerce.ApiService/Features/Reviews/Infrastructure/Configurations/ReviewConfiguration.cs
    - src/MicroCommerce.ApiService/Features/Reviews/Infrastructure/Migrations/20260213094800_InitialReviews.cs
    - src/MicroCommerce.ApiService/Features/Catalog/Infrastructure/Migrations/20260213094812_AddProductReviewStats.cs
  modified:
    - src/MicroCommerce.ApiService/Features/Catalog/Domain/Entities/Product.cs
    - src/MicroCommerce.ApiService/Features/Catalog/Infrastructure/Configurations/ProductConfiguration.cs
    - src/MicroCommerce.ApiService/Program.cs
decisions:
  - Composite unique index on (UserId, ProductId) enforces one review per user per product at database level
  - Rating value object validates 1-5 range with factory method
  - ReviewText value object validates 10-1000 character range with factory method
  - Domain events carry ProductId to enable downstream aggregate recalculation
  - Product.AverageRating and ReviewCount denormalized for query performance (event handlers will maintain)
  - PostgreSQL xmin row versioning for optimistic concurrency on Review entity
  - CreatedAt index descending for chronological sorting (newest first)
metrics:
  duration: 3
  tasks_completed: 2
  files_created: 11
  files_modified: 3
  completed_at: 2026-02-13
---

# Phase 12 Plan 01: Reviews Backend Foundation Summary

**One-liner:** Review aggregate root with Rating (1-5) and ReviewText (10-1000 chars) value objects, ReviewsDbContext with 'reviews' schema isolation, and Product entity extended with denormalized AverageRating/ReviewCount fields.

## Overview

Established the domain model and database infrastructure for the product reviews system. This plan created the Review aggregate root following DDD patterns, implemented value objects with validation, set up a dedicated ReviewsDbContext with schema isolation, and extended the Product entity with denormalized review statistics for query performance.

## What Was Built

### Domain Model

**Review Aggregate Root:**
- ReviewId strongly-typed ID
- ProductId and UserId (Guid) for relationships
- Rating value object (validates 1-5 range)
- ReviewText value object (validates 10-1000 chars, trimmed)
- CreatedAt and UpdatedAt timestamps
- PostgreSQL xmin concurrency token (Version property)
- Factory method: `Review.Create(Guid productId, Guid userId, int rating, string text)`
- Update method: `Update(int rating, string text)` with ReviewUpdatedDomainEvent
- MarkDeleted method: raises ReviewDeletedDomainEvent

**Domain Events:**
- ReviewCreatedDomainEvent(Guid ReviewId, Guid ProductId)
- ReviewUpdatedDomainEvent(Guid ReviewId, Guid ProductId)
- ReviewDeletedDomainEvent(Guid ReviewId, Guid ProductId)
All carry ProductId to enable downstream aggregate recalculation.

**Product Entity Extensions:**
- AverageRating (decimal?, precision 3,2) - denormalized average rating
- ReviewCount (int, default 0) - denormalized review count
- UpdateReviewStats method for event handlers to maintain aggregates

### Infrastructure

**ReviewsDbContext:**
- Dedicated 'reviews' schema isolation
- DbSet<Review> Reviews
- Applies ReviewConfiguration via assembly scanning (namespace filter)
- Registered in Program.cs with separate migrations history table

**ReviewConfiguration:**
- Composite unique index on (UserId, ProductId) - enforces one review per user per product
- Index on ProductId (for listing reviews by product)
- Index on CreatedAt descending (for chronological sorting)
- OwnsOne for Rating value object (Rating column)
- OwnsOne for ReviewText value object (ReviewText column, max 1000)
- PostgreSQL xmin row version mapping
- Ignores DomainEvents collection

**Database Migrations:**
- InitialReviews (Reviews table with all indexes and constraints)
- AddProductReviewStats (Catalog schema - adds AverageRating and ReviewCount columns to Products table)

## Tasks Completed

### Task 1: Create Reviews Domain Model
**Commit:** ac98f756

Created Review aggregate root with Rating and ReviewText value objects, three domain events, and extended Product entity with review statistics.

**Files created:**
- Review.cs (aggregate root)
- ReviewId.cs, Rating.cs, ReviewText.cs (value objects)
- ReviewCreatedDomainEvent.cs, ReviewUpdatedDomainEvent.cs, ReviewDeletedDomainEvent.cs

**Files modified:**
- Product.cs (added AverageRating, ReviewCount, UpdateReviewStats method)
- ProductConfiguration.cs (added column mappings for review stats)

**Verification:** Build succeeded with 0 errors.

### Task 2: Create ReviewsDbContext and Database Migrations
**Commit:** b466c1e9

Created ReviewsDbContext with 'reviews' schema isolation, ReviewConfiguration with composite unique index, registered in Program.cs, and generated both Reviews and Catalog migrations.

**Files created:**
- ReviewsDbContext.cs
- ReviewConfiguration.cs
- 20260213094800_InitialReviews.cs (migration)
- 20260213094812_AddProductReviewStats.cs (Catalog migration)

**Files modified:**
- Program.cs (added ReviewsDbContext registration and using statement)
- CatalogDbContextModelSnapshot.cs (updated for new Product columns)

**Verification:**
- Build succeeded with 0 errors
- Migration files exist in correct locations
- ReviewsDbContext registered in Program.cs

## Deviations from Plan

None - plan executed exactly as written.

## Technical Decisions

1. **Composite unique constraint on (UserId, ProductId):** Enforces one review per user per product at the database level, preventing duplicate reviews through race conditions.

2. **Value objects for Rating and ReviewText:** Encapsulates validation logic (Rating 1-5, ReviewText 10-1000 chars) in domain layer, ensuring invalid data cannot be created.

3. **Domain events carry ProductId:** Enables event handlers in future plans to recalculate Product.AverageRating and ReviewCount without additional lookups.

4. **Denormalized review aggregates on Product:** Trade-off for query performance - avoid expensive AVG() queries on product listing/detail pages. Event handlers will maintain consistency.

5. **PostgreSQL xmin for concurrency:** Follows existing pattern across all aggregates (UserProfile, Order, Cart) - prevents lost updates during concurrent review edits.

6. **CreatedAt index descending:** Optimizes "newest reviews first" sorting pattern common in e-commerce UIs.

## Verification Results

All verification criteria passed:

- [x] `dotnet build src/MicroCommerce.ApiService` compiles with 0 errors
- [x] Review.cs extends BaseAggregateRoot with factory method and Update method
- [x] Rating value object validates 1-5 range
- [x] ReviewText value object validates 10-1000 character range
- [x] ReviewConfiguration has composite unique index on (UserId, ProductId)
- [x] Product.cs has AverageRating (decimal?) and ReviewCount (int) properties
- [x] ReviewsDbContext is registered in Program.cs with 'reviews' schema
- [x] Migration files exist for both Reviews and Catalog changes

## Self-Check: PASSED

**Files exist:**
- [FOUND] src/MicroCommerce.ApiService/Features/Reviews/Domain/Entities/Review.cs
- [FOUND] src/MicroCommerce.ApiService/Features/Reviews/Domain/ValueObjects/ReviewId.cs
- [FOUND] src/MicroCommerce.ApiService/Features/Reviews/Domain/ValueObjects/Rating.cs
- [FOUND] src/MicroCommerce.ApiService/Features/Reviews/Domain/ValueObjects/ReviewText.cs
- [FOUND] src/MicroCommerce.ApiService/Features/Reviews/Infrastructure/ReviewsDbContext.cs
- [FOUND] src/MicroCommerce.ApiService/Features/Reviews/Infrastructure/Configurations/ReviewConfiguration.cs

**Commits exist:**
- [FOUND] ac98f756 - Task 1 (Reviews domain model)
- [FOUND] b466c1e9 - Task 2 (ReviewsDbContext and migrations)

**Verification commands:**
```bash
# Check Review aggregate compiles and follows patterns
dotnet build src/MicroCommerce.ApiService

# Verify migration files exist
ls src/MicroCommerce.ApiService/Features/Reviews/Infrastructure/Migrations/
ls src/MicroCommerce.ApiService/Features/Catalog/Infrastructure/Migrations/ | grep AddProductReviewStats

# Verify ReviewsDbContext registration
grep -n "ReviewsDbContext" src/MicroCommerce.ApiService/Program.cs
```

## Next Steps

**Immediate (Plan 12-02):**
- Implement review CRUD operations (commands/queries via MediatR)
- Add endpoint mappings for create, update, delete, and list reviews
- Implement domain event handlers to update Product.AverageRating and ReviewCount

**Future (Plan 12-03):**
- Build frontend review display and submission UI
- Add review filtering and sorting (helpful votes, verified purchases)
- Implement moderation capabilities

## Notes

- ImageSharp vulnerability warnings present but not blocking (tracked separately)
- Review domain model follows exact same patterns as UserProfile and Product aggregates
- Schema isolation ensures future extraction to microservice is straightforward
- Domain events prepared for event-driven aggregate maintenance (Plan 12-02 will implement handlers)
