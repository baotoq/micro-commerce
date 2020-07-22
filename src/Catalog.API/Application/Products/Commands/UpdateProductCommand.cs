using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Catalog.API.Data.Models;
using Catalog.API.Extensions;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Shared.FileStorage;
using Shared.MediatR.Exceptions;
using UnitOfWork;

namespace Catalog.API.Application.Products.Commands
{
    public class UpdateProductCommand : IRequest
    {
        [JsonIgnore]
        public long Id { get; set; }

        public string Name { get; set; }

        public decimal Price { get; set; }

        public int CartMaxQuantity { get; set; }

        public int SellQuantity { get; set; }

        public int StockQuantity { get; set; }

        public string Description { get; set; }

        public IFormFile Image { get; set; }

        public IList<long> CategoryIds { get; set; } = new List<long>();
    }

    public class UpdateProductCommandValidator : AbstractValidator<UpdateProductCommand>
    {
        public UpdateProductCommandValidator()
        {
            RuleFor(s => s.Name).NotEmpty();
            RuleFor(s => s.Price).GreaterThan(0);
        }
    }

    public class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand, Unit>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IStorageService _storageService;
        private readonly IRepository<Product> _repository;

        public UpdateProductCommandHandler(IUnitOfWork unitOfWork, IStorageService storageService)
        {
            _unitOfWork = unitOfWork;
            _storageService = storageService;
            _repository = _unitOfWork.Repository<Product>();
        }

        public async Task<Unit> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
        {
            var product = await _repository.Query()
                .Include(s => s.Categories)
                .SingleOrDefaultAsync(s => s.Id == request.Id, cancellationToken);

            if (product == null)
            {
                throw new NotFoundException(nameof(Product), request.Id);
            }

            var imageToDelete = product.ImageUri;

            product.Name = request.Name;
            product.Price = request.Price;
            product.CartMaxQuantity = request.CartMaxQuantity;
            product.SellQuantity = request.SellQuantity;
            product.StockQuantity = request.StockQuantity;
            product.Description = request.Description;

            if (request.Image != null)
            {
                var imageFileName = request.Image.FileName.ToFileName();
                await _storageService.SaveAsync(request.Image.OpenReadStream(), imageFileName, cancellationToken);
                product.ImageUri = imageFileName;
            }

            product.Categories.Clear();

            foreach (var categoryId in request.CategoryIds)
            { 
                product.Categories.Add(new ProductCategory
                {
                    CategoryId = categoryId
                });
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            if (request.Image != null)
            {
                await _storageService.DeleteAsync(imageToDelete, cancellationToken);
            }
          
            return Unit.Value;
        }
    }
}
