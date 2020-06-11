﻿using System.Threading;
using System.Threading.Tasks;
using Catalog.API.Data.Models;
using MediatR;
using UnitOfWork;

namespace Catalog.API.Application.Reviews.Commands.ChangeReviewStatus
{
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