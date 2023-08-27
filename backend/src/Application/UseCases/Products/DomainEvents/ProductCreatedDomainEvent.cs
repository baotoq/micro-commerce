using Domain;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Application.UseCases.Products.DomainEvents;

public record ProductCreatedDomainEvent : DomainEventBase
{
    public string Id { get; init; } = "";
    public string Name { get; init; } = "";
}

public class ProductCreatedDomainEventConsumer : IConsumer<ProductCreatedDomainEvent>
{
    private readonly ILogger<ProductCreatedDomainEventConsumer> _logger;

    public ProductCreatedDomainEventConsumer(ILogger<ProductCreatedDomainEventConsumer> logger)
    {
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<ProductCreatedDomainEvent> context)
    {
        _logger.LogInformation("Product Created");
    }
}