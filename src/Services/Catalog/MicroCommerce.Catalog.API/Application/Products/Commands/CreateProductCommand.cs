using System.IO;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using CSharpFunctionalExtensions;
using FluentValidation;
using MediatR;
using MicroCommerce.Catalog.API.Application.Products.Models;
using MicroCommerce.Catalog.API.Persistence;
using MicroCommerce.Catalog.API.Persistence.Entities;
using MicroCommerce.Shared.FileStorage;
using Microsoft.AspNetCore.Http;

namespace MicroCommerce.Catalog.API.Application.Products.Commands
{
    public class CreateProductCommand : IRequest<Result<ProductDto>>
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

    public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, Result<ProductDto>>
    {
        private readonly IMapper _mapper;
        private readonly ApplicationDbContext _context;
        private readonly IStorageService _storageService;

        public CreateProductCommandHandler(IMapper mapper, ApplicationDbContext context, IStorageService storageService)
        {
            _mapper = mapper;
            _context = context;
            _storageService = storageService;
        }

        public async Task<Result<ProductDto>> Handle(CreateProductCommand request, CancellationToken cancellationToken)
        {
            var result = await
                Result.Try(() => _mapper.Map<Product>(request))
                    .Tap(product =>
                        product.ImageUri = $"{Path.GetRandomFileName()}{Path.GetExtension(request.ImageFile.FileName)}")
                    .Tap(product => _storageService.SaveAsync(request.ImageFile.OpenReadStream(), product.ImageUri, cancellationToken))
                    .Tap(async product => await _context.Products.AddAsync(product, cancellationToken))
                    .Tap(() => _context.SaveChangesAsync(cancellationToken))
                    .Map(_mapper.Map<ProductDto>);

            return result;
        }
    }

    public class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
    {
        public CreateProductCommandValidator()
        {
            RuleFor(s => s.Name).NotEmpty();
            RuleFor(s => s.Description).NotEmpty();
            RuleFor(s => s.Price).NotEmpty();
            RuleFor(s => s.StockQuantity).NotEmpty();
            RuleFor(s => s.ImageFile).NotNull();
        }
    }
}
