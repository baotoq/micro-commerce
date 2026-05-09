# TC-S03 — Route: /seller/analytics

**Route:** `/seller/analytics`
**Batch:** A
**Worker:** worker-1
**Executed:** 2026-05-09

---

## Preconditions

- Dev server running on http://localhost:3000

---

## Test cases

### S03-01 — Heading and date range render

**Steps:**
1. Navigate to `/seller/analytics`

**Expected:**
- Heading "Analytics" (exact)
- Text "Apr 1 – Apr 30 · vs Mar 1 – Mar 30"

**Result:** [x] pass

**Evidence:**
- Screenshot: `evidence/screenshots/S03-01.png`

**Notes:** Heading "Analytics" at ref e9; date range text confirmed in paragraph StaticText.

---

### S03-02 — Range tab buttons and Export button render

**Steps:**
1. Navigate to `/seller/analytics`

**Expected:**
- Button "7d" (exact)
- Button "30d" (exact)
- Button "90d" (exact)
- Button "Year" (exact)
- Button matching `/Export/`

**Result:** [x] pass

**Evidence:**
- Screenshot: `evidence/screenshots/S03-01.png`

**Notes:** Buttons 7d (e10), 30d (e11), 90d (e12), Year (e13), Export (e14) all confirmed.

---

### S03-03 — KPI labels and key values render

**Steps:**
1. Navigate to `/seller/analytics`

**Expected:**
- Text "Revenue" (exact)
- Text "Orders" (exact)
- Text "Conversion" (exact)
- Text "Avg. order" (exact)
- Text "$12,480.00"
- Text matching `/3\.4%/`

**Result:** [x] pass

**Evidence:**
- Screenshot: `evidence/screenshots/S03-01.png`

**Notes:** All four KPI labels present as StaticText. "$12,480.00" and "3.4%" confirmed in snapshot.

---

### S03-04 — Sources card renders

**Steps:**
1. Navigate to `/seller/analytics`

**Expected:**
- Heading "Sources"
- Text "Organic search"
- Text "2,304"

**Result:** [x] pass

**Evidence:**
- Screenshot: `evidence/screenshots/S03-01.png`

**Notes:** "Sources" heading at ref e6. "Organic search" and "2,304" confirmed in listitem StaticText nodes.

---

### S03-05 — Top products and conversion funnel render

**Steps:**
1. Navigate to `/seller/analytics`

**Expected:**
- Heading "Top products"
- Text "Persimmon vase"
- Heading "Conversion funnel"
- Text "Storefront views" (exact)
- Text "Product views" (exact)
- Text "Added to cart" (exact)
- Text "Checkout started" (exact)
- Text "Purchased" (exact)

**Result:** [x] pass

**Evidence:**
- Screenshot: `evidence/screenshots/S03-01.png`

**Notes:** "Top products" heading (ref e7), "Persimmon vase" in list, "Conversion funnel" heading (ref e8), all five funnel stages confirmed.

---

### S03-06 — Analytics nav link navigates from overview

**Steps:**
1. Navigate to `/seller`
2. Click nav link "Analytics" (exact)

**Expected:**
- URL becomes `/seller/analytics`

**Result:** [x] pass

**Evidence:**
- Screenshot: `evidence/screenshots/S03-06.png`

**Notes:** Clicked ref e4 ("Analytics" link) from /seller; eval "window.location.pathname" returned "/seller/analytics".

---

## Summary

| Case   | Description                           | Result    |
|--------|---------------------------------------|-----------|
| S03-01 | Heading + date range                  | [x] pass  |
| S03-02 | Range tabs + Export button            | [x] pass  |
| S03-03 | KPI labels + key values               | [x] pass  |
| S03-04 | Sources card                          | [x] pass  |
| S03-05 | Top products + conversion funnel      | [x] pass  |
| S03-06 | Analytics nav link from overview      | [x] pass  |
