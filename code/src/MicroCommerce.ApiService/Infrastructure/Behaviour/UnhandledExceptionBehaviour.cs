using System.Diagnostics;
using MediatR;
using MicroCommerce.ApiService.Infrastructure.Exceptions;

namespace MicroCommerce.ApiService.Infrastructure.Behaviour;

[DebuggerStepThrough]
public class UnhandledExceptionBehaviour<TRequest, TResponse>(ILoggerFactory logger) : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
{
    private readonly ILogger<TRequest> _logger = logger.CreateLogger<TRequest>();

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        try
        {
            return await next();
        }
        catch (Exception ex) when(ex is not InvalidValidationException)
        {
            var requestName = typeof(TRequest).Name;

            _logger.LogError(ex, "Request: Unhandled Exception for Request {Name} {@Request}", requestName, request);

            throw;
        }
    }
}
