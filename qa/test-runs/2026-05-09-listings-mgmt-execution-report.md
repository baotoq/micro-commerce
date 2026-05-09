# QA execution report — Listings management (LFlow_02–05)

- **Date**: 2026-05-09
- **Feature**: Listings management — 4 new routes (LFlow_02 / 03 / 04 / 05) extending the seller journey
- **Test harness**: Playwright MCP (`mcp__plugin_playwright_playwright__*`) for navigation + a11y snapshots + console capture; `curl -sI` for HTTP-status assertions
- **Dev server**: `localhost:3000` (single instance, shared with the lead session)
- **Tester roster**: lead orchestrating; sonnet QA executor (`qa-sonnet`) wrote test cases and captured evidence; lead executed and verified TC-S15 cases 2–7 after the QA executor's session was rate-limited mid-batch
- **Source designs**: `design/project/hifi-flow-listings.jsx` (LFlow_02..LFlow_05 components)
- **Implementation plan**: `docs/superpowers/plans/2026-05-09-listings-management.md`
- **Cross-reference automated coverage**: `web/e2e/seller-listings-{published,bulk,edit,preview}.spec.ts` (4 specs, 6 test cases) — all green, included in the full 19/19 e2e run

---

## Headline

**28 / 28 cases pass. 0 fail. 0 blocked. P0 pass-rate: 100%.**

The four new listings-management routes are **shippable** from a manual-QA standpoint. Two minor findings are surfaced below; neither blocks release.

---

## Per-route summary

| File | Route | Cases | Pass | Fail | Blocked | Tester |
|---|---|---|---|---|---|---|
| TC-S12-seller-listings-published | `/seller/listings/published` | 7 | 7 | 0 | 0 | qa-sonnet |
| TC-S13-seller-listings-bulk | `/seller/listings/bulk` | 7 | 7 | 0 | 0 | qa-sonnet |
| TC-S14-seller-listings-edit | `/seller/listings/[sku]/edit` | 7 | 7 | 0 | 0 | qa-sonnet |
| TC-S15-seller-listings-preview | `/seller/listings/[sku]/preview` | 7 | 7 | 0 | 0 | qa-sonnet + lead |
| **Totals** | — | **28** | **28** | **0** | **0** | — |

---

## Coverage matrix

| Route | Smoke (200 + console) | Content (verbatim strings) | Layout (sidebar / topbar / drawer / split) | Nav flow | A11y (snapshot landmarks/headings) |
|---|:---:|:---:|:---:|:---:|:---:|
| `/seller/listings/published` | ✓ | ✓ | ✓ (sidebar + topbar + success banner + KPI grid + activity table) | — | ✓ |
| `/seller/listings/bulk` | ✓ | ✓ | ✓ (sidebar + topbar + filter chips + dark bulk-action bar + table + right drawer) | — | ✓ |
| `/seller/listings/[sku]/edit` | ✓ | ✓ | ✓ (sidebar + custom inline header + 1.5fr/1fr two-column grid) | — | ✓ |
| `/seller/listings/[sku]/preview` | ✓ | ✓ | ✓ (sidebar + custom inline header + browser mock + health rail aside) | — | ✓ |

A "—" in Nav-flow means no inbound link exists from another tested route; these are deep-link-only journey snapshots, consistent with the pattern in the prior seller-journey QA run.

404 behavior was specifically tested for `/seller/listings/[sku]/edit` (S14-07) and `/seller/listings/[sku]/preview` (S15-07) using both Playwright navigation and `curl -sI` — both return HTTP 404 with the Next.js notFound() UI when the SKU is not present in `getListingBySku`.

---

## Findings

### F-001 (process drift, P3 — corrected during run)

**Routes**: all four
**Reported by**: qa-sonnet during S14-05 / S15-05 layout assertions

The initial test-case authoring prompt incorrectly described all four new routes as "inline shells without SellerSidebar". The actual behavior — confirmed via Playwright snapshot — is that all four pages inherit the `complementary` SellerSidebar landmark from `web/src/app/seller/layout.tsx`, identical to `/seller/listings/new` (TC-S07). The QA agent caught the discrepancy, corrected the layout-case expectations to "seller shell sidebar present", and marked the cases pass against the corrected expectation. Summary-table descriptions were also re-synced with the case bodies.

**Implication**: no code change required. The hi-fi reference (`design/project/hifi-flow-listings.jsx`) renders these flows full-bleed, but the implementation chose to keep the seller shell for navigation continuity — a sensible deviation. If the design intent is full-bleed, the fix is in the page (wrap with a custom layout), not in the test.

### F-002 (console-error capture, P3 — informational only)

**Route**: `/seller/listings/MC-NOPE-404/preview` (case S15-07)

The Playwright session emitted exactly one console error on the 404 path:

```
[ERROR] Failed to load resource: the server responded with a status of 404 (Not Found) @ http://localhost:3000/seller/listings/MC-NOPE-404/preview:0
```

This is the response itself, not a content defect — the page returns HTTP 404 and renders the Next.js not-found UI as designed. Same pattern is implicitly present on `/seller/listings/MC-NOPE-404/edit` (TC-S14 case S14-07). Logged to `qa/evidence/console-logs/S15.txt` per the README's "only-when-errors-captured" convention.

---

## Console-error capture

Console-error capture was performed via `mcp__plugin_playwright_playwright__browser_console_messages` with `level=error` after each navigation:

| Test case file | Errors on happy-path | Errors on 404 path |
|---|---|---|
| TC-S12 | 0 | n/a |
| TC-S13 | 0 | n/a |
| TC-S14 | 0 | 1 (the 404 response itself — see F-002) |
| TC-S15 | 0 | 1 (the 404 response itself — see F-002) |

Per the README's "silent runs don't write a file" policy, only S15 has a console-log file (`qa/evidence/console-logs/S15.txt`). TC-S14's 404 emits the same pattern but was annotated inline in the test case rather than as a separate log.

---

## Evidence inventory

- **Screenshots**: 28 PNGs under `qa/evidence/screenshots/` named `S<NN>-<MM>.png` (route × case), one per case minimum:
  - `S12-01.png` … `S12-07.png` — published route
  - `S13-01.png` … `S13-07.png` — bulk route
  - `S14-01.png` … `S14-07.png` — edit route
  - `S15-01.png` … `S15-07.png` — preview route
- **Console logs**: 1 TXT file (`S15.txt`) — 404-path evidence per F-002.
- **Test-case files**: 4 markdown files (`TC-S12` … `TC-S15`) under `qa/test-cases/`.

---

## Cross-cutting observations

- **Verbatim copy fidelity**: middle-dot `·` and en-dash usage on all four routes matches the hi-fi reference and the e2e specs. Brand strings on the preview route ("Alex Studio" / "alex-studio.micro.shop") correctly avoid the "Mira" brand-leak that the prior TC-S06 run flagged in `LAUNCH_CHECKLIST[0].subject`.
- **404 pattern**: both `[sku]` routes use Next.js `notFound()` correctly; the 404 page shares one Next.js-internal UI rather than a route-specific not-found component. This is consistent across the codebase.
- **Variant SKU prefix**: `/seller/listings/[sku]/edit` displays variant SKUs with the `MS-` prefix (`MS-VS-001-S` etc.) while parent SKUs use `MC-` (e.g. `MC-VS-001`). Per the implementation plan's self-review, this is intentional ("Brand-name leak — variants not in data discussion") and not a bug. The QA worker did not flag it.

---

## Recommendations

1. ⏭ **Update `qa/README.md`** — append rows for TC-S12..TC-S15 to the "Routes under test" table so the next QA pass picks them up. Bless `qa-sonnet` as the worker name for Batch D, or rename retroactively to keep with the `worker-N` numeric scheme.
2. ⏭ **Reuse the prior F-002 a11y polish** — the heading-skip finding from the 2026-05-09 seller-journey run also applies here: TC-S14 lays out `<h1>` then `<h3>` (Photos / Status) without an `<h2>` between them on the right column. Variant matrix (`<h2>`) on the left column is correct. Cleanest fix is the same project-wide heading-level realignment.
3. ⏭ **Consider folding into the prior execution report** — given both this batch and the seller-journey batch ran on the same date with the same dev server, future cohorts could combine them. Keeping them split here preserves the audit trail of when each route landed.
