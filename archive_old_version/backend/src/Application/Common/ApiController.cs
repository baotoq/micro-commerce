using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Application.Common;

[ApiController]
[Route("api/[controller]")]
public abstract class ApiController : ControllerBase
{
    protected readonly IMediator Mediator;

    protected ApiController(IMediator mediator)
    {
        Mediator = mediator;
    }
}


