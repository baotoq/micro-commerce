# TC-S08 — Route: /seller/first-order

**Route:** `/seller/first-order`
**Batch:** B
**Worker:** worker-2
**Executed:** 2026-05-09

---

## Preconditions

- Dev server running on http://localhost:3000
- SellerSidebar is present (route is in the seller dashboard shell)

---

## Test cases

### S08-01 — Topbar heading and date subtitle

**Steps:**
1. Navigate to `/seller/first-order`
2. Observe the topbar

**Expected:**
- Heading contains "Good afternoon, Alex"
- Subtitle "Day 4 · Friday, March 15" is visible

**Result:** [x] pass

**Evidence:**
- Screenshot: `evidence/screenshots/S08-01.png`

**Notes:** Heading "Good afternoon, Alex" confirmed. Subtitle "DAY 4 · FRIDAY, MARCH 15" (CSS uppercase). Brand owner correctly shown as Alex.

---

### S08-02 — Celebration banner content

**Steps:**
1. Navigate to `/seller/first-order`
2. Observe the celebration banner at the top of the content area

**Expected:**
- Label "★ Your first order" is visible
- Heading "Sasha bought a Persimmon vase." is visible (exact match)
- Body text "$86.00 · placed 12 minutes ago · we held it for you to confirm." is visible
- Button "Send a thank-you" is visible
- Button "Open order →" is visible

**Result:** [x] pass

**Evidence:**
- Screenshot: `evidence/screenshots/S08-02.png`

**Notes:** Banner label "★ YOUR FIRST ORDER" (CSS uppercase). Heading and body text exact match confirmed. Both buttons present.

---

### S08-03 — Four KPI stat cards

**Steps:**
1. Navigate to `/seller/first-order`
2. Observe the KPI stat row below the banner

**Expected:**
- Card label "Sales · today" is visible
- Card label "Orders · today" is visible
- Card label "Visits · today" is visible
- Card label "Followers" is visible

**Result:** [x] pass

**Evidence:**
- Screenshot: `evidence/screenshots/S08-03.png`

**Notes:** All four stat labels confirmed in snapshot. Values: Sales $86, Orders 1, Visits 142, Followers 14.

---

### S08-04 — Orders table with first order row

**Steps:**
1. Navigate to `/seller/first-order`
2. Observe the Orders table

**Expected:**
- Table heading "Orders" is visible
- Order row "#1001" is visible
- Customer "Sasha L." is visible in the row
- Item "Persimmon vase" is visible in the row
- Status chip "New · pack today" is visible
- Link "All orders →" is visible

**Result:** [x] pass

**Evidence:**
- Screenshot: `evidence/screenshots/S08-04.png`

**Notes:** All table elements confirmed. Order cell ref=e12 "#1001", customer ref=e13 "Sasha L.", item ref=e14 "Persimmon vase", status ref=e16 "New · pack today", "All orders →" link ref=e3.

---

### S08-05 — Orders table footer note

**Steps:**
1. Navigate to `/seller/first-order`
2. Observe the bottom of the orders table

**Expected:**
- Text "Funds are released to your bank 2 days after the order ships." is visible

**Result:** [x] pass

**Evidence:**
- Screenshot: `evidence/screenshots/S08-05.png`

**Notes:** Footer text confirmed verbatim in snapshot.

---

### S08-06 — SellerSidebar present with Overview active

**Steps:**
1. Navigate to `/seller/first-order`
2. Observe the left sidebar

**Expected:**
- SellerSidebar navigation is present
- "Overview" item is highlighted/active

**Result:** [x] pass

**Evidence:**
- Screenshot: `evidence/screenshots/S08-06.png`

**Notes:** `complementary` sidebar confirmed with Overview, Orders, Listings, Analytics, Customers links (ref=e4–e8).

---

## Summary

| Case   | Description                                 | Result |
|--------|---------------------------------------------|--------|
| S08-01 | Topbar heading and date subtitle            | [x]    |
| S08-02 | Celebration banner content                  | [x]    |
| S08-03 | Four KPI stat cards                         | [x]    |
| S08-04 | Orders table with first order row           | [x]    |
| S08-05 | Orders table footer note                    | [x]    |
| S08-06 | SellerSidebar present with Overview active  | [x]    |

**Batch B / TC-S08 result: 6 pass, 0 fail, 0 blocked**
