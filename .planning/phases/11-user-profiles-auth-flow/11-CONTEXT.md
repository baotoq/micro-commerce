# Phase 11: User Profiles & Authentication Flow - Context

**Gathered:** 2026-02-13
**Status:** Ready for planning

<domain>
## Phase Boundary

Users can manage their profiles (display name, avatar, address book) and seamlessly transition from guest to authenticated with preserved cart. The profile lives behind a sidebar-navigated "My Account" page. Guest orders are NOT linked to accounts — only cart merges on login.

</domain>

<decisions>
## Implementation Decisions

### Profile page layout
- Sidebar navigation with four sections: Profile, Addresses, Orders, Security
- Profile info displayed in view mode with an "Edit" button to switch to editable form
- Security section links to Keycloak account management (not a custom password form)

### Avatar handling
- Click directly on the avatar circle to trigger file upload (no separate button)
- No crop UI — server-side auto-crop to square from center and resize
- Default placeholder: generic silhouette icon (no initials)
- Remove button available after uploading — reverts to silhouette placeholder
- Images stored in Azure Blob Storage (per success criteria)

### Address book behavior
- Modal/dialog form for adding and editing addresses
- No limit on number of saved addresses
- Star icon / "Set as default" toggle on each address card to mark default
- Delete with confirmation prompt

### Guest-to-auth migration
- Silent cart merge on login — guest cart items add to user's existing cart, quantities combine for same products
- Guest orders are NOT linked to authenticated accounts (no email matching)
- Login/register available via header account icon AND at checkout
- After login, user stays on the current page (no redirect)

### Claude's Discretion
- Order history display format (compact list vs cards)
- Address form fields (standard fixed vs country-adaptive)
- Exact loading states and error handling
- Typography, spacing, and visual polish
- Security section implementation details

</decisions>

<specifics>
## Specific Ideas

No specific requirements — open to standard approaches that fit the existing storefront design.

</specifics>

<deferred>
## Deferred Ideas

None — discussion stayed within phase scope

</deferred>

---

*Phase: 11-user-profiles-auth-flow*
*Context gathered: 2026-02-13*
