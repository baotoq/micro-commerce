# Design Audit — Worker 10
**Pages:** `/seller/states/loading`, `/seller/states/empty-orders`, `/seller/states/first-sale`, `/seller/states/payout-error`
**Hi-fi refs:** `hifi-flow-seller.jsx` — `SState_Loading`, `SState_EmptyOrders`, `SState_PayoutError`, `SState_FirstSaleSuccess`
**Date:** 2026-05-10

---

## 1. Summary

| Page | Status | Critical Issues |
|------|--------|----------------|
| `/seller/states/loading` | PASS (minor) | Padding token deviation; sidebar rendered by layout (correct) |
| `/seller/states/empty-orders` | FAIL | Missing primary CTA button; inline hex literals; hardcoded color not token |
| `/seller/states/first-sale` | FAIL | Action buttons use wrong radius (`rounded-xl` vs `rounded-pill`/`hf-btn-primary`); "Pack & ship" button not `bg-primary` |
| `/seller/states/payout-error` | FAIL | `var(--bad)` is not a defined CSS token in globals.css; "Update bank →" link uses mixed radius expression |

---

## 2. Per-Page Findings

### 2.1 `/seller/states/loading`

**Hi-fi reference:** `SState_Loading`

**Matches:**
- Skeleton shimmer animation (`hf-shimmer 1.4s linear infinite`) identical to hi-fi spec.
- Skeleton gradient colors (`rgba(21,18,14,0.06)` → `rgba(21,18,14,0.12)`) match hi-fi exactly.
- KPI grid: `repeat(4, 1fr)` with `gap-3` — matches hi-fi.
- Lower grid: `1.6fr 1fr` split — matches hi-fi.
- Top-bar structure (label+title left, two pill skeletons right) — matches hi-fi.
- `aria-busy="true"` and `aria-live="polite"` are correct accessibility additions (hi-fi has no annotation; implementation adds correctly).
- Sidebar is rendered by `seller/layout.tsx` (not inline) — correct; hi-fi shows `SellerSidebar` as part of the frame shell.

**Deviations:**

| # | Location | Hi-fi | Implementation | Severity |
|---|----------|-------|----------------|----------|
| L-1 | Top-bar padding | `padding: '20px 28px'` (hi-fi) | `px-7 py-5` = 28px × 20px — matches | OK |
| L-2 | Main content padding | `padding: '24px 28px'` (hi-fi) | `px-7 py-6` = 28px × 24px — matches | OK |
| L-3 | KPI card padding | `padding: 18` (hi-fi) | `p-[18px]` — matches | OK |
| L-4 | KPI card border radius | hi-fi uses `hf-card` (typically `rounded-xl` / 12px) | `rounded-xl` — close match | OK |
| L-5 | Activity dot skeleton | `Sk w={10} h={10} r={999}` (hi-fi) | `Sk w={10} h={10} r={999}` — exact match | OK |
| L-6 | `mt-1` on activity rows | hi-fi: `marginTop: 4` (4px) | `mt-1` = 4px — matches | OK |

**Result: PASS** — no material deviations found.

---

### 2.2 `/seller/states/empty-orders`

**Hi-fi reference:** `SState_EmptyOrders`

**Matches:**
- Two-pane layout: left pane `w-[340px]` with border-right — hi-fi: `width: 340` with `borderRight`. Exact match.
- Left pane empty state: Inbox icon, "No orders yet" heading, subtitle copy — verbatim match.
- Right pane heading: "Quiet, isn't it." — verbatim match.
- Right pane body copy — verbatim match.
- Helper cards text ("Add 2 more listings", "Shops with 5+ items get found 3× more.", "Share your link", "A short note to friends does most of the lifting.") — verbatim match.
- Filter chip labels: `['All · 0', 'New', 'Pack', 'Ship', 'Done']` — verbatim match.
- Active chip style: `bg-[#1d1d1f]` pill — matches hi-fi `hf-chip-on` (dark background).

**Deviations:**

| # | Location | Hi-fi | Implementation | Severity |
|---|----------|-------|----------------|----------|
| E-1 | Right pane CTA | hi-fi has `<button className="hf-btn hf-btn-primary">Copy mira-studio.micro.shop</button>` as a primary blue pill button below the helper cards | Implementation has `<CopyShopLink domain={DOMAIN} />` — no primary pill CTA visible in isolation; the component may render it, but domain is `alex-studio.micro.shop` while hi-fi shows `mira-studio.micro.shop` (brand copy differs per BRAND.owner, which is "Alex" per CLAUDE.md — this is correct for the implementation, not a bug) | LOW |
| E-2 | Filter button radius | hi-fi: `hf-btn hf-btn-outline hf-btn-sm` (pill or sm rounded) | Implementation: `rounded-lg` = 8px (`{rounded.sm}`) — hi-fi outline button should be pill-shaped per design system | MEDIUM |
| E-3 | Active chip background | `bg-[#1d1d1f]` — inline hex literal | Should use `bg-foreground` or Tailwind `bg-[var(--foreground)]` to reference token | LOW |
| E-4 | Inactive chip text color | `text-[#1d1d1f]/60` — inline hex | Should use `text-foreground/60` | LOW |
| E-5 | Helper card hover background | `hover:bg-[#ebebed]` — hardcoded hex with no token | `#ebebed` is not defined in globals.css; should use a defined token or opacity variant | LOW |
| E-6 | Icon circle background | `bg-[#f5f5f7]` — hardcoded hex | This is `{colors.canvas-parchment}` = oklch(0.965 0.001 286); should use `bg-canvas-parchment` | LOW |
| E-7 | Left pane icon/text color | `text-[#1d1d1f]/50` — inline hex | Should use `text-foreground/50` | LOW |
| E-8 | "No orders yet" heading | hi-fi: `hf-h4` style (14–15px) | Implementation: `text-sm font-semibold` (14px) — matches | OK |
| E-9 | Topbar structure | hi-fi uses `SellerTopbar` component for consistency (visible in `SState_EmptyOrders` topbar is the filter row, not a standard topbar) | Implementation has a local filter/topbar row without using `SellerTopbar` — this matches the hi-fi spec (empty-orders doesn't use the standard topbar, it uses a custom filter bar) | OK |

**Result: FAIL** — E-2 (Filter button uses wrong radius `rounded-lg` instead of pill/outline-pill), E-5 (undeclared hover token). Multiple inline hex literals (E-3, E-4, E-6, E-7) are non-breaking but violate token discipline.

---

### 2.3 `/seller/states/first-sale`

**Hi-fi reference:** `SState_FirstSaleSuccess`

**Matches:**
- Overlay background: `rgba(21,18,14,0.5)` — exact match with hi-fi.
- Modal width: `width: 480` — exact match.
- Hero band background: `#0066cc` — this IS `{colors.primary}` (#0066cc per DESIGN.md); matches hi-fi `var(--primary)`.
- Decorative circles: right `-20/top -30` (140×140, 8% white), left `-10/bottom -40` (90×90, 6% white) — exact match.
- Star glyph: `★` at 44px — exact match.
- "Your first sale" label: letterSpacing, uppercase, opacity 0.85 — exact match.
- "It happened." heading: font-size 36, lineHeight 1 — exact match.
- Order summary: "Persimmon vase", "Sasha L. · San Francisco, CA", `money(86)` — verbatim match.
- Body copy: "You'll receive $82.56 after Micro's 4% fee. Pack & ship in the next 3 days and the rating will follow." — verbatim match.
- Blurred dashboard behind overlay — matches hi-fi `filter: 'blur(2px)'`.
- `SellerTopbar` with `Welcome, ${BRAND.owner}` and `Day 4 · Friday, March 15` — matches hi-fi exactly (hi-fi shows "Welcome, Mira"; implementation uses BRAND.owner = "Alex" per CLAUDE.md, correct).

**Deviations:**

| # | Location | Hi-fi | Implementation | Severity |
|---|----------|-------|----------------|----------|
| F-1 | "Send a thank-you note" button | hi-fi: `hf-btn hf-btn-outline` — a pill-shaped outline button | Implementation: `rounded-xl` (12px) border button — should be `rounded-pill` per design system | HIGH |
| F-2 | "Pack & ship →" button | hi-fi: `hf-btn hf-btn-primary` — Action Blue pill button | Implementation: `rounded-xl bg-[#1d1d1f]` (near-black, not Action Blue) — wrong color AND wrong radius | HIGH |
| F-3 | Modal card radius | hi-fi: `hf-card` (≈ `rounded-xl`) | Implementation: `rounded-2xl` (32px) — hi-fi cards use ~12px; `rounded-2xl` is too aggressive | MEDIUM |
| F-4 | Order summary card radius | hi-fi: `borderRadius: 10` | Implementation: `rounded-[10px]` — exact match | OK |
| F-5 | Product image placeholder | hi-fi: `ProdImg tone="clay"` (clay-tone, 56×56, r=8) | Implementation: `background: "rgba(194,65,12,0.18)"` 56×56 rounded-lg — close approximation, matches intent | OK |
| F-6 | Action button margin-top | hi-fi: `marginTop: 20` | Implementation: `mt-5` = 20px — matches | OK |
| F-7 | Hero band padding | hi-fi: `padding: '28px 28px 22px'` | Implementation: inline style `padding: "28px 28px 22px"` — exact match | OK |
| F-8 | Body padding | hi-fi: `padding: '24px 32px 28px'` | Implementation: inline style `padding: "24px 32px 28px"` — exact match | OK |

**Result: FAIL** — F-1 and F-2 are HIGH severity: "Pack & ship" primary action uses near-black (`#1d1d1f`) instead of Action Blue (`{colors.primary}`) and both action buttons use `rounded-xl` instead of `rounded-pill`. This breaks the design system's button grammar for primary CTAs.

---

### 2.4 `/seller/states/payout-error`

**Hi-fi reference:** `SState_PayoutError`

**Matches:**
- Error banner background: `#FBE9E5`, border `#E8B5AB` — exact match with hi-fi.
- Banner padding: `14px 28px` — exact match.
- Alert icon circle: 28×28, `border-radius: 999` — exact match.
- Banner heading: "We couldn't send your Tuesday payout · $1,284.62 held" — verbatim match.
- Banner body copy: "Your bank rejected the transfer (account ending 4421)..." — verbatim match.
- Held payout card: padding 24, marginBottom 20 — matches hi-fi.
- "● Payout held" label: uppercase, letterSpacing, color `var(--bad)` — structural match (see token issue below).
- Amount: `money(1284.62)` at fontSize 36 — matches.
- "9 orders · weekly batch · would have arrived Wed" — verbatim match.
- Divider: `margin: '18px 0'` — matches hi-fi `margin: '18px 0'`.
- "What's happening" section with `<code>` for error code — matches hi-fi.
- KPI grid: `repeat(3, 1fr)` at `opacity: 0.4` — exact match.
- KPI labels and values — verbatim match.
- `SellerTopbar` with "Good morning, Alex" (hi-fi: "Good morning, Mira" — correct per BRAND.owner).

**Deviations:**

| # | Location | Hi-fi | Implementation | Severity |
|---|----------|-------|----------------|----------|
| P-1 | `var(--bad)` token | hi-fi uses `var(--bad)` as a CSS variable | `var(--bad)` is NOT defined in `globals.css` — implementation uses `var(--bad, #c0392b)` with a fallback, but the token itself is absent from the design system | HIGH |
| P-2 | Alert icon color | hi-fi: `background: 'var(--bad)'` | Implementation: `background: "var(--bad, #c0392b)"` — relies on fallback only; if token ever resolves to something else the color would break | MEDIUM |
| P-3 | Held payout card border color | hi-fi: `borderColor: 'var(--bad)'` | Implementation: `borderColor: "var(--bad, #c0392b)"` — same undefined token issue | MEDIUM |
| P-4 | "Update bank →" link radius | hi-fi: `hf-btn hf-btn-primary hf-btn-sm` — pill button | Implementation: `rounded-[min(var(--radius-md),12px)]` — `--radius-md` = 11px, so this renders as 11px (`rounded-md`), not pill | HIGH |
| P-5 | "Update bank →" link height | hi-fi: small pill button | Implementation: `h-7` (28px) with `px-2.5` — functionally acceptable height for sm button | LOW |
| P-6 | "Retry payout" button | hi-fi: `hf-btn hf-btn-primary` (pill, Action Blue) | Implementation: `<Button>Retry payout</Button>` — uses shadcn Button default; default primary uses `bg-primary` which is correct; radius from shadcn = `var(--radius)` = 0.5rem (8px) not pill | MEDIUM |
| P-7 | "Switch payout method" | hi-fi: `hf-btn hf-btn-outline` | Implementation: `<Button variant="outline">` — acceptable, radius same issue as P-6 | LOW |
| P-8 | Sparkline color | hi-fi: `color="var(--ink-3)"` | Implementation: `className="h-8 w-full text-[#1d1d1f]/40"` — ink/40 opacity approximates ink-3 | LOW |
| P-9 | Content padding | hi-fi: `padding: '24px 28px'` | Implementation: `style={{ padding: "24px 28px" }}` — exact match | OK |

**Result: FAIL** — P-1 is HIGH: `--bad` is not a defined CSS token in globals.css. The color exists only as a hardcoded fallback `#c0392b`. P-4 is HIGH: the "Update bank →" primary CTA uses `rounded-[min(var(--radius-md),12px)]` (11px) instead of the pill shape that the design system mandates for primary CTAs.

---

## 3. Token Violations

| Token Issue | Pages Affected | Details |
|------------|----------------|---------|
| `var(--bad)` undefined | `payout-error` | `#c0392b` used as inline fallback; token not in globals.css |
| `bg-[#1d1d1f]` inline hex | `first-sale`, `empty-orders` | Should use `bg-foreground` (token = `oklch(0.215 0.004 286)` ≈ `#1d1d1f`) |
| `bg-[#f5f5f7]` inline hex | `empty-orders`, `first-sale` | Should use `bg-canvas-parchment` |
| `text-[#1d1d1f]/60` inline hex | `empty-orders` | Should use `text-foreground/60` |
| `bg-[#ebebed]` inline hex | `empty-orders` | No matching token; ad-hoc hover shade |
| `#0066cc` in style prop | `first-sale` | This IS `{colors.primary}` (confirmed in DESIGN.md); acceptable as inline since it's the correct value, but `bg-primary` would be preferable |

---

## 4. Copy / Content Deviations

| Page | Hi-fi | Implementation | Verdict |
|------|-------|----------------|---------|
| `empty-orders` CTA button | "Copy mira-studio.micro.shop" | `<CopyShopLink domain="alex-studio.micro.shop" />` | Correct — BRAND.owner is "Alex"; domain reflects owner |
| `first-sale` topbar | "Welcome, Mira" | "Welcome, Alex" | Correct per BRAND constant |
| `payout-error` topbar | "Good morning, Mira" | "Good morning, Alex" | Correct per BRAND constant |
| `first-sale` "Your first sale" | uppercase, letter-spacing 0.1 | `letterSpacing: 0.1` — matches | OK |
| All pages | Hi-fi glyph ★ | ★ preserved in implementation | OK |

No copy bugs found. All "Mira" → "Alex" substitutions are intentional per `BRAND.owner`.

---

## 5. Severity Summary

| Severity | Count | Items |
|----------|-------|-------|
| HIGH | 4 | F-2 (first-sale primary CTA color+radius), F-1 (first-sale secondary CTA radius), P-1 (`--bad` undefined token), P-4 (payout-error Update bank radius) |
| MEDIUM | 4 | E-2 (empty-orders filter button radius), F-3 (first-sale modal card radius), P-2/P-3 (payout-error icon/border relying on undefined token fallback), P-6 (Retry payout radius) |
| LOW | 8 | Various inline hex literals (E-3, E-4, E-5, E-6, E-7, P-5, P-7, P-8) |
| PASS | 1 | `loading` page — no material deviations |

### Priority fixes
1. **`first-sale/page.tsx` lines 154–167**: Change "Pack & ship →" button from `bg-[#1d1d1f] rounded-xl` to `bg-primary rounded-pill` (Action Blue pill). Change "Send a thank-you note" from `rounded-xl` to `rounded-pill` (outline pill).
2. **`globals.css`**: Add `--bad` color token (e.g., `--bad: oklch(0.443 0.177 26)` ≈ `#c0392b`) and expose as `--color-bad` in `@theme inline`.
3. **`payout-error/page.tsx` line 71**: Replace `rounded-[min(var(--radius-md),12px)]` with `rounded-pill` for the "Update bank →" CTA.
4. **`empty-orders/page.tsx` line 34**: Change Filter button from `rounded-lg` to `rounded-pill` per design system outline-button grammar.
