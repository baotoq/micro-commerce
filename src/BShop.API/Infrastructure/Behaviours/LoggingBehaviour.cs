using System.Threading;
using System.Threading.Tasks;
using MediatR.Pipeline;
using Microsoft.Extensions.Logging;

namespace BShop.API.Infrastructure.Behaviours
{
    public class LoggingBehaviour<TRequest> : IRequestPreProcessor<TRequest>
    {
        private readonly ILogger<LoggingBehaviour<TRequest>> _logger;

        public LoggingBehaviour(ILogger<LoggingBehaviour<TRequest>> logger)
        {
            _logger = logger;
        }

        public Task Process(TRequest request, CancellationToken cancellationToken)
        {
            var requestName = typeof(TRequest).Name;

            _logger.LogInformation("Request: {Name} {@Request}", requestName, request);

            return Task.CompletedTask;
        }
    }
}
