# Phase 2 Context: Catalog Domain & Admin CRUD

**Captured:** 2026-01-30
**Source:** User discussion

---

## Vision

### Admin UI Experience

**Table-based list with slide-out drawer** for create/edit:
- Products displayed in data table (name, price, category, status)
- Slide-out drawer for create/edit forms (more space than modal)
- Inline quick actions per row (edit, delete, toggle status)
- Search and filter capabilities (by category, by status)

**Visual Layout:**
```
┌─────────────────────────────────────────────────────────────────┐
│  Products                                    [+ Add Product]    │
├─────────────────────────────────────────────────────────────────┤
│  🔍 Search...                    Category: [All ▾]  Status: [▾] │
├─────────────────────────────────────────────────────────────────┤
│  □  Image  Name           Category     Price    Status   Actions│
│  ─────────────────────────────────────────────────────────────  │
│  □  [img]  MacBook Pro    Electronics  $1,999   Published  ⋮    │
│  □  [img]  Running Shoes  Footwear     $129     Draft      ⋮    │
└─────────────────────────────────────────────────────────────────┘
```

### Routes

- `/admin/products` — Product list with table view
- `/admin/products/new` — Create (or drawer from list)
- `/admin/products/[id]/edit` — Edit (or drawer from list)
- `/admin/categories` — Category management (simple CRUD)

---

## Product Model

| Field | Type | Required | Notes |
|-------|------|----------|-------|
| Name | string | ✓ | Max 200 chars |
| Description | text | ✓ | Rich text or markdown |
| Price | decimal | ✓ | USD, 2 decimal places |
| ImageUrl | URL | ✓ | Azure Blob URL after upload |
| CategoryId | FK | ✓ | Single category (flat list) |
| SKU | string | ✗ | Optional product code |
| Status | enum | ✓ | Draft / Published / Archived |
| CreatedAt | datetime | ✓ | Auto-generated |
| UpdatedAt | datetime | ✓ | Auto-updated |

**Soft delete:** Products archived (status change), not hard deleted.

---

## Category Model

- **Flat list** (no hierarchy)
- Name only (simple)
- Used as foreign key in Product

---

## Image Upload

- **Azure Blob Storage** integration
- Upload endpoint returns blob URL
- Product stores URL reference (not binary)
- Aspire integration for local dev (Azurite emulator)

---

## Domain Events

- `ProductCreated` — when new product saved
- `ProductUpdated` — when product edited  
- `ProductStatusChanged` — when status transitions
- `ProductDeleted` — when soft deleted (archived)

---

## Out of Scope (Phase 2)

- Product variants (size/color)
- Multiple images / gallery
- Bulk import/export
- Inventory levels (Phase 4)
- SEO metadata
- Price history

---

## User Decisions

- Admin routes at `/admin/products` ✓
- Flat category structure ✓
- File upload to Azure Blob Storage ✓

