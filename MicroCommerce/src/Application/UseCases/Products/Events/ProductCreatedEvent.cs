using MassTransit;
using Microsoft.Extensions.Logging;

namespace Application.UseCases.Products.Events;

public record ProductCreatedEvent
{
    public string Id { get; init; } = "";
    public string Name { get; set; } = "";
}

public class ProductCreatedEventConsumer : IConsumer<ProductCreatedEvent>
{
    private readonly ILogger<ProductCreatedEventConsumer> _logger;

    public ProductCreatedEventConsumer(ILogger<ProductCreatedEventConsumer> logger)
    {
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<ProductCreatedEvent> context)
    {
        _logger.LogInformation("Product Created");
    }
}