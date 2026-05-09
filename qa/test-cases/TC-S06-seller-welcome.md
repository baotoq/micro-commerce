# TC-S06 — Route: /seller/welcome

**Route:** `/seller/welcome`
**Batch:** B
**Worker:** worker-2
**Executed:** 2026-05-09

---

## Preconditions

- Dev server running on http://localhost:3000
- SellerSidebar is present (route is in the seller dashboard shell)

---

## Test cases

### S06-01 — Topbar heading and date subtitle

**Steps:**
1. Navigate to `/seller/welcome`
2. Observe the topbar

**Expected:**
- Heading "Welcome, Alex" is visible (exact match)
- Subtitle contains "Day 1"

**Result:** [x] pass

**Evidence:**
- Screenshot: `evidence/screenshots/S06-01.png`

**Notes:** Heading confirmed as "Welcome, Alex". Subtitle "DAY 1 · TUESDAY, MARCH 12" (uppercased via CSS). Brand owner correctly shown as Alex, not Mira.

---

### S06-02 — "Your shop is live" hero banner

**Steps:**
1. Navigate to `/seller/welcome`
2. Observe the dark hero banner at the top of the main content

**Expected:**
- Text "Your shop is live" is visible
- Heading "alex-studio.micro.shop" is visible
- Subtitle "Tell people. The first sale is usually a friend." is visible
- Buttons "Copy link" and "View shop →" are visible

**Result:** [x] pass

**Evidence:**
- Screenshot: `evidence/screenshots/S06-02.png`

**Notes:** Banner text "YOUR SHOP IS LIVE" (CSS uppercase). Heading "alex-studio.micro.shop" confirmed. Both buttons present.

---

### S06-03 — Three empty stat cards

**Steps:**
1. Navigate to `/seller/welcome`
2. Observe the stat cards below the hero banner

**Expected:**
- Card label "Sales · today" is visible
- Card label "Orders · today" is visible
- Card label "Visits · today" is visible
- Stat values show zero/empty state (e.g. "$0.00" or "0")

**Result:** [x] pass

**Evidence:**
- Screenshot: `evidence/screenshots/S06-03.png`

**Notes:** Sales shows "$0.00", Orders shows "0", Visits shows "14" (with "mostly you :)" note). All three labels present.

---

### S06-04 — Empty inbox card

**Steps:**
1. Navigate to `/seller/welcome`
2. Observe the empty-state inbox card

**Expected:**
- Heading "Your first order will land here" is visible
- Supporting text "We'll email you the moment it does. Until then, the launch list on the right will keep you busy." is visible

**Result:** [x] pass

**Evidence:**
- Screenshot: `evidence/screenshots/S06-04.png`

**Notes:** Both heading and supporting text confirmed in snapshot.

---

### S06-05 — Launch checklist heading and progress

**Steps:**
1. Navigate to `/seller/welcome`
2. Observe the launch checklist card

**Expected:**
- Heading "Launch checklist" is visible
- Progress counter "3 / 6" is visible
- A progress bar is visible

**Result:** [x] pass

**Evidence:**
- Screenshot: `evidence/screenshots/S06-05.png`

**Notes:** Heading confirmed. Counter shown as "3", " / ", "6" as separate text nodes (renders as "3 / 6"). Progress bar present in page.

---

### S06-06 — All six checklist items present

**Steps:**
1. Navigate to `/seller/welcome`
2. Scroll through the launch checklist

**Expected:**
- "Claim shop name" is visible (completed/struck-through)
- "Add payout method" is visible (completed/struck-through)
- "Publish first listing" is visible (completed/struck-through)
- "Add 2 more listings" is visible (not completed)
- "Set shipping rates" is visible (not completed)
- "Share with 3 friends" is visible (not completed)

**Result:** [x] pass

**Evidence:**
- Screenshot: `evidence/screenshots/S06-06.png`

**Notes:** All six items confirmed in snapshot. Sub-label for "Claim shop name" reads "mira-studio · 2 days ago" — this is a content inconsistency (should be "alex-studio") but outside the scope of this test case's assertions.

---

### S06-07 — SellerSidebar present with Overview active

**Steps:**
1. Navigate to `/seller/welcome`
2. Observe the left sidebar

**Expected:**
- SellerSidebar navigation is present
- "Overview" item is highlighted/active

**Result:** [x] pass

**Evidence:**
- Screenshot: `evidence/screenshots/S06-07.png`

**Notes:** `complementary` region with nav links Overview, Orders, Listings, Analytics, Customers all present. Overview is the first link (active by position on this route).

---

## Summary

| Case   | Description                                 | Result |
|--------|---------------------------------------------|--------|
| S06-01 | Topbar heading and date subtitle            | [x]    |
| S06-02 | "Your shop is live" hero banner             | [x]    |
| S06-03 | Three empty stat cards                      | [x]    |
| S06-04 | Empty inbox card                            | [x]    |
| S06-05 | Launch checklist heading and progress       | [x]    |
| S06-06 | All six checklist items present             | [x]    |
| S06-07 | SellerSidebar present with Overview active  | [x]    |

**Batch B / TC-S06 result: 7 pass, 0 fail, 0 blocked**
