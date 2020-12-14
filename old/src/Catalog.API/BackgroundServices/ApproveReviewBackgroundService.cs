using MediatR;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading;
using System.Threading.Tasks;
using Catalog.API.Application.Reviews.Commands;

namespace Catalog.API.BackgroundServices
{
    public class ApproveReviewBackgroundService : BaseBackgroundService
    {

        public ApproveReviewBackgroundService(IServiceProvider services) : base(services)
        {
        }

        public override TimeSpan DelayTime { get; } = TimeSpan.FromMinutes(1);

        public override async Task ProcessAsync(CancellationToken cancellationToken)
        {
            using var scope = ServiceProvider.CreateScope();

            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

            await mediator.Send(new ApprovePendingReviewsCommand(), cancellationToken);
        }
    }
}
