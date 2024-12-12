using Ardalis.GuardClauses;
using Elastic.Clients.Elasticsearch;
using MediatR;
using MicroCommerce.ApiService.Domain.Entities;
using MicroCommerce.ApiService.Infrastructure;
using MicroCommerce.ApiService.Services.Elasticsearch;
using Microsoft.AspNetCore.OutputCaching;

namespace MicroCommerce.ApiService.Features.Products;

public class GetProductsFromElasticsearch : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder builder)
    {
        builder.MapGet("/api/products",
            [OutputCache(Duration = 15)]
            async (IMediator mediator) => TypedResults.Ok(await mediator.Send(new Query())));
    }

    public record Query : IRequest<Response>
    {
    }

    public record Response
    {
        public IList<ProductViewModel> Data { get; init; } = new List<ProductViewModel>();
    }

    public record ProductViewModel
    {
        public Guid Id { get; init; }
        public string Name { get; init; } = "";
        public decimal Price { get; init; }
        public long RemainingStock { get; init; }
        public string ImageUrl { get; set; } = "";
    }

    public class Handler : IRequestHandler<Query, Response>
    {
        private readonly ApplicationDbContext _context;
        private readonly ElasticsearchClient _elasticsearchClient;
        private readonly ILogger<Handler> _logger;

        public Handler(ApplicationDbContext context, ElasticsearchClient elasticsearchClient, ILogger<Handler> logger)
        {
            _context = context;
            _elasticsearchClient = elasticsearchClient;
            _logger = logger;
        }

        public async Task<Response> Handle(Query request, CancellationToken cancellationToken)
        {
            var response = await _elasticsearchClient.SearchAsync<ProductDocument>(
                q => q
                    .Size(100)
                    .From(0), cancellationToken);

            if (!response.IsSuccess())
            {
                _logger.LogError("Failed to get products from Elasticsearch {Info}", response.DebugInformation);
                throw new Exception("Failed to get products from Elasticsearch");
            }

            var products = _context.Products
                .Where(p => response.Documents.Select(d => d.Id).Contains(p.Id))
                .ToList();

            return new Response
            {
                Data = products.Select(s => new ProductViewModel
                {
                    Id = s.Id,
                    Name = s.Name,
                    Price = s.Price,
                    RemainingStock = s.RemainingStock,
                    ImageUrl = s.ImageUrl
                }).ToList()
            };
        }
    }
}
