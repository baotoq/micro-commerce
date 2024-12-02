namespace MicroCommerce.ApiService.Services;

public static class LockKeys
{
    public static string Cart(Guid id) => $"Cart:{id.ToString()}";
}
