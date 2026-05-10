# Design Audit — Worker 6
## Pages: /seller/orders (inbox) + /seller/orders/[id] (detail)
### Date: 2026-05-10

---

## 1. Scope

| Item | Value |
|---|---|
| Routes audited | `/seller/orders`, `/seller/orders/[id]` |
| Hi-fi references | `hifi-seller-mgmt.jsx` — `SMgmt_OrdersInbox`, `SMgmt_OrderDetail` |
| Implementation files | `web/src/app/seller/orders/page.tsx`, `web/src/app/seller/orders/[id]/page.tsx` and all sub-components under `web/src/components/seller/` |
| Design spec | `DESIGN.md` |
| Token source | `web/src/app/globals.css` |

---

## 2. Token Verification

Hex literals found in implementation that are **not** raw design tokens but resolve to token values:

| Hex / value in code | Equivalent token | Verdict |
|---|---|---|
| `#1d1d1f` (ink) | `--foreground` / `--color-foreground` | Token exists; raw hex used instead of `text-foreground` Tailwind class — minor but consistent pattern across all seller components |
| `#0066cc` | `--primary` (oklch(0.508 0.176 255)) | Token exists; raw hex only in `orders-inbox-bulk-bar.tsx` line 12 (`text-[#0066cc]`) |
| `#f5f5f7` | `--canvas-parchment` / `--color-canvas-parchment` | Token exists; raw hex used inline in several detail components |
| `#16a34a` (green) | No dedicated token — emerald-600 Tailwind default | No custom token defined for "good/success green"; usage consistent with Tailwind utility |
| `rgba(0,102,204,0.06)` | Derived from `--primary`; no alpha-variant token | No token defined; acceptable as a one-off tint |
| `rgba(194,65,12,0.05)` | Orange-tinted surface for unfulfilled fulfillment header | No token; acceptable one-off |

**Conclusion:** No hex literals reference colors that are undefined — all either map to an existing CSS custom property or to a standard Tailwind color. The main issue is **bypassing the token system** by writing raw hex instead of using the defined CSS variable or Tailwind utility class.

---

## 3. Findings

### 3.1 /seller/orders — Orders Inbox

#### F-01 · Topbar action buttons use wrong border-radius token
- **Severity:** Low
- **Location:** `web/src/app/seller/orders/page.tsx:36–49`
- **Hi-fi:** Both "Export CSV" and "Manual order" buttons are rendered as `hf-btn hf-btn-sm` — small compact utility buttons matching `{rounded.sm}` (8px).
- **Implementation:** "Export CSV" uses `rounded-lg` (Tailwind default 8px — correct). "Manual order" also uses `rounded-lg` — correct. **No gap.**

#### F-02 · Bulk bar selected-row tint uses raw hex instead of token
- **Severity:** Low
- **Location:** `web/src/components/seller/orders-inbox-bulk-bar.tsx:9`
- **Hi-fi:** `background: 'rgba(0,102,204,0.06)'` on the bulk bar.
- **Implementation:** `bg-[rgba(0,102,204,0.06)]` — matches hi-fi exactly but bypasses `--primary` token. Should be `bg-[color-mix(in_oklch,var(--primary)_6%,transparent)]` or a defined alpha token.

#### F-03 · "3 orders selected" text color uses raw hex
- **Severity:** Low
- **Location:** `web/src/components/seller/orders-inbox-bulk-bar.tsx:12`
- **Hi-fi:** `color: 'var(--primary)'`
- **Implementation:** `text-[#0066cc]` — hardcoded hex instead of `text-primary`.

#### F-04 · Selected row tint in table uses raw rgba instead of token
- **Severity:** Low
- **Location:** `web/src/components/seller/orders-inbox-table.tsx:124`
- **Hi-fi:** `background: 'rgba(0,102,204,0.04)'`
- **Implementation:** `style={{ background: "rgba(0,102,204,0.04)" }}` — inline style, matches hi-fi value but should use a Tailwind token utility.

#### F-05 · Filter chip height and font size match hi-fi
- **Severity:** None (pass)
- **Location:** `web/src/components/seller/orders-inbox-filter-row.tsx:69–76`
- **Hi-fi:** `height: 30, padding: '0 12px', fontSize: 11.5`
- **Implementation:** `h-[30px] px-3 text-[11.5px]` — exact match.

#### F-06 · Tab count badge uses `bg-black/[0.06]` instead of `var(--paper-2)`
- **Severity:** Low
- **Location:** `web/src/components/seller/orders-inbox-tabs.tsx:24`
- **Hi-fi:** inactive tab count badge uses `background: 'var(--paper-2)'`
- **Implementation:** `bg-black/[0.06]` — visually equivalent (paper-2 is ~6% black on white) but deviates from the hi-fi token intent. Active badge uses `bg-[#1d1d1f]` instead of `bg-foreground` — raw hex.

#### F-07 · Pagination page-number buttons use `rounded-full` instead of `rounded-md`
- **Severity:** Low
- **Location:** `web/src/components/seller/orders-inbox-pagination.tsx:66`
- **Hi-fi:** page chips use `hf-chip` class which resolves to a subtle pill shape with `borderRadius: 999`.
- **Implementation:** `rounded-full` — correct match (pill/999px).

#### F-08 · Table header column labels use `text-[11px] font-medium text-[#1d1d1f]/40`
- **Severity:** None (pass)
- **Hi-fi:** header row uses `hf-tiny hf-muted` — maps to ~11px muted.
- **Implementation:** matches in size and opacity.

---

### 3.2 /seller/orders/[id] — Order Detail

#### F-09 · Breadcrumb status badge uses Tailwind semantic colors instead of design system chip pattern
- **Severity:** Medium
- **Location:** `web/src/app/seller/orders/[id]/page.tsx:39–43`
- **Hi-fi:** `hf-chip hf-chip-warn` — a unified chip pattern with `fontSize: 11`, using `var(--warn)` tones.
- **Implementation:** `rounded-full bg-orange-100 px-2.5 py-1 text-[11px] font-semibold text-orange-700` with an `inline-block h-1.5 w-1.5 rounded-full bg-orange-500` dot.
- **Gap:** Uses Tailwind semantic `orange-*` instead of the design system's amber/warn pattern used elsewhere in the table (`bg-amber-50 text-amber-700`). The two representations of "warn" are inconsistent: inbox table uses `amber-*`, breadcrumb uses `orange-*`.

#### F-10 · "Buy label" primary CTA uses `rounded-xl` instead of `rounded-lg`
- **Severity:** Low
- **Location:** `web/src/app/seller/orders/[id]/page.tsx:69`
- **Hi-fi:** `hf-btn hf-btn-primary hf-btn-sm` — small primary button matching `rounded-sm` (8px) or `rounded-lg` for utility buttons.
- **Implementation:** `rounded-xl` (12px in Tailwind) — one step larger than expected for a compact utility button. Other outline buttons in the same row use `rounded-lg` (8px).

#### F-11 · Fulfillment shipped indicator uses hardcoded `#16a34a` instead of emerald/good token
- **Severity:** Low
- **Location:** `web/src/components/seller/order-detail-fulfillments.tsx:44`
- **Implementation:** `background: "#16a34a"` — no CSS custom property exists for success green; Tailwind `bg-emerald-600` would be more consistent.

#### F-12 · Fulfillment unfulfilled header background uses inline `rgba(194,65,12,0.05)` — no token
- **Severity:** Low
- **Location:** `web/src/components/seller/order-detail-fulfillments.tsx:33`
- **Hi-fi:** `background: 'rgba(194,65,12,0.05)'`
- **Implementation:** exact match but inline style; no token. Could use `bg-orange-900/5` Tailwind.

#### F-13 · Refund card uses inline `background: "#f5f5f7"` instead of `bg-canvas-parchment`
- **Severity:** Low
- **Location:** `web/src/components/seller/order-detail-refund.tsx:28, 117`
- **Token available:** `--canvas-parchment` (oklch 0.965) is defined in globals.css and exposed as `--color-canvas-parchment` → Tailwind `bg-canvas-parchment`.
- **Gap:** Two occurrences use `background: "#f5f5f7"` inline style.

#### F-14 · "You'll receive" amount uses hardcoded `#16a34a` instead of token or Tailwind utility
- **Severity:** Low
- **Location:** `web/src/components/seller/order-detail-summary.tsx:47`
- **Implementation:** `style={{ color: "#16a34a" }}` — should be `text-emerald-600` or a defined success-color token.

#### F-15 · Internal note background uses inline `#f5f5f7` instead of `bg-canvas-parchment`
- **Severity:** Low
- **Location:** `web/src/components/seller/order-detail-internal-note.tsx:17`
- **Implementation:** `style={{ background: "#f5f5f7", fontSize: 12 }}` — token bypass.

#### F-16 · Timeline connector line uses `rgba(0,0,0,0.08)` — no token
- **Severity:** Low
- **Location:** `web/src/components/seller/order-detail-timeline.tsx:57`
- **Hi-fi:** `background: 'var(--line)'`
- **Implementation:** inline `rgba(0,0,0,0.08)` — visually equivalent to `border-black/[0.08]` pattern used elsewhere but inconsistent; should use `bg-black/[0.08]` Tailwind utility.

#### F-17 · Timeline warn-tone icon color uses hardcoded `#c2410c` (orange-700)
- **Severity:** Low
- **Location:** `web/src/components/seller/order-detail-timeline.tsx:43`
- **Implementation:** `color: "#c2410c"` — no token; should be `text-orange-700` Tailwind.

#### F-18 · Order detail breadcrumb "Cancel order" button missing X icon
- **Severity:** Medium
- **Location:** `web/src/app/seller/orders/[id]/page.tsx:61–66`
- **Hi-fi:** `<button ... style={{ color: 'var(--bad)' }}><Ico n="x" s={11} /> Cancel order</button>`
- **Implementation:** Button text "Cancel order" present and uses `text-red-600` (acceptable). However the `<Ico n="x">` leading icon is **missing** from the implementation.

#### F-19 · Order detail body grid uses inline style for template columns
- **Severity:** Low
- **Location:** `web/src/app/seller/orders/[id]/page.tsx:80`
- **Implementation:** `style={{ gridTemplateColumns: "1.6fr 1fr", alignItems: "start" }}` — matches hi-fi ratio exactly. Minor: `alignItems` could be Tailwind `items-start` but the non-standard column ratio requires inline style regardless; acceptable.

#### F-20 · Customer "Profile →" button is unstyled text — no icon, no hover state
- **Severity:** Low
- **Location:** `web/src/components/seller/order-detail-customer.tsx:22–25`
- **Hi-fi:** `hf-btn hf-btn-ghost hf-btn-sm` with `marginLeft: 'auto'`
- **Implementation:** bare `<button>` with `ml-auto text-xs font-medium text-[#1d1d1f]/50` — ghost style not applied, no hover background.

---

## 4. Summary Table

| ID | Page | Component | Severity | Category |
|---|---|---|---|---|
| F-02 | /seller/orders | OrdersInboxBulkBar | Low | Token bypass (raw rgba) |
| F-03 | /seller/orders | OrdersInboxBulkBar | Low | Token bypass (raw hex instead of `text-primary`) |
| F-04 | /seller/orders | OrdersInboxTable | Low | Token bypass (inline rgba) |
| F-06 | /seller/orders | OrdersInboxTabs | Low | Token divergence (paper-2 vs black/6%) |
| F-09 | /seller/orders/[id] | breadcrumb status chip | Medium | Color inconsistency (orange vs amber for warn) |
| F-10 | /seller/orders/[id] | "Buy label" CTA | Low | Wrong border-radius (`rounded-xl` vs `rounded-lg`) |
| F-11 | /seller/orders/[id] | OrderDetailFulfillments | Low | Token bypass (hardcoded green) |
| F-12 | /seller/orders/[id] | OrderDetailFulfillments | Low | Token bypass (inline orange tint) |
| F-13 | /seller/orders/[id] | OrderDetailRefund | Low | Token bypass (`#f5f5f7` × 2) |
| F-14 | /seller/orders/[id] | OrderDetailSummary | Low | Token bypass (hardcoded green) |
| F-15 | /seller/orders/[id] | OrderDetailInternalNote | Low | Token bypass (`#f5f5f7`) |
| F-16 | /seller/orders/[id] | OrderDetailTimeline | Low | Token bypass (inline rgba connector) |
| F-17 | /seller/orders/[id] | OrderDetailTimeline | Low | Token bypass (hardcoded orange-700) |
| F-18 | /seller/orders/[id] | breadcrumb "Cancel order" | Medium | Missing leading icon (X) |
| F-20 | /seller/orders/[id] | OrderDetailCustomer | Low | Missing ghost button hover state |

**Totals:** 2 Medium · 13 Low · 0 Critical

---

## 5. Passing Checks

- Filter row search input shape (rounded-full, h-8, correct placeholder text) — exact match.
- Filter chips height (30px), font size (11.5px) — exact match.
- Table column set (Order, Customer, Items, Ship, Total, Status, Age, chevron) — exact match.
- Status chip pattern in table (dot + label, amber/emerald/red/muted) — matches hi-fi tone semantics.
- Selected row highlight tint value `rgba(0,102,204,0.04)` — exact match to hi-fi.
- Bulk bar button set (Print labels, Mark packed, Bulk message, Cancel) — exact match.
- Pagination: "Showing 1 – 10 of N", 5 page chips, prev/next chevrons — exact match.
- Order detail two-column layout ratio (1.6fr 1fr) — exact match.
- Fulfillment card structure (header + body per fulfillment, tracking number mono, buy label button) — exact match.
- Refund card item list with partial amount input and reason/restock controls — exact match.
- Timeline event set and connector line structure — exact match.
- Customer card (avatar, name, lifetime label, address, email) — exact match.
- Summary card (subtotal / shipping / tax / paid / fee / label / net) — exact match.
- Internal note + tags — exact match.
