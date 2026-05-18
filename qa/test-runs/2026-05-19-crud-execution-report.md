# Execution report — Listings CRUD wiring

**Date:** 2026-05-19
**Branch:** `dev` (commit `4d722765` and ultraqa fixups)
**Stack:** Aspire AppHost (web on dynamic port, Catalog API on `:5481`, Postgres 17.6 + Redis 8.6)
**Drivers:** agent-browser CLI v(local) + Playwright 1.59 (Chromium-only)

## Scope

QA the newly-wired seller-listings create / update / delete flows that ride on top of the existing Catalog API.

## Test cases

3 new manual test files, 15 cases total:

- `qa/test-cases/TC-S24-seller-listings-create-flow.md` (5 cases) — wired POST flow
- `qa/test-cases/TC-S25-seller-listings-update-flow.md` (5 cases) — wired PUT flow
- `qa/test-cases/TC-S26-seller-listings-delete-flow.md` (5 cases) — wired DELETE confirm modal

## Results

| TC      | Pass | Fail | Blocked | Notes                                                                 |
|---------|------|------|---------|-----------------------------------------------------------------------|
| TC-S24  | 5    | 0    | 0       | All five create-flow cases verified via agent-browser                 |
| TC-S25  | 5    | 0    | 0       | S25-03 effectively passes via HTML5 `required` (record not mutated)   |
| TC-S26  | 5    | 0    | 0       | S26-05 initially failed; fixed during run; re-verified                |
| **TOTAL** | **15** | **0** | **0** |                                                                     |

## Findings

**F-001 (P2 a11y, FIXED during run) — Modal lacked Tab focus trap.** The initial `delete-listing-button.tsx` implementation set initial focus to the Confirm button and registered an Escape handler, but did not constrain Tab/Shift+Tab. A single Tab leaked focus to elements behind the modal (Cancel link, Delete listing button, form inputs).

- Detected by: S26-05 (`document.activeElement.closest("[role=alertdialog]")` returned `false` after 1 Tab press)
- Fix: added a key handler in `delete-listing-button.tsx:18-44` that queries focusable descendants of the dialog and cycles focus on Tab/Shift+Tab.
- Re-verified: focus alternates `Delete ↔ Cancel` and stays inside the dialog through 6 sequential Tab presses.

## Regression — Playwright e2e

Full `npx playwright test --workers=1` run (109 specs) on a clean DB:

- **109 passed / 0 failed** (40s)
- The two known failures observed mid-run (`seller-listings.spec.ts`, `seller-listings-pagination.spec.ts`) were caused by a single QA fixture (`QA-DELETE-02-…`) that leaked from an earlier driver iteration where the in-dialog click selector was wrong (`button:has-text(...)` is Playwright-only; agent-browser uses `find text` or `@eN` refs). After the orphan was deleted via API, both specs pass.

## Driver notes (for future runs)

- agent-browser's `click` does NOT trigger React form submission when the button is **outside** the form and associated via the `form="..."` attribute. Use `agent-browser eval "document.getElementById('new-listing-form').requestSubmit()"` instead. Playwright's real click handles this case correctly.
- agent-browser does not understand Playwright selectors (`:has-text(...)`). Use `find text "..." click`, `find role button click --name "..."`, or scope JavaScript via `eval`.
- The Aspire web port is reassigned each restart. Discover with `mcp__aspire__list_resources` or by tailing the AppHost log.

## Files

- 3 new test cases: `qa/test-cases/TC-S{24,25,26}-seller-listings-*-flow.md`
- 15 screenshots: `qa/evidence/screenshots/S{24,25,26}-*.png`
- 1 console-log slot: `qa/evidence/console-logs/S24-02.txt` (empty — no errors observed)
- 1 code fix: `src/web/src/components/seller/listings/delete-listing-button.tsx` (focus trap)

## Status

**Feature is shippable.** All three CRUD flows are wired, validated, persistent, and accessible (modal correctly traps focus). Unit (397) + lint + build + e2e (109) all green.
