# TC-S05 — Route: /seller/onboard

**Route:** `/seller/onboard`
**Batch:** B
**Worker:** worker-2
**Executed:** 2026-05-09

---

## Preconditions

- Dev server running on http://localhost:3000
- No SellerSidebar present (route is in the `(seller-marketing)` route group)

---

## Test cases

### S05-01 — Step rail renders with correct step count and labels

**Steps:**
1. Navigate to `/seller/onboard`
2. Observe the left rail

**Expected:**
- Text "Step 2 of 6" is visible
- All six step labels visible: "Shop name", "Location & payouts", "Brand", "First listing", "Shipping", "Review"
- Step 1 ("Shop name") shows a checkmark (completed)
- Step 2 ("Location & payouts") is highlighted as current

**Result:** [x] pass

**Evidence:**
- Screenshot: `evidence/screenshots/S05-01.png`

**Notes:** Step 1 shows a "Done" image (checkmark icon). Steps 2–6 show numerals. All six labels present.

---

### S05-02 — No SellerSidebar present

**Steps:**
1. Navigate to `/seller/onboard`
2. Observe the page layout

**Expected:**
- No sidebar navigation (the layout is full-page onboarding, not the seller dashboard shell)
- Logo "micro." and "Open a shop" label visible in the left rail header

**Result:** [x] pass

**Evidence:**
- Screenshot: `evidence/screenshots/S05-02.png`

**Notes:** No `complementary` / sidebar nav in snapshot. "micro." and "Open a shop" both present.

---

### S05-03 — Step 2 heading and eyebrow copy

**Steps:**
1. Navigate to `/seller/onboard`
2. Observe the center content area

**Expected:**
- Eyebrow text "Step 2 · Location & payouts" is visible
- Heading text matches: "Where are you shipping from, and where should we send the money?"

**Result:** [x] pass

**Evidence:**
- Screenshot: `evidence/screenshots/S05-03.png`

**Notes:** Eyebrow renders as "STEP 2 · LOCATION & PAYOUTS" (uppercased via CSS). Heading exact match confirmed.

---

### S05-04 — Studio location card renders with address fields

**Steps:**
1. Navigate to `/seller/onboard`
2. Observe the "Studio location" card

**Expected:**
- Section heading "Studio location" is visible
- Sub-label "Customers see only your city & state." is visible
- Field value "410 Linden St" is visible
- Field value "Oakland" is visible

**Result:** [x] pass

**Evidence:**
- Screenshot: `evidence/screenshots/S05-04.png`

**Notes:** —

---

### S05-05 — Payouts card renders all three options

**Steps:**
1. Navigate to `/seller/onboard`
2. Observe the payouts section

**Expected:**
- Heading "Where to send your payouts" is visible
- Option "Bank account" is visible with sub-label "ACH · 1–2 days"
- Option "Debit card" is visible with sub-label "Instant · 1.5%"
- Option "Add later" is visible with sub-label "Launch in draft"
- "Bank account" option has a checkmark indicating it is selected

**Result:** [x] pass

**Evidence:**
- Screenshot: `evidence/screenshots/S05-05.png`

**Notes:** "Bank account" shows "Selected" image (checkmark). All three options and sub-labels present.

---

### S05-06 — Back and Continue CTAs are present

**Steps:**
1. Navigate to `/seller/onboard`
2. Observe the form footer

**Expected:**
- Button "← Back" is visible
- Button "Continue · Brand" is visible (with right-arrow icon)

**Result:** [x] pass

**Evidence:**
- Screenshot: `evidence/screenshots/S05-06.png`

**Notes:** Both buttons confirmed in snapshot: ref=e4 "← Back", ref=e5 "Continue · Brand".

---

### S05-07 — Tip card in left rail

**Steps:**
1. Navigate to `/seller/onboard`
2. Observe the bottom of the left rail

**Expected:**
- Label "Tip" is visible (in terracotta/orange color per design)
- Tip body text "Add payouts last if you'd like — you can launch in draft and finish this when an order comes in." is visible

**Result:** [x] pass

**Evidence:**
- Screenshot: `evidence/screenshots/S05-07.png`

**Notes:** "TIP" renders uppercased via CSS. Full tip text confirmed in snapshot.

---

## Summary

| Case   | Description                                      | Result |
|--------|--------------------------------------------------|--------|
| S05-01 | Step rail renders with correct step count/labels | [x]    |
| S05-02 | No SellerSidebar present                         | [x]    |
| S05-03 | Step 2 heading and eyebrow copy                  | [x]    |
| S05-04 | Studio location card with address fields         | [x]    |
| S05-05 | Payouts card with all three options              | [x]    |
| S05-06 | Back and Continue CTAs present                   | [x]    |
| S05-07 | Tip card in left rail                            | [x]    |

**Batch B / TC-S05 result: 7 pass, 0 fail, 0 blocked**
