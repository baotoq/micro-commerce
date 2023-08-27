using Infrastructure.Persistence;
using MediatR;

namespace Application.Common;

public abstract class RequestHandlerBase<TRequest, TResponse> : IRequestHandler<TRequest, TResponse> where TRequest : IRequest<TResponse>
{
    protected readonly ApplicationDbContext Context;

    protected RequestHandlerBase(ApplicationDbContext context)
    {
        Context = context;
    }

    public abstract Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken = default);
}