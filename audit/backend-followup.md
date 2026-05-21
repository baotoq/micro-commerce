# Catalog Backend — Deferred Follow-up Work

Tracker for items the audit-fix pass intentionally stubbed instead of fully implementing.

## Transactional Outbox (audit#9)

**Status:** Stubbed (try/catch + log warning).

**Current state.** The three Dapr notification handlers
(`ProductCreated/Updated/DeletedEventHandler`) now wrap `dapr.PublishEventAsync`
in a `try/catch (Exception ex) { logger.LogWarning(...); }`. A Dapr publish
failure no longer surfaces as a 500 to the API caller, but the catalog and any
downstream consumer can still diverge silently if Dapr is down for an extended
period (the event is dropped on the floor after the DB row is committed).

For Delete in particular, the row is already gone by the time the publish
fails — there is no compensating record we can resend from.

**What's needed.**

1. Add an `Outbox` table to the catalog DB:
   ```
   id (uuid pk)
   event_type (text)             -- e.g. "catalog.product.created"
   payload (jsonb)               -- serialized event
   occurred_at (timestamptz)
   published_at (timestamptz nullable)
   attempts (int)
   ```
2. In each write handler, instead of calling `publisher.Publish(event)` directly
   after `SaveChangesAsync`, write an `OutboxEntry` row *in the same
   transaction* as the Product write. (Either wrap in an explicit
   `db.Database.BeginTransactionAsync()` or rely on the single
   `SaveChangesAsync` saving both entities atomically.)
3. Run a background dispatcher (`BackgroundService` or scheduled task) that
   polls unpublished rows, calls `dapr.PublishEventAsync`, sets `published_at`
   on success, and increments `attempts` on failure (with exponential backoff
   + alerting after N attempts).
4. The current INotificationHandler-based fan-out becomes the in-process
   side-effect path (cache invalidation, etc.). Dapr is no longer driven
   directly from there.

**Why deferred.** Requires a schema change + a new background service + a
poll-with-backoff dispatcher + retry/poison-message semantics. Not appropriate
to land inside a "fix the 10 audit findings" pass — needs its own design
review (e.g. consider Dapr's outbox feature on the state-store side instead
of rolling our own).

**Related files** (worktree `audit-fix/dotnet-backend`):

- `src/Services/Catalog.API/src/Application/Products/Commands/CreateProduct.cs`
- `src/Services/Catalog.API/src/Application/Products/Commands/UpdateProduct.cs`
- `src/Services/Catalog.API/src/Application/Products/Commands/DeleteProduct.cs`
- `src/Services/Catalog.API/src/Api/EventHandlers/Product*EventHandler.cs`
