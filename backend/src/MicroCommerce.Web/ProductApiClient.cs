namespace MicroCommerce.Web;

public class ProductApiClient(HttpClient httpClient)
{
    public async Task<Product[]> GetProductsAsync(int maxItems = 10, CancellationToken cancellationToken = default)
    {
        var products = await httpClient.GetFromJsonAsync<Product[]>("/api/products", cancellationToken);

        return products ?? [];
    }
}

public record Product(string Id, string Name)
{
}