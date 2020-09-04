using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bshop.Identity.V1;
using Catalog.API.Application.Reviews.Models;
using Catalog.API.Data.Models;
using Catalog.API.Data.Models.Enums;
using Data.Entities.Common;
using Data.UnitOfWork.EF.Core;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Shared.MediatR.Models;
using static Bshop.Identity.V1.IdentityService;

namespace Catalog.API.Application.Reviews.Queries
{
    public class FindReviewsCursorQuery : CursorPagedQuery<DateTime>, IRequest<CursorPaged<ReviewDto, DateTime?>>
    {
        public override DateTime PageToken { get; set; } = DateTime.UtcNow;
        public ReviewStatus? ReviewStatus { get; set; }
        public long? ProductId { get; set; }
    }

    public class FindReviewsCursorQueryHandler : IRequestHandler<FindReviewsCursorQuery, CursorPaged<ReviewDto, DateTime?>>
    {
        private readonly IRepository<Review> _repository;
        private readonly IdentityServiceClient _identityServiceClient;

        public FindReviewsCursorQueryHandler(IRepository<Review> repository, IdentityServiceClient identityServiceClient)
        {
            _repository = repository;
            _identityServiceClient = identityServiceClient;
        }

        public async Task<CursorPaged<ReviewDto, DateTime?>> Handle(FindReviewsCursorQuery request, CancellationToken cancellationToken)
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

            var paged = new CursorPaged<ReviewDto, DateTime?>
            {
                Data = result.ToList(),
                PreviousPageToken = previous?.CreatedDate,
                NextPageToken = next?.CreatedDate
            };

            var response = await _identityServiceClient.GetUsersByIdsAsync(new GetUsersByIdsRequest
            {
                Ids = { paged.Data.Select(s => s.CreatedById).Distinct() }
            });

            paged.Data.ForEach(s => s.CreatedByUserName = response.Users.SingleOrDefault(x => x.Id == s.CreatedById)?.UserName);

            return paged;
        }
    }
}
