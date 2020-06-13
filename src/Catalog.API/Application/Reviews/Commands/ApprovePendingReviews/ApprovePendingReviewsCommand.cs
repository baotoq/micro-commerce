using System;
using MediatR;

namespace Catalog.API.Application.Reviews.Commands.ApprovePendingReviews
{
    public class ApprovePendingReviewsCommand : IRequest<Unit>
    {
        public TimeSpan AgeForApprove { get; set; }

        public ApprovePendingReviewsCommand(TimeSpan ageForApprove)
        {
            AgeForApprove = ageForApprove;
        }
    }
}
