# Design Audit — worker-1
Date: 2026-05-10
Auditor: worker-1

---

## 1. Coverage

| Route | Source file | Hi-fi reference |
|---|---|---|
| `/` | `web/src/app/page.tsx` | `design/project/hifi-home.jsx` (Home_Tiles, Home_Editorial, Home_Rails) |
| `/seller/apply` | `web/src/app/(seller-marketing)/seller/apply/page.tsx` | `design/project/hifi-flow-seller.jsx` → `SFlow_01_Apply` |
| `/seller/onboard` | `web/src/app/(seller-marketing)/seller/onboard/page.tsx` | `design/project/hifi-flow-seller.jsx` → `SFlow_02_Setup` |

Supporting files read: `DESIGN.md`, `web/src/app/globals.css`, `web/src/components/seller/marketing-top.tsx`, `design/project/hifi-onboarding.jsx`.

Note: `web/src/components/primitives.tsx` was not found at the specified path; no such file exists in the repo, so token cross-reference was done via `globals.css` directly.

---

## 2. Findings Table

### Route: `/` (home page)

| # | Element | Implemented | Hi-fi / DESIGN.md spec | Severity |
|---|---|---|---|---|
| H-1 | Page content | Single `<p>` placeholder: "Ready for revamp." | Hi-fi shows three full home layouts (tiles, editorial, rails) with full-bleed hero, product grid, shopper topbar, filter bar, product rails | **Critical** — page is entirely unimplemented |
| H-2 | Shopper topbar | Absent | ShopperTopbar with logo, nav links (Shop/Collections/Journal/About), search pill, user icon, bag icon with cart count | **Critical** |
| H-3 | Hero tile | Absent | Full-bleed photography hero (200px strip), gradient overlay, headline + CTA buttons | **Critical** |
| H-4 | Product grid | Absent | 4-up grid with product image tiles, title, price, category label | **Critical** |
| H-5 | Typography | `text-sm text-muted-foreground` (14px, muted) | N/A — placeholder only; real spec requires `{typography.hero-display}` 56px/600 headline | **Critical** |

**Summary for `/`:** Page is a stub placeholder. Zero design coverage against hi-fi.

---

### Route: `/seller/apply`

| # | Element | Implemented | Hi-fi / DESIGN.md spec | Severity |
|---|---|---|---|---|
| A-1 | Eyebrow color | `style={{ color: "#0066cc" }}` — raw hex | `{colors.primary}` (#0066cc) is defined as CSS `--primary: oklch(0.508 0.176 255)` in globals.css; `text-primary` Tailwind class should be used instead of raw hex | Low (correct value, wrong delivery) |
| A-2 | Eyebrow text | `"For makers · 4% per sale, no monthly fee"` | Hi-fi: `"For makers · 4% per sale, no monthly fee"` using `·` middle dot | Pass |
| A-3 | H1 headline | `"Open a shop in about ten minutes."` with `<i>about ten minutes.</i>` | Hi-fi: same text, same italic | Pass |
| A-4 | H1 font size | `style={{ fontSize: 64 }}` — raw px value | Spec: `{typography.hero-display}` = 56px. Implementation is 64px — 8px larger than spec | Medium |
| A-5 | H1 letter-spacing | `letterSpacing: "-0.02em"` | DESIGN.md `{tracking-hero}` = `-0.28px` (absolute, not em). At 64px, `-0.02em` ≈ `-1.28px` — significantly tighter than spec `-0.28px` | Medium |
| A-6 | Body copy | `text-[#1d1d1f]/60` — raw hex with opacity | `{colors.ink}` at 60% opacity; `--foreground` / `text-foreground/60` should be used | Low |
| A-7 | Stat values font size | `style={{ fontSize: 28 }}` — raw px | Hi-fi uses `hf-display-2 hf-num` at 28px — matches. But raw inline style vs. token | Low |
| A-8 | Stat "v" values | `"4 800"`, `"$2.1M"`, `"12 min"` | Hi-fi: `'4 800'`, `'$2.1M'`, `'12 min'` | Pass |
| A-9 | Stat "l" labels | `"active makers"`, `"paid out · April"`, `"avg. setup"` | Hi-fi: `'active makers'`, `'paid out · April'`, `'avg. setup'` — uses middle dot `·` | Pass |
| A-10 | Right panel background | `bg-[#f5f5f7]` — raw hex | `{colors.canvas-parchment}` (#f5f5f7) maps to `bg-canvas-parchment` in Tailwind via `--color-canvas-parchment` | Low (correct value, wrong delivery) |
| A-11 | Claim card border-radius | `rounded-lg` (18px per globals `--radius-lg`) | Hi-fi: `hf-card` uses `var(--r-lg)` which is 18px; spec `{rounded.lg}` = 18px | Pass |
| A-12 | Input border | `border: "1.5px solid #1d1d1f"` — raw hex | Should use `border-foreground` or `border-[color:var(--foreground)]`; raw hex `#1d1d1f` = `{colors.ink}` | Low |
| A-13 | Input border-radius | `rounded-[10px]` — non-token radius | DESIGN.md border radius scale: `{rounded.sm}` = 8px, `{rounded.md}` = 11px. 10px is between tokens — mixed radii grammar violation | Medium |
| A-14 | Availability indicator color | `style={{ background: "#34c759" }}` and `style={{ color: "#34c759" }}` — raw hex | `#34c759` = Apple system green (`var(--good)` in hi-fi). Not a defined DESIGN.md token. Raw hex with no corresponding CSS variable in globals.css | Medium |
| A-15 | Category chips — selected | `background: "#1d1d1f", color: "#fff"` — raw hex | Should use `bg-foreground text-background` | Low |
| A-16 | Category chips — unselected | `background: "#f5f5f7", border: "1px solid #e0e0e0"` — raw hex | `{colors.canvas-parchment}` and `{colors.hairline}` (#e0e0e0); raw hex `#e0e0e0` matches hairline token value but not referenced via CSS var | Low |
| A-17 | CTA button | `rounded-full bg-[#0066cc]` — raw hex | `{colors.primary}` = `--primary` in globals; should be `bg-primary` | Low (correct value, wrong delivery) |
| A-18 | CTA button text | `"Continue · {app.stepsLeft} steps left"` — uses middle dot `·` | Hi-fi: `"Continue · 6 steps left"` with `·` | Pass |
| A-19 | MarketingTop "Sign in" | Rendered as `<a>` styled as ghost button | Hi-fi: `hf-btn hf-btn-ghost hf-btn-sm` = ghost button; implementation is an `<a>` with border styling — functionally present but semantic difference | Low |
| A-20 | MarketingTop "Sell on Micro" CTA | Present; `rounded-full bg-[#0066cc]` | Hi-fi: `hf-btn hf-btn-primary hf-btn-sm` with `bg-[#0066cc]` | Pass (raw hex noted) |
| A-21 | Cursor blink animation | Uses inline `animation: "blink 1s steps(1) infinite"` | Hi-fi uses `animation: 'hf-blink 1s steps(1) infinite'`; globals.css defines `@keyframes hf-shimmer` but no `blink` keyframe — animation likely broken | Medium |
| A-22 | Card `<h2>` font size | `text-xl font-semibold` (20px/600) | Hi-fi uses `hf-h3` at ~17px/600; 20px is slightly large | Low |
| A-23 | Disclaimer text | `"By continuing you agree to our maker terms · No card needed"` | Hi-fi: same string with `·` | Pass |

---

### Route: `/seller/onboard`

| # | Element | Implemented | Hi-fi / DESIGN.md spec | Severity |
|---|---|---|---|---|
| O-1 | Page background | `bg-[#f5f5f7]` — raw hex | `{colors.canvas-parchment}`; should be `bg-canvas-parchment` | Low |
| O-2 | Left rail width | `width: 280` — raw px | Hi-fi: `width: 280` — matches numerically; inline style vs. token | Low |
| O-3 | Left rail border | `borderRight: "1px solid #e0e0e0"` — raw hex | `{colors.hairline}` (#e0e0e0); should use `border-r` with token color | Low |
| O-4 | Progress bar fill | `bg-[#1d1d1f]` — raw hex | Should be `bg-foreground` | Low |
| O-5 | Progress bar track | `bg-[#e0e0e0]` — raw hex | `{colors.hairline}` — should be `bg-[color:var(--border)]` or similar | Low |
| O-6 | Step indicator — done state | `background: "#34c759"` — raw hex | Hi-fi uses `var(--good)` (green). No CSS variable defined in globals.css for this | Medium |
| O-7 | Step indicator — active state | `background: "#1d1d1f"` — raw hex | `{colors.ink}` / `var(--foreground)` | Low |
| O-8 | Step indicator text — inactive | `color: "#1d1d1f99"` — raw hex with inline alpha | Should be `text-foreground/60` | Low |
| O-9 | Tip box background | `style={{ background: "#f5f5f7" }}` — raw hex | `{colors.canvas-parchment}` | Low |
| O-10 | Tip label color | `style={{ color: "#c2410c" }}` — raw hex, Tailwind orange-700 | Hi-fi uses `var(--terra)` (terracotta brand color). No CSS variable for `--terra` in globals.css | Medium |
| O-11 | Form h1 font size | `style={{ fontSize: 40, letterSpacing: "-0.02em" }}` | Hi-fi: 40px / `-0.02em`; DESIGN.md `{typography.display-lg}` = 40px/600 — size matches. But `-0.02em` at 40px ≈ `-0.8px` vs spec `-0.374px` for display-md or 0px for display-lg | Medium |
| O-12 | Eyebrow copy | `"Step 2 · Location &amp; payouts"` | Hi-fi: `"Step 2 · Location &amp; payouts"` — uses `·` middle dot; matches | Pass |
| O-13 | Studio location card border | `border border-black/[0.06]` | Hi-fi: `hf-card` which uses `1px solid var(--line)`; `rgba(0,0,0,0.06)` matches intent | Pass |
| O-14 | Studio location field border-radius | `rounded-lg` = 18px | Hi-fi input fields use `borderRadius: 8` = `{rounded.sm}`. Implementation uses `rounded-lg` (18px) — wrong token | High |
| O-15 | Studio location card padding | `p-6` (24px) | Hi-fi: `padding: 24` — matches `{spacing.lg}` | Pass |
| O-16 | Payout option selected border | `border: "1.5px solid #1d1d1f"` — raw hex | Hi-fi: `borderColor: 'var(--ink)', borderWidth: 1.5` — correct value, raw hex | Low |
| O-17 | Payout option icons | Emoji (🏦, 💳, ⏱) | Hi-fi: SVG icons via `<Ico n="shield"/>`, `<Ico n="card"/>`, `<Ico n="clock"/>` | Low |
| O-18 | Payout option selected check badge | `background: "#1d1d1f"` — raw hex | `{colors.ink}` / `var(--foreground)` | Low |
| O-19 | Action row "Back" button | `"← Back"` with left arrow character | Hi-fi: `"← Back"` — matches | Pass |
| O-20 | Action row "Continue" button | `"Continue · Brand"` with middle dot | Hi-fi: `"Continue · Brand"` — matches | Pass |
| O-21 | Action row CTA border-radius | `rounded-full` (pill) | Hi-fi: `hf-btn hf-btn-primary` uses `{rounded.pill}` — matches | Pass |
| O-22 | Action row CTA padding | `px-6 py-2.5` (24px h, 10px v) | Hi-fi: `height: 44, padding: '0 22px'` — padding close, height explicit. Implemented via padding classes only | Low |
| O-23 | "Back" button rounded | `rounded-full px-5 py-2.5` | Hi-fi: `hf-btn hf-btn-ghost` — rounded-full matches `{rounded.pill}`; ghost style fits | Pass |
| O-24 | Left rail logo font size | `style={{ fontSize: 18 }}` — raw px | Hi-fi: `fontSize: 18` — matches numerically | Pass |
| O-25 | Form area input field height | `h-9` (36px) | Hi-fi: `height: 38` — 2px discrepancy | Low |
| O-26 | Payout security badge | `rounded-full bg-[#f5f5f7]` with text "256-bit · Stripe" | Hi-fi: `hf-chip hf-chip-soft` with lock icon + "256-bit · Stripe". Implementation missing the lock icon | Low |
| O-27 | MarketingTop nav | Not present on onboard page | Hi-fi `SFlow_02_Setup` does NOT include `MarketingTop` — only the left rail logo. Implementation is also correct in omitting it | Pass |

---

## 3. Token Violations

| File | Location | Violation | Token to use |
|---|---|---|---|
| `apply/page.tsx:19` | Eyebrow color | `"#0066cc"` raw hex | `var(--primary)` / `text-primary` |
| `apply/page.tsx:64` | Right panel bg | `bg-[#f5f5f7]` raw hex | `bg-canvas-parchment` |
| `apply/page.tsx:83-84` | Input border | `"1.5px solid #1d1d1f"` raw hex | `var(--foreground)` |
| `apply/page.tsx:83` | Input border-radius | `rounded-[10px]` non-token (10px) | `rounded-sm` (8px) or `rounded-md` (11px) |
| `apply/page.tsx:104` | Availability indicator | `background: "#34c759"` raw hex | No token defined; use semantic CSS var |
| `apply/page.tsx:141-149` | Category chips unselected | `background: "#f5f5f7"`, `border: "1px solid #e0e0e0"` raw hex | `bg-canvas-parchment`, `border-[color:var(--border)]` |
| `apply/page.tsx:160` | CTA button bg | `bg-[#0066cc]` raw hex | `bg-primary` |
| `marketing-top.tsx:24-25` | Nav link color | `"#1d1d1f"`, `"#1d1d1f99"` raw hex | `text-foreground`, `text-foreground/60` |
| `marketing-top.tsx:43` | CTA button bg | `bg-[#0066cc]` raw hex | `bg-primary` |
| `onboard/page.tsx:14` | Page bg | `bg-[#f5f5f7]` raw hex | `bg-canvas-parchment` |
| `onboard/page.tsx:23` | Rail border | `borderRight: "1px solid #e0e0e0"` raw hex | `border-r border-border` or `border-[color:var(--border)]` |
| `onboard/page.tsx:37-40` | Step dot backgrounds | `"#34c759"`, `"#1d1d1f"`, `"#f5f5f7"` raw hex | No green token; `var(--foreground)`, `var(--muted)` |
| `onboard/page.tsx:104` | Tip label color | `"#c2410c"` raw hex | `var(--terra)` not defined in globals.css — gap in token system |
| `onboard/page.tsx:35-39` | Progress bar | `bg-[#e0e0e0]`, `bg-[#1d1d1f]` raw hex | `bg-border`, `bg-foreground` |

**Weight 500 violations:** None found — all `font-semibold` uses are 600, `font-medium` is 500 only on button/label text which is permissible as nav-link usage. However `fontWeight: 500` appears in `marketing-top.tsx:27` on nav links — DESIGN.md states weight ladder is 300/400/600/700 with 500 deliberately absent. This is a token violation.

**Gradient violations:** `apply/page.tsx` and `onboard/page.tsx` — no decorative gradients found. Pass.

**Multi-shadow violations:** None found.

**Mixed radii grammar:** `apply/page.tsx:83` uses `rounded-[10px]` (10px) — outside the defined scale (8px, 11px, 18px, 9999px). This is a mixed radii grammar violation.

**Missing token — `--terra` / `--good`:** Both the terracotta (`#c2410c`) and system green (`#34c759`) colors appear as raw hex values in both pages. Neither is defined as a CSS custom property in `globals.css`. This is a token system gap: hi-fi uses `var(--terra)` and `var(--good)` but the production token file does not include them.

---

## 4. Verbatim Copy Diffs

| Page | Location | Implemented | Hi-fi spec | Match? |
|---|---|---|---|---|
| `/seller/apply` | Eyebrow | `"For makers · 4% per sale, no monthly fee"` | `'For makers · 4% per sale, no monthly fee'` — `·` middle dot | Pass |
| `/seller/apply` | H1 | `"Open a shop in about ten minutes."` | same | Pass |
| `/seller/apply` | Body | `"Bring your goods. We bring the storefront, payments, and a soft-spoken little community of buyers who want to know who made the thing."` | same | Pass |
| `/seller/apply` | Stat labels | `"active makers"`, `"paid out · April"`, `"avg. setup"` | same with `·` | Pass |
| `/seller/apply` | Card h2 | `"Claim your shop name"` | `"Claim your shop name"` | Pass |
| `/seller/apply` | Card subtitle | `"You can change this later. We'll spin up a free .micro.shop URL too."` | same | Pass |
| `/seller/apply` | Shop name shown | dynamic `{app.shopName}` | hi-fi shows `"Mira Studio"` — CLAUDE.md: "Mira" only appears intentionally on `/seller/apply` | Pass (correct intentional use) |
| `/seller/apply` | Domain shown | dynamic `{app.domain}` | hi-fi shows `"mira-studio.micro.shop"` | Pass |
| `/seller/apply` | Categories | dynamic `{app.categories}` | hi-fi: `['Ceramics', 'Bakery', 'Textiles', 'Jewelry', 'Vintage', 'Other']` | Cannot verify without runtime data |
| `/seller/apply` | CTA | `"Continue · {app.stepsLeft} steps left"` | `"Continue · 6 steps left"` | Pass (dynamic) |
| `/seller/apply` | Disclaimer | `"By continuing you agree to our maker terms · No card needed"` | same with `·` | Pass |
| `/seller/onboard` | Step label | `"Step 2 of 6"` | `"Step 2 of 6"` | Pass |
| `/seller/onboard` | Eyebrow | `"Step 2 · Location &amp; payouts"` (→ "Step 2 · Location & payouts") | `"Step 2 · Location &amp; payouts"` with `·` | Pass |
| `/seller/onboard` | H1 | `"Where are you shipping from, and where should we send the money?"` | same | Pass |
| `/seller/onboard` | Studio card h2 | `"Studio location"` | `"Studio location"` | Pass |
| `/seller/onboard` | Studio card sub | `"Customers see only your city &amp; state."` | same | Pass |
| `/seller/onboard` | Payout card h2 | `"Where to send your payouts"` | `"Where to send your payouts"` | Pass |
| `/seller/onboard` | Payout card sub | `"Pick one — you can add more later."` | `"Pick one — you can add more later."` — uses en-dash `—` | Pass |
| `/seller/onboard` | Payout options | `"Bank account"`, `"Debit card"`, `"Add later"` | same | Pass |
| `/seller/onboard` | Payout subs | `"ACH · 1–2 days"`, `"Instant · 1.5%"`, `"Launch in draft"` | same with `·` and en-dash `–` in "1–2 days" | Pass |
| `/seller/onboard` | Tip label | `"Tip"` uppercase | `"Tip"` uppercase | Pass |
| `/seller/onboard` | Tip body | `"Add payouts last if you'd like — you can launch in draft and finish this when an order comes in."` | same with em-dash `—` | Pass |
| `/seller/onboard` | Back button | `"← Back"` | `"← Back"` | Pass |
| `/seller/onboard` | Continue button | `"Continue · Brand"` | `"Continue · Brand"` | Pass |
| `/seller/onboard` | Security badge | `"256-bit · Stripe"` | `"256-bit · Stripe"` with lock icon prefix | Pass (copy); icon missing (Low) |

---

## 5. Pass/Fail Verdict

| Route | Status | Notes |
|---|---|---|
| `/` | **FAIL** | Page is a stub placeholder. No UI implemented against hi-fi. |
| `/seller/apply` | **CONDITIONAL PASS** | All copy matches verbatim. Layout and structure match hi-fi. Primary issues are raw hex values throughout (should use CSS tokens), one non-token border-radius (10px), incorrect H1 size (64px vs 56px spec), and a broken `blink` animation keyframe. No missing sections. |
| `/seller/onboard` | **CONDITIONAL PASS** | All copy matches verbatim. Layout matches hi-fi. Primary issues are raw hex values throughout, input field `rounded-lg` (18px) instead of `rounded-sm` (8px), use of emoji instead of SVG icons in payout cards, and missing `--terra` / `--good` tokens in the global token system. |

### Severity Summary

| Severity | Count | Routes affected |
|---|---|---|
| Critical | 5 | `/` |
| High | 1 | `/seller/onboard` (input border-radius) |
| Medium | 6 | `/seller/apply` (H1 size, letter-spacing, availability color, blink animation, input radius), `/seller/onboard` (step done indicator, tip color, letter-spacing) |
| Low | ~30 | Both marketing pages — pervasive raw hex instead of token references |

### Top High-Severity Findings

1. **`/` is entirely unimplemented** — the home page is a single placeholder paragraph with no shopper topbar, no hero, no product grid, and no product rails.
2. **`/seller/onboard` — input field border-radius** (`rounded-lg` = 18px) should be `rounded-sm` (8px) per hi-fi spec — wrong token used.
3. **`--terra` and `--good` CSS variables are missing from `globals.css`** — both marketing pages use raw hex fallbacks (`#34c759`, `#c2410c`) because these hi-fi tokens were never added to the production token file.
4. **`/seller/apply` H1 font-size is 64px** — spec `{typography.hero-display}` = 56px; 8px over-scale.
5. **`blink` animation keyframe undefined** — `apply/page.tsx` references `animation: "blink 1s steps(1) infinite"` but only `hf-shimmer` is defined in `globals.css`; the cursor blink is non-functional.
