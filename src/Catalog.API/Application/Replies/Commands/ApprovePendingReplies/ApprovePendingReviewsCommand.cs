using System;
using MediatR;

namespace Catalog.API.Application.Replies.Commands.ApprovePendingReplies
{
    public class ApprovePendingRepliesCommand : IRequest<Unit>
    {
        public TimeSpan AgeForApprove { get; set; }

        public ApprovePendingRepliesCommand(TimeSpan ageForApprove)
        {
            AgeForApprove = ageForApprove;
        }
    }
}
