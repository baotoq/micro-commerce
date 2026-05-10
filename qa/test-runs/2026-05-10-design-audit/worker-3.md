# Design Audit — Worker 3
## Pages: /seller/listings · /seller/listings/published
**Date:** 2026-05-10  
**Auditor:** worker-3  
**Hi-fi refs:** hifi-listings.jsx (Listings_Table, Listings_Cards, Listings_Editor), hifi-flow-listings.jsx (LFlow_01_Catalog through LFlow_05_Published)

---

## 1. Summary

| Area | Status |
|---|---|
| Topbar subtitle copy | FAIL |
| Filter chip labels | PASS |
| Table column headers | PASS |
| Table rows — Sales · 30d calculation | DIVERGE |
| Topbar actions — extra "Bulk edit" button | DIVERGE |
| Missing topbar subtitle layout (subtitle above title) | FAIL |
| Published page — activity log items | DIVERGE |
| Published page — "Activity log →" link vs plain text | DIVERGE |
| Hex literals instead of design tokens | FAIL (widespread) |
| Published page — KPI delta glyphs | PASS |
| Status badge casing | DIVERGE |

---

## 2. Per-page findings

### 2a. /seller/listings (ListingsPage)

#### 2a-1. Topbar subtitle — wrong copy format
**Hi-fi (`Listings_Table`):** `subtitle="42 products · 38 active"`  
**Implementation (`page.tsx:53`):** `` `${counts.total} listings · ${published} published` ``

The hi-fi uses the word "products" and "active"; the implementation uses "listings" and "published". Minor wording divergence. Additionally `published` is calculated as `active + low + out` (line 48), which includes low-stock and out-of-stock items in the "published" count — semantically debatable but visually different from the hi-fi label "38 active".

#### 2a-2. Topbar actions — extra "Bulk edit" button not in hi-fi
**Hi-fi (`Listings_Table`, `LFlow_01_Catalog`):** Actions: `Import CSV` + `New listing`  
**Implementation (`page.tsx:63–77`):** Actions: `Import CSV` + `Bulk edit` + `New listing`

The "Bulk edit" button is an addition not present in the hi-fi for this route. The hi-fi does show bulk editing as a separate flow (`LFlow_02_Bulk`), but it is accessed via filter/selection, not a dedicated topbar button. This is a divergence from the hi-fi chrome.

#### 2a-3. Topbar subtitle rendered above title — layout inverted
**Hi-fi (`SellerTopbar` pattern):** Title is the large primary heading; subtitle is the small secondary line _below_ the title (based on hi-fi structure showing `hf-display` title then smaller subtitle).  
**Implementation (`seller-topbar.tsx:18–25`):** The subtitle `<p>` is rendered **before** (above) the `<h1>` in DOM order and visually appears above the title. In the hi-fi, the subtitle ("42 products · 38 active") is smaller text _beneath_ the large "Listings" heading, not above it.

**Verdict:** Visual layout is inverted relative to hi-fi — subtitle above title rather than below.

#### 2a-4. Filter chip format uses " · " separator — matches hi-fi
**Hi-fi:** `'All · 42'`, `'Active · 38'`, etc.  
**Implementation (`filter-chips.tsx:38`):** `{c.label} · {c.count}` — correct separator and format. PASS.

#### 2a-5. Filter chip counts use live data
**Hi-fi:** Static counts (42, 38, 3, 1, 4)  
**Implementation:** Dynamic from `getListingCounts()`. Actual data counts: total=42, active=34, low=3, out=1, draft=4. The active count diverges from the hi-fi's "38 active". This is a data fixture divergence, not a UI bug per se, but the hi-fi shows `Active · 38` while the live data gives `Active · 34`.

#### 2a-6. Table column headers — match hi-fi
**Hi-fi:** `Product | SKU | Status | Stock | Price | Views · 30d | Sales · 30d`  
**Implementation (`listings-table.tsx:87–94`):** Matches exactly. PASS.

#### 2a-7. Sales · 30d — derived, not from data
**Hi-fi:** Shows distinct per-product sales figures (14, 9, 12, 6, 4, 8, 7, 0, 3).  
**Implementation (`listings-table.tsx:99`):** `const sales = Math.max(0, Math.round(l.views7d / 28))`

Sales are computed from views (views ÷ 28), not from a dedicated `sales` field. The `Listing` type in `data.ts` has no `sales` field — only `views7d`. The computed values will differ substantially from the hi-fi figures. For example, "Persimmon vase" has `views7d=412` → computed sales=15 vs hi-fi's 14 (close, but synthetic). This is a structural divergence: the data model doesn't carry sales data.

#### 2a-8. Table thumbnail — color swatch vs ProdImg
**Hi-fi:** `ProdImg` renders a styled ceramic product photo placeholder with tone colors.  
**Implementation (`listings-table.tsx:116–121`):** Renders a plain colored `<span>` block (size-9 ~36px) using `TONE_BY_CATEGORY` mapping (3 category colors). This is a simplified placeholder with no per-product tone — all Vessels get the same `#E5C0A6` swatch regardless of individual product tone. Acceptable simplification but lacks per-product differentiation.

#### 2a-9. No checkbox column interaction
**Hi-fi:** Checkboxes are present for bulk selection in the table.  
**Implementation (`listings-table.tsx:108–111`):** Renders a `<span>` styled as a checkbox (`size-3.5 rounded-[3px] border-[1.5px]`) with `aria-hidden`. Non-interactive — clicking does nothing. This is a static placeholder, consistent with the page being a read-only view component, but the hi-fi implies selection functionality.

#### 2a-10. Pagination footer text — matches hi-fi
**Hi-fi:** "9 of 42 shown"  
**Implementation (`listings-table.tsx:170–172`):** `{visible} of {listings.length} shown` — matches format. PASS.

#### 2a-11. Inline hex literals — design tokens not used
The following raw hex values appear in implementation code that should use CSS tokens:

| Location | Hex used | Correct token |
|---|---|---|
| `filter-chips.tsx:33` | `#1d1d1f` | `--foreground` / `text-foreground` |
| `filter-chips.tsx:34` | `#1d1d1f` | `--foreground` |
| `listings-table.tsx:24` | `#E5C0A6`, `#C9D4BC`, `#F0E8D7` | No defined token — product tone colors; acceptable as component-local |
| `listings-table.tsx:105` | `#1d1d1f` | `--foreground` |
| `listings-table.tsx:123` | `#1d1d1f` | `--foreground` |
| `listings-table.tsx:146` | `#1d1d1f` | `--foreground` |
| `listings-table.tsx:149,152` | `#1d1d1f` | `--foreground` |
| `listings-table.tsx:190` | `#1d1d1f` | `--foreground` |
| `page.tsx:87` | `#1d1d1f` | `--foreground` |
| `seller-topbar.tsx:19` | `#1d1d1f` | `--foreground` |
| `seller-topbar.tsx:23` | `#1d1d1f` | `--foreground` |
| `seller-topbar.tsx:39` | `#e8e3da` | No token defined — avatar background; close to `--canvas-parchment` (`oklch(0.965 0.001 286)` ≈ `#f5f5f7`). Diverges. |

`#1d1d1f` is `{colors.ink}` / `{colors.body}` from DESIGN.md; globals.css maps `--foreground: oklch(0.215 0.004 286)` which resolves to approximately `#1d1d1f`. The inline hex works but bypasses the token system. The product tone colors (`#E5C0A6` etc.) have no token equivalents — these are implementation-local, flagged as informational only.

`#e8e3da` (avatar background, topbar.tsx:39) is not a defined token. Nearest is `--canvas-parchment` (#f5f5f7) — different color entirely.

#### 2a-12. Search input — missing label text alignment
**Hi-fi:** Search placeholder is "Search products…" with a magnifying glass icon.  
**Implementation (`page.tsx:90–93`):** `placeholder="Search products…"` with absolute-positioned search icon. Matches. PASS.

#### 2a-13. Filter bar background
**Hi-fi (`Listings_Table` line 27):** Filter bar has `borderBottom: '1px solid var(--line)'` — no explicit background (inherits).  
**Implementation (`page.tsx:81`):** `bg-white` explicitly set on the filter bar. Acceptable.

---

### 2b. /seller/listings/published (ListingsPublishedPage)

#### 2b-1. Topbar subtitle — PASS
**Hi-fi (`LFlow_05_Published`):** `subtitle="42 products · 39 active"`  
**Implementation (`published/page.tsx:51`):** `subtitle="42 products · 39 active"` — exact match. PASS.

#### 2b-2. Success banner — content match
**Hi-fi:** "Persimmon vase published · 2 variants updated, 1 went live."  
**Implementation (`published/page.tsx:71`):** `"Persimmon vase published · 2 variants updated, 1 went live."` — exact match. PASS.

**Banner background:** Hi-fi uses `background: '#DDEDE1'`; implementation uses `bg-[#DDEDE1]`. Same hex — no token defined for this mint-green success surface. `#DDEDE1` is not in globals.css. Should be a token (e.g., `--surface-success` or similar), but it's a hard-coded color in both hi-fi and implementation.

#### 2b-3. Success banner — checkmark glyph
**Hi-fi:** Uses `<Ico n="check" />` SVG icon inside a circle.  
**Implementation (`published/page.tsx:69`):** Uses `✓` Unicode character inside a `<span>` styled as a circle. Functional equivalent but lower quality than an SVG — the Unicode checkmark may render inconsistently across platforms.

#### 2b-4. KPI cards — values and deltas match hi-fi
**Hi-fi:** `Active listings: 39 ↑ +1`, `Variants in stock: 128 ↑ +2`, `Out-of-stock items: 1 ↑ −2`  
**Implementation `KPIS` array (`published/page.tsx:7–11`):** Identical values and deltas. The "↑" arrow glyph is present. PASS.

**Note:** All three cards show green `↑` arrows even for "Out-of-stock items: 1 ↑ −2" — reducing out-of-stock is positive, but the hi-fi also styles all three with `color: 'var(--good)'` (green), so this matches. PASS.

#### 2b-5. Activity log — item content diverges from hi-fi
**Hi-fi (`LFlow_05_Published`):** Activity rows:
1. `Persimmon vase · Medium` / `Price · $86 → $95` / `You`
2. `Persimmon vase · Small` / `Price · $64 → $70` / `You`
3. `Persimmon vase · Large` / `Status · Out → still out (no stock)` / `You`
4. `Soft hand vessel` / `Price · $92 → $101` / `You · bulk`
5. `Indigo carafe` / `Price · $110 → $121` / `You · bulk`

**Implementation (`published/page.tsx:13–44`):** Rows 1–3 match exactly. Rows 4–5 diverge:
- Row 4: `"Ember tea bowl"` / `"Price · $36 → $39.60"` / `"You · bulk"` — hi-fi has "Soft hand vessel" / "$92 → $101"
- Row 5: `"Peat serving bowl"` / `"Price · $90 → $99"` / `"You · bulk"` — hi-fi has "Indigo carafe" / "$110 → $121"

The product names and prices in rows 4–5 are different. The implementation uses products from `data.ts` (Ember tea bowl MC-SC-008, Peat serving bowl MC-BW-051), which are real listings, while the hi-fi used different products (Soft hand vessel, Indigo carafe) that appear in the hi-fi catalog but with different SKU prefixes (`MS-` vs `MC-`). This is a brand/data consistency swap — the hi-fi's "Soft hand vessel" (MS-VS-019) doesn't exist in `data.ts` under that name. The substitution is intentional but produces copy that diverges from hi-fi.

#### 2b-6. Activity log — "Activity log →" is plain text, not a link
**Hi-fi:** `<a className="hf-small">Activity log →</a>` — styled as a link.  
**Implementation (`published/page.tsx:107–109`):** `<span className="text-[13px] font-medium text-[#1d1d1f]/70">Activity log →</span>` — a non-interactive `<span>`, not an anchor. Not navigable.

#### 2b-7. Activity log — "By" column missing avatar
**Hi-fi (`LFlow_05_Published`):** The "By" column shows `<Avatar name="Mira" size="sm" />` + the by-text.  
**Implementation (`published/page.tsx:130`):** Only renders the text `{r.by}` — no avatar. The avatar is absent.

#### 2b-8. Activity log — "When" column text style
**Hi-fi:** When column uses `hf-tiny hf-muted` (small, muted text).  
**Implementation (`published/page.tsx:123–124`):** `text-[11px] text-[#1d1d1f]/60` — matches intent (small, muted). PASS.

#### 2b-9. Hex literal — success green badge color
**Implementation (`published/page.tsx:67`):** `bg-emerald-600` for the checkmark circle. This is a Tailwind utility, not a defined token. The hi-fi uses `var(--good)`. The globals.css does not define `--good` or a success color — this is a gap in the token system. Using `bg-emerald-600` is pragmatic but bypasses the design system.

#### 2b-10. KPI card text — muted label uses inline opacity
**Implementation (`published/page.tsx:89`):** `text-[#1d1d1f]/50` for the KPI label. No token for `--ink-muted-48` (which is `#7a7a7a` per DESIGN.md). The `[#1d1d1f]/50` approach produces `rgba(29,29,31,0.5)` — close to `#7a7a7a` but not identical. Functionally acceptable but not using the token.

---

## 3. Missing features / structural gaps

1. **Bulk selection** — The listings table renders non-interactive checkbox placeholders. The hi-fi implies full row selection → bulk action bar pattern (LFlow_02_Bulk). The bulk action bar and drawer are implemented at `/seller/listings/bulk` (not audited here) but the index page has no selection state wired.

2. **Card/grid view toggle** — `Listings_Cards` hi-fi shows a 4-column visual grid view with a grid/list toggle in the filter bar. The implementation only has table view; no grid/list toggle exists on the index page.

3. **Collection / Best-selling filter buttons** — Present in implementation (page.tsx:96–111) and hi-fi, but non-functional (no dropdown, no filter logic).

4. **Listings_Cards subtitle** — Hi-fi `Listings_Cards` uses `subtitle="The shop · arranged by you"` for the card view. Implementation's table view uses the product count subtitle correctly.

5. **`published` page — no "View shop →" navigation** — The button exists but has no `href`. `Button variant="ghost"` without an `href` is non-navigable.

---

## 4. Token / color violations

All confirmed hex literals that should use defined CSS tokens:

| File | Line | Value | Issue |
|---|---|---|---|
| `filter-chips.tsx` | 33 | `#1d1d1f` | Use `text-foreground` |
| `filter-chips.tsx` | 34 | `#1d1d1f` | Use `text-foreground` |
| `listings-table.tsx` | 105 | `#1d1d1f` | Use `text-foreground` |
| `listings-table.tsx` | 123, 146, 149, 152, 190 | `#1d1d1f` | Use `text-foreground` |
| `seller-topbar.tsx` | 19, 23 | `#1d1d1f` | Use `text-foreground` |
| `seller-topbar.tsx` | 39 | `#e8e3da` | No token defined; nearest `--canvas-parchment` is `#f5f5f7` — different |
| `published/page.tsx` | 62 | `#DDEDE1` | No token defined for success surface |
| `published/page.tsx` | 89 | `#1d1d1f` | Use `text-foreground` |
| `published/page.tsx` | 107 | `#1d1d1f` | Use `text-foreground` |
| `page.tsx` | 87 | `#1d1d1f` | Use `text-foreground` |

Not flagged (confirmed no token exists): `#E5C0A6`, `#C9D4BC`, `#F0E8D7` (product tone swatches — no design token; implementation-local).

---

## 5. Prioritized fix list

| Priority | File | Finding |
|---|---|---|
| P1 | `seller-topbar.tsx` | Subtitle renders **above** title in DOM; hi-fi shows title above, subtitle below — invert order |
| P1 | `page.tsx` | Topbar subtitle copy: change `"listings"` → `"products"` and `"published"` → `"active"` (hi-fi: "42 products · 38 active") |
| P1 | `published/page.tsx` | "Activity log →" is a `<span>`, not a link — make it an `<a>` or `<Link>` |
| P1 | `published/page.tsx` | Checkmark uses `✓` Unicode — replace with SVG icon for rendering consistency |
| P2 | `page.tsx` | Remove the "Bulk edit" topbar action button — not in hi-fi for this route |
| P2 | `published/page.tsx` | Add avatar to "By" column in the activity table (hi-fi shows user avatar next to "You") |
| P2 | `seller-topbar.tsx:39` | `#e8e3da` avatar background has no token and doesn't match any defined token — define `--surface-avatar` or use `--canvas-parchment` |
| P2 | `published/page.tsx` | `bg-emerald-600` for success circle — wire to a `--good` / `--surface-success` token |
| P3 | `filter-chips.tsx`, `listings-table.tsx`, `seller-topbar.tsx`, `published/page.tsx` | Replace all `#1d1d1f` inline hex with `text-foreground` Tailwind utility |
| P3 | `listings-table.tsx` | Status badge `capitalize` CSS lowercases "active/low/out/draft" — hi-fi shows capitalized "Active", "Low", "Out", "Draft". Either store capitalized or uppercase first char in render. |
| P3 | Published page | Define a `--surface-success: #DDEDE1` token in globals.css and use it instead of inline hex |
