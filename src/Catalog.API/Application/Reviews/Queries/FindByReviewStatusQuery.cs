using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Catalog.API.Application.Reviews.Models;
using Catalog.API.Data.Models;
using Catalog.API.Data.Models.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Shared.MediatR.Models;
using UnitOfWork;
using UnitOfWork.Common;

namespace Catalog.API.Application.Reviews.Queries
{
    public class FindByReviewStatusQuery : CursorPagedQuery<DateTime>, IRequest<CursorPaged<ReviewDto, DateTime?>>
    {
        public override DateTime PageToken { get; set; } = DateTime.Now;
        public ReviewStatus ReviewStatus { get; set; }
    }

    public class FindByReviewStatusQueryHandler : IRequestHandler<FindByReviewStatusQuery, CursorPaged<ReviewDto, DateTime?>>
    {
        private readonly IRepository<Review> _repository;

        public FindByReviewStatusQueryHandler(IRepository<Review> repository)
        {
            _repository = repository;
        }

        public async Task<CursorPaged<ReviewDto, DateTime?>> Handle(FindByReviewStatusQuery request, CancellationToken cancellationToken)
        {
            var result = await _repository.Query()
                .OrderByDescending(s => s.CreatedDate)
                .Where(s => s.CreatedDate < request.PageToken && s.ReviewStatus == request.ReviewStatus)
                .Take(request.PageSize)
                .Select(s => new ReviewDto
                {
                    Id = s.Id,
                    Title = s.Title,
                    Comment = s.Comment,
                    Rating = s.Rating,
                    ReviewStatus = s.ReviewStatus,
                    CreatedById = s.CreatedById,
                    CreatedDate = s.CreatedDate
                })
                .ToListAsync(cancellationToken);

            var next = await _repository.Query()
                .OrderByDescending(s => s.CreatedDate)
                .Where(s => s.CreatedDate < request.PageToken && s.ReviewStatus == request.ReviewStatus)
                .Skip(request.PageSize)
                .Select(s => new { s.CreatedDate })
                .FirstOrDefaultAsync(cancellationToken);

            var previous = await _repository.Query()
                .OrderBy(s => s.CreatedDate)
                .Where(s => s.CreatedDate >= request.PageToken && s.ReviewStatus == request.ReviewStatus)
                .Skip(request.PageSize - 1)
                .Select(s => new { s.CreatedDate })
                .FirstOrDefaultAsync(cancellationToken);

            var paged = new CursorPaged<ReviewDto, DateTime?>
            {
                Data = result.ToList(),
                PreviousPageToken = previous?.CreatedDate,
                NextPageToken = next?.CreatedDate,
            };

            return paged;
        }
    }
}
