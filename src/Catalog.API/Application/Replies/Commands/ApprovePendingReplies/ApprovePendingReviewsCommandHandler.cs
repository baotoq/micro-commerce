using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Catalog.API.Data.Models;
using Catalog.API.Data.Models.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using UnitOfWork;

namespace Catalog.API.Application.Replies.Commands.ApprovePendingReplies
{
    public class ApprovePendingRepliesCommandHandler : IRequestHandler<ApprovePendingRepliesCommand, Unit>
    {
        private readonly ILogger<ApprovePendingRepliesCommandHandler> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<Reply> _repository;

        public ApprovePendingRepliesCommandHandler(ILogger<ApprovePendingRepliesCommandHandler> logger, IUnitOfWork unitOfWork, IRepository<Reply> repository)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
            _repository = repository;
        }

        public async Task<Unit> Handle(ApprovePendingRepliesCommand request, CancellationToken cancellationToken)
        {
            var utcNow = DateTime.UtcNow;

            var reviews = await _repository
                .Query()
                .Where(s => s.ReplyStatus == ReplyStatus.Pending && s.CreatedDate.Add(request.AgeForApprove) <= utcNow)
                .ToListAsync(cancellationToken);

            foreach (var review in reviews)
            {
                review.ReplyStatus = ReplyStatus.Approved;
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Approved {count} reviews with Id: {reviews}", reviews.Count, reviews.Select(s => s.Id));

            return Unit.Value;
        }
    }
}
