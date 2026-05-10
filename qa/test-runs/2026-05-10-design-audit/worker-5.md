# Design Audit — worker-5
**Pages:** `/seller/listings/[sku]/edit` · `/seller/listings/[sku]/preview`
**Hi-fi refs:** `hifi-flow-listings.jsx` (LFlow_03_Edit, LFlow_04_Preview) · `hifi-listings.jsx` (Listings_Editor)
**Auditor:** worker-5 · 2026-05-10

---

## 1. Layout & Structure

### Edit page (`/seller/listings/[sku]/edit`)

**Matches hi-fi:**
- Two-column grid `1.5fr 1fr` with variant matrix left and right column (photos + status) — correct.
- Header: back chevron, breadcrumb/title area, "Unsaved changes" chip, three action buttons (Discard / Save draft / Publish →) — all present.
- Variant matrix table with columns: Variant, SKU, Price, Stock, Status — matches LFlow_03_Edit exactly.
- Photos section shows 3-col grid of 4 photos + 1 upload slot — matches.
- Status segmented control (Active / Draft / Archived) — matches.
- Footer note "● 2 variants updated · prices +10%" — present and verbatim.

**Deviations:**
- **[MINOR] Photos grid count mismatch.** Hi-fi (Listings_Editor) shows a 5-col grid with 4 photos + 1 add slot; LFlow_03_Edit shows 3-col with 4 photos + 1 add slot. Implementation renders 3-col, which matches LFlow_03_Edit (the more specific Edit flow). No fault found vs. LFlow_03_Edit.
- **[MINOR] Header breadcrumb format.** Hi-fi LFlow_03_Edit shows `hf-tiny hf-muted` "Listings · Vessels" as the breadcrumb above the h1. Implementation renders `Listings · {listing.category}` — correct content. However, Listings_Editor hi-fi uses a full `hf-crumb` element with "Listings / Vessels / Persimmon vase" — the flow-specific LFlow_03_Edit uses just the muted tiny line, which the implementation follows. No fault.
- **[MINOR] No sidebar.** Both hi-fi refs (LFlow_03_Edit, Listings_Editor) include `<SellerSidebar active="Listings" />`. The implementation renders a full-screen layout with no sidebar. This is a structural omission — the seller sidebar chrome is missing from the edit page.

### Preview page (`/seller/listings/[sku]/preview`)

**Matches hi-fi:**
- Split layout: browser preview left, health rail right (320px) — correct.
- Header: back chevron, "Preview · {name}" eyebrow, "How shoppers will see it" h1, Desktop/Mobile toggle, Back to edit, Publish now → — all present and match LFlow_04_Preview exactly.
- Browser chrome: macOS traffic-light dots (`#FF6058`, `#FFBD2E`, `#28C941`), address bar with slug — correct.
- Product preview: 2-col split (image left 1.4fr, info right 1fr), listing name, price $95, stock note, SIZE chips (Small/Medium/Large·out), GLAZE swatches — all match.
- Health rail: score 96, progress bar, 5 check items — labels and sub-values match hi-fi exactly.

**Deviations:**
- **[MINOR] No sidebar.** Same issue as edit — LFlow_04_Preview includes SellerSidebar; implementation omits it.
- **[MINOR] Browser chrome background hex literal.** Implementation uses `style={{ background: '#E8E3D8' }}` for the browser chrome bar. This is a raw hex, not a CSS variable. This matches the hi-fi (`background: '#E8E3D8'`) where it is also hardcoded, so it is an intentional design decision, not a token. Noting for awareness.

---

## 2. Typography

### Edit page

| Element | Hi-fi spec | Implementation | Status |
|---|---|---|---|
| Page h1 (listing name) | `hf-display` 24px / lineHeight 1 | `text-[24px] font-semibold leading-none tracking-tight` | Match |
| Breadcrumb | `hf-tiny hf-muted` | `text-[11px] uppercase tracking-wider text-[#1d1d1f]/60` | Match |
| Section heading "Variant matrix" | `hf-h3` | `text-[15px] font-semibold` | Match |
| Table header | `hf-table` thead | `text-[11px] uppercase tracking-wider` | Match |
| Status card heading | `hf-h4` | `text-[13.5px] font-semibold` | Match |
| Photos card heading | `hf-h4` | `text-[13.5px] font-semibold` | Match |
| Footer note | `hf-tiny hf-muted` | `text-[11px] text-[#1d1d1f]/60` | Match |

**Issue — weight 500 used on stock column:**
Implementation applies `fontWeight: 500` to the stock cell (`style={{ fontWeight: 500 }}`).
DESIGN.md explicitly states: *"Weight 500 is deliberately absent. The ladder is 300 / 400 / 600 / 700."*
Hi-fi uses `fontWeight: 500` on the same cell in the JSX (`fontWeight: 500`), so this is carried from the hi-fi. Flagging as a **design-system violation inherited from hi-fi** — both hi-fi and implementation are inconsistent with DESIGN.md's weight ladder. The correct value should be 600.

### Preview page

| Element | Hi-fi spec | Implementation | Status |
|---|---|---|---|
| Page h1 | `hf-display` 22px / lineHeight 1 | `text-[22px] font-semibold leading-none tracking-tight` | Match |
| Eyebrow (Preview · name) | `hf-tiny hf-muted` | `text-[11px] uppercase tracking-wider text-[#1d1d1f]/60` | Match |
| Product name (inside preview) | `hf-display` 32px | `text-[32px] font-semibold leading-none tracking-tight` | Match |
| Price | `hf-display-2 hf-num` 22px | `text-[22px] font-semibold tabular-nums` | Match |
| Health score "96" | `hf-num hf-h4` color good | `text-[13.5px] font-semibold tabular-nums text-emerald-600` | Match |
| Check item label | `hf-h4` 12.5px | `text-[12.5px] font-medium` | Match |

---

## 3. Color & Token Usage

### Confirmed design tokens used correctly

- `bg-white` for header and cards — maps to `{colors.canvas}` ✓
- `bg-[#f5f5f7]` / `bg-canvas-parchment` for body background — maps to `{colors.canvas-parchment}` ✓
- `text-[#1d1d1f]` — maps to `{colors.ink}` ✓
- `border-black/[0.06]` — a close approximation of `rgba(0,0,0,0.06)` hairline ✓

### Raw hex literals present (not mapped to tokens)

| Location | Hex value | Should be |
|---|---|---|
| Edit: back button hover | `hover:bg-[#f5f5f7]` | `hover:bg-canvas-parchment` — functionally correct but not using token class |
| Edit: breadcrumb text | `text-[#1d1d1f]/60` | `text-foreground/60` or `text-ink/60` — `#1d1d1f` is the ink token value |
| Edit: h1 text | `text-[#1d1d1f]` | `text-foreground` |
| Edit: status card muted text | `text-[#1d1d1f]/60` | `text-muted-foreground` |
| Edit: changed-price color | `color: "#16a34a"` inline style | Tailwind `text-emerald-600` or a semantic token |
| Edit: photo tone colors | `const PHOTO_TONES = ["#e2d5c8","#efe8d9","#cfc7c2","#d8c0a8"]` | These are placeholder product image swatches — intentional mock colors, not token violations |
| Edit: changed-row highlight | `background: "rgba(27,94,63,0.04)"` | This matches the hi-fi exactly (`rgba(27,94,63,0.04)`) — intentional semantic color for changed-row state |
| Preview: browser chrome bar | `background: '#E8E3D8'` | Intentional chrome aesthetic color, matches hi-fi — not a token violation |
| Preview: glaze swatch Persimmon | `background: "#c2410c"` | Product color swatch — intentional mock data color |
| Preview: glaze swatch Cream | `background: "#F0E8D7"` | Product color swatch — intentional mock data color |
| Preview: stock cell color | `color: "#dc2626"` / `"#b45309"` | These are Tailwind red-600/amber-700 hex values; should use `text-red-600` / `text-amber-700` Tailwind classes |

**Summary:** The dominant use of `text-[#1d1d1f]` directly inlines the ink token hex rather than using the CSS custom property. This is a pervasive pattern across both pages — `#1d1d1f` appears approximately 15 times as a bare hex. The token `--foreground` (oklch equivalent of `#1d1d1f`) should be used via `text-foreground` class.

### Status chip colors

Edit page uses semantic Tailwind classes (`bg-emerald-50 text-emerald-700`, `bg-amber-50 text-amber-700`, `bg-red-50 text-red-700`) — these are not design-system tokens but are appropriate semantic colors consistent with the hi-fi's `var(--good)`, `var(--warn)`, `var(--bad)` variables. Acceptable.

---

## 4. Component Fidelity

### Edit page

| Component | Hi-fi | Implementation | Verdict |
|---|---|---|---|
| Back button | `hf-icon-btn` (icon-only) | `h-8 w-8 rounded-md` link with `‹` glyph | Acceptable — functionally equivalent |
| "Unsaved changes" chip | `hf-chip hf-chip-warn` with amber dot | `rounded-full bg-amber-50 px-2.5 py-1 text-[11px] font-medium text-amber-700` with amber dot | Match |
| Action buttons | ghost / outline / primary | `buttonVariants({ variant: "ghost" })` / `variant="outline"` / default | Match |
| Variant table row highlight | `rgba(27,94,63,0.04)` | Same via `style` prop | Match |
| Changed-price color | `var(--good)` green | `#16a34a` inline | Functionally correct, token not used |
| Status segmented control | paper-2 bg, active card bg with shadow | `bg-[#f5f5f7]` with active `bg-white shadow-sm` | Match |
| Photo upload slot | dashed border, `+` icon | `border-dashed border-black/20` with `+` | Match |
| "Visible at /slug" | `hf-mono` | `font-mono text-[#1d1d1f]` | Match |

**Missing: "Add to edit" right-rail sections from Listings_Editor.** The Listings_Editor hi-fi shows a right rail with Pricing, Inventory, Shipping, and SEO & tags sections. LFlow_03_Edit (the edit flow) does NOT include these — its right rail has only Photos and Status. Implementation correctly follows LFlow_03_Edit (the closer reference for this specific route), so this is not a deviation.

### Preview page

| Component | Hi-fi | Implementation | Verdict |
|---|---|---|---|
| Desktop/Mobile toggle | `paper-2` bg, active paper+shadow | `bg-[#f5f5f7] p-[3px]` with active `bg-white shadow-sm` | Match |
| Browser chrome | traffic lights + address bar | Same colors and layout | Match |
| "Add to bag · $95" button | `hf-btn-primary` h=44, full-width | `h-11 w-full rounded-lg` with default Button | Match except: hi-fi uses pill (`hf-btn-primary` which implies pill radius from design system), implementation uses `rounded-lg` (8px) — **radius mismatch** |
| Health score progress bar | `hf-progress` → `var(--good)` fill | `h-1.5 rounded-full bg-emerald-500 w-[96%]` | Match |
| Check icons | `hf-center` 18×18 circle, good/warn bg | Same structure with `h-[18px] w-[18px] rounded-full` | Match |
| Health check text: "✓" and "i" | Icon for good, icon for info | Text literals `"✓"` and `"i"` | Acceptable |
| Preview address bar URL | `mira-studio.micro.shop/persimmon-vase` | `alex-studio.micro.shop/{slug}` | **Brand correction applied** — hi-fi uses "Mira Studio" but CLAUDE.md mandates BRAND.owner="Alex"; implementation uses "alex-studio" ✓ |
| Product eyebrow in preview | `Mira Studio · Vessels` | `Alex Studio · {listing.category}` | **Brand correction applied** — correct per CLAUDE.md ✓ |

**Button radius issue (preview):** The "Add to bag · $95" button in the product preview uses `rounded-lg` (radius-sm, 8px). The hi-fi uses `hf-btn-primary` which in the design system maps to `{rounded.pill}` (9999px). DESIGN.md says primary action buttons should use `{rounded.pill}`. This is a **spec deviation** — the in-preview "Add to bag" button should be pill-shaped to match the product page design spec.

---

## 5. Findings Summary

### Critical / Spec-breaking
| # | Page | Finding | Hi-fi ref |
|---|---|---|---|
| F-01 | Both | **Missing seller sidebar** — SellerSidebar chrome is absent from both pages; hi-fi shows it on all seller pages including LFlow_03_Edit and LFlow_04_Preview | LFlow_03_Edit line 153, LFlow_04_Preview line 258 |
| F-02 | Preview | **"Add to bag" button uses `rounded-lg` (8px) instead of pill** — product CTA should use `{rounded.pill}` per DESIGN.md and `hf-btn-primary` spec | LFlow_04_Preview line 308 |

### Moderate
| # | Page | Finding | Detail |
|---|---|---|---|
| F-03 | Edit | **`fontWeight: 500` on stock column** — DESIGN.md explicitly excludes weight 500; should be 600. Inherited from hi-fi but both are non-conformant | edit/page.tsx line 205 |
| F-04 | Both | **Pervasive bare `#1d1d1f` hex** — `text-[#1d1d1f]` used ~15 times instead of `text-foreground` token class | Both pages throughout |
| F-05 | Edit | **Inline hex `#16a34a` for changed-price** — should use `text-emerald-600` Tailwind class or a semantic token | edit/page.tsx line 194 |
| F-06 | Preview | **Inline hex `#dc2626` / `#b45309`** for stock state colors in preview mini — should use Tailwind `text-red-600` / `text-amber-700` classes | preview/page.tsx line 200-203 |

### Minor / Informational
| # | Page | Finding | Detail |
|---|---|---|---|
| F-07 | Preview | Browser chrome bar `#E8E3D8` is a raw hex — acceptable as intentional mock chrome aesthetic, matches hi-fi | preview/page.tsx line 95 |
| F-08 | Both | `hover:bg-[#f5f5f7]` on back button should use `hover:bg-canvas-parchment` | Both pages, back button |
| F-09 | Preview | Glaze swatch hex values (`#c2410c`, `#F0E8D7`) are product mock data colors — intentional, not token violations | preview/page.tsx lines 161-167 |

### Confirmed Correct (notable)
- Brand swapped from "Mira Studio" → "Alex Studio" / "alex-studio.micro.shop" per CLAUDE.md mandate ✓
- Variant data in edit page matches LFlow_03_Edit exactly (SKUs, prices, stock counts, statuses) ✓
- Health check items match LFlow_04_Preview exactly (all 5 labels and sub-values verbatim) ✓
- Changed-row highlight `rgba(27,94,63,0.04)` matches hi-fi exactly ✓
- "● 2 variants updated · prices +10%" footer note matches hi-fi verbatim ✓
- Status segmented control (Active/Draft/Archived) with `paper-2` background and active-white style ✓
