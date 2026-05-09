# TC-S14 — Route: /seller/listings/[sku]/edit

**Route:** `/seller/listings/MC-VS-001/edit`
**Batch:** D
**Worker:** qa-sonnet
**Executed:** 2026-05-09

---

## Preconditions

- Dev server running on http://localhost:3000
- SKU `MC-VS-001` ("Persimmon vase", category "Vessels") exists in the data layer
- No SellerSidebar on this route — inline shell layout with header + two-column body

---

## Test cases

### S14-01 — Smoke: page loads with no console errors

**Steps:**
1. Navigate to `/seller/listings/MC-VS-001/edit`
2. Wait for network idle
3. Capture console messages

**Expected:**
- HTTP 200 response
- No console errors emitted
- Page content visible

**Result:** [x] pass

**Evidence:**
- Screenshot: `evidence/screenshots/S14-01.png`

**Notes:** 0 console errors. Page title "micro-commerce". All content rendered.

---

### S14-02 — Header: breadcrumb, title, unsaved-changes badge, action buttons

**Steps:**
1. Navigate to `/seller/listings/MC-VS-001/edit`
2. Observe the page header

**Expected:**
- Breadcrumb label "Listings · Vessels" is visible (middle-dot ·)
- `<h1>` "Persimmon vase" is visible (exact match)
- "Unsaved changes" amber badge is visible
- Back button (‹) is present with aria-label "Back"
- "Discard" button is visible
- "Save draft" button is visible
- "Publish →" button is visible

**Result:** [x] pass

**Evidence:**
- Screenshot: `evidence/screenshots/S14-02.png`

**Notes:** Breadcrumb "Listings · Vessels" confirmed. h1 "Persimmon vase" confirmed. "Unsaved changes" badge confirmed. Back button with aria-label "Back" confirmed. All three action buttons confirmed.

---

### S14-03 — Variant matrix: heading, subtitle, and table rows

**Steps:**
1. Navigate to `/seller/listings/MC-VS-001/edit`
2. Observe the "Variant matrix" card

**Expected:**
- `<h2>` "Variant matrix" is visible
- Subtitle "Size × Glaze · 6 combinations" is visible (middle-dot ·)
- "+ Add option" button is visible
- Column headers: Variant / SKU / Price / Stock / Status
- Row: "Small · Persimmon" / "MS-VS-001-S" / "$70" (green, changed) / 6 / "Active" badge
- Row: "Medium · Persimmon" / "MS-VS-001-M" / "$95" (green, changed) / 4 / "Active" badge
- Row: "Large · Persimmon" / "MS-VS-001-L" / "$136" / 0 / "Out" badge (red)
- Row: "Small · Cream" / "MS-VS-001-SC" / "$70" / 8 / "Active" badge
- Row: "Medium · Cream" / "MS-VS-001-MC" / "$95" / 5 / "Active" badge
- Row: "Large · Cream" / "MS-VS-001-LC" / "$136" / 2 / "Low" badge (amber)
- Footer note "● 2 variants updated · prices +10%" is visible

**Result:** [x] pass

**Evidence:**
- Screenshot: `evidence/screenshots/S14-03.png`

**Notes:** h2 "Variant matrix" and subtitle "Size × Glaze · 6 combinations" confirmed. "+ Add option" button confirmed. All 6 variant rows with correct label/SKU/price/stock/status confirmed. Footer note "● 2 variants updated · prices +10%" confirmed.

---

### S14-04 — Photos card and Status card in right column

**Steps:**
1. Navigate to `/seller/listings/MC-VS-001/edit`
2. Observe the right column cards

**Expected:**
- `<h3>` "Photos · 4 of 8" is visible (middle-dot ·)
- Four photo thumbnail swatches are visible
- An "+" add-photo slot with dashed border is visible
- `<h3>` "Status" is visible
- Status segmented control with "Active" / "Draft" / "Archived" buttons is visible ("Active" is selected)
- "Visible at /persimmon-vase" text is visible (slug derived from listing name)

**Result:** [x] pass

**Evidence:**
- Screenshot: `evidence/screenshots/S14-04.png`

**Notes:** h3 "Photos · 4 of 8" confirmed. "+" slot confirmed. h3 "Status" confirmed. Buttons "Active"/"Draft"/"Archived" confirmed. Paragraph "Visible at /persimmon-vase" confirmed (slug computed from listing.name "Persimmon vase").

---

### S14-05 — Layout: seller shell sidebar present, two-column body grid

**Steps:**
1. Navigate to `/seller/listings/MC-VS-001/edit`
2. Take an a11y snapshot to inspect landmarks

**Expected:**
- Seller shell layout with sidebar and `main` content area
- Body content uses two-column grid: left (variant matrix) and right (photos + status)

**Result:** [x] pass

**Evidence:**
- Screenshot: `evidence/screenshots/S14-05.png`

**Notes:** FINDING — SellerSidebar IS present (complementary landmark) consistent with all other seller routes. Body grid confirmed: left card has variant matrix table, right column has Photos and Status cards.

---

### S14-06 — A11y: heading hierarchy, button roles

**Steps:**
1. Navigate to `/seller/listings/MC-VS-001/edit`
2. Take an a11y snapshot

**Expected:**
- `<h1>` "Persimmon vase" is present
- `<h2>` "Variant matrix" is present
- `<h3>` "Photos · 4 of 8" and `<h3>` "Status" are present
- Buttons "Discard", "Save draft", "Publish →" are accessible as `button` role
- Back button has accessible aria-label "Back"

**Result:** [x] pass

**Evidence:**
- Screenshot: `evidence/screenshots/S14-06.png`

**Notes:** h1 "Persimmon vase", h2 "Variant matrix", h3 "Photos · 4 of 8", h3 "Status" all confirmed in snapshot. All action buttons accessible. Back button (ref=e34) has aria-label "Back".

---

### S14-07 — 404 behavior: unknown SKU returns not-found page

**Steps:**
1. Navigate to `/seller/listings/MC-NOPE-404/edit`
2. Check HTTP response status
3. Verify the page renders a not-found state

**Expected:**
- HTTP response is 404 (not 200)
- Page does not render "Persimmon vase" content
- Next.js not-found UI or a 404 indicator is shown

**Result:** [x] pass

**Evidence:**
- Screenshot: `evidence/screenshots/S14-07.png`

**Notes:** curl returned HTTP 404. Page title "404: This page could not be found." Snapshot shows h1 "404" and h2 "This page could not be found." — Next.js notFound() called correctly when SKU not in data layer.

---

## Summary

| Case   | Description                                                        | Result |
|--------|--------------------------------------------------------------------|--------|
| S14-01 | Smoke: page loads with no console errors                           | [x]    |
| S14-02 | Header: breadcrumb, title, unsaved-changes badge, action buttons   | [x]    |
| S14-03 | Variant matrix: heading, subtitle, and table rows                  | [x]    |
| S14-04 | Photos card and Status card in right column                        | [x]    |
| S14-05 | Layout: seller shell sidebar present, two-column body grid          | [x]    |
| S14-06 | A11y: heading hierarchy, button roles                              | [x]    |
| S14-07 | 404 behavior: unknown SKU returns not-found page                   | [x]    |

**Batch D / TC-S14 result: 7 pass, 0 fail, 0 blocked**
