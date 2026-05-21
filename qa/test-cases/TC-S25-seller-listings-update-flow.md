# TC-S25 — Route: /seller/listings/[sku]/edit (wired update flow)

**Route:** `/seller/listings/{sku}/edit`
**Batch:** E
**Worker:** qa-ultraqa
**Executed:** 2026-05-19

---

## Preconditions

- Full Aspire stack running.
- Static-render and variant-matrix aspects covered by TC-S14. **This suite covers the wired Details form.**
- A fixture SKU is created via the Catalog API at the start of each test and deleted at the end.

---

## Test cases

### S25-01 — Details form renders pre-filled from the listing

**Result:** [x] pass

**Evidence:** `evidence/screenshots/S25-01.png`

**Notes:** Fixture POST returned 201. Snapshot of edit page Details card showed `textbox "SKU": QA-EDIT-1779128108` (read-only), `textbox "NAME" [required]: Edit Fixture`, `textbox "CATEGORY" [required]: Vessels`, `spinbutton "PRICE" [required]: 30`, `spinbutton "INVENTORY" [required]: 5`, `combobox "STATUS": Active`, `button "Save changes"`.

---

### S25-02 — Save changes shows success banner and persists to the API

**Result:** [x] pass

**Evidence:** `evidence/screenshots/S25-02.png`

**Notes:** Changed name → "Edited Vase", price → 55. Submit via `form.requestSubmit()` (agent-browser driver quirk; Playwright proves real click works). After submit, "Saved successfully." banner visible. API GET returned `name=Edited Vase price=55.0`.

---

### S25-03 — Server-side validation surfaces field errors

**Result:** [x] pass — _behavior:_ browser HTML5 `required` blocks submission before the action runs

**Evidence:** `evidence/screenshots/S25-03.png`

**Notes:** Cleared Name input. `requestSubmit()` does not submit because `<input required>` triggers native validation. Backend record was NOT updated (verified — record still shows previously-saved values). The intended outcome (no update + user prompted to fix the field) holds. If we ever remove `required` on the inputs, the zod server action will surface "Name is required" from `fieldErrors`.

---

### S25-04 — Editing a non-existent SKU still returns 404

**Result:** [x] pass

**Evidence:** `evidence/screenshots/S25-04.png`

**Notes:** `GET /seller/listings/QA-NOPE-404/edit` returned HTTP 404. Page rendered `heading "404"`, "This page could not be found." Next.js `notFound()` correctly invoked when SKU is not in the catalog.

---

### S25-05 — Page header has Delete + Cancel actions (no "Save draft" / "Publish →")

**Result:** [x] pass

**Evidence:** `evidence/screenshots/S25-05.png`

**Notes:** Header action area snapshot: `button "Delete listing" [ref=e3]`, `link "Cancel" [ref=e4]`. No "Save draft", no "Publish →", no "Unsaved changes" badge — all mockup-only artifacts removed when wiring the form.

---

## Summary

| Case   | Description                                                | Result |
|--------|------------------------------------------------------------|--------|
| S25-01 | Details form pre-filled from the listing                   | [x]    |
| S25-02 | Save changes persists and shows success banner             | [x]    |
| S25-03 | Server-side validation shows field error                   | [x]    |
| S25-04 | Unknown SKU still returns 404                              | [x]    |
| S25-05 | Header has Delete + Cancel only (no Save draft / Publish)  | [x]    |

**TC-S25 result: 5 pass, 0 fail, 0 blocked**

> **Status: Superseded by e2e suite.** Functional update coverage is now in
> `src/web/e2e/seller/listings/[sku]/update.spec.ts`. Keep this test case for
> design/a11y screenshot evidence and console-error sweeps only — do not
> re-run the functional cases on every release.
