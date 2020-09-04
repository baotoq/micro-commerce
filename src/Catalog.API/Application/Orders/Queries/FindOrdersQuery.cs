using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bshop.Identity.V1;
using Catalog.API.Application.Orders.Models;
using Catalog.API.Data.Models;
using Data.Entities.Common;
using Data.UnitOfWork.EF.Common;
using Data.UnitOfWork.EF.Core;
using MediatR;
using Shared.MediatR.Models;
using static Bshop.Identity.V1.IdentityService;

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
                    Id = s.Id,
                    CustomerId = s.CustomerId,
                    OrderNote = s.OrderNote,
                    OrderStatus = s.OrderStatus,
                    SubTotal = s.SubTotal,
                    OrderItems = s.OrderItems.Select(x => new OrderItemDto
                    {
                        Id = x.Id,
                        Quantity = x.Quantity,
                        ProductPrice = x.ProductPrice,
                        ProductName = x.Product.Name,
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
