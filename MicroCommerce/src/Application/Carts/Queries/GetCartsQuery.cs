using Domain.Entities;
using Infrastructure;
using Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Carts.Queries;

public class GetCartsQuery : IRequest<GetCartsQueryResponse>
{
    
}

public class GetCartsQueryResponse
{
    public IEnumerable<Cart> Data { get; set; } = new List<Cart>();
    
    public class Cart
    {
        public string Id { get; set; } = "";
    }
}

public class GetCartsQueryHandler : IRequestHandler<GetCartsQuery, GetCartsQueryResponse>
{
    private readonly ApplicationDbContext _context;

    public GetCartsQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<GetCartsQueryResponse> Handle(GetCartsQuery request, CancellationToken cancellationToken)
    {
        var carts = await _context.Carts.ToListAsync(cancellationToken);

        return new GetCartsQueryResponse
        {
            Data = carts.Select(s => new GetCartsQueryResponse.Cart
            {
                Id = s.Id
            })
        };
    }
}