using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Catalog.API.Application.Products.Models;
using Catalog.API.Data.Models;
using Catalog.API.Extensions;
using Data.UnitOfWork.EF.Core;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http;
using Shared.FileStorage;

namespace Catalog.API.Application.Products.Commands
{
    public class CreateProductCommand : IRequest<ProductDto>
    {
        public string Name { get; set; }

        public decimal Price { get; set; }

        public int CartMaxQuantity { get; set; }

        public int SellQuantity { get; set; }

        public int StockQuantity { get; set; }

        public string Description { get; set; }

        public IFormFile Image { get; set; }

        public IList<long> CategoryIds { get; set; } = new List<long>();
    }

    public class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
    {
        public CreateProductCommandValidator()
        {
            RuleFor(s => s.Name).NotEmpty();
            RuleFor(s => s.Price).GreaterThan(0);
            RuleFor(s => s.Image).NotNull();
        }
    }

    public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, ProductDto>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<Product> _repository;
        private readonly IStorageService _storageService;

        public CreateProductCommandHandler(IUnitOfWork unitOfWork, IStorageService storageService)
        {
            _unitOfWork = unitOfWork;
            _repository = _unitOfWork.Repository<Product>();
            _storageService = storageService;
        }

        public async Task<ProductDto> Handle(CreateProductCommand request, CancellationToken cancellationToken)
        {
            var imageFileName = request.Image.FileName.ToFileName();
            await _storageService.SaveAsync(request.Image.OpenReadStream(), imageFileName, cancellationToken);

            var product = new Product
            {
                Name = request.Name,
                Price = request.Price,
                CartMaxQuantity = request.CartMaxQuantity,
                SellQuantity = request.SellQuantity,
                StockQuantity = request.StockQuantity,
                ImageUri = imageFileName
            };

            foreach (var categoryId in request.CategoryIds)
            {
                var productCategory = new ProductCategory
                {
                    CategoryId = categoryId
                };
                product.Categories.Add(productCategory);
            }

            await _repository.AddAsync(product, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new ProductDto
            {
                Id = product.Id,
                Name = product.Name
            };
        }
    }
}
