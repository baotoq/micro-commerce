using System.Collections.Generic;
using Catalog.API.Data.Models;
using Catalog.API.Data.Models.Enums;
using MediatR;

namespace Catalog.API.Application.Reviews.Queries.FindByReviewStatus
{
    public class FindByReviewStatusQuery : IRequest<List<Review>>
    {
        public ReviewStatus ReviewStatus { get; set; }
    }
}
