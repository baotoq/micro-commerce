# TC-S07 — Route: /seller/listings/new

**Route:** `/seller/listings/new`
**Batch:** B
**Worker:** worker-2
**Executed:** 2026-05-09

---

## Preconditions

- Dev server running on http://localhost:3000
- SellerSidebar is present (route is in the seller dashboard shell)

---

## Test cases

### S07-01 — Topbar title, subtitle, and action buttons

**Steps:**
1. Navigate to `/seller/listings/new`
2. Observe the topbar

**Expected:**
- Heading "New listing" is visible (exact match)
- Subtitle contains "Listings · Drafts"
- Button "Save draft" is visible
- Button "Publish" is visible

**Result:** [x] pass

**Evidence:**
- Screenshot: `evidence/screenshots/S07-01.png`

**Notes:** Subtitle "LISTINGS · DRAFTS · 1 OF 1" (CSS uppercase). "Save draft" and "✓ Publish" buttons both present.

---

### S07-02 — Photos section with upload state

**Steps:**
1. Navigate to `/seller/listings/new`
2. Observe the Photos card

**Expected:**
- Section heading "Photos · 3 of 6" is visible
- Three photo thumbnails visible (clay, rose, cream tones)
- An uploading placeholder is visible (progress bar + filename "vase-04.heic · 1.4mb")
- A drag-to-add slot is visible with text "Drag to add"

**Result:** [x] pass

**Evidence:**
- Screenshot: `evidence/screenshots/S07-02.png`

**Notes:** Heading "Photos · 3 of 6" confirmed. "vase-04.heic · 1.4mb" upload text and "Drag to add" slot both present. Photo thumbnails are visual elements (rendered as images, not in a11y tree).

---

### S07-03 — Title and description card

**Steps:**
1. Navigate to `/seller/listings/new`
2. Observe the "Title & description" card

**Expected:**
- Section heading "Title & description" is visible
- Title field shows "Persimmon vase"
- Description textarea contains text starting with "Hand-thrown stoneware"
- Character count "142 / 800" is visible
- "Markdown OK" hint is visible

**Result:** [x] pass

**Evidence:**
- Screenshot: `evidence/screenshots/S07-03.png`

**Notes:** All elements confirmed. Description text starts with "Hand-thrown stoneware with a soft persimmon glaze." Character count "142 / 800" present.

---

### S07-04 — Price and stock card

**Steps:**
1. Navigate to `/seller/listings/new`
2. Observe the "Price & stock" card

**Expected:**
- Section heading "Price & stock" is visible
- Price field shows "$86.00"
- Stock field shows "12"
- Price suggestion banner "Suggested: $78–$94 based on 6 similar shops" is visible

**Result:** [x] pass

**Evidence:**
- Screenshot: `evidence/screenshots/S07-04.png`

**Notes:** Price rendered as separate "$" and "86.00" text nodes (visually "$86.00"). Stock "12" and suggestion banner confirmed.

---

### S07-05 — Category and tags card

**Steps:**
1. Navigate to `/seller/listings/new`
2. Observe the "Category & tags" card

**Expected:**
- Section heading "Category & tags" is visible
- Category shows "Ceramics · Vessels"
- Tags "hand-thrown", "stoneware", "persimmon", "small batch" are visible
- "+ add" tag chip is visible

**Result:** [x] pass

**Evidence:**
- Screenshot: `evidence/screenshots/S07-05.png`

**Notes:** Category rendered as "Ceramics · " + "Vessels" (two text nodes). All four tags and "+ add" chip confirmed.

---

### S07-06 — Listing health score card

**Steps:**
1. Navigate to `/seller/listings/new`
2. Observe the "Listing health" card

**Expected:**
- Text "Listing health" is visible
- Score "92" is visible
- A progress bar is visible (filled to ~92%)
- Description text "3 photos · clear title · price set · description over 100 chars. Add 1 more photo to reach 100." is visible

**Result:** [x] pass

**Evidence:**
- Screenshot: `evidence/screenshots/S07-06.png`

**Notes:** All elements confirmed in snapshot.

---

### S07-07 — SellerSidebar present with Listings active

**Steps:**
1. Navigate to `/seller/listings/new`
2. Observe the left sidebar

**Expected:**
- SellerSidebar navigation is present
- "Listings" item is highlighted/active

**Result:** [x] pass

**Evidence:**
- Screenshot: `evidence/screenshots/S07-07.png`

**Notes:** `complementary` sidebar with nav links present. Listings link confirmed (ref=e10).

---

## Summary

| Case   | Description                                      | Result |
|--------|--------------------------------------------------|--------|
| S07-01 | Topbar title, subtitle, and action buttons       | [x]    |
| S07-02 | Photos section with upload state                 | [x]    |
| S07-03 | Title and description card                       | [x]    |
| S07-04 | Price and stock card                             | [x]    |
| S07-05 | Category and tags card                           | [x]    |
| S07-06 | Listing health score card                        | [x]    |
| S07-07 | SellerSidebar present with Listings active       | [x]    |

**Batch B / TC-S07 result: 7 pass, 0 fail, 0 blocked**
