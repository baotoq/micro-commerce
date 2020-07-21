using Catalog.API.Data.Models;
using MediatR;
using Shared.MediatR.Exceptions;
using System.Threading;
using System.Threading.Tasks;
using UnitOfWork;

namespace Catalog.API.Application.Reviews.Commands
{
    public class DeleteReviewCommand : IRequest
    {
        public long Id { get; set; }

        public DeleteReviewCommand(long id)
        {
            Id = id;
        }
    }

    public class DeleteReviewCommandHandler : IRequestHandler<DeleteReviewCommand, Unit>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<Review> _repository;

        public DeleteReviewCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _repository = _unitOfWork.Repository<Review>();
        }

        public async Task<Unit> Handle(DeleteReviewCommand request, CancellationToken cancellationToken)
        {
            var review = await _repository.FindAsync(request.Id, cancellationToken);

            if (review == null)
            {
                throw new NotFoundException(nameof(Review), request.Id);
            }

            _repository.Remove(review);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}
