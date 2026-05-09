# TC-S17 — Route: /seller/states/empty-orders

**Route:** `/seller/states/empty-orders`
**Batch:** E
**Worker:** worker-empty

---

## Preconditions

- Dev server running on http://localhost:3000
- Route renders within the seller layout (SellerSidebar + main content)
- No order data — this is a static empty-state page

---

## Test cases

### S17-01 — Smoke: page loads with no console errors

**Steps:**
1. Navigate to `/seller/states/empty-orders`
2. Wait for network idle
3. Capture console messages

**Expected:**
- HTTP 200 response
- No console errors emitted
- Page content visible

**Result:** [ ] not run

**Evidence:**
- Screenshot: `evidence/screenshots/S17-01.png`

---

### S17-02 — Topbar: "Orders" heading + filter chips ("All · 0" active)

**Steps:**
1. Navigate to `/seller/states/empty-orders`
2. Observe the topbar row

**Expected:**
- `<h2>` "Orders" is visible (text-lg, font-semibold)
- "All · 0" chip is visible with active dark background
- "New", "Pack", "Ship", "Done" chips are visible (muted style)
- "Filter" outline button is visible on the right

**Result:** [ ] not run

**Evidence:**
- Screenshot: `evidence/screenshots/S17-02.png`

---

### S17-03 — Left pane: "No orders yet" panel

**Steps:**
1. Navigate to `/seller/states/empty-orders`
2. Observe the left pane (fixed 340px width)

**Expected:**
- Inbox icon (32px) is visible
- "No orders yet" heading is visible
- "They'll show up here as soon as someone buys." text is visible (muted)
- Left pane has a right border separating it from the right pane

**Result:** [ ] not run

**Evidence:**
- Screenshot: `evidence/screenshots/S17-03.png`

---

### S17-04 — Right pane: "Quiet, isn't it." display + body copy

**Steps:**
1. Navigate to `/seller/states/empty-orders`
2. Observe the right pane centered content

**Expected:**
- 84×84 rounded circle with inbox icon (36px) centered at top
- `<h2>` "Quiet, isn't it." visible at 32px (exact match, period and smart apostrophe)
- Body text "Most shops get their first order within a week of sharing the link. While you wait, two things tend to help." visible (muted, max-w ~380)

**Result:** [ ] not run

**Evidence:**
- Screenshot: `evidence/screenshots/S17-04.png`

---

### S17-05 — Helper cards: both cards with exact strings

**Steps:**
1. Navigate to `/seller/states/empty-orders`
2. Observe the 2-column grid of helper cards

**Expected:**
- Card 1 title: "Add 2 more listings" (exact)
- Card 1 body: "Shops with 5+ items get found 3× more." (times sign × is intentional)
- Card 2 title: "Share your link" (exact)
- Card 2 body: "A short note to friends does most of the lifting." (exact)

**Result:** [ ] not run

**Evidence:**
- Screenshot: `evidence/screenshots/S17-05.png`

---

### S17-06 — Copy CTA: "Copy alex-studio.micro.shop" button (renders, no "Mira")

**Steps:**
1. Navigate to `/seller/states/empty-orders`
2. Observe the primary call-to-action button below helper cards
3. Check for any occurrence of "Mira" in the page

**Expected:**
- Button "Copy alex-studio.micro.shop" is visible with primary (dark) styling
- Clicking the button triggers clipboard copy (navigator.clipboard)
- The string "Mira" does NOT appear anywhere on the page
- The annotation "empty · no orders" does NOT appear on the page

**Result:** [ ] not run

**Evidence:**
- Screenshot: `evidence/screenshots/S17-06.png`

---

### S17-07 — A11y: heading hierarchy and landmarks

**Steps:**
1. Navigate to `/seller/states/empty-orders`
2. Take an a11y snapshot

**Expected:**
- `<h2>` "Orders" present in topbar (level 2)
- `<h4>` or equivalent "No orders yet" present in left pane
- `<h2>` "Quiet, isn't it." present in right pane (level 2)
- Inbox icons have `aria-hidden` (decorative)
- "Filter" button has accessible role=button
- "Copy alex-studio.micro.shop" button has accessible role=button

**Result:** [ ] not run

**Evidence:**
- Screenshot: `evidence/screenshots/S17-07.png`

---

## Summary

| Case   | Description                                          | Result      |
|--------|------------------------------------------------------|-------------|
| S17-01 | Smoke: page loads with no console errors             | [ ] not run |
| S17-02 | Topbar: "Orders" + filter chips ("All · 0" active)   | [ ] not run |
| S17-03 | Left pane: "No orders yet" panel                     | [ ] not run |
| S17-04 | Right pane: "Quiet, isn't it." display + body copy   | [ ] not run |
| S17-05 | Helper cards: both cards with exact strings          | [ ] not run |
| S17-06 | Copy CTA: button renders, no "Mira"                  | [ ] not run |
| S17-07 | A11y: heading hierarchy and landmarks                | [ ] not run |

**Batch E / TC-S17 result: 0 pass, 0 fail, 0 blocked, 7 not run**
