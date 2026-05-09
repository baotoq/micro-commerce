# TC-S02 — Route: /seller/listings

**Route:** `/seller/listings`
**Batch:** A
**Worker:** worker-1
**Executed:** 2026-05-09

---

## Preconditions

- Dev server running on http://localhost:3000

---

## Test cases

### S02-01 — Heading and listing count render

**Steps:**
1. Navigate to `/seller/listings`

**Expected:**
- Heading "Listings" (exact)
- Text "42 listings · 38 published"

**Result:** [x] pass

**Evidence:**
- Screenshot: `evidence/screenshots/S02-01.png`

**Notes:** Heading "Listings" at ref e11; "42 listings · 38 published" in paragraph StaticText.

---

### S02-02 — Filter chips render

**Steps:**
1. Navigate to `/seller/listings`

**Expected:**
- "All · 42" (exact)
- "Active · 34" (exact)
- "Low · 3" (exact)
- "Out · 1" (exact)
- "Drafts · 4" (exact)

**Result:** [x] pass

**Evidence:**
- Screenshot: `evidence/screenshots/S02-01.png`

**Notes:** All five filter chip buttons confirmed (refs e6–e10).

---

### S02-03 — Toolbar buttons render

**Steps:**
1. Navigate to `/seller/listings`

**Expected:**
- Button matching `/Import CSV/`
- Button matching `/New listing/`

**Result:** [x] pass

**Evidence:**
- Screenshot: `evidence/screenshots/S02-01.png`

**Notes:** "Import CSV" (ref e12) and "New listing" (ref e13) buttons confirmed.

---

### S02-04 — First-page SKU rows render

**Steps:**
1. Navigate to `/seller/listings`

**Expected:**
- Table cells with SKUs: MC-VS-001, MC-VS-002, MC-BW-014, MC-TB-007, MC-CR-003, MC-PL-022, MC-MG-041, MC-SC-008, MC-VS-031
- Cell "Persimmon vase" (exact)
- Text "9 of 42 shown"

**Result:** [x] pass

**Evidence:**
- Screenshot: `evidence/screenshots/S02-01.png`

**Notes:** All 9 SKU cells confirmed (refs e15, e22, e29, e36, e43, e50, e57, e64, e71). "Persimmon vase" at ref e16. "9 of 42 shown" StaticText present.

---

### S02-05 — Listings nav link navigates from overview

**Steps:**
1. Navigate to `/seller`
2. Click nav link "Listings" (exact)

**Expected:**
- URL becomes `/seller/listings`

**Result:** [x] pass

**Evidence:**
- Screenshot: `evidence/screenshots/S02-05.png`

**Notes:** Clicked ref e3 ("Listings" link) from /seller; eval "window.location.pathname" returned "/seller/listings".

---

## Summary

| Case   | Description                        | Result    |
|--------|------------------------------------|-----------|
| S02-01 | Heading + listing count            | [x] pass  |
| S02-02 | Filter chips                       | [x] pass  |
| S02-03 | Toolbar buttons                    | [x] pass  |
| S02-04 | First-page SKU rows + pagination   | [x] pass  |
| S02-05 | Listings nav link from overview    | [x] pass  |
