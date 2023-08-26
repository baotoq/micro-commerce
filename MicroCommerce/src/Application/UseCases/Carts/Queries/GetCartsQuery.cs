using Application.Common.AutoMapper;
using AutoMapper;
using Domain.Entities;
using Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.UseCase.Carts.Queries;

public class GetCartsQuery : IRequest<GetCartsQuery.Response>
{
    public class Response
    {
        public IEnumerable<CartDto> Data { get; set; } = new List<CartDto>();
    
        public class CartDto : IMapFrom<Cart>
        {
            public string Id { get; set; } = "";
        }
    }
}

public class GetCartsQueryHandler : IRequestHandler<GetCartsQuery, GetCartsQuery.Response>
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetCartsQueryHandler(ApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<GetCartsQuery.Response> Handle(GetCartsQuery request, CancellationToken cancellationToken)
    {
        var carts = await _context.Carts.ToListAsync(cancellationToken);

        return new GetCartsQuery.Response
        {
            Data = _mapper.Map<IEnumerable<GetCartsQuery.Response.CartDto>>(carts)
        };
    }
}