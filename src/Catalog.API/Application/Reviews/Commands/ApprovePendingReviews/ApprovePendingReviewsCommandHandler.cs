using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Catalog.API.Data.Models;
using Catalog.API.Data.Models.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using UnitOfWork;

namespace Catalog.API.Application.Reviews.Commands.ApprovePendingReviews
{
    public class ApprovePendingReviewsCommandHandler : IRequestHandler<ApprovePendingReviewsCommand, Unit>
    {
        private readonly ILogger<ApprovePendingReviewsCommandHandler> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<Review> _repository;

        public ApprovePendingReviewsCommandHandler(ILogger<ApprovePendingReviewsCommandHandler> logger, IUnitOfWork unitOfWork, IRepository<Review> repository)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
            _repository = repository;
        }

        public async Task<Unit> Handle(ApprovePendingReviewsCommand request, CancellationToken cancellationToken)
        {
            var utcNow = DateTime.UtcNow;

            var reviews = await _repository
                .Query()
                .Where(s => s.ReviewStatus == ReviewStatus.Pending && s.CreatedDate.Add(request.AgeForApprove) <= utcNow)
                .ToListAsync(cancellationToken);

            foreach (var review in reviews)
            {
                review.ReviewStatus = ReviewStatus.Approved;
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Approved {count} reviews with Id: {reviews}", reviews.Count, reviews.Select(s => s.Id));

            return Unit.Value;
        }
    }
}
