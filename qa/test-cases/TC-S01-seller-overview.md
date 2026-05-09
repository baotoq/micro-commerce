# TC-S01 — Route: /seller (Overview)

**Route:** `/seller`
**Batch:** A
**Worker:** worker-1
**Executed:** 2026-05-09

---

## Preconditions

- Dev server running on http://localhost:3000

---

## Test cases

### S01-01 — Brand name and sidebar nav links render

**Steps:**
1. Navigate to `/seller`

**Expected:**
- Text "Micro Commerce" visible
- Nav links present: Overview, Orders, Listings, Analytics, Customers

**Result:** [x] pass

**Evidence:**
- Screenshot: `evidence/screenshots/S01-01.png`

**Notes:** All five nav links confirmed present (refs e1–e5). "Micro Commerce" in sidebar StaticText.

---

### S01-02 — Greeting heading and date render

**Steps:**
1. Navigate to `/seller`

**Expected:**
- Heading matching `/Good morning, Alex/`
- Text "Tuesday · April 8"

**Result:** [x] pass

**Evidence:**
- Screenshot: `evidence/screenshots/S01-01.png`

**Notes:** Heading "Good morning, Alex" confirmed. Date text rendered as "TUESDAY · APRIL 8" in DOM via CSS text-transform:uppercase; underlying value matches expected.

---

### S01-03 — KPI labels render

**Steps:**
1. Navigate to `/seller`

**Expected:**
- Text "Revenue · 7 days" (exact)
- Text "Orders · 7 days" (exact)
- Text "Storefront views" (exact)

**Result:** [x] pass

**Evidence:**
- Screenshot: `evidence/screenshots/S01-01.png`

**Notes:** All three KPI labels confirmed in snapshot StaticText nodes.

---

### S01-04 — Today panel and Recent orders table render

**Steps:**
1. Navigate to `/seller`

**Expected:**
- Heading "Today" (exact)
- Heading "Recent orders"
- Order IDs in table: #1042, #1041, #1040, #1039, #1038

**Result:** [x] pass

**Evidence:**
- Screenshot: `evidence/screenshots/S01-01.png`

**Notes:** "Today" heading (ref e7), "Recent orders" heading (ref e8), and all five order ID cells confirmed in snapshot.

---

## Summary

| Case   | Description                          | Result |
|--------|--------------------------------------|--------|
| S01-01 | Brand + sidebar nav links            | [x] pass |
| S01-02 | Greeting heading + date              | [x] pass |
| S01-03 | KPI labels                           | [x] pass |
| S01-04 | Today panel + Recent orders table    | [x] pass |
