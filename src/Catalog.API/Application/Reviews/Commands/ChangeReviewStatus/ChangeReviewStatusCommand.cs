using Catalog.API.Data.Models.Enums;
using MediatR;

namespace Catalog.API.Application.Reviews.Commands.ChangeReviewStatus
{
    public class ChangeReviewStatusCommand : IRequest<Unit>
    {
        public long Id { get; set; }

        public ReviewStatus ReviewStatus { get; set; }
    }
}
