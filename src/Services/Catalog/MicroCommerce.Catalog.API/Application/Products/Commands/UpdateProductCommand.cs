using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using CSharpFunctionalExtensions;
using FluentValidation;
using MediatR;
using MicroCommerce.Catalog.API.Application.Products.Models;
using MicroCommerce.Catalog.API.Persistence;
using MicroCommerce.Catalog.API.Persistence.Entities;
using MicroCommerce.Shared.MediatR.Exceptions;

namespace MicroCommerce.Catalog.API.Application.Products.Commands
{
    public class UpdateProductCommand : IRequest<Result<ProductDto>>
    {
        public int Id { get; init; }
        
        public string Name { get; set; }
        
        public decimal Price { get; set; }
        
        public int StockQuantity { get; set; }
        
        public string Description { get; set; }
        
        public string ImageUri { get; set; }
    }

    public class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand, Result<ProductDto>>
    {
        private readonly IMapper _mapper;
        private readonly ApplicationDbContext _context;

        public UpdateProductCommandHandler(IMapper mapper, ApplicationDbContext context)
        {
            _mapper = mapper;
            _context = context;
        }

        public async Task<Result<ProductDto>> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
        {
            var result = await Result.Try(async () => await _context.Products.FindAsync(request.Id))
                .TapIf(product => product is null, () => throw new NotFoundException(nameof(Product), request.Id))
                .Tap(product =>
                {
                    product.Name = request.Name;
                    product.Description = request.Description;
                    product.Price = request.Price;
                    product.StockQuantity = request.StockQuantity;
                    product.ImageUri = request.ImageUri;
                })
                .Tap(() => _context.SaveChangesAsync(cancellationToken))
                .Map(_mapper.Map<ProductDto>);

            return result;
        }
    }

    public class UpdateProductCommandValidator : AbstractValidator<UpdateProductCommand>
    {
        public UpdateProductCommandValidator()
        {
            RuleFor(s => s.Name).NotEmpty();
            RuleFor(s => s.Description).NotEmpty();
            RuleFor(s => s.Price).NotEmpty();
            RuleFor(s => s.StockQuantity).NotEmpty();
        }
    }
}
