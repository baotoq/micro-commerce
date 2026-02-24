using FluentResults;
using FluentValidation;
using MediatR;

namespace MicroCommerce.ApiService.Common.Behaviors;

/// <summary>
/// MediatR pipeline behavior that runs FluentValidation validators for handlers
/// that return a Result or Result&lt;T&gt; type (i.e., implement IResultBase).
///
/// When validation fails, returns Result.Fail with error messages instead of throwing
/// a ValidationException (unlike ValidationBehavior). This allows the handler to
/// remain in the railway-oriented error handling path.
///
/// NOTE: The (TResponse)(object)Result.Fail(errors) cast works for non-generic Result.
/// Result&lt;T&gt; support in the validation short-circuit path requires reflection and is
/// deferred to ADOPT-05. Pilot handlers should use non-generic Result in the validation path.
/// </summary>
public sealed class ResultValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
    where TResponse : IResultBase
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ResultValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!_validators.Any())
        {
            return await next();
        }

        ValidationContext<TRequest> context = new ValidationContext<TRequest>(request);

        FluentValidation.Results.ValidationResult[] validationResults = await Task.WhenAll(
            _validators.Select(v => v.ValidateAsync(context, cancellationToken)));

        List<FluentValidation.Results.ValidationFailure> failures = validationResults
            .SelectMany(r => r.Errors)
            .Where(f => f != null)
            .ToList();

        if (failures.Count == 0)
        {
            return await next();
        }

        List<IError> errors = failures
            .Select(f => (IError)new Error($"{f.PropertyName}: {f.ErrorMessage}"))
            .ToList();

        return (TResponse)(object)Result.Fail(errors);
    }
}
