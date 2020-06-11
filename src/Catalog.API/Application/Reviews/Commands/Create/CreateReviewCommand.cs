using MediatR;

namespace Catalog.API.Application.Reviews.Commands.Create
{
    public class CreateReviewCommand : IRequest<Unit>
    {
        public string? Title { get; set; }

        public string? Comment { get; set; }

        public int Rating { get; set; }

        public long ProductId { get; set; }
    }
}
