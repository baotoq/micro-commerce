using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using AutoMapper;
using CSharpFunctionalExtensions;
using MediatR;
using MicroCommerce.Catalog.API.Application.Products.Models;
using MicroCommerce.Catalog.API.Persistence;
using MicroCommerce.Catalog.API.Persistence.Entities;
using Microsoft.AspNetCore.Http;

namespace MicroCommerce.Catalog.API.Application.Products.Commands
{
    public class CreateProductCommand : IRequest<ProductDto>
    {
        public string Name { get; set; }

        public decimal Price { get; set; }

        public int StockQuantity { get; set; }

        public string Description { get; set; }

        public IFormFile ImageFile { get; set; }

        public class MapperProfile : Profile
        {
            public MapperProfile()
            {
                CreateMap<CreateProductCommand, Product>()
                    .ForMember(s => s.Id, opt => opt.Ignore())
                    .ForMember(s => s.Categories, opt => opt.Ignore());
            }
        }
    }

    public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, ProductDto>
    {
        private readonly IMapper _mapper;
        private readonly ApplicationDbContext _context;

        public CreateProductCommandHandler(IMapper mapper, ApplicationDbContext context)
        {
            _mapper = mapper;
            _context = context;
        }

        public async Task<ProductDto> Handle(CreateProductCommand request, CancellationToken cancellationToken)
        {
            using var transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

            var result = await Result.Try(async () =>
                {
                    var path = "/test.pdf";
                    await using var stream = File.Create(path);
                    await request.ImageFile.CopyToAsync(stream, cancellationToken);
                    return path;
                })
                .Map(path =>
                {
                    var product = _mapper.Map<Product>(request);
                    product.ImageUri = path;
                    return product;
                })
                .Tap(async product => await _context.Products.AddAsync(product, cancellationToken))
                .Tap(async () => await _context.SaveChangesAsync(cancellationToken))
                .Map(product => _mapper.Map<ProductDto>(product))
                .Tap(() => transaction.Complete());

            return result.Value;
        }
    }
}
