using Domain;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Application.UseCases.Carts.DomainEvents;

public record ProductAddedToCartDomainEvent : DomainEventBase
{
    public string CartId { get; set; } = "";
    public string ProductId { get; set; } = "";
    public int Quantities { get; set; }
}

public class ProductAddedToCartDomainEventConsumer : IConsumer<ProductAddedToCartDomainEvent>
{
    private readonly ILogger<ProductAddedToCartDomainEventConsumer> _logger;

    public ProductAddedToCartDomainEventConsumer(ILogger<ProductAddedToCartDomainEventConsumer> logger)
    {
        _logger = logger;
    }

    public Task Consume(ConsumeContext<ProductAddedToCartDomainEvent> context)
    {
        _logger.LogInformation("Consumed ProductAddedToCartDomainEvent");
        
        return Task.CompletedTask;
    }
}