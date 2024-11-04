using Elastic.Clients.Elasticsearch;
using MassTransit;
using MicroCommerce.ApiService.Infrastructure;
using MicroCommerce.ApiService.Infrastructure.Interceptors;
using Microsoft.EntityFrameworkCore;

namespace MicroCommerce.ApiService.UseCases.Products.DomainEvents;

public class IndexProductDomainEventConsumer : IConsumer<IndexProductDomainEvent>
{
    private readonly ApplicationDbContext _context;
    private readonly ElasticsearchClient _elasticsearchClient;
    private readonly ILogger<IndexProductDomainEventConsumer> _logger;

    public IndexProductDomainEventConsumer(ApplicationDbContext context, ElasticsearchClient elasticsearchClient, ILogger<IndexProductDomainEventConsumer> logger)
    {
        _context = context;
        _elasticsearchClient = elasticsearchClient;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<IndexProductDomainEvent> context)
    {
        var message = context.Message;
        
        var product = await _context.Products
            .FirstOrDefaultAsync(x => x.Id == message.ProductId);

        if (product == null)
        {
            return;
        }

        var response = await _elasticsearchClient.IndexAsync(new ProductDocument
        {
            Id = product.Id,
            Name = product.Name,
            Price = product.Price
        });

        if (!response.IsValidResponse)
        {
            _logger.LogError("Index product [{Id}] failed", product.Id);
        }
        else
        {
            _logger.LogInformation("Index product [{Id}] successfully", product.Id);

        }
    }
}