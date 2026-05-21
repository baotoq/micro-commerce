using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Vogen;

namespace MicroCommerce.Catalog.Api.ExceptionHandlers;

/// <summary>
/// Translates input/domain validation exceptions into RFC 7807 400 responses (audit#7).
/// Domain still throws — only the HTTP surface changes.
/// </summary>
internal sealed class InvalidInputExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken ct)
    {
        if (exception is not (ArgumentException or ValueObjectValidationException))
            return false;

        httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
        await Results.Problem(
            title: "Invalid input",
            detail: exception.Message,
            statusCode: StatusCodes.Status400BadRequest,
            type: "https://tools.ietf.org/html/rfc7231#section-6.5.1")
            .ExecuteAsync(httpContext);
        return true;
    }
}
