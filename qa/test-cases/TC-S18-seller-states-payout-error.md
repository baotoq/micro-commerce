# TC-S18 — Route: /seller/states/payout-error

**Route:** `/seller/states/payout-error`
**Batch:** E
**Worker:** worker-error

---

## Preconditions

- Dev server running on http://localhost:3000
- SellerSidebar is provided by the `/seller` layout

---

## Test cases

### S18-01 — Smoke: page loads with no console errors

**Steps:**
1. Navigate to `/seller/states/payout-error`
2. Wait for network idle
3. Capture console messages

**Expected:**
- HTTP 200 response
- No console errors emitted
- Page content visible (banner + held payout card + KPI row)

**Result:** [ ] not run

**Evidence:**
- Screenshot: `evidence/screenshots/S18-01.png`

---

### S18-02 — Topbar: greets BRAND.owner

**Steps:**
1. Navigate to `/seller/states/payout-error`
2. Observe the topbar area

**Expected:**
- Heading "Good morning, Alex" is visible
- Subtitle "Tuesday · April 8" is visible (middle-dot ·)
- "Mira" does not appear anywhere on the page

**Result:** [ ] not run

**Evidence:**
- Screenshot: `evidence/screenshots/S18-02.png`

---

### S18-03 — Error banner: copy and action buttons

**Steps:**
1. Navigate to `/seller/states/payout-error`
2. Observe the full-width banner below the topbar

**Expected:**
- Banner background is #FBE9E5 with a bottom border #E8B5AB
- Orange/bad-color circle with alert icon on the left
- Heading text: "We couldn't send your Tuesday payout · $1,284.62 held" (smart apostrophe, middle-dot ·)
- Body text: "Your bank rejected the transfer (account ending 4421). This sometimes happens after an address change. Update the account and we'll retry within an hour." (smart apostrophe)
- Outline button "View details" visible
- Primary button "Update bank →" visible
- Banner has `role="alert"` for accessibility

**Result:** [ ] not run

**Evidence:**
- Screenshot: `evidence/screenshots/S18-03.png`

---

### S18-04 — Held payout card: amount, metadata, and action buttons

**Steps:**
1. Navigate to `/seller/states/payout-error`
2. Observe the held payout card (first card below banner)

**Expected:**
- Tiny uppercase label "● Payout held" in bad/error color
- Large display number "$1,284.62" at 36px
- Subtext "9 orders · weekly batch · would have arrived Wed" (middle-dot ·)
- Outline button "Switch payout method" visible
- Primary button "Retry payout" visible
- Card border uses bad/error color

**Result:** [ ] not run

**Evidence:**
- Screenshot: `evidence/screenshots/S18-04.png`

---

### S18-05 — What's happening row: Stripe R03 mono code

**Steps:**
1. Navigate to `/seller/states/payout-error`
2. Observe the info row inside the held payout card (below the divider)

**Expected:**
- Info icon visible on the left
- Heading "What's happening" visible (smart apostrophe)
- Body text contains "Stripe returned"
- Inline mono code `R03 · No account / unable to locate` is rendered in monospace font (middle-dot ·)
- Body text continues ". The funds are safe with us; nothing left your shop. Once you update the routing or account number, we'll auto-retry."

**Result:** [ ] not run

**Evidence:**
- Screenshot: `evidence/screenshots/S18-05.png`

---

### S18-06 — Dimmed KPI row: 3 cards with labels and sparklines

**Steps:**
1. Navigate to `/seller/states/payout-error`
2. Observe the 3-column KPI row below the held payout card

**Expected:**
- Row has opacity ~0.4 (visually dimmed)
- Three cards: "Revenue · 7 days" / "$4,280.00", "Orders · 7 days" / "38", "Avg. order" / "$112.00"
- Each card contains a sparkline SVG in muted ink color
- Cards have white background, rounded corners, subtle border

**Result:** [ ] not run

**Evidence:**
- Screenshot: `evidence/screenshots/S18-06.png`

---

### S18-07 — A11y: alert-region landmark for banner

**Steps:**
1. Navigate to `/seller/states/payout-error`
2. Take an a11y snapshot

**Expected:**
- The error banner element has `role="alert"` attribute
- "error · payout failed" design-canvas annotation string is NOT present anywhere in the rendered output

**Result:** [ ] not run

**Evidence:**
- Screenshot: `evidence/screenshots/S18-07.png`

---

## Summary

| Case   | Description                                                        | Result      |
|--------|--------------------------------------------------------------------|-------------|
| S18-01 | Smoke: page loads with no console errors                           | [ ] not run |
| S18-02 | Topbar greets BRAND.owner (Alex, not Mira)                         | [ ] not run |
| S18-03 | Error banner: copy + Update bank / View details actions            | [ ] not run |
| S18-04 | Held payout card: amount + metadata + Retry/Switch actions         | [ ] not run |
| S18-05 | What's happening row: Stripe R03 mono code                         | [ ] not run |
| S18-06 | Dimmed KPI row: 3 cards with labels + sparklines                   | [ ] not run |
| S18-07 | A11y: alert-region landmark for banner                             | [ ] not run |

**Batch E / TC-S18 result: 0 pass, 0 fail, 7 not run**
