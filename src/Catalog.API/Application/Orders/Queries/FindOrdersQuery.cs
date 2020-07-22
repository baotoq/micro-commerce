using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bshop.V1.Identity;
using Catalog.API.Application.Orders.Models;
using Catalog.API.Data.Models;
using MediatR;
using Shared.MediatR.Models;
using UnitOfWork;
using UnitOfWork.Common;
using static Bshop.V1.Identity.IdentityService;

namespace Catalog.API.Application.Orders.Queries
{
    public class FindOrdersQuery : OffsetPagedQuery, IRequest<OffsetPaged<OrderDto>>
    {
    }

    public class FindOrdersQueryHandler : IRequestHandler<FindOrdersQuery, OffsetPaged<OrderDto>>
    {
        private readonly IRepository<Order> _repository;
        private readonly IdentityServiceClient _identityServiceClient;

        public FindOrdersQueryHandler(IRepository<Order> repository, IdentityServiceClient identityServiceClient)
        {
            _repository = repository;
            _identityServiceClient = identityServiceClient;
        }

        public async Task<OffsetPaged<OrderDto>> Handle(FindOrdersQuery request, CancellationToken cancellationToken)
        {
            var result = await _repository.Query()
                .Select(s => new OrderDto
                {
                    CustomerId = s.CustomerId,
                    OrderNote = s.OrderNote,
                    OrderStatus = s.OrderStatus,
                    SubTotal = s.SubTotal,
                    OrderItems = s.OrderItems.Select(x => new OrderItemDto
                    {
                        Quantity = x.Quantity,
                        ProductPrice = x.ProductPrice
                    }).ToList()
                })
                .ToPagedAsync(request.Page, request.PageSize, cancellationToken);

            var response = await _identityServiceClient.GetUsersByIdsAsync(new GetUsersByIdsRequest
            {
                Ids = { result.Data.Select(s => s.CustomerId).Distinct() }
            }, cancellationToken: cancellationToken);

            result.Data.ForEach(s => s.CustomerName = response.Users.SingleOrDefault(x => x.Id == s.CustomerId)?.UserName);

            return result;
        }
    }
}
