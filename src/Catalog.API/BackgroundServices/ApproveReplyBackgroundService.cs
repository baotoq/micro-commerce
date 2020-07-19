using System;
using System.Threading;
using System.Threading.Tasks;
using Catalog.API.Application.Replies.Commands;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Catalog.API.BackgroundServices
{
    public class ApproveReplyBackgroundService : BaseBackgroundService
    {

        public ApproveReplyBackgroundService(IServiceProvider services) : base(services)
        {
        }

        public override TimeSpan StartDelay { get; } = TimeSpan.FromMinutes(0.5);
        public override TimeSpan DelayTime { get; } = TimeSpan.FromMinutes(1);

        public override async Task ProcessAsync(CancellationToken cancellationToken)
        {
            using var scope = ServiceProvider.CreateScope();

            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

            await mediator.Send(new ApprovePendingRepliesCommand(), cancellationToken);
        }
    }
}
