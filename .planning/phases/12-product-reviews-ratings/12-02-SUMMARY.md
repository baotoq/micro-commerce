---
phase: 12-product-reviews-ratings
plan: 02
subsystem: backend
tags: [cqrs, api, application-layer, endpoints, reviews]
dependencies:
  requires:
    - 12-01 (reviews domain model and database)
    - phase-11-profiles (user identity and display names)
    - ordering (purchase verification)
  provides:
    - review-cqrs-operations
    - review-rest-api
    - aggregate-rating-maintenance
    - product-review-statistics
  affects:
    - catalog (ProductDto extended with rating fields)
tech_stack:
  added:
    - ReviewsEndpoints (6 REST endpoints)
  patterns:
    - CQRS via MediatR (commands/queries)
    - FluentValidation for input validation
    - Purchase verification gate (OrderingDbContext query)
    - Ownership verification for update/delete
    - Batch queries to avoid N+1 (display names, verified purchases)
    - Aggregate rating recalculation (synchronous after CUD)
key_files:
  created:
    - src/MicroCommerce.ApiService/Features/Reviews/Application/Commands/CreateReview/CreateReviewCommand.cs
    - src/MicroCommerce.ApiService/Features/Reviews/Application/Commands/CreateReview/CreateReviewCommandValidator.cs
    - src/MicroCommerce.ApiService/Features/Reviews/Application/Commands/UpdateReview/UpdateReviewCommand.cs
    - src/MicroCommerce.ApiService/Features/Reviews/Application/Commands/UpdateReview/UpdateReviewCommandValidator.cs
    - src/MicroCommerce.ApiService/Features/Reviews/Application/Commands/DeleteReview/DeleteReviewCommand.cs
    - src/MicroCommerce.ApiService/Features/Reviews/Application/Queries/GetReviewsByProduct/GetReviewsByProductQuery.cs
    - src/MicroCommerce.ApiService/Features/Reviews/Application/Queries/GetUserReviewForProduct/GetUserReviewForProductQuery.cs
    - src/MicroCommerce.ApiService/Features/Reviews/Application/Queries/CheckUserPurchased/CheckUserPurchasedQuery.cs
    - src/MicroCommerce.ApiService/Features/Reviews/ReviewsEndpoints.cs
  modified:
    - src/MicroCommerce.ApiService/Features/Catalog/Application/Queries/GetProducts/ProductDto.cs
    - src/MicroCommerce.ApiService/Features/Catalog/Application/Queries/GetProducts/GetProductsQueryHandler.cs
    - src/MicroCommerce.ApiService/Features/Catalog/Application/Queries/GetProductById/GetProductByIdQueryHandler.cs
    - src/MicroCommerce.ApiService/Program.cs
decisions:
  - Purchase verification gates review creation by querying Orders with Paid/Confirmed/Shipped/Delivered status
  - One review per user per product enforced at database level (composite unique constraint) and caught in command handler
  - Aggregate rating recalculation happens synchronously after each CUD operation for immediate consistency
  - Batch queries for display names and verified purchases to avoid N+1 problems in list endpoint
  - Pagination defaults: page=1, pageSize=5, max pageSize=50
  - IsVerifiedPurchase flag shows badge on reviews from confirmed purchasers
  - ProductDto extended with AverageRating (decimal?) and ReviewCount (int) for frontend consumption
metrics:
  duration: 3
  tasks_completed: 2
  files_created: 9
  files_modified: 4
  completed_at: 2026-02-13
---

# Phase 12 Plan 02: Reviews CQRS Application Layer Summary

**One-liner:** Complete review CQRS operations (create with purchase verification, update/delete with ownership checks, paginated list with batch lookups) exposed through 6 REST API endpoints, with synchronous aggregate rating recalculation and ProductDto extended to include review statistics.

## Overview

Implemented the full application layer for the product reviews system. This plan created all CQRS commands and queries following MediatR patterns, added FluentValidation validators, implemented purchase verification and ownership checks, created 6 REST API endpoints, and extended the Catalog ProductDto to expose review statistics to the frontend.

## What Was Built

### CQRS Commands

**CreateReviewCommand:**
- Accepts UserId, ProductId, Rating (1-5), Text (10-1000 chars)
- Step 1: Verifies purchase by querying OrderingDbContext for orders with Paid/Confirmed/Shipped/Delivered status containing the ProductId
- Step 2: Creates Review via factory method
- Step 3: Saves to ReviewsDbContext, catches DbUpdateException for duplicate key (unique constraint violation) and throws ConflictException
- Step 4: Recalculates aggregate ratings (queries all reviews for product, computes AVG and COUNT, updates Product entity in CatalogDbContext)
- Returns Guid (review ID)

**UpdateReviewCommand:**
- Accepts UserId, ReviewId, Rating, Text
- Loads review, verifies ownership (UserId matches), calls review.Update(), saves
- Recalculates aggregate ratings for the product
- Returns void

**DeleteReviewCommand:**
- Accepts UserId, ReviewId
- Loads review, verifies ownership, stores ProductId, calls review.MarkDeleted(), removes from DbSet, saves
- Recalculates aggregate ratings (handles zero reviews case by setting AverageRating to null and ReviewCount to 0)
- Returns void

**FluentValidation Validators:**
- CreateReviewCommandValidator: Rating 1-5, Text 10-1000 chars, ProductId and UserId not empty
- UpdateReviewCommandValidator: Same rules as Create

### CQRS Queries

**GetReviewsByProductQuery:**
- Accepts ProductId, Page (default 1), PageSize (default 5)
- Returns ReviewListDto with Items, TotalCount, Page, PageSize
- ReviewDto contains: Id, UserId, DisplayName, Rating, Text, CreatedAt, IsVerifiedPurchase
- Sorts reviews by CreatedAt descending (newest first)
- Batch lookups to avoid N+1: queries ProfilesDbContext for display names, queries OrderingDbContext for verified purchases
- Uses HashSet for efficient IsVerifiedPurchase checks

**GetUserReviewForProductQuery:**
- Accepts UserId, ProductId
- Returns ReviewDto or null if user has not reviewed the product
- Looks up display name from ProfilesDbContext (defaults to "User")
- Checks IsVerifiedPurchase from OrderingDbContext

**CheckUserPurchasedQuery:**
- Accepts UserId, ProductId
- Returns bool indicating if user has purchased the product
- Queries OrderingDbContext for orders with Paid/Confirmed/Shipped/Delivered status containing ProductId

### REST API Endpoints

**ReviewsEndpoints.cs** with 6 endpoints:

1. **GET /api/reviews/products/{productId}** (public)
   - Query params: page (default 1), pageSize (default 5, max 50)
   - Returns paginated ReviewListDto
   - No authentication required (anyone can read reviews)

2. **GET /api/reviews/products/{productId}/mine** (authenticated)
   - Returns current user's review for the product or 404
   - Uses GetUserId helper to extract JWT claims

3. **GET /api/reviews/products/{productId}/can-review** (authenticated)
   - Returns CanReviewDto(HasPurchased, HasReviewed)
   - Frontend can use this to show/hide review form

4. **POST /api/reviews/products/{productId}** (authenticated)
   - Body: CreateReviewRequest(Rating, Text)
   - Returns 201 Created with CreateReviewResult(Id)
   - Returns 409 Conflict if user already reviewed
   - Returns 400 validation errors if input invalid

5. **PUT /api/reviews/{reviewId}** (authenticated)
   - Body: UpdateReviewRequest(Rating, Text)
   - Returns 204 No Content on success
   - Returns 404 if review not found
   - Returns 401 if not review owner

6. **DELETE /api/reviews/{reviewId}** (authenticated)
   - Returns 204 No Content on success
   - Returns 404 if review not found
   - Returns 401 if not review owner

**GetUserId helper:**
- Extracts user ID from JWT claims (ClaimTypes.NameIdentifier or "sub")
- Throws UnauthorizedAccessException if not found or invalid GUID

### Catalog ProductDto Extension

**ProductDto.cs:**
- Added two new parameters: AverageRating (decimal?) and ReviewCount (int)

**GetProductsQueryHandler.cs:**
- Updated Select projection to include p.AverageRating and p.ReviewCount

**GetProductByIdQueryHandler.cs:**
- Updated Select projection to include p.AverageRating and p.ReviewCount

**Result:** Frontend now receives review statistics with every product query

## Tasks Completed

### Task 1: Create CQRS commands, queries, validators, and aggregate rating recalculation
**Commit:** dfd9f882

Created all MediatR command/query handlers with validators, purchase verification, ownership checks, and synchronous aggregate rating recalculation.

**Files created:**
- CreateReviewCommand.cs, CreateReviewCommandValidator.cs
- UpdateReviewCommand.cs, UpdateReviewCommandValidator.cs
- DeleteReviewCommand.cs
- GetReviewsByProductQuery.cs (includes ReviewDto and ReviewListDto)
- GetUserReviewForProductQuery.cs
- CheckUserPurchasedQuery.cs

**Files modified:**
- ProductDto.cs (added AverageRating and ReviewCount)
- GetProductsQueryHandler.cs (project new fields)
- GetProductByIdQueryHandler.cs (project new fields)

**Verification:** Build succeeded with 0 errors.

### Task 2: Create ReviewsEndpoints and register in Program.cs
**Commit:** 4c511489

Created ReviewsEndpoints with 6 REST API endpoints following existing patterns (ProfilesEndpoints), registered in Program.cs.

**Files created:**
- ReviewsEndpoints.cs (6 endpoints, GetUserId helper, 4 request/response DTOs)

**Files modified:**
- Program.cs (added using statement and MapReviewsEndpoints() call)

**Verification:**
- Build succeeded with 0 errors
- MapReviewsEndpoints() is called in Program.cs (line 248)
- All 6 endpoints are defined in ReviewsEndpoints.cs

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 1 - Bug] Fixed OrderItem navigation property access**
- **Found during:** Task 1, GetReviewsByProductQuery batch verification lookup
- **Issue:** Plan specified `.Select(item => item.Order.BuyerId)` but OrderItem entity does not have a navigation property to Order
- **Fix:** Changed to `.Where(o => o.Items.Any(item => item.ProductId == request.ProductId)).Select(o => o.BuyerId)` to access BuyerId from Order directly
- **Files modified:** GetReviewsByProductQuery.cs
- **Commit:** dfd9f882 (Task 1)

## Technical Decisions

1. **Purchase verification prevents non-purchasers from submitting reviews:** CreateReviewCommand queries OrderingDbContext for orders with Paid/Confirmed/Shipped/Delivered status. Throws InvalidOperationException if no matching order found.

2. **Unique constraint enforcement:** Database-level composite unique index (UserId, ProductId) prevents duplicate reviews. Command handler catches DbUpdateException and throws ConflictException with user-friendly message.

3. **Synchronous aggregate recalculation:** After each CUD operation, handler queries all reviews for the product, computes AVG(Rating) and COUNT, and updates Product entity. Trade-off: immediate consistency for user experience (rating updates instantly) vs. potential performance impact (mitigated by indexing).

4. **Batch queries to avoid N+1:** GetReviewsByProductQuery extracts all UserIds from reviews, then queries ProfilesDbContext and OrderingDbContext once each. Uses Dictionary and HashSet for O(1) lookups when mapping to DTOs.

5. **Pagination with reasonable defaults:** page=1, pageSize=5, max pageSize=50. Frontend can request more but is capped to prevent abuse.

6. **IsVerifiedPurchase badge:** Each ReviewDto includes IsVerifiedPurchase flag. Frontend can display "Verified Purchase" badge for trust signaling.

7. **ProductDto includes review statistics:** Frontend receives AverageRating (decimal?, 3 decimal places) and ReviewCount (int) with every product query. Enables star rating display and "X reviews" text without additional API calls.

## Verification Results

All verification criteria passed:

- [x] `dotnet build src/MicroCommerce.ApiService` compiles with 0 errors
- [x] GET /api/reviews/products/{id} endpoint defined (public, paginated)
- [x] POST /api/reviews/products/{id} endpoint requires auth and purchase verification
- [x] PUT /api/reviews/{id} endpoint requires auth and ownership check
- [x] DELETE /api/reviews/{id} endpoint requires auth and ownership check
- [x] GET /api/reviews/products/{id}/can-review returns purchase and review status
- [x] GET /api/reviews/products/{id}/mine returns user's review or 404
- [x] ProductDto record includes AverageRating and ReviewCount parameters
- [x] GetProductsQueryHandler projects AverageRating and ReviewCount
- [x] GetProductByIdQueryHandler projects AverageRating and ReviewCount

## Self-Check: PASSED

**Files exist:**
- [FOUND] src/MicroCommerce.ApiService/Features/Reviews/Application/Commands/CreateReview/CreateReviewCommand.cs
- [FOUND] src/MicroCommerce.ApiService/Features/Reviews/Application/Commands/CreateReview/CreateReviewCommandValidator.cs
- [FOUND] src/MicroCommerce.ApiService/Features/Reviews/Application/Commands/UpdateReview/UpdateReviewCommand.cs
- [FOUND] src/MicroCommerce.ApiService/Features/Reviews/Application/Commands/UpdateReview/UpdateReviewCommandValidator.cs
- [FOUND] src/MicroCommerce.ApiService/Features/Reviews/Application/Commands/DeleteReview/DeleteReviewCommand.cs
- [FOUND] src/MicroCommerce.ApiService/Features/Reviews/Application/Queries/GetReviewsByProduct/GetReviewsByProductQuery.cs
- [FOUND] src/MicroCommerce.ApiService/Features/Reviews/Application/Queries/GetUserReviewForProduct/GetUserReviewForProductQuery.cs
- [FOUND] src/MicroCommerce.ApiService/Features/Reviews/Application/Queries/CheckUserPurchased/CheckUserPurchasedQuery.cs
- [FOUND] src/MicroCommerce.ApiService/Features/Reviews/ReviewsEndpoints.cs

**Commits exist:**
- [FOUND] dfd9f882 - Task 1 (CQRS commands/queries/validators)
- [FOUND] 4c511489 - Task 2 (ReviewsEndpoints and Program.cs registration)

**Verification commands:**
```bash
# Check build compiles
dotnet build src/MicroCommerce.ApiService

# Verify MapReviewsEndpoints called in Program.cs
grep -n "MapReviewsEndpoints" src/MicroCommerce.ApiService/Program.cs

# Count endpoint mappings (should be 6)
grep -c "group.Map" src/MicroCommerce.ApiService/Features/Reviews/ReviewsEndpoints.cs

# Verify ProductDto includes rating fields
grep -A 2 "DateTimeOffset? UpdatedAt" src/MicroCommerce.ApiService/Features/Catalog/Application/Queries/GetProducts/ProductDto.cs
```

## Next Steps

**Immediate (Plan 12-03):**
- Build frontend review display UI (star rating, review cards, verified purchase badges)
- Implement review submission form with validation
- Add "Can you review this product?" check to product detail page
- Display aggregate ratings on product listing cards

**Future:**
- Add review helpfulness voting (thumbs up/down)
- Implement review filtering (by rating, verified purchases only)
- Add review moderation capabilities
- Implement review images upload

## Notes

- ImageSharp vulnerability warnings present but not blocking (tracked separately)
- All CQRS handlers follow exact same patterns as Profiles module (consistency across codebase)
- Purchase verification logic duplicated in CreateReviewCommand and CheckUserPurchasedQuery (acceptable for query performance)
- Aggregate rating recalculation is synchronous for simplicity - could be moved to domain event handler if performance becomes issue
- Frontend will need to handle 409 Conflict (duplicate review) and display appropriate message
