using Application.Common;
using Application.Common.AutoMapper;
using AutoMapper;
using Domain.Entities;
using Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.UseCases.Carts.Queries;

public class GetCartsQuery : IRequest<GetCartsQuery.Response>
{
    public class Response
    {
        public IEnumerable<CartDto> Data { get; set; } = new List<CartDto>();
    
        public class CartDto : IMapFrom<Cart>
        {
            public string Id { get; set; } = string.Empty;
        }
    }
    
    public class Handler : RequestHandlerBase<GetCartsQuery, GetCartsQuery.Response>
    {
        private readonly IMapper _mapper;

        public Handler(ApplicationDbContext context, IMapper mapper) : base(context)
        {
            _mapper = mapper;
        }

        public override async Task<Response> Handle(GetCartsQuery request, CancellationToken cancellationToken = default)
        {
            var carts = await Context.Carts.ToListAsync(cancellationToken);

            return new Response
            {
                Data = _mapper.Map<IEnumerable<Response.CartDto>>(carts)
            };
        }
    }
}