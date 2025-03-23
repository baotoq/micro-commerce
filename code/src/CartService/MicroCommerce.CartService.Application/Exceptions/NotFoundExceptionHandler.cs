using Ardalis.GuardClauses;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MicroCommerce.CartService.Application.Exceptions;

public class NotFoundExceptionHandler : IExceptionHandler
{
    private readonly IProblemDetailsService _problemDetailsService;

    public NotFoundExceptionHandler(IProblemDetailsService problemDetailsService)
    {
        _problemDetailsService = problemDetailsService;
    }

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is not NotFoundException notFoundException)
        {
            return false;
        }

        httpContext.Response.StatusCode = StatusCodes.Status404NotFound;

        var problemDetails = new ValidationProblemDetails
        {
            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.4",
            Status = StatusCodes.Status404NotFound,
            Title = notFoundException.Message
        };

        await _problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = problemDetails
        });

        return true;
    }
}
