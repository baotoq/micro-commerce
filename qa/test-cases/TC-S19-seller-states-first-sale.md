# TC-S19 — Route: /seller/states/first-sale

**Route:** `/seller/states/first-sale`
**Batch:** E
**Worker:** worker-success
**Executed:** [ ] not run

---

## Preconditions

- Dev server running on http://localhost:3000
- Static page — no dynamic data required
- Modal renders always-open as a static visual (no toggle)

---

## Test cases

### S19-01 — Smoke: page loads with no console errors

**Steps:**
1. Navigate to `/seller/states/first-sale`
2. Wait for network idle
3. Capture console messages

**Expected:**
- HTTP 200 response
- No console errors emitted
- Page content visible

**Actual:** [ ] not run

**Status:** [ ] not run

**Evidence:** —

---

### S19-02 — Topbar greets BRAND.owner ("Welcome, Alex" — not "Mira")

**Steps:**
1. Navigate to `/seller/states/first-sale`
2. Inspect the topbar heading

**Expected:**
- Heading reads exactly "Welcome, Alex"
- Subtitle reads "Day 4 · Friday, March 15"
- The string "Mira" does not appear anywhere in the topbar

**Actual:** [ ] not run

**Status:** [ ] not run

**Evidence:** —

---

### S19-03 — Blurred dashboard skeleton behind overlay

**Steps:**
1. Navigate to `/seller/states/first-sale`
2. Inspect the content area behind the modal overlay

**Expected:**
- Dashboard placeholder is visible behind the overlay (filter blur applied)
- 4-column KPI card grid with muted placeholder blocks visible
- One large card (height ~240) with header bar and body block visible
- Placeholder blocks use muted background (#f5f5f7)

**Actual:** [ ] not run

**Status:** [ ] not run

**Evidence:** —

---

### S19-04 — Hero band: ★, "Your first sale" label, "It happened." display

**Steps:**
1. Navigate to `/seller/states/first-sale`
2. Inspect the modal hero band (dark top section)

**Expected:**
- Star glyph "★" visible at ~44px
- "Your first sale" label visible in small uppercase text
- "It happened." heading visible at display size (~36px)
- Hero band has dark (#1d1d1f) background with white text
- Two decorative semi-transparent circles in the hero band (right-top and left-bottom)

**Actual:** [ ] not run

**Status:** [ ] not run

**Evidence:** —

---

### S19-05 — Order summary: product, customer, $86.00

**Steps:**
1. Navigate to `/seller/states/first-sale`
2. Inspect the order summary row inside the modal body

**Expected:**
- Clay-tone product image placeholder (56×56, rounded-lg) visible
- "Persimmon vase" product name visible
- "Sasha L. · San Francisco, CA" customer line visible (middle-dot · glyph)
- "$86.00" price displayed right-aligned

**Actual:** [ ] not run

**Status:** [ ] not run

**Evidence:** —

---

### S19-06 — Body copy + fee math ($82.56)

**Steps:**
1. Navigate to `/seller/states/first-sale`
2. Inspect the paragraph text below the order summary

**Expected:**
- Text reads: "You'll receive $82.56 after Micro's 4% fee. Pack & ship in the next 3 days and the rating will follow."
- Smart apostrophe ' in "You'll" and "Micro's"
- Ampersand & in "Pack & ship"
- $82.56 formatted correctly (money() output)

**Actual:** [ ] not run

**Status:** [ ] not run

**Evidence:** —

---

### S19-07 — Action buttons: Send a thank-you note (outline) + Pack & ship → (primary)

**Steps:**
1. Navigate to `/seller/states/first-sale`
2. Inspect the two action buttons at the bottom of the modal

**Expected:**
- "Send a thank-you note" button visible with outline style (border, white/light bg)
- "Pack & ship →" button visible with primary style (dark bg, white text, → arrow glyph)
- Both buttons have equal flex width (side by side)

**Actual:** [ ] not run

**Status:** [ ] not run

**Evidence:** —

---

### S19-08 — A11y: dialog role + aria-modal + heading association

**Steps:**
1. Navigate to `/seller/states/first-sale`
2. Inspect the modal element accessibility attributes

**Expected:**
- Modal element has `role="dialog"`
- Modal element has `aria-modal="true"`
- Modal element has `aria-labelledby` pointing to the "It happened." heading id
- "It happened." heading has matching `id` attribute

**Actual:** [ ] not run

**Status:** [ ] not run

**Evidence:** —

---

## Summary

| Case | Description | Status |
|------|-------------|--------|
| S19-01 | Smoke | [ ] not run |
| S19-02 | Topbar greets BRAND.owner | [ ] not run |
| S19-03 | Blurred dashboard skeleton | [ ] not run |
| S19-04 | Hero band content | [ ] not run |
| S19-05 | Order summary | [ ] not run |
| S19-06 | Body copy + fee math | [ ] not run |
| S19-07 | Action buttons | [ ] not run |
| S19-08 | A11y attributes | [ ] not run |

**Pass:** 0 / **Fail:** 0 / **Not run:** 8
