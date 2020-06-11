using System.Threading;
using System.Threading.Tasks;
using Catalog.API.Data.Models;
using MediatR;
using UnitOfWork;

namespace Catalog.API.Application.Reviews.Commands.Create
{
    public class CreateReviewCommandHandler : IRequestHandler<CreateReviewCommand, Unit>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<Review> _repository;

        public CreateReviewCommandHandler(IUnitOfWork unitOfWork, IRepository<Review> repository)
        {
            _unitOfWork = unitOfWork;
            _repository = repository;
        }

        public async Task<Unit> Handle(CreateReviewCommand request, CancellationToken cancellationToken)
        {
            await _repository.AddAsync(new Review
            {
                Title = request.Title,
                Comment = request.Comment,
                Rating = request.Rating,
                ProductId = request.ProductId
            }, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}