# Design Audit — Worker 9: Marketing & Promos Pages

**Pages audited:**
- `/seller/marketing` → `web/src/app/seller/marketing/page.tsx` + sub-components
- `/seller/promos` → `web/src/app/seller/promos/page.tsx` + sub-components

**Hi-fi reference:** `hifi-seller-mgmt.jsx` → `SMgmt_Marketing` (line 548) and `SMgmt_Promos` (line 377)

---

## 1. Token Conformance

### Confirmed design tokens in globals.css
- `--canvas-parchment` (oklch ≈ #f5f5f7) ✓
- `--surface-pearl` ✓
- `--color-primary` (oklch ≈ #0066cc) ✓
- `--ink-muted-80` ✓
- `--divider-soft` ✓
- `--radius-xl` (24px), `--radius-lg` (18px), `--radius-sm` (8px), `--radius-pill` (9999px) ✓

### Hex literals used in implementation (not tokenised)

| File | Hex | DESIGN.md token | Verdict |
|---|---|---|---|
| `marketing/page.tsx:21` | `#1d1d1f` | `{colors.ink}` | Should use `text-foreground` or CSS var |
| `marketing/page.tsx:33` | `#1d1d1f` (bg) | `{colors.ink}` / `{colors.surface-black}` | Should use token |
| `marketing/page.tsx:57` | `#f5f5f7` (bg inline style) | `{colors.canvas-parchment}` | **Token exists** (`var(--canvas-parchment)`); use it |
| `email-composer-audience.tsx:25` | `rgba(0,102,204,0.04)` | tint of `{colors.primary}` | Acceptable tint; no direct token but consistent with hi-fi |
| `email-preview.tsx:137` | `#f5f5f7` (bg inline style) | `{colors.canvas-parchment}` | **Token exists**; use it |
| `email-preview.tsx:143` | `#0066cc` | `{colors.primary}` | Should use `text-primary` Tailwind utility |
| `promos-table.tsx:104` | `#f5f5f7` (bg inline style) | `{colors.canvas-parchment}` | **Token exists**; use it |
| `new-promo-drawer.tsx:136` | `#f5f5f7` (bg) | `{colors.canvas-parchment}` | **Token exists**; use it |
| `new-promo-drawer.tsx:249` | `#f5f5f7` (bg) | `{colors.canvas-parchment}` | **Token exists**; use it |
| `promos-stat-card.tsx:16` | `#0066cc` (text color) | `{colors.primary}` | Use `text-primary` |

**Summary:** No phantom hex values — every hex is either `#1d1d1f` (ink), `#f5f5f7` (canvas-parchment), or `#0066cc` (primary), all of which have defined CSS custom properties / Tailwind token equivalents. The issue is inconsistent usage of raw hex vs. token utilities.

---

## 2. Copy Fidelity

### /seller/marketing (SMgmt_Marketing reference)

| Element | Hi-fi text | Implementation | Status |
|---|---|---|---|
| Topbar title | "Email recent buyers" | "Email recent buyers" | ✓ |
| Topbar subtitle | "Marketing · drafted Tuesday" | "Marketing · drafted Tuesday" | ✓ |
| Actions | "Save draft" / "Send test" / "Schedule send" | "Save draft" / "Send test" / "Schedule send" | ✓ |
| Step 1 eyebrow | "Step 1 of 3 · Audience" | "Step 1 of 3 · Audience" | ✓ |
| Step 2 eyebrow | "Step 2 of 3 · Content" | "Step 2 of 3 · Content" | ✓ |
| Step 3 eyebrow | "Step 3 of 3 · Schedule" | "Step 3 of 3 · Schedule" | ✓ |
| Audience card heading | "Recipients" | "Recipients" | ✓ |
| Deliverable line | "184 buyers · 96% deliverable" | driven by `draft.deliverable` data | ✓ |
| Subject line in preview | "The persimmon vase is back · just 8 this batch" | driven by `draft.subject` | ✓ |
| Preview preview text | "A small restock — three glaze variations this round." | driven by `draft.previewText` | ✓ |
| Email sender name | "Mira Studio" | driven by `draft.senderName` | Note: "Mira Studio" is acceptable here — it is the shop name shown inside the email mock, not the brand name. Not a copy bug. |
| Email footer link | "Unsubscribe" | "Unsubscribe" | ✓ |
| Preview panel heading | "Preview" | "Preview" | ✓ |

### /seller/promos (SMgmt_Promos reference)

| Element | Hi-fi text | Implementation | Status |
|---|---|---|---|
| Topbar title | "Discounts & promotions" | "Discounts & promotions" | ✓ |
| Topbar subtitle | "3 active · $2,740 driven · 281 redemptions" | "3 active · $2,740 driven · 281 redemptions" | ✓ |
| Stat card 1 | "Driven revenue" | driven by data | ✓ |
| Stat card 2 | "Redemptions" | driven by data | ✓ |
| Stat card 3 | "Avg. discount" | driven by data | ✓ |
| Stat card 4 | "New buyers" | driven by data | ✓ |
| Tab labels | "Promotions" / "Automatic" / "Gift cards" | driven by data | ✓ |
| Table columns | Code / What it does / Redemptions / Driven revenue / Window / Status | matching | ✓ |
| Drawer heading | "New promotion" | "New promotion" | ✓ |
| Drawer section "Code" label | "Code" | "Code" | ✓ |
| Drawer label "Discount" | "Discount" | "Discount" | ✓ |
| Discount type pills | "% off" / "$ off" / "Free shipping" / "BOGO" | matching | ✓ |
| "Who can use it" section | "Who can use it" | "Who can use it" | ✓ |
| "Limits" section | "Limits" | "Limits" | ✓ |
| Forecast callout heading | "Forecast" | "Forecast" | ✓ |
| Footer buttons | "Save draft" / "Activate · Tue 12:00 AM" | "Save draft" / "Activate · Tue 12:00 AM" | ✓ |

**Copy fidelity: excellent.** All significant strings match the hi-fi verbatim.

---

## 3. Layout & Spacing Delta

### /seller/marketing

| Element | Hi-fi spec | Implementation | Delta |
|---|---|---|---|
| Composer left panel padding | `24px 28px` | `style={{ padding: "24px 28px" }}` | ✓ Exact |
| Preview right panel width | 480px | `width: 480` | ✓ Exact |
| Preview right panel bg | `var(--paper-2)` ≈ canvas-parchment | `background: "#f5f5f7"` (inline hex) | Correct value, wrong authoring (use token) |
| Preview right panel padding | `24px 28px` | `padding: "24px 28px"` | ✓ Exact |
| Topbar layout | title + subtitle + actions | SellerTopbar component | ✓ Matches |
| Left panel border-right | `1px solid var(--line)` | `border-r border-black/[0.06]` | ✓ Equivalent |

**Note:** The hi-fi shows `SellerSidebar active="Customers"` on the left of the page shell, but the page implementation does not show a sidebar — the layout delegates sidebar rendering to the parent shell (seller layout). This is consistent with the rest of the seller area and is not a bug.

### /seller/promos

| Element | Hi-fi spec | Implementation | Delta |
|---|---|---|---|
| Stat row padding | `20px 28px 0` | `style={{ padding: "20px 28px 0", marginRight: 460 }}` | ✓ Exact; `marginRight: 460` correctly reserves drawer space |
| Stat row grid | `repeat(4, 1fr)` with gap 12px | `grid-cols-4 gap-3` (12px gap) | ✓ Exact |
| Tabs container | `padding: 20px 28px 0` | `marginRight: 460` only (tabs rendered by PromosTabs with `px-7 pt-5`) | Minor: tabs use `px-7` (28px) and `pt-5` (20px) — matches; OK |
| Table card top radius removal | `borderTopLeftRadius: 0, borderTopRightRadius: 0` | `borderTopLeftRadius: 0, borderTopRightRadius: 0` | ✓ Exact |
| Table card margin-top | `-1` (overlaps tab border) | `marginTop: -1` | ✓ Exact |
| Drawer width | 460px | `width: 460` | ✓ Exact |
| Drawer padding body | 22px | `padding: 22` | ✓ Exact |
| Drawer header padding | `16px 22px` | `padding: "16px 22px"` | ✓ Exact |
| Drawer footer padding | `14px 22px` | `padding: "14px 22px"` | ✓ Exact |
| Promo stat card padding | 16px | `p-4` (16px) | ✓ Exact |
| Drawer forecast callout bg | `var(--paper-2)` | `background: "#f5f5f7"` | Correct value, raw hex (use token) |

---

## 4. Component & Interaction Gaps

### /seller/marketing

1. **Email CTA button uses wrong shape** — `email-preview.tsx:122–128`: the "Shop the restock →" CTA inside the email mock uses `rounded-lg` (8px). The hi-fi uses `hf-btn hf-btn-primary` which maps to `{rounded.pill}` per DESIGN.md. A pill shape (`rounded-full` or `rounded-pill`) is expected for the primary action inside the email, as the system-wide CTA grammar.

2. **Topbar action buttons use `rounded-lg` not `rounded-xl`** — `marketing/page.tsx:20,26,33`: all three topbar buttons use `rounded-lg` (8px). The `promos/page.tsx` uses `rounded-xl` (24px) for its topbar button. The hi-fi uses `hf-btn-outline hf-btn-sm` / `hf-btn-primary hf-btn-sm` which rely on the shared button class — these should be consistent across pages.

3. **"Send test" button gap between icon and label** — `marketing/page.tsx:26`: uses `gap-1.5` which is correct and matches the hi-fi pattern. ✓

4. **No dark-mode toggle functional state** — `email-preview.tsx:12–62`: light/dark mode toggle is rendered but purely decorative (no state change). This matches the hi-fi which shows static artboards only; acceptable for a static reference render.

5. **`Switch` component duplicated** — `email-composer-audience.tsx:48–68` and `email-composer-schedule.tsx:3–23` both define an identical local `Switch` component. Not a visual defect but a code quality issue (not in scope for design audit).

### /seller/promos

6. **Forecast dot uses `bg-emerald-500`** — `new-promo-drawer.tsx:251` and `promos-table.tsx:142,148`: the active status indicator and forecast dot use Tailwind's semantic `bg-emerald-500` / `text-emerald-700` / `bg-emerald-50`. The hi-fi uses `var(--good)` for the green "Active" indicator dot. There is no `--good` token in `globals.css`. The implementation's emerald color is visually consistent with the hi-fi intent, but it introduces a color not defined in the design system tokens. **Flag:** if `--good` is added as a token, these should be updated.

7. **Drawer "Save draft" button radius** — `new-promo-drawer.tsx:275`: `rounded-xl` (24px). The hi-fi uses `hf-btn hf-btn-ghost` which is rounded-pill in the design system. The implementation uses `rounded-xl` which does not match the pill shape. Low severity — the overall button reads correctly.

8. **Limits grid cards use `rounded-xl border border-black/[0.06]`** — `new-promo-drawer.tsx:231`. The hi-fi uses `hf-card` style for these mini cards. The `rounded-xl` (24px) is larger than the `{rounded.lg}` (18px) that `store-utility-card` specifies, but these are drawer mini-cards, not store cards — `rounded-xl` is visually consistent and acceptable.

9. **Table `Redemptions` and `Driven revenue` columns** — `promos-table.tsx:75,79`: both use `text-right` in `<th>` but `td` cells also carry `text-right tabular-nums`. ✓ Consistent with hi-fi numeric right-alignment.

10. **Topbar "New promotion" button** — `promos/page.tsx:20–37`: uses `rounded-xl` (24px) with `font-semibold`. The hi-fi uses `hf-btn hf-btn-primary` which is `{rounded.pill}` per DESIGN.md. The implementation does not use pill shape for this primary CTA. **This is a clear deviation from the DESIGN.md specification** that primary CTAs use `{rounded.pill}`.

---

## 5. Summary

### Findings by severity

| # | Severity | Page | Location | Finding |
|---|---|---|---|---|
| F1 | Medium | marketing | `email-preview.tsx:122` | Email body CTA uses `rounded-lg` (8px) — should be `rounded-pill` per DESIGN.md primary button spec |
| F2 | Medium | promos | `promos/page.tsx:21` | "New promotion" primary CTA uses `rounded-xl` — should be `rounded-pill` per DESIGN.md |
| F3 | Low | marketing | `marketing/page.tsx:20,26,33` | Topbar action buttons use `rounded-lg`; promos page uses `rounded-xl`; inconsistency across sibling pages |
| F4 | Low | marketing | `marketing/page.tsx:57` | Preview panel background uses raw `"#f5f5f7"` inline style; should use `var(--canvas-parchment)` CSS token |
| F5 | Low | marketing | `email-preview.tsx:137` | Email footer background uses raw `"#f5f5f7"`; should use `var(--canvas-parchment)` |
| F6 | Low | marketing | `email-preview.tsx:143` | Unsubscribe link uses raw `"#0066cc"`; should use `text-primary` Tailwind utility |
| F7 | Low | promos | `new-promo-drawer.tsx:136,249` | Two instances of raw `"#f5f5f7"` background; should use `var(--canvas-parchment)` token |
| F8 | Low | promos | `promos-table.tsx:104` | Code badge background uses raw `"#f5f5f7"`; should use `bg-canvas-parchment` |
| F9 | Low | promos | `promo-stat-card.tsx:16` | Sparkline color uses raw `"#0066cc"`; should use `text-primary` / CSS var |
| F10 | Info | promos | `new-promo-drawer.tsx:251`, `promos-table.tsx:142` | Emerald green used for "Active" status has no defined token (`--good` absent from globals.css); visually correct but outside token system |

### Pass / Fail tallies

| Route | Copy | Layout/Spacing | Token usage | Shape/Radius | Overall |
|---|---|---|---|---|---|
| /seller/marketing | PASS | PASS | FAIL (F4, F5, F6) | FAIL (F1, F3) | **FAIL** |
| /seller/promos | PASS | PASS | FAIL (F7, F8, F9) | FAIL (F2) | **FAIL** |

**Both pages pass on copy fidelity and layout/spacing. Both fail on token usage (raw hex literals where CSS variables exist) and shape/radius conformance (primary CTAs missing `rounded-pill`).**

The most actionable fixes are F1 and F2 (pill shape on primary CTAs) and F4–F9 (replace raw `#f5f5f7` / `#0066cc` hex with token references).
