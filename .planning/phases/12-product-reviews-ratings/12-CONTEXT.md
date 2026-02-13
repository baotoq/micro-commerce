# Phase 12: Product Reviews & Ratings - Context

**Gathered:** 2026-02-13
**Status:** Ready for planning

<domain>
## Phase Boundary

Users can submit star ratings and written reviews for products they have purchased, with verified purchase badges. All users can view aggregate ratings and reviews on product pages. Review moderation, reporting, and helpfulness voting are out of scope.

</domain>

<decisions>
## Implementation Decisions

### Review display
- Compact list layout with minimal dividers between reviews, not bordered cards
- Essential info per review: display name, star rating, date, review text, verified purchase badge
- No avatar or helpful count displayed
- Sorted by most recent first (chronological, newest at top)
- Show first 5 reviews with "Load more" button to fetch next batch

### Star rating presentation
- Aggregate on product detail page: filled star icons + numeric average + review count (e.g., ★★★★☆ 4.2 (47 reviews))
- Product cards on browse/listing pages also show stars + count below product name
- No rating distribution breakdown (no bar chart) — just aggregate
- Classic filled yellow/gold stars with gray empty ones

### Review submission flow
- "Write a Review" button opens a modal dialog form
- Both star rating and text review required (no rating-only submissions)
- Accessible from product detail page reviews section AND order history (review link per purchased item)
- Button hidden for non-purchasers and unauthenticated users — show "Purchase this product to leave a review" message instead
- No review title/headline field — just star rating + review text

### Review policies
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

</decisions>

<specifics>
## Specific Ideas

No specific requirements — open to standard approaches

</specifics>

<deferred>
## Deferred Ideas

None — discussion stayed within phase scope

</deferred>

---

*Phase: 12-product-reviews-ratings*
*Context gathered: 2026-02-13*
