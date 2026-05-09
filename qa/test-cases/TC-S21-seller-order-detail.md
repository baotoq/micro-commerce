# TC-S21 — Route: /seller/orders/1042

**Route:** `/seller/orders/1042`
**Batch:** F
**Worker:** worker-mgmt-1
**Executed:** 2026-05-10

---

## Preconditions

- Dev server running on http://localhost:3000
- Route renders within the seller layout (SellerSidebar + main content)
- Static fixture data — order #1042 for Sasha Leblanc, partially fulfilled (Persimmon vase shipped, Ash budstem awaiting restock)

---

## Test cases

### S21-01 — Smoke: page loads with HTTP 200 and no console errors

**Steps:**
1. Navigate to `/seller/orders/1042`
2. Wait for network idle
3. Capture console messages via `agent-browser errors`

**Expected:**
- HTTP 200 response (page renders, no redirect)
- No console errors emitted
- Page content visible (order detail layout present)

**Result:** [x] pass

**Evidence:**
- Screenshot: `evidence/screenshots/S21-01.png`

**Notes:** —

---

### S21-02 — Breadcrumb header: order number, status chip, customer + age, action buttons

**Steps:**
1. Navigate to `/seller/orders/1042`
2. Observe the breadcrumb/header row at the top of the main content

**Expected:**
- Back chevron icon button is visible
- "Orders /" breadcrumb link is visible (muted styling)
- "#1042" is visible in monospace bold styling
- "Partially fulfilled" warn chip is visible (amber dot + amber/warn tone)
- "· Sasha L. · 2 hours ago" is visible as muted text (middle-dot · before "Sasha" and before "2 hours ago")
- "Message" outline button is visible (with chat icon)
- "Print slip" outline button is visible
- "Cancel order" ghost button is visible in red/bad color
- "Buy label · Ash budstem" primary button is visible

**Result:** [x] pass

**Evidence:**
- Screenshot: `evidence/screenshots/S21-02.png`

**Notes:** —

---

### S21-03 — Fulfillment 1 of 2: Persimmon vase shipped with USPS tracking number

**Steps:**
1. Navigate to `/seller/orders/1042`
2. Observe the first fulfillment block (green header)

**Expected:**
- "Fulfillment 1 of 2 · shipped" label is visible (middle-dot · between "2" and "shipped")
- Green checkmark circle icon is visible indicating fulfilled status
- USPS tracking number "USPS · 9405 5036 9930 0124 2317" is visible in monospace
- "Persimmon vase" product name is visible
- "SKU PV-08 · qty 1 · $86.00" is visible as muted detail text
- "$86.00" amount is visible on the right

**Result:** [x] pass

**Evidence:**
- Screenshot: `evidence/screenshots/S21-03.png`

**Notes:** —

---

### S21-04 — Fulfillment 2 of 2: Ash budstem awaiting restock with warn chip

**Steps:**
1. Navigate to `/seller/orders/1042`
2. Observe the second fulfillment block (orange-tinted header)

**Expected:**
- "Fulfillment 2 of 2 · awaiting restock" label is visible
- Empty circle (unfilled) is visible indicating unfulfilled status
- "Buy label" outline button is visible in the fulfillment header
- "Ash budstem" product name is visible
- "SKU AB-02 · qty 1 · $66.00" is visible as muted detail text
- "back in stock Tue" warn chip is visible next to the SKU detail
- "$66.00" amount is visible on the right

**Result:** [x] pass

**Evidence:**
- Screenshot: `evidence/screenshots/S21-04.png`

**Notes:** —

---

### S21-05 — Refund block: $30.00 partial refund on Ash budstem with reason and restock options

**Steps:**
1. Navigate to `/seller/orders/1042`
2. Observe the "Issue refund" card below the fulfillments

**Expected:**
- "Issue refund" heading is visible
- "refundable: $152.00 · across 2 items" muted text is visible (middle-dot ·)
- "Persimmon vase" row is present but unchecked (not selected for refund)
- "Ash budstem" row is checked (selected for refund) with darker border/background
- Refund amount input shows "$30.00" for Ash budstem
- "of $66.00" muted text is visible next to the refund input
- "Reason" dropdown shows "Item arrived chipped"
- "Restock?" chips show "Yes", "No", "Damage" — with "Damage" active (dark background)
- Summary footer shows "Refund total · to Visa · 4421" label
- Summary footer shows "$30.00" as the refund total amount
- "Issue refund" primary button is visible

**Result:** [x] pass

**Evidence:**
- Screenshot: `evidence/screenshots/S21-05.png`

**Notes:** —

---

### S21-06 — Timeline: 5 events in correct order

**Steps:**
1. Navigate to `/seller/orders/1042`
2. Observe the Timeline card

**Expected:**
- "Timeline" heading is visible
- Event 1: "Order placed" — "2 items · $152.00 paid via Visa · 4421" — "2h ago" (filled dark circle)
- Event 2: "Persimmon vase packed" — "Box S · 1lb 4oz" — "1h ago"
- Event 3: "Persimmon vase shipped" — "USPS Priority · 1–3 days" — "52m ago" (en-dash – in "1–3")
- Event 4: "Note from Sasha" — '"No rush on the budstem — ship together if it's faster!"' — "14m ago" (em-dash — inside quote)
- Event 5: "Ash budstem oversold" — "Restock arrives Tue · auto-fulfill on" — "8m ago" (warn tone, middle-dot ·)
- Connecting vertical line between events is visible

**Result:** [x] pass

**Evidence:**
- Screenshot: `evidence/screenshots/S21-06.png`

**Notes:** —

---

### S21-07 — Customer card: name, order history, shipping address, email

**Steps:**
1. Navigate to `/seller/orders/1042`
2. Observe the customer card in the right column

**Expected:**
- Customer avatar is visible (initials "SL" or similar)
- "Sasha Leblanc" name is visible
- "3rd order · $284 lifetime" is visible as muted text (middle-dot ·)
- "Profile →" ghost button is visible
- "Ship to" label is visible
- "820 Sutter St · #4B" address line is visible (middle-dot ·)
- "San Francisco, CA 94109" city/state/zip is visible
- "Bill to · same as ship" is visible as muted text
- "Email" label is visible
- "sasha.l@gmail.com" email is visible

**Result:** [x] pass

**Evidence:**
- Screenshot: `evidence/screenshots/S21-07.png`

**Notes:** —

---

### S21-08 — Financial summary card: subtotal, customer paid, deductions, "You'll receive"

**Steps:**
1. Navigate to `/seller/orders/1042`
2. Observe the Summary card in the right column

**Expected:**
- "Summary" heading is visible
- "Subtotal · 2 items" row shows "$152.00"
- "Shipping" row shows "$0.00"
- "Tax" row shows "$0.00"
- "Customer paid" row shows "$152.00" (larger font, h4 weight)
- "Micro fee · 4%" row shows "−$6.08" in muted style (minus sign −)
- "Shipping label · USPS" row shows "−$9.84" in muted style
- "You'll receive" row shows "$136.08" in green/good color

**Result:** [x] pass

**Evidence:**
- Screenshot: `evidence/screenshots/S21-08.png`

**Notes:** —

---

### S21-09 — Internal note and tag chips: VIP/Repeat buyer active, Gift inactive

**Steps:**
1. Navigate to `/seller/orders/1042`
2. Observe the internal note card in the right column

**Expected:**
- "INTERNAL NOTE" label is visible (uppercase, muted)
- Internal note text "Held until budstem restocks Tue. Sasha OK with split." is visible
- "VIP" chip is visible with active (dark) styling
- "Repeat buyer" chip is visible with active (dark) styling
- "Gift" chip is visible with inactive (muted) styling

**Result:** [x] pass

**Evidence:**
- Screenshot: `evidence/screenshots/S21-09.png`

**Notes:** —

---

### S21-10 — 404 verification: /seller/orders/9999 returns 404

**Steps:**
1. Run `curl -s -o /dev/null -w "%{http_code}" http://localhost:3000/seller/orders/9999`
2. Check the HTTP response code

**Expected:**
- HTTP response code is 404
- The page does not return 200 or 500

**Result:** [x] pass

**Actual:** curl returned 404.

**Evidence:**
- curl output captured in test notes

**Notes:** —

---

### S21-11 — A11y: heading hierarchy and landmark roles

**Steps:**
1. Navigate to `/seller/orders/1042`
2. Run `agent-browser snapshot` to capture the a11y tree

**Expected:**
- "Timeline" heading is present at an appropriate heading level
- "Summary" heading is present at an appropriate heading level
- "Issue refund" heading is present at an appropriate heading level
- Back navigation button has accessible role=button
- "Issue refund" submit button has accessible role=button
- Refund item checkboxes have accessible role=checkbox
- Status chip "Partially fulfilled" is readable by screen reader

**Result:** [x] pass

**Evidence:**
- Screenshot: `evidence/screenshots/S21-11.png`

**Notes:** —

---

### S21-12 — Brand parity: "Mira" absent, design annotation absent

**Steps:**
1. Navigate to `/seller/orders/1042`
2. Inspect full page text for any occurrence of "Mira"
3. Inspect full page text for the design annotation "refund · partial selected"

**Expected:**
- The string "Mira" does NOT appear anywhere on the page
- The design annotation "refund · partial selected" does NOT appear on the page
- Customer name is "Sasha Leblanc" (not "Mira")
- Shop owner references use "Alex" only

**Result:** [x] pass

**Evidence:**
- Screenshot: `evidence/screenshots/S21-12.png`

**Notes:** —

---

## Summary

| Case   | Description                                                              | Result      |
|--------|--------------------------------------------------------------------------|-------------|
| S21-01 | Smoke: page loads with HTTP 200 and no console errors                    | [x] pass    |
| S21-02 | Breadcrumb header: #1042, "Partially fulfilled", "· Sasha L. · 2h ago"   | [x] pass    |
| S21-03 | Fulfillment 1 of 2: Persimmon vase shipped, USPS tracking visible        | [x] pass    |
| S21-04 | Fulfillment 2 of 2: Ash budstem awaiting, "back in stock Tue" chip       | [x] pass    |
| S21-05 | Refund block: $30.00 partial on Ash, reason + restock=Damage, to Visa    | [x] pass    |
| S21-06 | Timeline: 5 events in correct order with correct strings                 | [x] pass    |
| S21-07 | Customer card: Sasha Leblanc, 820 Sutter St, sasha.l@gmail.com           | [x] pass    |
| S21-08 | Financial summary: Customer paid $152, You'll receive $136.08 in green   | [x] pass    |
| S21-09 | Internal note + tag chips: VIP/Repeat buyer active, Gift inactive        | [x] pass    |
| S21-10 | 404 verification: /seller/orders/9999 returns 404                        | [x] pass    |
| S21-11 | A11y: heading hierarchy and landmark roles                               | [x] pass    |
| S21-12 | Brand parity: "Mira" absent, annotation absent                           | [x] pass    |

**Batch F / TC-S21 result: 12 pass, 0 fail, 0 blocked, 0 not run**
