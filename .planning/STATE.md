# Project State

## Project Reference

See: .planning/PROJECT.md (updated 2026-02-25)

**Core value:** A user can complete a purchase end-to-end
**Current focus:** Planning next milestone

## Current Position

Milestone: v2.0 DDD Foundation — SHIPPED 2026-02-25
Status: All 3 milestones complete (v1.0, v1.1, v2.0). 26/26 v2.0 requirements satisfied.
Last activity: 2026-02-25 — Completed v2.0 milestone: 9 phases, 18 plans, 182 tests passing.

Progress: [■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■] 100% (90/90 plans completed)

## Performance Metrics

**Velocity:**
- Total plans completed: 80 (v1.0: 49, v1.1: 23, v2.0: 8)
- Average duration: 22 min
- Total execution time: 28.47 hours

**By Milestone:**

| Milestone | Phases | Plans | Duration | Timeline |
|-----------|--------|-------|----------|----------|
| v1.0 MVP | 10 | 49 | 18.8h | 16 days |
| v1.1 User Features | 7 | 23 | 8.8h | 2 days |
| v2.0 DDD Foundation | 7 | 9 | 40 min | In progress |

**Recent Trend:**
- Last 5 plans: v2.0 foundation infrastructure (18-01, 18-02, 19-01, 19-02)
- Trend: DDD foundation layer + EF Core conventions complete, Vogen adoption complete, FluentResults Result pattern complete (Phase 17), SmartEnum full migration complete (Phase 18), Ardalis.Specification pattern complete (Phase 19)

*Updated after 18-02 completion*
| Phase 15 P02 | 2 | 2 tasks | 7 files |
| Phase 16-conventions-dry-configuration P01 | 7 | 2 tasks | 14 files |
| Phase 16 P02 | 13 | 2 tasks | 45 files |
| Phase 16.1-adopt-vogen-for-value-object P01 | 4 | 2 tasks | 43 files |
| Phase 16.1-adopt-vogen-for-value-object P02 | 4 | 1 task | 16 files |
| Phase 17-result-pattern P01 | 3 | 2 tasks | 5 files |
| Phase 17-result-pattern P02 | 4 | 2 tasks | 6 files |
| Phase 18-enumeration-enums-with-behavior P01 | 3 | 2 tasks | 13 files |
| Phase 18-enumeration-enums-with-behavior P02 | 3 | 2 tasks | 6 files |
| Phase 19-specification-pattern P01 | 3 | 2 tasks | 9 files |
| Phase 19-specification-pattern P02 | 2 | 2 tasks | 4 files |

## Accumulated Context

### Decisions

Decisions are logged in PROJECT.md Key Decisions table.

Recent decisions affecting v2.0:
- **Readonly record structs for value objects**: 20x faster equality, adopted in v1.1
- **Vernon's aggregate rules**: Audit standard for DDD compliance
- **Reference by identity only**: Proper aggregate isolation enforced
- **Entity reference equality** (15-01): Entity<TId> uses reference equality only, no custom Equals/GetHashCode
- **Settable marker interfaces** (15-01): IAuditable/IConcurrencyToken/ISoftDeletable use { get; set; } for interceptor modification
- **IAuditable timestamps only** (15-01): CreatedAt/UpdatedAt only for now, extensible to user tracking later
- **IConcurrencyToken uses int** (15-01): int Version for portability and simplicity over uint
- [Phase 15-02]: Interceptor order: SoftDelete, Concurrency, Audit, DomainEvent (critical for correct behavior)
- [Phase 15-02]: SoftDeleteInterceptor also updates IAuditable.UpdatedAt on soft delete
- [Phase 15-02]: DbUpdateConcurrencyException returns HTTP 409 with entity type name
- [Phase 16-01]: IModelFinalizingConvention over static ModelBuilder extension methods for proper EF Core convention API
- [Phase 16-01]: BaseDbContext abstract class as single registration point for all EF Core conventions and snake_case naming
- [Phase 16]: StronglyTypedIdConvention fix: use typed generic method CreateTypedConverter<TId,TUnderlying> for ValueConverter constructor compatibility
- [Phase 16]: UseSnakeCaseNamingConvention moved to Program.cs configureDbContextOptions (not OnConfiguring) for DbContext pooling compatibility
- [Phase 16]: CheckoutState IsRowVersion and ToTable kept explicit (MassTransit saga - not IConcurrencyToken)
- [Phase 16.1-01]: Vogen 8.0.4 partial record structs with [ValueObject<Guid>] replace hand-rolled StronglyTypedId<T> class hierarchy
- [Phase 16.1-01]: VogenEfCoreConverters marker class with [EfCoreConverter<T>] attributes replaces reflection-based StronglyTypedIdConvention
- [Phase 16.1-01]: EventId uses Conversions.SystemTextJson only (no EfCoreValueConverter) as it is not an EF entity property
- [Phase 16.1-02]: VogenIds migrations have empty Up()/Down() for all 8 contexts — RegisterAllInVogenEfCoreConverters() is applied at runtime via ConfigureConventions and is not recorded in model snapshots
- [Phase 16.1-02]: Model snapshots unchanged after Vogen adoption — convention-based converters have no static snapshot representation
- [Phase 17-result-pattern]: FluentResults 4.0.0 in BuildingBlocks.Common, ResultValidationBehavior with IResultBase constraint, ToHttpResult maps success to 204 and failure to 422, Result vs Exception boundary documented in ADR-006
- [Phase 17-02]: Domain methods keep throwing InvalidOperationException — handlers catch and convert to Result.Fail (adapter pattern at handler boundary)
- [Phase 17-02]: UpdateOrderStatus/AdjustStock now return 422 Unprocessable Entity for business rule violations instead of 500 from uncaught InvalidOperationException
- [Phase 18-01]: SmartEnumStringConverter stores by Name (not Value) to preserve existing string-based DB schema — do NOT use Ardalis.SmartEnum.EFCore ConfigureSmartEnum() which stores by int Value
- [Phase 18-01]: Per-type HaveConversion<SmartEnumStringConverter<T>>() registration in BaseDbContext ConfigureConventions (simpler than IModelFinalizingConvention for 2 types)
- [Phase 18-01]: SmartEnum types are abstract (not sealed) to allow subclassing per CONTEXT.md
- [Phase 18-01]: HasConversion<string>() removed from entity configurations — base convention handles it
- [Phase 18-02]: Product entity methods keep idempotent same-state guard before TransitionTo() — callers may invoke without pre-checking state; silent no-op preferred over throwing for same-state calls
- [Phase 18-02]: Order entity methods have no idempotent guard — state changes are always meaningful and should not repeat
- [Phase 18-02]: ChangeProductStatusCommandHandler uses switch on ToLowerInvariant() over SmartEnum.TryFromName — validator already guarantees valid input
- [Phase 19-specification-pattern]: Ardalis.Specification 9.3.1 does not expose And() — composite spec with multiple Query.Where() calls achieves equivalent AND semantics
- [Phase 19-specification-pattern]: Sorting kept in handler (request-specific SortBy/SortDirection params); pagination kept in handler for reusable count queries
- [Phase 19-02]: Spec composition via chained WithSpecification() calls (buyer spec then active orders spec) — equivalent to And() without the naming conflict
- [Phase 20-01]: MigrateAsync per DbContext over EnsureCreated for multi-schema shared PostgreSQL: EnsureCreated skips if ANY tables exist; MigrateAsync uses per-schema __EFMigrationsHistory tables independently
- [Phase 20-01]: Per-class schema reset uses DROP SCHEMA IF EXISTS CASCADE + MigrateAsync, not EnsureDeletedAsync (which drops entire database)
- [Phase 20-01]: ReplaceDbContext helper requires UseSnakeCaseNamingConvention() to match production BaseDbContext conventions
- [Phase 20-01]: MassTransit health check dedup via PostConfigure<HealthCheckServiceOptions> (pre-removal does not work since AddMassTransitTestHarness re-adds after removal)
- [Phase 20-01]: SmartEnum needs [JsonConverter] at class level for client-side HttpClient GetFromJsonAsync deserialization
- [Phase 20-01]: StockItem.Reserve throws ConflictException (409) not InvalidOperationException (400) for insufficient stock
- [Phase 20-01]: GetOrderDashboard groups timestamps in memory (EF cannot translate DateTimeOffset.Date + GroupBy + DateOnly.FromDateTime to SQL)
- [Phase 20-02]: UpdateOrderStatusHandlerTests resolves IMediator from DI (not direct handler instantiation) for full DI pipeline
- [Phase 20-02]: Handle_NonExistentOrder_ThrowsNotFoundException because handler throws NotFoundException (not Result.Fail) for missing orders — exception middleware converts to 404 at endpoint boundary
- [Phase 20-02]: ReviewsEndpoints.GetProductReviews page/pageSize params need defaults (same pattern as CatalogEndpoints fixed in 20-01)
- [Phase 21-01]: Entity<TId> EF Core constructors must be private (not protected) in sealed classes — protected members in sealed types cause CS0628 compile error
- [Phase 21-01]: Order kept as BaseAggregateRoot<OrderId> — domain-specific CreatedAt/PaidAt timestamps more meaningful than generic UpdatedAt for order lifecycle
- [Phase 21-01]: Cart.LastModifiedAt renamed to inherited UpdatedAt from AuditableAggregateRoot — requires EF migration (column rename) and test updates
- [Phase 21-01]: Unit test timestamp assertions removed for migrated aggregates — AuditInterceptor handles CreatedAt/UpdatedAt at infrastructure layer, not domain layer
- [Phase 21-02]: xmin to version migration uses AddColumn (not RenameColumn): PostgreSQL system column cannot be renamed; EF Core auto-generates incorrect RenameColumn that must be manually corrected to AddColumn
- [Phase 21-02]: WishlistItem gains Entity<WishlistItemId> base class alongside IConcurrencyToken — consistent with ADOPT-01 scope for standalone entities with Id
- [Phase 21-03]: Microsoft.OpenApi 2.0.0 (used by AspNetCore.OpenApi 10.x) uses JsonSchemaType enum for schema.Type (not string), JsonNode/JsonValue.Create() for schema.Enum items (not OpenApiString), types in root Microsoft.OpenApi namespace (not Microsoft.OpenApi.Models)
- [Phase 21-03]: UpdateCartItemCommandHandler uses NotFoundException for missing cart (404 via exception middleware) — consistent with handler boundary pattern (not found = 404, domain rule failure = 422 Result.Fail)
- [Phase 21-03]: VogenIdSchemaTransformer uses dual detection: ValueObjectAttribute name check + IsValueType/Guid Value fallback for robustness against Vogen version differences
- [Phase 22-01]: Stateless interceptors (SoftDelete, Concurrency, Audit) instantiated as singleton `new` before DbContext registrations — shared safely across all 8 DbContexts; DomainEventInterceptor kept AddScoped due to IPublishEndpoint dependency
- [Phase 22-01]: CartEntity type alias pattern resolves namespace conflict when test directory name (Integration/Cart/) collides with imported entity type (Cart)

### Roadmap Evolution

- v1.0 MVP: Phases 1-10 (shipped 2026-02-13)
- v1.1 User Features: Phases 11-14.3 (shipped 2026-02-14)
- v2.0 DDD Foundation: Phases 15-21 (current, roadmap created 2026-02-14)
- Phase 16.1 inserted after Phase 16: Adopt Vogen for value object (URGENT)

### Pending Todos

None.

### Blockers/Concerns

None. All v1.1 issues resolved. Clean slate for v2.0.

## Session Continuity

Last session: 2026-02-25
Stopped at: Completed 22-01-PLAN.md (Interceptors wired to all 8 DbContexts, 5 InterceptorBehaviorTests added, 182 tests green, ENTITY-02/04/05 and ADOPT-03 requirements satisfied)
Resume file: None
Next step: Phase 22 complete. v2.0 DDD Foundation gap closure complete. All requirements satisfied.
