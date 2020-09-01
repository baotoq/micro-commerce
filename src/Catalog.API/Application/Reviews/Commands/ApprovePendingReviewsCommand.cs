using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Catalog.API.Data.Models;
using Catalog.API.Data.Models.Enums;
using Data.UnitOfWork;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Catalog.API.Application.Reviews.Commands
{
    public class ApprovePendingReviewsCommand : IRequest<Unit>
    {
    }

    public class ApprovePendingReviewsCommandHandler : IRequestHandler<ApprovePendingReviewsCommand, Unit>
    {
        private readonly ILogger _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<Review> _reviewRepository;

        public ApprovePendingReviewsCommandHandler(ILogger<ApprovePendingReviewsCommandHandler> logger, IUnitOfWork unitOfWork, IRepository<Review> reviewRepository)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
            _reviewRepository = reviewRepository;
        }

        public async Task<Unit> Handle(ApprovePendingReviewsCommand request, CancellationToken cancellationToken)
        {
            var durationToApprove = DateTime.UtcNow.AddMinutes(-5);

            var reviews = await _reviewRepository.Query()
                .Include(s => s.Product)
                .Where(s => s.ReviewStatus == ReviewStatus.Pending && s.CreatedDate < durationToApprove)
                .ToListAsync(cancellationToken);

            foreach (var review in reviews)
            {
                review.ReviewStatus = ReviewStatus.Approved;

                var product = review.Product;
                product.RatingAverage = (product.ReviewsCount * (product.RatingAverage ?? 0) + review.Rating) / (++product.ReviewsCount);
            }

            await _unitOfWork.CommitAsync(cancellationToken);

            _logger.LogInformation("Approved {Count} reviews with Id: {ReviewIds}", reviews.Count, reviews.Select(s => s.Id));

            return Unit.Value;
        }
    }
}
