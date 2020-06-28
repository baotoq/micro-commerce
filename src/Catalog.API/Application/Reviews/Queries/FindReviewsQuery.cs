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
    public class FindReviewsQuery : CursorPagedQuery<DateTime>, IRequest<CursorPaged<ReviewDto, DateTime?>>
    {
        public override DateTime PageToken { get; set; } = DateTime.Now;
        public ReviewStatus? ReviewStatus { get; set; }
        public long? ProductId { get; set; }
    }

    public class FindReviewsQueryHandler : IRequestHandler<FindReviewsQuery, CursorPaged<ReviewDto, DateTime?>>
    {
        private readonly IRepository<Review> _repository;

        public FindReviewsQueryHandler(IRepository<Review> repository)
        {
            _repository = repository;
        }

        public async Task<CursorPaged<ReviewDto, DateTime?>> Handle(FindReviewsQuery request, CancellationToken cancellationToken)
        {
            var filterQuery = _repository.Query();

            if (request.ReviewStatus != null)
            {
                filterQuery = filterQuery.Where(s => s.ReviewStatus == request.ReviewStatus);
            }

            if (request.ProductId != null)
            {
                filterQuery = filterQuery.Where(s => s.ProductId == request.ProductId);
            }

            var result = await filterQuery
                .Where(s => s.CreatedDate <= request.PageToken)
                .OrderByDescending(s => s.CreatedDate)
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

            var next = await filterQuery
                .Where(s => s.CreatedDate <= request.PageToken)
                .OrderByDescending(s => s.CreatedDate)
                .Skip(request.PageSize)
                .Select(s => new { s.CreatedDate })
                .FirstOrDefaultAsync(cancellationToken);

            var previous = await filterQuery
                .Where(s => s.CreatedDate > request.PageToken)
                .OrderBy(s => s.CreatedDate)
                .Skip(request.PageSize - 1)
                .Select(s => new { s.CreatedDate })
                .FirstOrDefaultAsync(cancellationToken);

            var totalPages = (int)Math.Ceiling((double)await filterQuery.CountAsync(cancellationToken) / request.PageSize);

            var paged = new CursorPaged<ReviewDto, DateTime?>
            {
                Data = result.ToList(),
                PreviousPageToken = previous?.CreatedDate,
                NextPageToken = next?.CreatedDate,
                TotalPages = totalPages
            };

            return paged;
        }
    }
}
