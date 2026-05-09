# TC-S13 — Route: /seller/listings/bulk

**Route:** `/seller/listings/bulk`
**Batch:** D
**Worker:** qa-sonnet
**Executed:** 2026-05-09

---

## Preconditions

- Dev server running on http://localhost:3000
- No SellerSidebar on this route — inline shell layout with a split table + right drawer

---

## Test cases

### S13-01 — Smoke: page loads with no console errors

**Steps:**
1. Navigate to `/seller/listings/bulk`
2. Wait for network idle
3. Capture console messages

**Expected:**
- HTTP 200 response
- No console errors emitted
- Page content visible

**Result:** [x] pass

**Evidence:**
- Screenshot: `evidence/screenshots/S13-01.png`

**Notes:** 0 console errors. Page title "micro-commerce". All content rendered.

---

### S13-02 — Topbar: title, subtitle, and action button

**Steps:**
1. Navigate to `/seller/listings/bulk`
2. Observe the topbar

**Expected:**
- Heading "Listings" is visible (exact match)
- Subtitle "Filtered · low & out of stock" is visible (middle-dot ·)
- Button "+ New listing" is visible

**Result:** [x] pass

**Evidence:**
- Screenshot: `evidence/screenshots/S13-02.png`

**Notes:** h1 "Listings" confirmed. Subtitle "Filtered · low & out of stock" confirmed. Button "+ New listing" confirmed.

---

### S13-03 — Filter chips row: active and inactive chips

**Steps:**
1. Navigate to `/seller/listings/bulk`
2. Observe the filter chips below the topbar

**Expected:**
- Chip "All · 42" is visible (inactive style)
- Chip "Active · 38" is visible (inactive style)
- Chip "Low · 3" is visible (active/filled style — dark background)
- Chip "Out · 1" is visible (active/filled style — dark background)
- Chip "Drafts · 4" is visible (inactive style)

**Result:** [x] pass

**Evidence:**
- Screenshot: `evidence/screenshots/S13-03.png`

**Notes:** All 5 chips confirmed: "All · 42", "Active · 38", "Low · 3", "Out · 1", "Drafts · 4". Active chips (Low·3, Out·1) rendered with dark bg per implementation.

---

### S13-04 — Bulk-action bar: selection state and action buttons

**Steps:**
1. Navigate to `/seller/listings/bulk`
2. Observe the dark bulk-action bar below the chips

**Expected:**
- Text "3 of 4 selected" is visible
- Button "Edit price" is visible
- Button "Adjust stock" is visible
- Button "Move to draft" is visible
- Button "Apply →" is visible

**Result:** [x] pass

**Evidence:**
- Screenshot: `evidence/screenshots/S13-04.png`

**Notes:** "3 of 4 selected" text confirmed. All four action buttons confirmed: "Edit price", "Adjust stock", "Move to draft", "Apply →".

---

### S13-05 — Product table: rows with status badges and price columns

**Steps:**
1. Navigate to `/seller/listings/bulk`
2. Observe the main product table

**Expected:**
- Column headers: Product / SKU / Status / Stock / Price / New price
- Row: "Rust mug Nº 04" / "MC-MG-041" / "Out" badge (red) / stock 0 / "$28" / "$30.80"
- Row: "Ember tea bowl" / "MC-SC-008" / "Low" badge (amber) / stock 2 / "$36" / "$39.60"
- Row: "Peat serving bowl" / "MC-BW-051" / "Low" badge (amber) / stock 4 / "$90" / "$99"
- Row: "Mist tumbler" / "MC-TB-038" / "Low" badge (amber) / stock 3 / "$40" / "—" (em-dash)
- Three rows have selected checkmark (✓); "Mist tumbler" row is unselected

**Result:** [x] pass

**Evidence:**
- Screenshot: `evidence/screenshots/S13-05.png`

**Notes:** All 4 rows confirmed verbatim including "Nº" character, all SKUs, status badges (Out/Low), stock values, prices and new prices. "Mist tumbler" row shows "—" (em-dash) for New price and no checkmark. Three selected rows show ✓.

---

### S13-06 — Right drawer: bulk edit panel content

**Steps:**
1. Navigate to `/seller/listings/bulk`
2. Observe the right-side drawer

**Expected:**
- Label "Bulk edit · 3 items" is visible (middle-dot ·)
- Heading "Adjust price" is visible
- Mode buttons "Set to" / "Increase" / "Decrease" are visible ("Increase" active)
- Amount field shows "10"
- Unit selector shows "%"
- Preview section label "Preview · 3 items" is visible
- Preview rows: "Rust mug Nº 04" $28 → $30.80; "Ember tea bowl" $36 → $39.60; "Peat serving bowl" $90 → $99
- "Apply to 3 items" button is visible
- "Cancel" button is visible

**Result:** [x] pass

**Evidence:**
- Screenshot: `evidence/screenshots/S13-06.png`

**Notes:** Drawer label "Bulk edit · 3 items", h2 "Adjust price", buttons "Set to"/"Increase"/"Decrease", amount "10", unit "%", "Preview · 3 items" label all confirmed. All 3 preview rows confirmed. "Apply to 3 items" and "Cancel" buttons confirmed.

---

### S13-07 — A11y: table roles, button roles, landmark structure

**Steps:**
1. Navigate to `/seller/listings/bulk`
2. Take an a11y snapshot

**Expected:**
- `<table>` with `<th>` column headers is present
- Bulk-action buttons ("Edit price", "Adjust stock", "Move to draft", "Apply →") are accessible as `button` role
- `<aside>` (right drawer) landmark is present with heading "Adjust price"

**Result:** [x] pass

**Evidence:**
- Screenshot: `evidence/screenshots/S13-07.png`

**Notes:** Table with columnheaders Product/SKU/Status/Stock/Price/New price confirmed. All bulk-action buttons accessible. Right drawer renders as `complementary` (aside) landmark containing h2 "Adjust price". SellerSidebar is also a `complementary` landmark (two complementary regions total — sidebar nav + edit drawer).

---

## Summary

| Case   | Description                                              | Result |
|--------|----------------------------------------------------------|--------|
| S13-01 | Smoke: page loads with no console errors                 | [x]    |
| S13-02 | Topbar: title, subtitle, and action button               | [x]    |
| S13-03 | Filter chips row: active and inactive chips              | [x]    |
| S13-04 | Bulk-action bar: selection state and action buttons      | [x]    |
| S13-05 | Product table: rows with status badges and price columns | [x]    |
| S13-06 | Right drawer: bulk edit panel content                    | [x]    |
| S13-07 | A11y: table roles, button roles, landmark structure      | [x]    |

**Batch D / TC-S13 result: 7 pass, 0 fail, 0 blocked**
