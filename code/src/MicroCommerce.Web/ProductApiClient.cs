using MicroCommerce.ApiService.Features.Products;

namespace MicroCommerce.Web;

public class ProductApiClient(HttpClient httpClient)
{
    public async Task<GetProductsFromElasticsearch.ProductViewModel[]> GetProductsAsync(int maxItems = 10, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.GetFromJsonAsync<GetProductsFromElasticsearch.Response>("/api/products", cancellationToken);

        return response?.Data.ToArray() ?? [];
    }
}

