# Design Audit — worker-4
**Pages:** `/seller/listings/new` · `/seller/listings/bulk`
**Hi-fi refs:** `hifi-listings.jsx` (Listings_Editor) · `hifi-flow-listings.jsx` (LFlow_02_Bulk)
**Date:** 2026-05-10

---

## 1. Summary

Both pages implement the correct structural skeleton (topbar + content area with left-main / right-rail split) and capture the hi-fi intent reasonably well. However each page contains a cluster of hard-coded hex literals that should reference design tokens, a copy mismatch in the new-listing topbar, and several minor spacing / color deviations from the hi-fi.

---

## 2. Findings — `/seller/listings/new` (`new/page.tsx`)

### 2.1 Topbar copy mismatch (CONTENT)
- **Hi-fi:** The hi-fi `Listings_Editor` shows the topbar title as the product name (`Persimmon vase`) with a breadcrumb `Listings / Vessels / Persimmon vase`. Actions are `Preview` + `Publish changes`.
- **Implementation:** Title is `"New listing"`, subtitle `"Listings · Drafts · 1 of 1"`, actions are `Save draft` + `Publish` (with a ✓ checkmark via HTML entity).
- **Assessment:** The new-listing flow does not have a matching named component in the hi-fi; the closest is `Listings_Editor` which is the edit view. The implementation's title and subtitle are reasonable for a "new" state. The ✓ (`&#10003;`) in the Publish button label is not present in the hi-fi action buttons — hi-fi uses plain text labels. **Minor copy deviation.**

### 2.2 Hard-coded hex literals (TOKENS)
The following hex values appear inline and are not mapped to a design token:

| Location | Hex | Nearest token | File line(s) |
|---|---|---|---|
| Photo swatch fills | `#e2d5c8`, `#e3d4ce`, `#efe8d9` | No token — product image placeholders | Lines 36–45 |
| Title input border | `1.5px solid #1d1d1f` | Should be `border-foreground` or `border-border` (focus state) | Line 103 |
| Text color `text-[#1d1d1f]` throughout | `#1d1d1f` | `text-foreground` / `--foreground` | Lines 95, 106, 142, 161, 186, 209, 214 |
| Muted text `text-[#1d1d1f]/50`, `/40`, `/60` | opacity variants of `#1d1d1f` | `text-muted-foreground` | Lines 55, 79, 123, 127, 143, 152, 202, 214, 233 |
| `bg-[#f5f5f7]` | `#f5f5f7` | `bg-canvas-parchment` (`--canvas-parchment`) | Lines 49, 61, 85, 167, 221, 230 |
| `bg-[#f0f0f0]` (tag chips) | `#f0f0f0` | Close to `--divider-soft` (`#f0f0f0` = `oklch(0.949)`) — should use `bg-divider-soft` or `bg-muted` | Line 208 |
| Progress bar fill `bg-[#1d1d1f]` | `#1d1d1f` | `bg-foreground` | Line 53 |
| Emerald green `text-emerald-600`, `bg-emerald-500` | Tailwind utility, no design token | Design system uses `--good` (var, not tokenized in globals.css for Tailwind) | Lines 227, 231 |

**Note on emerald:** `--good` is a hi-fi CSS variable but is NOT defined in `globals.css`. The implementation uses Tailwind's `emerald-500/600` as a workaround. This is a gap in the token system, not strictly a page bug, but it creates inconsistency if `--good` is ever formally defined.

### 2.3 Photo section deviations (LAYOUT/CONTENT)
- **Hi-fi** (`Listings_Editor`): 5-column photo grid with `4 of 8` label, `drag to reorder` hint, and a single `+` / `Add` upload cell at end.
- **Implementation**: 3-column grid, 6 cells (`3 of 6`), with an in-progress upload placeholder (progress bar cell) and a "Drag to add" cell and a `+` cell. This is a valid creative variant for a "new listing" state (showing uploading in progress) but differs structurally from the hi-fi: column count (3 vs 5), max count (6 vs 8), and the presence of an upload-in-progress state.

### 2.4 Right column — missing Variants / Shipping / SEO sections (CONTENT)
- **Hi-fi** right rail includes: Status toggle, Pricing (Price + Compare-at), Inventory (stock + pre-orders toggle), Shipping (weight/origin/class), SEO & tags.
- **Implementation** right column has: Price & stock, Category & tags, Listing health.
- **Missing:** Status toggle segment, Compare-at price field, Inventory section with pre-orders toggle, Shipping section. These are significant omissions from the hi-fi editor spec.

### 2.5 Listing health section (MINOR)
- Implementation uses `bg-[#f5f5f7]` for the health card — correct parchment token value, but should use `bg-canvas-parchment`.
- Health score is `92` in implementation vs `96` in the hi-fi preview variant — acceptable as sample data difference.
- Description text `"3 photos · clear title · price set · description over 100 chars. Add 1 more photo to reach 100."` is reasonable but not matched to hi-fi text.

### 2.6 Border radius on cards (MINOR)
- Cards use `rounded-xl` (Tailwind default 12px). The design system token `--radius-lg` = 18px. `rounded-xl` ≈ 12px is short of the hi-fi's `hf-card` style. Should use `rounded-[18px]` or a custom token class.

---

## 3. Findings — `/seller/listings/bulk` (`bulk/page.tsx`)

### 3.1 Hard-coded hex literals (TOKENS)
| Location | Hex | Nearest token |
|---|---|---|
| Bulk-action bar background `bg-[#1d1d1f]` | `#1d1d1f` | `bg-foreground` |
| Check indicator `text-[#1d1d1f]` / `bg-white` inline | `#1d1d1f` | `text-foreground` |
| `text-[13.5px] font-semibold` (selected count) | non-standard size | Should match design typography scale |
| `text-[#1d1d1f]/50` throughout | opacity on foreground | `text-muted-foreground` |
| `text-[#1d1d1f]/60` | opacity on foreground | `text-muted-foreground` |
| `border-black/[0.06]`, `border-black/[0.04]` | raw black alpha | `border-border` |
| Stock color `#dc2626` (red-600) | Tailwind red — no token | Should use `--destructive` or a `--bad` token |
| Stock color `#b45309` (amber-700) | Tailwind amber — no token | Should use `--warn` token (not defined in globals.css) |
| New price color `#16a34a` | green-700 — no token | Should use `--good` token (not defined in globals.css) |
| New price zero-state `#1d1d1f40` | hex+alpha shorthand | Should use `text-muted-foreground` |
| Row selected highlight `rgba(194,65,12,0.04)` | raw rgba | No matching design token — carried from hi-fi |
| Drawer background `bg-white` | `#ffffff` | `bg-card` or `bg-background` |
| Eyebrow `text-[#1d1d1f]/50 uppercase tracking-wider` | raw opacity | `text-muted-foreground` |
| Drawer `bg-[#f5f5f7]` | `#f5f5f7` | `bg-canvas-parchment` |
| `border-l border-black/[0.06]` drawer border | raw black alpha | `border-border` |

### 3.2 Product names differ from hi-fi (CONTENT)
- **Hi-fi** rows: `Persimmon vase`, `Soft hand vessel`, `Indigo carafe`, `Shadow vase, tall` (ceramic vases — consistent with the "Mira" shop story).
- **Implementation** rows: `Rust mug Nº 04`, `Ember tea bowl`, `Peat serving bowl`, `Mist tumbler` — different products, different SKU prefix (`MC-` vs `MS-`).
- The hi-fi specifically uses `MS-` SKUs (Mira Studio). Implementation uses `MC-` (Micro Commerce). This is a naming inconsistency — SKU prefix should match the shop story.

### 3.3 Status badge uses Tailwind semantic colors, not design tokens (TOKENS)
```tsx
// Implementation
r.status === "Out"
  ? "rounded-full bg-red-50 px-2 py-0.5 text-[11px] font-medium text-red-700"
  : "rounded-full bg-amber-50 px-2 py-0.5 text-[11px] font-medium text-amber-700"
```
- Hi-fi uses `hf-chip hf-chip-bad` / `hf-chip hf-chip-warn` which map to `--bad` / `--warn` tokens. The implementation's `red-50/red-700` and `amber-50/amber-700` are Tailwind colors with no token backing. These work visually but are fragile if the token system is extended.

### 3.4 "Apply →" button pill shape (LAYOUT)
- Hi-fi renders Apply as a plain button with ink background. Implementation wraps it in `buttonVariants({ size: "sm" })` with `bg-white text-[#1d1d1f]` — correct.
- The drawer "Apply to 3 items" button uses `rounded-lg` (8px), but hi-fi has `hf-btn hf-btn-primary` (pill). DESIGN.md specifies `rounded.pill` (9999px) for primary CTAs. This is a **radius mismatch** — should be `rounded-full`.

### 3.5 Drawer "Cancel" uses Link not button (MINOR)
- Cancel navigates to `/seller/listings`, which is reasonable, but the hi-fi renders it as a `hf-btn hf-btn-ghost` with no href — suggesting it should dismiss the drawer state rather than navigate away. Functionally acceptable for a static page, but worth noting.

### 3.6 Filter chip row — border-b color (MINOR)
- Filter chips row uses `border-black/[0.06]`. The design system `--border` token is `oklch(0.901 0 0)` which is approximately `#e6e6e6` — slightly lighter than `rgba(0,0,0,0.06)` (~`#f0f0f0`). Minor discrepancy, should use `border-border`.

### 3.7 Table header row style (MINOR)
- Implementation uses `text-[11px] uppercase tracking-wider` for table headers — reasonable convention, matches hi-fi's small-caps approach for `th` elements.
- Hi-fi does not explicitly uppercase table headers but uses small text weight. This is acceptable.

---

## 4. Token Gaps (shared between both pages)

These are missing tokens in `globals.css` that both pages work around with raw values:

| Missing token | Purpose | Used via workaround |
|---|---|---|
| `--good` | Success/positive color | `emerald-500/600`, `#16a34a` |
| `--warn` | Warning color | `amber-50/700`, `#b45309` |
| `--bad` | Error/danger color | `red-50/700`, `#dc2626` |

These three semantic colors are central to the listings UI (stock levels, status badges, variant health) and should be formally added to `globals.css` as tokens to ensure consistency.

---

## 5. Verdict

| Area | `/seller/listings/new` | `/seller/listings/bulk` |
|---|---|---|
| Structure / layout | Mostly correct, 3-col photo vs 5-col hi-fi | Correct — table + drawer matches hi-fi |
| Topbar copy | Minor deviation (checkmark in button) | Matches hi-fi |
| Content completeness | Missing Status, Shipping, Compare-at, Inventory sections | All hi-fi sections present |
| Color tokens | Many raw hex values; should use CSS vars | Many raw hex values; same issue |
| Border radius (cards) | `rounded-xl` (12px) vs token 18px | Cards not present; drawer uses `rounded-lg` |
| Primary CTA radius | N/A | "Apply to 3 items" uses `rounded-lg` not `rounded-full` |
| Product/SKU data | Acceptable (new listing state) | SKU prefix `MC-` vs hi-fi `MS-` |
| Semantic color tokens | Uses emerald utilities (gap in tokens) | Uses red/amber utilities (gap in tokens) |

**Priority fixes:**
1. Add `--good`, `--warn`, `--bad` tokens to `globals.css` and replace all Tailwind color utilities referencing those states.
2. Replace all inline `#1d1d1f`, `#f5f5f7`, `border-black/[N]` with `text-foreground`, `bg-canvas-parchment`, `border-border`.
3. Restore missing right-rail sections on `/new`: Status toggle, Compare-at, Inventory (pre-orders), Shipping.
4. Fix "Apply to 3 items" button on `/bulk` drawer from `rounded-lg` → `rounded-full` (design system primary CTA pill).
5. Align card border radius on `/new` to 18px (`rounded-[18px]`).
