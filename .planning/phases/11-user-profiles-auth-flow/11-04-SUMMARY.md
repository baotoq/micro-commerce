---
phase: 11
plan: 04
subsystem: frontend-account
tags: [ui, profile, avatar, react-query, account-layout]
dependency_graph:
  requires: [session-provider, api-client, react-query]
  provides: [account-layout, profile-page, avatar-upload, profile-hooks]
  affects: [user-profile-management]
tech_stack:
  added: [avatar-upload-component, account-sidebar, profile-form]
  patterns: [view-edit-toggle, click-to-upload, sidebar-navigation]
key_files:
  created:
    - src/MicroCommerce.Web/src/hooks/use-profile.ts
    - src/MicroCommerce.Web/src/components/account/account-sidebar.tsx
    - src/MicroCommerce.Web/src/components/account/avatar-upload.tsx
    - src/MicroCommerce.Web/src/components/account/profile-form.tsx
    - src/MicroCommerce.Web/src/app/(storefront)/account/layout.tsx
    - src/MicroCommerce.Web/src/app/(storefront)/account/page.tsx
    - src/MicroCommerce.Web/src/app/(storefront)/account/profile/page.tsx
    - src/MicroCommerce.Web/src/app/(storefront)/account/addresses/page.tsx
    - src/MicroCommerce.Web/src/app/(storefront)/account/orders/page.tsx
    - src/MicroCommerce.Web/src/app/(storefront)/account/security/page.tsx
  modified:
    - src/MicroCommerce.Web/src/lib/api.ts
    - src/MicroCommerce.Web/.env
decisions:
  - "Profile form uses view/edit mode toggle pattern (not inline editing)"
  - "Avatar uses generic User icon as fallback (not initials per user decision)"
  - "Security section links to Keycloak account management (no custom password form)"
  - "/account/orders redirects to existing /orders page to avoid duplication"
  - "Avatar upload validates image type and 5MB max file size"
  - "SessionProvider already exists in root layout, no need to duplicate"
metrics:
  duration_minutes: 3
  tasks_completed: 2
  files_created: 12
  commits: 2
  completed_at: "2026-02-13T08:21:36Z"
---

# Phase 11 Plan 04: My Account Frontend Layout Summary

**One-liner:** Account layout with sidebar navigation, profile view/edit, and click-to-upload avatar using React Query hooks

## What Was Built

### Task 1: Profile API Types and Hooks
- Added ProfileDto, AddressDto types with full CRUD request types to api.ts
- Implemented profile API functions: getMyProfile, updateProfile, uploadAvatar, removeAvatar
- Implemented address API functions: addAddress, updateAddress, deleteAddress, setDefaultAddress
- All API functions support both Bearer token (Authorization header) and credentials: include
- Created use-profile.ts with React Query hooks: useProfile, useUpdateProfile, useUploadAvatar, useRemoveAvatar, and full address CRUD hooks
- Hooks include toast notifications and automatic cache invalidation

### Task 2: Account Layout and Components
- **AccountSidebar**: Client component with 4 sections (Profile, Addresses, Orders, Security)
  - Uses usePathname for active state highlighting
  - Responsive: stacks on mobile, sidebar on desktop
  - Icons: User, MapPin, Package, Shield from lucide-react
- **AvatarUpload**: Click-to-upload component
  - Hidden file input triggered by avatar button click
  - Hover overlay with Upload icon
  - File validation: image/* type, max 5MB size
  - Loading spinner during upload
  - Remove button (visible only when avatar exists)
  - Fallback: generic User icon silhouette (not initials)
- **ProfileForm**: View/edit toggle pattern
  - View mode: display name, email (read-only), member since date, Edit button
  - Edit mode: display name input, Save/Cancel buttons
  - Uses useProfile and useUpdateProfile hooks
  - Loading skeleton while fetching
  - AvatarUpload component integrated at top
- **Account Layout**: Responsive grid layout
  - Desktop: sidebar (256px) + content area
  - Mobile: sidebar stacks above content
  - Max width 6xl, proper spacing
- **Account Pages**:
  - /account: Redirects to /account/profile
  - /account/profile: Renders ProfileForm
  - /account/addresses: Placeholder with address count (Plan 05 will complete)
  - /account/orders: Redirects to existing /orders page
  - /account/security: Keycloak account management link with ExternalLink icon
- **Environment**: Added NEXT_PUBLIC_KEYCLOAK_ISSUER for client-side access to Keycloak account URL

## Deviations from Plan

None - plan executed exactly as written. All must-have truths and artifacts delivered.

## Verification Results

- TypeScript compilation: PASSED (npx tsc --noEmit)
- All account routes exist: VERIFIED
- Components created: 3 (sidebar, avatar-upload, profile-form)
- Pages created: 5 (layout, page, profile, addresses, orders, security)
- API hooks: 9 functions exported from use-profile.ts
- Success criteria: ALL MET
  - Sidebar highlights active section
  - Profile form has view/edit toggle
  - Avatar triggers upload on click with validation
  - Remove avatar button visible when avatar exists
  - Security links to Keycloak (not custom form)
  - /account redirects to /account/profile
  - /account/orders redirects to /orders

## Integration Points

- **SessionProvider**: Already in root layout (app/layout.tsx), provides session to useSession hook
- **QueryProvider**: Already in storefront layout, provides React Query context
- **API Client**: Profile hooks use session.accessToken from useSession
- **Existing UI Components**: Avatar, Card, Button, Input, Label from shadcn/ui
- **Toast Notifications**: Sonner toast for all mutations
- **Icons**: lucide-react (User, MapPin, Package, Shield, Upload, ExternalLink, Loader2)

## Known Limitations

- Addresses page is a placeholder (will be completed in Plan 05)
- Backend Profile API endpoints not yet implemented (plan focuses on frontend)
- No tests written yet (testing strategy TBD)

## Self-Check: PASSED

**Created Files:**
- FOUND: src/MicroCommerce.Web/src/hooks/use-profile.ts
- FOUND: src/MicroCommerce.Web/src/components/account/account-sidebar.tsx
- FOUND: src/MicroCommerce.Web/src/components/account/avatar-upload.tsx
- FOUND: src/MicroCommerce.Web/src/components/account/profile-form.tsx
- FOUND: src/MicroCommerce.Web/src/app/(storefront)/account/layout.tsx
- FOUND: src/MicroCommerce.Web/src/app/(storefront)/account/page.tsx
- FOUND: src/MicroCommerce.Web/src/app/(storefront)/account/profile/page.tsx
- FOUND: src/MicroCommerce.Web/src/app/(storefront)/account/addresses/page.tsx
- FOUND: src/MicroCommerce.Web/src/app/(storefront)/account/orders/page.tsx
- FOUND: src/MicroCommerce.Web/src/app/(storefront)/account/security/page.tsx

**Modified Files:**
- FOUND: src/MicroCommerce.Web/src/lib/api.ts
- FOUND: src/MicroCommerce.Web/.env

**Commits:**
- FOUND: 21acb5a9 (feat(11-04): add profile/address API types and hooks)
- FOUND: 0b6eb71e (feat(11-04): create account layout with sidebar and profile page)
