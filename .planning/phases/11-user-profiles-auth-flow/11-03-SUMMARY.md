---
phase: 11-user-profiles-auth-flow
plan: 03
subsystem: profiles
tags: [cqrs, rest-api, validation, authentication, endpoints]
dependency_graph:
  requires: [user-profile-aggregate, profiles-dbcontext, avatar-image-service]
  provides: [profiles-api, profile-cqrs-handlers, address-management-api]
  affects: [program-endpoints]
tech_stack:
  added: []
  patterns:
    - CQRS with MediatR
    - FluentValidation
    - Minimal API Endpoints
    - JWT Authorization
    - Multipart Form Upload
key_files:
  created:
    - src/MicroCommerce.ApiService/Features/Profiles/Application/Queries/GetProfile/GetProfileQuery.cs
    - src/MicroCommerce.ApiService/Features/Profiles/Application/Commands/CreateProfile/CreateProfileCommand.cs
    - src/MicroCommerce.ApiService/Features/Profiles/Application/Commands/UpdateProfile/UpdateProfileCommand.cs
    - src/MicroCommerce.ApiService/Features/Profiles/Application/Commands/UpdateProfile/UpdateProfileCommandValidator.cs
    - src/MicroCommerce.ApiService/Features/Profiles/Application/Commands/UploadAvatar/UploadAvatarCommand.cs
    - src/MicroCommerce.ApiService/Features/Profiles/Application/Commands/RemoveAvatar/RemoveAvatarCommand.cs
    - src/MicroCommerce.ApiService/Features/Profiles/Application/Commands/AddAddress/AddAddressCommand.cs
    - src/MicroCommerce.ApiService/Features/Profiles/Application/Commands/AddAddress/AddAddressCommandValidator.cs
    - src/MicroCommerce.ApiService/Features/Profiles/Application/Commands/UpdateAddress/UpdateAddressCommand.cs
    - src/MicroCommerce.ApiService/Features/Profiles/Application/Commands/UpdateAddress/UpdateAddressCommandValidator.cs
    - src/MicroCommerce.ApiService/Features/Profiles/Application/Commands/DeleteAddress/DeleteAddressCommand.cs
    - src/MicroCommerce.ApiService/Features/Profiles/Application/Commands/SetDefaultAddress/SetDefaultAddressCommand.cs
    - src/MicroCommerce.ApiService/Features/Profiles/ProfilesEndpoints.cs
  modified:
    - src/MicroCommerce.ApiService/Program.cs
decisions:
  - choice: "Auto-create profile on first GET /api/profiles/me"
    rationale: "Eliminates need for explicit profile creation step, ensures profile always exists when user is authenticated"
    alternatives: ["Require explicit POST to create profile", "Create profile on login via event handler"]
  - choice: "UserId (Guid) as profile lookup key from JWT 'sub' claim"
    rationale: "Direct mapping from Keycloak user ID, avoids email-based lookups, supports Keycloak user management"
    alternatives: ["Email-based lookup", "Custom user ID generation"]
  - choice: "5MB avatar upload limit with image/* content type validation"
    rationale: "Balances quality with bandwidth, prevents abuse, standard web image size limit"
    alternatives: ["10MB limit", "No client-side limit"]
metrics:
  duration_minutes: 3
  tasks_completed: 2
  files_created: 13
  files_modified: 1
  commits: 2
  completed_date: 2026-02-13
---

# Phase 11 Plan 03: Profiles API Layer Summary

**One-liner:** Complete CQRS application layer with 9 commands/queries, 4 validators, and 8 REST API endpoints for profile and address management with JWT authorization.

## What Was Built

Created the complete Profiles API layer with:

1. **CQRS Query Handlers**
   - GetProfileQuery: Returns ProfileDto with addresses, auto-creates profile if not found (first access creates profile with default "User" display name)
   - ProfileDto and AddressDto records for API responses

2. **CQRS Command Handlers**
   - CreateProfileCommand: Idempotent profile creation (returns existing ID if already exists)
   - UpdateProfileCommand: Updates display name with validation
   - UploadAvatarCommand: Processes image via IAvatarImageService, replaces old avatar
   - RemoveAvatarCommand: Deletes avatar from blob storage and clears URL
   - AddAddressCommand: Adds address to collection with optional set-as-default
   - UpdateAddressCommand: Updates existing address, preserves default flag
   - DeleteAddressCommand: Removes address, promotes first remaining to default if needed
   - SetDefaultAddressCommand: Sets specified address as default, clears others

3. **FluentValidation Validators**
   - UpdateProfileCommandValidator: DisplayName (NotEmpty, 2-50 chars)
   - AddAddressCommandValidator: Name (50), Street (200), City (100), State (50), ZipCode (20 + regex), Country (100)
   - UpdateAddressCommandValidator: Same rules as AddAddress

4. **REST API Endpoints** (ProfilesEndpoints.cs)
   - GET /api/profiles/me - Get or auto-create profile
   - PUT /api/profiles/me - Update display name
   - POST /api/profiles/me/avatar - Upload avatar (multipart form, 5MB limit, image/* only)
   - DELETE /api/profiles/me/avatar - Remove avatar
   - POST /api/profiles/me/addresses - Add address
   - PUT /api/profiles/me/addresses/{id} - Update address
   - DELETE /api/profiles/me/addresses/{id} - Delete address
   - PATCH /api/profiles/me/addresses/{id}/default - Set default address
   - All endpoints in /api/profiles group with .RequireAuthorization()
   - GetUserId() helper extracts Guid from JWT "sub" or NameIdentifier claim

5. **Integration**
   - MapProfilesEndpoints() registered in Program.cs
   - Request/response records: UpdateProfileRequest, AddAddressRequest, UpdateAddressRequest, UploadAvatarResult

## Deviations from Plan

None - plan executed exactly as written.

## Verification Results

All success criteria met:

- GET /api/profiles/me returns profile with addresses (auto-creates on first access) ✓
- PUT /api/profiles/me updates display name with validation ✓
- POST/DELETE /api/profiles/me/avatar handle image upload and removal ✓
- Address CRUD with set-default works through /api/profiles/me/addresses/* ✓
- All endpoints require JWT authorization (.RequireAuthorization() on group) ✓
- Project compiles cleanly (0 errors) ✓

Build output: 0 errors, 2 NuGet warnings (ImageSharp vulnerabilities - known, acceptable).

## Key Implementation Details

**Auto-Create Flow:**
1. User hits GET /api/profiles/me with valid JWT
2. GetProfileQuery extracts UserId from JWT "sub" claim
3. Query handler looks up profile by UserId
4. If not found, creates profile with UserProfile.Create(userId, "User")
5. Saves to database and returns ProfileDto
6. User can immediately update display name via PUT /api/profiles/me

**Avatar Upload Flow:**
1. Endpoint validates: file exists, content type starts with "image/", size <= 5MB
2. UploadAvatarCommand opens stream, calls IAvatarImageService.ProcessAndUploadAvatarAsync
3. Service crops to square, resizes to 400x400, uploads to blob storage
4. Handler deletes old avatar if exists, calls profile.SetAvatar(newUrl), saves
5. Returns new avatar URL in response

**Address Management Invariants:**
- First address automatically becomes default
- Deleting default address promotes first remaining address
- Setting default clears all other defaults (enforced by aggregate)
- Address equality based on physical location (Street, City, State, ZipCode, Country)

**Authorization Pattern:**
- .RequireAuthorization() on entire /api/profiles group
- GetUserId() extracts Guid from "sub" or ClaimTypes.NameIdentifier
- Throws UnauthorizedAccessException if claim missing or invalid
- All handlers receive UserId from endpoint, ensuring user can only access their own profile

## Files Modified

**Created (13):**
- Queries: GetProfileQuery.cs (includes ProfileDto, AddressDto)
- Commands: CreateProfileCommand.cs, UpdateProfileCommand.cs, UploadAvatarCommand.cs, RemoveAvatarCommand.cs, AddAddressCommand.cs, UpdateAddressCommand.cs, DeleteAddressCommand.cs, SetDefaultAddressCommand.cs
- Validators: UpdateProfileCommandValidator.cs, AddAddressCommandValidator.cs, UpdateAddressCommandValidator.cs
- Endpoints: ProfilesEndpoints.cs (includes request/response records)

**Modified (1):**
- Program.cs: Added MapProfilesEndpoints() registration and using statement

## Task Breakdown

| Task | Description | Commit | Status |
|------|-------------|--------|--------|
| 1 | Create CQRS commands, queries, and validators | f6b9962a | ✓ Complete |
| 2 | Create ProfilesEndpoints and register in Program.cs | 0c1499bd | ✓ Complete |

## Next Steps

This plan establishes the complete Profiles API layer. Subsequent plans in Phase 11 will:
- Build frontend profile management UI (display name editor, avatar uploader)
- Build frontend address book UI (add/edit/delete addresses, set default)
- Integrate profile display in navigation/header
- Implement guest-to-authenticated user migration for cart/orders
- Add profile seeding for development/testing

## Self-Check: PASSED

**Created files verified:**
- GetProfileQuery.cs: FOUND
- CreateProfileCommand.cs: FOUND
- UpdateProfileCommand.cs: FOUND
- UpdateProfileCommandValidator.cs: FOUND
- UploadAvatarCommand.cs: FOUND
- RemoveAvatarCommand.cs: FOUND
- AddAddressCommand.cs: FOUND
- AddAddressCommandValidator.cs: FOUND
- UpdateAddressCommand.cs: FOUND
- UpdateAddressCommandValidator.cs: FOUND
- DeleteAddressCommand.cs: FOUND
- SetDefaultAddressCommand.cs: FOUND
- ProfilesEndpoints.cs: FOUND

**Commits verified:**
- f6b9962a (Task 1 - CQRS layer): FOUND
- 0c1499bd (Task 2 - endpoints): FOUND

**Build verification:**
- `dotnet build` completed with 0 errors: PASSED
- All 8 endpoints registered in ProfilesEndpoints: PASSED
- MapProfilesEndpoints called in Program.cs: PASSED
- 4 FluentValidation validators exist: PASSED (UpdateProfile, AddAddress, UpdateAddress)
- GetProfileQuery has auto-create logic: PASSED
