using System.IO;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using CSharpFunctionalExtensions;
using FluentValidation;
using MediatR;
using MicroCommerce.Catalog.API.Application.Products.Models;
using MicroCommerce.Catalog.API.IntegrationEvents;
using MicroCommerce.Catalog.API.Persistence;
using MicroCommerce.Catalog.API.Persistence.Entities;
using MicroCommerce.Shared.EventBus.Abstractions;
using MicroCommerce.Shared.FileStorage;
using MicroCommerce.Shared.MediatR.Exceptions;
using Microsoft.AspNetCore.Http;

namespace MicroCommerce.Catalog.API.Application.Products.Commands
{
    public record UpdateProductCommand : IRequest<Result<ProductDto>>
    {
        public int Id { get; init; }

        public string Name { get; init; }

        public decimal Price { get; init; }

        public int StockQuantity { get; init; }

        public string Description { get; init; }

        public IFormFile ImageFile { get; init; }
    }

    public class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand, Result<ProductDto>>
    {
        private readonly IMapper _mapper;
        private readonly ApplicationDbContext _context;
        private readonly IStorageService _storageService;
        private readonly IEventBus _eventBus;

        public UpdateProductCommandHandler(IMapper mapper, ApplicationDbContext context, IStorageService storageService, IEventBus eventBus)
        {
            _mapper = mapper;
            _context = context;
            _storageService = storageService;
            _eventBus = eventBus;
        }

        public async Task<Result<ProductDto>> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
        {
            string oldFilePath = null;
            
            var result = await Result.Try(async () => await _context.Products.FindAsync(request.Id))
                .TapIf(product => product is null, () => throw new NotFoundException(nameof(Product), request.Id))
                .Tap(product => oldFilePath = product.ImageUri)
                .Tap(product =>
                {
                    product.Name = request.Name;
                    product.Description = request.Description;
                    product.Price = request.Price;
                    product.StockQuantity = request.StockQuantity;
                    product.ImageUri = $"{Path.GetRandomFileName()}{Path.GetExtension(request.ImageFile.FileName)}";
                })
                .Tap(product => _storageService.SaveAsync(request.ImageFile.OpenReadStream(), product.ImageUri, cancellationToken))
                .Tap(() => _context.SaveChangesAsync(cancellationToken))
                .Map(_mapper.Map<ProductDto>);
            
            await Result.Try(() => _eventBus.PublishAsync(new ProductUpdated(oldFilePath), cancellationToken));
            
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
            RuleFor(s => s.ImageFile).NotNull();
        }
    }
}
