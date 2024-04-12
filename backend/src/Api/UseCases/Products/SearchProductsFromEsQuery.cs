using Domain.Entities;
using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.QueryDsl;
using Infrastructure;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Api.UseCases.Products;

public record SearchProductsFromEsQuery : IRequest<GetProductFromEsResponse>
{
    public string SearchTerm { get; init; }
    
    public static Func<IMediator, Task<GetProductFromEsResponse>> EndpointHandler => (mediator) => mediator.Send(new SearchProductsFromEsQuery());
}

public class GetProductFromEsQueryHandler(ApplicationDbContext context, ElasticsearchClient esClient) : IRequestHandler<SearchProductsFromEsQuery, GetProductFromEsResponse>
{
    public async Task<GetProductFromEsResponse> Handle(SearchProductsFromEsQuery request, CancellationToken cancellationToken)
    {
        var esRequest = new SearchRequest
        {
            From = 0,
            Size = 10,
            Query = new TermQuery("name") { Value = request.SearchTerm }
        };
        
        var response = await esClient.SearchAsync<ProductDocument>(esRequest, cancellationToken);
        
        if (!response.IsValidResponse)
        {
            throw new Exception("Error");
        }
        
        var doc = response.Hits.FirstOrDefault().Source;
        
        if (doc == null)
        {
            throw new Exception("Not found");
        }
            
        return new GetProductFromEsResponse(doc.Id, doc.Name);
    }
}

public record GetProductFromEsResponse(string Id, string Name);