using MediatR;

namespace MicroCommerce.ApiService.Infrastructure.Behaviour;

public class UnhandledExceptionBehaviour<TRequest, TResponse>(ILoggerFactory logger) : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
{
    private readonly ILogger<TRequest> _logger = logger.CreateLogger<TRequest>();

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        try
        {
            return await next();
        }
        catch (Exception ex)
        {
            var requestName = typeof(TRequest).Name;

            _logger.LogError(ex, "Request: Unhandled Exception for Request {Name} {@Request}", requestName, request);

            throw;
        }
    }
}