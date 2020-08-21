using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Catalog.API.Data.Models;
using Catalog.API.Data.Models.Enums;
using Data.UnitOfWork.EF;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Shared.MediatR.Exceptions;

namespace Catalog.API.Application.Reviews.Commands
{
    public class ChangeReviewStatusCommand : IRequest<Unit>
    {
        [JsonIgnore]
        public long Id { get; set; }
        public ReviewStatus ReviewStatus { get; set; }
    }

    public class ChangeReviewStatusCommandHandler : IRequestHandler<ChangeReviewStatusCommand, Unit>
    {
        private readonly IEfUnitOfWork _unitOfWork;
        private readonly IRepository<Review> _repository;
        private readonly IRepository<Product> _productRepository;

        public ChangeReviewStatusCommandHandler(IEfUnitOfWork unitOfWork, IRepository<Review> repository, IRepository<Product> productRepository)
        {
            _unitOfWork = unitOfWork;
            _repository = repository;
            _productRepository = productRepository;
        }

        public async Task<Unit> Handle(ChangeReviewStatusCommand request, CancellationToken cancellationToken)
        {
            var review = await _repository.Query()
                .SingleOrDefaultAsync(s => s.Id == request.Id, cancellationToken);

            if (review == null)
            {
                throw new NotFoundException(nameof(Review), request.Id);
            }

            review.ReviewStatus = request.ReviewStatus;
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var approvedReviews = _repository.Query()
                .Where(s => s.ProductId == review.ProductId && s.ReviewStatus == ReviewStatus.Approved);

            var product = await _productRepository.FindAsync(review.ProductId, cancellationToken);
            product.ReviewsCount = await approvedReviews.CountAsync(cancellationToken);

            if (product.ReviewsCount == 0)
            {
                product.RatingAverage = null;
            }
            else
            {
                product.RatingAverage = await approvedReviews.SumAsync(s => s.Rating, cancellationToken) / product.ReviewsCount;
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}
