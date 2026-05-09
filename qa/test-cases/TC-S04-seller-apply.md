# TC-S04 — Route: /seller/apply

**Route:** `/seller/apply`
**Batch:** A
**Worker:** worker-1
**Executed:** 2026-05-09

---

## Preconditions

- Dev server running on http://localhost:3000

---

## Test cases

### S04-01 — Marketing top nav renders

**Steps:**
1. Navigate to `/seller/apply`

**Expected:**
- Text "micro." (exact)
- Link "Discover" (exact)
- Link "Shops" (exact)
- Link "Journal" (exact)
- Link "For makers" (exact)
- Button matching `/Sell on Micro/`

**Result:** [x] pass

**Evidence:**
- Screenshot: `evidence/screenshots/S04-01.png`

**Notes:** "micro." StaticText confirmed. Links Discover (e1), Shops (e2), Journal (e3), For makers (e4) present. "Sell on Micro" button at ref e6.

---

### S04-02 — Hero pitch section renders

**Steps:**
1. Navigate to `/seller/apply`

**Expected:**
- Text "For makers · 4% per sale, no monthly fee"
- Heading matching `/Open a shop in/`
- Text "4 800" (exact)
- Text "$2.1M" (exact)
- Text "12 min" (exact)

**Result:** [x] pass

**Evidence:**
- Screenshot: `evidence/screenshots/S04-01.png`

**Notes:** Pitch text rendered as "FOR MAKERS · 4% PER SALE, NO MONTHLY FEE" in DOM (CSS text-transform:uppercase); underlying value matches. Heading "Open a shop in about ten minutes." at ref e7. Stats "4 800", "$2.1M", "12 min" all confirmed as StaticText.

---

### S04-03 — Claim-shop card renders with shop name and URL

**Steps:**
1. Navigate to `/seller/apply`

**Expected:**
- Heading "Claim your shop name"
- Text "Mira Studio" (exact)
- Text "mira-studio.micro.shop" (exact)
- Text "is available" (exact)

**Result:** [x] pass

**Evidence:**
- Screenshot: `evidence/screenshots/S04-01.png`

**Notes:** "Claim your shop name" heading at ref e8. "Mira Studio", "mira-studio.micro.shop", and "is available" all confirmed as StaticText.

---

### S04-04 — Category chips render in claim-shop card

**Steps:**
1. Navigate to `/seller/apply`

**Expected:**
- Text "Ceramics" (exact)
- Text "Bakery" (exact)
- Text "Textiles" (exact)
- Text "Jewelry" (exact)
- Text "Vintage" (exact)
- Text "Other" (exact)

**Result:** [x] pass

**Evidence:**
- Screenshot: `evidence/screenshots/S04-01.png`

**Notes:** All six category StaticText nodes confirmed in snapshot.

---

### S04-05 — Continue button renders

**Steps:**
1. Navigate to `/seller/apply`

**Expected:**
- Button matching `/Continue · 6 steps left/`

**Result:** [x] pass

**Evidence:**
- Screenshot: `evidence/screenshots/S04-01.png`

**Notes:** "Continue · 6 steps left" button at ref e9 confirmed.

---

## Summary

| Case   | Description                          | Result    |
|--------|--------------------------------------|-----------|
| S04-01 | Marketing top nav                    | [x] pass  |
| S04-02 | Hero pitch section                   | [x] pass  |
| S04-03 | Claim-shop card with name + URL      | [x] pass  |
| S04-04 | Category chips                       | [x] pass  |
| S04-05 | Continue button                      | [x] pass  |
