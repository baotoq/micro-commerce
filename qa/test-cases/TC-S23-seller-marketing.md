# TC-S23 — Route: /seller/marketing

**Route:** `/seller/marketing`
**Batch:** F
**Worker:** worker-mgmt-2
**Executed:** 2026-05-10

---

## Preconditions

- Dev server running on http://localhost:3000
- Route renders within the seller layout (SellerSidebar + main content)
- Static fixture — 3-step email composer on the left, email preview pane on the right
- Brand: sender must be "Micro Commerce", sign-off must be "— Alex" (NOT "Mira Studio" / "— Mira")

---

## Test cases

### S23-01 — Smoke: page loads with HTTP 200 and no console errors

**Steps:**
1. Navigate to `/seller/marketing`
2. Wait for network idle
3. Capture console messages via `agent-browser errors`

**Expected:**
- HTTP 200 response (page renders, no redirect)
- No console errors emitted
- Page content visible (composer + preview pane present)

**Result:** [x] pass

**Evidence:**
- Screenshot: `evidence/screenshots/S23-01.png`

**Notes:** —

---

### S23-02 — Topbar: title, subtitle, and 3 action buttons

**Steps:**
1. Navigate to `/seller/marketing`
2. Observe the SellerTopbar component

**Expected:**
- Page title reads exactly "Email recent buyers"
- Subtitle reads exactly "Marketing · drafted Tuesday" (middle-dot · between "Marketing" and "drafted Tuesday")
- "Save draft" outline button is visible in topbar actions
- "Send test" outline button is visible (with eye icon)
- "Schedule send" primary button is visible

**Result:** [x] pass

**Evidence:**
- Screenshot: `evidence/screenshots/S23-02.png`

**Notes:** —

---

### S23-03 — Composer Step 1: Audience — Recipients heading and 4 audience rows

**Steps:**
1. Navigate to `/seller/marketing`
2. Observe the Step 1 section in the left composer pane

**Expected:**
- Eyebrow text "Step 1 of 3 · Audience" is visible (middle-dot ·)
- "Recipients" heading is visible
- "184 buyers · 96% deliverable" is visible as muted stat text (middle-dot ·)
- Row "Buyers · last 30 days" with "47 buyers · avg $74 spend" — switch ON — count "47" visible
- Row "Repeat buyers" with "23 buyers · 2+ orders" — switch ON — count "23" visible
- Row "Followers without an order" with "114 followers" — switch ON — count "114" visible
- Row "All-time buyers" with "142 buyers · since Mar" — switch OFF — count "142" visible
- ON rows have blue-tinted background, OFF row has plain background

**Result:** [x] pass

**Evidence:**
- Screenshot: `evidence/screenshots/S23-03.png`

**Notes:** —

---

### S23-04 — Composer Step 2: Content — template chips, subject, char count, open-rate, preview text, featured product

**Steps:**
1. Navigate to `/seller/marketing`
2. Observe the Step 2 section in the left composer pane

**Expected:**
- Eyebrow text "Step 2 of 3 · Content" is visible (middle-dot ·)
- "Template" label is visible
- Template chip "Restock" is visible with active (dark) styling
- Template chips "New drop", "Behind the scenes", "Discount code", "Plain text" are visible (inactive)
- "Subject" label is visible
- Subject field shows "The persimmon vase is back · just 8 this batch" (middle-dot · between "back" and "just")
- Blinking cursor is rendered at the end of the subject
- Character count "52 / 80" is visible (right-aligned below subject)
- Open-rate forecast "32%" is visible (left-aligned below subject, with "above your avg" note)
- "Preview text" label is visible
- Preview text field shows "A small restock — three glaze variations this round." (em-dash — after "restock")
- "Featured product" label is visible
- "Persimmon vase" product name is visible in the featured product row
- "8 in stock · $86" product detail is visible (middle-dot ·)
- "Change" ghost button is visible next to the featured product

**Result:** [x] pass

**Evidence:**
- Screenshot: `evidence/screenshots/S23-04.png`

**Notes:** —

---

### S23-05 — Composer Step 3: Schedule — Send radios and Follow-ups toggles

**Steps:**
1. Navigate to `/seller/marketing`
2. Observe the Step 3 section in the left composer pane

**Expected:**
- Eyebrow text "Step 3 of 3 · Schedule" is visible (middle-dot ·)
- "Send" label is visible
- Radio "Now" — "Sends within 5 min" — is visible (inactive)
- Radio "Best time · Thu 6 PM" — "Highest opens for your list" — is visible and selected (filled radio, darker border)
- Radio "Pick a time" — "— select date & time —" — is visible (inactive, em-dash at start)
- "Follow-ups" label is visible
- Toggle row "Re-send to non-openers · 3 days later" — switch ON (middle-dot ·)
- Toggle row "Auto-pause if > 0.5% spam complaint" — switch OFF (greater-than > sign literal)

**Result:** [x] pass

**Evidence:**
- Screenshot: `evidence/screenshots/S23-05.png`

**Notes:** —

---

### S23-06 — Preview pane: sun/moon toggle, email header, subject, preview text

**Steps:**
1. Navigate to `/seller/marketing`
2. Observe the right preview pane

**Expected:**
- "Preview" heading is visible
- Sun/moon mode toggle is visible (2-button pill toggle)
- Sun icon button is active (dark/filled background)
- Moon icon button is inactive (transparent)
- Email mock card is visible
- Email header shows sender name "Micro Commerce" (NOT "Mira Studio")
- Email header shows "to you · Tue 6:00 PM" (middle-dot ·)
- Subject line "The persimmon vase is back · just 8 this batch" is visible in the email card
- Preview text "A small restock — three glaze variations this round." is visible below subject (em-dash —)

**Result:** [x] pass

**Evidence:**
- Screenshot: `evidence/screenshots/S23-06.png`

**Notes:** —

---

### S23-07 — Preview pane: hero image, body copy, CTA button, sign-off

**Steps:**
1. Navigate to `/seller/marketing`
2. Scroll the preview pane to see the email body

**Expected:**
- Hero product image (Persimmon vase / clay tone) is rendered below the email header
- "Hi Sasha," greeting is visible in larger display font
- First body paragraph mentions "persimmon vases out of the kiln Sunday"
- Second body paragraph mentions "followers get $5 off through Friday"
- "Shop the restock →" CTA button is visible (primary styling, right arrow →)
- Sign-off reads "— Alex" (em-dash — followed by "Alex", NOT "— Mira")

**Result:** [x] pass

**Evidence:**
- Screenshot: `evidence/screenshots/S23-07.png`

**Notes:** —

---

### S23-08 — Preview pane: email footer with "Micro Commerce" and Unsubscribe link

**Steps:**
1. Navigate to `/seller/marketing`
2. Observe the bottom of the email mock card in the preview pane

**Expected:**
- Email footer text reads "You're getting this because you bought from Micro Commerce" (NOT "Mira Studio")
- "Unsubscribe" is visible as a link (blue/primary color)
- Footer has a top border separating it from the email body

**Result:** [x] pass

**Evidence:**
- Screenshot: `evidence/screenshots/S23-08.png`

**Notes:** —

---

### S23-09 — Sidebar navigation: "Customers" item is active

**Steps:**
1. Navigate to `/seller/marketing`
2. Observe the SellerSidebar

**Expected:**
- Sidebar is present on the left side
- "Customers" nav item is highlighted as the active route (the design sets `active="Customers"`)
- Other nav items are visible but not active

**Result:** [x] pass

**Evidence:**
- Screenshot: `evidence/screenshots/S23-09.png`

**Notes:** —

---

### S23-10 — A11y: heading hierarchy, step eyebrows, switch roles, radio roles

**Steps:**
1. Navigate to `/seller/marketing`
2. Run `agent-browser snapshot` to capture the a11y tree

**Expected:**
- "Email recent buyers" heading is present at an appropriate heading level
- Step eyebrow texts "Step 1 of 3 · Audience", "Step 2 of 3 · Content", "Step 3 of 3 · Schedule" are readable
- "Save draft", "Send test", "Schedule send" buttons have accessible role=button
- Audience toggle switches have accessible role=switch or role=checkbox
- Send-time radio options have accessible role=radio
- Follow-up toggle switches have accessible role=switch or role=checkbox
- "Shop the restock →" button/link in preview has accessible role

**Result:** [x] pass

**Evidence:**
- Screenshot: `evidence/screenshots/S23-10.png`

**Notes:** —

---

### S23-11 — Brand parity: "Mira" entirely absent from page, design annotation absent

**Steps:**
1. Navigate to `/seller/marketing`
2. Run `agent-browser snapshot` and inspect page text for "Mira"
3. Inspect for the design annotation "marketing · email composer"

**Expected:**
- The string "Mira" does NOT appear anywhere on the page (sender, sign-off, footer, or anywhere else)
- Sender in preview email is "Micro Commerce" (not "Mira Studio")
- Email sign-off is "— Alex" (not "— Mira")
- Email footer says "bought from Micro Commerce" (not "Mira Studio")
- The design annotation "marketing · email composer" does NOT appear on the page

**Result:** [x] pass

**Evidence:**
- Screenshot: `evidence/screenshots/S23-11.png`

**Notes:** This is a HIGH priority case. The hi-fi design source (`hifi-seller-mgmt.jsx` lines 682–703) still uses "Mira Studio" as the avatar name, "— Mira" as the sign-off, and "Mira Studio" in the footer. The production implementation must use "Micro Commerce" / "— Alex" per brand constants.

---

## Summary

| Case   | Description                                                               | Result      |
|--------|---------------------------------------------------------------------------|-------------|
| S23-01 | Smoke: page loads with HTTP 200 and no console errors                     | [x] pass    |
| S23-02 | Topbar: "Email recent buyers", subtitle, 3 action buttons                 | [x] pass    |
| S23-03 | Step 1 Audience: Recipients heading, 3 ON rows + 1 OFF row with counts    | [x] pass    |
| S23-04 | Step 2 Content: template chips, subject, char count, preview text, product | [x] pass    |
| S23-05 | Step 3 Schedule: Send radios ("Best time" active), Follow-ups toggles     | [x] pass    |
| S23-06 | Preview pane: sun active, sender = "Micro Commerce", subject + preview    | [x] pass    |
| S23-07 | Preview pane: hero image, body copy, CTA "Shop the restock →", "— Alex"  | [x] pass    |
| S23-08 | Preview footer: "bought from Micro Commerce" + Unsubscribe link           | [x] pass    |
| S23-09 | Sidebar nav: "Customers" item active                                      | [x] pass    |
| S23-10 | A11y: heading hierarchy, switch/radio roles, button roles                 | [x] pass    |
| S23-11 | Brand parity: "Mira" entirely absent, annotation absent                   | [x] pass    |

**Batch F / TC-S23 result: 11 pass, 0 fail, 0 blocked, 0 not run**
