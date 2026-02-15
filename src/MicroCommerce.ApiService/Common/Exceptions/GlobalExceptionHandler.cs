using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
            DbUpdateConcurrencyException dbConcurrencyEx => (
                StatusCodes.Status409Conflict,
                "Concurrency Conflict",
                $"The resource was modified by another request. {FormatConcurrencyDetail(dbConcurrencyEx)}"),
            InvalidOperationException invalidOpException => (
                StatusCodes.Status400BadRequest,
                "Invalid Operation",
                invalidOpException.Message),
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

    private static string FormatConcurrencyDetail(DbUpdateConcurrencyException ex)
    {
        var entry = ex.Entries.FirstOrDefault();
        if (entry is not null)
        {
            return $"Entity: {entry.Entity.GetType().Name}. Please refresh and retry.";
        }
        return "Please refresh and retry.";
    }
}
