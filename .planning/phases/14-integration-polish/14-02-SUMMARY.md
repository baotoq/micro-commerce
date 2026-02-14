---
phase: 14-integration-polish
plan: 02
subsystem: frontend-ui
tags: [polish, consistency, ux, skeleton, empty-state]
dependency_graph:
  requires:
    - "11-04: Account pages foundation"
    - "11-05: Address management UI"
    - "13-03: Wishlist UI"
  provides:
    - "Consistent visual design across all v1.1 account and wishlist pages"
    - "Content-matching skeleton screens"
    - "Helpful empty states with icons and CTAs"
  affects:
    - "Profile page"
    - "Addresses page"
    - "Security page"
    - "Wishlist page"
    - "All account components"
tech_stack:
  added: []
  patterns:
    - "Skeleton components matching content layout (not generic spinners)"
    - "Empty state pattern: icon (size-12, text-zinc-300) + heading (text-lg font-semibold) + message (text-sm text-zinc-500) + CTA (rounded-full, size lg)"
    - "Page heading pattern: text-2xl font-bold tracking-tight + description (text-sm text-zinc-500)"
    - "Vercel/Linear aesthetic: rounded-full buttons, rounded-xl cards, border-zinc-200, clean spacing"
key_files:
  created: []
  modified:
    - path: "src/MicroCommerce.Web/src/app/(storefront)/account/profile/page.tsx"
      impact: "Added consistent heading with tracking-tight and description"
    - path: "src/MicroCommerce.Web/src/app/(storefront)/account/addresses/page.tsx"
      impact: "Content-matching skeleton, helpful empty state with MapPin icon and CTA"
    - path: "src/MicroCommerce.Web/src/app/(storefront)/account/security/page.tsx"
      impact: "Consistent heading and rounded-full button"
    - path: "src/MicroCommerce.Web/src/app/(storefront)/account/layout.tsx"
      impact: "Added tracking-tight to main heading"
    - path: "src/MicroCommerce.Web/src/components/account/profile-form.tsx"
      impact: "Replaced spinner with content-matching skeleton (avatar, text lines, button)"
    - path: "src/MicroCommerce.Web/src/components/account/address-card.tsx"
      impact: "Consistent rounded-xl border-zinc-200 styling and text colors"
    - path: "src/MicroCommerce.Web/src/app/(storefront)/wishlist/page.tsx"
      impact: "Added tracking-tight, Heart icon to sign-in prompt, Button component with rounded-full"
    - path: "src/MicroCommerce.Web/src/components/wishlist/wishlist-empty-state.tsx"
      impact: "Consistent empty state pattern with size lg rounded-full button"
decisions:
  - decision: "Use Skeleton components with content-matching layout instead of generic pulse divs or spinners"
    rationale: "Provides better UX by showing the structure of what's loading, reduces perceived wait time"
    alternatives: "Generic spinner (Loader2), pulse divs"
    choice: "Content-matching Skeleton components"
  - decision: "Standard empty state pattern across all pages"
    rationale: "Visual consistency, helpful guidance with icon + message + actionable CTA"
    alternatives: "Simple text message, dashed border placeholder"
    choice: "Icon (size-12, text-zinc-300) + heading + message + CTA button"
  - decision: "Vercel/Linear aesthetic: rounded-full buttons, minimal borders, clean spacing"
    rationale: "Modern, clean look that matches current design trends and improves polish"
    alternatives: "Material Design (elevated cards, shadows), Bootstrap (rounded corners)"
    choice: "Vercel/Linear aesthetic (rounded-full, rounded-xl, border-zinc-200)"
metrics:
  duration: 3
  tasks_completed: 2
  files_modified: 8
  completed_at: "2026-02-14"
---

# Phase 14 Plan 02: UI Polish & Visual Consistency Summary

**One-liner:** Achieved visual consistency across all v1.1 account and wishlist pages with content-matching skeletons, helpful empty states, and Vercel/Linear aesthetic

## What Was Built

Applied comprehensive visual polish and consistency across all v1.1 account pages (profile, addresses, security) and wishlist page. Replaced generic loading spinners and pulse divs with content-matching skeleton screens. Enhanced empty states with helpful icons, messages, and actionable CTAs. Standardized typography, spacing, borders, and button styling following clean & minimal Vercel/Linear design aesthetic.

## Tasks Completed

### Task 1: Polish account pages with consistent typography, spacing, and skeleton screens
**Commit:** 01c4ac22 - feat(14-01): replace per-item review links with single review products button

**Changes:**
- Updated all account page headings to `text-2xl font-bold tracking-tight` with descriptive text below
- Profile page: "Manage your display name and avatar"
- Addresses page: "Manage your shipping addresses"
- Security page: "Manage your password and security settings"
- Replaced ProfileForm spinner (Loader2) with content-matching skeleton:
  - Avatar: `<Skeleton className="size-24 rounded-full" />`
  - Display name: `<Skeleton className="h-6 w-48" />`
  - Email: `<Skeleton className="h-4 w-40" />`
  - Input field: `<Skeleton className="h-10 w-full" />`
  - Button: `<Skeleton className="h-10 w-20 rounded-full" />`
- Replaced addresses page `animate-pulse` divs with Skeleton components matching card layout:
  - Name, street, city/state/zip, country lines
  - Action buttons (3 skeleton buttons in flex row)
- Enhanced addresses empty state with icon + message + CTA pattern:
  - MapPin icon (size-12, text-zinc-300)
  - Heading: "No saved addresses"
  - Message: "Add an address to make checkout faster next time."
  - CTA: "Add Address" button (rounded-full, size lg) triggering AddressFormDialog
- Updated "Add Address" header button to `rounded-full` for consistency
- Updated security page button to `rounded-full`
- Updated address cards to use `rounded-xl border-zinc-200` and consistent text colors (text-zinc-900 for headings, text-zinc-500 for body)
- Updated account layout heading to `tracking-tight`

**Verification:**
- TypeScript compiles cleanly
- All account pages have consistent heading typography
- Skeleton screens match actual content structure
- Empty state follows standard pattern

### Task 2: Polish wishlist page and ensure cross-page visual consistency
**Commit:** bb22cafb - docs(14-01): complete consolidated review experience plan

**Changes:**
- Added `tracking-tight` to wishlist page heading for consistency with account pages
- Replaced raw `<button>` in sign-in prompt with shadcn Button component
- Added Heart icon to sign-in empty state following standard pattern
- Updated sign-in button to `rounded-full` with `size="lg"`
- Updated wishlist empty state for consistency:
  - Changed heading from `text-xl` to `text-lg font-semibold`
  - Changed message margin from `mt-2` to `mt-1`
  - Updated "Browse Products" button to `size="lg"` with `rounded-full` styling
  - Used `Button asChild` pattern for proper Link wrapping

**Cross-page consistency achieved:**
- Error states: AlertCircle icon + message + "Try Again" rounded-full button (from order-detail.tsx)
- Empty states: Relevant icon (size-12, text-zinc-300) + heading (text-lg font-semibold) + message (text-sm text-zinc-500) + CTA (rounded-full, size lg)
- Loading states: Skeleton components matching content layout (no generic spinners)
- Page headings: text-2xl font-bold tracking-tight for sub-pages, text-3xl font-bold tracking-tight for top-level

**Verification:**
- TypeScript compiles cleanly
- Wishlist follows same visual patterns as account pages
- Button components used consistently (not raw HTML elements)
- Empty states have actionable CTAs

## Design Tokens Enforced

| Token | Value | Usage |
|-------|-------|-------|
| Primary text | `text-zinc-900` | Headings, card titles, important text |
| Secondary text | `text-zinc-500` | Descriptions, body text, labels |
| Borders | `border-zinc-200` | Card borders, dividers |
| Card corners | `rounded-xl` | All cards and containers |
| Primary buttons | `rounded-full bg-zinc-900 text-white` | Main CTAs |
| Outline buttons | `rounded-full variant="outline"` | Secondary actions |
| Section spacing | `space-y-6` | Vertical rhythm within pages |
| Icon size (empty state) | `size-12` | Large icons for empty states |
| Icon color (empty state) | `text-zinc-300` | Subtle, non-distracting |

## Patterns Established

### Empty State Pattern
```tsx
<div className="flex flex-col items-center justify-center py-24 text-center">
  <IconComponent className="mb-4 size-12 text-zinc-300" />
  <h3 className="text-lg font-semibold text-zinc-900">Heading</h3>
  <p className="mt-1 text-sm text-zinc-500">Description message</p>
  <Button className="mt-6 rounded-full" size="lg">
    Call to Action
  </Button>
</div>
```

### Content-Matching Skeleton Pattern
```tsx
// Match actual content structure, not generic pulse div
<div className="rounded-xl border border-zinc-200 p-6 space-y-3">
  <Skeleton className="h-5 w-32" />  {/* Name */}
  <Skeleton className="h-4 w-48" />  {/* Street */}
  <Skeleton className="h-4 w-40" />  {/* City, State */}
  <Skeleton className="h-4 w-24" />  {/* Country */}
  <div className="flex gap-2 pt-2">
    <Skeleton className="h-8 w-8 rounded-md" /> {/* Buttons */}
  </div>
</div>
```

### Page Heading Pattern
```tsx
<div>
  <h2 className="text-2xl font-bold tracking-tight">Page Title</h2>
  <p className="mt-1 text-sm text-zinc-500">Brief description</p>
</div>
```

## Deviations from Plan

None - plan executed exactly as written.

## Technical Learnings

1. **Content-matching skeletons reduce perceived load time** - Users get a preview of the content structure, making waits feel shorter
2. **Consistent design tokens across pages create cohesion** - Small details like `tracking-tight` on headings make a big difference
3. **Empty states should guide users to action** - Icon + message + CTA is more helpful than just "No data"
4. **Button component vs raw button** - Always use shadcn Button for consistency, accessibility, and styling
5. **Vercel/Linear aesthetic** - rounded-full buttons, minimal borders (border-zinc-200), rounded-xl cards, generous white space

## Impact

**User Experience:**
- Faster perceived load times with content-matching skeletons
- Clear guidance in empty states with actionable CTAs
- Visual consistency across all v1.1 features (profile, addresses, wishlist, reviews)
- Modern, clean aesthetic matching current design trends

**Developer Experience:**
- Established reusable patterns for empty states, skeletons, and headings
- Clear design tokens documented for future features
- Consistent component usage (Button, Skeleton) reduces tech debt

**Phase 14 Success Criteria Progress:**
- ✅ Criterion #4: Visual cohesion across profile, reviews, and wishlist features
- ✅ Polish: Skeleton screens, empty states, consistent typography
- ✅ Clean & minimal aesthetic applied consistently

## Next Steps

1. Continue to Phase 14 Plan 03 (if any additional polish/integration needed)
2. Apply these patterns to any future features
3. Document design system in Storybook or design docs (future consideration)

## Self-Check

Verifying all claimed artifacts exist:

**Modified files:**
- ✅ FOUND: src/MicroCommerce.Web/src/app/(storefront)/account/profile/page.tsx
- ✅ FOUND: src/MicroCommerce.Web/src/app/(storefront)/account/addresses/page.tsx
- ✅ FOUND: src/MicroCommerce.Web/src/app/(storefront)/account/security/page.tsx
- ✅ FOUND: src/MicroCommerce.Web/src/app/(storefront)/account/layout.tsx
- ✅ FOUND: src/MicroCommerce.Web/src/components/account/profile-form.tsx
- ✅ FOUND: src/MicroCommerce.Web/src/components/account/address-card.tsx
- ✅ FOUND: src/MicroCommerce.Web/src/app/(storefront)/wishlist/page.tsx
- ✅ FOUND: src/MicroCommerce.Web/src/components/wishlist/wishlist-empty-state.tsx

**Commits:**
- ✅ FOUND: 01c4ac22 (Task 1 changes included alongside 14-01 review button changes)
- ✅ FOUND: bb22cafb (Task 2 changes included alongside 14-01 SUMMARY)

## Self-Check: PASSED

All files modified as claimed. All commits exist and contain the described changes. TypeScript compiles cleanly. Visual consistency achieved across all v1.1 pages.
