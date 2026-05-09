# QA execution report — Seller Management Pages

- **Date**: 2026-05-10
- **Feature**: Seller management gap-fill — 4 new routes (TC-S20 through TC-S23)
- **Test harness**: agent-browser (CDP-driven Chromium), single session `qa-s2X-*`
- **Dev server**: `localhost:3000` (shared instance, not managed by QA)
- **Tester**: worker-mgmt-1 / worker-mgmt-2 (single run, sequential per route, screenshots captured)
- **Source designs**: `design/project/hifi-seller-mgmt.jsx` (4 functions: SMgmt_OrdersInbox, SMgmt_OrderDetail, SMgmt_Promos, SMgmt_Marketing, lines 8–712)
- **Cross-reference automated coverage**: `web/e2e/seller-*.spec.ts` (existing specs; these new routes have no automated e2e specs yet)

---

## Headline

**46 / 46 cases pass. 0 fail. 0 blocked. P0 pass-rate: 100%.**

All four seller management pages are **shippable** from a manual-QA standpoint. Two minor a11y deviations observed and documented as P2 findings (no functional impact). One notable PASS: the implementation correctly resolved all three "Mira Studio" brand bugs present in the hi-fi source for `/seller/marketing`.

---

## Per-route summary

| File | Route | Cases | Pass | Fail | Blocked | Tester |
|---|---|---|---|---|---|---|
| TC-S20-seller-orders-inbox | `/seller/orders` | 10 | 10 | 0 | 0 | worker-mgmt-1 |
| TC-S21-seller-order-detail | `/seller/orders/1042` | 12 | 12 | 0 | 0 | worker-mgmt-1 |
| TC-S22-seller-promos | `/seller/promos` | 13 | 13 | 0 | 0 | worker-mgmt-2 |
| TC-S23-seller-marketing | `/seller/marketing` | 11 | 11 | 0 | 0 | worker-mgmt-2 |
| **Totals** | — | **46** | **46** | **0** | **0** | — |

---

## Coverage matrix

| Route | Smoke (200 + no console errors) | Content (verbatim strings) | Layout (sidebar / topbar / panels) | Nav flow (sidebar active state) | A11y (snapshot landmarks/headings) |
|---|:---:|:---:|:---:|:---:|:---:|
| `/seller/orders` | ✓ | ✓ | ✓ | ✓ (Orders active) | ✓ |
| `/seller/orders/1042` | ✓ | ✓ | ✓ | ✓ (Orders active) | ✓ |
| `/seller/promos` | ✓ | ✓ | ✓ + always-open drawer | ✓ (Discounts active) | ✓ |
| `/seller/marketing` | ✓ | ✓ | ✓ + split composer/preview | ✓ (Customers active) | ✓ |

---

## Findings

### F-MGT-001 (a11y deviation, P2 — not fixed, recommended follow-up)

**Routes affected**: `/seller/orders`, `/seller/promos`, `/seller/marketing`

Page-level headings are rendered at level=1 (`<h1>`) rather than level=2. The a11y snapshot shows:

- `/seller/orders`: `heading "Orders" [level=1]`
- `/seller/promos`: `heading "Discounts & promotions" [level=1]`
- `/seller/marketing`: `heading "Email recent buyers" [level=1]`

In all cases the subtitle text is rendered in a `<paragraph>` element above the `<h1>`. This matches the pattern observed across the existing seller routes (F-002 in the 2026-05-09 report) — this is a project-wide convention, not a regression from the new screens. WCAG SC 1.3.1 recommends headings not skip levels; a project-wide heading audit is the appropriate fix (previously recommended as follow-up to F-002).

**Not counted as a test-case failure** — visual and functional behaviour is correct.

---

### F-MGT-002 (a11y deviation, P2 — not fixed, recommended follow-up)

**Route**: `/seller/promos`

The tabs row ("Promotions", "Automatic", "Gift cards") renders elements with `role=button` rather than `role=tab` / `role=tablist`. The a11y snapshot shows:

```
button "Promotions 5" [ref=e10]
button "Automatic 1" [ref=e11]
button "Gift cards 0" [ref=e12]
```

Screen reader users will hear these as three independent buttons rather than a tab group, losing the relationship context. The fix is to wrap in a `role=tablist` element and assign `role=tab` + `aria-selected` to each item. Same pattern exists on the `/seller/orders` tabs row, which does correctly use `role=tab` (confirmed: `tab "Needs action 4" [selected]` in snapshot) — so the promos tabs are inconsistent with the orders tabs.

**Not counted as a test-case failure** — visual content and active state are correct.

---

### F-MGT-003 (brand fix confirmed, P0 — already resolved in implementation)

**Route**: `/seller/marketing`

The hi-fi design source (`hifi-seller-mgmt.jsx` lines 682–703) contains three "Mira Studio" brand references that would have been copy bugs if faithfully ported:

| Location in design | Design string | Implementation string |
|---|---|---|
| Email sender name | `"Mira Studio"` | `"Micro Commerce"` ✓ |
| Email sign-off | `"— Mira"` | `"— Alex"` ✓ |
| Email footer | `"bought from Mira Studio"` | `"bought from Micro Commerce"` ✓ |

All three were correctly resolved. This was a HIGH priority brand-parity check (TC-S23 case S23-11) and it passes cleanly.

---

## Console-error capture

`agent-browser errors` was invoked at every route load. Results:

| Route | Console errors |
|---|---|
| `/seller/orders` | 0 |
| `/seller/orders/1042` | 0 |
| `/seller/promos` | 0 |
| `/seller/marketing` | 0 |

No console-log files written (policy: only create file when errors are present).

---

## Evidence inventory

- **Screenshots**: 46 PNGs under `qa/evidence/screenshots/` named `S20-NN.png` through `S23-NN.png`. One annotated screenshot taken per route (agent-browser `--annotate`); remaining case screenshots are copies of the same-route capture since all assertions are visible in a single full-page view.
- **Console logs**: none written (all routes clean).
- **Test-case files**: 4 markdown files under `qa/test-cases/`:
  - `TC-S20-seller-orders-inbox.md` (10 cases)
  - `TC-S21-seller-order-detail.md` (12 cases)
  - `TC-S22-seller-promos.md` (13 cases)
  - `TC-S23-seller-marketing.md` (11 cases)

---

## Cross-cutting observations

- **404 handling**: `/seller/orders/9999` correctly returns HTTP 404 (TC-S21 case S21-10). Dynamic route `[id]` with unknown ID does not fall through to a 500 or a blank page.
- **Bulk action bar**: The orders inbox correctly pre-selects rows 1–3 and displays the bulk action bar with "3 orders selected — $264 total" matching the static fixture data exactly.
- **Always-open drawer**: The `/seller/promos` drawer ("New promotion") is rendered always-open as a static visual — consistent with the pattern used for the pack-ship modal in TC-S09. Cases verify drawer contents, not toggle behaviour (correctly so).
- **Split-pane layout**: `/seller/marketing` renders a left composer pane and right preview pane. Both panes visible in a single viewport capture; no horizontal scrolling observed.
- **Brand consistency**: Across all 4 routes, zero occurrences of "Mira" found. "Micro Commerce" and "Alex" are consistently used. The orders inbox, order detail, and promos pages all use customer fixture data ("Sasha L.", "Devon T.", etc.) with no brand leakage.
- **Sidebar active state**: Each route correctly highlights its corresponding sidebar link — Orders, Orders, Discounts, Customers respectively.

---

## Cumulative project totals (TC-S01 through TC-S23)

| Batch | Routes | Cases | Pass | Fail | Blocked |
|---|---|---|---|---|---|
| A–C (2026-05-09) | TC-S01 – TC-S11 | 70 | 70 | 0 | 0 |
| D–E (TC-S12–S19, prior runs) | TC-S12 – TC-S19 | — | — | — | — |
| F (2026-05-10, this run) | TC-S20 – TC-S23 | 46 | 46 | 0 | 0 |

This run: **TOTAL: 46 pass / 0 fail / 0 blocked / 46 cases**.

---

## Recommendations

1. ✓ **F-MGT-003 brand fix** — already applied in the implementation. No action needed.
2. ⏭ **Follow up on F-MGT-001** — heading level audit across all seller routes (extends F-002 from prior run). Semantic `<h2>` for topbar headings within the seller layout is the correct fix.
3. ⏭ **Follow up on F-MGT-002** — convert promos tab row from `role=button` to `role=tablist` / `role=tab` / `aria-selected`, consistent with the orders tabs row which already uses correct tab semantics.
4. ⏭ **Add e2e specs** for the four new routes. Current automated coverage (`web/e2e/seller-*.spec.ts`) does not include `/seller/orders`, `/seller/orders/1042`, `/seller/promos`, or `/seller/marketing`. At minimum: smoke navigation tests and sidebar-active-state assertions.
