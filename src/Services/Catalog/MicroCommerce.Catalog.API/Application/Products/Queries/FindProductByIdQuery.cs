using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using CSharpFunctionalExtensions;
using MediatR;
using MicroCommerce.Catalog.API.Application.Products.Models;
using MicroCommerce.Catalog.API.Persistence;
using MicroCommerce.Catalog.API.Persistence.Entities;
using MicroCommerce.Shared.MediatR.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace MicroCommerce.Catalog.API.Application.Products.Queries
{
    public class FindProductByIdQuery : IRequest<Result<ProductDto>>
    {
        public int Id { get; init; }
    }

    public class FindProductByIdQueryHandler : IRequestHandler<FindProductByIdQuery, Result<ProductDto>>
    {
        private readonly IMapper _mapper;
        private readonly ApplicationDbContext _context;

        public FindProductByIdQueryHandler(IMapper mapper, ApplicationDbContext context)
        {
            _mapper = mapper;
            _context = context;
            _context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        }

        public async Task<Result<ProductDto>> Handle(FindProductByIdQuery request, CancellationToken cancellationToken)
        {
            var result = await
                Result.Try(async () => await _context.Products.FindAsync(new object[] {request.Id}, cancellationToken))
                    .TapIf(product => product is null, () => throw new NotFoundException(nameof(Product), request.Id))
                    .Map(_mapper.Map<ProductDto>);

            return result;
        }
    }
}
