using System.Threading;
using System.Threading.Tasks;
using MediatR;
using MicroCommerce.Catalog.API.Persistence;
using MicroCommerce.Catalog.API.Persistence.Entities;

namespace MicroCommerce.Catalog.API.Application.Products.Commands
{
    public class CreateProductCommand : IRequest<Product>
    {
        public long Id { get; set; }

        public string Name { get; set; }

        public decimal Price { get; set; }

        public int StockQuantity { get; set; }

        public string Description { get; set; }

        public string ImageUri { get; set; }
    }

    public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, Product>
    {
        private readonly ApplicationDbContext _context;

        public CreateProductCommandHandler(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Product> Handle(CreateProductCommand request, CancellationToken cancellationToken)
        {
            var product = new Product
            {
                Name = request.Name,
                Price = request.Price,
                StockQuantity = request.StockQuantity,
                Description = request.Description,
                ImageUri = request.ImageUri
            };

            await _context.Products.AddAsync(product, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            return product;
        }
    }
}
