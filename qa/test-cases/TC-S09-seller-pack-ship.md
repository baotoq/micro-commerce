# TC-S09 — Route: /seller/orders/1001/pack

**Route:** `/seller/orders/1001/pack`
**Batch:** C
**Worker:** worker-3
**Executed:** 2026-05-09

---

## Preconditions

- Dev server running on http://localhost:3000
- Navigate to `/seller/orders/1001/pack`
- The "Buy your shipping label" modal is rendered always-open as a static visual — it is not a closeable dialog

---

## Test cases

### S09-01 — Page loads with SellerSidebar and breadcrumb

**Steps:**
1. Navigate to `/seller/orders/1001/pack`
2. Observe the left sidebar and breadcrumb in the topbar

**Expected:**
- SellerSidebar is visible
- Breadcrumb shows "Orders /"
- Order number "#1001" is visible in the topbar
- Status chip "Needs shipping" is visible

**Result:** [x] pass / [ ] fail / [ ] blocked

**Evidence:**
- Screenshot: `evidence/screenshots/S09-01.png`
- Console log: `evidence/console-logs/S09-01.txt` _(fail only)_

**Notes:** —

---

### S09-02 — Topbar action buttons present

**Steps:**
1. Navigate to `/seller/orders/1001/pack`
2. Observe the buttons in the topbar

**Expected:**
- Button "Message Sasha" is visible
- Button "Print packing slip" is visible

**Result:** [x] pass / [ ] fail / [ ] blocked

**Evidence:**
- Screenshot: `evidence/screenshots/S09-02.png`
- Console log: `evidence/console-logs/S09-02.txt` _(fail only)_

**Notes:** —

---

### S09-03 — Order heading identifies customer and item

**Steps:**
1. Navigate to `/seller/orders/1001/pack`
2. Observe the main heading

**Expected:**
- Heading "Sasha Leblanc · Persimmon vase" is visible

**Result:** [x] pass / [ ] fail / [ ] blocked

**Evidence:**
- Screenshot: `evidence/screenshots/S09-03.png`
- Console log: `evidence/console-logs/S09-03.txt` _(fail only)_

**Notes:** —

---

### S09-04 — Product card shows item details and financials

**Steps:**
1. Navigate to `/seller/orders/1001/pack`
2. Observe the product card (left column)

**Expected:**
- "Persimmon vase" is visible
- "Glazed terra · qty 1" is visible
- "Customer paid" label is visible
- "Micro fee · 4%" label is visible
- "You'll receive" label is visible

**Result:** [x] pass / [ ] fail / [ ] blocked

**Evidence:**
- Screenshot: `evidence/screenshots/S09-04.png`
- Console log: `evidence/console-logs/S09-04.txt` _(fail only)_

**Notes:** —

---

### S09-05 — Ship-to card shows correct address

**Steps:**
1. Navigate to `/seller/orders/1001/pack`
2. Observe the "Ship to" card (right column)

**Expected:**
- "Ship to" label is visible
- "Sasha Leblanc" is visible
- Address "820 Sutter St" is visible

**Result:** [x] pass / [ ] fail / [ ] blocked

**Evidence:**
- Screenshot: `evidence/screenshots/S09-05.png`
- Console log: `evidence/console-logs/S09-05.txt` _(fail only)_

**Notes:** —

---

### S09-06 — Customer note is displayed

**Steps:**
1. Navigate to `/seller/orders/1001/pack`
2. Observe the customer note card

**Expected:**
- Text `"So excited — please pack carefully, this is for my mom."` is visible

**Result:** [x] pass / [ ] fail / [ ] blocked

**Evidence:**
- Screenshot: `evidence/screenshots/S09-06.png`
- Console log: `evidence/console-logs/S09-06.txt` _(fail only)_

**Notes:** —

---

### S09-07 — Shipping label modal is always-open with correct header

**Steps:**
1. Navigate to `/seller/orders/1001/pack`
2. Observe the modal overlay

**Expected:**
- Modal overlay is visible (not a closeable dialog — rendered static)
- "Step 2 of 2" eyebrow text is visible
- Heading "Buy your shipping label" is visible

**Result:** [x] pass / [ ] fail / [ ] blocked

**Evidence:**
- Screenshot: `evidence/screenshots/S09-07.png`
- Console log: `evidence/console-logs/S09-07.txt` _(fail only)_

**Notes:** Modal is always-open; close button present in design but modal does not dismiss.

---

### S09-08 — Modal shows all three shipping options with correct selection

**Steps:**
1. Navigate to `/seller/orders/1001/pack`
2. Observe the three shipping options in the modal

**Expected:**
- "USPS Priority · 1–3 days" is visible and selected (highlighted border)
- "USPS Ground Advantage" is visible and unselected
- "UPS Ground" is visible and unselected

**Result:** [x] pass / [ ] fail / [ ] blocked

**Evidence:**
- Screenshot: `evidence/screenshots/S09-08.png`
- Console log: `evidence/console-logs/S09-08.txt` _(fail only)_

**Notes:** —

---

### S09-09 — Modal footer shows price and primary CTA

**Steps:**
1. Navigate to `/seller/orders/1001/pack`
2. Observe the modal footer area

**Expected:**
- "Buy label · charge to payouts" text is visible
- "$9.84" price is visible in the footer summary row
- Button "Buy & print label →" is visible
- "Marks order shipped automatically when scanned" footer note is visible

**Result:** [x] pass / [ ] fail / [ ] blocked

**Evidence:**
- Screenshot: `evidence/screenshots/S09-09.png`
- Console log: `evidence/console-logs/S09-09.txt` _(fail only)_

**Notes:** —

---

## Summary

| Case   | Description                                          | Result |
|--------|------------------------------------------------------|--------|
| S09-01 | Page loads with SellerSidebar and breadcrumb         | [x]    |
| S09-02 | Topbar action buttons present                        | [x]    |
| S09-03 | Order heading identifies customer and item           | [x]    |
| S09-04 | Product card shows item details and financials       | [x]    |
| S09-05 | Ship-to card shows correct address                   | [x]    |
| S09-06 | Customer note is displayed                           | [x]    |
| S09-07 | Shipping label modal is always-open with header      | [x]    |
| S09-08 | Modal shows three shipping options, USPS selected    | [x]    |
| S09-09 | Modal footer shows price and primary CTA             | [x]    |
