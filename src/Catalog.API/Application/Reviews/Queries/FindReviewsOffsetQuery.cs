using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bshop.Identity.V1;
using Catalog.API.Application.Reviews.Models;
using Catalog.API.Data.Models;
using Catalog.API.Data.Models.Enums;
using Data.UnitOfWork;
using Data.UnitOfWork.EF.Common;
using MediatR;
using Shared.MediatR.Models;
using static Bshop.Identity.V1.IdentityService;

namespace Catalog.API.Application.Reviews.Queries
{
    public class FindReviewsOffsetQuery : OffsetPagedQuery, IRequest<OffsetPaged<ReviewDto>>
    {
        public ReviewStatus? ReviewStatus { get; set; }
        public long? ProductId { get; set; }
        public string QueryString { get; set; }
    }

    public class FindReviewsOffsetQueryHandler : IRequestHandler<FindReviewsOffsetQuery, OffsetPaged<ReviewDto>>
    {
        private readonly IRepository<Review> _repository;
        private readonly IdentityServiceClient _identityServiceClient;

        public FindReviewsOffsetQueryHandler(IRepository<Review> repository, IdentityServiceClient identityServiceClient)
        {
            _repository = repository;
            _identityServiceClient = identityServiceClient;
        }

        public async Task<OffsetPaged<ReviewDto>> Handle(FindReviewsOffsetQuery request, CancellationToken cancellationToken)
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

            if (!string.IsNullOrEmpty(request.QueryString))
            {
                request.QueryString = request.QueryString.ToLowerInvariant();
                filterQuery = filterQuery.Where(
                    s => s.Title.ToLower().Contains(request.QueryString) ||
                         s.Comment.ToLower().Contains(request.QueryString));
            }

            var paged = await filterQuery.Select(s => new ReviewDto
            {
                Id = s.Id,
                Title = s.Title,
                Comment = s.Comment,
                Rating = s.Rating,
                ReviewStatus = s.ReviewStatus,
                ProductName = s.Product.Name,
                CreatedById = s.CreatedById,
                CreatedDate = s.CreatedDate
            }).ToPagedAsync(request.Page, request.PageSize, cancellationToken);

            var response = await _identityServiceClient.GetUsersByIdsAsync(new GetUsersByIdsRequest
            {
                Ids = { paged.Data.Select(s => s.CreatedById).Distinct() }
            }, cancellationToken: cancellationToken);

            paged.Data.ForEach(s => s.CreatedByUserName = response.Users.SingleOrDefault(x => x.Id == s.CreatedById)?.UserName);

            return paged;
        }
    }
}
