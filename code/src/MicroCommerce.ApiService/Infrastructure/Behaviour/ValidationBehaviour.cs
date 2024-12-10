using System.Diagnostics;
using FluentValidation;
using MediatR;
using MicroCommerce.ApiService.Infrastructure.Exceptions;

namespace MicroCommerce.ApiService.Infrastructure.Behaviour;

[DebuggerStepThrough]
public class ValidationBehaviour<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators) : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (validators.Any())
        {
            var context = new ValidationContext<TRequest>(request);

            var validationResults = await Task.WhenAll(
                validators.Select(v =>
                    v.ValidateAsync(context, cancellationToken)));

            var failures = validationResults
                .Where(r => r.Errors.Any())
                .SelectMany(r => r.Errors)
                .ToList();

            if (failures.Any())
            {
                throw new InvalidValidationException(failures);
            }
        }

        return await next();
    }
}
