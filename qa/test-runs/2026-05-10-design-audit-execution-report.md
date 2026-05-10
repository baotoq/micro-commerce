# Design Audit — Execution Report

**Date:** 2026-05-10
**Scope:** All 25 page routes under `web/src/app/` audited against `DESIGN.md` and the hi-fi JSX in `design/project/`.
**Team:** `design-audit` (10 verifier workers, parallel)
**Per-worker reports:** `qa/test-runs/2026-05-10-design-audit/worker-{1..10}.md`

## Coverage Matrix

| # | Worker | Routes audited | Verdict |
|---|---|---|---|
| 1 | worker-1 | `/`, `/seller/apply`, `/seller/onboard` | FAIL (`/`), Conditional Pass (apply, onboard) |
| 2 | worker-2 | `/seller/welcome`, `/seller` | FAIL, FAIL |
| 3 | worker-3 | `/seller/listings`, `/seller/listings/published` | FAIL, FAIL |
| 4 | worker-4 | `/seller/listings/new`, `/seller/listings/bulk` | FAIL, FAIL |
| 5 | worker-5 | `/seller/listings/[sku]/edit`, `/seller/listings/[sku]/preview` | FAIL, FAIL |
| 6 | worker-6 | `/seller/orders`, `/seller/orders/[id]` | Pass-with-issues × 2 (no critical) |
| 7 | worker-7 | `/seller/orders/[id]/pack`, `/seller/payouts` | Pass-with-issues × 2 |
| 8 | worker-8 | `/seller/analytics`, `/seller/customers`, `/seller/first-order`, `/seller/first-month` | FAIL, FAIL (stub), FAIL, Pass-with-issues |
| 9 | worker-9 | `/seller/marketing`, `/seller/promos` | FAIL, FAIL |
| 10 | worker-10 | `/seller/states/loading`, `…/empty-orders`, `…/first-sale`, `…/payout-error` | PASS, FAIL, FAIL, FAIL |

**Tally:** 25 routes audited · 4 PASS / Conditional Pass · 16 FAIL · 1 STUB (`/seller/customers`) · 1 PLACEHOLDER (`/`).

## Cross-Cutting Systemic Issues (the same defect across many pages)

These show up on the majority of pages and should be fixed at the design-system layer rather than per-page.

### S1 — Missing semantic color CSS variables in `globals.css`
Hi-fi files reference `var(--good)`, `var(--warn)`, `var(--bad)`, `var(--terra)`. None of these are declared in `web/src/app/globals.css`. Pages work around this with raw hex literals, producing drift:

| Semantic | Hex(es) used in code today | Where |
|---|---|---|
| good (green) | `#22c55e`, `#34c759`, `#16a34a`, `bg-emerald-600`, `bg-emerald-100/text-emerald-700` | first-order, first-month, payouts, listings, promos |
| warn (amber/orange) | `#b45309`, `bg-amber-100/text-amber-700`, `bg-orange-100/text-orange-700` | listings new/bulk, welcome, orders inbox, first-order |
| bad (red) | `#c0392b`, `#dc2626` | listings new/bulk, payout-error |
| terra | `#c2410c`, `rgba(194,65,12,0.04)` | first-month chart, listings bulk row highlight, pack swatch |

**Recommendation:** Add `--good`, `--warn`, `--bad`, `--terra` to `:root` in `globals.css` and migrate all inline hex usages to those tokens (or to corresponding Tailwind utility classes that read from them).

### S2 — `#1d1d1f` raw hex instead of `text-foreground`
Pervasive across primitives, listings, orders, welcome, marketing, promos, edit, preview, first-sale. `--foreground` is defined; the utility class is `text-foreground`. ~30+ occurrences across the audited pages.

### S3 — `#f5f5f7` raw hex instead of `bg-canvas-parchment`
Same pattern as S2; the token exists, the literal is used anyway. Affects most pages with off-white surfaces.

### S4 — Mixed radius grammar on primary CTAs
DESIGN.md mandates `rounded.pill` (9999px) for primary blue CTAs. Worker reports flagged 7 routes using `rounded-lg` / `rounded-xl` / `rounded-md` instead:
- `/seller/welcome` — "New listing"
- `/seller/listings/bulk` — "Apply to N items"
- `/seller/listings/[sku]/preview` — "Add to bag"
- `/seller/marketing` — email body CTA
- `/seller/promos` — "New promotion"
- `/seller/states/first-sale` — "Pack & ship →" and "Send a thank-you note"
- `/seller/states/payout-error` — "Update bank →"

This is the second-most-impactful systemic delta after the missing semantic colors.

### S5 — `fontWeight: 500` in two places, banned by DESIGN.md
DESIGN.md §Typography Principles: "Weight 500 is deliberately absent. The ladder is 300 / 400 / 600 / 700."
- `marketing-top.tsx` nav links (worker-1)
- `seller/listings/[sku]/edit/page.tsx` stock column (worker-5)

### S6 — Avatar background `#e8e3da` has no token
Used in `seller-topbar.tsx`. No definition in `globals.css` and no entry in DESIGN.md. Add as `--surface-avatar-warm` or similar, or remove.

## High-Severity Per-Page Issues (route-specific)

| Route | Issue | Source |
|---|---|---|
| `/` | Page is a placeholder stub ("Ready for revamp."). Zero hi-fi content rendered. | worker-1 |
| `/seller/customers` | Page is a "Coming soon" stub. | worker-8 |
| `/seller` | "New listing" CTA is near-black (`#1d1d1f`) — should be Action Blue (`var(--primary)`). Direct DESIGN.md violation. | worker-2 |
| `/seller` | "← Back to welcome" link is unspecified by hi-fi; Export button from hi-fi is missing. | worker-2 |
| `/seller/welcome` | Checklist done-circle is black — should be `var(--good)` (semantic mismatch). | worker-2 |
| `/seller/listings`, `/seller/listings/published` | Topbar title/subtitle DOM order inverted (subtitle first). Subtitle copy "X listings · Y published" diverges from hi-fi "42 products · 38 active". "Activity log →" rendered as `<span>`, not link. | worker-3 |
| `/seller/listings` | Card/grid view toggle (Listings_Cards hi-fi) not implemented. Checkbox column non-interactive. | worker-3 |
| `/seller/listings/new` | Right-rail missing Status toggle, Compare-at price, Inventory (pre-orders), Shipping section. Card radius `rounded-xl` (12px) instead of `--radius-lg` (18px). | worker-4 |
| `/seller/listings/bulk` | SKU prefix `MC-` vs hi-fi `MS-` (Mira Studio). Drawer primary uses `rounded-lg`. | worker-4 |
| `/seller/listings/[sku]/edit`, `…/preview` | SellerSidebar chrome missing — present in every other seller page and in hi-fi. | worker-5 |
| `/seller/listings/[sku]/preview` | "Add to bag" uses `rounded-lg` instead of `rounded-pill`. | worker-5 |
| `/seller/orders/[id]/pack` | `min-h-screen flex-col` on root fights parent grid; modal backdrop overflows content pane. | worker-7 |
| `/seller/analytics` | Revenue-over-time chart is a placeholder div (no chart rendered). | worker-8 |
| `/seller/first-order` | "Open order →" uses `<Link role="button">` overriding semantics (biome-ignore present — symptom, not fix). | worker-8 |
| `/seller/states/first-sale` | Primary CTA uses `bg-[#1d1d1f]` + `rounded-xl` — should be `bg-primary` + `rounded-pill`. | worker-10 |
| `/seller/states/payout-error` | Page references `var(--bad)` which doesn't exist; only inline CSS fallback `#c0392b` keeps the red rendering. Primary CTA uses `rounded-[min(var(--radius-md),12px)]` instead of pill. | worker-10 |

## Medium-Severity Patterns (worth a sweep PR)

- Unicode glyphs used where SVG icons specified: `✓` success banners (listings), `✕` modal close (pack), checkmark in Publish button (listings/new). DESIGN.md treats these as utility iconography — replace with icon components.
- Status badges: `CSS capitalize` lowercases hi-fi labels (`Active/Low/Out/Draft` → `active/low/out/draft`) — worker-3.
- Inconsistent within-page Tailwind palette: orders inbox uses `amber-*`, breadcrumb status chip on the same flow uses `orange-*` for the same warn tone — worker-6.
- KPI delta arrow color: blue (`#0066cc`) used for positive on `/seller`; should be green per hi-fi — worker-2.
- Hero H1 sizing: `/seller/apply` renders 64px but spec calls for 56px (`{typography.hero-display}`) — worker-1.
- Letter-spacing: marketing pages use `em` units, DESIGN.md specifies absolute px values (`-0.28px → -0.374px`).
- Input radius drift: `/seller/onboard` uses `rounded-lg` (18px) but should be `rounded-sm` (8px) — worker-1.

## Recommended Fix Order

The remediation pulls naturally into four PR-sized buckets:

1. **Token foundation** (unblocks ~50% of findings)
   - Add `--good`, `--warn`, `--bad`, `--terra`, `--surface-avatar-warm` to `globals.css`.
   - Migrate raw hex literals (`#1d1d1f`, `#f5f5f7`, `#22c55e`/`#34c759`/`#16a34a`, `#dc2626`/`#c0392b`, `#c2410c`) to tokens or Tailwind utilities reading from them.
2. **Radius grammar sweep** (S4)
   - Audit every primary blue CTA and switch to `rounded-pill`. 7 routes confirmed plus a sweep-search for `rounded-(lg|xl|md)` on `bg-primary`/`bg-[#0066cc]` elements.
3. **Banned weight 500 + Unicode-glyph icons**
   - Remove `fontWeight: 500` from `marketing-top.tsx` nav links and listings edit table stock column.
   - Replace `✓` / `✕` Unicode usages with SVG icon components.
4. **Per-page structural gaps**
   - Implement `/` homepage from `hifi-home.jsx`.
   - Build out `/seller/customers` from hi-fi.
   - Add SellerSidebar chrome to `/seller/listings/[sku]/{edit,preview}`.
   - Restore right-rail sections on `/seller/listings/new` (Status, Compare-at, Inventory, Shipping).
   - Wire real revenue chart on `/seller/analytics`.
   - Fix `/seller` primary CTA color and remove un-spec'd "Back to welcome" link.
   - Fix listings topbar title/subtitle DOM order and copy.

## Methodology Notes

- Workers used the `oh-my-claudecode:verifier` agent type. Each cross-referenced its assigned page(s) against DESIGN.md tokens, the hi-fi JSX in `design/project/`, `web/src/components/primitives.tsx`, and `web/src/app/globals.css` before flagging any hex literal as a violation (i.e. they confirmed no token mapping existed).
- This audit was static-only — no dev server boot, no Playwright snapshots. A follow-up visual QA pass (per `qa/README.md` agent-browser flow) would be the natural complement once the token-foundation PR lands.

## Related Reports

- `qa/test-runs/2026-05-09-execution-report.md` (functional QA, 70/70 pass)
- `qa/test-runs/2026-05-09-listings-mgmt-execution-report.md`
- `qa/test-runs/2026-05-10-seller-mgmt-pages-execution-report.md`
