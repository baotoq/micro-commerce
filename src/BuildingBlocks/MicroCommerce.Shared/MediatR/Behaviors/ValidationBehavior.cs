using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace MicroCommerce.Shared.MediatR.Behaviors
{
    public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        private readonly IEnumerable<IValidator<TRequest>> _validators;
        private readonly ILogger<ValidationBehavior<TRequest, TResponse>> _logger;

        public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators, ILogger<ValidationBehavior<TRequest, TResponse>> logger)
        {
            _validators = validators;
            _logger = logger;
        }

        public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<TResponse> next)
        {
            _logger.LogInformation("Validating {request}", typeof(TRequest));

            if (_validators.Any())
            {
                var validationResults = await Task.WhenAll(_validators.Select(v => v.ValidateAsync(request, cancellationToken)));
                var failures = validationResults.SelectMany(r => r.Errors).ToList();
                if (failures.Any())
                {
                    throw new ValidationException(failures);
                }
            }

            _logger.LogInformation("Validated {request}", typeof(TRequest));

            return await next();
        }
    }
}
