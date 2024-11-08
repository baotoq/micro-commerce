using MediatR.Pipeline;

namespace MicroCommerce.ApiService.Infrastructure.Behaviour;

public class LoggingBehaviour<TRequest>(ILoggerFactory logger) : IRequestPreProcessor<TRequest>
    where TRequest : notnull
{
    private readonly ILogger<TRequest> _logger = logger.CreateLogger<TRequest>();

    public Task Process(TRequest request, CancellationToken cancellationToken)
    {
        using var _ = _logger.BeginScope("CorrelationId: {CorrelationId}", Guid.NewGuid());

        var requestName = typeof(TRequest).Name;

        _logger.LogInformation("Request: {Name} {@Request}", requestName, request);

        return Task.CompletedTask;
    }
}
