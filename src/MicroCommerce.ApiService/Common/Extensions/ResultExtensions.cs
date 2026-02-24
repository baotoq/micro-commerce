using FluentResults;
using Microsoft.AspNetCore.Mvc;

namespace MicroCommerce.ApiService.Common.Extensions;

public static class ResultExtensions
{
    /// <summary>
    /// Maps a non-generic Result to an HTTP response.
    /// Success: returns onSuccess() if provided, otherwise 204 No Content.
    /// Failure: returns 422 Unprocessable Entity with ProblemDetails.
    /// </summary>
    public static Microsoft.AspNetCore.Http.IResult ToHttpResult(
        this Result result,
        Func<Microsoft.AspNetCore.Http.IResult>? onSuccess = null)
    {
        if (result.IsSuccess)
        {
            return onSuccess?.Invoke() ?? Results.NoContent();
        }

        return result.ToFailureProblem();
    }

    /// <summary>
    /// Maps a generic Result&lt;T&gt; to an HTTP response.
    /// Success: returns onSuccess(result.Value).
    /// Failure: returns 422 Unprocessable Entity with ProblemDetails.
    /// </summary>
    public static Microsoft.AspNetCore.Http.IResult ToHttpResult<T>(
        this Result<T> result,
        Func<T, Microsoft.AspNetCore.Http.IResult> onSuccess)
    {
        if (result.IsSuccess)
        {
            return onSuccess(result.Value);
        }

        return result.ToFailureProblem();
    }

    /// <summary>
    /// Creates a 422 Unprocessable Entity ProblemDetails response from a failed result.
    /// </summary>
    private static Microsoft.AspNetCore.Http.IResult ToFailureProblem(this IResultBase result)
    {
        ProblemDetails problemDetails = new ProblemDetails
        {
            Status = StatusCodes.Status422UnprocessableEntity,
            Title = "Business Rule Violation",
            Detail = string.Join("; ", result.Errors.Select(e => e.Message))
        };

        return Results.Problem(problemDetails);
    }
}
