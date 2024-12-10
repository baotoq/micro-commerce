namespace MicroCommerce.Web;

public class CartService
{
    public List<GetProductsResponse.Product> CartItems { get; private set; } = new();

    public void AddToCart(GetProductsResponse.Product product)
    {
        CartItems.Add(product);
    }
}
