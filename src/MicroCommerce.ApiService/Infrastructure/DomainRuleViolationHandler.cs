using MicroCommerce.Domain.SharedKernel;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace MicroCommerce.ApiService.Infrastructure;

/// <summary>
/// Turns a broken domain rule into a <c>422</c> ProblemDetails whose detail is the rule's message.
/// Any other exception falls through to the default <c>500</c> ProblemDetails, without a stack trace.
/// </summary>
public sealed class DomainRuleViolationHandler(IProblemDetailsService problemDetails) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is not DomainRuleViolation violation) return false;

        httpContext.Response.StatusCode = StatusCodes.Status422UnprocessableEntity;
        // Handled even if no writer accepts the client's Accept header: the 422 status still stands.
        await problemDetails.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = new ProblemDetails
            {
                Status = StatusCodes.Status422UnprocessableEntity,
                Title = "A domain rule was violated.",
                Detail = violation.Message,
            },
        });
        return true;
    }
}
