namespace Infrastructure;

public class ProductDocument
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public decimal Price { get; set; }
}

public static class ElasticSearchIndexKey
{
    public class Product
    {
        public const string Key = "product";
        public const string Alias = "product*";
    }
}