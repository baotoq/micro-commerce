using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using MicroCommerce.Catalog.API.Application.Products.Models;
using MicroCommerce.Catalog.API.Persistence;

namespace MicroCommerce.Catalog.API.Application.Products.Commands
{
    public class UpdateProductCommand : IRequest<ProductDto>
    {
        public int Id { get; init; }
        
        public string Name { get; set; }
        
        public decimal Price { get; set; }
        
        public int StockQuantity { get; set; }
        
        public string Description { get; set; }
        
        public string ImageUri { get; set; }
    }

    public class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand, ProductDto>
    {
        private readonly IMapper _mapper;
        private readonly ApplicationDbContext _context;

        public UpdateProductCommandHandler(IMapper mapper, ApplicationDbContext context)
        {
            _mapper = mapper;
            _context = context;
        }

        public async Task<ProductDto> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
        {
            var product = await _context.Products.FindAsync(request.Id);
            
            product.Name = request.Name;
            product.Description = request.Description;
            product.Price = request.Price;
            product.StockQuantity = request.StockQuantity;
            product.ImageUri = request.ImageUri;

            await _context.SaveChangesAsync(cancellationToken);

            return _mapper.Map<ProductDto>(product);
        }
    }
}
