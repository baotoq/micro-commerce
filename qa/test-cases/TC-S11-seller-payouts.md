# TC-S11 — Route: /seller/payouts

**Route:** `/seller/payouts`
**Batch:** C
**Worker:** worker-3
**Executed:** 2026-05-09

---

## Preconditions

- Dev server running on http://localhost:3000
- Navigate to `/seller/payouts`

---

## Test cases

### S11-01 — Page loads with SellerSidebar and topbar

**Steps:**
1. Navigate to `/seller/payouts`
2. Observe the sidebar and topbar

**Expected:**
- SellerSidebar is visible
- Heading "Payouts" is visible
- Subtitle "Finance · all time" is visible

**Result:** [x] pass / [ ] fail / [ ] blocked

**Evidence:**
- Screenshot: `evidence/screenshots/S11-01.png`
- Console log: `evidence/console-logs/S11-01.txt` _(fail only)_

**Notes:** —

---

### S11-02 — Topbar Statements button is present

**Steps:**
1. Navigate to `/seller/payouts`
2. Observe the topbar

**Expected:**
- Button containing "Statements" is visible

**Result:** [x] pass / [ ] fail / [ ] blocked

**Evidence:**
- Screenshot: `evidence/screenshots/S11-02.png`
- Console log: `evidence/console-logs/S11-02.txt` _(fail only)_

**Notes:** —

---

### S11-03 — Last payout big-number card shows correct amount and status

**Steps:**
1. Navigate to `/seller/payouts`
2. Observe the large payout card (left, top)

**Expected:**
- "Last payout · sent today" label is visible
- "$1,284.62" amount is visible
- "Sent · arriving Wed" chip is visible

**Result:** [x] pass / [ ] fail / [ ] blocked

**Evidence:**
- Screenshot: `evidence/screenshots/S11-03.png`
- Console log: `evidence/console-logs/S11-03.txt` _(fail only)_

**Notes:** —

---

### S11-04 — Last payout card body text and action buttons

**Steps:**
1. Navigate to `/seller/payouts`
2. Observe the body and buttons within the last payout card

**Expected:**
- Text "Nine orders, less Micro's 4% and three shipping labels." is visible
- Button "View receipt" is visible
- Button "Switch to instant payouts" is visible

**Result:** [x] pass / [ ] fail / [ ] blocked

**Evidence:**
- Screenshot: `evidence/screenshots/S11-04.png`
- Console log: `evidence/console-logs/S11-04.txt` _(fail only)_

**Notes:** —

---

### S11-05 — Available next payout sub-card

**Steps:**
1. Navigate to `/seller/payouts`
2. Observe the "Available · next payout" card (right column, top)

**Expected:**
- "Available · next payout" label is visible
- "$184.08" amount is visible

**Result:** [x] pass / [ ] fail / [ ] blocked

**Evidence:**
- Screenshot: `evidence/screenshots/S11-05.png`
- Console log: `evidence/console-logs/S11-05.txt` _(fail only)_

**Notes:** —

---

### S11-06 — Lifetime earned sub-card

**Steps:**
1. Navigate to `/seller/payouts`
2. Observe the "Lifetime earned" card (right column, bottom)

**Expected:**
- "Lifetime earned" label is visible
- "$2,148.36" amount is visible

**Result:** [x] pass / [ ] fail / [ ] blocked

**Evidence:**
- Screenshot: `evidence/screenshots/S11-06.png`
- Console log: `evidence/console-logs/S11-06.txt` _(fail only)_

**Notes:** —

---

### S11-07 — Activity ledger heading and filter chips

**Steps:**
1. Navigate to `/seller/payouts`
2. Observe the activity ledger section

**Expected:**
- Heading "Activity" is visible
- Filter chips "All", "Payouts", "Sales", "Fees" are all visible
- "All" chip is shown as active/selected

**Result:** [x] pass / [ ] fail / [ ] blocked

**Evidence:**
- Screenshot: `evidence/screenshots/S11-07.png`
- Console log: `evidence/console-logs/S11-07.txt` _(fail only)_

**Notes:** —

---

### S11-08 — Activity ledger shows payout and order rows

**Steps:**
1. Navigate to `/seller/payouts`
2. Observe the ledger table rows

**Expected:**
- "Payout · weekly" row is visible
- "Order #1042 · Sasha L." row is visible

**Result:** [x] pass / [ ] fail / [ ] blocked

**Evidence:**
- Screenshot: `evidence/screenshots/S11-08.png`
- Console log: `evidence/console-logs/S11-08.txt` _(fail only)_

**Notes:** —

---

## Summary

| Case   | Description                                          | Result |
|--------|------------------------------------------------------|--------|
| S11-01 | Page loads with SellerSidebar and topbar             | [x]    |
| S11-02 | Topbar Statements button present                     | [x]    |
| S11-03 | Last payout big-number card with amount and status   | [x]    |
| S11-04 | Last payout card body text and action buttons        | [x]    |
| S11-05 | Available next payout sub-card                       | [x]    |
| S11-06 | Lifetime earned sub-card                             | [x]    |
| S11-07 | Activity ledger heading and filter chips             | [x]    |
| S11-08 | Activity ledger shows payout and order rows          | [x]    |
