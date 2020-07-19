using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Identity.API.Application.Roles;
using Identity.API.Application.Roles.Queries;
using IdentityServer4;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Identity.API.ApiControllers
{
    [Authorize(IdentityServerConstants.LocalApi.PolicyName)]
    [ApiController]
    [Route("api/[controller]")]
    public class RolesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public RolesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<ActionResult<List<RoleDto>>> FindRoles(CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new FindRolesQuery(), cancellationToken);

            return result;
        }
    }
}
