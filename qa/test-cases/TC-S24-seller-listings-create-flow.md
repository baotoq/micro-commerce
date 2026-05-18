# TC-S24 — Route: /seller/listings/new (wired create flow)

**Route:** `/seller/listings/new`
**Batch:** E
**Worker:** qa-ultraqa
**Executed:** 2026-05-19

---

## Preconditions

- Full Aspire stack running (web + Catalog API + Postgres). The Aspire AppHost assigns the web port dynamically — discover via `mcp__aspire__list_resources` or by reading `web-*` resource URLs.
- Catalog API reachable at `http://localhost:<catalog_port>` (default `5481` per recent run). Seed data present (42 products).
- Static-render aspects covered by TC-S07. **This test suite covers the wired form interactions only.**

---

## Test cases

### S24-01 — Form fields render with the wired inputs

**Result:** [x] pass

**Evidence:** `evidence/screenshots/S24-01.png`

**Notes:** Snapshot confirmed: `heading "New listing"`, `textbox "SKU"`, `textbox "Name"`, `spinbutton "Price"`, `spinbutton "Total in stock"`, `textbox "Category"`, `combobox "Status"` defaulting to "Draft", `button "Publish"` in topbar with `form="new-listing-form"`.

---

### S24-02 — Submitting empty form shows field-level validation

**Result:** [x] pass

**Evidence:** `evidence/screenshots/S24-02.png`

**Notes:** After clicking Publish, snapshot showed "SKU is required", "Name is required", "Category is required". No console errors.

---

### S24-03 — Happy path: valid input creates a product and redirects to /seller/listings

**Result:** [x] pass

**Evidence:** `evidence/screenshots/S24-03.png`

**Notes:** Post-submit `window.location.pathname = "/seller/listings"`. API GET returned `{ sku: "QA-CREATE-1779128057", name: "QA Test Vase", category: "Ceramics", price: 49.99, inventory: 10, status: "active" }`. Cleanup DELETE returned 204. (Note: agent-browser's `click` on a button outside the form via `form=` attr does not trigger React form submission — `requestSubmit()` via `eval` was used as the driver. Playwright e2e confirms the real click works in Chromium.)

---

### S24-04 — Duplicate SKU shows an error and does not navigate

**Result:** [x] pass

**Evidence:** `evidence/screenshots/S24-04.png`

**Notes:** Posted SKU `MC-VS-001` (seeded). Stayed on `/seller/listings/new`; rendered "A listing with that SKU already exists." No console errors.

---

### S24-05 — SKU is normalized to uppercase + trimmed by the schema

**Result:** [x] pass

**Evidence:** `evidence/screenshots/S24-05.png`

**Notes:** Input SKU `"  qa-lower-1779128085  "`; created record SKU is `QA-LOWER-1779128085` (uppercase + trimmed). API GET returned 200 against the uppercase form. Cleanup DELETE returned 204.

---

## Summary

| Case   | Description                                              | Result |
|--------|----------------------------------------------------------|--------|
| S24-01 | Wired form fields render                                 | [x]    |
| S24-02 | Empty submit shows per-field errors                      | [x]    |
| S24-03 | Happy path creates product and redirects                 | [x]    |
| S24-04 | Duplicate SKU shows error, no navigation                 | [x]    |
| S24-05 | SKU is uppercased + trimmed                              | [x]    |

**TC-S24 result: 5 pass, 0 fail, 0 blocked**
