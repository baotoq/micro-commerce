# TC-S26 — Route: /seller/listings/[sku]/edit (wired delete flow)

**Route:** `/seller/listings/{sku}/edit`
**Batch:** E
**Worker:** qa-ultraqa
**Executed:** 2026-05-19

---

## Preconditions

- Full Aspire stack running.
- A fixture SKU `QA-DELETE-<ts>` is created via API for each scenario and either deleted by the test (happy path) or cleaned up afterwards.

---

## Test cases

### S26-01 — Delete listing button opens the confirm modal

**Result:** [x] pass

**Evidence:** `evidence/screenshots/S26-01.png`

**Notes:** Snapshot showed `[role="alertdialog"]` with `aria-modal="true"`, `aria-labelledby` pointing to `heading "Delete listing?" [level=2]`, `aria-describedby` for the permanence message, `button "Delete"` (confirm) and `button "Cancel"` inside the dialog, plus a backdrop `button "Close dialog"`. `document.activeElement.textContent === "Delete"` confirms initial focus on the confirm button.

---

### S26-02 — Confirming the modal deletes the listing and redirects to /seller/listings

**Result:** [x] pass

**Evidence:** `evidence/screenshots/S26-02.png`

**Notes:** After clicking the in-dialog Delete, `window.location.pathname === "/seller/listings"` and `GET /api/products/<sku>` returned **404**. Confirm button label switches to "Deleting…" during the transition (disabled). No console errors.

---

### S26-03 — Cancel button dismisses the modal without deleting

**Result:** [x] pass

**Evidence:** `evidence/screenshots/S26-03.png`

**Notes:** After Cancel, `document.querySelector("[role=alertdialog]") === null` (modal removed). URL stays on `/seller/listings/<sku>/edit`. API GET still returns **200** — record intact.

---

### S26-04 — Escape key closes the modal without deleting

**Result:** [x] pass

**Evidence:** `evidence/screenshots/S26-04.png`

**Notes:** After Escape, modal is removed from the DOM. URL unchanged. API GET still returns **200**. Escape handler is registered via `document.addEventListener("keydown", ...)` in the component.

---

### S26-05 — Modal a11y: keyboard focus stays inside the dialog while open

**Result:** [x] pass — _after fix in this run_

**Evidence:** `evidence/screenshots/S26-05.png`

**Notes:** **FINDING (fixed during this run):** initial implementation did not trap focus — tabbing once leaked focus to elements behind the modal. Fixed by adding a Tab/Shift+Tab handler in `delete-listing-button.tsx:18-37` that cycles between the first and last focusable element inside the dialog. Re-verified: after 6 sequential Tab presses, focus alternates Delete → Cancel → Delete → Cancel → … and `document.activeElement.closest("[role=alertdialog]")` remains non-null throughout.

---

## Summary

| Case   | Description                                          | Result |
|--------|------------------------------------------------------|--------|
| S26-01 | Delete button opens accessible confirm modal         | [x]    |
| S26-02 | Confirm deletes record and redirects to listings     | [x]    |
| S26-03 | Cancel button dismisses without deleting             | [x]    |
| S26-04 | Escape key dismisses without deleting                | [x]    |
| S26-05 | Focus stays trapped inside the dialog                | [x]    |

**TC-S26 result: 5 pass, 0 fail, 0 blocked** — one P2 a11y defect (no focus trap) found and fixed during this run.
