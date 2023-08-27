using Application.Common;
using Application.Common.AutoMapper;
using AutoMapper;
using Domain.Entities;
using Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.UseCases.Products.Queries;

public class GetProductsQuery : IRequest<GetProductsQuery.Response>
{
    public class Response
    {
        public IEnumerable<ProductDto> Data { get; set; } = new List<ProductDto>();
    }
    
    public class ProductDto : IMapFrom<Product>
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
    }
    
    public class Handler : RequestHandlerBase<GetProductsQuery, Response>
    {
        private readonly IMapper _mapper;

        public Handler(ApplicationDbContext context, IMapper mapper) : base(context)
        {
            _mapper = mapper;
        }

        public override async Task<Response> Handle(GetProductsQuery request, CancellationToken cancellationToken = default)
        {
            var products = await Context.Products.ToListAsync(cancellationToken);

            return new Response
            {
                Data = _mapper.Map<IEnumerable<ProductDto>>(products)
            };
        }
    }
}