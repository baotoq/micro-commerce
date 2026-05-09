# TC-S16 — Route: /seller/states/loading

**Route:** `/seller/states/loading`
**Batch:** E
**Worker:** worker-loading

---

## Preconditions

- Dev server running on http://localhost:3000
- SellerSidebar is provided by the `/seller` layout (no active link highlights for this sub-route — Overview is not highlighted because pathname does not match `/seller` exactly)

---

## Test cases

### S16-01 — Smoke: page loads with no console errors

**Steps:**
1. Navigate to `/seller/states/loading`
2. Wait for network idle
3. Capture console messages

**Expected:**
- HTTP 200 response
- No console errors emitted
- Page content visible (skeleton blocks rendered)

**Result:** [ ] not run

**Evidence:**
- Screenshot: `evidence/screenshots/S16-01.png`

---

### S16-02 — Sidebar: SellerSidebar is present in the layout

**Steps:**
1. Navigate to `/seller/states/loading`
2. Observe the left sidebar

**Expected:**
- `<aside>` landmark is present
- "Micro Commerce" brand name is visible in the sidebar header
- Nav links "Overview", "Orders", "Listings", "Analytics", "Customers" are visible

**Result:** [ ] not run

**Evidence:**
- Screenshot: `evidence/screenshots/S16-02.png`

---

### S16-03 — Top-bar skeleton: title and actions blocks present

**Steps:**
1. Navigate to `/seller/states/loading`
2. Observe the top-bar area (below any layout chrome, above the KPI grid)

**Expected:**
- Two stacked skeleton blocks visible on the left (narrow 140px and wider 220px)
- Two pill-shaped skeleton blocks visible on the right (94px and 120px wide, fully rounded)
- A border separates the top-bar from the content below

**Result:** [ ] not run

**Evidence:**
- Screenshot: `evidence/screenshots/S16-03.png`

---

### S16-04 — KPI grid: 4 skeleton cards

**Steps:**
1. Navigate to `/seller/states/loading`
2. Observe the upper grid section

**Expected:**
- 4 skeleton cards arranged in a 4-column grid
- Each card has 3 skeleton lines (label ~40% wide, value ~65% wide, chart bar 100% wide)
- Cards have white background, rounded corners, and subtle border

**Result:** [ ] not run

**Evidence:**
- Screenshot: `evidence/screenshots/S16-04.png`

---

### S16-05 — Lower grid: chart card and activity list with 4 rows

**Steps:**
1. Navigate to `/seller/states/loading`
2. Observe the lower 2-column grid

**Expected:**
- Left card (wider, ~1.6fr): 2 skeleton header lines + a large 200px-tall chart placeholder skeleton block
- Right card (~1fr): 1 skeleton header line + 4 activity rows, each row having a small circular dot skeleton and 2 stacked text skeletons
- Both cards have white background, rounded corners, and subtle border

**Result:** [ ] not run

**Evidence:**
- Screenshot: `evidence/screenshots/S16-05.png`

---

### S16-06 — A11y: aria-busy and aria-live on skeleton container

**Steps:**
1. Navigate to `/seller/states/loading`
2. Take an a11y snapshot

**Expected:**
- The skeleton content wrapper has `aria-busy="true"` attribute
- The skeleton content wrapper has `aria-live="polite"` attribute
- No interactive elements are present inside the skeleton (no focusable buttons or links within the skeleton content area)

**Result:** [ ] not run

**Evidence:**
- Screenshot: `evidence/screenshots/S16-06.png`

---

### S16-07 — No design-canvas annotation: "loading · skeleton" string is absent

**Steps:**
1. Navigate to `/seller/states/loading`
2. Search rendered HTML for the text "loading · skeleton"

**Expected:**
- The string "loading · skeleton" does not appear anywhere in the rendered page output
- No annotation chip or overlay is rendered

**Result:** [ ] not run

**Evidence:**
- Screenshot: `evidence/screenshots/S16-07.png`

---

## Summary

| Case   | Description                                                        | Result     |
|--------|--------------------------------------------------------------------|------------|
| S16-01 | Smoke: page loads with no console errors                           | [ ] not run |
| S16-02 | Sidebar: SellerSidebar present in layout                           | [ ] not run |
| S16-03 | Top-bar skeleton: title + actions blocks present                   | [ ] not run |
| S16-04 | KPI grid: 4 skeleton cards                                         | [ ] not run |
| S16-05 | Lower grid: chart card + activity list with 4 rows                 | [ ] not run |
| S16-06 | A11y: aria-busy/aria-live attributes on skeleton container         | [ ] not run |
| S16-07 | No design-canvas annotation: "loading · skeleton" absent           | [ ] not run |

**Batch E / TC-S16 result: 0 pass, 0 fail, 7 not run**
