using System;
using System.Threading;
using System.Threading.Tasks;
using Catalog.API.Application.Replies.Commands.ApprovePendingReplies;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Catalog.API.BackgroundServices
{
    public class ApproveReplyBackgroundService : BaseBackgroundService
    {

        public ApproveReplyBackgroundService(IServiceProvider services) : base(services)
        {
        }

        public override string BackgroundName { get; } = "Approve replies job";
        public override TimeSpan DelayTime { get; } = TimeSpan.FromMinutes(1);

        public override async Task ProcessAsync(CancellationToken cancellationToken)
        {
            using var scope = ServiceProvider.CreateScope();

            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

            await mediator.Send(new ApprovePendingRepliesCommand(TimeSpan.FromMinutes(5)), cancellationToken);
        }
    }
}
