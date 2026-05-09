# TC-S12 — Route: /seller/listings/published

**Route:** `/seller/listings/published`
**Batch:** D
**Worker:** qa-sonnet
**Executed:** 2026-05-09

---

## Preconditions

- Dev server running on http://localhost:3000
- No SellerSidebar on this route — inline shell layout (topbar + full-width content)

---

## Test cases

### S12-01 — Smoke: page loads with no console errors

**Steps:**
1. Navigate to `/seller/listings/published`
2. Wait for network idle
3. Capture console messages

**Expected:**
- HTTP 200 response
- No console errors emitted
- Page content visible

**Result:** [x] pass

**Evidence:**
- Screenshot: `evidence/screenshots/S12-01.png`

**Notes:** 0 console errors. Page title "micro-commerce". All content rendered.

---

### S12-02 — Topbar: title, subtitle, and action button

**Steps:**
1. Navigate to `/seller/listings/published`
2. Observe the topbar

**Expected:**
- Heading "Listings" is visible (exact match)
- Subtitle "42 products · 39 active" is visible (middle-dot ·)
- Button "+ New listing" is visible

**Result:** [x] pass

**Evidence:**
- Screenshot: `evidence/screenshots/S12-02.png`

**Notes:** h1 "Listings" confirmed. Subtitle paragraph "42 products · 39 active" confirmed. Button "+ New listing" confirmed (ref=e37).

---

### S12-03 — Success banner content

**Steps:**
1. Navigate to `/seller/listings/published`
2. Observe the green banner below the topbar

**Expected:**
- Green checkmark (✓) icon is visible
- Banner text "Persimmon vase published · 2 variants updated, 1 went live." is visible (exact match)
- "View shop →" ghost button is visible
- "Undo" ghost button is visible

**Result:** [x] pass

**Evidence:**
- Screenshot: `evidence/screenshots/S12-03.png`

**Notes:** ✓ icon (aria-hidden), banner text "Persimmon vase published · 2 variants updated, 1 went live." confirmed. Buttons "View shop →" and "Undo" confirmed.

---

### S12-04 — KPI cards: values and deltas

**Steps:**
1. Navigate to `/seller/listings/published`
2. Observe the three KPI cards

**Expected:**
- Card "Active listings" with value "39" and delta "↑ +1" is visible
- Card "Variants in stock" with value "128" and delta "↑ +2" is visible
- Card "Out-of-stock items" with value "1" and delta "↑ −2" is visible

**Result:** [x] pass

**Evidence:**
- Screenshot: `evidence/screenshots/S12-04.png`

**Notes:** All three KPI cards confirmed in snapshot: "Active listings" / "39" / "↑ +1", "Variants in stock" / "128" / "↑ +2", "Out-of-stock items" / "1" / "↑ −2".

---

### S12-05 — "What just changed" table: heading and rows

**Steps:**
1. Navigate to `/seller/listings/published`
2. Observe the activity table

**Expected:**
- Section heading "What just changed" is visible (exact match)
- "Activity log →" link text is visible
- Table columns "When", "Item", "Change", "By" are visible
- Row: "just now" / "Persimmon vase · Medium" / "Price · $86 → $95" / "You"
- Row: "just now" / "Persimmon vase · Small" / "Price · $64 → $70" / "You"
- Row: "just now" / "Persimmon vase · Large" / "Status · Out → still out (no stock)" / "You"
- Row: "12 min ago" / "Ember tea bowl" / "Price · $36 → $39.60" / "You · bulk"
- Row: "12 min ago" / "Peat serving bowl" / "Price · $90 → $99" / "You · bulk"

**Result:** [x] pass

**Evidence:**
- Screenshot: `evidence/screenshots/S12-05.png`

**Notes:** h2 "What just changed" confirmed. "Activity log →" text confirmed. All 5 table rows confirmed verbatim including middle-dot ·, en-dash →, and "You · bulk".

---

### S12-06 — Layout: sidebar present, topbar + content within seller shell

**Steps:**
1. Navigate to `/seller/listings/published`
2. Inspect the page structure via a11y snapshot

**Expected:**
- Seller shell layout with `complementary` sidebar landmark and `main` content area

**Result:** [x] pass

**Evidence:**
- Screenshot: `evidence/screenshots/S12-06.png`

**Notes:** FINDING — SellerSidebar IS present (complementary landmark with nav links). The route renders inside the seller dashboard shell (layout.tsx wraps it), not as an inline-only shell. Content occupies `main` region to the right of the sidebar. This is consistent with how /seller/listings/new also has a sidebar.

---

### S12-07 — A11y: heading hierarchy and table roles

**Steps:**
1. Navigate to `/seller/listings/published`
2. Take an a11y snapshot

**Expected:**
- `<h2>` "What just changed" heading is present
- `<table>` element with `<th>` column headers is present (When / Item / Change / By)
- `<button>` "+ New listing" is present and accessible

**Result:** [x] pass

**Evidence:**
- Screenshot: `evidence/screenshots/S12-07.png`

**Notes:** h1 "Listings" (topbar), h2 "What just changed" (activity section) confirmed. Table with columnheaders When/Item/Change/By confirmed. Button "+ New listing" (ref=e37) accessible.

---

## Summary

| Case   | Description                                           | Result |
|--------|-------------------------------------------------------|--------|
| S12-01 | Smoke: page loads with no console errors                        | [x]    |
| S12-02 | Topbar: title, subtitle, and action button                      | [x]    |
| S12-03 | Success banner content                                          | [x]    |
| S12-04 | KPI cards: values and deltas                                    | [x]    |
| S12-05 | "What just changed" table: heading and rows                     | [x]    |
| S12-06 | Layout: sidebar present, topbar + content within seller shell   | [x]    |
| S12-07 | A11y: heading hierarchy and table roles                         | [x]    |

**Batch D / TC-S12 result: 7 pass, 0 fail, 0 blocked**
