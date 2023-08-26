using Application.Common.AutoMapper;
using AutoMapper;
using Domain.Entities;
using Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.UseCase.Products;

public class GetProductsQuery : IRequest<GetProductsQueryResponse>
{
    
}

public class GetProductsQueryResponse
{
    public IEnumerable<ProductDto> Data { get; set; } = new List<ProductDto>();
    
    public class ProductDto : IMapFrom<Product>
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
    }
}

public class GetProductsQueryHandler : IRequestHandler<GetProductsQuery, GetProductsQueryResponse>
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetProductsQueryHandler(ApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<GetProductsQueryResponse> Handle(GetProductsQuery request, CancellationToken cancellationToken)
    {
        var products = await _context.Products.ToListAsync(cancellationToken);

        return new GetProductsQueryResponse
        {
            Data = _mapper.Map<IEnumerable<GetProductsQueryResponse.ProductDto>>(products)
        };
    }
}