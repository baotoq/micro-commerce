using System.Linq;
using System.Threading.Tasks;
using Bshop.V1.Identity;
using Grpc.Core;
using Identity.API.Application.Users.Queries;
using MediatR;

namespace Identity.API.Grpc
{
    public class IdentityGrpcService : IdentityService.IdentityServiceBase
    {
        private readonly IMediator _mediator;

        public IdentityGrpcService(IMediator mediator)
        {
            _mediator = mediator;
        }

        public override async Task<GetUsersByIdsResponse> GetUsersByIds(GetUsersByIdsRequest request, ServerCallContext context)
        {
            var result = await _mediator.Send(new FindUsersByIdsQuery
            {
                Ids = request.Ids
            }, context.CancellationToken);

            return new GetUsersByIdsResponse
            {
                Users =
                { 
                    result.Select(s => new GetUsersByIdsResponse.Types.User
                    {
                        Id = s.Id,
                        Email = s.Email,
                        UserName = s.UserName
                    }).ToList()
                }
            };
        }
    }
}
