---
phase: 11-user-profiles-auth-flow
plan: 01
subsystem: profiles
tags: [domain-model, infrastructure, image-processing, entity-framework]
dependency_graph:
  requires: [base-aggregate-root, value-object, strongly-typed-id, blob-storage]
  provides: [user-profile-aggregate, avatar-processing, profiles-dbcontext]
  affects: [program-di, ef-migrations]
tech_stack:
  added:
    - SixLabors.ImageSharp 3.1.6
  patterns:
    - DDD Aggregate Root
    - Owned Entity Collections
    - Value Objects with Validation
    - EF Core Schema Isolation
    - Image Processing Pipeline
key_files:
  created:
    - src/MicroCommerce.ApiService/Features/Profiles/Domain/Entities/UserProfile.cs
    - src/MicroCommerce.ApiService/Features/Profiles/Domain/ValueObjects/UserProfileId.cs
    - src/MicroCommerce.ApiService/Features/Profiles/Domain/ValueObjects/AddressId.cs
    - src/MicroCommerce.ApiService/Features/Profiles/Domain/ValueObjects/DisplayName.cs
    - src/MicroCommerce.ApiService/Features/Profiles/Domain/ValueObjects/Address.cs
    - src/MicroCommerce.ApiService/Features/Profiles/Domain/Events/ProfileCreatedDomainEvent.cs
    - src/MicroCommerce.ApiService/Features/Profiles/Domain/Events/ProfileUpdatedDomainEvent.cs
    - src/MicroCommerce.ApiService/Features/Profiles/Infrastructure/ProfilesDbContext.cs
    - src/MicroCommerce.ApiService/Features/Profiles/Infrastructure/Configurations/UserProfileConfiguration.cs
    - src/MicroCommerce.ApiService/Features/Profiles/Infrastructure/AvatarImageService.cs
    - src/MicroCommerce.ApiService/Migrations/20260213082150_InitialProfiles.cs
  modified:
    - src/MicroCommerce.ApiService/Program.cs
    - src/MicroCommerce.ApiService/MicroCommerce.ApiService.csproj
decisions:
  - choice: "ImageSharp 3.1.6 for avatar processing"
    rationale: "Industry-standard image library with crop and resize capabilities, matches existing blob storage patterns"
    alternatives: ["SkiaSharp", "System.Drawing"]
  - choice: "Address as owned entity collection with single-default invariant"
    rationale: "Follows DDD pattern for lifecycle management, ensures exactly one default address through aggregate"
    alternatives: ["Separate Address aggregate", "Address table with foreign key"]
  - choice: "PostgreSQL xmin for optimistic concurrency"
    rationale: "Consistent with other aggregates (Cart, Product), leverages PostgreSQL system column"
    alternatives: ["Manual version column", "Timestamp column"]
metrics:
  duration_minutes: 4
  tasks_completed: 2
  files_created: 13
  files_modified: 2
  commits: 2
  completed_date: 2026-02-13
---

# Phase 11 Plan 01: Profiles Backend Foundation Summary

**One-liner:** UserProfile aggregate with display name, avatar processing (ImageSharp crop/resize to 400x400), and address book with single-default enforcement in 'profiles' schema.

## What Was Built

Created the complete Profiles backend foundation with:

1. **Domain Model (DDD)**
   - UserProfile aggregate root managing display name, avatar URL, and address collection
   - Strongly-typed IDs: UserProfileId, AddressId
   - DisplayName value object with validation (2-50 characters, trimmed)
   - Address value object with default flag and equality based on physical location
   - Domain events: ProfileCreatedDomainEvent, ProfileUpdatedDomainEvent
   - Single-default address invariant enforced at aggregate level

2. **Infrastructure**
   - ProfilesDbContext with 'profiles' schema isolation
   - UserProfileConfiguration with EF Core mappings:
     - Unique index on UserId (Keycloak sub claim)
     - OwnsOne for DisplayName value object
     - OwnsMany for Address collection with backing field
     - PostgreSQL xmin for optimistic concurrency
   - AvatarImageService implementing IAvatarImageService:
     - Image loading with ImageSharp
     - Crop to square from center (min dimension)
     - Resize to 400x400 pixels
     - Upload to 'avatars' blob container as JPEG
     - Delete avatar support with error tolerance
   - EF migration: InitialProfiles (UserProfiles + Addresses tables in profiles schema)

3. **Integration**
   - ProfilesDbContext registered with Aspire PostgreSQL integration
   - IAvatarImageService registered as scoped service
   - SixLabors.ImageSharp 3.1.6 package installed

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 1 - Bug] Fixed incorrect navigation configuration in UserProfileConfiguration**
- **Found during:** Task 2 - EF migration generation
- **Issue:** `address.Navigation("Addresses")` referenced wrong scope - tried to configure navigation on owned entity instead of owner
- **Fix:** Moved navigation configuration outside OwnsMany lambda: `builder.Navigation(p => p.Addresses).UsePropertyAccessMode(PropertyAccessMode.Field)`
- **Files modified:** UserProfileConfiguration.cs
- **Commit:** 564baab8 (included in Task 2 commit)

**Reasoning:** This was a bug (Rule 1) because the code prevented EF migration generation with error "Navigation 'Address.Addresses' was not found." The fix was required to make the entity configuration work correctly - a correctness issue, not an architectural choice.

## Verification Results

All success criteria met:

- UserProfile aggregate root with full DDD pattern (factory, domain events, invariants) ✓
- Address owned entity collection with single-default enforcement ✓
- ProfilesDbContext with schema isolation and entity configurations ✓
- AvatarImageService with ImageSharp crop/resize/upload pipeline ✓
- All registered in Program.cs DI container ✓
- Project compiles cleanly (0 errors, 4 NuGet warnings about ImageSharp known vulnerabilities - acceptable) ✓

## Key Implementation Details

**UserProfile Aggregate Invariants:**
- UserId must be unique (enforced via DB index)
- DisplayName must be 2-50 characters (enforced via value object)
- At most one address can be default (enforced via aggregate methods)
- First address automatically becomes default
- Deleting default address promotes first remaining address

**Avatar Processing Pipeline:**
1. Load image from stream using ImageSharp
2. Calculate minimum dimension (width or height)
3. Calculate crop offsets to center square
4. Crop to square
5. Resize to 400x400
6. Save as JPEG to memory stream
7. Upload to Azure Blob Storage 'avatars' container
8. Return blob URI

**Address Value Object Equality:**
- Based on physical location: Street, City, State, ZipCode, Country
- Name and IsDefault excluded from equality (labels, not identity)

## Files Modified

**Created (13):**
- Domain: UserProfile.cs, UserProfileId.cs, AddressId.cs, DisplayName.cs, Address.cs
- Events: ProfileCreatedDomainEvent.cs, ProfileUpdatedDomainEvent.cs
- Infrastructure: ProfilesDbContext.cs, UserProfileConfiguration.cs, AvatarImageService.cs
- Migrations: 20260213082150_InitialProfiles.cs, InitialProfiles.Designer.cs, ProfilesDbContextModelSnapshot.cs

**Modified (2):**
- Program.cs: Added ProfilesDbContext registration, IAvatarImageService DI
- MicroCommerce.ApiService.csproj: Added SixLabors.ImageSharp 3.1.6 package reference

## Task Breakdown

| Task | Description | Commit | Status |
|------|-------------|--------|--------|
| 1 | Create Profiles domain model (aggregate, value objects, events) | 89ffbb38 | ✓ Complete |
| 2 | Create ProfilesDbContext, EF configurations, AvatarImageService, register in Program.cs | 564baab8 | ✓ Complete |

## Next Steps

This plan establishes the Profiles domain foundation. Subsequent plans in Phase 11 will:
- Add CQRS commands/queries for profile operations (GetProfile, UpdateProfile, ManageAddresses)
- Add ProfilesEndpoints for REST API exposure
- Integrate avatar upload with multipart form handling
- Create frontend components for profile management
- Implement guest-to-authenticated user migration for cart/orders

## Self-Check: PASSED

**Created files verified:**
- UserProfile.cs: FOUND
- UserProfileId.cs: FOUND
- AddressId.cs: FOUND
- DisplayName.cs: FOUND
- Address.cs: FOUND
- ProfileCreatedDomainEvent.cs: FOUND
- ProfileUpdatedDomainEvent.cs: FOUND
- ProfilesDbContext.cs: FOUND
- UserProfileConfiguration.cs: FOUND
- AvatarImageService.cs: FOUND

**Commits verified:**
- 89ffbb38 (Task 1 - domain model): FOUND
- 564baab8 (Task 2 - infrastructure): FOUND

**Build verification:**
- `dotnet build` completed with 0 errors: PASSED
- ImageSharp package reference in .csproj: FOUND
- ProfilesDbContext registration in Program.cs: FOUND
- IAvatarImageService registration in Program.cs: FOUND
