namespace MicroCommerce.ApiService.Services;

public class ProductDocument
{
    public const string IndexPattern = "*product*";
    
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public decimal Price { get; set; }
}
