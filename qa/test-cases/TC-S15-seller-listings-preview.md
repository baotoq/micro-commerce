# TC-S15 — Route: /seller/listings/[sku]/preview

**Route:** `/seller/listings/MC-VS-001/preview`
**Batch:** D
**Worker:** qa-sonnet
**Executed:** 2026-05-09

---

## Preconditions

- Dev server running on http://localhost:3000
- SKU `MC-VS-001` ("Persimmon vase", category "Vessels") exists in the data layer
- No SellerSidebar on this route — inline shell with header + browser-preview + health rail

---

## Test cases

### S15-01 — Smoke: page loads with no console errors

**Steps:**
1. Navigate to `/seller/listings/MC-VS-001/preview`
2. Wait for network idle
3. Capture console messages

**Expected:**
- HTTP 200 response
- No console errors emitted
- Page content visible

**Result:** [x] pass

**Evidence:**
- Screenshot: `evidence/screenshots/S15-01.png`

**Notes:** 0 console errors. Page title "micro-commerce". All content rendered.

---

### S15-02 — Header: breadcrumb, title, viewport toggle, action buttons

**Steps:**
1. Navigate to `/seller/listings/MC-VS-001/preview`
2. Observe the page header

**Expected:**
- Breadcrumb label "Preview · Persimmon vase" is visible (middle-dot ·)
- `<h1>` "How shoppers will see it" is visible (exact match)
- Back button (‹) is present with aria-label "Back"
- Viewport toggle with "Desktop" (active/selected) and "Mobile" buttons is visible
- "Back to edit" outline button is visible
- "Publish now →" button is visible

**Result:** [x] pass

**Evidence:**
- Screenshot: `evidence/screenshots/S15-02.png`

**Notes:** Breadcrumb "Preview · Persimmon vase" (e36), `<h1>` "How shoppers will see it" (e37), Back button with aria-label "Back" (e34), Desktop/Mobile toggle buttons (e40/e41), "Back to edit" outline button (e42), "Publish now →" button (e43) — all confirmed in a11y snapshot.

---

### S15-03 — Browser preview: address bar and product mock

**Steps:**
1. Navigate to `/seller/listings/MC-VS-001/preview`
2. Observe the browser mock-frame in the main area

**Expected:**
- Fake browser address bar shows "alex-studio.micro.shop/persimmon-vase" (slug from listing name)
- Red/yellow/green traffic-light circles are visible (aria-hidden)
- Product name "Persimmon vase" appears inside the mock frame
- Price "$95" appears inside the mock frame
- "Medium · Persimmon · 4 in stock" caption appears
- Size chips "Small" / "Medium" (selected) / "Large · out" appear
- "Add to bag · $95" button appears inside the mock frame

**Result:** [x] pass

**Evidence:**
- Screenshot: `evidence/screenshots/S15-03.png`

**Notes:** Lock icon 🔒 (e52) and address "alex-studio.micro.shop/persimmon-vase" (e53) confirmed. Inside frame: `<h2>` "Persimmon vase" (e58), price "$95" (e59), caption "Medium · Persimmon · 4 in stock" (e60), size chips "Small"/"Medium"/"Large · out" (e63/e64/e65), "Add to bag · $95" button (e71). Traffic-light dots are aria-hidden decorative spans — visible in screenshot.

---

### S15-04 — Browser preview: category breadcrumb and glaze swatches

**Steps:**
1. Navigate to `/seller/listings/MC-VS-001/preview`
2. Observe the product detail section inside the mock frame

**Expected:**
- "Alex Studio · Vessels" category breadcrumb is visible inside the frame (middle-dot ·)
- SIZE label is visible
- GLAZE label is visible
- Two glaze color swatches (persimmon orange and cream) are visible

**Result:** [x] pass

**Evidence:**
- Screenshot: `evidence/screenshots/S15-04.png`

**Notes:** "Alex Studio · Vessels" breadcrumb (e57), "SIZE" label (e61), "GLAZE" label (e66) all confirmed. Glaze color swatches are decorative spans (no role/text in a11y tree) — visible in screenshot.

---

### S15-05 — Health rail: score, progress bar, and check items

**Steps:**
1. Navigate to `/seller/listings/MC-VS-001/preview`
2. Observe the right-side health rail

**Expected:**
- `<h2>` "Listing health" is visible
- Score "96" is visible (green text)
- Progress bar filled to ~96% is visible
- Check item "Title under 60 chars" with sub "14 / 60" (green ✓) is visible
- Check item "Description over 100 chars" with sub "208 / 800" (green ✓) is visible
- Check item "4 photos" with sub "recommend 6+" (amber i) is visible
- Check item "Variants in stock" with sub "5 of 6 active" (green ✓) is visible
- Check item "Tagged & categorized" with sub "4 tags · Vessels" (green ✓) is visible (middle-dot ·)

**Result:** [x] pass

**Evidence:**
- Screenshot: `evidence/screenshots/S15-05.png`

**Notes:** `<h2>` "Listing health" (e74) and score "96" (e75) confirmed. All 5 check items confirmed: "Title under 60 chars" / "14 / 60" (✓), "Description over 100 chars" / "208 / 800" (✓), "4 photos" / "recommend 6+" (i — amber), "Variants in stock" / "5 of 6 active" (✓), "Tagged & categorized" / "4 tags · Vessels" (✓). Progress-bar fill is decorative — visible in screenshot.

---

### S15-06 — A11y: heading hierarchy and landmark structure

**Steps:**
1. Navigate to `/seller/listings/MC-VS-001/preview`
2. Take an a11y snapshot

**Expected:**
- `<h1>` "How shoppers will see it" is present
- `<h2>` "Persimmon vase" (inside mock frame) is present
- `<h2>` "Listing health" (in health rail) is present
- `<aside>` landmark for the health rail is present
- Back button has accessible aria-label "Back"
- "Publish now →" button is accessible as `button` role

**Result:** [x] pass

**Evidence:**
- Screenshot: `evidence/screenshots/S15-06.png`

**Notes:** `<h1>` "How shoppers will see it" [level=1] (e37), `<h2>` "Persimmon vase" [level=2] (e58), `<h2>` "Listing health" [level=2] (e74) all confirmed. Health rail rendered as `complementary` landmark (e72) — `<aside>` semantics correct. Back button accessible name "Back" (e34). "Publish now →" exposed as `button` role (e43).

---

### S15-07 — 404 behavior: unknown SKU returns not-found page

**Steps:**
1. Navigate to `/seller/listings/MC-NOPE-404/preview`
2. Check HTTP response status
3. Verify the page renders a not-found state

**Expected:**
- HTTP response is 404 (not 200)
- Page does not render "How shoppers will see it" content
- Next.js not-found UI or a 404 indicator is shown

**Result:** [x] pass

**Evidence:**
- Screenshot: `evidence/screenshots/S15-07.png`
- Console log: `evidence/console-logs/S15.txt`

**Notes:** `curl -sI` returned HTTP 404. Page title "404: This page could not be found." Snapshot shows the Next.js notFound() UI (no "How shoppers will see it" content). The 1 captured console error is the 404 itself (`Failed to load resource: the server responded with a status of 404 (Not Found)`) — expected for the not-found state, not a content defect. Same pattern as TC-S14 case S14-07.

---

## Summary

| Case   | Description                                                            | Result |
|--------|------------------------------------------------------------------------|--------|
| S15-01 | Smoke: page loads with no console errors                               | [x]    |
| S15-02 | Header: breadcrumb, title, viewport toggle, action buttons             | [x]    |
| S15-03 | Browser preview: address bar and product mock                          | [x]    |
| S15-04 | Browser preview: category breadcrumb and glaze swatches                | [x]    |
| S15-05 | Health rail: score, progress bar, and check items                      | [x]    |
| S15-06 | A11y: heading hierarchy and landmark structure                         | [x]    |
| S15-07 | 404 behavior: unknown SKU returns not-found page                       | [x]    |

**Batch D / TC-S15 result: 7 pass, 0 fail, 0 blocked**
