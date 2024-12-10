using MicroCommerce.ApiService.Domain.Entities;
using MicroCommerce.ApiService.Features.Carts;

namespace MicroCommerce.Web;

public class CartApiClient(HttpClient httpClient)
{
    public async Task<GetCart.Response?> GetCartById(Guid id, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.GetFromJsonAsync<GetCart.Response>($"/api/carts/{id}", cancellationToken);

        return response;
    }

    public async Task<AddProductToCart.Response?> AddProductToCart(AddProductToCart.Command request, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.PostAsJsonAsync($"/api/carts/{request.CartId}/products", request, cancellationToken);

        response.EnsureSuccessStatusCode();

        var data = await response.Content.ReadFromJsonAsync<AddProductToCart.Response>(cancellationToken);

        return data;
    }
}
