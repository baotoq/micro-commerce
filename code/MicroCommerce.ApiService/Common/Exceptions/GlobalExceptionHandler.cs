using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace MicroCommerce.ApiService.Common.Exceptions;

/// <summary>
/// Global exception handler that maps domain exceptions to appropriate HTTP responses.
/// Implements IExceptionHandler for .NET 8+ exception handling pipeline.
/// </summary>
public sealed class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        _logger.LogError(exception, "Exception occurred: {Message}", exception.Message);

        (int statusCode, string title, string detail) = exception switch
        {
            ValidationException => (
                StatusCodes.Status400BadRequest,
                "Validation error",
                "One or more validation errors occurred."),
            NotFoundException notFoundException => (
                StatusCodes.Status404NotFound,
                "Not Found",
                notFoundException.Message),
            ConflictException conflictException => (
                StatusCodes.Status409Conflict,
                "Conflict",
                conflictException.Message),
            _ => (0, string.Empty, string.Empty)
        };

        if (statusCode == 0)
        {
            // Let the default exception handler deal with it
            return false;
        }

        ProblemDetails problemDetails;
        if (exception is ValidationException validationEx)
        {
            problemDetails = new ValidationProblemDetails(validationEx.Errors)
            {
                Status = statusCode, Title = title, Detail = detail, Instance = httpContext.Request.Path
            };
        }
        else
        {
            problemDetails = new ProblemDetails
            {
                Status = statusCode, Title = title, Detail = detail, Instance = httpContext.Request.Path
            };
        }

        httpContext.Response.StatusCode = problemDetails.Status!.Value;
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }
}
