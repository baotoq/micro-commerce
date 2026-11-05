# Feature Specification: Product Catalog

**Feature Branch**: `002-product-catalog`

**Created**: 2026-11-05

**Status**: Draft

**Input**: User description: "implement inventory product listing catalog"

## Overview

MicroCommerce is a multi-merchant Marketplace (see `CONTEXT.md`). The project skeleton (001) lets an
Account sign in, but nothing can be sold yet. This feature delivers the Catalog:

- An Account opens its own Merchant.
- The Merchant lists Products with Variants and images in the Merchant Centre, and publishes them.
- Buyers browse, search and open those Products across every Merchant in one place.
- Platform Operators maintain the Category tree and can remove Products.

In the user's request, "inventory" means each Variant's stock and the Availability Buyers see, and
"listing" means a Product. The glossary avoids both words. The scope follows the earlier
Marketplace design in issues #99, #103, #104, #105 and #156. Cart, Checkout, Reservations and
Merchant suspension are separate features.

## Clarifications

### Session 2026-11-05

- Q: How do Merchants come to exist in this feature? → A: An Account opens its own Merchant, with a
  Shop name and Pickup Address. Suspension and reinstatement are out of scope.
- Q: Is managing the Category tree in scope? → A: Yes. Platform Operators maintain it, and a starter
  tree is seeded in development.
- Q: Are Product images in scope? → A: Yes. Merchants upload images. The storage dependency was
  approved as Azure Blob storage, with the Azurite emulator locally (see plan).

## User Scenarios & Testing *(mandatory)*

### User Story 1 - An Account opens a Merchant (Priority: P1)

A signed-in Account that owns no Merchant opens one by giving a Shop name, an optional description
and a Pickup Address. It can start selling at once, with no approval step. Later it can edit its
Shop name, description and Pickup Address. Each Account owns at most one Merchant.

**Why this priority**: Every Product belongs to a Merchant. Nobody can list anything until an Account
can become a Merchant.

**Independent Test**: Sign in as an Account with no Merchant. Open a Merchant and confirm the
Account now shows as owning it. Then try to open a second one and confirm that is refused. Finally,
as another Account, try a Shop name that differs only in letter case and confirm it is refused.

**Acceptance Scenarios**:

1. **Given** a signed-in Account without a Merchant, **When** it opens a Merchant with a valid Shop
   name and Pickup Address, **Then** the Merchant exists, the Account is its owner and its Shop page
   is reachable.
2. **Given** an Account that already owns a Merchant, **When** it tries to open another, **Then** it
   is refused and nothing changes.
3. **Given** an existing Shop named "Sunny Mugs", **When** another Account tries to open "sunny mugs",
   **Then** it is refused because the Shop name is taken.
4. **Given** a missing Shop name, or a Pickup Address without a province, **When** the Account submits,
   **Then** nothing is saved and every invalid field is named in the error.
5. **Given** a Merchant owner, **When** they edit the Shop name, description or Pickup Address,
   **Then** the change is saved, and the Shop page and the Merchant's Products show the new Shop name.
6. **Given** a signed-in Account, **When** it asks who it is, **Then** it also learns whether it owns a
   Merchant and which one.

---

### User Story 2 - A Merchant lists a Product in the Merchant Centre (Priority: P1)

A Merchant owner opens the Merchant Centre and creates a Product. They give it a name, a description
and a leaf Category, plus a price, stock and weight for its single default Variant. The Product
starts as Draft. The Merchant publishes it when ready and can unpublish it again. The Merchant Centre
lists all of the Merchant's own Products in every status, with each Product's price range and total
stock.

**Why this priority**: There is nothing to browse until a Merchant can list a Product.

**Independent Test**: As a Merchant owner, create a Product, confirm it is Draft and appears in the
Merchant's own list, then publish it. As a second Merchant owner, confirm the first Merchant's
Product can be neither seen nor changed.

**Acceptance Scenarios**:

1. **Given** a Merchant owner, **When** they create a Product with a name, description, leaf Category,
   price, stock and weight, **Then** the Product is saved as Draft with one default Variant holding
   that price, stock and weight.
2. **Given** a Draft Product, **When** its Merchant publishes it, **Then** its status becomes
   Published. **When** the Merchant unpublishes it, **Then** it returns to Draft.
3. **Given** a Published Product, **When** its Merchant edits the name, description, price or stock,
   **Then** the changes are saved and the Product stays Published.
4. **Given** invalid input (an empty name, a price of zero or less, negative stock, a weight of zero or
   less, or a Category that is unknown or not a leaf), **When** the Merchant saves the Product,
   **Then** nothing is saved and every invalid field is named in the error.
5. **Given** a Merchant with Products in several statuses, **When** they open their Product list,
   **Then** they see every one of their own Products with its status, price range and total stock,
   and no other Merchant's Products.
6. **Given** a Product of Merchant A, **When** Merchant B's owner tries to read, edit, publish or
   unpublish it, **Then** the request behaves as if the Product does not exist and nothing changes.
7. **Given** a signed-in Account that owns no Merchant, **When** it calls any Merchant Centre
   operation, **Then** it is refused.
8. **Given** no sign-in, **When** any Merchant Centre operation is called, **Then** it is refused as
   unauthenticated.

---

### User Story 3 - Buyers browse, search and open Products across Merchants (Priority: P1)

Anyone, signed in or not, browses one paginated list of Published Products from every Merchant. They
search by words in the Product name or description, filter by Category, and open a Product to see
its images, description, Category, options and every Variant with its price and Availability. Each
Product shows which Shop sells it, and the Shop page lists that Merchant's Published Products.

**Why this priority**: Browsing is what Buyers come for, and every later Buying feature starts from a
Product a Buyer has found. User Stories 1–3 together are the MVP.

**Independent Test**: With Published and Draft Products from two Merchants in the Catalog, browse
without signing in. Confirm that only Published Products appear, that search and the Category filter
narrow the list, that a Product's page shows each Variant's price and Availability but never its
stock, and that opening a Draft Product directly is not found.

**Acceptance Scenarios**:

1. **Given** Published Products from several Merchants, **When** a visitor browses the Marketplace
   without signing in, **Then** they see one paginated list of those Products. Each shows its name,
   cover image, price (or price range across its Variants), whether it is available, and the Shop
   that sells it.
2. **Given** Draft and Removed Products, **When** a visitor browses, searches or opens a Shop page,
   **Then** those Products never appear. Opening one directly is not found.
3. **Given** a Published Product named "Ceramic coffee mug", **When** a visitor searches for "mugs",
   **Then** the Product is in the results (word variations match).
4. **Given** a Category tree Home › Kitchen › Mugs, **When** a visitor filters by Home, **Then**
   Products in Home and in all of its descendant Categories are listed.
5. **Given** a Published Product, **When** a visitor opens it, **Then** they see its images in order,
   its description, Category, options, and every Variant with its option values, price and
   Availability.
6. **Given** a Variant with stock 0, **When** a visitor views it, **Then** it shows as unavailable.
   **Given** a Product whose Variants all have stock 0, **Then** its list entry shows it as not
   available.
7. **Given** a Merchant's Shop, **When** a visitor opens the Shop page, **Then** they see the Shop
   name and description and that Merchant's Published Products only.
8. **Given** a Product without images, **When** a visitor sees it, **Then** a placeholder is shown in
   place of the cover image.

---

### User Story 4 - A Merchant sells option combinations as Variants (Priority: P2)

A Merchant gives a Product up to two options (for example Colour: Red, Blue and Size: S, M, L).
Every combination of option values becomes a Variant with its own price, stock and weight. When the
Merchant later edits the options, the combinations that survive keep their identity.

**Why this priority**: Most Marketplace goods come in options, but simple single-Variant Products
from User Story 2 are a usable MVP without it.

**Independent Test**: Create a Product with Colour (Red, Blue) and Size (S, M, L). Confirm it has
exactly 6 Variants, each with its own price, stock and weight. Remove Size L, and confirm that the
4 surviving Variants keep their identity and the 2 L Variants are gone.

**Acceptance Scenarios**:

1. **Given** a Product with two options of 2 and 3 values, **When** the Merchant saves a price, stock
   and weight for each of the 6 combinations, **Then** the Product has exactly those 6 Variants.
2. **Given** a Product with options, **When** the Merchant saves Variants with a combination missing,
   duplicated or using an unknown option value, **Then** nothing is saved and the error names the
   problem.
3. **Given** a Merchant defines three options, or an option with no values or duplicate values,
   **When** they save, **Then** it is refused.
4. **Given** a Product with Variants, **When** the Merchant adds or removes option values, **Then**
   surviving combinations keep their identity, dropped combinations disappear and new ones are
   added.
5. **Given** a Product with options, **When** a Buyer opens it, **Then** they can tell every Variant
   apart by its option values and see each one's price and Availability.

---

### User Story 5 - A Merchant adds images to a Product (Priority: P2)

A Merchant uploads images for a Product, reorders them and removes ones it no longer wants. The first
image is the cover that Buyers see in lists. Images belong to the Product, not to a Variant.

**Why this priority**: Buyers rarely buy what they cannot see, but browsing and listing work with
placeholders, so images can follow the MVP.

**Independent Test**: As a Merchant owner, upload three images to a Product, move the third to the
front, and remove the second. Publish the Product and confirm that Buyers see the two remaining
images in the new order, with the moved image as the cover.

**Acceptance Scenarios**:

1. **Given** a Merchant's Product, **When** the owner uploads a JPEG, PNG or WebP image of at most
   5 MB, **Then** the image is added at the end of the Product's images.
2. **Given** a Product with images, **When** the owner sets a new order, **Then** Buyers see the
   images in that order and the first is the cover.
3. **Given** a Product with images, **When** the owner removes one, **Then** Buyers no longer see it.
4. **Given** a file that is not a JPEG, PNG or WebP image (judged by its content, not its name) or is
   larger than 5 MB, **When** the owner uploads it, **Then** it is refused and the Product is
   unchanged.
5. **Given** a Product that already has 9 images, **When** the owner uploads another, **Then** it is
   refused.
6. **Given** another Merchant's Product, **When** an owner tries to add, reorder or remove its images,
   **Then** it behaves as if the Product does not exist.

---

### User Story 6 - Platform Operators maintain the Category tree (Priority: P2)

A Platform Operator builds and maintains the Marketplace-wide Category tree. They add top-level and
child Categories, rename Categories and delete ones that are no longer needed. The rules keep every
Product in a leaf Category. A starter tree is available in local development.

**Why this priority**: Every Product needs a leaf Category, and Buyers filter by Category. The
seeded starter tree is enough for the MVP, so managing the tree can come second.

**Independent Test**: As a Platform Operator, create a top-level Category and a child under it, then
rename the child. Then put a Product in the child and confirm the child can be neither given a child
of its own nor deleted. Finally, confirm an ordinary Account cannot change the tree.

**Acceptance Scenarios**:

1. **Given** a Platform Operator, **When** they create a top-level Category or a child of an existing
   Category, **Then** it appears in the Category tree.
2. **Given** a Category, **When** a Platform Operator renames it, **Then** its Products stay in it and
   show the new name.
3. **Given** a Category that holds Products, **When** a Platform Operator tries to add a child to it,
   **Then** it is refused.
4. **Given** a Category with children or Products, **When** a Platform Operator tries to delete it,
   **Then** it is refused.
5. **Given** two sibling Categories, **When** a Platform Operator gives one the same name as the
   other, ignoring letter case, **Then** it is refused.
6. **Given** an ordinary Account or a Merchant owner, **When** it tries to change the Category tree,
   **Then** it is refused.
7. **Given** anyone, signed in or not, **When** they read the Category tree, **Then** they get the
   whole tree.

---

### User Story 7 - Platform Operators remove prohibited Products (Priority: P3)

A Platform Operator can view any Product, whatever its status or Merchant, and mark it Removed.
A Removed Product leaves the Marketplace at once. Its Merchant still sees it, marked Removed, but
can no longer edit it, publish it or change its images.

**Why this priority**: Moderation matters before the Marketplace goes public, but browsing and
listing work without it.

**Independent Test**: As a Platform Operator, remove a Published Product. Confirm that Buyers can no
longer find or open it, and that its Merchant sees it as Removed and cannot edit or publish it.

**Acceptance Scenarios**:

1. **Given** any Product, **When** a Platform Operator opens it, **Then** they see it whatever its
   status or Merchant.
2. **Given** a Published Product, **When** a Platform Operator removes it, **Then** it no longer
   appears in browsing, search or its Shop page, and opening it directly is not found.
3. **Given** a Removed Product, **When** its Merchant views its Product list, **Then** the Product is
   shown as Removed. **When** the Merchant tries to edit, publish or change its images, **Then** it is
   refused.
4. **Given** an Account that is not a Platform Operator, **When** it tries to remove a Product,
   **Then** it is refused.

---

### Edge Cases

- Two Merchants give Products the same name. Both are allowed. Product names need not be unique.
- Two Accounts open Merchants with the same Shop name at the same moment. Exactly one succeeds.
- A Merchant edits a Product while a Buyer is viewing it. The Buyer sees the change on their next
  view.
- A Merchant sets a Variant's stock to 0. The Variant stays on the Product page, shown as
  unavailable. It is not hidden.
- Search matches nothing. The visitor gets an empty result, not an error.
- A visitor asks for a page past the last page, or a page size above the maximum. They get an empty
  page or the maximum page size, not an error.
- A visitor filters by a Category that does not exist. They get an empty result.
- A Merchant edits options so that a Variant's combination disappears. That Variant is gone. Its
  identity is not reused for a different combination.
- Two edits of the same Product, for example from two browser tabs, are saved at once. The second
  save must not silently overwrite the first. It is refused, and the owner must reload.
- A Platform Operator adds a child to a Category at the same moment a Merchant puts a Product into
  it. The leaf rule still holds: one of the two is refused.
- An image upload is interrupted or fails half way. The Product is unchanged, and no broken image is
  shown.
- A Merchant uses a Product, image or Category identifier that belongs to another Merchant or does
  not exist. It is treated as not found and reveals nothing about other Merchants' Products.
- A request names a Merchant in its body or address. It is ignored. The Merchant always comes from
  the signed-in Account.

## Requirements *(mandatory)*

### Functional Requirements

**Merchants**

- **FR-001**: A signed-in Account that owns no Merchant MUST be able to open one by giving a Shop name,
  an optional description and a Pickup Address. The Merchant can sell at once, with no approval.
- **FR-002**: An Account MUST own at most one Merchant. A second attempt to open one MUST be refused.
- **FR-003**: Shop names MUST be unique across the Marketplace, ignoring letter case and surrounding
  spaces, and MUST be 3 to 50 characters long.
- **FR-004**: A Pickup Address MUST include a street address, a district and a province.
- **FR-005**: A Merchant owner MUST be able to edit the Shop name, description and Pickup Address.
- **FR-006**: The signed-in Account's details MUST show whether it owns a Merchant, and if so the
  Merchant's identifier and Shop name.

**Products and Variants (Merchant Centre)**

- **FR-007**: A Merchant MUST be able to create a Product with a name, a description and exactly one
  leaf Category.
- **FR-008**: A new Product MUST start as Draft. Its Merchant MUST be able to publish a Draft Product
  and unpublish a Published Product back to Draft. Editing a Product MUST NOT change its status.
- **FR-009**: A Product without options MUST have exactly one default Variant.
- **FR-010**: A Merchant MUST be able to give a Product up to two options, each with at least one
  value and no duplicate values.
- **FR-011**: A Product's Variants MUST be exactly the full set of combinations of its option values,
  each present once. Missing, duplicate or unknown combinations MUST be rejected.
- **FR-012**: Every Variant MUST have a price above zero, in whole units of the single Marketplace
  currency, a stock of zero or more and a weight in grams above zero.
- **FR-013**: When a Merchant edits a Product's options or Variants, each combination that survives
  the edit MUST keep its Variant identity. Dropped combinations MUST be removed, and their identity
  MUST NOT be reused.
- **FR-014**: A Merchant MUST be able to edit a Product's name, description, Category, options and
  Variants, including stock and price.
- **FR-015**: A Merchant MUST be able to list all of its own Products in every status. Each entry
  shows the status, the cover image, the price range across Variants and the total stock.
- **FR-016**: Every Merchant Centre operation MUST act only on the Merchant owned by the signed-in
  Account, resolved from the sign-in and never from the request. Another Merchant's Product MUST be
  treated as not found for reads and writes.
- **FR-017**: Merchant Centre operations MUST be refused for anonymous callers and for Accounts that
  own no Merchant.
- **FR-018**: Invalid input MUST be rejected with an error that names each invalid field, and nothing
  may be saved.
- **FR-019**: A save based on an outdated version of a Product MUST be refused rather than overwrite a
  newer change.

**Product images**

- **FR-020**: A Merchant MUST be able to upload images to its Product. Only JPEG, PNG and WebP are
  accepted, judged by the file's content. The limits are 5 MB per image and 9 images per Product.
- **FR-021**: A Merchant MUST be able to reorder and remove its Product's images. The first image is
  the cover.
- **FR-022**: A failed or refused upload MUST leave the Product's images unchanged.
- **FR-023**: Buyers MUST be able to see a Published Product's images. A Product without images shows
  a placeholder.

**Browsing (Buyers and visitors)**

- **FR-024**: Browsing, search, Product pages, Shop pages and the Category tree MUST be available
  without signing in.
- **FR-025**: Buyers MUST see a Product only when it is Published. Every other Product MUST be absent
  from lists, search and Shop pages, and not found when opened directly. Its images MUST NOT be
  listed either.
- **FR-026**: The Marketplace MUST list Published Products from every Merchant in one paginated list.
  Each entry shows the name, the cover image, the price or price range across Variants, whether any
  Variant is available, and the Shop that sells it.
- **FR-027**: Visitors MUST be able to search by words in the Product name and description, with word
  variations matching (for example "mugs" finds "mug").
- **FR-028**: Visitors MUST be able to filter by a Category. The result includes Products in that
  Category and all of its descendants. Search and the filter MUST combine.
- **FR-029**: A Product page MUST show the images, description, Category, options, and every Variant
  with its option values, price and Availability.
- **FR-030**: Buyers MUST see Availability and never stock. Until Reservations exist, a Variant's
  Availability equals its stock. A Variant with Availability 0 is shown as unavailable.
- **FR-031**: A Shop page MUST show the Merchant's Shop name, description and its Published Products.
  Its Pickup Address MUST NOT be shown to Buyers.
- **FR-032**: Changes a Merchant or Platform Operator saves MUST be visible to Buyers on their next
  view.

**Category tree**

- **FR-033**: The Marketplace MUST have one Category tree that anyone can read. A starter tree MUST be
  present in local development without manual steps.
- **FR-034**: Platform Operators MUST be able to create top-level and child Categories, rename a
  Category, and delete a Category that has neither children nor Products. Sibling Categories MUST
  have distinct names, ignoring letter case.
- **FR-035**: A Category that holds Products MUST NOT gain children, so that every Product stays in a
  leaf Category, even when the two changes happen at the same moment.
- **FR-036**: Only Platform Operators MUST be able to change the Category tree.

**Moderation**

- **FR-037**: A Platform Operator MUST be able to view any Product, whatever its status or Merchant.
- **FR-038**: A Platform Operator MUST be able to mark any Product Removed. Removed is final in this
  feature: its Merchant sees it as Removed but can neither edit it, publish it nor change its images.
- **FR-039**: Only Platform Operators MUST be able to remove Products or view other Merchants' Draft
  and Removed Products.

### Key Entities

- **Merchant**: A selling business owned by exactly one Account (see `CONTEXT.md`). It has a unique
  Shop name, an optional description and a Pickup Address.
- **Pickup Address**: Where a Merchant's parcels ship from: a street address, a district and a
  province. Later features use the province for Shipping Fees.
- **Shop**: The Merchant's public page. It shows the Shop name, description and Published Products.
- **Category**: A node in the Marketplace-wide tree, with a name and an optional parent. Products sit
  only in leaf Categories.
- **Product**: A Merchant's listing. It has a name, a description, one leaf Category, a status (Draft,
  Published or Removed), up to two options, one or more Variants and up to 9 ordered images.
- **Option** and **option value**: A named dimension of a Product (for example Colour) and its allowed
  values (for example Red and Blue). This is a new domain term. `CONTEXT.md` must define it in the
  same change (Constitution Principle I).
- **Product image**: One picture of a Product, at a position in the Product's image order. The first
  is the cover.
- **Variant**: One purchasable combination of a Product's option values, with a price, stock and
  weight. It has a stable identity that later Cart and Checkout work refers to.
- **Availability**: What a Buyer can buy of a Variant right now. In this feature it equals stock.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: A signed-in Account goes from having no Merchant to a Published single-Variant Product
  in under 5 minutes.
- **SC-002**: A Merchant owner lists a Product with 2 options, 6 Variants and 3 images in under
  5 minutes.
- **SC-003**: With at least 10,000 Published Products in the Catalog, 95% of browse, search and
  Category-filter requests show results in under 1 second.
- **SC-004**: Buyers never see a Draft or Removed Product, its images, another Merchant's stock
  figures or a Pickup Address. Automated tests verify this for every browsing operation.
- **SC-005**: In automated tests, 100% of attempts by one Merchant to read or change another
  Merchant's Products or images are refused or treated as not found. 100% of Merchant Centre and
  Platform Operator operations refuse unauthorised callers.
- **SC-006**: A change a Merchant saves (publish, unpublish, price, stock or images) is visible to
  Buyers on their next view, within 5 seconds.
- **SC-007**: In a test search set, at least 90% of searches using a plural or singular form of a
  word in a Product's name find that Product.
- **SC-008**: A 5 MB image upload completes in under 10 seconds on a typical broadband connection.

## Assumptions

- **Earlier design**: The scope and rules follow the earlier Marketplace design in issues #99
  (parent), #103, #104, #105 and #156, adapted to the current constitution. Their technology choices
  (Next.js, Wolverine, Redis caching) do not carry over, because the constitution sets the stack.
- **Terminology**: "Inventory" maps to Variant stock and Availability, and "listing" maps to Product.
  "Storefront" is not used, because the glossary does not define it. Browsing happens on the
  Marketplace and on Shop pages.
- **Out of scope**: Merchant suspension and reinstatement, Cart, Checkout, Reservations, Payments,
  Orders, Shipping Fees, Merchant Balance, Product reviews, discounts, Variant-specific images, image
  editing (cropping, resizing), and import or export of Products. FR-025 is the one place a later
  suspension feature will change: it will also hide a Suspended Merchant's Products.
- **Availability**: Without Reservations, Availability equals stock. The Reservations feature will
  change only that one rule.
- **Currency**: There is one Marketplace currency with no fractional units, and listed prices are
  final, because the glossary does not model tax.
- **Pagination**: The default page size is 20 and the maximum is 100. Lists are ordered newest first
  unless a search ranks them by relevance.
- **Image storage**: The user approved Azure Blob storage, with the Azurite emulator locally, on
  2026-11-05. It is a new dependency and requires a constitution amendment to the Technology
  Constraints (see plan).
- **Seed data**: Local development seeds a starter Category tree and a Merchant owner Account in
  addition to 001's development Accounts, so the feature can be demonstrated without setup.
- **Caching**: No caching is required. The constitution allows caching only where a spec justifies
  it, and SC-003 is expected to be met without it.
- **Web app**: The web app gets Marketplace browsing, Product and Shop pages, an "open your Shop" flow,
  a Merchant Centre with the Product form, Variant grid and image manager, and a Platform Operator
  area for Categories and Product removal. Visual design stays minimal.
