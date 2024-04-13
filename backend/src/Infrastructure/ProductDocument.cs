namespace Infrastructure;

public class ProductDocument
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public decimal Price { get; set; }
}

public static class ElasticSearchIndexKey
{
    public const string Product = "product";
}