# Design Audit — worker-2
**Routes:** `/seller/welcome` · `/seller` (overview/dashboard)
**Date:** 2026-05-10
**Auditor:** worker-2

---

## 1. Coverage

| Source file | Read |
|---|---|
| `web/src/app/seller/welcome/page.tsx` | yes |
| `web/src/app/seller/page.tsx` | yes |
| `web/src/components/seller/seller-topbar.tsx` | yes |
| `web/src/components/seller/kpi-card.tsx` | yes |
| `web/src/components/seller/revenue-chart.tsx` | yes (hex grep) |
| `web/src/components/seller/today-panel.tsx` | yes (hex grep) |
| `web/src/components/seller/recent-orders.tsx` | yes (hex grep) |
| `design/project/hifi-flow-seller.jsx` → `SFlow_04_DayOne` | yes (Welcome ref) |
| `design/project/hifi-seller.jsx` → `Seller_Overview` | yes |
| `design/project/hifi-seller-mgmt.jsx` | yes (chrome ref) |
| `DESIGN.md` | yes |
| `web/src/app/globals.css` | yes |

---

## 2. Per-page findings

### 2a. `/seller/welcome` (`SFlow_04_DayOne` hi-fi reference)

| # | Location | Severity | Finding |
|---|---|---|---|
| W-01 | `welcome/page.tsx:14` topbar `title` | Medium | Hi-fi says **"Welcome, Mira"**; impl says **"Welcome, Alex"**. `BRAND.owner` is "Alex" — copy diverges from hi-fi name but is intentional branding. Not a bug if `BRAND` is the source of truth. Flag for confirmation. |
| W-02 | `welcome/page.tsx:15` topbar `subtitle` | Low | Hi-fi: `"Day 1 · Tuesday, March 12"`. Impl hardcodes identical string. Date is static placeholder — no live date logic. Acceptable for prototype but label is frozen. |
| W-03 | `welcome/page.tsx:16–27` topbar `actions` | Medium | Hi-fi uses `<button hf-btn-outline>` with icon `share` labeled **"Share shop"**. Impl uses shadcn `<Button variant="outline">` labeled **"Share shop"** (no icon). Icon omitted. |
| W-04 | `welcome/page.tsx:19–26` "New listing" CTA | Medium | Hi-fi uses standard `hf-btn hf-btn-primary` (rounded-full pill). Impl uses an inline `<Link>` with `rounded-lg` (`{rounded.sm}` = 8 px). DESIGN.md specifies primary pill CTAs use `{rounded.pill}` (9999 px). Wrong radius grammar. |
| W-05 | `welcome/page.tsx:31` live-shop banner bg | Low | `bg-[#1d1d1f]` — this matches `{colors.ink}` / `{colors.surface-tile-1}` (#1d1d1f / #272729). Hex `#1d1d1f` is the ink token; hi-fi uses `var(--ink)`. **Defined token exists** (`--foreground` maps to OKLCH equivalent of #1d1d1f). Should use `bg-foreground` or `bg-surface-tile-1`. Raw hex instead of token. |
| W-06 | `welcome/page.tsx:93` empty-inbox card bg | Low | `bg-[#f5f5f7]` — this is `{colors.canvas-parchment}`. Token exists as `bg-canvas-parchment`. Raw hex instead of token. |
| W-07 | `welcome/page.tsx:136` progress-bar fill | Low | `bg-[#1d1d1f]` — same ink token issue as W-05. |
| W-08 | `welcome/page.tsx:147–148` checklist circle | Low | Inline `style` with `background: "#1d1d1f"` and `border: "1.5px solid #1d1d1f40"`. Same token miss. |
| W-09 | `welcome/page.tsx:180` hint chip | Medium | `bg-amber-100 text-amber-700`. Hi-fi uses `hf-chip hf-chip-warn` (which maps to the `{colors.warn}` system). Amber-100/700 are Tailwind palette colours outside the design token system entirely — no equivalent in globals.css. Token violation. |
| W-10 | `welcome/page.tsx:74` stat value size | Low | `text-[28px]` — hi-fi stat value is `fontSize: 28` (`hf-display-2`). Match. |
| W-11 | `welcome/page.tsx:34` "Your shop is live" eyebrow | Info | `text-[10px] uppercase tracking-widest`. Hi-fi: `hf-tiny` + `opacity: 0.6` + `letterSpacing: 0.08` + `textTransform: uppercase`. Implementation is structurally correct; `tracking-widest` (0.1em) is slightly wider than the hi-fi's 0.08 but acceptable. |
| W-12 | `welcome/page.tsx:37` shop URL headline | Low | `text-[32px] font-semibold leading-none`. Hi-fi: `fontSize: 32, lineHeight: 1`. Match. Colour is `text-white` which is correct for dark surface. |
| W-13 | `welcome/page.tsx:66–88` stat cards grid | Info | `grid-cols-3 gap-5` — hi-fi shows `repeat(3, 1fr)` gap. Correct. |
| W-14 | `welcome/page.tsx` stat card `subject` field | Low | Hi-fi shows a `{s.s}` subline like `"no activity yet"` for zeros. Impl shows `s.subject`. Confirm data shape matches — cosmetic if data is equivalent. |
| W-15 | `welcome/page.tsx:95–120` empty inbox card | Info | Structure matches hi-fi: icon circle, h3, body text. Colors use raw hex (see W-05, W-06). |
| W-16 | `welcome/page.tsx:123` launch checklist card | Info | `rounded-xl border border-black/[0.06]` — hi-fi uses `hf-card` which is `var(--line)` border + rounded. `black/[0.06]` is equivalent to DESIGN hairline. Acceptable. |
| W-17 | `welcome/page.tsx` checklist done state | Low | Done items use `background: "#1d1d1f"` (black fill + white check). Hi-fi uses `var(--good)` (green fill) for done circles. **Color mismatch** — green vs black for completed state. |

### 2b. `/seller` overview (`Seller_Overview` hi-fi reference)

| # | Location | Severity | Finding |
|---|---|---|---|
| O-01 | `seller/page.tsx:20` layout | Medium | Hi-fi shows sidebar (`SellerSidebar`) + topbar chrome. Impl renders a bare `<section>` with an inline header — **no sidebar is present**. The overview page appears to rely on a parent layout for the sidebar. Confirm that `web/src/app/seller/layout.tsx` provides the sidebar; if not, sidebar is missing. |
| O-02 | `seller/page.tsx:25–28` greeting | Low | Hi-fi: `"Good morning, Mira"` with `subtitle="Tuesday · April 8"`. Impl: `"Good morning, {BRAND.owner}"` with `DATE_LABEL` constant. Functionally equivalent; date is static placeholder. |
| O-03 | `seller/page.tsx:31–43` header actions | High | Hi-fi has `<button hf-btn-outline>Export</button>` and `<button hf-btn-primary>+ New listing</button>`. Impl has **"← Back to welcome"** link and **"+ New listing"** link. The "Back to welcome" link **does not exist in any hi-fi frame** — it is an implementation-only addition not specified in the design. Export button is missing. |
| O-04 | `seller/page.tsx:33` "Back to welcome" link | Medium | `rounded-full px-4 py-2 text-sm font-medium text-[#1d1d1f]` — uses raw hex. Also, this element is unspecified in hi-fi (see O-03). |
| O-05 | `seller/page.tsx:37–42` "New listing" button | Medium | `rounded-full bg-[#1d1d1f] px-4 py-2` — dark pill button. Hi-fi uses `hf-btn-primary` (Action Blue `{colors.primary}` #0066cc background). **Color mismatch**: dark/near-black vs Action Blue. Per DESIGN.md: "All primary pill CTAs use `{colors.primary}`." |
| O-06 | `seller/page.tsx:46–49` KPI grid | Low | `grid-cols-3 gap-4`. Hi-fi shows `repeat(4, 1fr)` for 4 stat cards. Impl maps `getOverviewKpis()` which may return 3 or 4 items — if 3, layout differs from hi-fi's 4-up row. |
| O-07 | `kpi-card.tsx:24` delta colour | Medium | Positive delta uses `text-[#0066cc]` (Action Blue). Hi-fi uses `var(--good)` (green) for `↑` trend arrows. Action Blue on a trend indicator is a token misuse — blue is reserved for interactive/link elements per DESIGN.md. |
| O-08 | `kpi-card.tsx:15` card radius | Low | `rounded-lg` = 8 px (`{rounded.sm}`). Hi-fi `hf-card` defaults to the system card radius which appears to be ~10–12 px. Minor radius mismatch; within acceptable tolerance. |
| O-09 | `seller/page.tsx:52–57` chart + today layout | Info | `grid-cols-3 col-span-2` — hi-fi shows `1.6fr 1fr`. Equivalent. |
| O-10 | `seller/page.tsx:22` date eyebrow | Low | `text-[#1d1d1f]/60` — raw hex. Token `text-foreground/60` exists. |

---

## 3. Token violations

All hex literals found below are raw colour values that have defined tokens in `globals.css` and should use Tailwind token classes instead.

| File | Line(s) | Raw hex | Correct token |
|---|---|---|---|
| `welcome/page.tsx` | 31, 51, 78–79, 112, 136, 147–148, 174 | `#1d1d1f` | `foreground` / `surface-tile-1` |
| `welcome/page.tsx` | 72, 84, 128, 185 | `#1d1d1f` (opacity variants) | `text-foreground/50` |
| `welcome/page.tsx` | 93 | `#f5f5f7` | `canvas-parchment` |
| `welcome/page.tsx` | 96 | `#1d1d1f` (opacity) | `text-foreground/40` |
| `welcome/page.tsx` | 115 | `#1d1d1f` (opacity) | `text-foreground/50` |
| `seller/page.tsx` | 23 | `#1d1d1f` (opacity) | `text-foreground/60` |
| `seller/page.tsx` | 33, 39 | `#1d1d1f` | `text-foreground` / `bg-foreground` |
| `kpi-card.tsx` | 16, 25 | `#1d1d1f` (opacity) | `text-foreground/70`, `text-foreground/60` |
| `kpi-card.tsx` | 24 | `#0066cc` | `text-primary` |
| `seller-topbar.tsx` | 19, 23, 32, 39 | `#1d1d1f` | `text-foreground` |
| `seller-topbar.tsx` | 39 | `#e8e3da` | no token defined — unspecified avatar bg colour |
| `today-panel.tsx` | 11 | `#0066cc` | `text-primary` |
| `revenue-chart.tsx` | 29, 36 | `#1d1d1f`, `#1d1d1f99` | `foreground` |

**Unspecified token:** `#e8e3da` (avatar background in `seller-topbar.tsx:39`) — this colour does not appear in DESIGN.md or globals.css. Not a defined token.

---

## 4. Verbatim copy diffs

| Element | Hi-fi text | Implementation text | Status |
|---|---|---|---|
| Welcome topbar title | `"Welcome, Mira"` | `"Welcome, Alex"` | Different name; `BRAND.owner = "Alex"` is intentional — not a bug |
| Welcome topbar subtitle | `"Day 1 · Tuesday, March 12"` | hardcoded `"Day 1 · Tuesday, March 12"` | Match (static) |
| Share shop button | `"Share shop"` (with icon) | `"Share shop"` (no icon) | Icon missing |
| Empty inbox headline | `"Your first order will land here"` | `"Your first order will land here"` | Match |
| Empty inbox body | `"We'll email you the moment it does. Until then, the launch list on the right will keep you busy."` | `"We'll email you the moment it does. Until then, the launch list on the right will keep you busy."` | Match |
| Checklist header | `"Launch checklist"` | `"Launch checklist"` | Match |
| Overview greeting | `"Good morning, Mira"` | `"Good morning, {BRAND.owner}"` (= "Good morning, Alex") | Name differs; intentional |
| Overview subtitle | `"Tuesday · April 8"` | `DATE_LABEL` constant | Static; acceptable |
| Overview header Export button | `"Export"` | absent — replaced with "← Back to welcome" | **Missing** |

---

## 5. Pass / fail verdict per route

| Route | Structural layout | Copy accuracy | Token hygiene | Color fidelity | CTA grammar | Verdict |
|---|---|---|---|---|---|---|
| `/seller/welcome` | PASS — layout matches hi-fi two-column grid | PASS — copy strings match | FAIL — pervasive raw hex throughout | FAIL — checklist done-circle is black (#1d1d1f) instead of green (var(--good)); hint chip uses amber-100/700 not warn token | FAIL — "New listing" uses rounded-lg not rounded-pill | **FAIL** |
| `/seller` (overview) | CONDITIONAL — sidebar presence depends on layout.tsx; page itself has no sidebar; KPI count may differ (3 vs 4) | PASS — greeting correct | FAIL — raw hex in page and component files | FAIL — "New listing" button is dark/black, not Action Blue (#0066cc); KPI delta arrow is blue not green | FAIL — primary CTA uses dark fill instead of `{colors.primary}` | **FAIL** |

---

## 6. Summary

**Critical issues (fix before ship):**
1. **O-03 / O-05** — Overview "New listing" button is near-black (`#1d1d1f`) instead of Action Blue (`{colors.primary}`). DESIGN.md is explicit: every primary pill CTA uses `{colors.primary}`. The "← Back to welcome" link is unspecified in any hi-fi frame and should be reviewed for inclusion.
2. **W-17** — Checklist done-state circles are black. Hi-fi uses `var(--good)` (green). A black filled check circle reads as a different semantic than a green one — the completion affordance is wrong.
3. **W-09** — Hint chip (`bg-amber-100 text-amber-700`) uses Tailwind palette colours entirely outside the design token system. Should map to `hf-chip-warn` equivalent tokens.
4. **W-04** — "New listing" CTA in welcome topbar uses `rounded-lg` (8 px) instead of `rounded-pill`. Wrong radius grammar for a primary action.

**Moderate issues:**
5. **O-07** — KPI positive-delta colour is `#0066cc` (Action Blue). Hi-fi uses green (`var(--good)`). Blue is reserved for interactive elements only.
6. **Pervasive raw hex** — `#1d1d1f`, `#f5f5f7`, `#0066cc`, and `#e8e3da` appear as inline strings across both pages and their components. All except `#e8e3da` have defined globals.css tokens.

**Minor / confirmatory:**
7. Sidebar presence on `/seller` depends on `layout.tsx` — needs verification but is architecturally expected.
8. Static date strings (`DATE_LABEL`, subtitle) are placeholder-level; no live date logic.
9. `#e8e3da` avatar background in `seller-topbar.tsx` is not defined in DESIGN.md or globals.css — origin unknown.
