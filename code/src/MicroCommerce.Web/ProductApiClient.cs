namespace MicroCommerce.Web;

public class ProductApiClient(HttpClient httpClient)
{
    public async Task<GetProductsResponse.Product[]?> GetProductsAsync(int maxItems = 10, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.GetFromJsonAsync<GetProductsResponse>("/api/products", cancellationToken);

        return response?.Data ?? [];
    }
}

public record GetProductsResponse
{
    public Product[]? Data { get; init; } = [];

    public record Product
    {
        public Guid Id { get; init; }
        public string Name { get; init; } = "";
        public decimal Price { get; init; }
        public long RemainingStock { get; init; }
        public string ImageUrl { get; set; } = "";
    }
}
