using System.Threading;
using System.Threading.Tasks;
using Catalog.API.Data.Models;
using Catalog.API.Data.Models.Enums;
using MediatR;
using UnitOfWork;

namespace Catalog.API.Application.Reviews.Commands
{
    public class ChangeReviewStatusCommand : IRequest<Unit>
    {
        public long Id { get; set; }

        public ReviewStatus ReviewStatus { get; set; }
    }

    public class ChangeReviewStatusCommandHandler : IRequestHandler<ChangeReviewStatusCommand, Unit>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<Review> _repository;

        public ChangeReviewStatusCommandHandler(IUnitOfWork unitOfWork, IRepository<Review> repository)
        {
            _unitOfWork = unitOfWork;
            _repository = repository;
        }

        public async Task<Unit> Handle(ChangeReviewStatusCommand request, CancellationToken cancellationToken)
        {
            var review = await _repository.FindAsync(request.Id, cancellationToken);

            review.ReviewStatus = request.ReviewStatus;

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}
