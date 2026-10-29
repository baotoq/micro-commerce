using JasperFx.Core;
using MicroCommerce.ApiService.Features.Catalog.Data;
using MicroCommerce.ApiService.Features.Ordering.Data;
using Wolverine;
using Wolverine.EntityFrameworkCore;
using Wolverine.ErrorHandling;
using Wolverine.Persistence;
using Wolverine.Postgresql;

namespace MicroCommerce.ApiService;

/// <summary>
/// Wolverine carries events between modules (ADR-0004): a transactional outbox and durable local queues, stored in
/// the shared Postgres database. FastEndpoints stays the HTTP layer; Wolverine only handles messages.
/// </summary>
public static class Messaging
{
    /// <summary>The schema of Wolverine's envelope tables, which Wolverine creates and updates on startup.</summary>
    public const string Schema = "wolverine";

    public static void AddMessaging(this WebApplicationBuilder builder) =>
        builder.Host.UseWolverine(opts =>
        {
            opts.PersistMessagesWithPostgresqlUsingNamedConnection("micro-commerce-db", Schema);

            // Lightweight: no explicit transaction, which Aspire's retrying EF Core execution strategy would refuse.
            // Endpoints publish through IDbContextOutbox<T> on a context that maps the envelope tables, so one
            // SaveChangesAsync writes the change and its events together.
            opts.UseEntityFrameworkCoreTransactions(TransactionMiddlewareMode.Lightweight);
            opts.Policies.UseDurableLocalQueues();

            // Aspire registers each DbContext through a pooling factory, which generated handler code can't construct.
            opts.CodeGeneration.AlwaysUseServiceLocationFor<CatalogDbContext>();
            opts.CodeGeneration.AlwaysUseServiceLocationFor<OrderingDbContext>();

            // Handlers are idempotent, so a message that failed (a database or Redis blip) is safe to retry.
            opts.Policies.OnAnyException().RetryWithCooldown(100.Milliseconds(), 1.Seconds(), 5.Seconds());
        });
}
