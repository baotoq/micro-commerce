using Elastic.Clients.Elasticsearch;
using MassTransit;
using MicroCommerce.ApiService.Domain.Events;
using MicroCommerce.ApiService.Infrastructure;
using MicroCommerce.ApiService.Services;
using MicroCommerce.ApiService.Services.Elasticsearch;

namespace MicroCommerce.ApiService.Features.DomainEvents;

public record IndexProductDomainEvent : DomainEventBase
{
    public required Guid ProductId { get; init; }
}

public class IndexProductConsumer(ILogger<IndexProductConsumer> logger, ElasticsearchClient elasticsearchClient, ApplicationDbContext context) : IConsumer<IndexProductDomainEvent>
{
    private readonly ILogger<IndexProductConsumer> _logger = logger;
    private readonly ElasticsearchClient _elasticsearchClient = elasticsearchClient;
    private readonly ApplicationDbContext _context = context;

    public async Task Consume(ConsumeContext<IndexProductDomainEvent> context)
    {
        var @event = context.Message;

        _logger.LogInformation("Indexing product {ProductId}", @event.ProductId);

        var product = await _context.Products.FindAsync(@event.ProductId);

        if (product == null)
        {
            _logger.LogWarning("Product {ProductId} not found", @event.ProductId);
            return;
        }

        var result = await _elasticsearchClient.IndexAsync(ProductDocument.FromDomain(product), index: ProductDocument.IndexKey);

        if (!result.IsSuccess())
        {
            _logger.LogError("Failed to index product {ProductId} {Info}", @event.ProductId, result.DebugInformation);
            return;
        }

        _logger.LogInformation("Product {ProductId} indexed successfully", @event.ProductId);
    }
}
