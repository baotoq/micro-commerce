# TC-S10 — Route: /seller/first-month

**Route:** `/seller/first-month`
**Batch:** C
**Worker:** worker-3
**Executed:** 2026-05-09

---

## Preconditions

- Dev server running on http://localhost:3000
- Navigate to `/seller/first-month`

---

## Test cases

### S10-01 — Page loads with SellerSidebar and topbar

**Steps:**
1. Navigate to `/seller/first-month`
2. Observe the sidebar and topbar

**Expected:**
- SellerSidebar is visible
- Heading "Your first month" is visible
- Subtitle "Analytics · March 12 → April 11" is visible

**Result:** [x] pass / [ ] fail / [ ] blocked

**Evidence:**
- Screenshot: `evidence/screenshots/S10-01.png`
- Console log: `evidence/console-logs/S10-01.txt` _(fail only)_

**Notes:** —

---

### S10-02 — Topbar action buttons present

**Steps:**
1. Navigate to `/seller/first-month`
2. Observe the topbar action buttons

**Expected:**
- Button "Compare" is visible
- Button containing "Export" is visible

**Result:** [x] pass / [ ] fail / [ ] blocked

**Evidence:**
- Screenshot: `evidence/screenshots/S10-02.png`
- Console log: `evidence/console-logs/S10-02.txt` _(fail only)_

**Notes:** —

---

### S10-03 — Four KPI sparkline cards are present

**Steps:**
1. Navigate to `/seller/first-month`
2. Observe the four metric cards at the top of the content area

**Expected:**
- "Revenue" label is visible
- "Orders" label is visible
- "Conversion" label is visible
- "Repeat buyers" label is visible
- "$2,148.00" value is visible (Revenue KPI)

**Result:** [x] pass / [ ] fail / [ ] blocked

**Evidence:**
- Screenshot: `evidence/screenshots/S10-03.png`
- Console log: `evidence/console-logs/S10-03.txt` _(fail only)_

**Notes:** —

---

### S10-04 — Daily revenue chart renders with controls and annotation

**Steps:**
1. Navigate to `/seller/first-month`
2. Observe the large daily revenue chart card

**Expected:**
- "Daily revenue" label is visible
- Period tabs "Day", "Week", "Month" are visible
- Chart annotation "Day 4 · first sale" is visible

**Result:** [x] pass / [ ] fail / [ ] blocked

**Evidence:**
- Screenshot: `evidence/screenshots/S10-04.png`
- Console log: `evidence/console-logs/S10-04.txt` _(fail only)_

**Notes:** —

---

### S10-05 — Insight card shows Friday afternoon insight

**Steps:**
1. Navigate to `/seller/first-month`
2. Observe the insight panel

**Expected:**
- "★ Insight" label is visible
- Heading containing "Friday afternoons sell 2.3× more" is visible

**Result:** [x] pass / [ ] fail / [ ] blocked

**Evidence:**
- Screenshot: `evidence/screenshots/S10-05.png`
- Console log: `evidence/console-logs/S10-05.txt` _(fail only)_

**Notes:** —

---

### S10-06 — Top sellers card lists three products

**Steps:**
1. Navigate to `/seller/first-month`
2. Observe the top sellers panel

**Expected:**
- Heading "Top sellers" is visible
- "Persimmon vase" is visible
- "Forest bowl, lg." is visible
- "Cream tumbler set" is visible

**Result:** [x] pass / [ ] fail / [ ] blocked

**Evidence:**
- Screenshot: `evidence/screenshots/S10-06.png`
- Console log: `evidence/console-logs/S10-06.txt` _(fail only)_

**Notes:** —

---

## Summary

| Case   | Description                                        | Result |
|--------|----------------------------------------------------|--------|
| S10-01 | Page loads with SellerSidebar and topbar           | [x]    |
| S10-02 | Topbar action buttons present                      | [x]    |
| S10-03 | Four KPI sparkline cards are present               | [x]    |
| S10-04 | Daily revenue chart with controls and annotation   | [x]    |
| S10-05 | Insight card shows Friday afternoon insight        | [x]    |
| S10-06 | Top sellers card lists three products              | [x]    |
