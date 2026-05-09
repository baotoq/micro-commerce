# TC-S22 — Route: /seller/promos

**Route:** `/seller/promos`
**Batch:** F
**Worker:** worker-mgmt-2
**Executed:** 2026-05-10

---

## Preconditions

- Dev server running on http://localhost:3000
- Route renders within the seller layout (SellerSidebar + main content)
- Static fixture data — 5 promo codes, always-open "New promotion" drawer on the right

---

## Test cases

### S22-01 — Smoke: page loads with HTTP 200 and no console errors

**Steps:**
1. Navigate to `/seller/promos`
2. Wait for network idle
3. Capture console messages via `agent-browser errors`

**Expected:**
- HTTP 200 response (page renders, no redirect)
- No console errors emitted
- Page content visible (promos table and drawer present)

**Result:** [x] pass

**Evidence:**
- Screenshot: `evidence/screenshots/S22-01.png`

**Notes:** —

---

### S22-02 — Topbar: title with literal ampersand, subtitle, and "+ New promotion" button

**Steps:**
1. Navigate to `/seller/promos`
2. Observe the SellerTopbar component

**Expected:**
- Page title reads exactly "Discounts & promotions" (literal ampersand &, not HTML entity)
- Subtitle reads exactly "3 active · $2,740 driven · 281 redemptions" (middle-dot · between segments, comma in $2,740)
- "+ New promotion" primary button is visible in topbar actions (plus icon + label)

**Result:** [x] pass

**Evidence:**
- Screenshot: `evidence/screenshots/S22-02.png`

**Notes:** —

---

### S22-03 — Stat row: 4 metric cards with correct labels, values, and sparklines

**Steps:**
1. Navigate to `/seller/promos`
2. Observe the 4-column stat row below the topbar

**Expected:**
- Card 1: label "Driven revenue", value "$2,740", sub-label "Last 30 days", sparkline visible
- Card 2: label "Redemptions", value "281", sub-label "14% of orders", sparkline visible
- Card 3: label "Avg. discount", value "$9.74", sub-label "per redemption", sparkline visible
- Card 4: label "New buyers", value "38", sub-label "from WELCOME10", sparkline visible
- All 4 sparklines are rendered (SVG or canvas element with a line/area)

**Result:** [x] pass

**Evidence:**
- Screenshot: `evidence/screenshots/S22-03.png`

**Notes:** —

---

### S22-04 — Tabs row: Promotions (5, active) / Automatic (1) / Gift cards (0)

**Steps:**
1. Navigate to `/seller/promos`
2. Observe the tabs below the stat row

**Expected:**
- Tab "Promotions" with count badge "5" is visible and styled as active
- Tab "Automatic" with count badge "1" is visible (inactive)
- Tab "Gift cards" with count badge "0" is visible (inactive)
- "Promotions" tab has the active indicator (underline or dark badge)

**Result:** [x] pass

**Actual:** Tabs render as role=button ("Promotions 5", "Automatic 1", "Gift cards 0") rather than role=tab — visual content correct; a11y role is button not tab (minor deviation, see F-MGT-002).

**Evidence:**
- Screenshot: `evidence/screenshots/S22-04.png`

**Notes:** —

---

### S22-05 — Promos table: 5 rows with correct codes, status chips, and STUDIO15 highlighted

**Steps:**
1. Navigate to `/seller/promos`
2. Observe the promotions table

**Expected:**
- Row 1: code "SPRING20" (monospace), "Active" chip with good (green) tone
- Row 2: code "WELCOME10" (monospace), "Active" chip with good (green) tone
- Row 3: code "STUDIO15" (monospace), "Active" chip with good (green) tone — row has blue highlight background
- Row 4: code "BLOOM" (monospace), "Ended" chip with muted tone
- Row 5: code "FRIENDS" (monospace), "Draft" chip with muted tone
- Table columns visible: Code, What it does, Redemptions, Driven revenue, Window, Status
- Edit and chevron icon buttons are visible on each row

**Result:** [x] pass

**Evidence:**
- Screenshot: `evidence/screenshots/S22-05.png`

**Notes:** —

---

### S22-06 — Drawer: header "New promotion" and CODE section with "STUDIO15" input

**Steps:**
1. Navigate to `/seller/promos`
2. Observe the right-side drawer panel

**Expected:**
- Drawer is visible on the right side of the page (always-open)
- Drawer header "New promotion" is visible
- Close (X) icon button is visible in the drawer header
- "CODE" section label is visible (uppercase)
- Code input field shows "STUDIO15" in monospace bold
- "Generate" ghost button is visible next to the code input
- Helper text "Buyers will type or click this at checkout" is visible below the input

**Result:** [x] pass

**Evidence:**
- Screenshot: `evidence/screenshots/S22-06.png`

**Notes:** —

---

### S22-07 — Drawer: DISCOUNT section with segmented toggle, "15 %" value, description

**Steps:**
1. Navigate to `/seller/promos`
2. Observe the DISCOUNT section in the drawer

**Expected:**
- "DISCOUNT" section label is visible (uppercase)
- Segmented toggle shows 4 options: "% off", "$ off", "Free shipping", "BOGO"
- "% off" is the active option (white background, higher contrast)
- Large "15" numeral is visible with "%" suffix
- "off the entire order" label is visible next to the value

**Result:** [x] pass

**Evidence:**
- Screenshot: `evidence/screenshots/S22-07.png`

**Notes:** —

---

### S22-08 — Drawer: WHO CAN USE IT section with "Followers only" selected

**Steps:**
1. Navigate to `/seller/promos`
2. Observe the "WHO CAN USE IT" section in the drawer

**Expected:**
- "WHO CAN USE IT" section label is visible (uppercase)
- Radio card "Anyone with the code" — "Public · share on socials" is visible (unselected)
- Radio card "Followers only" — "Auto-applied · 184 buyers eligible" is visible and selected (dark border + filled radio)
- Radio card "Specific customers" — "Pick from your CRM" is visible (unselected)

**Result:** [x] pass

**Evidence:**
- Screenshot: `evidence/screenshots/S22-08.png`

**Notes:** —

---

### S22-09 — Drawer: LIMITS 2×2 grid and Forecast callout

**Steps:**
1. Navigate to `/seller/promos`
2. Observe the LIMITS section and Forecast callout in the drawer

**Expected:**
- "LIMITS" section label is visible (uppercase)
- "Min. order" card shows "$40.00"
- "Per buyer" card shows "1 use"
- "Total uses" card shows "200"
- "Window" card shows "Apr 22 → May 06" (arrow → between dates)
- Forecast callout is visible with a green dot
- Forecast text contains "~24 redemptions" (tilde ~ before 24)
- Forecast text contains "$420–$640" (en-dash – between values)
- Forecast text contains "-$72" as margin impact

**Result:** [x] pass

**Evidence:**
- Screenshot: `evidence/screenshots/S22-09.png`

**Notes:** —

---

### S22-10 — Drawer footer: "Save draft" and "Activate · Tue 12:00 AM" buttons

**Steps:**
1. Navigate to `/seller/promos`
2. Observe the drawer footer

**Expected:**
- "Save draft" ghost button is visible on the left side of the footer
- "Activate · Tue 12:00 AM" primary button is visible on the right side (middle-dot · between "Activate" and "Tue 12:00 AM")
- Footer is separated from drawer body by a top border

**Result:** [x] pass

**Evidence:**
- Screenshot: `evidence/screenshots/S22-10.png`

**Notes:** —

---

### S22-11 — Sidebar navigation: "Discounts" item is active

**Steps:**
1. Navigate to `/seller/promos`
2. Observe the SellerSidebar

**Expected:**
- Sidebar is present on the left side
- "Discounts" nav item is highlighted as the active route (or "Discounts & promotions" depending on sidebar label)
- Other nav items are visible but not active

**Result:** [x] pass

**Evidence:**
- Screenshot: `evidence/screenshots/S22-11.png`

**Notes:** —

---

### S22-12 — A11y: heading hierarchy, table headers, drawer regions

**Steps:**
1. Navigate to `/seller/promos`
2. Run `agent-browser snapshot` to capture the a11y tree

**Expected:**
- "Discounts & promotions" heading is present at an appropriate heading level
- "New promotion" drawer heading is present (could be h2 or h3)
- Promotions table has accessible column headers
- "Activate · Tue 12:00 AM" button has accessible role=button
- "Save draft" button has accessible role=button
- Radio buttons in "Who can use it" have accessible role=radio
- Stat cards have readable text values accessible to screen readers

**Result:** [x] pass

**Evidence:**
- Screenshot: `evidence/screenshots/S22-12.png`

**Notes:** —

---

### S22-13 — Brand parity: "Mira" absent, design annotation absent

**Steps:**
1. Navigate to `/seller/promos`
2. Inspect full page text for any occurrence of "Mira"
3. Inspect full page text for the design annotation "drawer · new promo"

**Expected:**
- The string "Mira" does NOT appear anywhere on the page
- The design annotation "drawer · new promo" does NOT appear on the page
- All branding references use "Micro Commerce" or "Alex"

**Result:** [x] pass

**Evidence:**
- Screenshot: `evidence/screenshots/S22-13.png`

**Notes:** —

---

## Summary

| Case   | Description                                                              | Result      |
|--------|--------------------------------------------------------------------------|-------------|
| S22-01 | Smoke: page loads with HTTP 200 and no console errors                    | [x] pass    |
| S22-02 | Topbar: "Discounts & promotions" title, subtitle, "+ New promotion"       | [x] pass    |
| S22-03 | Stat row: 4 metric cards with correct labels, values, sparklines         | [x] pass    |
| S22-04 | Tabs: Promotions (5, active) / Automatic (1) / Gift cards (0)            | [x] pass    |
| S22-05 | Table: 5 rows, STUDIO15 highlighted, correct status chip tones           | [x] pass    |
| S22-06 | Drawer header: "New promotion" + CODE section with "STUDIO15" input      | [x] pass    |
| S22-07 | Drawer: DISCOUNT section, "% off" active, "15 %" value                  | [x] pass    |
| S22-08 | Drawer: WHO CAN USE IT, "Followers only" selected, 184 buyers eligible   | [x] pass    |
| S22-09 | Drawer: LIMITS 2×2 grid + Forecast "~24 redemptions", "$420–$640"        | [x] pass    |
| S22-10 | Drawer footer: "Save draft" + "Activate · Tue 12:00 AM"                  | [x] pass    |
| S22-11 | Sidebar nav: "Discounts" item active                                     | [x] pass    |
| S22-12 | A11y: heading hierarchy, table headers, drawer regions                   | [x] pass    |
| S22-13 | Brand parity: "Mira" absent, design annotation absent                    | [x] pass    |

**Batch F / TC-S22 result: 13 pass, 0 fail, 0 blocked, 0 not run**
