# Worker-8 Design Audit — Analytics · Customers · First-Order · First-Month

**Date:** 2026-05-10  
**Routes audited:** `/seller/analytics` · `/seller/customers` · `/seller/first-order` · `/seller/first-month`  
**Hi-fi refs:** `hifi-analytics.jsx` (Analytics_Standard) · `hifi-flow-seller.jsx` (SFlow_05_FirstOrder, SFlow_07_Analytics) · `hifi-flow-analytics.jsx` (AFlow_01_Overview)

---

## 1. Summary

| Route | Status | Critical | Minor |
|---|---|---|---|
| `/seller/analytics` | Partial match | 1 | 4 |
| `/seller/customers` | Stub only | — | 1 |
| `/seller/first-order` | Good match | 1 | 3 |
| `/seller/first-month` | Good match | 0 | 3 |

---

## 2. Token / Color Audit

### Confirmed design-system tokens in globals.css

The following values used as raw hex literals in source were cross-checked against `globals.css` and confirmed to be valid token values (even though they're inlined rather than referenced via CSS custom properties):

| Hex literal | Token equivalent | Verdict |
|---|---|---|
| `#1d1d1f` | `--foreground` / `--color-foreground` (oklch 0.215) | Valid token value, but hardcoded |
| `#0066cc` | `--primary` light-mode (oklch 0.508 0.176 255) | Valid token value, but hardcoded |
| `#c2410c` | Not a defined CSS custom property in globals.css | **Hardcoded non-token** (`var(--terra)` in hi-fi) |
| `#34c759` / `#22c55e` | Not a defined CSS custom property | **Hardcoded non-token** (hi-fi uses `var(--good)`) |
| `#f5f5f7` | `--canvas-parchment` (oklch 0.965 0.001 286) | Valid token value, but hardcoded |
| `#e0e0e0` | Matches `{colors.hairline}` from DESIGN.md; not in globals.css as a named var | Hardcoded |
| `#1d1d1f99` | Opacity variant of foreground, not a token | Hardcoded |

### Finding: `#c2410c` is not a globals.css token

The hi-fi consistently uses `var(--terra)` (terracotta orange) for the social-legend dot, the funnel "Purchased" bar, the first-order status badge highlight, and the area chart stroke in first-month. The implementation files inline `#c2410c` directly rather than a CSS variable. This color has no defined custom property in `globals.css`.

### Finding: `#34c759` / `#22c55e` are not globals.css tokens

The hi-fi uses `var(--good)` for positive deltas. The implementation uses `#34c759` (first-month KPI) and `#22c55e` (first-order KPI) — two slightly different greens with no shared token, meaning they drift independently.

---

## 3. Per-Route Findings

### 3a. `/seller/analytics` — `analytics/page.tsx`

**Hi-fi reference:** `Analytics_Standard` in `hifi-analytics.jsx`

**Matches:**
- 4-up KPI row structure with period-selector pill group and Export button in topbar — correct.
- `2fr 1fr` grid for Revenue chart + Sources donut — correct.
- `1fr 1fr` grid for Top products + Conversion funnel — correct.
- Chart axis labels (Apr 1 / Apr 8 / Apr 15 / Apr 22 / Apr 30) — correct.
- Subtitle text "Apr 1 – Apr 30 · vs Mar 1 – Mar 30" — exact match.

**Deviations:**

1. **(Critical) Revenue over time chart is a placeholder.** `analytics/page.tsx:101–105` renders a flat `bg-[#f5f5f7]` div (`aria-label="Revenue over time chart"` / `role="img"`) instead of an actual area chart. The hi-fi shows a real multi-series area chart with Organic / Social / Direct overlays. No chart library is wired.

2. **(Minor) `SourcesDonut` uses hardcoded `PALETTE` array** (`sources-donut.tsx:4`) with `["#1d1d1f", "#0066cc", "#7a7a7a", "#cccccc", "#aacbe7"]`. The hi-fi uses `var(--ink)`, `var(--terra)`, `var(--forest)`, `var(--ink-4)`. The palette colors differ: the implementation replaces terracotta (social) with blue (`#0066cc`) and drops forest green entirely, so source legend colors don't match hi-fi.

3. **(Minor) `SourcesDonut` uses `rounded-lg` card** (`border-radius: 18px`) while all other analytics cards in the same page use `rounded-xl` (`border-radius: 24px`). Inconsistent radius within a single view. Hi-fi uses the `hf-card` class uniformly.

4. **(Minor) `ConversionFunnel` final bar is `bg-[#1d1d1f]`** for all stages. The hi-fi uses `var(--terra)` for the "Purchased" (last) bar to visually distinguish the completed stage. Implementation misses this color differentiation.

5. **(Minor) Period selector active item uses hardcoded `"white"` fill** (`analytics/page.tsx:35`). Should use `var(--background)` or the canvas token. Also uses `"var(--paper-2, #f5f5f7)"` with a fallback hex — a pattern that adds fragility if the token is renamed.

---

### 3b. `/seller/customers` — `customers/page.tsx`

**Hi-fi reference:** No dedicated customers hi-fi artboard was identified in the provided design files.

**Matches:** Topbar with title "Customers" and subtitle "All time" — present.

**Deviations:**

1. **(Minor) Page is a stub.** `customers/page.tsx:8` renders only `<p className="text-sm text-[#1d1d1f]/70">Coming soon</p>`. If this route is intended as a placeholder, the "Coming soon" text color uses a hardcoded hex rather than `text-muted-foreground`. No structural issues beyond stub state — acceptable if intentionally deferred, but flag for completeness.

---

### 3c. `/seller/first-order` — `first-order/page.tsx`

**Hi-fi reference:** `SFlow_05_FirstOrder` in `hifi-flow-seller.jsx`

**Matches:**
- Celebration banner with `background: #0066cc` (matches hi-fi `var(--primary)`) — correct.
- Decorative circles (right -20 / top -20, 180×180; right 80 / bottom -40, 110×110) — exact match.
- Eyebrow text "★ Your first order" — exact match.
- Headline "Sasha bought a Persimmon vase." — exact match.
- Subtext "$86.00 · placed 12 minutes ago · we held it for you to confirm." — exact match.
- Two banner buttons ("Send a thank-you" / "Open order →") — correct.
- 4-up KPI grid with sparklines — present.
- Orders table structure with orange highlight row, #1001, Sasha L., Persimmon vase, $86.00 — correct.
- Footer note "Funds are released to your bank 2 days after the order ships." — exact match.
- Topbar title "Good afternoon, Alex" + subtitle "Day 4 · Friday, March 15" — correct.

**Deviations:**

1. **(Critical) "Open order →" is a `<Link>` with `role="button"`, not a `<button>`.** `first-order/page.tsx:73–80` uses `role="button"` to satisfy an e2e assertion. This is a semantics override noted with a `biome-ignore` comment. The hi-fi shows a plain button. While there's a code comment explaining the workaround, the implementation bypasses proper button semantics for a navigation element — a significant a11y deviation.

2. **(Minor) KPI delta color is `#22c55e`** (`first-order/page.tsx:99`) — a hardcoded green not matching any globals.css token. Hi-fi uses `var(--good)`.

3. **(Minor) "New · pack today" badge** uses Tailwind utility `bg-orange-100 text-orange-700` (`first-order/page.tsx:164–166`). The hi-fi uses `hf-chip hf-chip-warn` which maps to the terracotta warn color. Orange-100/orange-700 is a Tailwind palette color, not a design-system token — color may drift from the design intent.

4. **(Minor) Customer avatar** is rendered as a circle with `bg-[#0066cc]/10 text-[#0066cc]` initial "S" (`first-order/page.tsx:150–153`). The hi-fi uses the `Avatar` component. No design-system `Avatar` component is used here — implementation is inline.

---

### 3d. `/seller/first-month` — `first-month/page.tsx`

**Hi-fi reference:** `SFlow_07_Analytics` in `hifi-flow-seller.jsx`

**Matches:**
- Topbar title "Your first month" + subtitle "Analytics · March 12 → April 11" — exact match.
- Compare and Export buttons in topbar — present (rendered as `rounded-full` ghost buttons).
- 4-up KPI row: Revenue / Orders / Conversion / Repeat buyers — correct labels and sparklines.
- `1.6fr 1fr` grid layout for chart + right column — exact match.
- Left card: "Daily revenue" heading, `$2,148` value, Day/Week/Month period chips — correct.
- Chart annotation "Day 4 · first sale" — exact match.
- Date axis: Mar 12 / Mar 19 / Mar 26 / Apr 2 / Apr 11 — exact match.
- Right column: Insight card with "★ Insight" eyebrow + Friday-afternoons text — exact match.
- Top sellers card with Persimmon vase / Forest bowl / Cream tumbler set — exact match.

**Deviations:**

1. **(Minor) KPI delta color is `#34c759`** (`first-month/page.tsx:101`) — a hardcoded green not matching any globals.css token. Hi-fi uses `var(--good)`. Additionally differs from `#22c55e` used in first-order, so two different greens appear across the same dashboard experience.

2. **(Minor) Area chart stroke and fill use hardcoded `#c2410c`** (`first-month/page.tsx:43–44`, inline `AreaChart` component). Hi-fi uses `var(--terra)`. No CSS custom property exists for this color.

3. **(Minor) Period selector active chip** (`first-month/page.tsx:139`) uses `background: "#1d1d1f", color: "white"` inline rather than design-system token. Inactive chips use `background: "#f5f5f7"` and `border: "1px solid #e0e0e0"` — all hardcoded. The border color `#e0e0e0` matches the DESIGN.md `{colors.hairline}` value but has no globals.css custom property.

---

## 4. Structural / Layout Gaps

1. **Analytics page has no "Revenue over time" chart** — the most prominent element in the hi-fi is entirely missing (placeholder only). This affects the primary purpose of the analytics dashboard.

2. **Analytics page missing Conversion funnel's "Purchased" bar terracotta highlight** — visual hierarchy of the funnel is flattened.

3. **No `--terra` / `--good` CSS custom properties in globals.css.** Both the hi-fi and multiple implementation files reference these semantic colors. They should be added as `@theme` tokens so implementations can reference `var(--terra)` and `var(--good)` consistently instead of each file hardcoding different hex values.

---

## 5. Token Hardcoding Summary

| File | Hardcoded values | Recommended token |
|---|---|---|
| `analytics/page.tsx` | `#1d1d1f`, `#c2410c`, `#f5f5f7`, `var(--paper-2, #f5f5f7)` | `var(--foreground)`, `var(--terra)`, `var(--canvas-parchment)` |
| `sources-donut.tsx` | `#1d1d1f`, `#0066cc`, `#7a7a7a`, `#cccccc`, `#aacbe7`, `#1d1d1f` (text) | `var(--ink)`, `var(--primary)`, `var(--terra)`, `var(--forest)` |
| `top-products.tsx` | `#1d1d1f`, `#1d1d1f` (bar fill) | `var(--foreground)` |
| `conversion-funnel.tsx` | `#1d1d1f` (all bars, including final) | `var(--foreground)`, `var(--terra)` for final bar |
| `first-order/page.tsx` | `#0066cc`, `#c2410c`, `#22c55e`, `#1d1d1f` | `var(--primary)`, `var(--terra)`, `var(--good)`, `var(--foreground)` |
| `first-month/page.tsx` | `#c2410c`, `#34c759`, `#1d1d1f`, `#f5f5f7`, `#e0e0e0`, `#1d1d1f99` | `var(--terra)`, `var(--good)`, `var(--foreground)`, `var(--canvas-parchment)`, `var(--hairline)` |
| `customers/page.tsx` | `#1d1d1f` (opacity) | `text-muted-foreground` |
